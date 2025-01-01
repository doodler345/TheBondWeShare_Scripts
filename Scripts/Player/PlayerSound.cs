using System;
using UnityEngine;

public class PlayerSound : MonoBehaviour
{
    [SerializeField] AudioClip[] _footstepClips_default;
    [SerializeField] AudioClip[] _footstepClips_wood;
    [SerializeField] AudioClip[] _footstepClips_leather;
    [SerializeField] AudioClip[] _footstepClips_kitchenplatform;
    [SerializeField] AudioClip[] _footstepClips_tile;
    [SerializeField] AudioClip[] _footstepClips_towel;
    [SerializeField] AudioClip[] _footstepClips_metal;
    [SerializeField] AudioClip[] _footstepClips_book;
    [SerializeField] AudioClip[] _jumpClips;
    [SerializeField] AudioClip[] _ladderClips;
    [SerializeField] AudioClip[] _ropeswingClips;
    [SerializeField] AudioClip[] _otherClips;

    [Header("Volumes")]
    [Range(0f, 1f)]
    [SerializeField] float _footstepVol;
    [Range(0f, 1f)]
    [SerializeField] float _jumpVol, _landingVol, _ledgehangStartVol, _ledgehangVol, _fallingVol, _ladderClimbVol, _otherVol;
    [Range(0f, 1f)]
    [SerializeField] float _ropeSwingVol, _ropeBreakVol;    
    float _masterMultiplier;

    [SerializeField] AudioSource _audioSourceLoop, _audioSourceShot;
    AudioManager _audioManager;
    Platform.PLATFORM_MATERIAL _floorMaterial;

    private void Start()
    {
        _audioManager = AudioManager.instance;
        _audioManager.soundVolChange += SetMasterVolume;
        StageController stageController = StageController.instance;
        if (stageController != null) StageController.instance.gamePaused += PauseLoop;

        SetMasterVolume(_audioManager.GetMasterVolumeSound());
    }

    private float GetSoundVolume(string name)
    {
        switch (name)
        {
            case "footstep": return _footstepVol;
            case "jump": return _jumpVol;
            case "landing": return _landingVol;
            case "falling": return _fallingVol;
            case "ladderClimb": return _ladderClimbVol;
            case "ledgehangStart": return _ledgehangStartVol;
            case "ledgehang": return _ledgehangVol;
            case "ropeSwing": return _ropeSwingVol;
            case "ropeBreak": return _ropeBreakVol;

            default:
                Debug.LogWarning("Couldn't find Soundvolume for: " + name);
                return 1;
        }
    }

    public void Play(string name)
    {
        if (_masterMultiplier == 0) return;

        AudioClip clip = null;

        float volume = this.GetSoundVolume(name);

        switch (name)
        {
            case "footstep":
                switch (_floorMaterial)
                {
                    case Platform.PLATFORM_MATERIAL.UNKNOWN:
                        clip = _footstepClips_default[UnityEngine.Random.Range(0, _footstepClips_default.Length)];
                        break;
                    case Platform.PLATFORM_MATERIAL.WOOD:
                        clip = _footstepClips_wood[UnityEngine.Random.Range(0, _footstepClips_wood.Length)];
                        break;
                    case Platform.PLATFORM_MATERIAL.LEATHER:
                    case Platform.PLATFORM_MATERIAL.KITCHEN_PLATFORM:
                    case Platform.PLATFORM_MATERIAL.TILE:
                    case Platform.PLATFORM_MATERIAL.TOWEL:
                    case Platform.PLATFORM_MATERIAL.METAL:
                    case Platform.PLATFORM_MATERIAL.BOOK:
                        Debug.LogWarning("no footstep sound implemented for " + _floorMaterial);
                        break;
                }
                break;

            case "jump":
                clip = _jumpClips[UnityEngine.Random.Range(0, _jumpClips.Length)];
                break;

            case "ladderClimb":
                clip = _ladderClips[UnityEngine.Random.Range(0, _ladderClips.Length)];
                break;

            case "ropeSwing":
                clip = _ropeswingClips[UnityEngine.Random.Range(0, _ropeswingClips.Length)];
                break;

            default:
                clip = Array.Find(_otherClips, clip => clip.name == name);
                break;

        }

        if (clip == null)
        {
            Debug.LogWarning("Player OneShotSound " + name + " could not be found");
            return;
        }

        //        Debug.Log($"before OneShot {name} AudioSource isPlaying: {_audioSourceShot.isPlaying}");
        _audioSourceShot.PlayOneShot(clip, _masterMultiplier * volume);
    }

    public void PlayLoop(string name, bool play)
    {
        if (_audioSourceLoop.isPlaying) _audioSourceLoop.Stop();

        if (play)
        {

            AudioClip clip = null;

            switch (name)
            {
                default:
                    clip = Array.Find(_otherClips, clip => clip.name == name);
                    break;
            }

            if (clip == null)
            {
                Debug.LogWarning("Player LoopSound " + name + " could not be found");
                return;
            }

            _audioSourceLoop.clip = clip;
            _audioSourceLoop.volume = _masterMultiplier * GetSoundVolume(name);
            _audioSourceLoop.loop = true;
            _audioSourceLoop.Play();
        }
    }

    void PauseLoop(bool isPause)
    {
        if (isPause)
        {
            _audioSourceLoop.Pause();
        }
        else
        {
            _audioSourceLoop.UnPause();
        }
    }


    public void SetFloorMaterial(Platform.PLATFORM_MATERIAL material)
    {
        _floorMaterial = material;
    }


    public void SetMasterVolume(float volume)
    {
        _masterMultiplier = volume;
    }

    private void OnDestroy()
    {
        AudioManager.instance.soundVolChange -= SetMasterVolume;

        StageController stageController = StageController.instance;
        if (stageController != null) StageController.instance.gamePaused -= PauseLoop;
    }
}
