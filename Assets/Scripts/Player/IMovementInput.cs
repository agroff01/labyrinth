using UnityEngine;


namespace Labyrinth
{
    public interface IMovementInput
    {
        public Vector3 rawDirection {get;}
        public Vector3 worldDirection {get;}
        public bool JumpPressedThisFrame {get;}
        public bool IsSprintHeld {get;}

        public Vector3 GetWorldspaceInput();
        public Vector3 GetRawInput();

        public void PauseMovement();
        public void UnpauseMovement();
    }

}