using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.VersionControl;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM 
using UnityEngine.InputSystem;
using UnityEngine.SocialPlatforms.Impl;
using static Cinemachine.DocumentationSortingAttribute;
#endif

[RequireComponent(typeof(CharacterController))]
public class PlayableCharacterController : MonoBehaviour
{
    [Header("For Debug")]
    public Vector3 moveDirection;
    public Vector3 lookDirection;
    public Vector3 targDirection;
    public float targetAngle;

    // Player
    [Header("Player")]
    [Tooltip("Move speed of the character in m/s")]
    public float MoveSpeed = 2.0f;

    [Tooltip("Sprint speed of the character in m/s")]
    public float SprintSpeed = 5.335f;

    [Tooltip("Diagonal angle of moving")]
    public float diagonalAngle = 0.71f;

    [Tooltip("How fast the character turns to face movement direction")]
    [Range(0.0f, 0.3f)]
    public float RotationSmoothTime = 0.12f;

    [Tooltip("Acceleration and deceleration")]
    public float SpeedChangeRate = 10.0f;

    public AudioClip LandingAudioClip;
    public AudioClip[] FootstepAudioClips;
    [Range(0, 1)] public float FootstepAudioVolume = 0.5f;

    [Space(10)]
    [Tooltip("The height the player can jump")]
    public float JumpHeight = 1.2f;

    [Tooltip("The character uses its own gravity value. The engine default is -9.81f")]
    public float Gravity = -15.0f;

    [Space(10)]
    [Tooltip("Time required to pass before being able to jump again. Set to 0f to instantly jump again")]
    public float JumpTimeout = 0.50f;

    [Tooltip("Time required to pass before entering the fall state. Useful for walking down stairs")]
    public float FallTimeout = 0.15f;

    [Tooltip("Time required to animate idle uncombat motion")]
    public float IdleTimeout = 3.0f;

    // Player Grounded
    [Header("Player Grounded")]
    [Tooltip("If the character is grounded or not. Not part of the CharacterController built in grounded check")]
    public bool Grounded = true;

    [Tooltip("Useful for rough ground")]
    public float GroundedOffset = -0.14f;

    [Tooltip("The radius of the grounded check. Should match the radius of the CharacterController")]
    public float GroundedRadius = 0.28f;

    [Tooltip("What layers the character uses as ground")]
    public LayerMask GroundLayers;

    // Cinemachine
    [Header("Cinemachine")]
    [Tooltip("The follow target set in the Cinemachine Virtual Camera that the camera will follow")]
    public GameObject CinemachineCameraTarget;

    [Tooltip("How far in degrees can you move the camera up")]
    public float TopClamp = 70.0f;

    [Tooltip("How far in degrees can you move the camera down")]
    public float BottomClamp = -30.0f;

    [Tooltip("Additional degress to override the camera. Useful for fine tuning camera position when locked")]
    public float CameraAngleOverride = 0.0f;

    [Tooltip("For locking the camera position on all axis")]
    public bool LockCameraPosition = false;

    // cinemachine
    private float _cinemachineTargetYaw;
    private float _cinemachineTargetPitch;

    // player
    private float _speed;
    private float _targetRotation = 0.0f;
    private float _rotationVelocity;
    private float _verticalVelocity;
    private float _terminalVelocity = 53.0f;
    private const float _leftAttackTime = 0.9f;
    private const float _leftAtk = 50f;
    private const float _rightAtk = 100f;
    private const float _RightattackTime = 1.25f;
    private const float _DashTime = 5f;
    public static bool isAttack;
    public static bool isDash;
    private bool canDash;
    [SerializeField]
    private float _Life = 200f;
    public float Life
    {
        get { return _Life; }
    }

    // direction

    // timeout deltatime
    private float _jumpTimeoutDelta;
    private float _fallTimeoutDelta;
    private float _idleTimeoutDelta;

