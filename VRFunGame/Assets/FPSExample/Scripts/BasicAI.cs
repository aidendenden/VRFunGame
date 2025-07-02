using System.Collections;
using System.Collections.Generic;
using UnityEngine.AI;
using UnityEngine;
namespace UltimateFPSSpriteSystem
{
    public class BasicAI : MonoBehaviour
    {
        public Transform target;
        private NavMeshAgent nav;
        private Animator anim;
        // Start is called before the first frame update
        void Start()
        {
            nav = gameObject.GetComponent<NavMeshAgent>();
            anim = gameObject.GetComponentInChildren<Animator>();
        }

        // Update is called once per frame
        void Update()
        {
            if (Vector3.Distance(transform.position, target.position) > 1)
            {
                nav.SetDestination(target.position);
                nav.isStopped = false;
                anim.Play("Walk");
            }
            else
            {
                nav.isStopped = true;
                anim.Play("Idle");
            }
        }
    }
}