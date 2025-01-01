using Obi;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundDetection : Detector
{
    [SerializeField] PlayerSound _playerSound;
    [SerializeField] LayerMask _groundLayer;
    RaycastHit _hit;
    Transform _prevHitTransform;
    Platform _currentPlatform;
    Platform.PLATFORM_TYPE _detectedPlatformType;

    [SerializeField] float _rayLength = 1.0f;
    [SerializeField] Vector3 _rayOffset;
    public bool grounded;
    public bool moveablePlatform;
    private bool _ignore;
    
    private void FixedUpdate()
    {
        if (grounded)
        {
            if (Physics.Raycast(transform.position, Vector3.down, out _hit, _rayLength, _groundLayer) )
            {
                if (_hit.transform != _prevHitTransform)
                {
                    _currentPlatform = _hit.transform.GetComponentInChildren<Platform>();
                    DetectPlatform();
                    _prevHitTransform = _hit.transform;
                    //      if(_currentPlatform != null)    Debug.Log("enter " + _currentPlatform.name);
                }
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        ReliableOnTriggerExit.NotifyTriggerEnter(other, gameObject, OnTriggerExit);
    }

    private void OnTriggerStay(Collider other)
    {
        if (!grounded && !_ignore) grounded = true;
    }

    private void OnTriggerExit(Collider other)
    {
        ReliableOnTriggerExit.NotifyTriggerExit(other, gameObject);

        if (grounded)
        {
            grounded = false;
            moveablePlatform = false;
            
            if (_currentPlatform != null)
            {
                Platform platform = other.transform.GetComponent<Platform>();
                if (platform == null) return;


                if (_currentPlatform == platform && platform.GetPlatformType() == Platform.PLATFORM_TYPE.FRAGILE)
                {
                    //        Debug.Log($"breaking Platform // currentPlatform: {_currentPlatform.transform.name}  // exited platform: {platform.transform.name}");
                    platform.BreakFragilePlatform();
                    _currentPlatform = null;
                }

                //      Debug.Log("exit " + _currentPlatform.name);
                
            }

        }
    }

    private void DetectPlatform()
    {
        if (_currentPlatform == null) return;
        //    Debug.Log(_currentPlatform.GetPlatformMaterial());
        _detectedPlatformType = _currentPlatform.GetPlatformType();
        _playerSound.SetFloorMaterial(_currentPlatform.GetPlatformMaterial());

        switch (_detectedPlatformType)
        {
            case Platform.PLATFORM_TYPE.MOVING:
                moveablePlatform = true;
                break;
            case Platform.PLATFORM_TYPE.FRAGILE:
                _currentPlatform.IncrementPlayerCount();
                break;
        }
    }


    public void IgnoreGroundForSeconds(float seconds)
    {
        StartCoroutine(nameof(IgnoreForSeconds), seconds);
    }
    private IEnumerator IgnoreForSeconds(float seconds)
    {
        grounded = false;
        _ignore = true;
        yield return new WaitForSeconds(seconds);
        _ignore = false;
    }


    public Platform.WORLD GetPlatformWorld()
    {
        if (_currentPlatform == null) return Platform.WORLD.BOTH;
        return _currentPlatform.World;
    }
    public Vector3 GetPlatformDeltaPos()
    {
        if (_currentPlatform == null) return Vector3.zero;
        return _currentPlatform.deltaPos;
    }

    void OnDrawGizmos()
    {
        Debug.DrawRay(transform.position + _rayOffset, Vector3.down * _rayLength, Color.blue);
        Debug.DrawRay(transform.position - _rayOffset, Vector3.down * _rayLength, Color.blue);
    }
}
