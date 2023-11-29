using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Mirror;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace VRMultiplayerFPSKit
{
    public class CartridgeEjector : NetworkBehaviour
    {
        private const float CasingDestructionTime = 5f;
        
        public Transform spawnPoint;
        public GameObject cartridgeItemPrefab;
        [Space] 
        public Vector3 defaultVelocity;
        public float velocityRandomDeviationFactor;
        [Space] 
        public float maxRandomAngularVelocity;

        /// <summary>
        /// Spawns an ejected cartridge
        /// </summary>
        /// <param name="cartridge">Ejected cartridge properties</param>
        [Command]
        public void EjectCartridge(Cartridge cartridge)
        {
            if (!NetworkServer.active)
            {
                Debug.LogWarning("Server method was called on client");
                return;
            }

            if (cartridge.Equals(Cartridge.Empty))
            {
                Debug.LogWarning("Tried ejecting an empty cartridge");
                return;
            }
            
            //Instantiate cartridge & spawn on network
            GameObject obj = Instantiate(cartridgeItemPrefab, spawnPoint.position, spawnPoint.rotation);
            NetworkServer.Spawn(obj);
            
            Rigidbody rb = obj.GetComponent<Rigidbody>();
            
            //Apply Random Linear Velocity
            float velocityDeviation = velocityRandomDeviationFactor * Random.Range(-1f, 1f);
            Vector3 randomizedVelocity = defaultVelocity + (defaultVelocity * velocityDeviation);
            Vector3 worldSpaceVelocity = transform.TransformDirection(randomizedVelocity);
            rb.AddForce(worldSpaceVelocity, ForceMode.VelocityChange);
            
            //Apply random angular velocity
            rb.maxAngularVelocity = Single.PositiveInfinity;
            rb.AddTorque(new Vector3(
                    Random.Range(-1f, 1f) * maxRandomAngularVelocity,
                    Random.Range(-1f, 1f) * maxRandomAngularVelocity,
                    Random.Range(-1f, 1f) * maxRandomAngularVelocity),
                ForceMode.VelocityChange);

            //Apply cartridge item values
            CartridgeItem cartridgeItem = obj.GetComponent<CartridgeItem>();
            cartridgeItem.cartridge = cartridge;
            
            //If we ejected empty casing, Schedule Cartridge Destruction,
            //you can remove this if you want Casings to never disappear!
            if(cartridge.bulletType == BulletType.Empty_Casing)
                DestroyCartridgeLater(obj);
        }

        [Server]
        private async void DestroyCartridgeLater(GameObject cartridge)
        {
            await Task.Delay((int)(CasingDestructionTime*1000));
            
            NetworkServer.Destroy(cartridge);
        }
    }
}