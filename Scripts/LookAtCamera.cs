using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookAtCamera : MonoBehaviour
{
    Transform _camera;
    private void Start()
    {
        _camera = Camera.main.transform;
    }

    private void Update()
    {
        transform.forward = transform.position - _camera.transform.position;
    }

}
