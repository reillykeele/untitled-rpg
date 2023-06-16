using System.Linq;
using System.Numerics;
using ReiBrary.Attributes;
using ReiBrary.Extensions;
using ReiBrary.Helpers;
using UnityEditor.PackageManager;
using UnityEngine;
using UntitledRPG.Input;
using static UnityEngine.Rendering.DebugUI.Table;
using Quaternion = UnityEngine.Quaternion;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

namespace UntitledRPG.Actor
{
    // [RequireComponent(typeof(CharacterController))]
    public class PlayerMovementController : MonoBehaviour
    {
        private class GroundedState
        {
            public bool IsGrounded { get; }
            public Vector3 GroundNormal { get; }
            public Vector3 GroundHitPosition { get; }
            public float GroundDistance { get; }
            public GameObject Ground { get; }

            public GroundedState() { }
            public GroundedState(bool isGrounded, Vector3 groundNormal, Vector3 groundHitPosition, float groundDistance, GameObject ground)
            {
                IsGrounded = isGrounded;
                GroundNormal = groundNormal;
                GroundHitPosition = groundHitPosition;
                GroundDistance = groundDistance;
                Ground = ground;
            }
        }

        [SerializeField] private InputReader _input = default;

        [Header("Movement")]
        [SerializeField, Min(0f)] private float _rotationSpeed = 0.1f;
        [SerializeField, Min(0f)] private float _moveSpeed = 5.0f;
        [SerializeField, Min(0f)] private float _coyoteTime = 0.1f;
        [SerializeField, Min(0f)] private float _jumpBuffer = 0.1f;
        [SerializeField, Min(0f)] private float _jumpVelocity = 5f;
        [SerializeField, Min(0f)] private float _gravity = 0.1f;
        [SerializeField, Min(0f)] private float _maxFallSpeed = 5f;
        [SerializeField, Min(0f)] private float _maxSlopeAngle = 30f;
        [SerializeField, Min(0f)] private float _groundCheckDist = 0.1f;
        [SerializeField] private bool _useVariableJump = true;

        [Header("Animation")]
        [SerializeField, Range(0, 1f)] private float _startAnimTime = 0.3f;
        [SerializeField, Range(0, 1f)] private float _stopAnimTime = 0.15f;

        // Components
        [HideInInspector] public Animator _anim;
        // private CharacterController _controller;
        private Rigidbody _rb;
        private CapsuleCollider _collider;
        private Camera _camera;

        // Inputs
        [Header("Inputs")]
        [ReadOnly] public Vector2 MoveInput;
        [ReadOnly] public bool JumpInput = false;

        // State
        [SerializeField, ReadOnly] public bool AllowMovement = true;
        [SerializeField, ReadOnly] private Vector3 _lastPosition;
        [SerializeField, ReadOnly] private float _timeSinceGrounded;
        [SerializeField, ReadOnly] private bool _endJumpEarly;

        [Debuggable] public Vector3 Velocity { get; private set; }
        [Debuggable] public float TimeSinceGrounded => _timeSinceGrounded;
        [Debuggable] public bool IsGrounded => _groundedState.IsGrounded; //&& Vector3.Angle(_groundedState.GroundNormal, Vector3.up) <= _maxSlopeAngle;
        [Debuggable] public bool IsSliding => _groundedState.IsGrounded && Vector3.Angle(_groundedState.GroundNormal, Vector3.up) > _maxSlopeAngle;
        [Debuggable] public bool IsFalling => _groundedState.IsGrounded == false && Velocity.y < -0.5f;
        [Debuggable] public bool IsJumping => _groundedState.IsGrounded == false && Velocity.y >  0.5f;

        private GroundedState _groundedState = new();
        
        [SerializeField, ReadOnly] private Vector3 _movementVelocity;

        void Awake()
        {
            _anim = GetComponent<Animator>();
            // _controller = GetComponent<CharacterController>();
            _rb = GetComponent<Rigidbody>();
            _collider = GetComponent<CapsuleCollider>();
        }

        void Start()
        {
            _camera = Camera.main;

            _lastPosition = transform.position;
        }

