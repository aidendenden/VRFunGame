using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace UltimateFPSSpriteSystem
{
    public class Door : MonoBehaviour
    {
        public Animator anim;
        public bool requireBlueKey;
        public bool requireRedKey;
        public bool requireYellowKey;
        private TMP_Text doorText;

        private void Start()
        {
            doorText = GameObject.Find("DoorText").GetComponent<TMP_Text>();
        }
        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.CompareTag("Player") || other.gameObject.CompareTag("Monster"))
            {
                //if I don't require any keys
                if (!(requireBlueKey || requireRedKey || requireYellowKey) || other.gameObject.CompareTag("Monster"))
                {
                    anim.Play("DoorOpen");
                }
                else if (other.gameObject.GetComponent<FPSController>())
                {
                    bool unlocked = true;
                    FPSController player = other.gameObject.GetComponent<FPSController>();
                    if (requireBlueKey == true && player.hasBlueKey == false)
                    {
                        unlocked = false;
                        doorText.text = "Requires Blue Key Card";
                        Invoke("fadeOutText", 1.5f);
                    }
                    if (requireRedKey == true && player.hasRedKey == false)
                    {
                        unlocked = false;
                        doorText.text = ("Requires Red Key Card");
                        Invoke("fadeOutText", 1.5f);
                    }
                    if (requireYellowKey == true && player.hasYellowKey == false)
                    {
                        unlocked = false;
                        doorText.text = ("Requires Yellow Key Card");
                        Invoke("fadeOutText", 1.5f);
                    }
                    if(unlocked == true)
                    {
                        anim.Play("DoorOpen");
                    }
                }

            }
        }
        void fadeOutText()
        {
            doorText.text = "";
        }
    }
}
