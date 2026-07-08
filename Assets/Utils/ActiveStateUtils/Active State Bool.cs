using CustomUtils;
using UnityEngine;

namespace CustomUtils
{
    public class ActiveStateBool : MonoBehaviour, IActive
    {
        [SerializeField] private bool IsActive = false;
        public bool Active => IsActive;
    }
}