        void OnEnable()
        {
            _input.MoveEvent += OnMove;
            _input.JumpEvent += OnJumpStarted;
            _input.JumpCancelledEvent += OnJumpCancelled;
        }

        void OnDisable()
        {
            _input.MoveEvent -= OnMove;
            _input.JumpEvent -= OnJumpStarted;
            _input.JumpCancelledEvent -= OnJumpCancelled;
        }

        void Update()
        {
            if (AllowMovement == false)
                return;

            Velocity = (transform.position - _lastPosition) / Time.deltaTime;
            _lastPosition = transform.position;

            CheckGrounded3(); // TODO: should this be in fixed update..?

            var horizontalMovement = CalculateHorizontalMovement();
            var verticalMovement = CalculateVerticalMovement();

            Move(horizontalMovement * Time.deltaTime);
            Move(verticalMovement * Time.deltaTime);

            // set animations
            _anim.SetBool("IsGrounded", IsGrounded);
            _anim.SetBool("IsFalling", IsFalling || IsSliding);
            _anim.SetBool("IsJumping", IsJumping);

            var horizontalSpeed = Velocity.GetHorizontal().magnitude;
            if (horizontalSpeed == 0)
                _anim.SetFloat("HorizontalVelocity", 0f, _stopAnimTime, Time.deltaTime);
            else
                _anim.SetFloat("HorizontalVelocity", Velocity.GetHorizontal().magnitude, _startAnimTime, Time.deltaTime);
            
            // _anim.SetFloat("VerticalVelocity", Velocity.y);
        }

        /// <summary>
        /// Projects the input movement direction onto the XZ plane. 
        /// </summary>
        /// <returns></returns>
        public Vector3 CalculateInputDirection()
        {
            // Project movement onto the XZ plane
            var camForward = _camera.transform.forward;
            var camRight = _camera.transform.right;

            camForward.y = 0f;
            camRight.y = 0f;

            var inputDirection = camRight.normalized * MoveInput.x + camForward.normalized * MoveInput.y;

            return inputDirection.normalized;
        }

        public Vector3 CalculateHorizontalMovement()
        {
            var inputDirection = CalculateInputDirection();

            // project input movement onto the ground plane
            var projectedMovement = IsGrounded ?
                Vector3.ProjectOnPlane(inputDirection, _groundedState.GroundNormal) :
                inputDirection;

            // look in movement direction
            if (inputDirection.sqrMagnitude != 0)
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(inputDirection), _rotationSpeed);

            projectedMovement *= _moveSpeed;

            return projectedMovement;
        }
        
        public Vector3 CalculateVerticalMovement()
        {
            var movement = new Vector3(0f, _movementVelocity.y, 0f);

            if (IsGrounded == false || IsSliding)
            {
                // falling
                if (JumpInput == false && _endJumpEarly == false && Velocity.y > 0)
                {
                    _endJumpEarly = true;
                }

                _timeSinceGrounded += Time.deltaTime;

                // apply gravity
                if (_useVariableJump)
                {
                    var fallSpeed = _endJumpEarly && movement.y > 0 ? _gravity * 4f : _gravity;

                    movement.y += -fallSpeed * Time.deltaTime; // Mathf.Max(movement.y - fallSpeed * Time.deltaTime, -_maxFallSpeed);
                }
                else
                {
                    movement.y += -_gravity * Time.deltaTime;
                }
            }
            // else if (IsSliding)
            // {
            //     if (JumpInput == false && _endJumpEarly == false && Velocity.y > 0)
            //     {
            //         _endJumpEarly = true;
            //     }
            //
            //     _timeSinceGrounded += Time.deltaTime;
            //
            //     // apply gravity
            //     var fallSpeed = _constGravity; //_endJumpEarly && movement.y > 0 ? _constGravity * 4f : _constGravity;
            //
            //     movement.y = Mathf.Max(movement.y - fallSpeed * Time.deltaTime, -_maxFallSpeed);
            // }
            else // IsGrounded 
            {
                movement.y = 0f;
                _timeSinceGrounded = 0f;

                // check jump
                if (JumpInput && (_timeSinceGrounded <= _coyoteTime /* || TODO: Jump Buffer */))
                {
                    _endJumpEarly = false;

                    movement.y = _jumpVelocity;
                }
            }

            _movementVelocity.y = movement.y;

            return movement;
        }

