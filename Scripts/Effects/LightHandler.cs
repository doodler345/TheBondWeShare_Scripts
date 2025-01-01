using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Net.Http.Headers;
using UnityEditor;
using UnityEngine;
using static Platform;

public class LightHandler : MonoBehaviour
{
    public WORLD World;
    [SerializeField] Light _light;
    [SerializeField] Color _normalWorldColor;
    [SerializeField] Color _nightmareColorStart;
    [SerializeField] Color _nightmareColorEnd;
    [SerializeField] Color[] _nightmareColors;
    [SerializeField] float _lightSwitchDuration = 2f;
    Tween _lightSwitch;

    int _lightSwitchAmount = 5; //change if not right anymore
    int _lightIndex = 0;
    Color _falloffs;

    bool _initalized;

    private void Start()
    {
        StageController.instance.timerEvent += NightmareLightSwitch;
        switch (World)
        {
            case WORLD.NORMAL:
                break;
            case WORLD.BOTH:
                break;
            case WORLD.NIGHTMARE:
                break;
        }
    }

    public void Init()
    {
        _lightIndex = 0;

        switch (World)
        {
            case WORLD.NORMAL:
                _light.color = _normalWorldColor;
                break;
            case WORLD.BOTH:
            case WORLD.NIGHTMARE:
                _nightmareColors = new Color[_lightSwitchAmount];
                Color falloff = (_nightmareColorStart - _nightmareColorEnd) / (_lightSwitchAmount - 1);
                for (int i = 0; i < _lightSwitchAmount; i++)
                {
                    _nightmareColors[i] = _nightmareColorStart - (falloff * i);
                }

                _initalized = true;

                break;
        }
    }

    private void ToNormalWorld()
    {
        _lightIndex = 0;

        switch (World)
        {
            case WORLD.BOTH:
            case WORLD.NORMAL:
                _light.color = _normalWorldColor;
                break;
            case WORLD.NIGHTMARE:
                break;
        }
    }


    public void NightmareLightSwitch()
    {
        if (_lightIndex == 0)
        {
            switch (World)
            {
                case WORLD.NORMAL:
                    break;
                case WORLD.BOTH:
                case WORLD.NIGHTMARE:
                    _lightSwitch = _light.DOColor(_nightmareColorStart, _lightSwitchDuration);
                    break;
            }
        }

        else
        {
            switch (World)
            {
                case WORLD.NORMAL:
                    break;
                case WORLD.BOTH:
                case WORLD.NIGHTMARE:
                    _lightSwitch = _light.DOColor(_nightmareColors[_lightIndex], _lightSwitchDuration);
                    break;
            }
        }

        _lightIndex++;
    }

    public void Reset()
    {
        if (_lightSwitch != null) _lightSwitch.Kill();

        ToNormalWorld();
    }

    private void OnDestroy()
    {
        StageController.instance.timerEvent -= NightmareLightSwitch;
    }






    public void NightmareLightSwitch_EDITORONLY()
    {
        if (_lightIndex >= _lightSwitchAmount)
        {
            ToNormalWorld();
            return;
        }

        switch (World)
        {
            case WORLD.NORMAL:
                gameObject.SetActive(false);
                break;
            case WORLD.BOTH:
            case WORLD.NIGHTMARE:
                gameObject.SetActive(true);
                _light.color = _nightmareColors[_lightIndex];
                break;
        }

        _lightIndex++;

    }

    public enum WORLD
    {
        BOTH,
        NORMAL,
        NIGHTMARE
    }
}
