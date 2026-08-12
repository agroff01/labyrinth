using CustomInspector;
using CustomUtils;
using UnityEngine;

namespace Labyrinth
{
    public class GroundedProvider : MonoBehaviour, IActive, IGroundedProvider
    {

        [SerializeField, RequireType(typeof(ICharacterMovement))] private MonoBehaviour _movement;
        public ICharacterMovement CharacterMovement => _movement as ICharacterMovement;

        public float RaycastSphereRadius = .3f;
        public float MaxRaycastDistance = 1f;
        public float raycastOffset = -1f;
        public LayerMask GroundLayers = default;

        [HorizontalLine("Runtime", 5, FixedColor.Red)]

        [ShowMethod(nameof(SlopeAngleM), label = "Ground Slope")]
        [ReadOnly, SerializeField] private bool _grounded = false;
        private RaycastHit _lastHit = default;
        public bool IsGrounded => _grounded;
        public bool Active => _grounded;
        public RaycastHit? LastSpherecastHit => _grounded ? _lastHit : null;
        public Vector3 DownwardDirection => CharacterMovement.DownwardsDirection;
        public Vector3 UpwardsDirection => -DownwardDirection;
        public Vector3 FloorNormal => LastSpherecastHit?.normal ?? UpwardsDirection;
        public float SlopeAngle => Vector3.Angle(UpwardsDirection, FloorNormal);
        public float SlopeAngleM() => SlopeAngle;
        
        private Ray GroundCheckRay => new(transform.position + (DownwardDirection * raycastOffset), DownwardDirection);

        void FixedUpdate()
        {
            if (CharacterMovement != null)
            {
                _grounded = Physics.SphereCast(GroundCheckRay, RaycastSphereRadius, out _lastHit, MaxRaycastDistance, GroundLayers, QueryTriggerInteraction.Ignore);

                // Spherecast has a bad habit of not returning the correct surface normal
                // So lets raycast to get the actual normal
                // We can also raycast from the original point as we know the space between the sphere start and the point is clear
                if (_grounded && Physics.Raycast(GroundCheckRay.origin, _lastHit.point - GroundCheckRay.origin, out RaycastHit surface, MaxRaycastDistance + RaycastSphereRadius, GroundLayers, QueryTriggerInteraction.Ignore))
                {
                    _lastHit.normal = surface.normal;
                }
            }
        }


        void OnDrawGizmosSelected()
        {
            var endPos = (GroundCheckRay.direction.normalized * MaxRaycastDistance) + GroundCheckRay.origin;
            PhysicsUtil.DrawWireCapsule(GroundCheckRay.origin, endPos, RaycastSphereRadius);
        }
    }
}
