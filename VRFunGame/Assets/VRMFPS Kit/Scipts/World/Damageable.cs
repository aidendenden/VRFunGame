using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace VRMultiplayerFPSKit
{
    /// <summary>
    /// Simply tracks health, allowing for further behaviour extention by composition
    /// </summary>
    public class Damageable : NetworkBehaviour
    {
        [SyncVar] public float health;
        private float _startHealth;

        private void Start()
        {
            _startHealth = health;
        }

        [Server]
        public void TakeDamage(float damage)
        {
            health -= damage;
        }

        [Command]
        public void RequestResetHealth()
        {
            ResetHealth();
        }

        [Server]
        public void ResetHealth()
        {
            health = _startHealth;
        }
    }
}