    // animation blend value
    private float _animBlendIdle;
    private float _animBlendFront;
    private float _animBlendRight;

    // animation IDs
    private int _animParam_Idle;
    private int _animParam_Front;
    private int _animParam_Right;
    private int _animParam_IsMoving;

#if ENABLE_INPUT_SYSTEM
    private PlayerInput _playerInput;
#endif
    private Animator _animator;
    private CharacterController _charController;
    private PlayerInputData _playerInputData;
    private Camera _mainCamera;
    private Sword _sword;
    [SerializeField]
    private List<SkillData> _skills = new List<SkillData>();
    
    [SerializeField]
    private List<SkillData> _unlockSkills = new List<SkillData>();
    public List<SkillData> UnlockSkills
    {
        get { return _unlockSkills; }
        set { _unlockSkills = value; }
    }

    Coroutine CommandCoroutine = null;

    [SerializeField]
    private string _skillCammand;
    public string SkillCammand
    {
        get { return _skillCammand; }
        set { _skillCammand = value; }
    }

    private const float _threshold = 0.1f;

    private bool _hasAnimator;

    private bool IsCurrentDeviceMouse
    {
        get
        {
#if ENABLE_INPUT_SYSTEM
            return _playerInput.currentControlScheme == "PC"; // KeyboardMouse
#else
            return false;
#endif
        }
    }

    private void Awake()
    {
        if (_mainCamera == null)
        {
            _mainCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
        }
        if (_sword == null)
        {
            _sword = GetComponentInChildren<Sword>();
        }

        foreach(SkillData skill in _skills)
            skill.skillLevel = 0;
    }

    private void Start()
    {
        _animator = GetComponent<Animator>();
        _playerInputData = GetComponent<PlayerInputData>();
        _charController = GetComponent<CharacterController>();
        _skillCammand = "";
        canDash = true;
        foreach(SkillData skillData in _skills)
        {
            if(skillData.isUnlock)
            {
                _unlockSkills.Add(skillData);
            }
        }
#if ENABLE_INPUT_SYSTEM
        _playerInput = GetComponent<PlayerInput>();
#else
	    Debug.LogError( "Starter Assets package is missing dependencies. Please use Tools/Starter Assets/Reinstall Dependencies to fix it");
#endif

        AssignAnimationParameters();
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            SkillManager.instance.LevelUp();
        }

