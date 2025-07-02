using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.AI;
using UnityEngine;
using TMPro;

namespace UltimateFPSSpriteSystem
{
    public class Health : MonoBehaviour
    {
        [Header("Health Settings")]
        public int health = 100;
        public int maxHealth = 100;
        public bool dead;
        public float painChance;
        public GameObject dropItem;
        public Slider bossHealthBar;

        [Header("Player Settings")]
        public bool player;
        public Animator damageHud;
        public TMP_Text healthText;

        private Animator anim;
        private AudioSource audio;
        void Start()
        {
            anim = GetComponentInChildren<Animator>();
            audio = GetComponent<AudioSource>();
        }
        private void Update()
        {
            if(health > maxHealth)
            {
                health = maxHealth;
            }
            if (health < 0 && player)//don't do monster cause x-death
            {
                health = 0;
            }
            if (healthText != null)
                healthText.text = health + "%";
            if (bossHealthBar != null)
            {
                bossHealthBar.value = ((float)health)/maxHealth;
            }
        }
        public void takeDamage(int damage, GameObject source)
        {
            if(dead == false)
            {
                health -= damage;
                if(player == false)
                {
                    if (Random.Range(0, 10) < painChance)
                    {
                        anim.Play("Pain");
                        gameObject.GetComponent<AdvancedAI>().target = source.transform;
                        gameObject.GetComponent<AdvancedAI>().targetSighted = true;
                    }
                    if (health < 0)
                    {
                        if(dropItem != null)
                            Instantiate(dropItem, transform.position, transform.rotation);
                        if (bossHealthBar != null)
                            bossHealthBar.gameObject.SetActive(false);
                        if (health <= -maxHealth)
                        {
                            anim.Play("XDeath");
                        }
                        else
                        {
                            anim.Play("Death");
                        }
                        dead = true;
                        gameObject.tag = "Dead";

                        gameObject.GetComponent<AdvancedAI>().CancelInvoke();
                        if (gameObject.GetComponent<AdvancedAI>().flying == true)
                        {
                            gameObject.GetComponent<CapsuleCollider>().radius /= 2;
                            gameObject.GetComponent<Rigidbody>().useGravity = true;
                            Invoke("removeCollider", 2);
                        }
                        else
                        {
                            gameObject.GetComponent<CapsuleCollider>().enabled = false;
                        }     
                        gameObject.GetComponent<AdvancedAI>().enabled = false;
                        gameObject.GetComponentInParent<NavMeshAgent>().enabled = false;
                    }
                }
                if(player == true)
                {
                    if(health > -1)
                    {
                        damageHud.Play("PainFlash", -1, .0f);
                        gameObject.GetComponent<FPSController>().PlaySound(3);
                    }
                    if (health <= 0)
                    {
                        dead = true;
                        gameObject.tag = "Dead";
                        damageHud.Play("DeadFlash");
                        gameObject.GetComponent<FPSController>().PlaySound(4);
                        gameObject.GetComponent<FPSController>().enabled = false;
                        gameObject.GetComponent<CapsuleCollider>().height /= 4;
                        gameObject.GetComponentInChildren<Animator>().Play("WeaponDie");
                        Invoke("reloadLevel", 2);
                    }
                }
            }
            
        }
        void removeCollider()
        {
            gameObject.GetComponent<Rigidbody>().useGravity = false;
            gameObject.GetComponent<CapsuleCollider>().enabled = false;
        }
        void reloadLevel()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("DamagingFloor"))
            {
                takeDamage(200, other.gameObject);
            }
            if (other.CompareTag("Explosion"))
            {
                takeDamage(80, other.gameObject);
            }
        }
    }
}
