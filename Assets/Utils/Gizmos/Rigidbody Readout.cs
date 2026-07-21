using System.Text;
using UnityEditor;
using UnityEngine;

namespace Labyrinth
{
    public class RigidbodyReadout : MonoBehaviour
    {
        public new Rigidbody rigidbody = null;

        public bool showVelocity = false;
        public bool showSpeed = false;
        public bool showMass = false;
        public bool showUsesGravity = false;
        public bool showKinematic = false;

        public Vector3 offset = Vector3.up;
        public GUIStyle style = new();

        #if UNITY_EDITOR
        void OnDrawGizmos()
        {
            if (rigidbody && isActiveAndEnabled)
            {
                var text = new StringBuilder();
                if (showVelocity) text.Append($"Velocity: {rigidbody.linearVelocity}\n");
                if (showSpeed) text.Append($"Magnitude: {(float.IsSubnormal(rigidbody.linearVelocity.magnitude) ? 0 : rigidbody.linearVelocity.magnitude)}\n");
                if (showMass) text.Append($"Mass: {rigidbody.mass}\n");
                if (showUsesGravity) text.Append($"Uses Gravity: {rigidbody.useGravity}\n");
                if (showKinematic) text.Append($"Is Kinematic: {rigidbody.isKinematic}\n");

                Handles.Label(transform.position + offset, text.ToString(), style);
            }
        }
        #endif
    }
}
