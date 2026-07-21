using UnityEngine;

namespace Labyrinth
{
    public interface ICharacterMovement
    {
        public Vector3 DownwardsDirection {get;}
        public Rigidbody Rb {get;}
        public IMovementInput Input {get;}
        public bool IsGrounded {get;}

    }
}
