using System.Collections;
using System.Runtime.CompilerServices;
using CustomInspector;
using CustomUtils;
using JetBrains.Annotations;
using Mono.CSharp;
using NUnit.Framework.Constraints;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering.Universal.Internal;

namespace Labyrinth
{
    [SelectionBase]
    public class PlayerMovement : MonoBehaviour
    {

        [HorizontalLine("References", 5, FixedColor.Green)]
        // [Header("References")]
        [SerializeField] protected CapsuleCollider mainCollider = null;
        [SerializeField] private Rigidbody _rigidbody = null;
        [SerializeField, RequireType(typeof(IMovementInput))] private MonoBehaviour _input = null;

        [HorizontalLine("Variables", 5, FixedColor.Gray)]
        // [Header("Variables")]
        [Header2("Movement")]
        public float Acceleration = 10f;
        public float TopSpeed = 10;
        public float SprintMultiplier = 1.5f;
        public bool UseStickyMovement = false;
        public bool OverrideGravity = false;
        [ShowIf(nameof(OverrideGravity)), SerializeField] private Vector3 localRelativeGravity = new (0,-9.81f,0);
        public float AirStrength = .4f;
        public AnimationCurve SpeedAtSlopeAngle = new();
        public AnimationCurve SlopeDiagonalSpeed = new();

        [Header2("Jump")]
        public float JumpForce = 10;
        public int MultiJumpCount = 1;

        [Header2("Upright Balance")]
        [Min(0), Max(.5f)] public float UprightDeltaAllowance = float.Epsilon;
        [Min(0)] public float LerpPercentPerSec = .4f;
        
        
        // Raycasting
        [Space(5), Header("Grounded Raycast")]
        public float RaycastSphereRadius = .3f;
        public float MaxRaycastDistance = 1f;
        public float raycastOffset = -1f;
        public LayerMask GroundLayers = default;

        
        [Space(5), Header("Step Ledges Raycast")]
        public bool CheckForSteps = true;

        [ShowIf(nameof(CheckForSteps))]
        public float MaxStepHeight = .3f;

        [ShowIf(nameof(CheckForSteps)), Range(0,1)] 
        public float LowerRaycastHeightPercentage = .3f;

        [ShowIf(nameof(CheckForSteps))]
        public float StepCheckDistance = .4f;

        [ShowIf(nameof(CheckForSteps)), /* Range(0,.9f) */] 
        public float StepSmoothingFactor = .1f;


        [HorizontalLine("Runtime", 5, FixedColor.Red)]
        public bool DrawGroundCheck = false;
        [ShowMethod(nameof(SlopeAngleM), label = "Ground Slope"), ShowMethod(nameof(IsSprintHeldM), label = "Sprinting")]
        [ReadOnly, SerializeField] private bool _grounded = false;
        [ReadOnly, SerializeField] bool _isJumping = false;
        private RaycastHit _lastHit = default;
        [SerializeField] private int _airJumpsRemaining = 0;
        [ReadOnly, SerializeField] private Vector3 _floorSlopeAdjustedDirection = Vector3.zero;


        [HorizontalLine("Callbacks", 5, FixedColor.Blue)]
        public UnityEvent<bool> OnJump = new();
        public UnityEvent OnLanding = new();
        

        // Lambdas
        public Vector3 Center => mainCollider.transform.TransformPoint(mainCollider.center);
        public Vector3 DownwardsDirection { get => OverrideGravity ? transform.TransformDirection(localRelativeGravity).normalized : Physics.gravity.normalized;}
        public Rigidbody Rb => _rigidbody;
        public IMovementInput Input => _input as IMovementInput;

        private bool _canApplyMovement => Input != null && Rb;
        private Vector3 DownwardsAlignedInput => _canApplyMovement ? Input.worldDirection.RotateByReference(UpwardsDirection, Vector3.up): Vector3.zero;
        private float _targetTopSpeed => TopSpeed * (Input.IsSprintHeld && IsGrounded ? SprintMultiplier : 1);
        private bool IsSprintHeldM() => Input.IsSprintHeld;

        // Grounded Raycast Lambda
        public bool IsGrounded => _grounded;
        public RaycastHit? LastSpherecastHit => _grounded ? _lastHit : null;
        public Vector3 UpwardsDirection => -DownwardsDirection;
        public Vector3 FloorNormal => LastSpherecastHit?.normal ?? UpwardsDirection;
        public float SlopeAngle => Vector3.Angle(UpwardsDirection, FloorNormal);
        public float SlopeAngleM() => SlopeAngle;
        
