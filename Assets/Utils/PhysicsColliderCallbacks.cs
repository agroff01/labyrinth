using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace CustomUtils
{
    public class PhysicsColliderCallbacks : MonoBehaviour
    {
        public UnityEvent<Collision> OnCollisionEnterEvent = new();
        public UnityEvent<Collision> OnCollisionStayEvent = new();
        public UnityEvent<Collision> OnCollisionExitEvent = new();


        public bool OnlyAllowRigidbodiesOnce = false;
        public List<Rigidbody> rigidbodiesHit = new();

        void OnCollisionEnter(Collision collision)
        {
            if (!AlreadyHitBodyThisFixedUpdate(collision.collider)) OnCollisionEnterEvent.Invoke(collision);
        }

        void OnCollisionStay(Collision collision)
        {
            if (!AlreadyHitBodyThisFixedUpdate(collision.collider)) OnCollisionStayEvent.Invoke(collision);
        }

        void OnCollisionExit(Collision collision)
        {
            if (!AlreadyHitBodyThisFixedUpdate(collision.collider)) OnCollisionExitEvent.Invoke(collision);
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
