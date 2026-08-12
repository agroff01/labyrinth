using CustomUtils;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Labyrinth
{
    public class PlayerMovementInput : MonoBehaviour, IMovementInput
    {

        public InputActionReference movementAction = null;
        public InputActionReference JumpAction = null;
        public InputActionReference SprintAction = null;
        public Transform ForwardDirectionReference = null;
        public float inputThreshold = .2f;


        [Space(15), SerializeField, Header("Runtime")] private Vector2 rapidUpdateDirection = Vector2.zero;
        [SerializeField] private int pausedMutex = 0;


        public Vector3 worldDirection => GetWorldspaceInput();
        public Vector3 rawDirection => new (rapidUpdateDirection.x, 0, rapidUpdateDirection.y);
        public Pose ForwardPose => ForwardDirectionReference ? ForwardDirectionReference.GetPose() : transform.GetPose();
        public bool IsPaused => pausedMutex > 0;
        public bool JumpPressedThisFrame => JumpAction != null && JumpAction.action.WasPressedThisFrame();
        public bool IsSprintHeld => SprintAction != null && SprintAction.action.IsPressed();


        void Awake()
        {
            if (!ForwardDirectionReference && Camera.main != null) ForwardDirectionReference = Camera.main.transform;
        }

        void OnEnable()
        {
            if (movementAction != null)
            {
                movementAction.action.performed += InputCallback;
                movementAction.action.canceled += InputCallback;
            }
            else
            {
                Debug.LogError("Provided input action does not exist or does not return a Vector2 " + movementAction.action.activeValueType, this);
            }
        }

        void OnDisable()
        {
            if (movementAction != null)
            {
                movementAction.action.performed -= InputCallback;
                movementAction.action.canceled -= InputCallback;
                rapidUpdateDirection = Vector2.zero;
            }
            
        }
        void InputCallback(InputAction.CallbackContext con)
        {
            // End early if something has paused movement
            if (pausedMutex > 0) {
                rapidUpdateDirection = Vector2.zero;
                return;
            }

            if (con.performed)
            {
                rapidUpdateDirection = con.ReadValue<Vector2>();
            }
            if (con.canceled || rapidUpdateDirection.AbsCopy().Max() < inputThreshold)
            {
                rapidUpdateDirection = Vector2.zero;
            }

        }


        public Vector3 GetRawInput() => movementAction != null ? rapidUpdateDirection : Vector3.zero;

        public Vector3 GetWorldspaceInput()
        {
            var forwards = ForwardPose.AsFlat();


            var input = GetRawInput().xzy();

            return (forwards.rotation * input).normalized;

        }

        public void PauseMovement() => pausedMutex += 1 ;
        public void UnpauseMovement() 
        {
            if (pausedMutex > 0) 
            {
                pausedMutex--;
            } 
            else 
            {
                Debug.LogWarning("Attempted to unpause the player's movement when it was already unpaused. Try to only do this once after pause is already called.");
            }
        }

    }
}