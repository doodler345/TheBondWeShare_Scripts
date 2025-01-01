using TMPro;
using UnityEngine;

public class Detector : MonoBehaviour
{
    protected Sprite _iconSprite;
    protected Transform _iconHolder;
    protected int _detectedObjectID;
    [SerializeField]protected bool _showsThisPlayersIcon;

    protected string _device;

    protected bool _isGamepad;

    public void Init(PlayerMovement.DEVICE device, string type)
    {
        switch (device)
        {
            case PlayerMovement.DEVICE.KEYBOARD_WASD:
                _device = "wasd";
                //_iconSprite = _wasd;
                break;
            case PlayerMovement.DEVICE.KEYBOARD_ARROW:
                _device = "arrows";
               // _iconSprite = _arrow;
                break;
            case PlayerMovement.DEVICE.PS4:
                _device = "ps4";
                //_iconSprite = _ps4;
                _isGamepad = true;
                break;
            case PlayerMovement.DEVICE.XBOX:
                _device = "xbox";
                //_iconSprite = _xbox;
                _isGamepad = true;
                break;
            case PlayerMovement.DEVICE.UNKNOWN:
                _device = "unknown";
                break;
        }
        
        _iconSprite = Icons.instance.GetSprite(_device, type);
    }

    public void ShowIcon(bool isActive)
    {
        if (isActive)
        {
            InGameCanvas.instance.RegisterIcon(_iconHolder, _iconSprite, _isGamepad, out _detectedObjectID);
        }
        else if (_showsThisPlayersIcon) InGameCanvas.instance.UnregisterIcon(_detectedObjectID);

        _showsThisPlayersIcon = isActive;
    }
}
