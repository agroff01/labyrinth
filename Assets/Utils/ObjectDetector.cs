using System;
using System.Collections.Generic;
using UnityEngine;

namespace CustomUtils
{
    public class ObjectDetector<T> : MonoBehaviour where T : MonoBehaviour
    {
        public float requiredPresenceTime = 2f; // Time in seconds the object needs to be present
        private float currentPresenceTime = 0f;
        private bool isObjectPresent = false;

        public bool Active => isObjectPresent;
        public float progress => requiredPresenceTime > 0 ? Mathf.Clamp01(currentPresenceTime / requiredPresenceTime) : (currentPresenceTime > 0 ? 1 : 0);
        public bool InProgress => (progress > 0) && (progress < 1);
        public event Action<T> PlayerDetected;

        // This method is called when another collider enters the trigger
        private void OnTriggerEnter(Collider other)
        {
            // Check if the entering object is the one you are interested in (e.g., by tag)
            if (PlayerDetection(other, out var player))
            {
                currentPresenceTime = 0f; // Reset timer when object enters
                Debug.Log("Object entered trigger.");
            }
        }

        // This method is called every frame while another collider stays within the trigger
        private void OnTriggerStay(Collider other)
        {
            if (PlayerDetection(other, out var player))
            {
                currentPresenceTime += Time.deltaTime; // Increment timer
                if (currentPresenceTime >= requiredPresenceTime && !isObjectPresent)
                {
                    Debug.Log($"Object has been present for {requiredPresenceTime} seconds!");
                    PlayerDetected?.Invoke(player);
                    isObjectPresent = true; 
                }
            }
        }

        // This method is called when another collider exits the trigger
        private void OnTriggerExit(Collider other)
        {
            if (PlayerDetection(other, out var player))
            {
                isObjectPresent = false; // Object is no longer present
                currentPresenceTime = 0f; // Reset timer
                Debug.Log("Object exited trigger.");
            }
        }

        List<Rigidbody> rigidbodiesHit = new();

        void FixedUpdate()
        {
            rigidbodiesHit?.Clear();
        }

        bool PlayerDetection(Collider other, out T player)
        {
            player = null;
            Rigidbody hitRB = other.attachedRigidbody;
            if (hitRB && rigidbodiesHit.Contains(hitRB)) return false;
            rigidbodiesHit.Add(hitRB);

            if (hitRB) return hitRB.TryGetComponent(out player);

            return false;
        }
    }
}
