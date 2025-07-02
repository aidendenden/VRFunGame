using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace UltimateFPSSpriteSystem
{
    public class projectile : MonoBehaviour
    {
        public GameObject source;
        public int damage;
        public float speed;
        private Animator anim;
        private void Start()
        {
            anim = GetComponentInChildren<Animator>();
        }
        // Update is called once per frame
        void Update()
        {
            //hasn't collided yet keep going forward
            if(gameObject.GetComponent<CapsuleCollider>().enabled == true)
                transform.Translate(Vector3.forward * speed * Time.deltaTime);
        }
        private void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.GetComponent<Health>())
            {
                collision.gameObject.GetComponent<Health>().takeDamage(damage, source);
            }
            anim.Play("FireballExplode");
            gameObject.GetComponent<CapsuleCollider>().enabled = false;
            Destroy(gameObject, 0.5f);
    }
    }
}