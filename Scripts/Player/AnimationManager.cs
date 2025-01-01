using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationManager : MonoBehaviour
{
    [SerializeField] private Animator _animator;
    bool _walkLeft;

    public void Turn()
    {
        _walkLeft = !_walkLeft;
        _animator.SetBool("walkLeft", _walkLeft);
    }

    public void SetTrigger(string triggername)
    {
        _animator.SetTrigger(triggername);
    }

    public void SetBool(string name, bool value)
    {
        _animator.SetBool(name, value);
    }

    public void SetMoveSpeed(float speed)
    {
        _animator.SetFloat("movespeed", speed);
    }    
    public void SetClimbSpeed(float speed)
    {
        _animator.SetFloat("climbspeed", speed);
    }

}
