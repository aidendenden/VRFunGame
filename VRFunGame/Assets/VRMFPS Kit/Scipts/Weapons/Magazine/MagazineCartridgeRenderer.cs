using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VRMultiplayerFPSKit
{
    /// <summary>
    /// Makes the Cartridge Renderer render a cartridge from the parent magazine according to our sibling index
    /// </summary>
    [RequireComponent(typeof(CartridgeRenderer))]
    public class MagazineCartridgeRenderer : MonoBehaviour
    {
        private Magazine _magazine;
        private CartridgeRenderer _cartridgeRenderer;

        private void Update()
        {
            _cartridgeRenderer.cartridgeToRender = GetCartridgeToRender();
        }

        private Cartridge GetCartridgeToRender()
        {
            //Start at the top cartridge, and then work back with every sibling index
            int cartridgeIndex = (_magazine.cartridges.Count - 1) - transform.GetSiblingIndex();
            
            //Don't render if index is outside list
            if (cartridgeIndex < 0)
                return Cartridge.Empty;
            
            return _magazine.cartridges[cartridgeIndex];
        }
        
        private void Awake()
        {
            _cartridgeRenderer = GetComponent<CartridgeRenderer>();
            _magazine = GetComponentInParent<Magazine>();
            if (!_magazine)
                Debug.LogError("No parent Magazine component was found");
        }
    }
}