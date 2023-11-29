using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace VRMultiplayerFPSKit
{
    /// <summary>
    /// Helper class that automatically hosts or connects depending if you are in the editor or not
    /// </summary>
    [RequireComponent(typeof(NetworkManager))]
    public class NetworkAutoConnector : MonoBehaviour
    {
        [Header("Disable this script if you don't want to auto connect")]
        [Space]
        [Tooltip("How long we'll wait before trying to connect/host after starting game")]
        public float networkStartDelay = 1f;
        
        private NetworkManager _manager;
        
        // Start is called before the first frame update
        async void Start()
        {
            EnsureOfflineScene();
            
            //Wait a certain time before trying to connect/host (in milliseconds)
            await Task.Delay((int)(networkStartDelay * 1000));
            
            //Automatically host in the editor, auto connect to server on
            if (ShouldHost())
            {
                Debug.Log($"Auto Hosting...");
                _manager.StartHost();
            }
            else
            {
                Debug.Log($"Auto connecting to: {_manager.networkAddress}");
                _manager.StartClient();
            }
        }

        /// <summary>
        /// Automatically host when we are in the editor or running a server build
        /// </summary>
        public static bool ShouldHost() => Application.isEditor | Application.isBatchMode;
        
        private void EnsureOfflineScene()
        {
            //If we are offline
            if (_manager.mode != NetworkManagerMode.Offline) return;
            //Offline scene isnt null
            if (_manager.offlineScene == null) return;
            //Make sure we aren't already in the offline scene
            if (SceneManager.GetActiveScene().name == _manager.offlineScene) return;

            print("NetworkManager is offline, loading offline scene");
            SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene());
            SceneManager.LoadScene(_manager.offlineScene, LoadSceneMode.Single);
        }
        
        private void Awake()
        {
            _manager = GetComponent<NetworkManager>();
        }
    }
}