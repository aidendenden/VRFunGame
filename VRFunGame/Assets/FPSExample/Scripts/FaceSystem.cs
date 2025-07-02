using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

namespace UltimateFPSSpriteSystem
{
    public class FaceSystem : MonoBehaviour
    {
        public string spriteName = "STF";
        public string[] faceTypes;
        public int faceIndex;
        public int num;
        private int healthNum;
        public Image spriteRenderer;
        public Health health;

        // Update is called once per frame
        void Update()
        {
            if(health.health >= 2000)//god mode
            {
                healthNum = 0;
                faceIndex = 7;
            }
            else if (health.health <= 0)//dead
            {
                healthNum = 0;
                faceIndex = 6;
            }
            else
            {
                healthNum = (int)(((float)health.health / 20)+0.5f);
                healthNum = 5 - healthNum;
            }

            if (faceIndex == 0)
            {
                //print(spriteName + faceTypes[faceIndex] + healthNum + "" + num);
                if (Resources.Load<Sprite>(spriteName + faceTypes[faceIndex] + healthNum +""+ num) != null)
                {
                    spriteRenderer.sprite = Resources.Load<Sprite>(spriteName + faceTypes[faceIndex] + healthNum +""+ num);
                }
            }
            else if (faceIndex == 1 || faceIndex == 2)
            {
                if (Resources.Load<Sprite>(spriteName + faceTypes[faceIndex] + healthNum + "0") != null)
                {
                    spriteRenderer.sprite = Resources.Load<Sprite>(spriteName + faceTypes[faceIndex] + healthNum + "0");
                }
            }
            else
            {
                if (Resources.Load<Sprite>(spriteName + faceTypes[faceIndex] + healthNum) != null)
                {
                    spriteRenderer.sprite = Resources.Load<Sprite>(spriteName + faceTypes[faceIndex] + healthNum);
                }
            }
        }
    }
}
