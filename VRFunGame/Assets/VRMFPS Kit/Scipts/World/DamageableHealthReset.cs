using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

namespace VRMultiplayerFPSKit
{
    public class DamageableHealthReset : MonoBehaviour
    {
        public GameObject[] damageableContainers;

        public void ResetHealth()
        {
            if (!NetworkServer.active)
                Debug.LogWarning("Server method was called without a server running");

            foreach (GameObject damageableContainer in damageableContainers)
            {
                foreach (Damageable damageable in damageableContainer.GetComponentsInChildren<Damageable>())
                    damageable.ResetHealth();
            }
        }
    }
}