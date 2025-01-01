using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShatteredPlatform : MonoBehaviour
{
    public float destroyForce;
    public float timeTillDissappear = 2f;
    public float dissappearDuration = 1f;

    private void Awake()
    {
        Destroy(gameObject, timeTillDissappear + dissappearDuration);
    }
}
