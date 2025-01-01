using Obi;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class Menu : MonoBehaviour
{
    public MenuType menuType;

    [Header("Type: Get-Inputs")]
    [SerializeField] private GameObject _btnContinue;
    [SerializeField] private GameObject _containerP1, _containerP2;
    [SerializeField] private TMP_Text _txtP1_btnReciever, _txtP2_btnReciever;
    [SerializeField] private Image _imageP1, _imageP2;
    [SerializeField] private Sprite _spritePS4, _spriteXBOX, _spriteWASD, _spriteArrows, _spriteUnknown;
    [SerializeField] private Transform _p1BondParent, _p2BondParent;
    [SerializeField] private Transform _p1BondConnector, _p2BondConnector;
    private PressAnyButton _pressAnyBtnScript_P1, _pressAnyBtnScript_P2;
    private int _deviceCount = 0;
    private int[] _deviceIDs = new int[2];

    [Header("Type: MainMenu")]
    [SerializeField] private GameObject _mainButtons;     
    
    [Header("Type: Intro")]
    [SerializeField] private IntroHandler _introHandler;    

    [Header("Type: InGame")]
    [SerializeField] private GameObject _ingameMenuPanel;
    [SerializeField] private GameObject _levelFinishPanel;
    [SerializeField] Selectable _continueButton, _levelFinishContinueButton;
    [SerializeField] TMP_Text _txtDeathCount;
    bool _showInGameMenu, _levelFinished;
    int _deathCount = 0;

    [Header("Type: Credits")]
    [SerializeField] Animator _creditsAnimator;

    [Header("Settings")]
    [SerializeField] private GameObject _settingsPanel;
    [SerializeField] Selectable _settingsFirstSelected;
    [SerializeField] Selectable _settingsButton;
    bool _showSettings;

    [Header("Others")]
    [SerializeField] Slider _skipSlider;
    bool _holdSkipButton;


    private void Start()
    {
        switch (menuType)
        {
            case MenuType.GET_INPUTS:
                _containerP2.SetActive(false);
                _btnContinue.SetActive(false);
                _imageP1.gameObject.SetActive(false);
                _imageP2.gameObject.SetActive(false);

                _pressAnyBtnScript_P1 = _txtP1_btnReciever.gameObject.GetComponent<PressAnyButton>();
                _pressAnyBtnScript_P2 = _txtP2_btnReciever.gameObject.GetComponent<PressAnyButton>();

                AudioManager.instance.PlayMusic("NormalWorld");

                Invoke(nameof(ParentBond), 0.05f);
                break;

            case MenuType.MAIN_MENU:
                AudioManager.instance.PlayMusic("NormalWorld");
                _settingsPanel.SetActive(false);
                break;

            case MenuType.INTRO:
                AudioManager.instance.StopCurrentMusic();
                _skipSlider.gameObject.SetActive(false);
                break;

            case MenuType.OUTRO:
                AudioManager.instance.MuteNormalMusic(true);
                AudioManager.instance.StopCurrentMusic();
                _skipSlider.gameObject.SetActive(false);
                break;

            case MenuType.INGAME:
                _ingameMenuPanel.SetActive(false);
                _levelFinishPanel.SetActive(false);
                _settingsPanel.SetActive(false);
                break;

            case MenuType.CREDITS:
                AudioManager.instance.MuteNormalMusic(false);
                AudioManager.instance.PlayMusic("NormalWorld");
                _skipSlider.gameObject.SetActive(false);
                break;
        }

        RebindPlayerInputs();
    }

    private void RebindPlayerInputs()
    {
        var playerInputs = FindObjectsOfType<PlayerInputHandler>();
        foreach (PlayerInputHandler input in playerInputs)
        {
            input.BindPlayerToControlls();
        }
    }

    private void Update()
    {
        if (_holdSkipButton)
        {
            _skipSlider.value += Time.deltaTime / 2;

            if (_skipSlider.value == 1)
            {
                switch (menuType)
                {
                    case MenuType.INTRO:
                    case MenuType.OUTRO:
                        _introHandler.SkipIntro();
                        break;

                    case MenuType.CREDITS:
                        CloseCredits();
                        break;
                }
            }
        }
    }

    public void OnPauseButtonPress()
    {
        switch (menuType)
        {
            case MenuType.GET_INPUTS:
                return;

            case MenuType.MAIN_MENU:
                if (_showSettings)
                {
                    CloseSettings();
                }
                break;

            case MenuType.INGAME:
                if (_showSettings)
                {
                    CloseSettings();
                }
                else
                {
                    ToggleInGamePause();
                }
                break;

            case MenuType.CREDITS:
                SceneHandler.instance.MainMenu();
                break;
        }

        AudioManager.instance.PlayMonoSound("UI_Submit");
    }

    #region GetInputs
    public void InputRecieved(InputDevice inputDevice, out string deviceType)
    {
        Sprite deviceSprite = _spriteUnknown;
        deviceType = inputDevice.name;

        if (_deviceCount == 2) return;

        if (deviceType.Contains("Keyboard"))
        {
            deviceType = "Keyboard (WASD)";
            deviceSprite = _spriteWASD;
        }
        else if (deviceType.Contains("DualShock"))
        {
            deviceType = "DualShock";
            deviceSprite = _spritePS4;
        }
        else if (deviceType.Contains("XInput"))
        {
            deviceType = "XBOX";
            deviceSprite = _spriteXBOX;
        }
        else
        {
            deviceType = "Unknown Device";
        }


        switch (_deviceCount)
        {
            case 0:
                _pressAnyBtnScript_P1.gameObject.SetActive(false);
                _containerP2.gameObject.SetActive(true);
                _imageP1.sprite = deviceSprite;
                _imageP1.gameObject.SetActive(true);

                _pressAnyBtnScript_P1.DeviceRecived();
                _deviceIDs[0] = inputDevice.deviceId;
                break;

            case 1:

                if (_deviceIDs[0] == inputDevice.deviceId)
                {
                    if (deviceType != "Keyboard (WASD)") return;
                    else
                    {
                        deviceType = "Keyboard (ARROW KEYS)";
                        deviceSprite = _spriteArrows;
                    }
                }

                _pressAnyBtnScript_P2.gameObject.SetActive(false);
                _txtP2_btnReciever.text = deviceType;
                _imageP2.sprite = deviceSprite;
                _imageP2.gameObject.SetActive(true);

                Invoke(nameof(ActivateContinueButton), 0);

                _pressAnyBtnScript_P2.DeviceRecived();
                _deviceIDs[1] = inputDevice.deviceId;
                break;
        }
        
        AudioManager.instance.PlayMonoSound("UI_DeviceRecieved");

        _deviceCount++;
    }

    private void ActivateContinueButton()
    {
        _btnContinue.SetActive(true);
        _btnContinue.GetComponent<Button>().Select();   
    }

    private void ParentBond()
    {
        _p1BondConnector.SetParent(_p1BondParent);
        _p2BondConnector.SetParent(_p2BondParent);
    }
    #endregion

    #region InGame

    public void ToggleInGamePause()
    {
        if (_levelFinished) return;

        if (_showInGameMenu)
        {
            StageController.instance.TogglePause(false);
            InGameCanvas.instance.SetEverythingActive(true);
            _showInGameMenu = false;
            _ingameMenuPanel.SetActive(false);
        }
        else
        {
            StageController.instance.TogglePause(true);
            InGameCanvas.instance.SetEverythingActive(false);
            _showInGameMenu = true;
            _ingameMenuPanel.SetActive(true);
            _continueButton.Select();
        }
    }

    public void LevelFinished()
    {
        StageController.instance.TogglePause(true);
        InGameCanvas.instance.SetEverythingActive(false);
        _levelFinished = true;
        _txtDeathCount.text = StageController.instance.deathCount.ToString();
        _levelFinishPanel.SetActive(true);
        _levelFinishContinueButton.Select();
    }

    #endregion

    #region Credits

    public void CloseCredits()
    {
        SceneHandler.instance.MainMenu();
    }

    public void ChangeScrollSpeed(bool speedUp)
    {
        float speed = speedUp ? 3f : -1.0f;
        _creditsAnimator.SetFloat("speed", speed);
    }

    public void ResetScrollSpeed()
    {
        _creditsAnimator.SetFloat("speed", 1);
    }

    #endregion

    public void StartSkip()
    {
        _skipSlider.value = 0;
        _skipSlider.gameObject.SetActive(true);
        _holdSkipButton = true;
    }
    public void CancleSkip()
    {
        if (_skipSlider != null)
        {
            _skipSlider.gameObject.SetActive(false);
        }
        _holdSkipButton = false;
    }

    public void OpenSettings()
    {
        if (_showSettings) return;

        switch (menuType)
        {
            case MenuType.GET_INPUTS:
                return;

            case MenuType.MAIN_MENU:
                _mainButtons.SetActive(false);
                break;

            case MenuType.INGAME:
                _ingameMenuPanel.SetActive(false);
                break;
        }

        _showSettings = true;
        _settingsPanel.SetActive(true);
        _settingsFirstSelected.Select();
    }

    public void CloseSettings()
    {
        if (!_showSettings) return;

        switch (menuType)
        {
            case MenuType.GET_INPUTS:
                return;

            case MenuType.MAIN_MENU:
                _mainButtons.SetActive(true);
                break;

            case MenuType.INGAME:
                _ingameMenuPanel.SetActive(true);
                break;
        }

        _showSettings = false;
        _settingsButton.GetComponent<ButtonScript>().noSoundOnFirstSelect = true;
        _settingsButton.Select();
        _settingsPanel.SetActive(false);
    }

    public bool ShowInGameMenu()
    {
        return _showInGameMenu;
    }

    private void OnDisable()
    {
        if (Time.timeScale == 0) Time.timeScale = 1;
    }

        
    public enum MenuType
    {
        GET_INPUTS,
        INGAME,
        MAIN_MENU,
        INTRO,
        OUTRO,
        CREDITS
    }
}
