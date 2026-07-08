using UnityEngine;

namespace CustomUtils
{
    public class DisableGameObject : MonoBehaviour
    {
        private void Start()
        {
            gameObject.SetActive(false);
        }
    }
}