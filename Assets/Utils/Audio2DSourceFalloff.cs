using System;
using UnityEngine;

namespace CustomUtils
{
    public class Audio2DSourceFalloff : MonoBehaviour
    {
        public AudioSource audioSource;


        [Header("Runtime")]
        float startingVolume = 1;
        float maxDistance = 0;
        AnimationCurve savedVolumeCurve;
        AudioListener audioListener;

        void Awake()
        {
            if (!audioSource) audioSource = GetComponent<AudioSource>();
            if (!audioListener) audioListener = FindFirstObjectByType<AudioListener>();
            if (audioSource)
            {
                startingVolume = audioSource.volume;
                maxDistance = audioSource.maxDistance;
                savedVolumeCurve = audioSource.GetCustomCurve(AudioSourceCurveType.CustomRolloff);
            }

            UpdateVolume();
        }

        void Update()
        {
            UpdateVolume();
        }

        void UpdateVolume()
        {
            if (!audioSource || !audioListener || savedVolumeCurve == null || maxDistance == 0)
            {
                throw new NullReferenceException("2D Audio cannot be given falloff if factor is null.");
                
            }

            var expectedVolumeMultiplier = savedVolumeCurve.Evaluate(Vector3.Distance(audioListener.transform.position, audioSource.transform.position)/maxDistance);
            audioSource.volume = Mathf.Lerp(0, startingVolume, expectedVolumeMultiplier);
        }
        
    }
}
