using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;

namespace VRMultiplayerFPSKit
{
    /// <summary>
    /// Main class that handles behaviour that is in common for all grenades
    /// </summary>
    [RequireComponent(typeof(XRGrabInteractable), typeof(Rigidbody))]
    public class Grenade : NetworkBehaviour
    {
        public float fuseDuration;
        public GameObject explosionPrefab;
        [Space] 
        public Rigidbody safetyPin;
        public Rigidbody lever;
        [Space] 
        public AudioSource primerSound;
        public AudioSource pinPullSound;

        [SyncVar] private double _primedNetTime = -1;
        
        // Update is called once per frame
        void Update()
        {
            if (!XRMultiplayerInteractor.HasBehaviourAuthority(this)) return;
            
            TryDetachLever();
            
            //If is primed (_primedNetTime isn't = -1 anymore) & fuse duration has expired, explode
            if (_primedNetTime > 0 && 
                NetworkTime.time - _primedNetTime > fuseDuration)
                Explode();
        }

        public void DetachSafetyPin()
        {
            //Can only be called if we own object
            if (!XRMultiplayerInteractor.HasBehaviourAuthority(this)) return;

            CMD_DetachSafetyPinClientEffects();
        }
        
        private void TryDetachLever()
        {
            //Can only be called if we own object
            if (!XRMultiplayerInteractor.HasBehaviourAuthority(this)) return;
            //Wait until pin is detached
            if (safetyPin) return;
            //Dont try detaching lever if it already has been done
            if (!lever) return;
            //Don't try detaching if already primed
            if (_primedNetTime > 0) return;
            //Wait until rigidbody isn't kinematic, meaning no one is holding it
            if (GetComponent<Rigidbody>().isKinematic) return;

            CMD_DetachLeverClientEffects();
            
            //Detach if all conditions are met
            Prime();
        }

        private void Prime()
        {
            //Can only be called if we own object
            if (!XRMultiplayerInteractor.HasBehaviourAuthority(this)) return;
            
            //Schedule explosion
            _primedNetTime = NetworkTime.time;
        }

        [Command(requiresAuthority = false)]//Server doesn't have client authority, but should still be able to call
        private void Explode()
        {
            //Spawn explosion at our position
            GameObject explosion = Instantiate(explosionPrefab, transform.position, Quaternion.identity);
            NetworkServer.Spawn(explosion);
            
            //Destroy grenade
            NetworkServer.Destroy(gameObject);
        }
        
        #region Client Effects
        [Command]
        private void CMD_DetachSafetyPinClientEffects()
        {
            //RPC needs to be called on server
            //Chain of events goes: Owning client -> Server -> All clients
            RPC_DetachSafetyPinClientEffects();
        }

        [ClientRpc]
        private void RPC_DetachSafetyPinClientEffects()
        {
            //Play pin pull sound
            pinPullSound.Play();

            //Safety pin is no longer part of grenade
            safetyPin.transform.parent = null;

            //Enable physics on the pin, and reset velocity
            safetyPin.isKinematic = false;
            safetyPin.velocity = Vector3.zero;

            //Schedule safety pin object destruction
            Destroy(safetyPin.gameObject, 10);

            safetyPin = null;
        }
        
        [Command]
        private void CMD_DetachLeverClientEffects()
        {
            //RPC needs to be called on server
            //Chain of events goes: Owning client -> Server -> All clients
            RPC_DetachLeverClientEffects();
        }

        [ClientRpc]
        private void RPC_DetachLeverClientEffects()
        {
            //Play pin pull sound
            primerSound.Play();

            //Lever is no longer part of grenade
            lever.transform.parent = null;

            //Enable physics on the lever, and use same velocity as grenade
            lever.isKinematic = false;
            lever.velocity = GetComponent<Rigidbody>().velocity * .3f;
            //TODO random angular vel

            //Schedule lever object destruction
            Destroy(lever.gameObject, 5);

            lever = null;
        }
        
        #endregion
    }
}