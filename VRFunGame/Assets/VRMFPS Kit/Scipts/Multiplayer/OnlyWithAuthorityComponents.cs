using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.Serialization;

namespace VRMultiplayerFPSKit
{
    /// <summary>
    /// Script used to disable certain components if we don't have network authority over the object
    /// </summary>
    public class OnlyWithAuthorityComponents : MonoBehaviour
    {
        public List<Behaviour> authorityComponents;
        private NetworkIdentity _identity;

        // Start is called before the first frame update
        void Awake()
        {
            //Get the network identity on current object or its parents
            _identity = GetComponentInParent<NetworkIdentity>();
        }

        // Update is called once per frame
        void Update()
        {
            //Every frame, enable/disable components depending on whether client has authority
            bool enableComponents = _identity.isOwned;
            foreach (var component in authorityComponents)
            {
                component.enabled = enableComponents;
            }
        }
    }
}