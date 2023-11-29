using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.XR.Interaction.Toolkit;

namespace VRMultiplayerFPSKit
{
    /// <summary>
    /// Instead of just pressing an input to change Fire Mode, This component lets
    /// you physically grab the fire selector (on say an AK) and select fire mode
    /// </summary>
    [RequireComponent(typeof(XRSimpleInteractable))]
    public class PhysicalFireSelectInteractor : MonoBehaviour
    {
        public Vector3 fireSelectMovement;
        
        private Vector3 _handPositionLastFrame;
        private float _deltaMovementMagnitude;

        private XRSimpleInteractable _fireSelectInteractable;
        private Firearm _firearm;

        // Update is called once per frame
        void Update()
        {
            //Only run on authority
            if (!_firearm.isOwned) return;
            //Only run when holding action
            if (!_fireSelectInteractable.isSelected) return;
            
            TrackHandMovement();
            TryChangeFireMode();
        }

        /// <summary>
        /// Changes to the next fire mode once hand has moved far enough along specified direction
        /// </summary>
        private void TryChangeFireMode()
        {
            //Wait until fire switch has moved enough to trigger switch
            if (Mathf.Abs(_deltaMovementMagnitude) < fireSelectMovement.magnitude) return;

            int switchDirection = (_deltaMovementMagnitude > 0) ? 1 : -1;
            
            int currentFireModeIndex = Array.IndexOf(_firearm.availableFireModes, _firearm.currentFireMode);
            int nextFireModeIndex = currentFireModeIndex + switchDirection;

            //Reset delta movement
            _deltaMovementMagnitude = 0;
            
            //if next fire mode is outside the fire mode array, cancel
            if (nextFireModeIndex < 0 || nextFireModeIndex >= _firearm.availableFireModes.Length) return;
            
            //Switch fire mode
            _firearm.currentFireMode = _firearm.availableFireModes[nextFireModeIndex];
        }

        private void TrackHandMovement()
        {
            //return if _handPositionLastFrame isn't initialized
            if (_handPositionLastFrame == Vector3.zero) return;
            
            //Calculate hand movement change since last frame
            Vector3 deltaHandMovement = GetHandPosition() - _handPositionLastFrame;
            Vector3 deltaHandMovementInLocalSpace = transform.InverseTransformVector(deltaHandMovement);
            //Translate hand movement to fire mode movement direction
            Vector3 trackedDirectionDeltaMovement =
                Vector3.Scale(-deltaHandMovementInLocalSpace, fireSelectMovement.normalized);
            
            _deltaMovementMagnitude += trackedDirectionDeltaMovement.x + trackedDirectionDeltaMovement.y +
                                       trackedDirectionDeltaMovement.z;
        }

        private Vector3 GetHandPosition()
        {
            if(!_fireSelectInteractable.isSelected) return Vector3.zero;
            
            return _fireSelectInteractable.interactorsSelecting[0].transform.position;
        }
        
        private void LateUpdate()
        {
            _handPositionLastFrame = GetHandPosition();
        }

        // Start is called before the first frame update
        void Awake()
        {
            //Fetch components necessary
            _fireSelectInteractable = GetComponent<XRSimpleInteractable>();
            _firearm = GetComponentInParent<Firearm>();
        }
    }
}