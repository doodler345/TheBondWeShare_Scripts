using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallDetection : Detector
{
    [Header("Wall")] 
    [SerializeField] LayerMask _wallLayer;
    [SerializeField] float _wallRayLength = 0.05f;
    [SerializeField] Vector3 _halfExtends, _wallRaydirection;

    [Header("Ledge")]
    [SerializeField] LayerMask _groundLayer;
    [SerializeField] float _ledgeRayLength = 1.0f;
    [SerializeField] Vector3 _ledgeRayOffset;
    RaycastHit _ledgeHit;
    private Platform _currentPlatform;
    PlayerMovement _playerMovement;
    Rigidbody _rb;
    public bool hanging;
    public bool moveablePlatform;
    public bool fragilePlatform;
    private bool _ignore;


    private void Awake()
    {
        _rb = GetComponentInParent<Rigidbody>();
        _playerMovement = GetComponentInParent<PlayerMovement>();
    }


    private void FixedUpdate()
    {
        //ledge
        if (_ignore) return;

        if (_rb.velocity.y < 0 && Physics.Raycast(transform.position + _ledgeRayOffset, Vector3.down, out _ledgeHit, _ledgeRayLength, _groundLayer))
        {
            if (CheckForCeiling()) return;

            if (!hanging)
            {
                hanging = true;
                _currentPlatform = _ledgeHit.transform.GetComponentInChildren<Platform>();
                //        Debug.Log(_currentPlatform);
                CheckIfPlatformType();
                _playerMovement.LedgeHang(true);
            }
        }
    }

    public bool CheckForFrontWall()
    {
        return Physics.BoxCast(transform.position, _halfExtends, _wallRaydirection, Quaternion.identity, _wallRayLength, _wallLayer);
    }   
    public bool CheckForBackWall()
    {
        return Physics.BoxCast(transform.position, _halfExtends, -_wallRaydirection, Quaternion.identity, _wallRayLength, _wallLayer);
    }
    private bool CheckForCeiling()
    {
        return Physics.Raycast(transform.position, Vector3.up, 1.25f, _groundLayer);
    }


    public void Turn()
    {
        _wallRaydirection.x *= -1;
        _ledgeRayOffset.x *= -1;
    }

    public IEnumerator IgnoreLedgesForSeconds(float seconds)
    {
        _ignore = true;
        yield return new WaitForSeconds(seconds);
        _ignore = false;
    }

    public Platform.WORLD GetLedgePlatformWorld()
    {
        return _currentPlatform.World;
    }
    private void CheckIfPlatformType()
    {
        if (_currentPlatform == null) return;
        if (_currentPlatform.GetPlatformType() == Platform.PLATFORM_TYPE.MOVING) moveablePlatform = true;
        else if (_currentPlatform.GetPlatformType() == Platform.PLATFORM_TYPE.FRAGILE) fragilePlatform = true;
    }

    public void Unhang()
    {
        hanging = false;
        _currentPlatform = null;
        moveablePlatform = false;
        fragilePlatform = false;
    }
    public Vector3 GetPlatformDeltaPos()
    {
        if (_currentPlatform == null) return Vector3.zero;
        return _currentPlatform.deltaPos;
    }

    public Platform GetDetectedPlatform()
    { 
        return _currentPlatform; 
    }

    void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(transform.position + _wallRaydirection * _wallRayLength, _halfExtends);
        Gizmos.DrawWireCube(transform.position - _wallRaydirection * _wallRayLength, _halfExtends);
        Debug.DrawRay(transform.position + _ledgeRayOffset, Vector3.down * _ledgeRayLength, Color.yellow);

        Debug.DrawRay(transform.position, Vector3.up * 1.25f, Color.blue);
    }

}
