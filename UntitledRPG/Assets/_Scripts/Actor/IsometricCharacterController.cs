using System.Collections.Generic;
using ReiBrary.Attributes;
using ReiBrary.Extensions;
using UnityEngine;
using UntitledRPG.Input;

[RequireComponent(typeof(CharacterController))]
public class IsometricCharacterController : MonoBehaviour
{       
    [SerializeField] private InputReader _input = default;

    [Header("Movement")]
    [SerializeField] private float _rotationSpeed = 0.1f;
    [SerializeField] private float _moveSpeed = 5.0f;
    [SerializeField] private float _coyoteTime = 0.1f;
    [SerializeField] private float _constGravity = -0.1f;
    [SerializeField] private int _numDashes = 1;
    [SerializeField] private float _dashDistance = 4f;
    [SerializeField] private float _dashSpeed = 3f;
    [SerializeField] private float _dashRechargeTime = 1.0f;

    [Header("Combat")] 
    [SerializeField] private BoxCollider _attackCollider;
    [SerializeField] private float _specialRadius = 4f;

    [Header("Animation")]
    [Range(0,1f)] [SerializeField]
    private float _startAnimTime = 0.3f;
    [Range(0, 1f)] [SerializeField]
    private float _stopAnimTime = 0.15f;

    [SerializeField] private Transform _swordTransform;
    [SerializeField] private ParticleSystem _dashParticleSystem;
    [SerializeField] private ParticleSystem _specialParticleSystem;

    // Components
    [HideInInspector] public Animator _anim;
    private CharacterController _controller;
    private Camera _camera;

    // Inputs
    [Header("Inputs")]
    [ReadOnly] public Vector2 MoveInput;
    [ReadOnly] public bool AttackInput = false;
    [ReadOnly] public bool SpecialInput = false;
    [ReadOnly] public bool DashInput = false;

    // 
    [ReadOnly] public bool AllowMovement = true;
    [ReadOnly] [SerializeField] private float _timeSinceGrounded;
    [ReadOnly] public int Dashes;
    private bool _isDashing = false;
    public bool IsDashing
    {
        get => _isDashing;
        set
        {
            _isDashing = value;
            _anim.SetBool("isDashing", value);
        }
    }
    // [ReadOnly] public float TimeSinceDash = 0f;
    private LinkedList<float> _timesSinceDashes;
    [ReadOnly] public float CurrDashDistance = 0f;
    [ReadOnly] public Vector3 CurrDashDir = Vector3.zero;

    private bool _dead = false;

    void Awake()
    {
        _anim = GetComponent<Animator>();
        _controller = GetComponent<CharacterController>();

        Dashes = _numDashes;
        _timesSinceDashes = new LinkedList<float>();
    }

    void Start()
    {
        _camera = Camera.main;
    }

    void OnEnable()
    {
        _input.MoveEvent += OnMove;
        // _input.AttackEvent += OnAttackStarted;
        // _input.AttackCancelledEvent += OnAttackCancelled;
        // _input.SpecialEvent += OnSpecialStarted;
        // _input.SpecialCancelledEvent += OnSpecialCancelled;
        // _input.DashEvent += OnDashStarted;
        // _input.DashCancelledEvent += OnDashCancelled;
    }

    void OnDisable()
    {
        _input.MoveEvent -= OnMove;
        // _input.AttackEvent -= OnAttackStarted;
        // _input.AttackCancelledEvent -= OnAttackCancelled;
        // _input.SpecialEvent -= OnSpecialStarted;
        // _input.SpecialCancelledEvent -= OnSpecialCancelled;
        // _input.DashEvent -= OnDashStarted;
        // _input.DashCancelledEvent -= OnDashCancelled;
    }

    void Update()
    {
        
    }

