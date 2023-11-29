using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using UnityEngine.XR.Interaction.Toolkit;

namespace VRMultiplayerFPSKit
{
    /// <summary>
    /// Loads cartridge items that enter trigger in to magazine
    /// </summary>
    [RequireComponent(typeof(XRGrabInteractable), typeof(Magazine))]
    public class MagazineLoader : NetworkBehaviour
    {
        public AudioSource unloadSound;
        public BoxCollider loadTrigger;
        public float cartridgeLoadCooldown = .3f;

        private Magazine _magazine;
        private XRGrabInteractable _interactable;
        private MagazineUnloader _unloader;
        private float _lastLoadTime;
        

        // Update is called once per frame
        void Update()
        {
            TryLoadCartridges();
        }

        private void TryLoadCartridges()
        {
            if (!isOwned) return;
            //Don't load mags if mag isn't held
            if (!_interactable.isSelected) return;
            //Don't load mags if mag is in a weapon
            if (_interactable.interactorsSelecting[0] is MagazineInteractor) return;
            //Don't load full mag
            if (_magazine.IsFull()) return;
            //Load cooldown
            if (Time.time - _lastLoadTime < cartridgeLoadCooldown) return;
            //Cooldown if recently unloaded a round, to prevent it from being loaded again
            if (_unloader && Time.time - _unloader.lastUnloadTime < 1) return;
            
            //Get all cartridge items inside trigger bounds
            Bounds loadBounds = loadTrigger.bounds;
            Collider[] collidersInBounds = Physics.OverlapBox(loadBounds.center, loadBounds.extents, Quaternion.identity);
            foreach (Collider collider in collidersInBounds)
            {
                CartridgeItem cartridge = collider.GetComponent<CartridgeItem>();
                
                if(!cartridge) continue;
                if (cartridge.cartridge.caliber != _magazine.caliber) continue;
                
                //If all checks pass, load the cartridge
                _magazine.AddCartridgeToTop(cartridge.cartridge);
                CMD_LoadCartridgeEffects(cartridge);
                
                _lastLoadTime = Time.time;
                
                //Only load max one cartridge per call
                return;
            }
        }
        
        #region Network Calls
        [Command]
        private void CMD_LoadCartridgeEffects(CartridgeItem cartridgeItem)
        {
            NetworkServer.Destroy(cartridgeItem.gameObject);
            RPC_LoadCartridgeEffects();
        }
        
        [ClientRpc]
        private void RPC_LoadCartridgeEffects()
        {
            unloadSound.Play();
        }
        #endregion
        
        // Start is called before the first frame update
        void Awake()
        {
            _magazine = GetComponent<Magazine>();
            _interactable = GetComponent<XRGrabInteractable>();
            _unloader = GetComponent<MagazineUnloader>();//Soft dependency on unloader
        }
    }
}