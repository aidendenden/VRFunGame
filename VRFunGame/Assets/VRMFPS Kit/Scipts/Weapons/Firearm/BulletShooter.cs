using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

namespace VRMultiplayerFPSKit
{
    /// <summary>
    /// Shoots a physical bullet projectile, as opposed to say shotgun pellets
    /// </summary>
    public class BulletShooter : NetworkBehaviour
    {
        public GameObject bulletPrefab;
        public Transform bulletSpawn;

        /// <summary>
        /// Calls for server to shoot a bullet with specified properties
        /// </summary>
        /// <param name="cartridge">Specifies bullet properties</param>
        [Command]
        public void ShootCartridge(Cartridge cartridge)
        {
            GameObject obj = Instantiate(bulletPrefab, bulletSpawn.position, bulletSpawn.rotation);
            Bullet bullet = obj.GetComponent<Bullet>();

            bullet.bulletType = cartridge.bulletType;
            
            NetworkServer.Spawn(obj);
        }

        private void Awake()
        {
            GetComponentInParent<Firearm>().ShootEvent += ShootCartridge;
        }
    }
}