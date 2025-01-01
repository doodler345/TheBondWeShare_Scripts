using DG.Tweening;
using Obi;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] PlayerMovement _otherPlayer;
    [SerializeField] RopeController _ropeController;
    public STATE State;
    public DEVICE Device;
    StageController _stageController;
    GroundDetection _groundDetection;
    WallDetection _wallDetection;
    MoveableDetection _moveableDetection;
    InteractableDetection _interactableDetection;
    HookDetection _hookDetection;
    AnimationManager _anim;
    PlayerSound _audio;

    [HideInInspector] public ObiRigidbody obiRB;
    Rigidbody _rb;
    Coroutine _delayedTearingEnable, _obiKinematicsEnable;
    Renderer _renderer;
    MoveableObject _moveableObject;
    Color _initColor;
    Tween _turningTween;

    public int playerID = 0;
    [HideInInspector] public float moveForceMultiply = 1, jumpForceMultiply = 1;
    [HideInInspector] public bool movedByPlatform;
    [SerializeField] Transform _playerModel, _collider;
    [SerializeField] float _walkSpeed = 5f, _pushPullSpeed = 3f, _climbSpeed = 3f;
    [SerializeField] float _turnDuration = 0.2f;
    [SerializeField] int _jumpForce = 15;
    [SerializeField] float _hangingJumpBoost = 2;
    [SerializeField] float _swingBoost = 4;
    [SerializeField] float _swingMaxVelocity = 10;
    [SerializeField] bool _allowDoubleJump;
    [SerializeField] bool _allowFallDamage;
    [SerializeField] float _maxFallHeight = 8;
    Vector3 _walkVelocity, _climbVelocity;
    float _tmpVelocityX;
    float _tmpDirection = 1;
    float _tmpPosX;
    float _startFallPosY;
    float _ropeImpactCooldownTimer;
    public bool isSaveFromFall = true;
    public bool isFirstSpawn;
    bool _ropeImpactCooldown;
    bool _doubleJmpPossible;
    bool _ladderClimb, _ladderClimbUP, _ladderClimbCanceled; //murks
    bool _ropeSwingPossible;
    bool _checkIfWalk;
    bool _deviceIsSet;
    bool _isPaused;


    #region init
    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        obiRB = GetComponent<ObiRigidbody>();
        _renderer = GetComponentInChildren<Renderer>();
        _initColor = _renderer.material.color;
        _groundDetection = GetComponentInChildren<GroundDetection>();
        _wallDetection = GetComponentInChildren<WallDetection>();
        _moveableDetection = GetComponentInChildren<MoveableDetection>();
        _interactableDetection = GetComponentInChildren<InteractableDetection>();
        _hookDetection = GetComponentInChildren<HookDetection>();
        _anim = GetComponent<AnimationManager>();
        _audio = GetComponent<PlayerSound>();

        moveForceMultiply = 1;
        jumpForceMultiply = 1;

        if (State == STATE.FALL)
        {
            _startFallPosY = transform.position.y;
            isSaveFromFall = false;
        }
    }

    private void Start()
    {
        _stageController = StageController.instance;

        _moveableDetection.Init(Device, "moveable");
        _interactableDetection.Init(Device, "interactable");
        _hookDetection.Init(Device, "interactable");

        StageController.instance.gamePaused += PauseGame;
    }
    public void SetDevice(PlayerInputHandler.DEVICE device)
    {
        _deviceIsSet = true;
        switch (device)
        {
            case PlayerInputHandler.DEVICE.KEYBOARD_WASD:
                Device = DEVICE.KEYBOARD_WASD;
                break;
            case PlayerInputHandler.DEVICE.KEYBOARD_ARROW:
                Device = DEVICE.KEYBOARD_ARROW;
                break;
            case PlayerInputHandler.DEVICE.PS4:
                Device = DEVICE.PS4;
                break;
            case PlayerInputHandler.DEVICE.XBOX:
                Device = DEVICE.XBOX;
                break;
            case PlayerInputHandler.DEVICE.UNKNOWN:
                Device = DEVICE.UNKNOWN;
                break;
        }
    }
    #endregion

    private void Update()
    {
        CheckMoveByPlatform();

        if (_ropeImpactCooldown)
        {
            _ropeImpactCooldownTimer += Time.deltaTime;
            if (_ropeImpactCooldownTimer > 2)
            {
                _ropeImpactCooldown = false;
                _ropeImpactCooldownTimer = 0;
            }
        }

        //Checking for State Interruptions
        switch (State)
        {
            case STATE.IDLE:
            case STATE.WALK:

                if (_rb.velocity.y < 0 && !_groundDetection.grounded)
                {
                    ChangeState(STATE.FALL);
                }
                else if (!_stageController.isUnbound && _rb.velocity.y > 0 && !_groundDetection.grounded
                    && _otherPlayer.State != STATE.FALL)
                {
                    ChangeState(STATE.ROPE_HANG);
                }
                break;

            case STATE.PUSH_OBJECT:

                if (_rb.velocity.y < 0 && !_groundDetection.grounded)
                {
                    PushPullObject(false);
                }
                else if (!_moveableDetection.detected)
                {
                    StartCoroutine(nameof(CheckIfMoveableFalling));
                }
                break;

            case STATE.PULL_OBJECT:

                if (_rb.velocity.y < 0 && !_groundDetection.grounded)
                {
                    PushPullObject(false);
                }
                break;

            case STATE.JUMP:

                if (_rb.velocity.y < 0)
                {
                    ChangeState(STATE.FALL);
                }
                break;

            case STATE.FALL:

                if (_groundDetection.grounded)
                {
                    if (!_deviceIsSet) //in the beginning if no device is set yet
                    {
                        ChangeState(STATE.IDLE);
                        break;
                    }

                    if (!_checkIfWalk) _checkIfWalk = true;
                }
                else if (!_stageController.isUnbound && _rb.velocity.y > 0 && !_wallDetection.CheckForFrontWall()
                    && _otherPlayer.State != STATE.FALL)
                {
                    ChangeState(STATE.ROPE_HANG);
                }
                break;

            case STATE.LEGDE_HANG:
                if (_groundDetection.grounded) //hanging at a moving platform
                {
                    ChangeState(STATE.IDLE);
                }
                else if (_wallDetection.fragilePlatform && _wallDetection.GetDetectedPlatform().IsFragileBroken())
                {
                    ChangeState(STATE.FALL);
                }
                break;

            case STATE.LADDERCLIMB:
                LadderClimb(_climbVelocity);
                if (!_interactableDetection.ladderDetected)
                {
                    ChangeState(STATE.FALL);
                }
                break;

            case STATE.ROPE_HANG:

                if (_otherPlayer.State == STATE.FALL)
                {
                    ChangeState(STATE.FALL);
                    return;
                }

                if (transform.position.y >= _otherPlayer.transform.position.y)
                {
                    _rb.velocity = Vector3.zero;
                    ChangeState(STATE.FALL);
                    return;
                }

                if (_rb.velocity.x * _tmpVelocityX < 0)
                {
                    float xThreshold = 1.6f;
                    float playerDistanceX = transform.position.x - _otherPlayer.transform.position.x;
                    if (Mathf.Abs(playerDistanceX) > xThreshold)
                    {
                        _ropeSwingPossible = true;
                    }
                }
                _tmpVelocityX = _rb.velocity.x;

                if (_wallDetection.CheckForFrontWall())
                {
                    ChangeState(STATE.FALL);
                }
                if (_wallDetection.CheckForBackWall())
                {
                    Debug.Log("backwall detected!");
                    ChangeState(STATE.FALL);
                }

                else if (_groundDetection.grounded)
                {
                    ChangeState(STATE.IDLE);
                }
                break;

            default:
                break;
        }
    }
    public void WorldSwitch()
    {
        switch (State)
        {
            case STATE.LEGDE_HANG:
                if (_wallDetection.GetLedgePlatformWorld() == Platform.WORLD.BOTH) return;
                break;
            case STATE.PULL_OBJECT:
            case STATE.PUSH_OBJECT:
                if (_moveableObject.transform.parent.gameObject.activeInHierarchy)
                {
                    break;
                }
                else
                {
                    PushPullObject(false);
                }
                break;
        }

        if (_groundDetection.grounded && _groundDetection.GetPlatformWorld() != Platform.WORLD.BOTH)
            _groundDetection.grounded = false;

        ChangeState(STATE.FALL);
    }


    #region movement
    public void Move(float xVelocity)
    {
        if (_isPaused) return;

        switch (State)
        {
            case STATE.LEGDE_HANG:
            case STATE.HOOK_HANG:
                return;
        }

        if (xVelocity == 0)
        {
            if (_checkIfWalk)
            {
                ChangeState(STATE.IDLE);
                _checkIfWalk = false;
                return;
            }

            switch (State)
            {
                case STATE.PUSH_OBJECT:
                case STATE.PULL_OBJECT:
                    _moveableObject.PushPull(0);
                    _anim.SetMoveSpeed(0);
                    return;
                case STATE.IDLE:
                case STATE.JUMP:
                case STATE.FALL:
                case STATE.LADDERCLIMB:
                    return;

                default:
                    if (_groundDetection.grounded)
                    {
                        ChangeState(STATE.IDLE);
                        return;
                    }
                    break;
            }

            _walkVelocity = Vector3.zero;
        }
        else
        {

            float speedX = xVelocity;

            CheckForTurn(xVelocity);

            if (_wallDetection.CheckForFrontWall())
            {
                switch (State)
                {
                    case STATE.JUMP:
                        return;
                    case STATE.FALL:
                        if (_checkIfWalk)
                        {
                            _checkIfWalk = false;
                            ChangeState(STATE.WALK);
                        }
                        return;
                    case STATE.PUSH_OBJECT:
                    case STATE.PULL_OBJECT:
                        break;

                    case STATE.ROPE_HANG:
                        ChangeState(STATE.FALL);
                        break;

                    default:
                        ChangeState(STATE.IDLE);
                        return;
                }
            }

            switch (State)
            {
                case STATE.PUSH_OBJECT:
                case STATE.PULL_OBJECT:

                    if (_checkIfWalk)
                    {
                        _checkIfWalk = false;
                        ChangeState(STATE.FALL);
                        break;
                    }

                    speedX *= _pushPullSpeed;

                    float deltaX = transform.position.x - _tmpPosX;
                    //        Debug.Log(deltaX + " posX: " + transform.position.x + " / tmpPosX: " + _tmpPosX);

                    _moveableObject.PushPull(deltaX);
                    break;

                case STATE.LADDERCLIMB:
                    speedX *= _climbSpeed;
                    break;

                case STATE.WALK:
                case STATE.JUMP:
                    speedX *= _walkSpeed;
                    break;

                case STATE.FALL:
                    if (_checkIfWalk)
                    {
                        _checkIfWalk = false;
                        ChangeState(STATE.WALK);
                    }
                    speedX *= _walkSpeed;
                    break;

                case STATE.ROPE_HANG:
                    _walkVelocity = _rb.velocity.normalized * _swingBoost;

                    if (speedX * _walkVelocity.x < 0) return;

                    if (_ropeSwingPossible)
                    {
                        _audio.Play("ropeSwing");
                        _anim.SetTrigger("ropeswing");

                        _ropeSwingPossible = false;
                    }
                
                    if (_rb.velocity.sqrMagnitude > _swingMaxVelocity) return;

                    _rb.AddForce(_walkVelocity * moveForceMultiply, ForceMode.Acceleration);
                    return;

                default:
                    ChangeState(STATE.WALK);
                    speedX *= _walkSpeed;
                    break;
            }

            _tmpPosX = transform.position.x;

            _walkVelocity = new Vector3(speedX * moveForceMultiply, 0, 0);
            _anim.SetMoveSpeed(Mathf.Abs(xVelocity));
            _rb.MovePosition(transform.position += _walkVelocity * Time.deltaTime);
        }
    }
    private void CheckForTurn(float directionX)
    {
        if (_ladderClimbCanceled)
        {
            if (_turningTween != null) _turningTween.Kill();
            float rotY = directionX > 0f ? 90 : -90;
            _playerModel.DOLocalRotate(new Vector3(0, rotY, 0), _turnDuration);

            _ladderClimbCanceled = false;
        }

        if (_tmpDirection * directionX < 0f)
        {
            switch (State)
            {
                case STATE.PULL_OBJECT:
                    ChangeState(STATE.PUSH_OBJECT);
                    break;
                case STATE.PUSH_OBJECT:
                    ChangeState(STATE.PULL_OBJECT);
                    break;
                case STATE.LADDERCLIMB:
                    return;

                default:
                    if (_turningTween != null) _turningTween.Kill();
                    float rotY = directionX > 0f ? 90 : -90;
                    _playerModel.DOLocalRotate(new Vector3(0, rotY, 0), _turnDuration);
                    _wallDetection.Turn();
                    _moveableDetection.Turn();
                    _anim.Turn();
                    break;

            }

            _tmpDirection = directionX;
        }
    }
    private void CheckMoveByPlatform()
    {
        if (_groundDetection.moveablePlatform || _wallDetection.moveablePlatform)
        {
            movedByPlatform = true;
        }
        else movedByPlatform = false;

        if (movedByPlatform)
        {
            //        Debug.Log("Player " + playerID + " moved by platform");
            Vector3 delta = _groundDetection.grounded ? _groundDetection.GetPlatformDeltaPos() : _wallDetection.GetPlatformDeltaPos();
            _rb.MovePosition(transform.position += delta);
        }
    }

    public void Up(bool keyDown)
    {
        if (_isPaused) return;

        if (keyDown)
        {
            if (ToggleLadderClimb(true)) return;
            else Jump();
        }

        else
        {
            ToggleLadderClimb(false);
        }
    }
    public void Jump()
    {
        if (!_groundDetection.grounded && !_doubleJmpPossible && State != STATE.LEGDE_HANG) return;

        switch (State)
        {
            case STATE.JUMP:
            case STATE.FALL:
            case STATE.PUSH_OBJECT:
            case STATE.PULL_OBJECT:
            case STATE.ROPE_HANG:
            case STATE.HOOK_HANG:
                return;
        }

        if (State == STATE.LEGDE_HANG)
        {
            ChangeState(STATE.JUMP);
            _groundDetection.IgnoreGroundForSeconds(1);
            _rb.AddForce(Vector3.up * _jumpForce * jumpForceMultiply * _hangingJumpBoost, ForceMode.Impulse);
            _doubleJmpPossible = false;
            return;
        }

        else
        {
            ChangeState(STATE.JUMP);
            _rb.velocity = Vector3.zero;
            _rb.AddForce(Vector3.up * _jumpForce * jumpForceMultiply, ForceMode.Impulse);
            if (_allowDoubleJump) _doubleJmpPossible = !_doubleJmpPossible;
        }

    }
    public bool ToggleLadderClimb(bool keyDown)
    {
        if (keyDown && _interactableDetection.ladderDetected)
        {
            switch (State)
            {
                case STATE.PUSH_OBJECT:
                case STATE.PULL_OBJECT:
                case STATE.ROPE_HANG:
                    return false;
            }

            if (State != STATE.LADDERCLIMB)
            {
                ChangeState(STATE.LADDERCLIMB);
                //_playerModel.LookAt(_playerModel.position + new Vector3(0, 0, 1));
                if (_turningTween != null) _turningTween.Kill();
                _turningTween = _playerModel.DOLookAt(_playerModel.position + new Vector3(0, 0, 1), _turnDuration);
            }

            if (_climbVelocity.normalized != Vector3.up)
            {
                _climbVelocity = Vector3.up * _climbSpeed;
            }
            return true;
        }
        else if (State == STATE.LADDERCLIMB)
        {
            _climbVelocity = Vector3.zero;
            return true;
        }
        else return false;
    }
    void LadderClimb(Vector3 yVelocity)
    {
        _rb.MovePosition(transform.position += yVelocity * Time.deltaTime);
        _anim.SetClimbSpeed(yVelocity.normalized.y);

        if (yVelocity.y < 0 && _groundDetection.grounded)
        {
            ChangeState(STATE.FALL);
        }
    }

    public void Down(bool keyDown)
    {
        if (_isPaused) return;

        if (keyDown)
        {
            switch (State)
            {
                case STATE.LEGDE_HANG:
                    ChangeState(STATE.FALL);
                    break;
                case STATE.HOOK_HANG:
                    ChangeState(STATE.FALL);
                    break;
                case STATE.LADDERCLIMB:
                    if (_climbVelocity.normalized != Vector3.down)
                    {
                        _climbVelocity = Vector3.down * _climbSpeed;
                    }

                    break;

                default:
                    //Anchor(keyDown);
                    break;
            }
        }

        else
        {
            if (State == STATE.LADDERCLIMB)
            {
                _climbVelocity = Vector3.zero;
            }

        }
    }

    public void Interact()
    {
        if (_isPaused) return;

        if (_interactableDetection.leverDetected)
        {
            _interactableDetection.GetLever().ToggleLever();
            //_anim.SetTrigger("leverInteract");
            _interactableDetection.ShowIcon(false);
        }
        else if (_interactableDetection.doorDetected)
        {
            _interactableDetection.GetDoor().EnterDoor();
            //_anim.SetTrigger("leverInteract");
            _interactableDetection.ShowIcon(false);
        }
        else if (_hookDetection.hookDetected && State != STATE.HOOK_HANG)
        {
            HookHang(true);
        }
    }

    public void LedgeHang(bool isHanging)
    {
        if (isHanging)
        {
            if (State == STATE.LADDERCLIMB) return;
            ChangeState(STATE.LEGDE_HANG);
        }
        else
        {
            StartCoroutine(_wallDetection.IgnoreLedgesForSeconds(0.5f));
            _wallDetection.Unhang();
        }

        _rb.isKinematic = isHanging;
    }
    private void HookHang(bool isHanging)
    {
        if (isHanging)
        {
            _hookDetection.ShowIcon(false);
            ChangeState(STATE.HOOK_HANG);
        }
        else
            ChangeState(STATE.FALL);

        _rb.isKinematic = isHanging;
    }
    public void PushPullObject(bool isGrabbing)
    {
        if (_isPaused) return;

        //        Debug.Log("PushPull: " + isGrabbing);

        if (!isGrabbing)
        {
            if (State == STATE.PUSH_OBJECT || State == STATE.PULL_OBJECT)
            {
                _checkIfWalk = true;
            }
            return;
        }

        if (!_moveableDetection.detected || !_groundDetection.grounded) return;

        switch (State)
        {
            case STATE.FALL:
            case STATE.LADDERCLIMB:
            case STATE.HOOK_HANG:
            case STATE.ROPE_HANG:
            case STATE.LEGDE_HANG:
            case STATE.JUMP:
                return;
        }

        _moveableObject = _moveableDetection.GetDetectedObject();

        if (_moveableObject.getsMoved) return;

        _moveableObject.StartMove();
        _moveableDetection.ShowIcon(false);

        ChangeState(STATE.PUSH_OBJECT);
    }
    private void AdjustMoveableMinDistance()
    {
        float minDistance = 0.9f * _moveableObject.transform.localScale.x;
        Vector2 dir = _moveableObject.transform.position - transform.position;
        if (dir.sqrMagnitude <= Mathf.Pow(minDistance, 2))
        {
            _moveableObject.PushPull(Mathf.Sign(dir.x) * 0.1f * _moveableObject.transform.localScale.x);
        }
    }


    private IEnumerator CheckIfMoveableFalling()
    {
        yield return new WaitForSeconds(0.1f);
        if (!_moveableDetection.detected) PushPullObject(false);
    }
    private bool TrySurviveLanding()
    {
        if (!_allowFallDamage) return true;
        //      Debug.Log(_startFallPosY - transform.position.y);

        if (_startFallPosY - transform.position.y > _maxFallHeight)
        {
            return false;
        }
        else
        {
            return true;
        }
    }
    public void RopeLimitExeeded()
    {
        if (_otherPlayer.movedByPlatform)
        {
            switch (State)
            {
                case STATE.LEGDE_HANG:
                    ChangeState(STATE.FALL);
                    break;
                default:
                    break;
            }
        }
    }
    public void BoundUnbound()
    {
        if (_isPaused) return;

        if (_stageController == null) return;

        if (_stageController.isUnbound)
        {
            _stageController.ReboundPlayers();
        }
        else
        {
            _stageController.UnboundPlayers();
        }
    }

    private void DecreaseMovementPower(bool decrease)
    {
        if (decrease)
        {
            if (!_ropeImpactCooldown)
            {
                _anim.SetTrigger("ropeImpact_upperBody");
                _ropeImpactCooldown = true;
            }
            moveForceMultiply = 0.3f;
            jumpForceMultiply = 0.2f;
        }
        else
        {
            moveForceMultiply = 1;
            jumpForceMultiply = 1;
        }
    }

    public void ProtectHeadFromFragile(bool isActive)
    {
        if (isActive)
        {
            _anim.SetTrigger("protectHead");
        }
        _anim.SetBool("protectHeadFragile", isActive);
    }

    #endregion

    #region kinematics
    IEnumerator EnableObiKinematics(bool isActive, float delay)
    {
        yield return new WaitForSeconds(delay);
        obiRB.kinematicForParticles = isActive;
    }

    public void ObiKinematicManagement(bool isActive)
    {

        if (!_stageController.isUnbound)
        {
            //check if player is inside maxRadius again and stop him
            if (isActive && !_ropeController.limitExeeded)
                _rb.velocity = new Vector3(0, _rb.velocity.y, 0);

            if (State == STATE.ROPE_HANG)
            {
                obiRB.kinematicForParticles = false;
                return;
            }

            if (State == STATE.IDLE && !_otherPlayer.movedByPlatform)
            {
                obiRB.kinematicForParticles = true;
                return;
            }
        }
        obiRB.kinematicForParticles = isActive;
    }

    #endregion
    private void ChangeState(STATE newState)
    {
        if (State == newState) return;

        STATE oldState = State;
        State = newState;


        //       if (playerID == 1) Debug.Log(newState + " " + Time.frameCount);

        //changing away from a state
        switch (oldState)
        {
            case STATE.IDLE:
                if (_ropeController.limitExeeded) ObiKinematicManagement(false);
                break;
            case STATE.FALL:
                _audio.PlayLoop("falling", false);
                if (TrySurviveLanding())
                {
                    isSaveFromFall = true;

                    switch (newState)
                    {
                        case (STATE.WALK):
                        case (STATE.IDLE):
                            if (!isFirstSpawn)
                            {
                                _audio.Play("landing");
                            }  
                            else
                            {
                                isFirstSpawn = false;
                            }
                            break;
                    }
                }
                else
                {
                    AudioManager.instance.PlayMonoSound("PlayerLandingDeath");
                    _stageController.RespawnPlayers();
                }
                break;
            case STATE.LADDERCLIMB:
                _rb.useGravity = true;
                _rb.velocity = Vector3.zero;
                _ladderClimbCanceled = true;
                break;
            case STATE.PUSH_OBJECT:
                if (newState != STATE.PULL_OBJECT)
                {
                    _moveableObject.StopMove();
                    _moveableObject = null;
                }
                break;
            case STATE.PULL_OBJECT:
                if (newState != STATE.PUSH_OBJECT)
                {
                    _tmpDirection *= -1;
                    _moveableObject.StopMove();
                    _moveableObject = null;
                }
                break;
            case STATE.LEGDE_HANG:
                _audio.PlayLoop("ledgehang", false);
                LedgeHang(false);
                break;
            case STATE.HOOK_HANG:
                _audio.PlayLoop("ledgehang", false);
                HookHang(false);
                break;
            case STATE.ROPE_HANG:
                _otherPlayer.DecreaseMovementPower(false);
                _collider.localPosition += new Vector3(0, 0.5f, 0);
                _collider.localScale = Vector3.one;
                _groundDetection.transform.localPosition += new Vector3(0, 0.5f, 0);
                _ropeController.SetMaxPlayerDistance(1);
                ObiKinematicManagement(true);
                break;
            default:
                break;
        }

        //entering new State
        switch (State)
        {
            case STATE.IDLE:
                ObiKinematicManagement(true);
                _anim.SetTrigger("idle");
                _anim.SetMoveSpeed(0);
                break;
            case STATE.WALK:
                _anim.SetTrigger("walk");
                break;
            case STATE.JUMP:
                _anim.SetTrigger("jump");
                _audio.Play("jump");
                break;
            case STATE.FALL:
                if (!isFirstSpawn)
                {
                    _anim.SetTrigger("fall");
                    _audio.PlayLoop("falling", true);
                }
                if (_allowFallDamage)
                {
                    isSaveFromFall = false;
                    _startFallPosY = transform.position.y;
                }
                break;
            case STATE.LEGDE_HANG:
                _anim.SetTrigger("ledgehang");
                //_audio.Play("ledgehangStart");
                _audio.PlayLoop("ledgehang", true);
                break;
            case STATE.LADDERCLIMB:
                _rb.useGravity = false;
                _rb.velocity = Vector3.zero;
                _anim.SetTrigger("ladderclimb");
                break;
            case STATE.PUSH_OBJECT:
                _tmpPosX = transform.position.x;
                AdjustMoveableMinDistance();
                _anim.SetTrigger("push");
                break;
            case STATE.PULL_OBJECT:
                _anim.SetTrigger("pull");
                break;
            case STATE.ROPE_HANG:
                _otherPlayer.DecreaseMovementPower(true);
                _rb.velocity = Vector3.zero;
                _collider.localPosition -= new Vector3(0, 0.5f, 0);
                _collider.localScale = new Vector3(1, 0.75f, 1);
                _groundDetection.transform.localPosition -= new Vector3(0, 0.5f, 0);
                _ropeController.SetMaxPlayerDistance(2);
                ObiKinematicManagement(false);
                _anim.SetTrigger("ropehang");
                break;
            case STATE.HOOK_HANG:
                _anim.SetTrigger("hookhang");
                //_audio.Play("ledgehangStart");
                _audio.PlayLoop("ledgehang", true);
                break;
            default:
                break;
        }
    }

    private void PauseGame(bool isPaused)
    {
        _isPaused = isPaused;
    }

    private void OnDestroy()
    {
        StageController.instance.gamePaused -= PauseGame;
    }

    #region enums
    public enum STATE
    {
        IDLE,
        WALK,
        JUMP,
        LEGDE_HANG,
        ROPE_HANG,
        HOOK_HANG,
        FALL,
        PUSH_OBJECT,
        PULL_OBJECT,
        LADDERCLIMB
    }

    public enum DEVICE
    {
        KEYBOARD_WASD,
        KEYBOARD_ARROW,
        PS4,
        XBOX,
        UNKNOWN
    }
    #endregion

    #region gizmos
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        if (_rb != null) Gizmos.DrawRay(transform.position, _rb.velocity * 5);
    }
    #endregion
}
