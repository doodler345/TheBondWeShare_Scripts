using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class Claw : MonoBehaviour
{
    [SerializeField] Transform _leftPivot, _rightPivot;
    [SerializeField] Vector3 _finalRotLeft, _finalRotRight;
    [SerializeField] float _openingDuration, _closeDuration;
    Vector3 _initRotLeft, _initRotRight;
    Tween _openingTweenL, _openingTweenR;
    AudioSource _audioSource;

    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();

        _initRotLeft = _leftPivot.rotation.eulerAngles;
        _initRotRight = _rightPivot.rotation.eulerAngles;
    }

    private void Start()
    {
        StageController.instance.playersDied += Reset;
    }

    public void Open()
    {
        _openingTweenL.Kill();
        _openingTweenR.Kill();
        AudioManager.instance.PlayWorldSound(_audioSource, "claw");
        _openingTweenL = _leftPivot.DORotate(_finalRotLeft, _openingDuration);
        _openingTweenR = _rightPivot.DORotate(_finalRotRight, _openingDuration);
    }
    public void Close()
    {
        _openingTweenL.Kill();
        _openingTweenR.Kill();
        AudioManager.instance.PlayWorldSound(_audioSource, "claw");
        _openingTweenL = _leftPivot.DORotate(_initRotLeft, _closeDuration);
        _openingTweenR = _rightPivot.DORotate(_initRotRight, _closeDuration);
    }

    private void Reset()
    {
        _leftPivot.DORotate(_initRotLeft, 0);
        _rightPivot.DORotate(_initRotRight, 0);
        _openingTweenL.Kill();
        _openingTweenR.Kill();
    }

    private void OnDestroy()
    {
        StageController.instance.playersDied -= Reset;
    }
}
