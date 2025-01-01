using UnityEngine;
using UnityEngine.Events;
using System;
using UnityEngine.InputSystem;

public class StageController : MonoBehaviour
{

    [SerializeField] VCamController _vCamController;
    
    [SerializeField] GameObject _playerSetup_p1Left_Prefab, _playerSetup_p2Left_Prefab;
    private GameObject _playerSetupPrefab;
    [SerializeField] Mesh _p1Mesh, _p2Mesh;
    RopeController _ropeController;
    
    [HideInInspector] public GameObject playerSetup, player1, player2;
    public Transform playerMid;
    bool _firstTimeSpawn = true;
    
    public Transform spawn;
    MeshRenderer _spawnMeshRenderer;

    Coroutine _ropeIsTearing;

    [HideInInspector] public Vector2 currentPlayerDistance;
    [HideInInspector] public bool isUnbound;

    [Header("Set manually per stage")]
    [SerializeField] GameObject world1;
    [SerializeField] GameObject world2;
    [SerializeField] GameObject world_both;

    [Header("General Options")]
    [SerializeField] bool _showSpawn;
    [SerializeField] float _maxReboundDistance = 2;
    [SerializeField] float _worldswitchCooldownThreshold = 0.5f;
    float _worldswitchCooldownTimer;
    bool _worldswitchCooldown;

    [Header("NormalWorld")]
    [SerializeField] UnityEvent _resetEffects;

    [Header("NightmareWorld")]
    [SerializeField] float _maxUnboundTime = 25.0f;
    [SerializeField] float[] _percentEventThreshold;
    [SerializeField] UnityEvent[] _timerEvents;
    int _eventIndex = 0, _eventsSize;
    bool _timerActive;
    float _timer;


    [HideInInspector] public int deathCount = 0;

    public Action timerEvent;
    [HideInInspector] public Action worldSwitching;
    [HideInInspector] public Action worldSwitchingNormal;
    [HideInInspector] public Action worldSwitchingNightmare;
    [HideInInspector] public Action playersDied;
    [HideInInspector] public Action resetFragilePlatforms;
    [HideInInspector] public Action checkpointChanged;
    [HideInInspector] public Action<bool> gamePaused;


    public static StageController instance;

    private void Awake()
    {
        if (instance != null && instance != this)
            Destroy(this);
        else
            instance = this;

        _eventsSize = _timerEvents.Length;

        _spawnMeshRenderer = spawn.GetComponent<MeshRenderer>();
        if(!_showSpawn) _spawnMeshRenderer.enabled = false;
    }

    private void Start()
    {
        SpawnPlayers(spawn.position, true);
        AudioManager.instance.PlayNormalMusic();
        AudioManager.instance.PlayAllNightmareLayers();
        AudioManager.instance.MuteAllNightmareLayers();
    }

    private void Update()
    {
        UpdatePlayerMid();
        NightmareWorldTimer();
        CheckForCheats();

        if (_worldswitchCooldown)
        {
            _worldswitchCooldownTimer += Time.deltaTime;
            if (_worldswitchCooldownTimer > _worldswitchCooldownThreshold)
            {
                _worldswitchCooldown = false;
            }
        }

    }


    private void SpawnPlayers(Vector3 position, bool isP1Left)
    {
        _playerSetupPrefab = isP1Left ? _playerSetup_p1Left_Prefab : _playerSetup_p2Left_Prefab; 

        playerSetup = Instantiate(_playerSetupPrefab, position, Quaternion.identity);
        _ropeController = playerSetup.GetComponentInChildren<RopeController>();

        player1 = playerSetup.transform.GetChild(1).gameObject;
        player2 = playerSetup.transform.GetChild(2).gameObject;

        if (_firstTimeSpawn)
        {
            player1.GetComponent<PlayerMovement>().isFirstSpawn = true;
            player2.GetComponent<PlayerMovement>().isFirstSpawn = true;
            _firstTimeSpawn = false;
        }

        //rebind Players
        PlayerInputOld oldInputP1 = player1.GetComponent<PlayerInputOld>();
        PlayerInputOld oldInputP2 = player2.GetComponent<PlayerInputOld>();
        oldInputP1.UpdatePlayerID();
        oldInputP2.UpdatePlayerID();
        oldInputP1.enabled = true;
        oldInputP2.enabled = true;
        var inputHandlers = FindObjectsOfType<PlayerInputHandler>();
        foreach (var inputHandler in inputHandlers)
        {
            inputHandler.BindPlayerToControlls();
        }

        SwitchWorld(true);
        isUnbound = false;
    }

