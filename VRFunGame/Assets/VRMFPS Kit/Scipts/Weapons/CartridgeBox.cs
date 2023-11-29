using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.Serialization;

namespace VRMultiplayerFPSKit
{
    [RequireComponent(typeof(XRDoubleHandScaler))]
    public class CartridgeBox : NetworkBehaviour
    {
        public Cartridge cartridgeType;
        public int cartridgeAmount = 20;
        public float cartridgeSpawnRadius = .075f;
        [Space]
        public GameObject cartridgePrefab;

        private XRDoubleHandScaler _scaler;
        

        // Update is called once per frame
        void Update()
        {
            if (!isOwned)
                return;
            //Wait until box has been maximally opened/scaled by player
            if(!Mathf.Approximately(_scaler.GetDistanceScaledRatio(), _scaler.maxSizeRatio))
                return;
            
            OpenBox();
        }

        [Command]
        private void OpenBox()
        {
            //Spawn x amounts of cartridges at random position
            for (int i = 0; i < cartridgeAmount; i++)
            {
                Vector3 randomPosition = new Vector3(
                    Random.Range(-cartridgeSpawnRadius, cartridgeSpawnRadius), 
                    Random.Range(-cartridgeSpawnRadius, cartridgeSpawnRadius), 
                    Random.Range(-cartridgeSpawnRadius, cartridgeSpawnRadius));
                
                GameObject cartridgeObj = Instantiate(
                    cartridgePrefab, 
                    transform.position + randomPosition,
                    Quaternion.identity);
                NetworkServer.Spawn(cartridgeObj);
                
                CartridgeItem cartridgeItem = cartridgeObj.GetComponent<CartridgeItem>();
                cartridgeItem.cartridge = cartridgeType;
            }
            
            //Destroy the box when it has been used
            NetworkServer.Destroy(gameObject);
        }
        
        void Awake()
        {
            _scaler = GetComponent<XRDoubleHandScaler>();
        }
    }
}
