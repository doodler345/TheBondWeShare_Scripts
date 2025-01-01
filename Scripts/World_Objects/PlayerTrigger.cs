using Cinemachine;
using System.Collections;
using UnityEngine;

public class PlayerTrigger : MonoBehaviour
{
    StageController _stageController;
    InGameCanvas _gameCanvas;

    [SerializeField] TriggerType _triggerType;
    bool _p1Recognized, _p2Recognized;

    [Header("Optional (Will be forced by script if needed)")]
    [SerializeField] bool _bothPlayersNeedToEnter;
    [SerializeField] bool _bothPlayersNeedToExit;

    [Header("Checkpoint")]
    [SerializeField] Candle[] _candles;
    [SerializeField] Transform _checkpointSpawn;
    bool _checkpointActive;
    bool _p1SaveFromFall, _p2SaveFromFall;

    [Header("Text_Hint")]
    [SerializeField] int _deathsTillShow;
    [SerializeField] Transform _textHolder;
    [SerializeField] Transform _iconHolder1, _iconHolder2;
    [SerializeField] string _textToShow;
    [SerializeField] float _fontSizeMultiply = 1;
    [SerializeField] bool _showIcons;
    [SerializeField] Sprite _WASD, _ARROWS, _PS4, _XBOX, _UNKNOWN;
    Sprite[] _icon = new Sprite[2];
    int[] _iconIndex = new int[2];
    bool[] _isGamepad = new bool[2];
    int _textIndex;
    bool _showsText;

    [Header("Level-Finish")]
    [SerializeField] InteractableDoor _door;
    [SerializeField] Collider _nextLevelCollider;

    [Header("Camera_Change")]
    [SerializeField] CinemachineVirtualCamera _dedicatedVCam;

    [Header("Audio-Emitter")]
    [SerializeField] Transform _colliderTransform;
    [SerializeField] AudioClip _audioClip;
    [Range(0f, 1f)]
    [SerializeField] float _volume = 1f;
    [SerializeField] float _cooldownTime = 20f;
    [SerializeField] bool _loopAudio;
    AudioSource _audioSource;
    float[] _playerDistances = new float[2];
    float _masterVolume;
    bool _isAudioCooldown;

    #region init
    private void Awake()
    {
        switch (_triggerType)
        {
            case TriggerType.CHECKPOINT:
                _bothPlayersNeedToEnter = true;
                break;
            case TriggerType.DEADZONE:
                _bothPlayersNeedToEnter = false;
                float deadzoneScaleX = transform.localScale.x;
                ParticleSystem ps = GetComponentInChildren<ParticleSystem>();
                ParticleSystem.MainModule main = ps.main;
                main.maxParticles = (int)(deadzoneScaleX * 100);
                ParticleSystem.EmissionModule particleEmission = ps.emission;
                particleEmission.rateOverTime = deadzoneScaleX * 10;
                break;
            case TriggerType.TEXT_HINT:
                _bothPlayersNeedToExit = true;
                break;
            case TriggerType.CAMERA_CHANGE:
                break;
            case TriggerType.LEVEL_FINISH:
                _nextLevelCollider.enabled = false;
                _bothPlayersNeedToEnter = true;
                _bothPlayersNeedToExit = false;
                break;
            case TriggerType.AUDIO_EMITTER:
                _audioSource = GetComponent<AudioSource>();

                if (_loopAudio)
                {
                    _audioSource.clip = _audioClip;
                    _audioSource.loop = true;
                    _bothPlayersNeedToExit = true;
                }
                else
                {
                    _bothPlayersNeedToExit = false;
                }

                _bothPlayersNeedToEnter = false;
                break;
            case TriggerType.FRAGILE_RESETTER:
                _bothPlayersNeedToEnter = true;
                _bothPlayersNeedToExit = true;
                break;
        }
    }

    private void Start()
    {
        _gameCanvas = InGameCanvas.instance;
        _stageController = StageController.instance;


        _stageController.worldSwitching += WorldSwitches;
        _stageController.playersDied += PlayersDied;

        switch (_triggerType)
        {
            case TriggerType.TEXT_HINT:
                if (_showIcons)
                {
                    StartCoroutine(SetPlayerIcons());
                }
                break;

            case TriggerType.AUDIO_EMITTER:
                AudioManager audioManager = AudioManager.instance;
                _masterVolume = audioManager.GetMasterVolumeSound();
                audioManager.soundVolChange += SetMasterVolume;
                _stageController.gamePaused += PauseLoop;
                break;

            case TriggerType.CHECKPOINT:
                _stageController.checkpointChanged += ResetCheckpoint;
                break;
        }
    }

