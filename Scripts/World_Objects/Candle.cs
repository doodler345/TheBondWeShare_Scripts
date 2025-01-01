using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Candle : MonoBehaviour
{
    [SerializeField] Material _matOn, _matOff;
    [SerializeField] Renderer _renderer;
    [SerializeField] GameObject _flame, _audioEmitter;
    [SerializeField] float _lightUpDuration = 2;
    float _initSizeY;

    private void Start()
    {
        _initSizeY = _flame.transform.localScale.y;
        ActivateCandle(false);
    }

    public void ActivateCandle(bool isActive)
    {
        if (isActive)
        {
            _renderer.material = _matOn;
            _flame.SetActive(true);
            if (_audioEmitter != null) _audioEmitter.SetActive(true);
            _flame.transform.DOScaleY(_initSizeY, _lightUpDuration);
        }
        else
        {
            _renderer.material = _matOff;
            _flame.SetActive(false);
            if (_audioEmitter != null) _audioEmitter.SetActive(false);
            _flame.transform.localScale = new Vector3(1, 0, 1);
        }
    }

}
