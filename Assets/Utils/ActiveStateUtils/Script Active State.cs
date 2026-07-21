using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CustomInspector;
using UnityEngine;

namespace CustomUtils
{
    public class ScriptActiveState : MonoBehaviour, IActive
    {
        
        [RequireType(typeof(IActive)), SerializeField]
        private MonoBehaviour _script = new();
        public bool Active => _script.isActiveAndEnabled;

    }
}
