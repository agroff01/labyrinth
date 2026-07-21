using CustomInspector;
using CustomUtils;
using Mono.CSharp;
using UnityEngine;
using UnityEngine.Rendering.Universal.Internal;

namespace Labyrinth
{
    [RequireComponent(typeof(IGroundedProvider))]
    public class PlayerMovement : MonoBehaviour, ICharacterMovement
    {

        [Header("References")]
        [SerializeField] protected CapsuleCollider mainCollider = null;
        [SerializeField] private Rigidbody _rigidbody = null;
        [SerializeField, RequireType(typeof(IMovementInput))] private MonoBehaviour _input = null;
        [SerializeField, RequireType(typeof(IGroundedProvider))] private MonoBehaviour _groundedProvider = null;
        public IGroundedProvider GroundedProvider => _groundedProvider as IGroundedProvider;

        [Header("Variables")]
        public float Acceleration = 10f;
        public float topSpeed = 10;
        public bool overrideGravity = false;
        [ShowIf(nameof(overrideGravity)), SerializeField] private Vector3 gravity = new (0,-9.81f,0);
        public float AirStrength = .4f;
        public AnimationCurve SpeedAtSlopeAngle = new();

        // Private Variables
        public Vector3 center => mainCollider.transform.TransformPoint(mainCollider.center);

        // // ICharacterMovement interface
        public Vector3 DownwardsDirection { get => overrideGravity ? gravity.normalized : Physics.gravity.normalized;}
        public Rigidbody Rb => _rigidbody;
        public IMovementInput Input => _input as IMovementInput;
        public bool IsGrounded => GroundedProvider.IsGrounded;

        // // [HorizontalLine("Runtime", 5, FixedColor.Red)]


        private Vector3 _floorSlopeAdjustedDirection = Vector3.zero;
        void FixedUpdate()
        {
            Rb.useGravity = !overrideGravity;
            if (overrideGravity)
            {
                Rb.AddForce(gravity);
            }

            if (Input != null && Rb)
            {
                // Has valid Input
                if (Input.worldDirection.sqrMagnitude > Mathf.Epsilon)
                {
                    // Translate World Direction input (world up oriented) into the designated downwards direction from ground provider
                    var DownwardsAlignedInput = Input.worldDirection.RotateByReference(-DownwardsDirection, Vector3.up);

                    Vector3 targetMovement;
                    Vector3 floorDirection = -DownwardsDirection;
                    if (IsGrounded)
                    {
                        // Then Project our final intended movement direction onto the plane of the surface below our feet
                        _floorSlopeAdjustedDirection = DownwardsAlignedInput.RotateByReference(GroundedProvider.FloorNormal, -DownwardsDirection);
                        // _floorSlopeAdjustedDirection = Vector3.ProjectOnPlane(DownwardsAlignedInput, GroundedProvider.FloorNormal).normalized;

                        floorDirection = GroundedProvider.FloorNormal;

                        targetMovement = topSpeed * _floorSlopeAdjustedDirection;
                        if (targetMovement.y > 0)
                        {
                            var speedAtSlope = SpeedAtSlopeAngle.Evaluate(GroundedProvider.SlopeAngle);
                            targetMovement *= speedAtSlope;
                        }
                        Debug.Log($"Slope Angle: {GroundedProvider.SlopeAngle} -- Sloped Target Float {targetMovement}");
                    }
                    else
                    {
                        targetMovement = topSpeed * DownwardsAlignedInput;
                    }

                    // Ammount of horizontal movement to be applied from input this frame
                    var perUpdateMoveDelta = Acceleration * Time.fixedDeltaTime * (IsGrounded ? 1 : AirStrength);

                    // Preserve any vertical direction movement from 
                    var verticalVelocity = Vector3.Project(Rb.linearVelocity, floorDirection);
                    var newHorizontalVelocity = Vector3.MoveTowards(
                                                    Vector3.ProjectOnPlane(Rb.linearVelocity, floorDirection),
                                                    targetMovement, 
                                                    perUpdateMoveDelta
                                                );

                    var finalVelocity = verticalVelocity + newHorizontalVelocity;

                    Rb.linearVelocity = finalVelocity;
                    
                }
                // if no valid input and grounded, prevent extreme sliding
                else 
                {
                    if (IsGrounded) Rb.AddForce(-Rb.linearVelocity / 4, ForceMode.VelocityChange);
                }
            }
            // Should really just use RigidbodyReadout.cs component vvv
            // Debug.Log($"Final rb velocity of {Rb.linearVelocity} with mag of {Rb.linearVelocity.magnitude}");
        }


        void OnDrawGizmosSelected()
        {
            Gizmos.DrawRay(center, _floorSlopeAdjustedDirection);
        }
    }
}
