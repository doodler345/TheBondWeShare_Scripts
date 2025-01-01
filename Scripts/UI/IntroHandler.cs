using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public class IntroHandler : MonoBehaviour
{

    [SerializeField] Vector2 _videoDuration;
    Menu _menu;

    void Start()
    {
        _menu = FindObjectOfType<Menu>();

        GetComponent<AudioSource>().volume *= AudioManager.instance.GetMasterVolumeMusic();

        float seconds = _videoDuration.x * 60 + _videoDuration.y;
        Invoke(nameof(IntroEnd), seconds);
    }

    public void StartSkipping(bool isStart)
    {
        if (isStart)
        {
            _menu.StartSkip();
        }
        else
        {
            _menu.CancleSkip();
        }
    }

    public void SkipIntro()
    {
        CancelInvoke(nameof(IntroEnd));
        IntroEnd();
    }

    private void IntroEnd()
    {
        SceneHandler sceneHandler = SceneHandler.instance;
        sceneHandler.NextScene();
    }

}
