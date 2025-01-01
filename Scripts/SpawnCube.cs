using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnCube : MonoBehaviour
{

    Vector3 initPos;
    // Start is called before the first frame update
    void Start()
    {
        initPos = transform.position;
        InvokeRepeating("Respawn", 0, 5);
    }

    void Respawn()
    {
        GetComponent<Rigidbody>().velocity = Vector3.zero;
        transform.position = initPos;
    }
}