        private Ray GroundCheckRay => new(transform.position + (DownwardsDirection * raycastOffset), DownwardsDirection);



        void OnEnable()
        {
            OnLanding.AddListener(ResetRemainingAirJumps);

            ResetRemainingAirJumps();
        }
        void OnDisable()
        {
            OnLanding.RemoveListener(ResetRemainingAirJumps);
        }


        void FixedUpdate()
        {

            CheckGrounded();
            if (CheckForSteps && IsGrounded) StepUpChecks();

            PreformGravity();

            // Jump Check
            if (Input.JumpPressedThisFrame)
            {
                Jump();
            }

            if (UseStickyMovement)
            {
                GroundStickMovement();
            }
            else
            {
                StandardMovement();
            }
        }

        private void StandardMovement()
        {

            if (!_canApplyMovement) return;

            // Translate World Direction input (world up oriented) into the designated downwards direction from ground spherecast
            
            // Ammount of horizontal movement to be applied from input this frame
            var perUpdateMoveDelta = Acceleration * Time.fixedDeltaTime * (IsGrounded ? 1 : AirStrength);

            Vector3 finalVelocity;
            if (IsGrounded)
            {
                
                finalVelocity = Vector3.MoveTowards(
                                    Rb.linearVelocity,
                                    _targetTopSpeed * DownwardsAlignedInput, 
                                    perUpdateMoveDelta
                                );
                
            }
            else
            {
                // Preserve any vertical direction movement from gravity
                var verticalVelocity = Vector3.Project(Rb.linearVelocity, UpwardsDirection);
                var newHorizontalVelocity = Vector3.MoveTowards(
                                                Vector3.ProjectOnPlane(Rb.linearVelocity, UpwardsDirection),
                                                _targetTopSpeed * DownwardsAlignedInput, 
                                                perUpdateMoveDelta
                                            );

                finalVelocity = verticalVelocity + newHorizontalVelocity;
            }

            
            Rb.linearVelocity = finalVelocity;
        }

