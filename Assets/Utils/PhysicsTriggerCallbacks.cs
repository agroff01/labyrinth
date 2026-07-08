using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace CustomUtils
{
    public class PhysicsTriggerCallbacks : MonoBehaviour
    {
        public UnityEvent<Collider> onTriggerEnterEvent = new();
        public UnityEvent<Collider> onTriggerStayEvent = new();
        public UnityEvent<Collider> onTriggerExitEvent = new();

        public bool OnlyAllowRigidbodiesOnce = false;
        public List<Rigidbody> rigidbodiesHit = new();


        void OnTriggerEnter(Collider other)
        {    
            if (!AlreadyHitBodyThisFixedUpdate(other)) onTriggerEnterEvent.Invoke(other);
        }

        void OnTriggerStay(Collider other)
        {
            if (!AlreadyHitBodyThisFixedUpdate(other)) onTriggerStayEvent.Invoke(other);
        }

        void OnTriggerExit(Collider other)
        {
            if (!AlreadyHitBodyThisFixedUpdate(other)) onTriggerExitEvent.Invoke(other);
        }

        void FixedUpdate()
        {
            if (OnlyAllowRigidbodiesOnce) rigidbodiesHit.Clear();
        }
        
        bool AlreadyHitBodyThisFixedUpdate(Collider collider)
        {
            if (!OnlyAllowRigidbodiesOnce) return false;
            Rigidbody hitRB = collider.attachedRigidbody;
            if (hitRB && rigidbodiesHit.Contains(hitRB)) return true;
            rigidbodiesHit.Add(hitRB);
            return false;
            
        }

    }
}
