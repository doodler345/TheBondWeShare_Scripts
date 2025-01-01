using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using DG.Tweening;

public class InteractableLever : MonoBehaviour
{
    public UnityEvent leverUp, leverDown, change;

    [SerializeField] Transform _handlePivot;
    [SerializeField] Vector3 _targetRot;
    [SerializeField] float _rotationDuration;

    private bool _isUp = true;
    public bool showsIcon;
    public Transform iconHolder;
    AudioSource _audioSource;

    Tween _openingTween, _closingTween;

    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
    }
    private void Start()
    {
        StageController.instance.worldSwitching += OnWorldSwitch;
        StageController.instance.playersDied += Reset;
    }

    public void ToggleLever()
    {
        showsIcon = false;
        AudioManager.instance.PlayWorldSound(_audioSource, "floorButton");

        if (_isUp)
        {
            _openingTween.Kill();
            _closingTween.Kill();
            _openingTween = _handlePivot.DORotate(_targetRot, _rotationDuration);

            _isUp = false;
            leverDown?.Invoke();
            change?.Invoke();
        }
        else
        {
            _openingTween.Kill();
            _closingTween.Kill();
            _closingTween = _handlePivot.DORotate(Vector3.zero, _rotationDuration);

            _isUp = true;
            leverUp?.Invoke();
            change?.Invoke();
        }
    }

    private void OnWorldSwitch()
    {
        showsIcon = false;
    }

    private void Reset()
    {
        showsIcon = false;
        _isUp = true;
        _handlePivot.rotation = Quaternion.identity;

    }

    private void OnDestroy()
    {
        StageController.instance.worldSwitching -= OnWorldSwitch;
        StageController.instance.playersDied -= Reset;
    }
}
