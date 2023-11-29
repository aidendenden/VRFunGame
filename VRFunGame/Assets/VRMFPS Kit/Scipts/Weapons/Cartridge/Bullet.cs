using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

namespace VRMultiplayerFPSKit
{
    /// <summary>
    /// Represents a physical bullet that has been fired
    /// </summary>
    public class Bullet : NetworkBehaviour
    {
        public AudioSource hitSound;
        public ParticleSystem hitParticle;
        public GameObject tracerTail;
        [Space]
        public float startVelocity;
        public float damage;
        [SyncVar] public BulletType bulletType;

        // Start is called before the first frame update
        void Start()
        {
            GetComponent<Rigidbody>().AddForce(transform.forward * startVelocity, ForceMode.VelocityChange);
            tracerTail.SetActive(bulletType == BulletType.Tracer);
        }

        private void OnCollisionEnter(Collision other)
        {
            if (!isServer)
                return;
            
            //Get damageable component in collider or in parents
            Damageable damageable = other.gameObject.GetComponentInParent<Damageable>();

            if (damageable != null)
            {
                damageable.TakeDamage(damage);
            }

            //Schedule destruction, let sound play out
            Invoke(nameof(DestroyBullet), 1);
            //Stop moving
            GetComponent<Rigidbody>().isKinematic = true;

            //Play hit effects on clients
            RPC_PlayHitEffect();
        }

        [ClientRpc]
        private void RPC_PlayHitEffect()
        {
            hitSound.Play();
            hitParticle.Play();
        }

        [Server]
        private void DestroyBullet()
        {
            NetworkServer.Destroy(gameObject);
        }
    }
}