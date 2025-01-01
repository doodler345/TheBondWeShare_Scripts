using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AvoidPlatformStuck : MonoBehaviour
{
    [SerializeField] Rigidbody _rb;
    
    [SerializeField] float _power = 0.3f;
    private void OnTriggerStay(Collider other)
    {
        if (other.tag == "Platform")
        {

            float yDistance = transform.position.y - other.transform.position.y;
            float direction = yDistance < 0 ? -1 : 1;

            Vector3 addMovement = new Vector3(0, direction * _power, 0);
            _rb.position += addMovement;
        }
    }
}
