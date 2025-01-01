using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ButtonScript : MonoBehaviour, ISelectHandler, IDeselectHandler
{
    [SerializeField] GameObject _staticText, _animatedText, _hoverOutline;
    [SerializeField] string _effectTag = "wave";
    [SerializeField] float _effectIntensity = 0.07f;
    public bool noSoundOnFirstSelect;
    string _startTag, _endTag;


    private void Awake()
    {
        TextMeshProUGUI _animTxt = _animatedText.GetComponent<TextMeshProUGUI>();
        /*
        _animTxt.color = GetComponent<Button>().colors.selectedColor;
        */

        string _text = _staticText.GetComponent<TextMeshProUGUI>().text;

        _startTag = "<" + _effectTag + " a=" + _effectIntensity.ToString().Replace(",", ".") + ">";
        _endTag = "<" + _effectTag + ">";
        _animTxt.text = _startTag + _text + _endTag;

        GetComponent<Button>().onClick.AddListener(OnClick);

        _hoverOutline.SetActive(false);
    }


    void ISelectHandler.OnSelect(BaseEventData eventData)
    {
        if (noSoundOnFirstSelect)
        {
            noSoundOnFirstSelect = false;
        }
        else
        {
            AudioManager.instance.PlayMonoSound("UI_Navigate");
        }

        _staticText.SetActive(false);
        _animatedText.SetActive(true);
        _hoverOutline.SetActive(true);
    }
    void IDeselectHandler.OnDeselect(BaseEventData eventData)
    {
        _staticText.SetActive(true);
        _animatedText.SetActive(false);
        _hoverOutline.SetActive(false);
    }

    void OnClick() 
    {
        AudioManager.instance.PlayMonoSound("UI_Submit");
    }
}
