using UnityEngine;

namespace Labyrinth
{
    public class DebugGridVisualizerCore : MonoBehaviour
    {

        public static DebugGridVisualizerCore Instance = null;

        public bool ShowRoomVisualizers = false;

        void Awake()
        {
            if (Instance == null) Instance = this;
            if (Instance != this) Destroy(this);
        }

    }
}
