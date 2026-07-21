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
        [ShowIf(nameof(overrideGravity))] public float gravity = -9.8f;
        public float AirStrength = .4f;
        public AnimationCurve SpeedAtSlopeAngle = new();

        // Private Variables
        public Vector3 center => mainCollider.transform.TransformPoint(mainCollider.center);

        // // ICharacterMovement interface
        public Vector3 DownwardsDirection { get => GroundedProvider.DownwardDirection;}
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
                Rb.AddForce(gravity * DownwardsDirection);
            }

            if (Input != null && Rb)
            {
                // Has valid Input
                if (Input.worldDirection.sqrMagnitude > Mathf.Epsilon)
                {
                    // Translate World Direction input (world up oriented) into the designated downwards direction from ground provider
                    var DownwardsAlignedInput = Input.worldDirection.RotateByReference(Vector3.up, -GroundedProvider.DownwardDirection);

                    if (IsGrounded)
                    {
                        // Then Project our final intended movement direction onto the plane of the surface below our feet
                        _floorSlopeAdjustedDirection = Vector3.ProjectOnPlane(DownwardsAlignedInput, GroundedProvider.FloorNormal).normalized;
                        
                        // Debug.Log($"Slope Angle: {GroundedProvider.SlopeAngle}");
                        var targetMovement = topSpeed * _floorSlopeAdjustedDirection;
                        if (targetMovement.y > 0)
                        {
                            var speedAtSlope = SpeedAtSlopeAngle.Evaluate(GroundedProvider.SlopeAngle);
                            targetMovement *= speedAtSlope;
                        }
                        

                        var finalVelocity = Vector3.MoveTowards(Rb.linearVelocity, targetMovement, Acceleration * Time.fixedDeltaTime);


                        Rb.linearVelocity = finalVelocity;
                        
                    }
                    // !IsGrounded
                    else
                    {
                        var targetMovement = topSpeed * DownwardsAlignedInput;


                        var verticalVelocity = Vector3.Project(Rb.linearVelocity, DownwardsDirection);
                        var newHorizontalVelocity = Vector3.MoveTowards(
                                                        Vector3.ProjectOnPlane(Rb.linearVelocity, DownwardsDirection),
                                                        targetMovement, 
                                                        Acceleration * Time.fixedDeltaTime * AirStrength);

                        var finalVelocity = verticalVelocity + newHorizontalVelocity;

                        Rb.linearVelocity = finalVelocity;

                        // Debug.Log($"Added Airforce: {newHorizontalVelocity}");


                        
                    }


                    
                }
                // if no valid input and grounded, prevent extreme sliding
                else 
                {
                    if (IsGrounded) Rb.AddForce(-Rb.linearVelocity / 4, ForceMode.VelocityChange);
                }
            }
            // Should really just use RigidbodyReadout.cs component vvv
            Debug.Log($"Final rb velocity of {Rb.linearVelocity} with mag of {Rb.linearVelocity.magnitude}");
        }


        void OnDrawGizmosSelected()
        {
            Gizmos.DrawRay(center, _floorSlopeAdjustedDirection);
        }
    }
}
