using System.Collections.Generic;
using System.Linq;
using ReiBrary.Attributes;
using ReiBrary.Extensions;
using ReiBrary.Helpers;
using UnityEngine;
using UntitledRPG.Actor;
using UntitledRPG.Actor.Interactable;
using UntitledRPG.Actor.Interface;
using UntitledRPG.Environment;
using UntitledRPG.Input;

namespace UntitledRPG
{
    public class PhysicsCharacterController : MonoBehaviour, IFloatie, IRangedInteracter
    {
        [SerializeField] private InputReader _input = default;

        [SerializeField, Min(0f)] private float _maxSpeed = 1f;
        [SerializeField, Min(0f)] private float _maxAcceleration = 1f;
        [SerializeField, Min(0f)] private float _maxAirAcceleration = 1f;

        [SerializeField, Min(0f)] private float _jumpHeight = 1;
        [SerializeField, Min(0f)] private float _coyoteTime = 0.1f;
        [SerializeField, Min(0f)] private float _jumpBuffer = 0.1f;

        [SerializeField, Min(0f)] private float _maxSlopeAngle = 30f;
        [SerializeField, Min(0f)] private float _maxSnapSpeed = 100f;
        [SerializeField, Min(0f)] private float _snapDistance = 1f;
        [SerializeField] private LayerMask _groundMask = -1;

        [Header("Inputs")] 
        [ReadOnly] public Vector2 MoveInput;
        [ReadOnly] public bool JumpInput = false;

        // Components
        private Rigidbody _rb;
        private CapsuleCollider _collider;

        // State information
        [SerializeField, ReadOnly] private Vector3 _velocity;

        [SerializeField, ReadOnly] private int _updatesSinceGrounded;
        [SerializeField, ReadOnly] private float _timeSinceGrounded;
        [SerializeField, ReadOnly] private int _updatesSinceJump;
        [SerializeField, ReadOnly] private float _timeSinceJump;

        [SerializeField, ReadOnly] private bool _isGrounded;
        [SerializeField, ReadOnly] private Vector3 _groundNormal;
        [SerializeField, ReadOnly] private int _groundContactCount;

        [SerializeField] private bool _isSwimming => _currentWater != null;
        [SerializeField, ReadOnly] private Water _currentWater;

        [Header("Deez")] 
        [SerializeField, Min(0f)] private float _radius = 0.5f;
        [SerializeField, Min(0f)] private float _distance = 0.5f;
        [SerializeField, Range(0f, 2000f)] private float _movementForce = 800f;
        [SerializeField, Range(0f, 2000f)] private float _airMovementForce = 120f;
        [SerializeField, Min(0f)] private float _turnSpeed = 10f;
        [SerializeField, Min(0f)] private float _standingFriction = 4f;
        [SerializeField, Min(0f)] private float _movingFriction = 0f;
        [SerializeField, Min(0f)] private float _groundStickyForce = 10f;
        [SerializeField, Min(0f)] private float _waterFloatForce = 70f;
        [SerializeField, Min(0f)] private float _floatHeight = 0f;
        [SerializeField, Min(0f)] private float _swimDrag = 0f;

        [SerializeField, ReadOnly] private List<RangedInteractable> _rangedInteractablesInRange = new();

        // Constants
        private float _jumpVelocity;
        private float _minSlopeDotProduct;

        void OnValidate()
        {
            _jumpVelocity = Mathf.Sqrt(-2f * Physics.gravity.y * _jumpHeight);
            _minSlopeDotProduct = Mathf.Cos(_maxSlopeAngle * Mathf.Deg2Rad);
        }

