using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Android;
using UnityEngine.UIElements;

public class DissolveParticleSystem : MonoBehaviour
{
    [SerializeField] float _emitMultiplier = 50;
    ParticleSystem _particleSystem;

    private void Awake()
    {
        _particleSystem = GetComponent<ParticleSystem>();
    }

    public void Setup(float platformScaleX, float platformScaleY)
    {
        var emission = _particleSystem.emission;
        emission.rateOverTime = platformScaleX * platformScaleY * _emitMultiplier;
    }

    public void OnParticleSystemStopped()
    {
        Destroy(this.transform.parent.gameObject);
    }
}
