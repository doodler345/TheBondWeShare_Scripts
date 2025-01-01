using UnityEngine.Audio;
using System;
using UnityEngine;
using UnityEngine.Rendering;

public class AudioManager : MonoBehaviour
{
    [SerializeField] Sound[] _sounds;
    [SerializeField] Sound[] _music;
    private float _soundVolMultiply = 1.0f;
    private float _musicVolMultiply = 1.0f;
    private string _currentMusicName = "";
    private float[] _nightmareLayerVolumes = new float[5];
    private float _normalMusicVolume;


    [Header("WorldAudioClips_Shots")]
    [SerializeField] AudioClip[] _oneshotClips;

    [Header("WorldAudioClips_Loops")]
    [SerializeField] AudioClip[] _loopClips;

    [Header("Volumes")]
    [Range(0f, 1f)]
    [SerializeField] float _clawVol;
    [Range(0f, 1f)]
    [SerializeField] float _fragileBreakVol, _fragileJiggleVol, _movingPlatformVol;
    [Range(0f, 1f)]
    [SerializeField] float _moveableObjectVol, _moveableObjectLandingVol, _doorCloseVol, _doorOpenVol;    
    [Range(0f, 1f)]
    [SerializeField] float _floorButtonVol, _leverVol;
    [Range(0f, 1f)]
    [SerializeField] float _tooltipTextVol, _tooltipIconVol;


    public Action<float> soundVolChange;

    public static AudioManager instance;


    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        foreach (Sound s in _sounds)
        {
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.clip = s.clip;

            s.source.volume = s.volume;
            s.source.pitch = s.pitch;
            s.source.loop = s.loop;
        }

        foreach (Sound s in _music)
        {
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.clip = s.clip;

            s.source.volume = s.volume;
            s.source.pitch = s.pitch;
            s.source.loop = true;
        }


        InitMusicVolumes();
    }

    #region Sound
    private float GetSoundClipVolume(string name)
    {
        switch (name)
        {
            case "claw":            return _clawVol;

            case "fragileJiggle":   return _fragileJiggleVol;
            case "fragileBreak":    return _fragileBreakVol;
            
            case "movingPlatform":  return _movingPlatformVol;

            case "moveableObject":          return _moveableObjectVol;
            case "moveableObjectLanding":   return _moveableObjectLandingVol;
            
            case "doorOpen":        return _doorOpenVol;
            case "doorClose":       return _doorCloseVol;
            
            case "floorButton":     return _floorButtonVol;
            case "lever":           return _leverVol;

            case "tooltipText":     return _tooltipTextVol;
            case "tooltipIcon":     return _tooltipIconVol;

            default:
                Debug.LogWarning("Couldn't find Soundvolume for: " + name);
                return 1;
        }
    }

    public void PlayMonoSound(string name, float pitch = 1)
    {
        Sound s = Array.Find(_sounds, sound => sound.name == name);

        if (s == null)
        {
            Debug.LogWarning("Sound " + name + " could not be found");
            return;
        }

        s.source.pitch = pitch;
        s.source.Play();
    }

    public void PlayWorldSound(AudioSource source, string name)
    {
        AudioClip clip = null;

        float volume = this.GetSoundClipVolume(name);

        switch (name)
        {
            default:
                clip = Array.Find(_oneshotClips, clip => clip.name == name);
                break;
        }

        if (clip == null)
        {
            Debug.LogWarning("Sound " + name + " could not be found");
            return;
        }

        source.spatialBlend = 1;
        source.PlayOneShot(clip, _soundVolMultiply * volume);
    }

    public void PlayWorldSoundLoop(AudioSource source, string name, bool play)
    {
        AudioClip clip = null;

        if (play)
        {
            switch (name)
            {
                default:
                    clip = Array.Find(_loopClips, clip => clip.name == name);
                    break;
            }

            source.clip = clip;
            source.volume = _soundVolMultiply * GetSoundClipVolume(name);
            source.loop = true;
            source.Play();
        }
        else
        {
            source.Stop();
        }
    }


    public void SetMasterVolumeSound(float volume)
    {
        _soundVolMultiply = volume;
        soundVolChange?.Invoke(volume);

        foreach (Sound s in _sounds)
        {
            s.source.volume = s.volume * volume;
        }
    }

    public float GetMasterVolumeSound()
    {
        return _soundVolMultiply;
    }

