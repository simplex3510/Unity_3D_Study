using System;
using System.Collections;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM 
using UnityEngine.InputSystem;
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
    }

    private void Start()
    {
        _animator = GetComponent<Animator>();
        _playerInputData = GetComponent<PlayerInputData>();
        _charController = GetComponent<CharacterController>();
#if ENABLE_INPUT_SYSTEM
        _playerInput = GetComponent<PlayerInput>();
#else
	    Debug.LogError( "Starter Assets package is missing dependencies. Please use Tools/Starter Assets/Reinstall Dependencies to fix it");
#endif

        AssignAnimationParameters();
    }

    private void Update()
    {
        Move();
        Rotate();
        AnimateMotion();
    }

    private void LateUpdate()
    {
        //CameraRotation();
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

    [Obsolete("Camera Rotation is replaced other method")]
    private void CameraRotation()
    {
        // if there is an input and camera position is not fixed
        if (_playerInputData.look.sqrMagnitude >= _threshold && !LockCameraPosition)
        {
            //Don't multiply mouse input by Time.deltaTime;
            float deltaTimeMultiplier = IsCurrentDeviceMouse ? 1.0f : Time.deltaTime;

            _cinemachineTargetYaw += _playerInputData.look.x * deltaTimeMultiplier;
            _cinemachineTargetPitch += _playerInputData.look.y * deltaTimeMultiplier;
        }

        // clamp our rotations so our values are limited 360 degrees
        _cinemachineTargetYaw = ClampAngle(_cinemachineTargetYaw, float.MinValue, float.MaxValue);
        _cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, BottomClamp, TopClamp);

        // Cinemachine will follow this target
        CinemachineCameraTarget.transform.rotation = Quaternion.Euler(_cinemachineTargetPitch + CameraAngleOverride, _cinemachineTargetYaw, 0.0f);
    }

    [Obsolete]
    private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
    {
        if (lfAngle < -360f)
        {
            lfAngle += 360f;
        }

        if (lfAngle > 360f)
        {
            lfAngle -= 360f;
        }
        
        return Mathf.Clamp(lfAngle, lfMin, lfMax);
    }
}