        private void GroundStickMovement()
        {
            
            if (!_canApplyMovement) return;

            // Translate World Direction input (world up oriented) into the designated downwards direction from ground provider

            Vector3 targetMovement;
            Vector3 floorDirection = UpwardsDirection;
            if (IsGrounded)
            {
                // Then Project our final intended movement direction onto the plane of the surface below our feet
                _floorSlopeAdjustedDirection = DownwardsAlignedInput.RotateByReference(FloorNormal, UpwardsDirection);

                floorDirection = FloorNormal;

                targetMovement = _targetTopSpeed * _floorSlopeAdjustedDirection;

                
                var slopeOfMovementDirection = Vector3.Angle(targetMovement, DownwardsAlignedInput);
                var isMovingUpSlope = targetMovement.RotateByReference(Vector3.up, UpwardsDirection).y > 0;
                if (isMovingUpSlope)
                {
                    var remappedSlopeSpeed = slopeOfMovementDirection.Remap(0, SlopeAngle, 0, SlopeAngle, SlopeDiagonalSpeed);
                    var speedAtSlope = SpeedAtSlopeAngle.Evaluate(remappedSlopeSpeed);
                    targetMovement *= speedAtSlope;
                }
                // Debug.Log($"Slope Angle: {SlopeAngle} -- Slope of Movement Direction: {slopeOfMovementDirection * (isMovingUpSlope ? 1 : -1)} -- Sloped Target Float: {targetMovement}");
            }
            else
            {
                targetMovement = _targetTopSpeed * DownwardsAlignedInput;
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

        private void PreformGravity()
        {
            Rb.useGravity = !OverrideGravity;
            var worldGravityDirection = Physics.gravity.normalized;

            if (OverrideGravity)
            {
                Rb.AddForce(transform.TransformDirection(localRelativeGravity), ForceMode.Acceleration);
            }
            else if (Vector3.Dot(-worldGravityDirection, Rb.transform.up) < 1 - UprightDeltaAllowance)
            {
                var uprightRotation = Quaternion.Slerp(Rb.transform.rotation, Quaternion.LookRotation(Vector3.ProjectOnPlane(Rb.transform.forward, worldGravityDirection), -worldGravityDirection), LerpPercentPerSec * Time.fixedDeltaTime);
                Rb.transform.rotation = uprightRotation;
                // Debug.Log("Attempting to upright with rotation: " + uprightRotation);
            }
        }

        private void StepUpChecks()
        {
            // if movement blocked or no input applied then dont bother
            if (!_canApplyMovement || Input.worldDirection.sqrMagnitude <= float.Epsilon) return;

            var obstacleCheckOrigin = Rb.position + (MaxStepHeight * LowerRaycastHeightPercentage * UpwardsDirection);
            var movementDirection = DownwardsAlignedInput;

            if (Physics.Raycast(obstacleCheckOrigin, movementDirection, StepCheckDistance, GroundLayers, QueryTriggerInteraction.Ignore))
            {
                obstacleCheckOrigin = Rb.position + (MaxStepHeight * UpwardsDirection);
                
                // If the lower ray hits an object, but the upper ray does NOT, it's a step!
                if (!Physics.Raycast(obstacleCheckOrigin, movementDirection, StepCheckDistance, GroundLayers, QueryTriggerInteraction.Ignore))
                {
                    // Smoothly lift the Rigidbody up by the step height
                    var delta = (StepSmoothingFactor * Time.fixedDeltaTime * Vector3.up).RotateByReference(UpwardsDirection);
                    Rb.position += delta;
                    // Debug.Log("Fixing step height difference with position delta of " + delta);
                }
            }
        }


        void OnDrawGizmosSelected()
        {
            Gizmos.DrawRay(Center, _floorSlopeAdjustedDirection);


            if (DrawGroundCheck)
            {
                var endPos = (GroundCheckRay.direction.normalized * MaxRaycastDistance) + GroundCheckRay.origin;
                PhysicsUtil.DrawWireCapsule(GroundCheckRay.origin, endPos, RaycastSphereRadius);
            }

            if (CheckForSteps)
            {
                // first check (should hit side of step)
                var origin = Rb.position + (MaxStepHeight * LowerRaycastHeightPercentage * UpwardsDirection);
                Gizmos.DrawLine(origin, origin + (DownwardsAlignedInput * StepCheckDistance));

                // second check (should be above step / max step height)
                origin = Rb.position + (MaxStepHeight * UpwardsDirection);
                Gizmos.DrawLine(origin, origin + (DownwardsAlignedInput * StepCheckDistance));
            }
        }

        void CheckGrounded()
        {
            var newGroundedState = Physics.SphereCast(GroundCheckRay, RaycastSphereRadius, out _lastHit, MaxRaycastDistance, GroundLayers, QueryTriggerInteraction.Ignore);

            // Spherecast has a bad habit of not returning the correct surface normal
            // So lets raycast to get the actual normal
            // We can also raycast from the original point as we know the space between the sphere start and the point is clear
            if (newGroundedState && Physics.Raycast(GroundCheckRay.origin, _lastHit.point - GroundCheckRay.origin, out RaycastHit surface, MaxRaycastDistance + RaycastSphereRadius, GroundLayers, QueryTriggerInteraction.Ignore))
            {
                _lastHit.normal = surface.normal;
                if (SpeedAtSlopeAngle.Evaluate(Vector3.Angle(UpwardsDirection, surface.normal)) <= 0 )
                {
                    newGroundedState = false;
                }
            }

            if (_grounded != newGroundedState)
            {
                if (newGroundedState)
                {
                    // Just landed
                    OnLanding.Invoke();
                }
                else
                {
                    // Just left ground
                }
            }

            _grounded = newGroundedState;
        }

        private void Jump()
        {
            bool airJump = !IsGrounded || _isJumping;
            if (airJump && _airJumpsRemaining <= 0) return;

            Rb.linearVelocity = Vector3.ProjectOnPlane(Rb.linearVelocity, DownwardsDirection);
            Rb.linearVelocity += UpwardsDirection * JumpForce;

            if (airJump) _airJumpsRemaining--;
            Debug.Log($"{(airJump ? "Air " : "")}Jumped!");
            OnJump.Invoke(airJump);

            StopCoroutine(nameof(IsJumpingCoroutine));
            StartCoroutine(nameof(IsJumpingCoroutine));
        }
        
        IEnumerator IsJumpingCoroutine()
        {
            _isJumping = true;

            // Wait until velocity is moving back downwards
            yield return new WaitUntil(() => IsGrounded || Rb.linearVelocity.RotateByReference(Vector3.up, UpwardsDirection).y <= 0);

            _isJumping = false;
        }

        private void ResetRemainingAirJumps() => _airJumpsRemaining = MultiJumpCount;
    }


}
