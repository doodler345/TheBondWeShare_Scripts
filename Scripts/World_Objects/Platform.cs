using DG.Tweening;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class Platform : MonoBehaviour
{
    public WORLD World;
    [SerializeField] PLATFORM_TYPE PlatformType;
    [SerializeField] PLATFORM_MATERIAL PlatformMaterial;
    StageController _stageController;
    AudioSource _audioSource;

    //MoveablePlatform
    [Header("MoveablePlatform Options")]
    [SerializeField] float _duration = 2.0f;
    [SerializeField] bool _loop;
    [SerializeField] private Vector3 _moveToPos;
    [HideInInspector] public Vector3 deltaPos = Vector3.zero;
    Tween _movingTween;
    int _loopIndex = 1;
    private Vector3 _initPos;
    private bool _isMoving;
    private Vector3 _tmpPos;

    //FragilePlatform
    [Header("FragilePlatform Options")]
    [SerializeField] GameObject _shatteredPlatformPrefab;
    [SerializeField] GameObject _fragileDangerZoneTrigger;
    [SerializeField] Transform _fragilePlatform;
    [SerializeField] MeshRenderer _meshRenderer;
    [SerializeField] float _shakeParticleMuliplier;
    [SerializeField] float shakeStrength;
    [SerializeField] int shakeVibrato;
    ParticleSystem _jiggleParticles;
    Tween _jigglePlatform;

    [Header("Particle System")]
    [SerializeField] GameObject _appearParticleSystem;
    [SerializeField] GameObject _dissolveParticleSystem;
    [SerializeField] MeshFilter _meshFilter;
    Mesh _mesh;

    Collider _collider;
    Rigidbody _rb;
    int _playerCount = 0;
    bool _isBroken;


    //specific level variables
    private int _currentPos = 0;

    private void Awake()
    {
        _collider = GetComponent<Collider>();
        _rb = GetComponent<Rigidbody>();
        _mesh = _meshFilter.mesh;

        _initPos = transform.localPosition;
        _tmpPos = _initPos;

        switch (PlatformType)
        {
            case PLATFORM_TYPE.MOVING:
                _audioSource = transform.AddComponent<AudioSource>();
                _audioSource.loop = true;
                _audioSource.spatialBlend = 1;
                break;

            case PLATFORM_TYPE.FRAGILE:
                _jiggleParticles = GetComponentInChildren<ParticleSystem>();
                _audioSource = GetComponent<AudioSource>();
                _fragileDangerZoneTrigger.SetActive(false);
                var emission = _jiggleParticles.emission;
                emission.rateOverTime = transform.localScale.x * transform.localScale.y * _shakeParticleMuliplier;
                break;

            default:
                break;
        }
    }

    private void Start()
    {
        StageController.instance.playersDied += Reset;

        switch (PlatformType)
        {
            case PLATFORM_TYPE.MOVING:
                StageController.instance.gamePaused += PauseLoop;
                if (_loop)
                {
                    StartLoopMove();
                }
                break;

            case PLATFORM_TYPE.FRAGILE:
                StageController.instance.gamePaused += PauseLoop;
                StageController.instance.resetFragilePlatforms += Reset;
                break;

            default:
                break;
        }
    }

    private void Update()
    {
        if (_isMoving)
        {
            deltaPos = transform.localPosition - _tmpPos;
            _tmpPos = transform.localPosition;
        }
    }


    #region moveablePlatform
    private void StartLoopMove()
    {
        _isMoving = true;
        AudioManager.instance.PlayWorldSoundLoop(_audioSource, "movingPlatform", true);
        LoopMove();
    }

    private void LoopMove()
    {

        if (_loopIndex == 0)
        {
            _movingTween = transform.DOLocalMove(_initPos, _duration).SetEase(Ease.Linear).OnComplete(() => LoopMove());
        }
        else
        {
            _movingTween = transform.DOLocalMove(_moveToPos, _duration).SetEase(Ease.Linear).OnComplete(() => LoopMove());
        }

        _loopIndex = 1 - _loopIndex;
    }

    public void MoveToPos1()
    {
        if (_loop)
        {
            Debug.LogWarning("Moving Platform cant be moved manually, because its set to loop");
            return;
        }

        _movingTween.Kill();
        _isMoving = true;
        AudioManager.instance.PlayWorldSoundLoop(_audioSource, "movingPlatform", true);
        _movingTween = transform.DOLocalMove(_initPos, _duration).OnComplete(() => StopMoving());
    }
    public void MoveToPos2()
    {
        if (_loop)
        {
            Debug.LogWarning("Moving Platform cant be moved manually, because its set to loop");
            return;
        }

        _movingTween.Kill();
        _isMoving = true;
        AudioManager.instance.PlayWorldSoundLoop(_audioSource, "movingPlatform", true);
        _movingTween = transform.DOLocalMove(_moveToPos, _duration).OnComplete(() => StopMoving());
    }

    public void SwapPos()
    {
        switch (_currentPos)
        {
            case 0:
                MoveToPos2();
                break;
            case 1:
                MoveToPos1();
                break;
            default:
                break;
        }

        _currentPos = 1 - _currentPos;
    }
    private void StopMoving()
    {
        _isMoving = false;
        _audioSource.Stop();
    }



    public void SwapPos_EDITORONLY()
    {
        switch (_currentPos)
        {
            case 0:
                if (_initPos == null)
                {
                    Debug.LogWarning("No init pos set!!!");
                    return;
                }
                transform.localPosition = _moveToPos;
                break;
            case 1:
                transform.localPosition = _initPos;
                break;
            default:
                break;
        }

        _currentPos = 1 - _currentPos;
    }

    public void SetInitPos_EDITORONLY()
    {
        if (transform.localPosition == _moveToPos)
        {
            Debug.LogWarning("Platform is at target position, this cannot be the init pos!!!");
            return;
        }

        _initPos = transform.localPosition;
    }


    #endregion

    #region fragilePlatform

    public void IncrementPlayerCount()
    {
        _playerCount++;
        if (_playerCount == 1) JigglePlatform();
        if (_playerCount > 1) BreakFragilePlatform();
    }
    private void JigglePlatform()
    {
        AudioManager.instance.PlayWorldSoundLoop(_audioSource, "fragileJiggle", true);
        _jigglePlatform = transform.DOShakePosition(1000, shakeStrength, shakeVibrato);
        _jiggleParticles.Play();
    }

    public void BreakFragilePlatform()
    {
        if (_isBroken) return;

        _fragileDangerZoneTrigger.SetActive(true);

        _audioSource.Stop();
        AudioManager.instance.PlayWorldSound(_audioSource, "fragileBreak");
        _jigglePlatform.Kill();
        _jiggleParticles.Stop();
        _isBroken = true;
        _meshRenderer.enabled = false;
        _collider.enabled = false;
        Instantiate(_shatteredPlatformPrefab, this.transform.position, this.transform.rotation);

        StartCoroutine(nameof(DisableFragileDangerZone));
    }

    private IEnumerator DisableFragileDangerZone()
    {
        yield return new WaitForSeconds(1);
        _fragileDangerZoneTrigger.SetActive(false);
    }

    public bool IsFragileBroken()
    {
        return _isBroken;
    }

    #endregion

    private void Reset()
    {
        if (PlatformType == PLATFORM_TYPE.STATIC) return;
        switch (PlatformType)
        {
            case PLATFORM_TYPE.STATIC:
                break;
            case PLATFORM_TYPE.MOVING:
                if (_loop) return;

                _movingTween.Kill();
                _isMoving = false;
                transform.localPosition = _initPos;
                _currentPos = 0;
                break;
            case PLATFORM_TYPE.FRAGILE:
                _jigglePlatform.Kill();
                _jiggleParticles.Stop();
                _meshRenderer.enabled = true;
                _collider.enabled = true;
                _fragileDangerZoneTrigger.SetActive(false);
                StopCoroutine(nameof(DisableFragileDangerZone));

                transform.localPosition = _initPos;
                _playerCount = 0;

                if (_isBroken && _appearParticleSystem != null && !SceneHandler.instance.isLoadingNewScene)
                {
                    GameObject particleSystemPrefab = Instantiate(_appearParticleSystem, this.transform.position, Quaternion.identity);
                    Vector2 scale = this.transform.localScale;
                    particleSystemPrefab.transform.rotation = this.transform.rotation;
                    particleSystemPrefab.transform.localScale = scale;
                    particleSystemPrefab.GetComponentInChildren<DissolveParticleSystem>().Setup(scale.x, scale.y);
                }
                _isBroken = false;

                break;
        }
    }

    public PLATFORM_TYPE GetPlatformType()
    {
        return PlatformType;
    }
    public PLATFORM_MATERIAL GetPlatformMaterial()
    {
        return PlatformMaterial;
    }

    void PauseLoop(bool isPause)
    {
        if (isPause)
        {
            _audioSource.Pause();
        }
        else
        {
            _audioSource.UnPause();
        }
    }

    private void OnDisable()
    {
        if (Time.timeSinceLevelLoad < 1) return;
        if (_isBroken) return;

        if (_dissolveParticleSystem != null && !SceneHandler.instance.isLoadingNewScene) 
        {
            GameObject particleSystemPrefab = Instantiate(_dissolveParticleSystem, this.transform.position, Quaternion.identity);
            Vector2 scale = this.transform.localScale;
            particleSystemPrefab.transform.rotation = this.transform.rotation;
            particleSystemPrefab.transform.localScale = scale;
            particleSystemPrefab.GetComponentInChildren<DissolveParticleSystem>().Setup(scale.x, scale.y);
        }

    }

    private void OnDestroy()
    {
        StageController.instance.playersDied -= Reset;

        switch (PlatformType)
        {
            case PLATFORM_TYPE.MOVING:
            case PLATFORM_TYPE.FRAGILE:
                StageController.instance.gamePaused -= PauseLoop;
                StageController.instance.resetFragilePlatforms -= Reset;
                break;
        }

        _jigglePlatform.Kill();
        _movingTween.Kill();
    }

    public enum WORLD
    {
        BOTH,
        NORMAL,
        NIGHTMARE
    }


    public enum PLATFORM_TYPE
    {
        STATIC,
        MOVING,
        FRAGILE
    }

    public enum PLATFORM_MATERIAL
    {
        UNKNOWN,
        WOOD,
        LEATHER,
        KITCHEN_PLATFORM,
        TILE,
        TOWEL,
        METAL,
        BOOK
    }
}
