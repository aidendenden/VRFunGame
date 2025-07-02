using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


namespace UltimateFPSSpriteSystem
{
    public class Item : MonoBehaviour
    {
        public int healthToGive = 0;
        public string weaponToGive = "";
        public int ammoIndexToGive = 0;
        public int ammoToGive = 0;
        public int weaponSlot;
        public bool givesBlueKey;
        public bool givesRedKey;
        public bool givesYellowKey;
        private AudioSource audio;
        private void Start()
        {
            audio = gameObject.GetComponent<AudioSource>();
        }
        // Start is called before the first frame update
        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.GetComponent<FPSController>())
            {
                FPSController player = other.gameObject.GetComponent<FPSController>();
                if (givesBlueKey) { player.hasBlueKey = true; GameObject.Find("BKEY").GetComponent<Image>().enabled = true; }
                if (givesRedKey) { player.hasRedKey = true; GameObject.Find("RKEY").GetComponent<Image>().enabled = true; }
                if (givesYellowKey) { player.hasYellowKey = true; GameObject.Find("YKEY").GetComponent<Image>().enabled = true; }
                if (healthToGive > 0) player.gameObject.GetComponent<Health>().health += healthToGive;
                if (weaponToGive != "")
                {
                    if(player.weapon[weaponSlot - 1] != weaponToGive)
                    {
                        player.weapon[weaponSlot - 1] = weaponToGive;
                        player.currentWeapon = weaponToGive;
                        GameObject.Find("ARMS"+weaponSlot).GetComponent<TMP_Text>().color = Color.yellow;
                        player.ammoIndex = ammoIndexToGive;
                        player.ammo[ammoIndexToGive] += ammoToGive;
                        //giveAmmoWeapon(player, true);
                    }
                    else
                    {
                        player.weapon[weaponSlot - 1] = weaponToGive;
                        player.ammo[ammoIndexToGive] += ammoToGive;
                        //giveAmmoWeapon(player, false);
                    }
                    
                }
                if(ammoToGive > 0)
                {
                    player.ammo[ammoIndexToGive] += ammoToGive;
                }
                audio.Play();
                gameObject.GetComponent<CapsuleCollider>().enabled = false;
                gameObject.GetComponent<SpriteRenderer>().enabled = false;
                //when all done destroy me
                Destroy(gameObject, 0.3f);
            }
        }
       /* void giveAmmoWeapon(FPSController player, bool switchAmmo)
        {
            if (weaponToGive == "RocketLauncher")
            {
                player.ammo[2] += 5;
                if (switchAmmo) player.ammoIndex = 2;
            }
            else if (weaponToGive == "Shotgun")
            {
                player.ammo[1] += 6;
                if (switchAmmo) player.ammoIndex = 1;
            }
            else//default bullets
            {
                player.ammo[0] += 10;
                if (switchAmmo) player.ammoIndex = 0;
            }
        }*/
    }
}
