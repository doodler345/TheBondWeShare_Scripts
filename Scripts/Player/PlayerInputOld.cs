using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Obi;
using Unity.VisualScripting;
using UnityEngine.SceneManagement;

public class PlayerInputOld : MonoBehaviour
{
    int playerID;
    PlayerMovement _playerMovement;
    bool _isPause;

    private void Awake()
    {
        _playerMovement = GetComponent<PlayerMovement>();
        UpdatePlayerID();
    }

    private void Start()
    {
        StageController.instance.gamePaused += Pause;
    }

    public void UpdatePlayerID()
    {
        playerID = _playerMovement.playerID;
    }

    public void Pause(bool isPause)
    {
        _isPause = isPause;
    }

    private void Update()
    {
        if (_isPause) return;

        switch (playerID)
        {
               /*

                if (Input.GetKeyDown("e"))
                {
                    _playerMovement.ToggleLever();
                }

                if (Input.GetKey("r"))
                {
                    _playerMovement.Crane(true);
                }
                if (Input.GetKey("t"))
                {
                    _playerMovement.Crane(false);
                }

                if (Input.GetKeyDown("v"))
                {
                    _playerMovement.PushPullObject(true);
                }                
                else if (Input.GetKeyUp("v"))
                {
                    _playerMovement.PushPullObject(false);
                }

                if(Input.GetKeyDown(KeyCode.LeftControl))
                {
                    _playerMovement.BoundUnbound();
                }

                break;
                */

            case 1:

                

                

                if (Input.GetKeyDown(KeyCode.UpArrow))
                {
                    _playerMovement.Up(true);
                }
                else if (Input.GetKeyUp(KeyCode.UpArrow))
                {
                    _playerMovement.Up(false);
                }
                else if (Input.GetKeyDown(KeyCode.DownArrow))
                {
                    _playerMovement.Down(true);
                }
                else if (Input.GetKeyUp(KeyCode.DownArrow))
                {
                    _playerMovement.Down(false);
                }

                if(Input.GetKeyDown("l"))
                {
                    _playerMovement.Interact();
                }                
                
                if (Input.GetKeyDown("m"))
                {
                    _playerMovement.PushPullObject(true);
                }
                else if (Input.GetKeyUp("m"))
                {
                    _playerMovement.PushPullObject(false);
                }

                if (Input.GetKeyDown(KeyCode.RightControl))
                {
                    _playerMovement.BoundUnbound();
                }

                break;
            default: 
                break;
        }
    }

    private void FixedUpdate()
    {
        switch (playerID)
        {
            /*
        case 0:

            if (Input.GetKey("a"))
            {
                _playerMovement.Move(-1);
            }
            else if (Input.GetKey("d"))
            {
                _playerMovement.Move(1);
            }

            else 
                _playerMovement.Move(0);

            if (Input.GetKeyDown("w"))
            {
                _playerMovement.Up(true);
            }               
            else if (Input.GetKeyUp("w"))
            {
                _playerMovement.Up(false);
            }
            if (Input.GetKeyDown("s"))
            {
                _playerMovement.Down(true);
            }
            else if (Input.GetKeyUp("s"))
            {
                _playerMovement.Down(false);
            }
            */

            case 1:


                if (Input.GetKey(KeyCode.LeftArrow))
                {
                    _playerMovement.Move(-1);
                }
                else if (Input.GetKey(KeyCode.RightArrow))
                {
                    _playerMovement.Move(1);
                }
                else
                    _playerMovement.Move(0);

                break;

            default:
                break;
        }

    }

    private void OnDestroy()
    {
        StageController.instance.gamePaused -= Pause;
    }

}
