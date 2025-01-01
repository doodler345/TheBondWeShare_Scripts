using UnityEngine;
using EasyButtons;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;

public class LightManager : MonoBehaviour
{
    [Header("ReadOnly!")]
    [SerializeField] int _currentLightEvent = 0;

    [SerializeField] GameObject world_Normal;
    [SerializeField] GameObject world_Nightmare;

    LightHandler[] _lightHandlers;

    [SerializeField] List<Platform> _movingPlatforms = new List<Platform>();
    bool _initialized;

    private void Awake()
    {
        _lightHandlers = transform.GetComponentsInChildren<LightHandler>();
    }

    private void Start()
    {
        StageController.instance.worldSwitchingNightmare += ToNightmareWorld;
        StageController.instance.worldSwitchingNormal += ToNormalWorld;

        _initialized = false;
        InitLights();
    }

    private void InitLights()
    {
        _lightHandlers = transform.GetComponentsInChildren<LightHandler>(true);

        foreach (var light in _lightHandlers)
        {
            light.gameObject.SetActive(true);
            light.Init();

            switch (light.World)
            {
                case LightHandler.WORLD.BOTH:
                    break;
                case LightHandler.WORLD.NORMAL:
                    break;
                case LightHandler.WORLD.NIGHTMARE:
                    break;
            }
        }

        StartCoroutine(nameof(FinishInitLights));
    }

    private IEnumerator FinishInitLights()
    {
        yield return null;
        foreach (var light in _lightHandlers)
        {
            switch (light.World)
            {
                case LightHandler.WORLD.BOTH:
                    break;
                case LightHandler.WORLD.NORMAL:
                    break;
                case LightHandler.WORLD.NIGHTMARE:
                    light.gameObject.SetActive(false);
                    break;
            }
        }

        _initialized = true;
    }


    [Button]
    private void NextLight_EDITORONLY()
    {
        _initialized = true;

        if (_lightHandlers.Length == 0) _lightHandlers = transform.GetComponentsInChildren<LightHandler>(true);
        
        _currentLightEvent++;
        _currentLightEvent %= 6;

        if (_currentLightEvent == 0) 
        {
            world_Normal.SetActive(true);
            world_Nightmare.SetActive(false);
            ToNormalWorld();
        }
        else
        {
            world_Normal.SetActive(false);
            world_Nightmare.SetActive(true);

            foreach(var light in _lightHandlers)
            {
                light.NightmareLightSwitch_EDITORONLY();
            }
        }

    }

    [Button]
    private void ResetLights_EDITORONLY()
    {
        _initialized = true;

        world_Normal.SetActive(true);
        world_Nightmare.SetActive(false);

        _lightHandlers = transform.GetComponentsInChildren<LightHandler>(true);
        foreach (var light in _lightHandlers)
        {
            light.Init();
        }

        ToNormalWorld();
    }


    private void ToNormalWorld()
    {
        if (!_initialized) return;

        _lightHandlers = transform.GetComponentsInChildren<LightHandler>(true);

        foreach (var light in _lightHandlers)
        {
            light.Reset();

            switch (light.World)
            {
                case LightHandler.WORLD.BOTH:
                    break;
                case LightHandler.WORLD.NORMAL:
                    light.gameObject.SetActive(true);
                    break;
                case LightHandler.WORLD.NIGHTMARE:
                    light.gameObject.SetActive(false);
                    break;
            }

        }

        _currentLightEvent = 0;
    }


    private void ToNightmareWorld()
    {
        foreach (var light in _lightHandlers)
        {
            switch (light.World)
            {
                case LightHandler.WORLD.BOTH:
                    break;
                case LightHandler.WORLD.NORMAL:
                    light.gameObject.SetActive(false);
                    break;
                case LightHandler.WORLD.NIGHTMARE:
                    light.gameObject.SetActive(true);
                    break;
            }
        }
    }



    [Button]
    private void SwitchMovingPlatformPositions_EDITORONLY()
    {
        if (_movingPlatforms.Count == 0)
        {
            Debug.LogWarning("No Moving Platforms in List");
            return;
        }

        foreach (Platform platform in _movingPlatforms)
        {
            platform.SwapPos_EDITORONLY();
        }
    }

    [Button]
    private void GetAllMovingPlatforms_EDITORONLY()
    {
        _movingPlatforms.Clear();

        Platform[] allPlatforms = FindObjectsOfType<Platform>(true);
        foreach (Platform platform in allPlatforms)
        {
            if (platform.GetPlatformType() == Platform.PLATFORM_TYPE.MOVING)
            {
                _movingPlatforms.Add(platform);
            }
        }
    }

    [Button]
    private void SetInitPosMovingPlatforms_EDITORONLY()
    {
        if (_movingPlatforms.Count == 0)
        {
            Debug.LogWarning("No Moving Platforms in List");
            return;
        }

        foreach (Platform platform in _movingPlatforms)
        {
            platform.SetInitPos_EDITORONLY();
        }
    }



    private void OnDestroy()
    {
        StageController.instance.worldSwitchingNightmare -= ToNightmareWorld;
        StageController.instance.worldSwitchingNormal -= ToNormalWorld;
    }
}