    private IEnumerator SetPlayerIcons()
    {
        yield return new WaitForEndOfFrame();

        PlayerMovement.DEVICE[] devices = new PlayerMovement.DEVICE[2];
        devices[0] = _stageController.player1.GetComponent<PlayerMovement>().Device;
        devices[1] = _stageController.player2.GetComponent<PlayerMovement>().Device;

        for (int i = 0; i < devices.Length; i++)
        {
            switch (devices[i])
            {
                case PlayerMovement.DEVICE.KEYBOARD_WASD:
                    _icon[i] = _WASD;
                    break;
                case PlayerMovement.DEVICE.KEYBOARD_ARROW:
                    _icon[i] = _ARROWS;
                    break;
                case PlayerMovement.DEVICE.PS4:
                    _icon[i] = _PS4;
                    _isGamepad[i] = true;
                    break;
                case PlayerMovement.DEVICE.XBOX:
                    _icon[i] = _XBOX;
                    _isGamepad[i] = true;
                    break;
                case PlayerMovement.DEVICE.UNKNOWN:
                    _icon[i] = _UNKNOWN;
                    break;
            }
        }
    }
    #endregion

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        ReliableOnTriggerExit.NotifyTriggerEnter(other, gameObject, OnTriggerExit);

        PlayerMovement detectedPlayer = other.gameObject.GetComponentInParent<PlayerMovement>();

        switch (detectedPlayer.playerID)
        {
            case 0:
                _p1Recognized = true;
                break;
            case 1:
                _p2Recognized = true;
                break;
        }
        //        Debug.Log(gameObject.name + " recognized P1: " + _p1Recognized + " P2: " + _p2Recognized);
        if (_bothPlayersNeedToEnter)
        {
            if (!_p1Recognized || !_p2Recognized) return;
        }


