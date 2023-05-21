using ReiBrary.Attributes;
using ReiBrary.Extensions;
using UnityEngine;
using UntitledRPG.Input;

namespace UntitledRPG.Actor
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerMovementController : MonoBehaviour
    {
        [SerializeField] private InputReader _input = default;

        [Header("Movement")]
        [SerializeField, Min(0f)] private float _rotationSpeed = 0.1f;
        [SerializeField, Min(0f)] private float _moveSpeed = 5.0f;
        [SerializeField, Min(0f)] private float _coyoteTime = 0.1f;
        [SerializeField, Min(0f)] private float _jumpBuffer = 0.1f;
        [SerializeField, Min(0f)] private float _jumpHeight = 5f;
        [SerializeField, Min(0f)] private float _constGravity = 0.1f;
        [SerializeField, Min(0f)] private float _maxFallSpeed = 5f;

        [Header("Animation")]
        [SerializeField, Range(0, 1f)] private float _startAnimTime = 0.3f;
        [SerializeField, Range(0, 1f)] private float _stopAnimTime = 0.15f;

        // Components
        [HideInInspector] public Animator _anim;
        private CharacterController _controller;
        private Camera _camera;

        // Inputs
        [Header("Inputs")]
        [ReadOnly] public Vector2 MoveInput;
        [ReadOnly] public bool JumpInput = false;

        // State
        [SerializeField, ReadOnly] public bool AllowMovement = true;
        [SerializeField, ReadOnly] public Vector3 Velocity;
        [SerializeField, ReadOnly] private Vector3 _lastPosition;
        [SerializeField, ReadOnly] private float _timeSinceGrounded;
        [SerializeField, ReadOnly] private bool _endJumpEarly;

        public bool IsGrounded => _isGrounded;
        public bool IsFalling => IsGrounded == false && Velocity.y < -0.5f;
        public bool IsJumping => IsGrounded == false && Velocity.y >  0.5f;

        private bool _isGrounded;
        [SerializeField, ReadOnly] private Vector3 _movement;

        void Awake()
        {
            _anim = GetComponent<Animator>();
            _controller = GetComponent<CharacterController>();
        }

        void Start()
        {
            _camera = Camera.main;
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

            _isGrounded = CheckGrounded();

            CalculateHorizontalMovement();
            CalculateVerticalMovement();

            Move();

            // set animations
            _anim.SetBool("IsGrounded", IsGrounded);
            _anim.SetBool("IsFalling", IsFalling);
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

        public void CalculateHorizontalMovement()
        {
            var inputDirection = CalculateInputDirection();

            // project input movement onto the XZ plane
            var adjustedMovement = MoveInput.sqrMagnitude == 0f ?
                transform.forward * (inputDirection.magnitude + .01f) :
                inputDirection;

            // look in movement direction
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(adjustedMovement), _rotationSpeed);

            adjustedMovement *= _moveSpeed;

            _movement.x = adjustedMovement.x;
            _movement.z = adjustedMovement.z;
        }
        
        public void CalculateVerticalMovement()
        {
            if (_isGrounded)
            {
                _movement.y = 0f;
                _timeSinceGrounded = 0f;

                // check jump
                if (JumpInput && (_timeSinceGrounded <= _coyoteTime /* || TODO: Jump Buffer */))
                {
                    _endJumpEarly = false;

                    _movement.y = _jumpHeight;
                }
            }
            else
            {
                if (JumpInput == false && _endJumpEarly == false && Velocity.y > 0)
                {
                    _endJumpEarly = true;
                }

                _timeSinceGrounded += Time.deltaTime;

                // apply gravity
                var fallSpeed = _endJumpEarly && _movement.y > 0 ? _constGravity * 4f : _constGravity;
                
                _movement.y -= fallSpeed * Time.deltaTime;
                _movement.y = Mathf.Max(_movement.y, -_maxFallSpeed);
            }
        }

        public void Move()
        {
            _controller.Move(_movement * Time.deltaTime);
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
            var controllerBottom = _controller.bounds.center - new Vector3(0f, _controller.bounds.extents.y, 0f);

            Debug.DrawRay(controllerBottom, Vector3.down * 0.1f);

            return Physics.Raycast(controllerBottom, Vector3.down, 0.1f);
        }

        private bool CheckAbove()
        {
            var controllerTop = _controller.bounds.center + new Vector3(0f, _controller.bounds.extents.y, 0f);

            Debug.DrawRay(controllerTop, Vector3.up * 0.1f);

            return Physics.Raycast(controllerTop, Vector3.down, 0.1f);
        }

        #endregion

        #region Input Handling

        private void OnMove(Vector2 move) => MoveInput = move;

        private void OnJumpStarted() => JumpInput = true;
        private void OnJumpCancelled() => JumpInput = false;

        #endregion
    }
}