        void Awake()
        {
            _rb = GetComponent<Rigidbody>();
            _collider = GetComponent<CapsuleCollider>();

            OnValidate();
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

        void FixedUpdate()
        {
            IllegalMovement();
            return;
        }

        private void IllegalMovement()
        {
            // Check grounded
            // DebugDrawHelper.DrawSphere(_collider.center+ Vector3.down * _distance, _collider.radius, Color.cyan);
            // _isGrounded = Physics.SphereCast(new Ray(_collider.center, Vector3.down), _collider.radius, out var hitInfo, _distance, _groundMask);
            // CastSelf(transform.position  + Vector3.up * 0.1f, Vector3.down, _distance, out _, _groundMask);
            // CastSelfAll(transform.position  + Vector3.up * 0.1f, Vector3.down, _distance, _groundMask);
            _isGrounded = Physics.Raycast(transform.position + Vector3.up * 0.05f, Vector3.down, _distance, _groundMask);

            // Movement
            var moveDir = GetMoveDirection();

            var desiredVelocity = moveDir * _maxSpeed;
            var velocity = desiredVelocity - _rb.velocity.GetXZ();

            var movementForce = _isGrounded ? _movementForce : _airMovementForce;
            // _rb.AddForceAtPosition(velocity * movementForce * Time.deltaTime, Vector3.zero);
            _rb.AddForce(velocity * movementForce * Time.fixedDeltaTime);

            // Rotation
            var rotationDir = GetRotationDirection();
            if (moveDir != Vector3.zero)
            {
                var n = Vector3.Angle(transform.forward.GetXZ(), rotationDir) * Mathf.Sign(Vector3.Cross(transform.forward.GetXZ(), rotationDir).y);
                _rb.MoveRotation(_rb.rotation * Quaternion.AngleAxis(n * _turnSpeed * Time.fixedDeltaTime, Vector3.up));
            }

            if (JumpInput && (_isGrounded || _isSwimming || (_timeSinceGrounded <= _coyoteTime /* || TODO: Jump Buffer */)))
            {
                JumpInput = false;

                _updatesSinceJump = 0;
                _timeSinceJump = 0f;

                // Jump
                _rb.velocity = _rb.velocity.SetY(Mathf.Max(_rb.velocity.y, _jumpVelocity));
            }
            else
            {
                _updatesSinceJump++;
                _timeSinceJump += Time.deltaTime;
            }

            if (_isGrounded)
            {
                _timeSinceGrounded = 0;

                _rb.AddForce(-transform.up  * _groundStickyForce);
            }
            else
            {
                _timeSinceGrounded += Time.deltaTime;
            }

            // Calculate ground friction
            var friction = _isGrounded && (desiredVelocity == Vector3.zero) ? _standingFriction : _movingFriction;
            if (_collider.material.dynamicFriction != friction)
            {
                _collider.material.dynamicFriction = friction;
                _collider.material.staticFriction = friction;
            }

            velocity = _rb.velocity;
            if (_currentWater != null)
            {
                var waterHeight = _currentWater.transform.position.y;
                var distanceUnderwater = waterHeight - (transform.position.y + _floatHeight);

                var floatDelta = Mathf.Max(0f, distanceUnderwater) * _waterFloatForce * Time.fixedDeltaTime / _rb.mass;
                velocity.y = Mathf.MoveTowards(velocity.y, 5f, floatDelta);

                velocity.y *= (1f - _swimDrag * Time.fixedDeltaTime);

                _rb.velocity = velocity;
            }
        }

        private Vector3 GetMoveDirection()
        {
            var norm = Camera.main.transform.forward.GetXZ().normalized;
            var v = -Vector3.Cross(norm, Vector3.up);

            return norm * MoveInput.y + v * MoveInput.x;
        }

        private Vector3 GetRotationDirection() => GetMoveDirection(); 

        private bool TrySnapToGround()
        {
            if (_updatesSinceGrounded > 1 || _updatesSinceJump <= 2 ||
                _velocity.magnitude > _maxSnapSpeed ||
                Physics.Raycast(_rb.position, Vector3.down, out var hitInfo, _snapDistance, _groundMask) == false ||
                hitInfo.normal.y < _minSlopeDotProduct)
                return false;

            _groundContactCount = 1;
            _groundNormal = hitInfo.normal;

            var dot = Vector3.Dot(_velocity, hitInfo.normal);
            if (dot > 0f)
                _velocity = (_velocity - hitInfo.normal * dot).normalized * _velocity.magnitude;

            return true;
        }

        private bool CastSelf(Vector3 pos, Vector3 direction, float dist, out RaycastHit hit, int layerMask = ~0)
        {
            var center = _collider.center + pos;
            var radius = _collider.radius;
            var height = _collider.height;

            // Get top and bottom points of collider
            var bottom = center + Vector3.down * (height / 2 - radius);
            var top = center + Vector3.up * (height / 2 - radius);

            DebugDrawHelper.DrawSphere(top + direction * dist, radius, Color.white);
            DebugDrawHelper.DrawSphere(bottom + direction * dist, radius, Color.white);

            var res = Physics.CapsuleCast(top, bottom, radius, direction, out hit, dist, layerMask);

            Debug.Log($"{res} | {_collider.center} + {pos} = {center}");

            return res;
        }

        private bool CastSelfAll(Vector3 pos, Vector3 direction, float dist, int layerMask = ~0)
        {
            var center = _collider.center + pos;
            var radius = _collider.radius;
            var height = _collider.height;

            // Get top and bottom points of collider
            var bottom = center + Vector3.down * (height / 2 - radius);
            var top = center + Vector3.up * (height / 2 - radius);

            DebugDrawHelper.DrawSphere(top + direction * dist, radius, Color.white);
            DebugDrawHelper.DrawSphere(bottom + direction * dist, radius, Color.white);

            var hits = Physics.CapsuleCastAll(top, bottom, radius, direction, dist, layerMask);
            var any = hits.Any(x => x.transform != transform);

            Debug.Log($"{any} | {_collider.center} + {pos} = {center}");

            return any;
        }

        #region Input Handling

        private void OnMove(Vector2 move) => MoveInput = Vector2.ClampMagnitude(move, 1f);
        private void OnJumpStarted() => JumpInput = true;
        private void OnJumpCancelled() => JumpInput = false;

        #endregion

        #region Floatie

         /// <inheritdoc />
        public void SubscribeToWater(Water water)
        {
            _currentWater = water;
        }

        /// <inheritdoc />
        public void UnsubscribeToWater(Water water)
        {
            _currentWater = null;
        }

        #endregion

        #region Interacter

        public IInteractable GetInteractable()
        {
            if (_rangedInteractablesInRange.Count <= 1)
                return _rangedInteractablesInRange.FirstOrDefault();

            // Sort by priority and distance
            var interactable = _rangedInteractablesInRange
                .GroupBy(x => x.Priority)
                .OrderByDescending(x => x.Key)
                .First()
                .OrderBy(x => Vector3.Distance(transform.position, x.transform.position))
                .First();

            return interactable;
        }

        /// <inheritdoc />
        public void Subscribe(RangedInteractable interactable)
        {
            _rangedInteractablesInRange.Add(interactable);

            GetInteractable().HighlightInteractable();
        }

        /// <inheritdoc />
        public void Unsubscribe(RangedInteractable interactable)
        {
            _rangedInteractablesInRange.Remove(interactable);

            if (_rangedInteractablesInRange.Any())
                GetInteractable().HighlightInteractable();
        }

        #endregion
    }
}
