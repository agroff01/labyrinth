using UnityEngine;


namespace CustomUtils
{
    [ExecuteInEditMode]
    public class Ruler : MonoBehaviour
    {
        public Transform point1;
        public Transform point2;
        public float distance;

        // Update is called once per frame
        void Update()
        {
            if (point1 && point2) distance = Vector3.Distance(point1.position, point2.position);
            else distance = 0;
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.DrawLine(point1.position, point2.position);
        }
    }
}