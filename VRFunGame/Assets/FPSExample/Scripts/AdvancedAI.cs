using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace UltimateFPSSpriteSystem
{
    public class AdvancedAI : MonoBehaviour
    {

        [Header("Sound Settings")]
        public AudioClip[] sightSounds;
        public AudioClip[] idleSounds;
        public AudioClip[] attackSounds;
        public AudioClip[] painSounds;
        public AudioClip[] deathSounds;

        [Header("Behaviour Settings")]
        [Tooltip("Keep this low for indoor areas, larger for outdoor areas")]
        public float alertRange = 2;
        [Tooltip("Raycast or projectile attack is defined via animation")]
        public bool hasRangedAttack;
        public bool hasMelee;
        [Tooltip("0-10, includes melee attack")]
        public float raycastAccuracy = 1f;
        [Tooltip("Includes melee attack")]
        public int raycastDamage = 10;
        public bool rapidFire;
        //public bool canReviveOtherAI;
        public bool flying;
        public GameObject bossHealthBar;
        [Tooltip("0-10")]
        public float missileChance;
        [Tooltip("If you want monster to have multiple types of fireballs, there is a custom fireball attack in the animation event.")]
        public GameObject mainProjectile;
        [Header("Target Settings")]
        public string enemyTag = "Player";
        //public Transform[] targets;
        public Transform target;
        public bool stoppedWalking;
        public bool targetSighted;
        private bool stillFiring;

        private bool charging;
        private NavMeshAgent nav;
        private Animator anim;
        private AudioSource audio;
        private float lostTargetTime;
        public float fireRate = 2f;
        private float nextFireTime;

        void Start()
        {
            nav = GetComponentInParent<NavMeshAgent>();
            anim = GetComponentInChildren<Animator>();
            audio = GetComponent<AudioSource>();
            InvokeRepeating("FindTargets", 0f, 0.3f); // Check for targets every 3rd of a second
        }

        void Update()
        {
            if (targetSighted)
            {
                if (target == null || !CanSeeTarget(target) || target.GetComponent<Health>().dead)
                {
                    if (Time.time - lostTargetTime > 5f) // Lost target for too long
                    {
                        Wander();
                    }
                }
                else
                {
                    lostTargetTime = Time.time;
                    ChaseTarget();
                    
                }
            }
            else
            {
                if (anim.GetCurrentAnimatorStateInfo(0).IsName("Walk") || anim.GetCurrentAnimatorStateInfo(0).IsName("Idle") || anim.GetCurrentAnimatorStateInfo(0).IsName("Ready"))
                {
                    anim.Play("Idle");
                }
                //Wander();
            }
        }
        void AlertMonsters()
        {
            GameObject[] allies = GameObject.FindGameObjectsWithTag(gameObject.tag);
            GameObject[] enemies = GameObject.FindGameObjectsWithTag(enemyTag);
            foreach (GameObject enemy in allies)
            {
                AdvancedAI enemyAI = enemy.GetComponent<AdvancedAI>();
                if (enemyAI != null && enemyAI.targetSighted == false && Vector3.Distance(transform.position, enemy.transform.position) <= alertRange)
                {
                    enemyAI.Wander(target);
                }
            }
            foreach (GameObject enemy in enemies)
            {
                AdvancedAI enemyAI = enemy.GetComponent<AdvancedAI>();
                if (enemyAI != null && enemyAI.targetSighted == false && Vector3.Distance(transform.position, enemy.transform.position) <= alertRange)
                {
                    enemyAI.Wander(target);
                }
            }
        }
        void FindTargets()
        {
            if(target == null || targetSighted == false || target.GetComponent<Health>().dead)
            {
                GameObject[] potentialTargets = GameObject.FindGameObjectsWithTag(enemyTag);
                List<Transform> validTargets = new List<Transform>();
                stillFiring = false;
                foreach (GameObject obj in potentialTargets)
                {
                    if (IsInFieldOfView(obj.transform) && CanSeeTarget(obj.transform))
                    {
                        validTargets.Add(obj.transform);
                    }
                }

                if (validTargets.Count > 0)
                {
                    if (targetSighted == false)
                    {
                        //add a bit of delay so that monster dosen't immediately shoot at you
                        nextFireTime = Time.time + 2;
                        PlaySound(0); // Sight sound
                        if (bossHealthBar != null)
                        {
                            bossHealthBar.gameObject.SetActive(true);
                        }
                    }
                        
                    target = GetClosestTarget(validTargets);
                    targetSighted = true;
                    lostTargetTime = Time.time;

                }
                else
                {
                    targetSighted = false;
                }
            }
            
        }

        bool IsInFieldOfView(Transform obj)
        {
            Vector3 directionToTarget = (obj.position - transform.position).normalized;
            float angle = Vector3.Angle(transform.forward, directionToTarget);
            return angle < 120f; // 180-degree field of view (90 degrees on each side)
        }

        bool CanSeeTarget(Transform obj)
        {
            RaycastHit hit;
            Vector3 direction = (obj.position + obj.transform.up * 0.13f - (transform.position + transform.up * 0.01f)).normalized;
            if (Physics.Raycast(transform.position + transform.up * 0.01f, direction, out hit, Mathf.Infinity))
            {
                return hit.transform == obj;
            }
            return false;
        }
        GameObject CanShootTarget(Transform obj)
        {
            RaycastHit hit;
            Vector3 direction = (obj.position + obj.transform.up * 0.13f - transform.position).normalized;
            if (Physics.Raycast(transform.position, direction, out hit, Mathf.Infinity))
            {
                if (hit.transform.gameObject.GetComponent<Health>())
                {
                    return hit.transform.gameObject;
                }
                else
                {
                    return null;
                }
                    
            }
            return null;
        }
        GameObject CanShootTarget(Transform obj, float range)
        {
            RaycastHit hit;
            Vector3 direction = (obj.position + obj.transform.up * 0.13f - transform.position).normalized;
            if (Physics.Raycast(transform.position, direction, out hit, range))
            {
                if (hit.transform.gameObject.GetComponent<Health>())
                {
                    return hit.transform.gameObject;
                }
                else
                {
                    return null;
                }

            }
            return null;
        }
        Transform GetClosestTarget(List<Transform> targets)
        {
            Transform closest = null;
            float minDistance = float.MaxValue;

            foreach (Transform t in targets)
            {
                float distance = Vector3.Distance(transform.position, t.position);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    closest = t;
                }
            }
            return closest;
        }

        void ChaseTarget()
        {
            if (target != null)
            {
                if(charging == true && anim.GetCurrentAnimatorStateInfo(0).IsName("Walk"))
                {
                    anim.Play("Ready");
                }
                if (Vector3.Distance(target.position, transform.position) < 1 && hasMelee == false)
                {
                    if (anim.GetCurrentAnimatorStateInfo(0).IsName("Walk") || anim.GetCurrentAnimatorStateInfo(0).IsName("Idle"))
                    {
                        anim.Play("Ready");

                        nav.isStopped = true;
                        transform.LookAt(target);
                    }
                    if (hasRangedAttack && Time.time >= nextFireTime && CanSeeTarget(target))
                    {
                        if (Random.Range(0, 1) < missileChance || (rapidFire && stillFiring))
                        {

                            AlertMonsters();
                            anim.Play("Fire");
                            nav.isStopped = true;
                            transform.LookAt(target);
                            stillFiring = true;
                        }
                        if (rapidFire && stillFiring)
                            nextFireTime = Time.time + fireRate;
                        else
                            nextFireTime = Time.time + fireRate - Random.Range(1.0f, 2.0f);
                    }
                }
                else if (Vector3.Distance(target.position, transform.position) < .5f && hasMelee == true)
                {

                    anim.Play("Melee");
                    nav.isStopped = true;
                    transform.LookAt(target);
                }
                else
                {
                    if (anim.GetCurrentAnimatorStateInfo(0).IsName("Walk") || anim.GetCurrentAnimatorStateInfo(0).IsName("Idle") || anim.GetCurrentAnimatorStateInfo(0).IsName("Ready") && !(rapidFire == true && stillFiring == true))
                    {
                        anim.Play("Walk");
                        nav.isStopped = false;
                        nav.SetDestination(target.position);
                    }
                    if (hasRangedAttack && Time.time >= nextFireTime && CanSeeTarget(target))
                    {
                        if (Random.Range(0, 10) < missileChance || (rapidFire && stillFiring))
                        {

                            AlertMonsters();
                            anim.Play("Fire");
                            nav.isStopped = true;
                            transform.LookAt(target);
                            stillFiring = true;
                        }
                        if (rapidFire && stillFiring)
                            nextFireTime = Time.time + fireRate;
                        else
                            nextFireTime = Time.time + fireRate + Random.Range(0.0f, 1.0f);
                    }
                }
            }
        }
        void Wander()
        {
            if (!nav.hasPath)
            {
                Vector3 randomDirection = Random.insideUnitSphere * 10f;
                randomDirection += transform.position;
                NavMeshHit hit;
                if (NavMesh.SamplePosition(randomDirection, out hit, 10f, NavMesh.AllAreas))
                {
                    nav.SetDestination(hit.position);
                }
                anim.Play("Walk");
            }
        }
        void Wander(Transform newPath)
        {
            nav.SetDestination(newPath.position);
            anim.Play("Walk");
        }

        public void PlaySound(int soundType)
        {
            AudioClip[] selectedSounds = null;
            switch (soundType)
            {
                case 0: selectedSounds = sightSounds; break;
                case 1: selectedSounds = idleSounds; break;
                case 2: selectedSounds = attackSounds; break;
                case 3: selectedSounds = painSounds; break;
                case 4: selectedSounds = deathSounds; break;
            }
            if (selectedSounds != null && selectedSounds.Length > 0)
            {
                audio.PlayOneShot(selectedSounds[Random.Range(0, selectedSounds.Length)]);
            }
        }
        public void raycastAttack()
        {
            transform.LookAt(target);
            GameObject hitTarget = CanShootTarget(target);
            if (hitTarget != null || Vector3.Distance(target.position, transform.position) < 0.5f)
            {
                if(Random.Range(0, 10) < raycastAccuracy)
                {
                    //damage
                    hitTarget.GetComponent<Health>().takeDamage(raycastDamage, gameObject);
                }
            }
        }
        public void raycastAttack(float range)
        {
            transform.LookAt(target);
            GameObject hitTarget = CanShootTarget(target, range);
            if (hitTarget != null || Vector3.Distance(target.position, transform.position) < 0.5f)
            {
                if (Random.Range(0, 10) < raycastAccuracy)
                {
                    //damage
                    hitTarget.GetComponent<Health>().takeDamage(raycastDamage, gameObject);
                }
            }
        }
        public void projectileAttack(GameObject projectile)
        {
            transform.LookAt(target);
            GameObject myProjectile = Instantiate(projectile, transform.position + (transform.forward * 0.6f), transform.rotation);
            //myProjectile.transform.LookAt(target.transform.position + target.transform.up * 0.02f, myProjectile.transform.up);
            myProjectile.GetComponent<projectile>().source = gameObject;
        }
        public void projectileAttack()
        {
            transform.LookAt(target);
            GameObject myProjectile = Instantiate(mainProjectile, transform.position + (transform.forward * 0.6f), transform.rotation);
            //myProjectile.transform.LookAt(target.transform.position + target.transform.up * 0.02f, myProjectile.transform.up);
            myProjectile.GetComponent<projectile>().source = gameObject;
        }
        public void chargeAttack()//this function requires a rigidbody to work correctly!
        {
            transform.LookAt(target);
            gameObject.GetComponent<Rigidbody>().AddForce(transform.forward * 10,ForceMode.VelocityChange);
            charging = true;
        }
        private void OnCollisionEnter(Collision collision)
        {
            if (charging)
            {
                raycastAttack(0.2f);
                charging = false;
                transform.parent.position = transform.position;
                transform.localPosition = new Vector3(0,transform.localPosition.y,0);
                gameObject.GetComponent<Rigidbody>().velocity = Vector3.zero;
            }
        }
    }
}