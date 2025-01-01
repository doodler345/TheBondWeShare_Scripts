using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FragileDangerZone : MonoBehaviour
{
    bool _playerDetected;
    PlayerMovement _detectedPlayer;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !_playerDetected)
        {
            ReliableOnTriggerExit.NotifyTriggerEnter(other, gameObject, OnTriggerExit);
            _detectedPlayer = other.GetComponentInParent<PlayerMovement>();
            _detectedPlayer.ProtectHeadFromFragile(true);
            _playerDetected = true;
        }
    }

    private void OnDisable()
    {
        if (!_playerDetected) return;
        
        _playerDetected = false;
        _detectedPlayer.ProtectHeadFromFragile(false);
    }

    private void OnTriggerExit(Collider other)
    {
        ReliableOnTriggerExit.NotifyTriggerExit(other, gameObject);

        if (_playerDetected && other.CompareTag("Player"))
        {
            _playerDetected = false;
            _detectedPlayer.ProtectHeadFromFragile(false);
        }
    }
}
