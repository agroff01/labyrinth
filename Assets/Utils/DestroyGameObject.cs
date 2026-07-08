using UnityEngine;

namespace CustomUtils
{
    public class DestroyGameObject : MonoBehaviour
    {
        public bool onlyInBuild;

        private void Awake()
        {
            if (onlyInBuild)
            {
                if (!Application.isEditor)
                {
                    DestroyImmediate(gameObject);
                }
            }
            else
            {
                DestroyImmediate(gameObject);
            }
        }
    }
}