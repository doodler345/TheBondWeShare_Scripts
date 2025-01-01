using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class InteractableDoor : MonoBehaviour
{
    [SerializeField] Menu _ingameMenu;
    [SerializeField] Animation _animation;
    [SerializeField] AnimationClip _animOpen, _animClose;
    public bool showsIcon;
    public Transform iconHolder;
    bool _isOpen;

    private void Start()
    {
        StageController.instance.playersDied += Reset;
        StageController.instance.worldSwitchingNormal += Reset;
    }

    public void EnterDoor()
    {
        AudioManager.instance.PlayMonoSound("LevelFinished");
        _ingameMenu.LevelFinished();
    }

    public void OpenDoor()
    {
        if (_isOpen) return;

        _animation.Play("doorOpen");
        _isOpen = true; 
    }

    public void CloseDoor()
    {
        if (!_isOpen) return;

        _animation.Play("doorClose");
        _isOpen = false;
    }

    private void Reset()
    {
        showsIcon = false;
    }

    private void OnDestroy()
    {
        StageController.instance.playersDied -= Reset;
        StageController.instance.worldSwitchingNormal -= Reset;

    }
}
