using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PlayerLoop;

public class MoveableObject : MonoBehaviour
{
    public bool getsMoved;
    public bool showsIcon;

    public Transform iconHolder;
    [SerializeField] private Vector3 _iconOffset;
    [SerializeField] private LayerMask _groundLayer;
    Vector3 _initPos;
    AudioSource _audioSource;
    Rigidbody _rb;
    bool _grounded = true;

    private void Awake()
    {
        _initPos = transform.position;
        _audioSource = GetComponent<AudioSource>(); 
        _rb = GetComponent<Rigidbody>();
    }
    private void Start()
    {
        StageController.instance.worldSwitching += OnWorldSwitch;
        StageController.instance.playersDied += Reset;
    }

    private void FixedUpdate()
    {
        if (_rb.velocity.y < 0)
        {
            if (!Physics.Raycast(transform.position, Vector3.down, 2, _groundLayer))
            {
                if(_grounded)
                {
                    _grounded = false;
                }
            }
        }
    }

    public void PushPull(float deltaX)
    {
        if(deltaX == 0)
        {
            if (_audioSource.isPlaying) _audioSource.Stop();
            return;
        }

        if (!_audioSource.isPlaying) AudioManager.instance.PlayWorldSoundLoop(_audioSource, "moveableObject", true);

        Vector3 deltaXPos = new Vector3(deltaX, 0, 0);
        _rb.MovePosition(transform.position += deltaXPos);
    }

    public void StartMove()
    {
        showsIcon = false;
        getsMoved = true;
    }

    public void StopMove()
    {
        getsMoved = false;
        if (_audioSource.isPlaying) _audioSource.Stop();
    }

    private void Update()
    {
        Vector3 newIconPos = transform.position + _iconOffset;
        if (iconHolder.position != newIconPos) iconHolder.position = newIconPos;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "MoveableBound")
        {
            _rb.velocity = Vector3.zero;
            transform.position = _initPos;
        }

    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            if (!_grounded)
            {
                AudioManager.instance.PlayWorldSound(_audioSource, "moveableObjectLanding");
                _grounded = true;
            }
        }
    }

    private void OnWorldSwitch()
    {
        showsIcon = false;
    }

    private void Reset()
    {
        showsIcon = false;
        getsMoved = false;
        _grounded = true;
        _rb.velocity = Vector3.zero;
        transform.position = _initPos;
    }

    private void OnDestroy()
    {
        StageController.instance.worldSwitching -= OnWorldSwitch;
        StageController.instance.playersDied -= Reset;
    }
}
