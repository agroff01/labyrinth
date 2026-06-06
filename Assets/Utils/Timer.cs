using System;
using UnityEngine;

namespace CustomUtils
{
    [Serializable]
    public struct Timer
    {
        [SerializeField] float _targetTime;
        [SerializeField] float _duration;
        public bool UseUnscaledTime;

        public readonly bool IsRunning => _targetTime > 0;
        public readonly float Duration => _duration;
        readonly float TimeReference => UseUnscaledTime ? Time.unscaledTime : Time.time;

        public static Timer None => default;

        public bool Expired()
        {
            return _targetTime > 0 && _targetTime <= TimeReference;
        }

        public bool ExpiredOrNotRunning()
        {
            return _targetTime == 0 || Expired();
        }


        public float? RemainingTime()
        {
            if (!IsRunning) return null;


            return Mathf.Clamp(_targetTime - TimeReference, 0f, float.MaxValue);
        }

        public static Timer CreateFromSeconds(float delayInSeconds, bool UseUnscaledTime = false)
        {
            Timer result = default;
            result.UseUnscaledTime = UseUnscaledTime;
            result._targetTime = result.TimeReference + delayInSeconds;
            result._duration = delayInSeconds;
            return result;
        }

        public override string ToString()
        {
            return $"Remaining Time [{RemainingTime()} seconds] until target time [{_targetTime}]";
        }
    }
}