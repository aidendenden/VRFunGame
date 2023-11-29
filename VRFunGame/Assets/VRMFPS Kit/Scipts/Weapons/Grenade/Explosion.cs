using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Mirror;
using UnityEngine;
using VRMultiplayerFPSKit;
using Object = UnityEngine.Object;

namespace VRMultiplayerFPSKit
{
    /// <summary>
    /// Handles behaviour for the explosion prefab
    /// </summary>
    public class Explosion : NetworkBehaviour
    {
        public float killRadius;
        public float damageRadius;

        // Start is called before the first frame update
        public override void OnStartServer()
        {
            //Explode immediately
            
            List<Damageable> damageables = GetAllDamageableInsideRadius();
            List<Damageable> damageablesWithLineOfSight = LineOfSightFilter(damageables);
            
            //Calculate damage for every Damageable component
            foreach (var damageable in damageablesWithLineOfSight)
            {
                float distanceToCenter = Vector3.Distance(damageable.transform.position, transform.position);
                float damageToApply = CalculateDamage(distanceToCenter);

                damageable.TakeDamage(damageToApply);
            }

            //Schedule explosion removal, make sure sound & Particles have time to play
            Invoke(nameof(DestroyObject), 5);
        }

        /// <summary>
        /// Calculates the damage that should be applied based on distance
        /// </summary>
        /// <param name="distance">The distance to the center</param>
        /// <returns>Damage amount</returns>
        private float CalculateDamage(float distance)
        {
            if (distance <= killRadius)
                return 100;
            if (distance > damageRadius)
                return 0;

            //Calculate linear damage falloff after the kill radius
            float distanceFromOuterRadius = damageRadius - distance;
            float damageZoneFalloff01 = distanceFromOuterRadius / (damageRadius - killRadius);

            return damageZoneFalloff01 * 100;
        }
        
        /// <summary>
        /// Returns every object with a Damageable inside of thee radius
        /// </summary>
        /// <returns>List of all Damageables</returns>
        private List<Damageable> GetAllDamageableInsideRadius()
        {
            Collider[] collidersInSphere = Physics.OverlapSphere(transform.position, damageRadius);
            List<Damageable> damageablesInSphere = new();

            foreach (var collider in collidersInSphere)
            {
                Damageable damageable = collider.GetComponentInParent<Damageable>();

                if (damageable == null)
                    continue;
                if (damageablesInSphere.Contains(damageable))
                    continue;

                damageablesInSphere.Add(damageable);
            }

            return damageablesInSphere;
        }

        /// <summary>
        /// Filters a list of Damageables and removes every entry that
        /// doesn't have a line of sight to the center of the explosion
        /// </summary>
        /// <param name="allDamageables">List to filter</param>
        /// <returns>Filtered list</returns>
        private List<Damageable> LineOfSightFilter(List<Damageable> allDamageables)
        {
            List<Damageable> damageablesWithLineOfSight = new();

            foreach (var damageable in allDamageables)
            {
                float distanceToDamageable = Vector3.Distance(damageable.transform.position, transform.position);
                Vector3 directionOfDamageable = damageable.transform.position - transform.position;

                if (Physics.Raycast(transform.position,
                        directionOfDamageable,
                        out var hit,
                        distanceToDamageable,
                        LayerMask.NameToLayer("Default"))) //Only layer Default blocks explosion ray
                    //If sight line is blocked by a collider that doesn't belong to the damageable, don't add it
                    if (hit.collider.GetComponentInParent<Damageable>() != damageable)
                        continue;

                damageablesWithLineOfSight.Add(damageable);
            }

            return damageablesWithLineOfSight;
        }
        
        private void OnDrawGizmos()
        {
            //Draw damage sphere shapes
            Gizmos.color = new Color(1, 0, 0, .3f);
            Gizmos.DrawSphere(transform.position, killRadius);

            Gizmos.color = new Color(1, 0, 0);
            Gizmos.DrawWireSphere(transform.position, damageRadius);
        }
        
        [Server]
        private void DestroyObject()
        {
            NetworkServer.Destroy(gameObject);
        }
    }
}