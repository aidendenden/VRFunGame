using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

namespace VRMultiplayerFPSKit
{
    /// <summary>
    /// Teleports player once Damageable health reaches 0
    /// </summary>
    [RequireComponent(typeof(Damageable))]
    public class PlayerRespawner : NetworkBehaviour
    {
        private Damageable _damageable;
        private float _healthLastFrame;
        
        private void Awake()
        {
            _damageable = GetComponent<Damageable>();
        }

        void Update()
        {
            if (!isOwned)
                return;
            
            if(_damageable.health != _healthLastFrame && _damageable.health <= 0)
                Respawn();

            _healthLastFrame = _damageable.health;
        }
        
        private void Respawn()
        {
            transform.position = Vector3.zero;
            GetComponent<Damageable>().RequestResetHealth();
        }
    }
}