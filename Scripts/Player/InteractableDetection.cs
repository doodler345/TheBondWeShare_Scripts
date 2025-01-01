using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractableDetection : Detector
{
    private int detectedObjectID;

    public bool ladderDetected, leverDetected, doorDetected;
    private InteractableLever detectedLever;
    private InteractableDoor detectedDoor;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Ladder")
        {
            ladderDetected = true;
            //detectedObject = other.GetComponent<MoveableObject>();
        }
        else if (other.gameObject.tag == "Lever")
        {
            leverDetected = true;
            detectedLever = other.gameObject.GetComponent<InteractableLever>();
            _iconHolder = detectedLever.iconHolder;
            if (!detectedLever.showsIcon)
            {
                ShowIcon(true);
                detectedLever.showsIcon = true;
            }
        }
        else if (other.gameObject.tag == "Door")
        {
            ReliableOnTriggerExit.NotifyTriggerEnter(other, gameObject, OnTriggerExit);

            doorDetected = true;
            detectedDoor = other.gameObject.GetComponent<InteractableDoor>();
            _iconHolder = detectedDoor.iconHolder;
            if (!detectedDoor.showsIcon)
            {
                ShowIcon(true);
                detectedDoor.showsIcon = true;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Ladder")
        {
            ladderDetected = false;
        }        
        else if (other.gameObject.tag == "Lever")
        {
            if (_showsThisPlayersIcon)
            {
                ShowIcon(false);
                if (detectedLever)
                {
                    detectedLever.showsIcon = false;
                }
            }
            leverDetected = false;
            detectedLever = null;
            _iconHolder = null;
        }        
        else if (other.gameObject.tag == "Door")
        {
            ReliableOnTriggerExit.NotifyTriggerExit(other, gameObject);
            if (_showsThisPlayersIcon)
            {
                ShowIcon(false);
                if (detectedDoor)
                {
                    detectedDoor.showsIcon = false;
                }
            }
            doorDetected = false;
            detectedDoor = null;
            _iconHolder = null;
        }
    }



    public InteractableLever GetLever()
    {
        return detectedLever;
    }
    public InteractableDoor GetDoor() 
    { 
        return detectedDoor; 
    }


}
