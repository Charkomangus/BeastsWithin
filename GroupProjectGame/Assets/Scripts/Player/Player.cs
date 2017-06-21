﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Remoting.Messaging;
using Assets.Scripts.MainManagers;
using Assets.Scripts.Objects;
using Assets.Scripts.Tiles;
using Assets.Scripts.Utility;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Assets.Scripts.Player
{
    enum PlayerMoveState
    {
        Up, Down, Right, Left, Idle, UpPush, DownPush, RightPush, LeftPush
    }

    public class Player : MonoBehaviour
    {

        private Animator _playerAnimator;

        [Header("Tiles")]
        [SerializeField]private Tile _parentTile;
        [SerializeField]private Tile _latestTile;


        [Header("Movement")]
        [SerializeField]private PlayerMoveState _playerMoveState;
        [SerializeField]private float _moveSpeed;
        [SerializeField]private float _slidingSpeed;
        [SerializeField]private float _normalSpeed;

        [Header("Status")]
        [SerializeField]private bool _sliding;
        [SerializeField]private bool _moving;


        private bool initialized;
        private int direction;
   
    

        // Use this for initialization
        void Start ()
        {
          
        }

        public void InitializePlayer()
        {
            _playerAnimator = GetComponent<Animator>();
            _playerMoveState = PlayerMoveState.Idle;

            //Start at entry tile - if it's null pick one at random.
            _parentTile = GameManager.Instance.ReturnLevelEntry() ?? FindObjectOfType<Tile>();
            transform.position = new Vector3(_parentTile.transform.position.x, transform.position.y, _parentTile.transform.position.z);
            _normalSpeed = _moveSpeed;
            _slidingSpeed = _moveSpeed * 2;
           
            initialized = true;
        }
	
        // Update is called once per frame
        void Update()
        {
            if (_parentTile == null || !initialized) return;

            Determine();
            if (HasReachedTile())
            {
                SmoothMove(transform.position, _parentTile.transform.position, 3 * _moveSpeed);
                PlayerInput();
                DetermineMoveState();             
            }
            else
            {
                SmoothMove(transform.position, _parentTile.transform.position, _moveSpeed);
            }

            //DEBUG - TEMP
            if (_latestTile != null)
            {
                _latestTile.GetComponentInChildren<SpriteRenderer>().color = new Color(0.5f, 0.5f, 0.5f);
            }
          
        }


        private void Determine()
        {
            switch (_parentTile.ReturnType())
            {
                case TileType.Oil:
                    _sliding = true;
                    _moveSpeed = _slidingSpeed;
                    break;
                case TileType.Ice:
                    _sliding = true;
                    _moveSpeed = _slidingSpeed;
                    break;
                case TileType.Normal:
                    _sliding = false;
                    _moveSpeed = _normalSpeed;
                    break;
                case TileType.Fire:
                    _parentTile = GameManager.Instance.ReturnLevelEntry();
                    transform.position = _parentTile.transform.position;
                    break;
                case TileType.IceCracks:
                    _parentTile = GameManager.Instance.ReturnLevelEntry();
                    transform.position = _parentTile.transform.position;
                    break;

                case TileType.Blocked:
                    break;
                case TileType.Wall:
                    break;
                case TileType.Door:
                    break;
                case TileType.GreenConveyorBelt:
                    break;
                case TileType.RedConveyorBelt:
                    break;
                case TileType.BlueConveyorBelt:
                    break;
                case TileType.Null:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void SmoothMove(Vector3 startPosition, Vector3 endPosition, float speed)
        { 
            transform.position = Vector3.Lerp(startPosition, endPosition, speed * Time.deltaTime);
        }
      
        //Check if player has reached parent tile
        private bool HasReachedTile()
        {
            return Math.Abs(transform.position.x - _parentTile.transform.position.x) < 0.6f && Math.Abs(transform.position.z - _parentTile.transform.position.z) < 0.6f;
        }

        private void Interact()
        {
        
            TileObject Object = _latestTile.ReturnObject();
            Debug.Log("Interacting with " + Object);

            switch (Object)
            {
                case TileObject.Box:
                    switch (direction) 
                    {
                        case 0:
                            _latestTile.GetComponentInChildren<Box>().SetParentTile(_latestTile.ReturnNorth(), direction);
                            break;
                        case 1:
                            _latestTile.GetComponentInChildren<Box>().SetParentTile(_latestTile.ReturnSouth(), direction);
                            break;
                        case 2:
                            _latestTile.GetComponentInChildren<Box>().SetParentTile(_latestTile.ReturnEast(), direction);
                            break;
                        case 3:
                            _latestTile.GetComponentInChildren<Box>().SetParentTile(_latestTile.ReturnWest(), direction);
                            break;
                    }
                    break;

                case TileObject.SlidingBox:
                    switch (direction)
                    {
                        case 0:
                            _latestTile.GetComponentInChildren<SlidingBox>().SetParentTile(_latestTile.ReturnNorth(), direction);
                            break;
                        case 1:
                            _latestTile.GetComponentInChildren<SlidingBox>().SetParentTile(_latestTile.ReturnSouth(), direction);
                            break;
                        case 2:
                            _latestTile.GetComponentInChildren<SlidingBox>().SetParentTile(_latestTile.ReturnEast(), direction);
                            break;
                        case 3:
                            _latestTile.GetComponentInChildren<SlidingBox>().SetParentTile(_latestTile.ReturnWest(), direction);
                            break;
                    }
                    break;
                    break;
                case TileObject.GreenSwitch:
                    break;
                case TileObject.RedSwitch:
                    break;
                case TileObject.BlueSwitch:
                    break;
                case TileObject.Empty:
                    break;
                case TileObject.Sign:
                    GameManager.Instance.UiManager.Open(_latestTile.GetComponentInChildren<Sign>().ReturnSignText());
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        //Determine if this tile can be moved into
        private bool IsValidTile(Tile tile)
        {

            if (tile == null)
                return false;

            if (tile.IsBlocked())
            {
              
                return false;
            }
          
            return true;
        }

        private void Move(Tile destination)
        {
            if (IsValidTile(destination))
            {
                _parentTile = destination;
                _moving = true;
                switch (direction) 
                {
                    case 0:
                        _latestTile = destination.ReturnNorth();
                        break;
                    case 1:
                        _latestTile = destination.ReturnSouth();
                        break;
                    case 2:
                        _latestTile = destination.ReturnEast();
                        break;
                    case 3:
                        _latestTile = destination.ReturnWest();
                        break;
                }
              
            }
            else
            {
                _latestTile = destination;
                _moving = false;
            }
        }

        private void DetermineAnimationStatus(string trigger)
        {
            if (_sliding && _moving)
            {
               return;
            }
            _playerAnimator.SetTrigger(trigger);
           
        }


        //Determing what happens depending on the player input and position 
        private void DetermineMoveState()
        {
            switch (_playerMoveState)
            {
                case PlayerMoveState.Up:

                    DetermineAnimationStatus("Up");
                    direction = 0;
                    Move(_parentTile.ReturnNorth());
                    break;

                case PlayerMoveState.Down:
                    DetermineAnimationStatus("Down");
                    direction = 1;
                    Move(_parentTile.ReturnSouth());
                    break;

                case PlayerMoveState.Right:
                    DetermineAnimationStatus("Right");
                    direction = 2;
                    Move(_parentTile.ReturnEast());
                    break;

                case PlayerMoveState.Left:
                    DetermineAnimationStatus("Left");
                    direction = 3;
                    Move(_parentTile.ReturnWest());
                    break;

                case PlayerMoveState.Idle:
                    _playerAnimator.SetTrigger("Idle");
                    _moving = false;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
         
        }

        void PlayerInput()
        {
            //A player cannot act if they are currently sliding
            if (_sliding && _moving) return;

            //
            if (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Keypad0))
            {
                Interact();
            }

            //If none of the movement keys are pressed the player is assumed to be idle.
            if (!Input.GetKey(KeyCode.D) && !Input.GetKey(KeyCode.A) && !Input.GetKey(KeyCode.W) &&
                !Input.GetKey(KeyCode.S) && !Input.GetKey(KeyCode.UpArrow) &&
                !Input.GetKey(KeyCode.RightArrow) && !Input.GetKey(KeyCode.LeftArrow) &&
                !Input.GetKey(KeyCode.DownArrow) && HasReachedTile())
            {
                _playerMoveState = PlayerMoveState.Idle;
                return;
            }


            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
            {
             
                _playerMoveState = PlayerMoveState.Up;
             

            }

            else if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
            {
            
                _playerMoveState = PlayerMoveState.Down;
               
            }

            else if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
            {

               
                _playerMoveState = PlayerMoveState.Left;
              
            }

            else if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
            {
                
                _playerMoveState = PlayerMoveState.Right;
               
            }
        }


        public void SetParentTile(Tile tile)
        {
            _parentTile = tile;
          
        }

        public bool IsMoving()
        {
            return _moving;

        }
    }

}
