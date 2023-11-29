using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

namespace VRMultiplayerFPSKit
{
    /// <summary>
    /// Representing a magazine that stores cartridges. Can be used on a magazine item
    /// object or as an internal magazine by attaching it to a firearm
    /// </summary>
    public class Magazine : NetworkBehaviour
    {
        [SyncVar] public Caliber caliber;
        [SyncVar] public int capacity;

        public Firearm[] compatibleWeaponPrefabs;
        
        [Space]
        //TODO let inspector change magazine contents
        public readonly SyncList<Cartridge> cartridges = new();
        
        
        private void Start()
        {
            AddCartridgeToTop(new Cartridge(caliber, BulletType.FMJ), capacity);
        }

        public Cartridge GetTopCartridge()
        {
            if (cartridges.Count == 0)
                return Cartridge.Empty;

            return cartridges[^1];
        }

        public void AddCartridgeToTop(Cartridge cartridge, int amount = 1)
        {
            //Check if we have authority
            if (!XRMultiplayerInteractor.HasBehaviourAuthority(this))
            {
                Debug.LogWarning("Tried adding cartridges to Magazine without authority, will be ignored");
                return;
            }

            //Add cartridge to top x times
            for (int i = 0; i < amount; i++)
            {
                //Check if cartridge fits
                if (IsFull())
                    return;

                cartridges.Add(cartridge);
            }
        }

        public void RemoveCartridgeFromTop(int amount = 1)
        {
            //Check if we have authority
            if (!XRMultiplayerInteractor.HasBehaviourAuthority(this))
            {
                Debug.LogWarning("Tried removing cartridges to Magazine without authority, will be ignored");
                return;
            }

            //Remove cartridge from top x times
            for (int i = 0; i < amount; i++)
            {
                int index = cartridges.Count - 1;

                if (index < 0)
                    break;
                
                cartridges.RemoveAt(index);
            }
        }

        public bool IsEmpty()
        {
            return cartridges.Count == 0;
        }

        public bool IsFull()
        {
            return cartridges.Count == capacity;
        }

        /// <summary>
        /// Determines whether compatibleWeaponPrefabs contains the specified Weapon, and thus is compatible
        /// </summary>
        /// <param name="firearm">Firearm to check compatibility with</param>
        /// <returns>Is compatible?</returns>
        public bool IsCompatibleWithWeapon(Firearm firearm){
            string weaponName = firearm.gameObject.name;

            //TODO prefab name comparison to determine compatibility is unreliable
            foreach(Firearm compatibleWeapon in compatibleWeaponPrefabs){
                if(weaponName.Contains(compatibleWeapon.gameObject.name))
                    return true;
            }   
                     
            return false;
        }
    }
}