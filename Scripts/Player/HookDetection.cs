using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HookDetection : Detector
{
    public bool hookDetected;
    private InteractableHook detectedHook;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Hook")
        {
            hookDetected = true;
            detectedHook = other.gameObject.GetComponent<InteractableHook>();
            _iconHolder = detectedHook.iconHolder;

            if (!detectedHook.showsIcon)
            {
                ShowIcon(true);
                detectedHook.showsIcon = true;
            }
        }        
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Hook")
        {
            hookDetected = false;
            if (_showsThisPlayersIcon)
            {
                ShowIcon(false);
                if (detectedHook) detectedHook.showsIcon = false;
            }
            hookDetected = false;
            _iconHolder = null;
            detectedHook = null;
        }
    }

    public InteractableHook GetHook()
    {
        return detectedHook;
    }
}
