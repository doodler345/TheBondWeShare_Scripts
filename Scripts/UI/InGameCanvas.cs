using DG.Tweening;
using DG.Tweening.Core.Easing;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

class Icon
{
    public int id;
    public GameObject iconHolder;    
};

class TextHint
{
    public int id;
    public GameObject textHolder;
};

public class InGameCanvas : MonoBehaviour
{
    Camera cam;

    [SerializeField] private bool _useHudBlackScreen;
    [SerializeField] private float _blackScreenTransitionDuration = 2;
    [SerializeField] private Image _blackScreen;

    [SerializeField] private Image _hudMaskImage;
    [SerializeField] private Sprite[] _hudMasks;
    private int _hudMaskIndex = 0;

    [SerializeField] private GameObject _textHolderPrefab;
    [SerializeField] private GameObject _iconHolderPrefab;

    int _iconID = 0;
    int _hintTextID = 0;
    List<Icon> _iconList = new List<Icon>();
    List<TextHint> _hintTextList = new List<TextHint>();

    public static InGameCanvas instance;

    private void Awake()
    {
        if (instance != null && instance != this)
            Destroy(this);
        else
            instance = this;

        cam = Camera.main;
    }

    private void Start()
    {
        StageController.instance.timerEvent += SwitchHUDMask;
        StageController.instance.worldSwitching += ResetScratchedHUD;

        if (_useHudBlackScreen)
        {
            _blackScreen.gameObject.SetActive(true);
            Color endColor = new Color(0,0,0,0);
            _blackScreen.DOColor(endColor, _blackScreenTransitionDuration);
        }
    }

    #region Icon
    public void RegisterIcon(Transform iconPos, Sprite sprite, bool isGamepad, out int iconID)
    {
        Icon newIcon = new Icon();
        GameObject iconHolderObject = Instantiate(_iconHolderPrefab);
        iconHolderObject.transform.position = iconPos.position;
        iconHolderObject.GetComponent<IconHolder>().SetImage(sprite, isGamepad);

        iconID = _iconID;

        newIcon.id = iconID;
        newIcon.iconHolder = iconHolderObject;
        _iconList.Add(newIcon);

        _iconID++;
    }

    public void UnregisterIcon(int iconID)
    {
        if (_iconList.Count == 0) return; //happens if worldswitching

        int index = _iconList.FindIndex(x => x.id == iconID);
        Destroy(_iconList[index].iconHolder);
        _iconList.RemoveAt(index);
    }

    public void ClearAllIcons()
    {
        for (int i = 0; i < _iconList.Count; i++)
        {
            Destroy(_iconList[i].iconHolder);
        }
        _iconList.Clear();
    }

    #endregion

    #region TextHint
    public void RegisterText(Transform textPos, string hintText, out int iconID, float _fontSizeMultiply = 1)
    {
        TextHint newHint = new TextHint();
        GameObject textHolderObject = Instantiate(_textHolderPrefab);
        TextMeshProUGUI text = textHolderObject.GetComponent<TextMeshProUGUI>();
        textHolderObject.transform.position = textPos.position;
        textHolderObject.transform.localScale *= _fontSizeMultiply;
        text.text = "<wave a=0.05>" + hintText + "</wave>";

        iconID = _hintTextID;

        newHint.id = _hintTextID;
        newHint.textHolder = textHolderObject;
        _hintTextList.Add(newHint);
        
        _hintTextID++;
    }

    public void UnregisterText(int textID)
    {
        if (_hintTextList.Count == 0) return; //happens if worldswitching

        int index = _hintTextList.FindIndex(x => x.id == textID);
        Destroy(_hintTextList[index].textHolder);
        _hintTextList.RemoveAt(index);
    }
    public void ClearAllTexts()
    {
        for (int i = 0; i < _hintTextList.Count; i++)
        {
            Destroy(_hintTextList[i].textHolder);
        }
        _hintTextList.Clear();
    }
    #endregion

    public void SetEverythingActive(bool isActive)
    {
        foreach (TextHint textHint in _hintTextList)
        {
            textHint.textHolder.SetActive(isActive);
        }
        foreach (Icon icon in _iconList)
        {
            icon.iconHolder.SetActive(isActive);
        }
    }


    #region HUD
    private void SwitchHUDMask()
    {
        _hudMaskIndex++;
        if(_hudMaskIndex > _hudMasks.Length - 1)
        {
            Debug.Log("missing HUD masks!");
        }
        else
        {
            _hudMaskImage.sprite = _hudMasks[_hudMaskIndex];
        }
    }

    private void ResetScratchedHUD()
    {
        _hudMaskIndex = 0;
        _hudMaskImage.sprite = _hudMasks[0];
    }

    #endregion

    private void OnDestroy()
    {
        StageController.instance.timerEvent -= SwitchHUDMask;
        StageController.instance.worldSwitching -= ResetScratchedHUD;   
    }
}
