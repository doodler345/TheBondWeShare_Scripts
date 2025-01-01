using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationEvents : MonoBehaviour
{
    [SerializeField] PlayerSound _playerSound;
    [SerializeField] Menu _menu;
    [SerializeField] AudioSource _audioSource;

    public void Footstep()
    {
        _playerSound.Play("footstep");
    }

    public void LadderClimb()
    {
        _playerSound.Play("ladderClimb");
    }

    public void DoorOpen()
    {
        AudioManager.instance.PlayWorldSound(_audioSource, "doorOpen");
    }

    public void DoorClose()
    {
        AudioManager.instance.PlayWorldSound(_audioSource, "doorClose");
    }

    public void CloseCredits()
    {
        _menu.CloseCredits();
    }
}
