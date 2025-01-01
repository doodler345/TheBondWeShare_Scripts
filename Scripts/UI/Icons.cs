using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Icons : MonoBehaviour
{
    [Header("Order: WASD, Arrow, Ps4, Xbox, Unknown")]

    [SerializeField] Sprite[] _moveableIcons = new Sprite[5];
    [SerializeField] Sprite[] _interactableIcons = new Sprite[5];

    public static Icons instance;

    private void Awake()
    {
        if (instance != null && instance != this)
            Destroy(this);
        else
            instance = this;
    }

    public Sprite GetSprite(string device, string type)
    {
        int index = 0;

        switch(device)
        {
            case "wasd":
                index = 0;
                break;
            case "arrows":
                index = 1;
                break;
            case "ps4":
                index = 2;
                break;
            case "xbox":
                index = 3;
                break;
            case "unknown":
                index = 4;
                break;
            default:
                index = -1;
                Debug.LogWarning("Couldnt find Device: " +  device);
                break;
        }

        switch(type)
        {
            case "moveable":
                return _moveableIcons[index];
            case "interactable":
                return _interactableIcons[index];
            default: 
                Debug.LogWarning("Couldnt find Type: " +  type);
                return null;
        }
    }
}
