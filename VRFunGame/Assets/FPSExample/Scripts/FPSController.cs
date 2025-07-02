using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;


namespace UltimateFPSSpriteSystem
{
    public class FPSController : MonoBehaviour
    {
        [Header("Movement Settings")]
        public float moveSpeed = 5f;
        public float jumpForce = 5f;
        public float gravity = 9.8f;
        public bool hasBlueKey;
        public bool hasRedKey;
        public bool hasYellowKey;

        [Header("Mouse Look Settings")]
        public float mouseSensitivity = 2f;
        public Transform cameraTransform;
        private float verticalRotation = 0f;

        [Header("Shooting Settings")]
        public Camera playerCamera;
        public GameObject hitEffect;
        public GameObject bloodEffect;
        public Animator gunMovementAnimator;
        public Animator gunAnimator;
        public string currentWeapon;
        public string[] weapon;
        [Tooltip("0 bullets, 1 shells, 2 rockets")]
        public int[] ammo;
        public TMP_Text ammoText;
        public TMP_Text[] ammoDisplays;
        public int ammoIndex;
        public GameObject[] projectiles;

        [Header("Sound Settings")]
        public AudioClip[] landSound;
        public AudioClip[] pistolSound;
        public AudioClip[] shotgunSound;
        public AudioClip[] painSounds;
        public AudioClip[] deathSounds;
        public AudioClip[] punchSounds;
        public AudioClip[] rocketSounds;

        private string formerWeapon;
        private CharacterController characterController;
        private Vector3 moveDirection;
        private AudioSource audio;
        

        void Start()
        {
            audio = GetComponent<AudioSource>();
            characterController = GetComponent<CharacterController>();
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            for(int i = 1; i < weapon.Length; i++)
            {
                if(weapon[i] != "Fist")
                    GameObject.Find("ARMS" + (i+1)).GetComponent<TMP_Text>().color = Color.yellow;
            }
        }

        void Update()
        {
            HandleMovement();
            HandleMouseLook();
            HandleShooting();
            //weapon numbers
            if(Input.GetKeyDown(KeyCode.Alpha1))
            {
                formerWeapon = currentWeapon;
                currentWeapon = weapon[0];
                
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2) )
            {
                formerWeapon = currentWeapon;
                currentWeapon = weapon[1];
                ammoIndex = 0;
            }
            else if(Input.GetKeyDown(KeyCode.Alpha3))
            {
                formerWeapon = currentWeapon;
                currentWeapon = weapon[2];
                ammoIndex = 1;

            }
            else if(Input.GetKeyDown(KeyCode.Alpha4))
            {
                formerWeapon = currentWeapon;
                currentWeapon = weapon[3];
                ammoIndex = 0;

            }
            else if (Input.GetKeyDown(KeyCode.Alpha5))
            {
                formerWeapon = currentWeapon;
                currentWeapon = weapon[4];
                ammoIndex = 2;

            }
            else if (Input.GetKeyDown(KeyCode.Alpha6))
            {
                formerWeapon = currentWeapon;
                currentWeapon = weapon[5];
                ammoIndex = 3;

            }
            else if (Input.GetKeyDown(KeyCode.Alpha7))
            {
                formerWeapon = currentWeapon;
                currentWeapon = weapon[6];
                ammoIndex = 3;

            }
            if (currentWeapon != formerWeapon)
            {
                //switch weapons
                
                gunAnimator.Play(currentWeapon + "Idle");
            }
            //update text
            ammoText.text = ""+ammo[ammoIndex];
            for(int i = 0; i < ammoDisplays.Length; i++)
            {
                ammoDisplays[i].text = ""+ammo[i];
            }
        }

        void HandleMovement()
        {
            float moveX = Input.GetAxis("Horizontal");
            float moveZ = Input.GetAxis("Vertical");

            Vector3 move = transform.right * moveX + transform.forward * moveZ;
            if(move != Vector3.zero)
            {
                gunMovementAnimator.Play("WeaponMove");
            }
            else
            {
                gunMovementAnimator.Play("WeaponIdle");
            }
            if (characterController.isGrounded)
            {
                moveDirection = move * moveSpeed;
                /*if (Input.GetButtonDown("Jump"))
                {
                    moveDirection.y += jumpForce;
                }*/
            }

            moveDirection.y -= gravity * Time.deltaTime;
            characterController.Move(moveDirection * Time.deltaTime);
        }

        void HandleMouseLook()
        {
            float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
            float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

            verticalRotation -= mouseY;
            verticalRotation = Mathf.Clamp(verticalRotation, -90f, 90f);

            cameraTransform.localRotation = Quaternion.Euler(verticalRotation, 0f, 0f);
            transform.Rotate(Vector3.up * mouseX);
        }

