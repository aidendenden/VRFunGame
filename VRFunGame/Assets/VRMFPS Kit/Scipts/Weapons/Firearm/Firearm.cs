using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using HapticPatterns;
using Mirror;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using UnityEngine.XR.Interaction.Toolkit;

namespace VRMultiplayerFPSKit
{
    /// <summary>
    /// Main script that handles behaviour which is in common for all firearms,
    /// the rest of the components are compositional and added when needed
    /// </summary>
    [RequireComponent(typeof(XRGrabInteractable))]
    public class Firearm : NetworkBehaviour
    {
        #if HAPTIC_PATTERNS
        public HapticPattern shootHaptic;
        #else
        [Header("VR Haptic Patterns Isn't Installed")]
        #endif
        
        [Space] 
        public FireMode[] availableFireModes;
        [Space] 
        
        [Header("Current State")]
        [SyncVar] public Cartridge chamberCartridge;
        [SyncVar] public Magazine magazine;
        [SyncVar] public FireMode currentFireMode;
        [HideInInspector] public bool isActionOpen;
        //TODO hammer state
        
        public event Action<Cartridge> ShootEvent;

        public void TryShoot()
        {
            //Check if we have authority
            if (!isOwned) { Debug.LogWarning("Tried shooting weapon without authority, will be ignored"); return; }
            
            //Can't shoot unless action is in battery
            if (isActionOpen) return;
            
            //Make sure the cartridge can fire
            if (!chamberCartridge.CanFire()) return;
            
            Cartridge unspentCartridge = chamberCartridge;
            chamberCartridge.Consume();
            
            //Fire if all checks pass
            ShootEvent?.Invoke(unspentCartridge);
            
            //If haptic patterns is installed, play shoot haptic
            #if HAPTIC_PATTERNS 
            if (shootHaptic != null)
                shootHaptic.PlayOverTime(GetComponent<XRGrabInteractable>());
            #endif
        }

        /// <summary>
        /// If a cartridge is in the chamber, eject it
        /// </summary>
        public void TryEjectChamber()
        {
            //Cant eject empty chamber
            if (chamberCartridge.IsNull())
                return;

            //If an ejector exists, visually eject the round
            GetComponent<CartridgeEjector>()?.EjectCartridge(chamberCartridge);

            //Chamber is emptied
            chamberCartridge = Cartridge.Empty;
        }
        
        /// <summary>
        /// If possible, load chamber from magazine
        /// </summary>
        public void TryLoadChamber()
        {
            //Cant load filled chamber
            if (!chamberCartridge.IsNull())
                return;
            //Cant load with empty mag
            if (magazine == null || magazine.IsEmpty())
                return;

            chamberCartridge = magazine.GetTopCartridge();
            magazine.RemoveCartridgeFromTop();
        }
    }
}