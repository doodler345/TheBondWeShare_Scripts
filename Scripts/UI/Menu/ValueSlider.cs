using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.Rendering.DebugUI;

public class ValueSlider : MonoBehaviour
{
    [SerializeField] private SliderType _sliderType;
    PlayerSound[] _playerSound = new PlayerSound[2];
    AudioManager _audioManager;
    StageController _stageController;
    Menu _menu;
    bool _init = true;

    private void Start()
    {
        _audioManager = AudioManager.instance;
        _stageController = StageController.instance;
        _menu = FindObjectOfType<Menu>();

        Slider slider = GetComponent<Slider>();

        switch (_sliderType)
        {
            case SliderType.SOUND_VOLUME:
                slider.value = _audioManager.GetMasterVolumeSound();
                break;
            case SliderType.MUSIC_VOLUME:
                slider.value = _audioManager.GetMasterVolumeMusic();
                break;
        }
    }

    public void ChangeVolume(float value)
    {
        if (_init)
        {
            _init = false;
            return;
        }

        switch (_sliderType)
        {

            case SliderType.SOUND_VOLUME:

                /*
                if (_menu.menuType == Menu.MenuType.INGAME)
                {
                    if (_playerSound[0] == null)
                    {
                        _playerSound[0] = _stageController.player1.GetComponent<PlayerSound>();
                        _playerSound[1] = _stageController.player2.GetComponent<PlayerSound>();
                    }

                    _playerSound[0].SetVolume(value);
                    _playerSound[1].SetVolume(value);
                }
                */

                _audioManager.PlayMonoSound("UI_Navigate");
                _audioManager.SetMasterVolumeSound(value);
                break;

            case SliderType.MUSIC_VOLUME:
                _audioManager.SetMasterVolumeMusic(value);
                break;
        }
    }

    private enum SliderType
    {
        SOUND_VOLUME,
        MUSIC_VOLUME
    }
}