        void HandleShooting()
        {
            if (Input.GetButtonDown("Fire1") && gunAnimator.GetCurrentAnimatorStateInfo(0).IsName(currentWeapon + "Idle"))//normal weapons
            {
                if (currentWeapon == "Fist")
                {
                    formerWeapon = currentWeapon;
                    gunAnimator.Play(currentWeapon + "Fire");
                    ammoIndex = 0;
                    bulletAttack(1, 1, 10);
                }
                if (currentWeapon == "Pistol" && ammo[0] > 0)
                {
                    formerWeapon = currentWeapon;
                    gunAnimator.Play(currentWeapon + "Fire");
                    ammoIndex = 0;
                    PlaySound(1);
                    bulletAttack(100, 1, 10);
                }
                if(currentWeapon == "Shotgun" && ammo[1] > 0)
                {
                    formerWeapon = currentWeapon;
                    gunAnimator.Play(currentWeapon + "Fire");
                    ammoIndex = 1;
                    PlaySound(2);
                    bulletAttack(100, 6, 10);
                }
                if (currentWeapon == "RocketLauncher" && ammo[2] > 0)
                {
                    formerWeapon = currentWeapon;
                    gunAnimator.Play(currentWeapon + "Fire");
                    ammoIndex = 2;
                    PlaySound(2);
                    projectileAttack(0);
                }

            }
            if(Input.GetButton("Fire1") && gunAnimator.GetCurrentAnimatorStateInfo(0).IsName(currentWeapon + "Idle"))//automatic weapons
            {
                if (currentWeapon == "Chaingun" && ammo[0] > 0)
                {
                    formerWeapon = currentWeapon;
                    gunAnimator.Play(currentWeapon + "Fire");
                    PlaySound(1);
                    ammoIndex = 0;
                    bulletAttack(100, 1, 10);
                }
            }
            if(currentWeapon != formerWeapon)
            {
                //switch weapons
                gunAnimator.Play(currentWeapon + "Idle");
            }
        }
        void projectileAttack(int projectileIndex)
        {
            ammo[ammoIndex] -= 1;
            GameObject myProjectile = Instantiate(projectiles[projectileIndex], transform.position + (cameraTransform.forward * 0.5f), cameraTransform.rotation);
            //myProjectile.transform.LookAt(target.transform.position + target.transform.up * 0.02f, myProjectile.transform.up);
            myProjectile.GetComponent<projectile>().source = gameObject;
        }
        void bulletAttack(float shootRange, int numberOfBullets, int shootDamage)
        {
            if (currentWeapon != "Fist")//make punching sound if punched something
            {
                ammo[ammoIndex] -= 1;
            }
            for (int i = 0; i < numberOfBullets; i++)
            {
                int otherDir = 1;
                if(i %2 == 1)
                {
                    otherDir *= -1;
                }
                RaycastHit hit;
                if (Physics.Raycast(playerCamera.transform.position + playerCamera.transform.forward * 0.1f, playerCamera.transform.forward+(playerCamera.transform.right*i*0.01f*otherDir), out hit, shootRange))
                {
                    if (hit.collider.gameObject != gameObject)
                    {
                        if(currentWeapon == "Fist")//make punching sound if punched something
                        {
                            PlaySound(5);
                        }

                        // Apply damage if object has health
                        if (hit.collider.GetComponent<Health>())
                        {
                            hit.collider.GetComponent<Health>().takeDamage(shootDamage, gameObject);
                            // Instantiate hit effect
                            if (bloodEffect != null)
                            {
                                Instantiate(bloodEffect, hit.point, Quaternion.LookRotation(hit.normal));
                            }
                        }
                        else
                        {
                            // Instantiate wall hit effect
                            if (hitEffect != null)
                            {
                                Instantiate(hitEffect, hit.point, Quaternion.LookRotation(hit.normal));
                            }
                        }


                    }
                }
            }
            
        }
        public void PlaySound(int soundType)
        {
            AudioClip[] selectedSounds = null;
            switch (soundType)
            {
                case 0: selectedSounds = landSound; break;
                case 1: selectedSounds = pistolSound; break;
                case 2: selectedSounds = shotgunSound; break;
                case 3: selectedSounds = painSounds; break;
                case 4: selectedSounds = deathSounds; break;
                case 5: selectedSounds = punchSounds; break;
                case 6: selectedSounds = rocketSounds; break;
            }
            if (selectedSounds != null && selectedSounds.Length > 0)
            {
                audio.PlayOneShot(selectedSounds[Random.Range(0, selectedSounds.Length)]);
            }
        }
    }
}