        Dash();
        Move();
        LeftAttack();
        RightAttack();
        Rotate();
        AnimateMotion();
    }
    private void UseSkill()
    {
        for (int i = _unlockSkills.Count - 1; i >= 0; i--)

        {
            if (_skillCammand.EndsWith(_unlockSkills[i].skillCommand))
            {
                if (_unlockSkills[i].skillLevel > 0)
                {
                    if (CommandCoroutine != null)
                        StopCoroutine(CommandCoroutine);
                    isAttack = true;
                    _sword.Use(_skills[i].skillAnimationTime, _skills[i].baseDamage + _skills[i].skillLevel * _skills[i].ad);
                    StartCoroutine(AttackEnd(_skills[i].skillAnimationTime, _skills[i].skillAnimationTrigger));
                    _skillCammand = "";
                    break;
                }
            }
        }
    }

    private void LeftAttack()
    {
        if (_playerInputData.leftAttack && !isAttack && !isDash)
        {
            if (CommandCoroutine != null)
                StopCoroutine(CommandCoroutine);
            _skillCammand += 'L';
            CommandCoroutine = StartCoroutine(ClearCommand());
            UseSkill();
            if (!isAttack)
            {
                isAttack = true;
                _sword.Use(_leftAttackTime, _leftAtk);
                StartCoroutine(AttackEnd(_leftAttackTime, "IsLeftAttack"));
            }
            _playerInputData.leftAttack = false;
        }
    }

    private void RightAttack()
    {
        if (_playerInputData.rightAttack && !isAttack && !isDash)
        {
            if (CommandCoroutine != null)
                StopCoroutine(CommandCoroutine);
            _skillCammand += 'R';
            CommandCoroutine = StartCoroutine(ClearCommand());
            UseSkill();
            if (!isAttack)
            {
                isAttack = true;
                _sword.Use(_RightattackTime, _rightAtk);
                StartCoroutine(AttackEnd(_RightattackTime, "IsRightAttack"));
            }
            _playerInputData.rightAttack = false;
        }
    }

    IEnumerator AttackEnd(float attackTime, string animationBool)
    {
        _animator.SetBool("IsAttack", true);
        _animator.SetBool(animationBool, true);
        yield return new WaitForSeconds(attackTime);
        isAttack = false;
        _animator.SetBool(animationBool, false);
        _animator.SetBool("IsAttack", false);
    }
    IEnumerator ClearCommand()
    {
        yield return new WaitForSeconds(7);
        _skillCammand = "";
    }

    private void Dash()
    {
        if (_playerInputData.dash && !canDash)
            _playerInputData.dash = false;
        if (_playerInputData.dash && !isAttack && !isDash && canDash)
        {
            if (CommandCoroutine != null)
                StopCoroutine(CommandCoroutine);
            _skillCammand += 'S';
            CommandCoroutine = StartCoroutine(ClearCommand());
            isDash = true;
            canDash = false;
            StartCoroutine(Dashing(lookDirection));
            StartCoroutine(DashEnd());
            _playerInputData.dash = false;
        }
    }

    IEnumerator Dashing(Vector3 dashDirection)
    {
        _animator.SetBool("IsDash", true);
        _charController.Move(dashDirection * 0.5f);
        yield return new WaitForSeconds(0.005f);
        _charController.Move(dashDirection * 0.2f);
        yield return new WaitForSeconds(0.005f);
        _animator.SetBool("IsDash", false);
        isDash = false;
    }

    IEnumerator DashEnd()
    {
        yield return new WaitForSeconds(_DashTime);
        canDash = true;
    }

    private void Move()
    {
        float targetSpeed = MoveSpeed;
        if (_playerInputData.move == Vector2.zero)
        {
            targetSpeed = 0.0f;
        }

        float currentHorizontalSpeed = new Vector3(_charController.velocity.x, 0f, _charController.velocity.z).magnitude;

        float speedOffest = 0.1f;
        float inputMagnitude = 1.0f;//_netInputData.analogMovement ? _netInputData.move.magnitude : 1.0f;

        if (currentHorizontalSpeed < targetSpeed - speedOffest || targetSpeed + speedOffest < currentHorizontalSpeed)
        {
            _speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed * inputMagnitude, Time.deltaTime * SpeedChangeRate);
            _speed = Mathf.Round(_speed * 1000f) / 1000f;
        }
        else
        {
            _speed = targetSpeed;
        }

        // Move
        moveDirection = new Vector3(_playerInputData.move.x, 0, _playerInputData.move.y);
        _charController.Move(moveDirection.normalized * (_speed * Time.deltaTime) + new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime);
    }

    private void Rotate()
    {
        Ray rayCamera = _mainCamera.ScreenPointToRay(new Vector3(_playerInputData.look.x, _playerInputData.look.y, _mainCamera.nearClipPlane));
        if (Physics.Raycast(rayCamera, out RaycastHit raycastHit, float.MaxValue, GroundLayers))
        {
            Vector3 mouseDirection = new Vector3(raycastHit.point.x, transform.position.y, raycastHit.point.z);
            lookDirection = (mouseDirection - transform.position).normalized;
            transform.rotation = Quaternion.LookRotation(lookDirection);
        }
    }

    private void AnimateMotion()
    {
        bool isInit = false;
        float targetAnimThreshold;
        if (moveDirection == Vector3.zero)
        {
            targetAnimThreshold = 3f;

            if (_idleTimeoutDelta < 0.1f)
            {
                _animator.SetBool(_animParam_IsMoving, false);
                _idleTimeoutDelta += Time.deltaTime;

            }
            else if (_idleTimeoutDelta < IdleTimeout)
            {
                _idleTimeoutDelta += Time.deltaTime;
            }
            else
            {
                _animBlendIdle = Mathf.Lerp(_animBlendIdle, targetAnimThreshold, Time.deltaTime);
                _animator.SetFloat(_animParam_Idle, _animBlendIdle);
            }
        }
        else
        {
            isInit = true;
        }

        if (isInit == true)
        {
            _idleTimeoutDelta = 0.0f;
            _animBlendIdle = 0.0f;
            _animator.SetFloat(_animParam_Idle, _animBlendIdle);
            _animator.SetBool(_animParam_IsMoving, true);
            isInit = false;
        }

        // get targetAngle
        targetAngle = Vector3.SignedAngle(transform.forward, moveDirection, Vector3.up);
        if (targetAngle < 0f)
        {
            targetAngle += 360f;
        }
        Debug.Log(targetAngle);

        // if has move
        if (moveDirection == Vector3.zero)
        {
            _animBlendFront = Mathf.Lerp(_animBlendFront, 0f, Time.deltaTime * SpeedChangeRate);
            _animBlendRight = Mathf.Lerp(_animBlendRight, 0f, Time.deltaTime * SpeedChangeRate);
            _animator.SetFloat(_animParam_Front, _animBlendFront);
            _animator.SetFloat(_animParam_Right, _animBlendRight);
        }
        else
        {
            // front
            if (337.5f < targetAngle || targetAngle <= 22.5f)
            {
                _animBlendFront = Mathf.Lerp(_animBlendFront, 1f, Time.deltaTime * SpeedChangeRate);
                _animBlendRight = Mathf.Lerp(_animBlendRight, 0f, Time.deltaTime * SpeedChangeRate);
                _animator.SetFloat(_animParam_Front, _animBlendFront);
                _animator.SetFloat(_animParam_Right, _animBlendRight);

                if (-0.1f < _animBlendFront && _animBlendFront < 0.1f) _animBlendFront = 0f;
                if (-0.1f < _animBlendRight && _animBlendRight < 0.1f) _animBlendRight = 0f;
            }
            // frontL45
            else if (292.5f < targetAngle && targetAngle <= 337.5f)
            {
                _animBlendFront = Mathf.Lerp(_animBlendFront, 1f, Time.deltaTime * SpeedChangeRate);
                _animBlendRight = Mathf.Lerp(_animBlendRight, -1f, Time.deltaTime * SpeedChangeRate);
                _animator.SetFloat(_animParam_Front, _animBlendFront);
                _animator.SetFloat(_animParam_Right, _animBlendRight);

                if (-0.1f < _animBlendFront && _animBlendFront < 0.1f) _animBlendFront = 0f;
                if (-0.1f < _animBlendRight && _animBlendRight < 0.1f) _animBlendRight = 0f;
            }
            // Left
            else if (247.5f < targetAngle && targetAngle <= 292.5f)
            {
                _animBlendFront = Mathf.Lerp(_animBlendFront, 0f, Time.deltaTime * SpeedChangeRate);
                _animBlendRight = Mathf.Lerp(_animBlendRight, -1f, Time.deltaTime * SpeedChangeRate);
                _animator.SetFloat(_animParam_Front, _animBlendFront);
                _animator.SetFloat(_animParam_Right, _animBlendRight);

                if (-0.1f < _animBlendFront && _animBlendFront < 0.1f) _animBlendFront = 0f;
                if (-0.1f < _animBlendRight && _animBlendRight < 0.1f) _animBlendRight = 0f;
            }
            // backL45
            else if (202.5f < targetAngle && targetAngle <= 247.5f)
            {
                _animBlendFront = Mathf.Lerp(_animBlendFront, -1f, Time.deltaTime * SpeedChangeRate);
                _animBlendRight = Mathf.Lerp(_animBlendRight, -1f, Time.deltaTime * SpeedChangeRate);
                _animator.SetFloat(_animParam_Front, _animBlendFront);
                _animator.SetFloat(_animParam_Right, _animBlendRight);

                if (-0.1f < _animBlendFront && _animBlendFront < 0.1f) _animBlendFront = 0f;
                if (-0.1f < _animBlendRight && _animBlendRight < 0.1f) _animBlendRight = 0f;
            }
            // back
            else if (157.5f < targetAngle && targetAngle <= 202.5f)
            {
                _animBlendFront = Mathf.Lerp(_animBlendFront, -1f, Time.deltaTime * SpeedChangeRate);
                _animBlendRight = Mathf.Lerp(_animBlendRight, 0f, Time.deltaTime * SpeedChangeRate);
                _animator.SetFloat(_animParam_Front, _animBlendFront);
                _animator.SetFloat(_animParam_Right, _animBlendRight);

                if (-0.1f < _animBlendFront && _animBlendFront < 0.1f) _animBlendFront = 0f;
                if (-0.1f < _animBlendRight && _animBlendRight < 0.1f) _animBlendRight = 0f;
            }
            // backR45
            else if (112.5f < targetAngle && targetAngle <= 157.5f)
            {
                _animBlendFront = Mathf.Lerp(_animBlendFront, -1f, Time.deltaTime * SpeedChangeRate);
                _animBlendRight = Mathf.Lerp(_animBlendRight, 1f, Time.deltaTime * SpeedChangeRate);
                _animator.SetFloat(_animParam_Front, _animBlendFront);
                _animator.SetFloat(_animParam_Right, _animBlendRight);

                if (-0.1f < _animBlendFront && _animBlendFront < 0.1f) _animBlendFront = 0f;
                if (-0.1f < _animBlendRight && _animBlendRight < 0.1f) _animBlendRight = 0f;
            }
            // Right
            else if (67.5f < targetAngle && targetAngle <= 112.5f)
            {
                _animBlendFront = Mathf.Lerp(_animBlendFront, 0f, Time.deltaTime * SpeedChangeRate);
                _animBlendRight = Mathf.Lerp(_animBlendRight, 1f, Time.deltaTime * SpeedChangeRate);
                _animator.SetFloat(_animParam_Front, _animBlendFront);
                _animator.SetFloat(_animParam_Right, _animBlendRight);

                if (-0.1f < _animBlendFront && _animBlendFront < 0.1f) _animBlendFront = 0f;
                if (-0.1f < _animBlendRight && _animBlendRight < 0.1f) _animBlendRight = 0f;
            }
            // frontR45
            else if (22.5f < targetAngle && targetAngle <= 67.5f)
            {
                _animBlendFront = Mathf.Lerp(_animBlendFront, 1f, Time.deltaTime * SpeedChangeRate);
                _animBlendRight = Mathf.Lerp(_animBlendRight, 1f, Time.deltaTime * SpeedChangeRate);
                _animator.SetFloat(_animParam_Front, _animBlendFront);
                _animator.SetFloat(_animParam_Right, _animBlendRight);

                if (-0.1f < _animBlendFront && _animBlendFront < 0.1f) _animBlendFront = 0f;
                if (-0.1f < _animBlendRight && _animBlendRight < 0.1f) _animBlendRight = 0f;
            }
        }
    }

    private void AssignAnimationParameters()
    {
        _animParam_Idle = Animator.StringToHash("Idle");
        _animParam_Front = Animator.StringToHash("Front");
        _animParam_Right = Animator.StringToHash("Right");
        _animParam_IsMoving = Animator.StringToHash("IsMoving");
    }

    public void Damage(float damage)
    {
        _Life -= damage;
        if(_Life <= 0)
            _Life = 0;
    }
}