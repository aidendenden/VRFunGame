using System.Collections;
using System.Collections.Generic;
using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace VRMultiplayerFPSKit
{
    public class ConnectionStatusUI : MonoBehaviour
    {
        public TextMeshProUGUI connectionStatus;
        public TextMeshProUGUI subtitle;

        private bool _hasTriedConnecting;

        // Update is called once per frame
        void Update()
        {
            //Subtitle by default not visible
            subtitle.text = "";
            
            if (!NetworkManager.singleton) { connectionStatus.text = "Couldn't Find NetworkManager"; return; }
            
            //Check If we are online
            if (NetworkManager.singleton.mode != NetworkManagerMode.Offline)
                OnlineText();
            else
                OfflineText();
        }

        private void OnlineText()
        {
            //If we are online, that means we have tried connecting
            _hasTriedConnecting = true;
                
            //If we are connecting as client
            if (NetworkManager.singleton.mode == NetworkManagerMode.ClientOnly)
            {
                connectionStatus.text =  "Connecting...";
                subtitle.text = $"Network Address; {NetworkManager.singleton.networkAddress}";
                return;
            }
                
            //If we are hosting
            connectionStatus.text = "Hosting...";
        }

        private void OfflineText()
        {
            //if we tried to connect, but still are offline, it means connection failed
            if (_hasTriedConnecting)
            {
                connectionStatus.text = "Connection Failed";
                subtitle.text = $"IP Address: {NetworkManager.singleton.networkAddress} " +
                                "\n 1. Check Logs for errors. " +
                                "\n 2. Consult Network Troubleshooting Guide in Docs. " +
                                "\n 3. Reach out in the support Discord";
                return;
            }
            
            NetworkAutoConnector autoConnector = NetworkManager.singleton.GetComponent<NetworkAutoConnector>();
            
            //Check if autoConnector is missing
            if (!autoConnector || !autoConnector.enabled)
            {
                connectionStatus.text = "Waiting For Manual Network Start";
                subtitle.text = "Couldn't Find NetworkAutoConnector Component on the NetworkManager";
                return;
            }
            
            //Otherwise, we are just waiting to connect
            connectionStatus.text = NetworkAutoConnector.ShouldHost() ? 
                $"Starting Automatic Host in {autoConnector.networkStartDelay} seconds" : 
                $"Starting Automatic Connect in {autoConnector.networkStartDelay} seconds";
        }
    }
}