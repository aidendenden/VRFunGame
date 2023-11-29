using System.Collections;
using System.Collections.Generic;
using HapticPatterns;
using Mirror;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.XR.Interaction.Toolkit;

namespace VRMultiplayerFPSKit
{
    /// <summary>
    /// Creates a proxy interactable for FirearmCyclingAction, allowing you to pull back the action.
    /// Also handles behaviour like chamber loading and ejecting.
    /// </summary>
    [RequireComponent(typeof(XRSimpleInteractable))]
    public class FirearmCyclingActionInteractable : MonoBehaviour
    {
        #if HAPTIC_PATTERNS
        public HapticPattern actionPullPattern;
        #else
        [Header("VR Haptic Patterns Isn't Installed")]
        #endif
        
        public Vector3 handMovementDirectionSensitivity;
        [HideInInspector] public XRSimpleInteractable actionInteractable;

        private Vector3 _handPositionLastFrame;

        private NetworkIdentity _identity;
        private FirearmCyclingAction _cyclingAction;

        // Update is called once per frame
        void Update()
        {
            //Only run on authority
            if (!_identity.isOwned) return;
            //Only run when holding action
            if (!actionInteractable.isSelected) return;

            //Unlock action if it has been grabbed
            _cyclingAction.TryUnlockAction();
            
            ActionTrackHandMovement();
            
            #if HAPTIC_PATTERNS
            if (actionInteractable.isSelected)
                actionPullPattern.PlayGradually(actionInteractable, _cyclingAction.actionPosition01);
            #endif
        }

        private void ActionTrackHandMovement()
        {
            //return if _handPositionLastFrame isn't initialized
            if (_handPositionLastFrame == Vector3.zero) return;
            
            //Calculate hand movement change since last frame
            Vector3 deltaHandMovement = GetHandPosition() - _handPositionLastFrame;
            Vector3 deltaHandMovementInLocalSpace = transform.InverseTransformVector(deltaHandMovement);
            //Translate hand movement to bolt position change
            Vector3 trackedDirectionDeltaMovement =
                Vector3.Scale(-deltaHandMovementInLocalSpace, handMovementDirectionSensitivity);
            
            float movementMagnitude = trackedDirectionDeltaMovement.x + trackedDirectionDeltaMovement.y +
                                      trackedDirectionDeltaMovement.z;
            
            //Apply bolt position change
            _cyclingAction.actionPosition01 = Mathf.Clamp01(_cyclingAction.actionPosition01 + movementMagnitude);
        }

        public void ForceDetachSelectors()
        {
            actionInteractable.interactionManager.CancelInteractableSelection(
                actionInteractable as IXRSelectInteractable);
        }

        private Vector3 GetHandPosition()
        {
            if(!actionInteractable.isSelected) return Vector3.zero;
            
            return actionInteractable.interactorsSelecting[0].transform.position;
        }

        private void LateUpdate()
        {
            _handPositionLastFrame = GetHandPosition();
        }

        // Start is called before the first frame update
        void Awake()
        {
            //Fetch components necessary
            _identity = GetComponentInParent<NetworkIdentity>();
            _cyclingAction = GetComponentInParent<FirearmCyclingAction>();
            actionInteractable = GetComponent<XRSimpleInteractable>();

            if (_cyclingAction == null)
                Debug.LogError("Action interfacer cycler was not found in parent!");
        }
    }
}