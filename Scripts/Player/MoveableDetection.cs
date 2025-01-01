using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static PlayerMovement.DEVICE;

public class MoveableDetection : Detector
{
    public bool detected;
    private MoveableObject detectedObject;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Moveable")
        {
            detected = true;
            detectedObject = other.GetComponent<MoveableObject>();
            _iconHolder = detectedObject.iconHolder;
            if (!detectedObject.showsIcon && !detectedObject.getsMoved)
            {
                ShowIcon(true);
                detectedObject.showsIcon = true;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Moveable")
        {
            if (_showsThisPlayersIcon)
            {
                ShowIcon(false);
                if (detectedObject) detectedObject.showsIcon = false;
            }

            detected = false;
            detectedObject = null;
            _iconHolder = null;
        }
    }

    public MoveableObject GetDetectedObject()
    {
        return detectedObject;
    }

    public void Turn()
    {
        Vector3 pos = transform.localPosition;
        transform.localPosition = new Vector3(-pos.x, pos.y, 0); 
    }

}
