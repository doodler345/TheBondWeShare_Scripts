using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PressAnyButton : MonoBehaviour
{
    private TMP_Text _text;
    private Color _initColor;

    [SerializeField] private float _blinkSpeed = 2.5f;
    private float _timer = 0;
    private float _alphaValue;
    private bool _deviceRecived;

    private void Awake()
    {
        _text = GetComponent<TMP_Text>();
        _initColor = _text.color;
    }

    private void Update()
    {
        if (!_deviceRecived)
        {
            _timer += Time.deltaTime;
            _alphaValue = Mathf.Sin(_timer * _blinkSpeed) * 0.5f + 0.5f;
            _text.color = new Color(_initColor.r, _initColor.g, _initColor.b, _alphaValue);
        }
    }

    public void DeviceRecived()
    {
        _deviceRecived = true;
        _text.color = new Color(_initColor.r, _initColor.g, _initColor.b, 1);
    }
}
