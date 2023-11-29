using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.Serialization;

namespace VRMultiplayerFPSKit
{
    /// <summary>
    /// Updates the animator controller variables
    /// </summary>
    [RequireComponent(typeof(Firearm),
        typeof(Animator))]
    public class FirearmAnimator : NetworkBehaviour
    {
        public AudioSource shootSound;
        public ParticleSystem shootParticle;
        [Space]

        private Firearm _firearm;
        private NetworkAnimator _netAnimator;
        private NetworkIdentity _identity;
        private FirearmCyclingAction _cyclingAction;

        // Update is called once per frame
        void Update()
        {
            if (!_identity.isOwned) return;

            if (_cyclingAction)
            {
                _netAnimator.animator.SetFloat("Action Position",
                    //Clamp between 0 & .99 since animation will overflow back to 0 if value reaches 1
                    Mathf.Clamp(_cyclingAction.actionPosition01, 0, .99f));
                
                //Only run this on weapons that can be locked back
                _netAnimator.animator.SetBool("Action Locked Back", _cyclingAction.isLockedBack);
            }

            _netAnimator.animator.SetBool("Magazine Attached", _firearm.magazine != null);
            _netAnimator.animator.SetInteger("Fire Mode Index", 
                Array.IndexOf(_firearm.availableFireModes, _firearm.currentFireMode));
        }

        [Client]
        public void Shoot(Cartridge cartridge)
        {
            _netAnimator.SetTrigger("Shoot");
            CMD_ShootEffects();
        }
        
        [Command]
        private void CMD_ShootEffects()
        {
            RPC_ShootEffects();
        }

        [ClientRpc]
        private void RPC_ShootEffects()
        {
            if (shootSound)
                shootSound.Play();
            
            if (shootParticle)
                shootParticle.Play();
        }
        
        // Start is called before the first frame update
        void Awake()
        {
            _firearm = GetComponent<Firearm>();
            _netAnimator = GetComponent<NetworkAnimator>();
            _identity = GetComponent<NetworkIdentity>();
            _cyclingAction = GetComponent<FirearmCyclingAction>();

            _firearm.ShootEvent += Shoot;
        }
    }
}