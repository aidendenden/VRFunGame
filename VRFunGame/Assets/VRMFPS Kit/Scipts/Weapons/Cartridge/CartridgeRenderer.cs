using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VRMultiplayerFPSKit
{
    /// <summary>
    /// Modifies render visibility status and renderer material to match with cartridgeToRender
    /// </summary>
    public class CartridgeRenderer : MonoBehaviour
    {
        public Cartridge cartridgeToRender;
        [Space] 
        public Renderer casingRenderer;
        public Renderer bulletRenderer;
        [Space] 
        public BulletTypeMaterial[] bulletTypeMaterials;

        
        // Update is called once per frame
        void Update()
        {
            //TODO only call when cartridgeToRender is updated, this is a really performance intensive call
            
            //Hide renders if we aren't supposed to render anything
            casingRenderer.enabled = (cartridgeToRender.caliber != Caliber.None);
            bulletRenderer.enabled = (cartridgeToRender.bulletType != BulletType.Empty_Casing);

            bulletRenderer.material = GetBulletMaterial();
        }

        private Material GetBulletMaterial()
        {
            foreach (var bulletMaterial in bulletTypeMaterials)
                //Return bullet material if active bullet type matches
                if (cartridgeToRender.bulletType == bulletMaterial.bulletType)
                    return bulletMaterial.bulletMaterial;

            return null;
        }
    }
    
    [Serializable]
    public struct BulletTypeMaterial
    {
        public BulletType bulletType;
        public Material bulletMaterial;
    }
}