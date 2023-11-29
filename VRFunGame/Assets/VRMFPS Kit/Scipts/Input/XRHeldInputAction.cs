using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

namespace VRMultiplayerFPSKit.Input
{
    /// <summary>
    /// Struct that lets us store one input action per hand, and later
    /// get the action that corresponds to the hand that is holding an object
    /// </summary>
    [Serializable]
    public struct XRHeldInputAction
    {
        public InputActionReference leftHandAction;
        public InputActionReference rightHandAction;

        /// <summary>
        /// Get the input action that corresponds to the hand that is holding an object
        /// </summary>
        /// <param name="heldObject">The held interactable that you want input on</param>
        /// <returns></returns>
        public InputAction GetActionForPrimaryHand(XRBaseInteractable heldObject)
        {
            IXRInteractor interactor = heldObject.interactorsSelecting[0];
            if (interactor is not XRBaseControllerInteractor hand)
                return null;

            //Make sure controls are enabled
            if(!rightHandAction.asset.enabled)
                rightHandAction.asset.Enable();
            
            //TODO solve without relying on hand object name
            if (hand.gameObject.name.ToLower().Contains("right"))
                return rightHandAction.action;
            if(hand.gameObject.name.ToLower().Contains("left"))
                return leftHandAction.action;

            Debug.LogError("XR Hand object did not have left/right in name, can't decide which input to use");
            return null;
        }
    }
}