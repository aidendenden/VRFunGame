using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UltimateFPSSpriteSystem
{
    public class SpriteSystem : MonoBehaviour
    {
        public string spriteName = "SPOS"; // Base sprite set
        public char spriteLetter = 'A'; // index
        private char secondLetter = 'D';
        public bool useNumberAlternate;
        public int numberAlternate = 0; //converts this to letter
        public SpriteRenderer spriteRenderer; // Attach the SpriteRenderer component
        public int spriteAngles = 8; // Number of angles you want (8 angles for example)
                                     //private Sprite[] sprites; // Array to hold the sprites
        private Camera mainCamera;
        private AdvancedAI mainScript;

        void Start()
        {
            mainCamera = Camera.main;
            mainScript = gameObject.GetComponentInParent<AdvancedAI>();
        }

        void Update()
        {
            RotateTowardsCamera();
            if(useNumberAlternate)
            {
                spriteLetter = (char)((int)'A' + numberAlternate - 1);
                secondLetter = (char)((int)'A' + numberAlternate + 2);//this is for rotations that do b and e, or a and d
            }
                
        }
        void RotateTowardsCamera()
        {
            int middleAngle = (spriteAngles / 2) + 1;
            if(mainCamera == null || mainCamera.isActiveAndEnabled == false)
            {
                mainCamera = Camera.main;
            }
            // Get the direction vector from the object to the camera
            Vector3 directionToCamera = mainCamera.transform.position - transform.parent.position;
            Vector3 adjustedDirection = transform.parent.InverseTransformDirection(directionToCamera);
            transform.LookAt(new Vector3(mainCamera.transform.position.x, transform.position.y, mainCamera.transform.position.z));
            // Calculate the angle in degrees
            float angle = Mathf.Atan2(adjustedDirection.x, adjustedDirection.z) * Mathf.Rad2Deg;

            // Normalize the angle to be between 0 and 360 degrees
            if (angle < 0) angle += 360f;

            // Calculate which sprite to use based on the angle
            int spriteIndex = Mathf.FloorToInt((angle / 360f) * spriteAngles) % spriteAngles;
            spriteIndex++;
            int spriteIndexDifference = Mathf.Abs(middleAngle - spriteIndex);

            // Assign the correct sprite to the SpriteRenderer using resources folder
            //for example SPOSA1
            if (Resources.Load<Sprite>(spriteName + spriteLetter + spriteIndex) != null)
            {
                spriteRenderer.sprite = Resources.Load<Sprite>(spriteName + spriteLetter + spriteIndex);
                spriteRenderer.flipX = false;
            }
            //for example SPOSA2A8
            else if (Resources.Load<Sprite>(spriteName + spriteLetter + (middleAngle - spriteIndexDifference) + spriteLetter + (middleAngle + spriteIndexDifference)) != null)
            {
                spriteRenderer.sprite = Resources.Load<Sprite>(spriteName + spriteLetter + (middleAngle - spriteIndexDifference) + spriteLetter + (middleAngle + spriteIndexDifference));
                //flip sprite if duplicate
                if (spriteIndex > middleAngle)
                    spriteRenderer.flipX = true;
                else
                    spriteRenderer.flipX = false;
            }
            //example SPOSA8A2
            else if (Resources.Load<Sprite>(spriteName + spriteLetter + (middleAngle + spriteIndexDifference) + spriteLetter + (middleAngle - spriteIndexDifference)) != null)
            {
                spriteRenderer.sprite = Resources.Load<Sprite>(spriteName + spriteLetter + (middleAngle + spriteIndexDifference) + spriteLetter + (middleAngle - spriteIndexDifference));
                //flip sprite if duplicate
                if (spriteIndex > middleAngle)
                    spriteRenderer.flipX = false;
                else
                    spriteRenderer.flipX = true;
            }
            //example SPOSA2D8
            else if (Resources.Load<Sprite>(spriteName + spriteLetter + (middleAngle - spriteIndexDifference) + secondLetter + (middleAngle + spriteIndexDifference)) != null)
            {
                spriteRenderer.sprite = Resources.Load<Sprite>(spriteName + spriteLetter + (middleAngle - spriteIndexDifference) + secondLetter + (middleAngle + spriteIndexDifference));
                //flip sprite if duplicate
                if (spriteIndex > middleAngle)
                    spriteRenderer.flipX = true;
                else
                    spriteRenderer.flipX = false;
            }
            //for example SPOSH0
            else
            {
                spriteRenderer.sprite = Resources.Load<Sprite>(spriteName + spriteLetter + "0");
                spriteRenderer.flipX = false;
            }

        }
        [Tooltip("0 sight, 1 idle, 2 attack, 3 pain, 4 death")]
        public void PlaySound(int soundType)
        {
            mainScript.PlaySound(soundType);
        }
        public void raycastAttack()
        {
            mainScript.raycastAttack();
        }
        public void rangedRaycastAttack(float range)
        {
            mainScript.raycastAttack(range);
        }
        public void customProjectileAttack(GameObject projectile)
        {
            mainScript.projectileAttack(projectile);
        }
        public void projectileAttack()
        {
            mainScript.projectileAttack();
        }
        public void chargeAttack()
        {
            mainScript.chargeAttack();
        }
    }
}