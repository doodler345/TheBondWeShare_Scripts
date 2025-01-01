using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShatteredParts : MonoBehaviour
{
    [SerializeField] ShatteredPlatform _shatteredPlatform;
    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        Vector3 dir = _shatteredPlatform.transform.position - this.transform.position;
        rb.AddForce(dir * _shatteredPlatform.destroyForce, ForceMode.Impulse);

        Invoke(nameof(Disappear), _shatteredPlatform.timeTillDissappear);
    }

    private void Disappear()
    {
        transform.DOScale(Vector3.zero, _shatteredPlatform.dissappearDuration - 0.1f);
    }
}