        switch (_triggerType)
        {
            case TriggerType.CHECKPOINT:
                break;
            case TriggerType.DEADZONE:
                AudioManager.instance.PlayMonoSound("NightmareWorldDeath");
                StageController.instance.RespawnPlayers();
                break;
            case TriggerType.TEXT_HINT:

                if (StageController.instance.deathCount < _deathsTillShow) return;

                if (!_showsText)
                {
                    _gameCanvas.RegisterText(_textHolder, _textToShow, out _textIndex, _fontSizeMultiply);
                    if (_showIcons)
                    {
                        _gameCanvas.RegisterIcon(_iconHolder1, _icon[0], _isGamepad[0], out _iconIndex[0]);
                        _gameCanvas.RegisterIcon(_iconHolder2, _icon[1], _isGamepad[1], out _iconIndex[1]);
                    }
                    _showsText = true;
                }
                break;
            case TriggerType.CAMERA_CHANGE:
                _dedicatedVCam.Priority = 100;
                break;
            case TriggerType.AUDIO_EMITTER:
                if (_loopAudio)
                {
                    if (!_audioSource.isPlaying)
                    {
                        _audioSource.Play();
                        //        Debug.Log("loop started playing");
                    }
                }
                else if (!_isAudioCooldown)
                {
                    PlayAudio();
                }
                break;
            case TriggerType.FRAGILE_RESETTER:
                StageController.instance.ResetFragilePlatforms();
                break;
        }


    }

    private void OnTriggerStay(Collider other)
    {
        if (!other.CompareTag("Player")) return;


        PlayerMovement detectedPlayer = other.gameObject.GetComponentInParent<PlayerMovement>();

        switch (_triggerType)
        {
            case TriggerType.CHECKPOINT:

                if (_checkpointActive) return;

                if (detectedPlayer.isSaveFromFall)
                {
                    SetPlayerSaveFromFall(detectedPlayer, true);
                }
                else return;

                if (_p1Recognized && _p2Recognized && _p1SaveFromFall && _p2SaveFromFall)
                {
                    _stageController.CheckpointReached(_checkpointSpawn.position);
                    AudioManager.instance.PlayMonoSound("CheckpointReached");
                    _checkpointActive = true;

                    foreach (Candle c in _candles)
                    {
                        //    Debug.Log("Switch on Candles: " + Time.frameCount);

                        c.ActivateCandle(true);
                    }
                }
                break;

            case TriggerType.DEADZONE:
                break;

            case TriggerType.TEXT_HINT:
                break;

            case TriggerType.LEVEL_FINISH:


                if (detectedPlayer.isSaveFromFall)
                {
                    SetPlayerSaveFromFall(detectedPlayer, true);
                }
                else return;

                //      Debug.Log(_p1Recognized + " " + _p2Recognized + " " + _p1SaveFromFall + " " + _p2SaveFromFall);

                if (_p1Recognized && _p2Recognized && _p1SaveFromFall && _p2SaveFromFall)
                {
                    _door.OpenDoor();
                    _nextLevelCollider.enabled = true;
                }
                break;

            case TriggerType.AUDIO_EMITTER:

                if (_loopAudio)
                {
                    UpdateLoopedAudioVolume(detectedPlayer);
                }
                break;
        }

    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        ReliableOnTriggerExit.NotifyTriggerExit(other, gameObject);
        PlayerMovement detectedPlayer = other.gameObject.GetComponentInParent<PlayerMovement>();
        if (detectedPlayer == null) return;

        if (_bothPlayersNeedToExit)
        {
            switch (detectedPlayer.playerID)
            {
                case 0:
                    _p1Recognized = false;
                    break;
                case 1:
                    _p2Recognized = false;
                    break;
            }

            if (_p1Recognized || _p2Recognized) return;
        }


        switch (_triggerType)
        {
            case TriggerType.CHECKPOINT:
                SetPlayerSaveFromFall(detectedPlayer, false);
                break;
            case TriggerType.DEADZONE:
                break;
            case TriggerType.TEXT_HINT:
                if (_showsText)
                {
                    _gameCanvas.UnregisterText(_textIndex);
                    if (_showIcons)
                    {
                        _gameCanvas.UnregisterIcon(_iconIndex[0]);
                        _gameCanvas.UnregisterIcon(_iconIndex[1]);
                    }
                    _showsText = false;
                }
                break;
            case TriggerType.CAMERA_CHANGE:
                _dedicatedVCam.Priority = 0;
                break;
            case TriggerType.LEVEL_FINISH:
                SetPlayerSaveFromFall(detectedPlayer, false);

                _door.CloseDoor();
                _nextLevelCollider.enabled = false;
                break;
            case TriggerType.AUDIO_EMITTER:
                if (_loopAudio)
                {
                    StopLoop();
                }
                break;


        }

    }

    #region Checkpoint

    private void ResetCheckpoint()
    {
        foreach (Candle c in _candles)
        {
            //        Debug.Log("Switch off Candles: " + Time.frameCount);
            c.ActivateCandle(false);
            _checkpointActive = false;
        }
    }

    #endregion

    #region Audio-Emitter

    void StopCooldown()
    {
        _isAudioCooldown = false;
    }

    void SetMasterVolume(float volume)
    {
        _masterVolume = volume;
    }

    void PlayAudio()
    {
        if (_masterVolume == 0) return;

        _audioSource.PlayOneShot(_audioClip, _masterVolume * _volume);
        _isAudioCooldown = true;
        Invoke(nameof(StopCooldown), _cooldownTime);
    }

    void StopLoop()
    {
        _audioSource.volume = 0;
        _audioSource.Stop();
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

    void UpdateLoopedAudioVolume(PlayerMovement player)
    {
        float sqrDistance;

        if (_p1Recognized && _p2Recognized)
        {
            _playerDistances[player.playerID] = (this.transform.position - player.transform.position).sqrMagnitude;
            if (_playerDistances[0] < _playerDistances[1])
            {
                sqrDistance = _playerDistances[0];
            }
            else
            {
                sqrDistance = _playerDistances[1];
            }
        }
        else
        {
            sqrDistance = (this.transform.position - player.transform.position).sqrMagnitude;
            //        Debug.Log(0.5f * _colliderTransform.localScale.x / sqrDistance);
        }

        _audioSource.volume = Mathf.Lerp(0, _masterVolume * _volume, 0.5f * _colliderTransform.localScale.x / sqrDistance);
    }

    #endregion

    private void SetPlayerSaveFromFall(PlayerMovement player, bool setSafe)
    {
        switch (player.playerID)
        {
            case 0:
                _p1SaveFromFall = setSafe;
                break;
            case 1:
                _p2SaveFromFall = setSafe;
                break;
        }
    }

    private void WorldSwitches()
    {
        switch (_triggerType)
        {
            case TriggerType.CHECKPOINT:
                break;
            case TriggerType.DEADZONE:
                break;
            case TriggerType.TEXT_HINT:
                _showsText = false;
                break;
        }

        switch (_triggerType)
        {
            case TriggerType.LEVEL_FINISH:
            case TriggerType.FRAGILE_RESETTER:
                break;
            default:
                _p1Recognized = false;
                _p2Recognized = false;
                break;
        }

        _p1SaveFromFall = false;
        _p2SaveFromFall = false;
    }

    private void PlayersDied()
    {
        if (_dedicatedVCam != null) _dedicatedVCam.Priority = 0;

        switch (_triggerType)
        {
            case TriggerType.AUDIO_EMITTER:
                if (_loopAudio)
                {
                    StopLoop();
                }
                break;
        }
    }

    private enum TriggerType
    {
        CHECKPOINT,
        DEADZONE,
        TEXT_HINT,
        CAMERA_CHANGE,
        LEVEL_FINISH,
        AUDIO_EMITTER,
        FRAGILE_RESETTER
    }

    private void OnDestroy()
    {
        switch (_triggerType)
        {
            case (TriggerType.AUDIO_EMITTER):
                AudioManager.instance.soundVolChange -= SetMasterVolume;
                StageController.instance.gamePaused -= PauseLoop;
                break;

            case TriggerType.CHECKPOINT:
                _stageController.checkpointChanged -= ResetCheckpoint;
                break;
        }
    }
}