    public void ReboundPlayers()
    {
        if (!isUnbound) return;
        if (_worldswitchCooldown) return;
        if (!player1.GetComponentInChildren<GroundDetection>().grounded || !player2.GetComponentInChildren<GroundDetection>().grounded) return;

        float distance;
        Vector3 respawnPos;
        Vector3 p1Pos = player1.transform.position;
        Vector3 p2Pos = player2.transform.position;

        distance = (p1Pos - p2Pos).sqrMagnitude;

        if (distance > _maxReboundDistance) return;

        _worldswitchCooldownTimer = 0;
        _worldswitchCooldown = true;

        respawnPos = p1Pos + 0.5f * (p2Pos - p1Pos);

        DestroyPlayers();
        AudioManager.instance.PlayMonoSound("RopeBound");
        SpawnPlayers(respawnPos, CheckPlayerOrder());
    }

    public void UnboundPlayers()
    {
        if (_ropeIsTearing != null || _worldswitchCooldown) return;
        _ropeIsTearing = StartCoroutine(_ropeController.CutRope(3));

        _worldswitchCooldownTimer = 0;
        _worldswitchCooldown = true;

        _ropeController = null;

        AudioManager.instance.PlayMonoSound("RopeUnbound");
        isUnbound = true;
        SwitchWorld(false);
    }
    private bool CheckPlayerOrder()
    {
        if (player1.transform.position.x < player2.transform.position.x) return true;
        else 
            return false;
    }

    public void RespawnPlayers()
    {
        DestroyPlayers();
        SpawnPlayers(spawn.transform.position, CheckPlayerOrder());
        deathCount++;
        playersDied?.Invoke();
    }

    private void DestroyPlayers()
    {
        Destroy(playerSetup);

        if (_ropeIsTearing != null)
        {
            StopCoroutine(_ropeIsTearing);
            _ropeIsTearing = null;
        }
    }



    private void UpdatePlayerMid()
    {
        if (player1 == null) return;

        currentPlayerDistance = player2.transform.position - player1.transform.position;
        Vector2 betweenPlayers = (Vector2)player1.transform.position + 0.5f * currentPlayerDistance;

        playerMid.position = betweenPlayers;
    }

    public void SwitchWorld(bool playersConnected)
    {
        if(playersConnected) ToNormalWorld();
        else ToNightmareWorld();

        InGameCanvas.instance.ClearAllIcons();
        InGameCanvas.instance.ClearAllTexts();
        player1.GetComponent<PlayerMovement>().WorldSwitch();
        player2.GetComponent<PlayerMovement>().WorldSwitch();

        worldSwitching?.Invoke();
    }

    private void ToNormalWorld()
    {
        world1.SetActive(true);
        world2.SetActive(false);
        world_both.SetActive(true);
        
        worldSwitchingNormal?.Invoke();
        _resetEffects?.Invoke();
        
        AudioManager.instance.MuteNormalMusic(false);
        AudioManager.instance.MuteAllNightmareLayers();

        _timerActive = false;
    }

    private void ToNightmareWorld()
    {
        world1.SetActive(false);
        world2.SetActive(true);
        world_both.SetActive(true);

        worldSwitchingNightmare?.Invoke();

        _timer = _maxUnboundTime;
        _eventIndex = 0;

        AudioManager.instance.MuteNormalMusic(true);
        AudioManager.instance.PlayMonoSound("EnterNightmareWorld");

        _timerActive = true;
    }

    private void NightmareWorldTimer()
    {
        if (!_timerActive) return;

        _timer -= Time.deltaTime;
        float timeLeftInPercent = _timer / _maxUnboundTime * 100;

        if (timeLeftInPercent < 0)
        {
            AudioManager.instance.PlayMonoSound("NightmareWorldDeath");
            RespawnPlayers();
            return;
        }

        if (_eventIndex < _eventsSize)
        {

            if (timeLeftInPercent <= _percentEventThreshold[_eventIndex])
            {
                _timerEvents[_eventIndex]?.Invoke();
                timerEvent?.Invoke();
                int layerIndex = _eventIndex + 1;
                AudioManager.instance.SoloUnmuteNightmareLayer(layerIndex);
                
                int switcher = _eventIndex % 2 + 1;
                float pitch = UnityEngine.Random.Range(0.5f, 1.2f);
                AudioManager.instance.PlayMonoSound("glassBreaking" + switcher.ToString(), pitch);

                _eventIndex++;
            }
        }
    }

