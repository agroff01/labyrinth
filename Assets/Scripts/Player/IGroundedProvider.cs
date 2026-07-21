using UnityEngine;

namespace Labyrinth
{
    public interface IGroundedProvider
    {
        public ICharacterMovement CharacterMovement { get; }
        public Vector3 DownwardDirection { get; }
        public Vector3 FloorNormal { get; }
        public float SlopeAngle { get; }
        public bool IsGrounded { get; }
    }
    
}