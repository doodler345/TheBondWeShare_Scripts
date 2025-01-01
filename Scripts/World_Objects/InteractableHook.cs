using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class InteractableHook : MonoBehaviour
{
    public bool showsIcon;
    public Transform iconHolder;

    private void Start()
    {
        StageController.instance.worldSwitching += OnWorldSwitch;
    }
    private void OnDisable()
    {
        StageController.instance.worldSwitching -= OnWorldSwitch;
    }
    private void OnWorldSwitch()
    {
        showsIcon = false;
    }
}
