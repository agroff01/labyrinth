using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CustomInspector;
using UnityEngine;

namespace CustomUtils
{
    public class ActiveStateCombination : MonoBehaviour, IActive
    {
        
        public enum LogicGateType
        {
            And = 0,
            Or = 1 << 0,
            XOR = 1 << 1,
        }


        [RequireType(typeof(IActive)), SerializeField]
        private List<MonoBehaviour> _components = new();
        public ReadOnlyCollection<IActive> Components => _components.ConvertAll(b => b as IActive).AsReadOnly();
        public LogicGateType LogicType = LogicGateType.And;

        [Space(10)]
        public UpdateLoopType UpdateFrequency = UpdateLoopType.Update;
        public bool EvaluateOnStartup = true;


        private bool _isActive = false;
        public bool Active => _isActive;

        void Awake()
        {
            if (EvaluateOnStartup) Evaluate();
        }

        void Update()
        {
            if (UpdateFrequency == UpdateLoopType.Update
                || (UpdateFrequency == UpdateLoopType.HalfUpdate && Time.frameCount % 2 == 0)
                || (UpdateFrequency == UpdateLoopType.QuarterUpdate && Time.frameCount % 4 == 0))
            {
                Evaluate();
            }
        }

        void FixedUpdate()
        {
            if (UpdateFrequency == UpdateLoopType.FixedUpdate) Evaluate();
        }

        bool Evaluate()
        {
            if (LogicType == LogicGateType.And)
            {
                return _components.TrueForAll(s => (s is IActive a) && a.Active);
            }
            else if (LogicType == LogicGateType.Or)
            {
                return _components.Any(s => (s is IActive a) && a.Active);
            }
            else if (LogicType == LogicGateType.XOR)
            {
                var isEven = true;
                foreach(var item in _components)
                {
                    if (item is IActive a && a.Active)
                    {
                        isEven = !isEven;
                    }
                }
                return isEven;
            }


            Debug.LogError("Selected logic type not accounted for.", this);
            return false;
        }

    }
}