    public void CheckpointReached(Vector3 newSpawnPos)
    {
        //        Debug.Log("new Checkpoint reached");
        spawn.position = newSpawnPos;
        checkpointChanged?.Invoke();
    }


    public void ResetFragilePlatforms()
    {
        resetFragilePlatforms?.Invoke();
    }

    public void TogglePause(bool isPause)
    {
        Time.timeScale = isPause ? 0.0f : 1.0f;
        gamePaused?.Invoke(isPause);

        //change action map
        string actionMap = isPause ? "UI" : "Player";

        var playerInputs = FindObjectsOfType<PlayerInput>();
        foreach (PlayerInput input in playerInputs)
        {
            input.SwitchCurrentActionMap(actionMap);
        }
    }

    private int _respawnCheatCounter = 0;
    private int _spawnBtwPlayersCheatCounter = 0;
    private int _moveSpawnFreelyCheatCounter = 0;
    bool _moveSpawnFreely;

    #region Cheats
    private void CheckForCheats()
    {
        if (Input.anyKey)
        {
            if (_moveSpawnFreely)
            {
                if (Input.GetKeyDown(KeyCode.Return))
                {
                    {
                        gamePaused?.Invoke(false);
                        _vCamController.ToggleLevelOverview(false);
                        _moveSpawnFreely = false;
                        RespawnPlayers();

                        if (!_showSpawn) _spawnMeshRenderer.enabled = false;
                    }
                }

                float moveSpawnSpeed = 10f;
                Vector3 direction = Vector2.zero;
                if (Input.GetKey(KeyCode.LeftArrow))
                {
                    direction += Vector3.left;
                }
                if (Input.GetKey(KeyCode.RightArrow))
                {
                    direction -= Vector3.left;
                }
                if (Input.GetKey(KeyCode.UpArrow))
                {
                    direction += Vector3.up;
                }
                if (Input.GetKey(KeyCode.DownArrow))
                {
                    direction -= Vector3.up;
                }

                spawn.transform.position += direction * moveSpawnSpeed * Time.deltaTime;

            }
        }

        if (Input.anyKeyDown)
        {


            if (Input.GetKeyDown("6"))
            {
                if (isUnbound || _moveSpawnFreely) return;

                _spawnBtwPlayersCheatCounter++;

                if (_spawnBtwPlayersCheatCounter == 3)
                {
                    spawn.transform.position = playerMid.position + Vector3.up * 0.5f;
                    _spawnMeshRenderer.enabled = true;
                    Invoke(nameof(HideSpawn), 1);
                    _spawnBtwPlayersCheatCounter = 0;
                }
            }
            else
            {
                _spawnBtwPlayersCheatCounter = 0;
            }            
            
            if (Input.GetKeyDown("7"))
            {
                if (_moveSpawnFreely) return;
               
                _respawnCheatCounter++;

                if (_respawnCheatCounter == 3)
                {
                    RespawnPlayers();
                    _respawnCheatCounter = 0;
                }
            }
            else
            {
                _respawnCheatCounter = 0;
            }
            
            if (Input.GetKeyDown("8"))
            {
                _moveSpawnFreelyCheatCounter++;

                if (_moveSpawnFreelyCheatCounter == 3)
                {
                    gamePaused?.Invoke(true);
                    _vCamController.ToggleLevelOverview(true);
                    _moveSpawnFreely = true;
                    _moveSpawnFreelyCheatCounter = 0;
                    _spawnMeshRenderer.enabled = true;
                }
            }
            else
            {
                if (_moveSpawnFreely) return;
                _moveSpawnFreelyCheatCounter = 0;
            }

        }
    }

    private void HideSpawn()
    {
        if (!_showSpawn) _spawnMeshRenderer.enabled = false;
    }

    #endregion
}
