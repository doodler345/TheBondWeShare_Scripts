using DG.Tweening;
using System.Net;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

public class FloorButton : MonoBehaviour
{
    [SerializeField] UnityEvent _buttonPressed, _buttonReleased;
    [SerializeField] Transform _model;
    Vector3 _modelInitPos;
    AudioSource _audioSource;
    
    Tween _moveDown, _moveUp;

    [SerializeField] bool _isPressed;
    [SerializeField] int _playerCount = 0;
    [SerializeField] int _moveableCount = 0;
    bool _moveableRemains;

    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
        _modelInitPos = _model.position;
    }
    private void Start()
    {
        StageController.instance.playersDied += Reset;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag.Contains("Player"))
        {
            _playerCount++;
        }        
        else if (other.tag.Contains("Moveable"))
        {
            if (_moveableRemains)  _moveableRemains = false;
            else _moveableCount++;
        }

        if (_isPressed) return;

        ButtonPressed(true);
    }

    private void OnTriggerExit(Collider other)
    {
        //      Debug.Log($"Exiting Button: {other.transform.name}");

        if (other.tag.Contains("Player"))
        {
            _playerCount--;
        }
        else if (other.tag.Contains("Moveable"))
        {
            _moveableCount--;
        }
        else return;


        if (_playerCount == 0 && _moveableCount == 0)
        {
            ButtonPressed(false);
        }
    }

    private void ButtonPressed(bool down)
    {
        //        Debug.Log($"ButtonPressed: {down}");

        if (down)
        {
            if (_isPressed) return;

            AudioManager.instance.PlayWorldSound(_audioSource, "floorButton");
            _isPressed = true;
            _buttonPressed?.Invoke();

            if (_moveUp != null && _moveUp.IsPlaying()) _moveUp.Kill();
            _moveDown = _model.DOMove(_model.position + Vector3.down * 0.2f, 0.3f);
        }
        else
        {
            if (!_isPressed) return;

            AudioManager.instance.PlayWorldSound(_audioSource, "floorButton");
            _isPressed = false;
            _buttonReleased?.Invoke();

            if (_moveDown != null && _moveDown.IsPlaying()) _moveDown.Kill();
            _moveUp = _model.DOMove(_modelInitPos, 0.2f);
        }
    }

    private void Reset()
    {
        _playerCount = 0;
        _moveableCount = 0;
        ButtonPressed(false); 
        _isPressed = false;
        _moveUp = _model.DOMove(_modelInitPos, 0f);
    }

    private void OnDisable()
    {
        if (_moveableCount == 0) Reset(); //reset on worldswitch if no moveable 
        else
        {
            _moveableRemains = true;
            _playerCount = 0;
        }
    }

    private void OnDestroy()
    {
        StageController.instance.playersDied -= Reset;
    }
}