    public void Respawn()
    {
        _controller.enabled = false;
        transform.position = Vector3.zero;
        _controller.enabled = true;

        _dead = false;
        _timeSinceGrounded = 0f;
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

    /// <summary>
    /// Projects the adjusted input movement onto the XZ plane. 
    /// </summary>
    /// <returns></returns>
    public Vector3 CalculateAdjustedMovement()
    {
        var inputDirection = CalculateInputDirection();

        var adjustedMovement = MoveInput.sqrMagnitude == 0f ? 
            transform.forward * (inputDirection.magnitude + .01f) :
            inputDirection;

        return adjustedMovement;
    }

    public void PlayerMovement()
    {   
        var adjustedMovement = CalculateAdjustedMovement();

        var inputMagnitude = MoveInput.sqrMagnitude;       
        if (inputMagnitude == 0f)            
            _anim.SetFloat ("Blend", 0f, _stopAnimTime, Time.deltaTime);           
        else           
            _anim.SetFloat ("Blend", _controller.velocity.magnitude, _startAnimTime, Time.deltaTime);

        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(adjustedMovement), _rotationSpeed);

        // apply gravity
        adjustedMovement.y = _constGravity;
        if (_controller.isGrounded == false)
        {
            // TODO: add coyote time
            _timeSinceGrounded += Time.deltaTime;
            if (_timeSinceGrounded >= _coyoteTime)
            {
                // die
                Respawn();
            }
        }
        else
        {
            _timeSinceGrounded = 0f;
        }

        _controller.Move(adjustedMovement * Time.deltaTime * _moveSpeed);
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

    public bool ShouldDash => Dashes > 0 && IsDashing == false && DashInput;

    public void StartDash()
    {
        var inputMagnitude = MoveInput.sqrMagnitude;
        var adjustedMovement = CalculateAdjustedMovement();

        IsDashing = true;
        Dashes--;
        _timesSinceDashes.AddLast(0.0f);
        CurrDashDir = inputMagnitude == 0f ? transform.forward : adjustedMovement.normalized;
        CurrDashDistance= 0.0f;

        transform.rotation = Quaternion.LookRotation (CurrDashDir);
    }

    public void DashMovement()
    {
        if (CurrDashDistance >= _dashDistance)
        { 
            IsDashing = false;
            return;
        }

        var dist = _dashSpeed * Time.deltaTime;
        CurrDashDistance += dist;
        
        _controller.Move(CurrDashDir * dist);
    }

    public void AddForce(Vector3 force)
    {
        _controller.Move(force * Time.deltaTime);
    }

    private Collider[] _colliders = new Collider[10];

    public void CheckAttackCollision()
    {
        var hits= Physics.OverlapBoxNonAlloc(_attackCollider.transform.position, _attackCollider.GetHalfExtents(), _colliders, _attackCollider.transform.rotation, LayerMask.GetMask("Enemy"));

        for (int i = 0; i < hits; i++)
        {
            // _colliders[i].GetComponent<IDamagable>()?.Damage(1f);
        }
    }

    public void CheckSpecialCollision()
    { 
        var hits = Physics.OverlapSphereNonAlloc(_attackCollider.transform.position, _specialRadius, _colliders, LayerMask.GetMask("Enemy"));

        for (int i = 0; i < hits; i++)
        {
            // _colliders[i].GetComponent<IDamagable>()?.Damage(1f);
        }
    }

    public void ShowSword() => _swordTransform.Enable();
    public void HideSword() => _swordTransform.Enable();

    public void StartDashParticleSystem() => _dashParticleSystem.Play();
    public void StopDashParticleSystem() =>_dashParticleSystem.Stop();

    public void PlaySpecialParticleSystem() => _specialParticleSystem.Play();

    #region Input Handling

    private void OnMove(Vector2 move) => MoveInput = move;

    private void OnAttackStarted() => AttackInput = true;
    private void OnAttackCancelled() => AttackInput = false;

    private void OnSpecialStarted() => SpecialInput = true;
    private void OnSpecialCancelled() => SpecialInput = false;

    private void OnDashStarted() => DashInput = true;
    private void OnDashCancelled() => DashInput = false;

    #endregion

}
