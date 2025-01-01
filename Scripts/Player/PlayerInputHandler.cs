using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using static UnityEngine.InputSystem.InputAction;
using static Menu.MenuType;
using UnityEngine.InputSystem.DualShock;
using UnityEngine.InputSystem.XInput;
using UnityEngine.XR;

public class PlayerInputHandler : MonoBehaviour
{
    PlayerMovement _playerMovement;
    PlayerInputOld _playerInputOld;
    Menu _menu;

    PlayerInput _playerInput;

    public DEVICE Device;

    float _moveX;
    int _index;
    bool _ingame;

    private void Awake()
    {
        _playerInput = GetComponent<PlayerInput>();
        _index = _playerInput.playerIndex;
        BindPlayerToControlls();
    }

    public void BindPlayerToControlls()
    {
        _menu = FindObjectOfType<Menu>();
        var movers = FindObjectsOfType<PlayerMovement>();
        
        //ingame
        if (movers.Length > 0)
        {
            _ingame = true;

            if (_menu.ShowInGameMenu())
            {
                _playerInput.SwitchCurrentActionMap("UI");
            }
            else
            {
                _playerInput.SwitchCurrentActionMap("Player");
            }


            _playerMovement = movers.FirstOrDefault(m => m.playerID == _index);
            _playerMovement.SetDevice(Device);
            _playerMovement.transform.GetComponent<PlayerInputOld>().enabled = false;

            return;
        }


        //menu
        else
        {
            _playerInput.SwitchCurrentActionMap("UI");
        }
    }

    private void FixedUpdate()
    {

        if (!_ingame) return;

        _playerMovement.Move(_moveX);

    }

    #region UI

    public void RecieveAnyKey(CallbackContext context)
    {
        if (context.performed)
        {
            switch (_menu.menuType)
            {
                case GET_INPUTS:

                    string device;
                    _menu.InputRecieved(context.action.activeControl.device, out device);

                    switch (device)
                    {
                        case "Keyboard (WASD)":
                            Device = DEVICE.KEYBOARD_WASD;
                            break;
                        /*
                        case "Keyboard (ARROW KEYS)":
                            Device = DEVICE.KEYBOARD_ARROW;
                            break;
                        */
                        case "DualShock":
                            Device = DEVICE.PS4;
                            break;
                        case "XBOX":
                            Device = DEVICE.XBOX;
                            break;
                        case "Unknown Device":
                            Device = DEVICE.UNKNOWN;
                            break;
                    }
                    break;

                case INTRO:
                case OUTRO:
                    FindObjectOfType<IntroHandler>().StartSkipping(true);
                    break;

            }


        }

        if (context.canceled)
        {
            switch (_menu.menuType)
            {
                case INTRO:
                case OUTRO:
                    FindObjectOfType<IntroHandler>().StartSkipping(false);
                    break;
            }
        }
    }


    public void TogglePause(CallbackContext context)
    {
        if (context.started)
        {
            _menu.OnPauseButtonPress();
        }
    }

    public void Submit(CallbackContext context)
    {
        if (_menu.menuType == CREDITS)
        {
            if (context.performed)
            {
                _menu.StartSkip();
            }

            if (context.canceled)
            {
                _menu.CancleSkip();
            }
        }
    }

    public void UI_Navigate(CallbackContext context)
    {
        if (_menu.menuType == CREDITS)
        {
            if (context.performed)
            {
                var v = context.ReadValue<Vector2>();
                if (v.y > 0)
                {
                    _menu.ChangeScrollSpeed(false);
                }
                else
                {
                    _menu.ChangeScrollSpeed(true);
                }
            }
            else if (context.canceled)
            {
                _menu.ResetScrollSpeed();
            }
        }
    }    

    #endregion

    public void Move(CallbackContext context)
    {
        if (context.performed)
        {
            _moveX = context.ReadValue<float>();
        }
        if (context.canceled)
        {
            _moveX = 0;
        }
    }    
    public void LeftJoystick(CallbackContext context)
    {
        if (context.canceled)
        {
            _moveX = 0;
            _playerMovement.Down(false);
            _playerMovement.ToggleLadderClimb(false);
            return;
        }

        Vector2 joystickInput = context.ReadValue<Vector2>();

        _moveX = joystickInput.x;

        if (joystickInput.y < -0.5f)
        {
            _playerMovement.Down(true);
        }        
        else if (joystickInput.y > 0.5f)
        {
            _playerMovement.ToggleLadderClimb(true);
        }
    }

    public void Up(CallbackContext context) //Ladder and Jump
    {
        if(context.started)
        {
            _playerMovement.Up(true);
        }
        else if (context.canceled) 
        {
            _playerMovement.Up(false);
        }
    }  
    
    public void JumpJoystick(CallbackContext context)
    {
        if (context.started)
        {
            _playerMovement.Jump();
        }
    }    
    public void LadderUpJoystick(CallbackContext context)
    {
        if (context.started)
        {
            _playerMovement.Jump();
        }
    }



    public void Down(CallbackContext context)
    {
        if(context.started)
        {
            _playerMovement.Down(true);
        }
        else if (context.canceled) 
        {
            _playerMovement.Down(false);
        }
    }
    public void Interact(CallbackContext context)
    {
        if (context.started)
        {
            _playerMovement.Interact();
        }
    }

    public void PushPullObject(CallbackContext context)
    {
        if (context.started)
        {
            _playerMovement.PushPullObject(true);
        }
        else if (context.canceled)
        {
            _playerMovement.PushPullObject(false);

        }
    }
    public void BoundUnbound(CallbackContext context)
    {
        if (context.started)
        {
            _playerMovement.BoundUnbound();
        }
    }


    public enum DEVICE
    {
        KEYBOARD_WASD,
        KEYBOARD_ARROW,
        PS4,
        XBOX,
        UNKNOWN
    }
}
