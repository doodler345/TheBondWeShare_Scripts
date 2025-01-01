using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class IconHolder : ToolTip
{
    [SerializeField] GameObject _bgGamepad, _bgKeyboard;
    [SerializeField] Image _buttonImage;

    private void Start()
    {
        AudioManager.instance.PlayWorldSound(_audioSource, "tooltipIcon");
    }

    public void SetImage(Sprite sprite, bool isController)
    {
        if (isController)
        {
            _bgKeyboard.SetActive(false);
        }
        else
        {
            _bgGamepad.SetActive(false);
        }

        _buttonImage.sprite = sprite;
    }
}