        public void Move(Vector3 movement)
        {
            var position = transform.position;

            var movementDist = movement.magnitude;

            if (CastSelf(position, movement.normalized, movement.magnitude, out var hit))
            {
                if (hit.distance == 0)
                    return;

                // hit something

                position += movement * (hit.distance / movementDist) + hit.normal * 0.001f * 2;
                
                movement *= (1 - hit.distance / movementDist);

                movement -= hit.normal * Vector3.Dot(movement, hit.normal);

                position += Vector3.ProjectOnPlane(movement, hit.normal).normalized * movement.magnitude;
            }
            else
            {
                // didn't hit anything, we can move
                position += movement;
            }

            transform.position = position;
        }

        /// <summary>
        /// Sets the player's rotation to the <c>lookDirection</c>. Will not rotate the player if
        /// <c>lookRotation</c> is <c>Vector3.Zero</c>.
        /// </summary>
        /// <param name="lookDirection"></param>
        public void SetLookRotation(Vector3 lookDirection)
        {
            if (lookDirection.sqrMagnitude != 0)
                transform.rotation = Quaternion.LookRotation(lookDirection);
        }

        #region Collider Checks

        private bool CheckGrounded()
        {
            var controllerBottom = _collider.bounds.center - new Vector3(0f, _collider.bounds.extents.y, 0f);

            Debug.DrawRay(controllerBottom, Vector3.down * 0.1f);

            return Physics.Raycast(controllerBottom, Vector3.down, 0.1f);
        }

        private void CheckGrounded2()
        {
            var controllerBottom = _collider.bounds.center - new Vector3(0f, _collider.bounds.extents.y, 0f);

            var isGrounded = Physics.Raycast(controllerBottom, Vector3.down, out var hitInfo, _groundCheckDist);
            
            // debug
            Debug.DrawRay(controllerBottom, Vector3.down * _groundCheckDist, Color.green);
            if (isGrounded)
                DebugDrawHelper.DrawSphere(hitInfo.point, 0.05f, Color.green, quality: 1);

            _groundedState = new GroundedState(isGrounded, hitInfo.normal, hitInfo.point, hitInfo.distance, hitInfo.collider?.gameObject);
        }

        private void CheckGrounded3()
        {
            var isGrounded = CastSelf(transform.position, Vector3.down, _groundCheckDist, out var hit);

            // debug 
            if (isGrounded)
                DebugDrawHelper.DrawSphere(hit.point, 0.05f, Color.green, quality: 1);

            _groundedState = new GroundedState(isGrounded, hit.normal, hit.point, hit.distance, hit.collider?.gameObject);
        }
        private bool CheckAbove()
        {
            var controllerTop = _collider.bounds.center + new Vector3(0f, _collider.bounds.extents.y, 0f);
            Debug.DrawRay(controllerTop, Vector3.up * 0.1f);
            return Physics.Raycast(controllerTop, Vector3.down, 0.1f);
        }

        private bool CastSelf(Vector3 pos, Vector3 dir, float dist, out RaycastHit hit)
        {
            var center = _collider.center + pos;
            var radius = _collider.radius;
            var height = _collider.height;

            // Get top and bottom points of collider
            var bottom = center + Vector3.down * (height / 2 - radius);
            var top = center + Vector3.up * (height / 2 - radius);

            var hits = Physics.CapsuleCastAll(top, bottom, _collider.radius, dir, dist)
                .Where(x => x.transform != transform);
            var hitAny = hits.Any();

            var minDist = hitAny ? hits.Select(x => x.distance).Min () : 0f;
            hit = hits.FirstOrDefault(x => x.distance == minDist);

            return hitAny;
        }

        #endregion

        #region Input Handling

        private void OnMove(Vector2 move) => MoveInput = move;

        private void OnJumpStarted() => JumpInput = true;
        private void OnJumpCancelled() => JumpInput = false;

        #endregion
    }
}
