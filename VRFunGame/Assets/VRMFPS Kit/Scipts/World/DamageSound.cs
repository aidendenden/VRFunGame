using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace VRMultiplayerFPSKit
{
    [RequireComponent(typeof(Damageable))]
    public class DamageSound : MonoBehaviour
    {
        public AudioSource audioSource;

        private float _healthLastFrame;
            
        private Damageable _damageable;
        private Quaternion _defaultRotation;

        // Start is called before the first frame update
        private void Start()
        {
            _damageable = GetComponent<Damageable>();
        }

        // Update is called once per frame
        void Update()
        {
            //Wait until damageable health has fallen
            if (_damageable.health < _healthLastFrame)
                audioSource.Play();
            
            _healthLastFrame = _damageable.health;
        }
    }
}