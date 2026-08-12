using UnityEngine;
using UnityEngine.InputSystem;

namespace Labyrinth
{
    public class TimeScale : MonoBehaviour
    {
        public float StepPerPress = .2f;
        public bool UseNewInputSystem = true;

        [Space(5), Header("Old System")]
        public KeyCode FasterKey = KeyCode.RightArrow;
        public KeyCode SlowerKey = KeyCode.LeftArrow;
        public KeyCode ResetKey = KeyCode.UpArrow;
        public KeyCode StopKey = KeyCode.DownArrow;

        [Space(5), Header("New System")]
        public InputActionProperty FasterInputAction = new();
        public InputActionProperty SlowerInputAction = new();
        public InputActionProperty ResetInputAction = new();
        public InputActionProperty StopInputAction = new();


        void OnEnable()
        {
            FasterInputAction.action.Enable();
            SlowerInputAction.action.Enable();
            ResetInputAction.action.Enable();
            StopInputAction.action.Enable();
        }
        void OnDisable()
        {
            FasterInputAction.action.Disable();
            SlowerInputAction.action.Disable();
            ResetInputAction.action.Disable();
            StopInputAction.action.Disable();
        }

        void Update()
        {
            if (UseNewInputSystem)
            {
                // speed up
                if (FasterInputAction.action.WasPressedThisFrame())
                {
                    Time.timeScale += StepPerPress;
                }
                // slow down
                else if (SlowerInputAction.action.WasPressedThisFrame())
                {
                    Time.timeScale -= StepPerPress;
                }
                // reset
                else if (ResetInputAction.action.WasPressedThisFrame())
                {
                    Time.timeScale = 1;
                }
                // stop
                else if (StopInputAction.action.WasPressedThisFrame())
                {
                    Time.timeScale = 0;
                }
            }
            else
            {
                // speed up
                if (Input.GetKeyDown(FasterKey))
                {
                    Time.timeScale += StepPerPress;
                }
                // slow down
                else if (Input.GetKeyDown(SlowerKey))
                {
                    Time.timeScale -= StepPerPress;
                }
                // reset
                else if (Input.GetKeyDown(ResetKey))
                {
                    Time.timeScale = 1;
                }
                // stop
                else if (Input.GetKeyDown(StopKey))
                {
                    Time.timeScale = 0;
                }
                
            }
        }
    }
}