#endregion

    #region Music
    public void PlayMusic(string name)
    {
        if (_currentMusicName == name) return;
        

        Sound s = Array.Find(_music, sound => sound.name == name);

        if (s == null)
        {
            Debug.LogWarning("Music " + name + " could not be found");
            return;
        }

        StopCurrentMusic();
        StopAllNightmareLayers();
        _currentMusicName = name;
        s.source.Play();
    }


    private void InitMusicVolumes()
    {
        for (int i = 1; i < 6; i++)
        {
            Sound nightmareLayer = Array.Find(_music, sound => sound.name == ("NightmareWorldLayer" + i.ToString()));

            if (nightmareLayer == null)
            {
                Debug.LogWarning("Music " + "NightmareWorldLayer" + i.ToString() + " could not be found");
                continue;
            }

            _nightmareLayerVolumes[i-1] = nightmareLayer.source.volume;
        }


        Sound normalSound = Array.Find(_music, sound => sound.name == ("NormalWorld"));
        if (normalSound == null)
        {
            Debug.LogWarning("Music " + "NormalWorld" + " could not be found");
        }
        else
        {
            _normalMusicVolume = normalSound.source.volume;
        }

        PlayNormalMusic();
        PlayAllNightmareLayers();
    }

    public void PlayNormalMusic()
    {
        Sound normalSound = Array.Find(_music, sound => sound.name == ("NormalWorld"));
        if (normalSound == null)
        {
            Debug.LogWarning("Music " + "NormalWorld" + " could not be found");
        }
        else
        {
            normalSound.source.Play();
        }
    }

    public void MuteNormalMusic(bool isMuted)
    {
        Sound normalSound = Array.Find(_music, sound => sound.name == ("NormalWorld"));
        if (normalSound == null)
        {
            Debug.LogWarning("Music " + "NormalWorld" + " could not be found");
        }
        else
        {
            normalSound.source.volume = isMuted ? 0 : _normalMusicVolume * _musicVolMultiply;
        }
    }

    public void PlayAllNightmareLayers()
    {
        StopCurrentMusic();

        for (int i = 1; i < 6;  i++)
        {
            Sound s = Array.Find(_music, sound => sound.name == ("NightmareWorldLayer" + i.ToString()));

            if (s == null)
            {
                Debug.LogWarning("Music " + "NightmareWorldLayer" + i.ToString() + " could not be found");
                continue;
            }

            s.source.Play();
        }
    }

    public void MuteAllNightmareLayers()
    {
        for (int i = 1; i < 6; i++)
        {
            Sound s = Array.Find(_music, sound => sound.name == ("NightmareWorldLayer" + i.ToString()));

            if (s == null)
            {
                Debug.LogWarning("Music " + "NightmareWorldLayer" + i.ToString() + " could not be found");
                continue;
            }

            s.source.volume = 0;
        }
    }

    public void SoloUnmuteNightmareLayer(int layer)
    {
        MuteAllNightmareLayers();

        Sound s = Array.Find(_music, sound => sound.name == ("NightmareWorldLayer" + layer.ToString()));

        if (s == null)
        {
            Debug.LogWarning("Music " + "NightmareWorldLayer" + layer.ToString() + " could not be found");
            return;
        }

        //        Debug.Log("SoloUnmute NightmareWorldLayer " + layer.ToString() + "  Vol: " + _nightmareLayerVolumes[layer - 1]);
        s.source.volume = _nightmareLayerVolumes[layer - 1] * _musicVolMultiply;
    }

    public void StopAllNightmareLayers()
    {
        for (int i = 1; i < 6; i++)
        {
            Sound s = Array.Find(_music, sound => sound.name == ("NightmareWorldLayer" + i.ToString()));

            if (s == null)
            {
                Debug.LogWarning("Music " + "NightmareWorldLayer" + i.ToString() + " could not be found");
                continue;
            }

            s.source.Stop();
        }
    }


    public void StopCurrentMusic()
    {
        if (_currentMusicName == "") return;

        Sound s = Array.Find(_music, sound => sound.name == _currentMusicName);
        
        if (s == null)
        {
            Debug.LogWarning("Sound " + _currentMusicName + " could not be found");
            return;
        }

        _currentMusicName = "";
        s.source.Stop();
    }
    public void SetMasterVolumeMusic(float volume)
    {
        _musicVolMultiply = volume;

        foreach (Sound s in _music)
        {
            s.source.volume = s.volume * volume;
        }
    }

    public float GetMasterVolumeMusic()
    {
        return _musicVolMultiply;
    }

#endregion
}
