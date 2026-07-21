using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CustomInspector;
using UnityEngine;

namespace CustomUtils
{
    public class FollowActiveState : MonoBehaviour, IActive
    {
        [RequireType(typeof(IActive)), SerializeField]
        private MonoBehaviour _targetState = null;
        public IActive TargetState => _targetState as IActive;
        public List<MonoBehaviour> Components = new();

        [Space(10)]
        public UpdateLoopType UpdateFrequency = UpdateLoopType.Update;
        public bool EvaluateOnStartup = true;


        private bool _lastState = true;
        public bool Active => _lastState;

        void Awake()
        {
            if (EvaluateOnStartup) Evaluate(true);
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

        void Evaluate(bool forceRefresh = false)
        {
            if (_targetState == null)
            {
                Debug.LogError("No reference state provided.", this);
                return;
            }

            var newState = TargetState.Active;

            if (newState != _lastState || forceRefresh)
            {
                Components.ForEach(mb => mb.enabled = newState);

                // Record last registered state
                _lastState = newState;
            }
        }

    }
}