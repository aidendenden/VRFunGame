using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;
using VRMultiplayerFPSKit.Input;

namespace VRMultiplayerFPSKit
{
    /// <summary>
    /// Ejects magazine when input is pressed
    /// </summary>
    [RequireComponent(typeof(MagazineInteractor))]
    public class MagazineEjectorInput : MonoBehaviour
    {
        private const float MaxMagazineDetachDelay = 2;
        public XRHeldInputAction ejectMagazineInput;

        private float _detachStartTime;

        private MagazineInteractor _magazineInteractor;
        private XRGrabInteractable _weaponInteractable;
        private NetworkIdentity _networkIdentity;

        // Update is called once per frame
        void Update()
        {
            //If no authority, return
            if (!_networkIdentity.isOwned) return;
            //Make sure someone is holding the weapon
            if (!_weaponInteractable.isSelected) return;
            //Return if magazine interactor isn't holding a mag
            if (_magazineInteractor.interactablesSelected.Count == 0) return;

            //Get input action for the hand currently holding
            InputAction detachInput = ejectMagazineInput.GetActionForPrimaryHand(_weaponInteractable);

            //If no input action is active, return
            if (detachInput == null) return;

            //Log detach start time
            if (detachInput.WasPressedThisFrame())
            {
                _detachStartTime = Time.time;
                _magazineInteractor.EjectMagazine();
            }

            //Detach when input is no longer held, also wait so minimum delay has passed
            if (detachInput.WasReleasedThisFrame())
                _magazineInteractor.EjectMagazine(true);
            
            //Detach when max detach time is reached
            if(Time.time - _detachStartTime > MaxMagazineDetachDelay && 
               detachInput.IsPressed())
                _magazineInteractor.DetachMagazine();
        }

        private void Awake()
        {
            _magazineInteractor = GetComponent<MagazineInteractor>();
            _networkIdentity = GetComponentInParent<NetworkIdentity>();
            _weaponInteractable = GetComponentInParent<XRGrabInteractable>();
        }
    }
}