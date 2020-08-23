using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;


public class PlayerControlls : MonoBehaviour
{
    private InputMaster controls;

    public PlayerInput playerInput;

    [SerializeField]
    private float _movementSpeed = 5;
    // private float _mouseSensativity = 10;

    //private float _controllerSensitivity = 100;

    //public float joystickDeadzone = .1f;

    float _cameraY = 0;
    float _cameraX = 0;

    public float cameraClamp = 45;
    public bool invert = true;

    [SerializeField]
    private GameObject _cameraTarget;
    [SerializeField]
    private GameObject _cameraHolder;

    private bool _playerControlsActive = true;

    [SerializeField]
    private Vector3 _cameraTurn = Vector3.zero;

    private void Awake()
    {
        controls = new InputMaster();

        FishingEventsController.current.onStartFishing += DisablePlayerMovement;
        FishingEventsController.current.onStopFishing += EnablePlayerMovement;
    }

    private void OnEnable()
    {
        controls.Player.Movement.Enable();
        controls.Player.Camera.Enable();
    }

    private void OnDisable()
    {
        controls.Player.Movement.Disable();
        controls.Player.Camera.Disable();
    }
    public void OnCamera(InputValue value)
    {
        Vector2 cameraDelta = value.Get<Vector2>();

        _cameraTurn = new Vector3(cameraDelta.x, cameraDelta.y, 0);
        //Debug.Log(_cameraTurn);
    }


    void Update()
    {
        Vector2 playerMovement = controls.Player.Movement.ReadValue<Vector2>();
        //Debug.Log(playerMovement);
        Vector3 directionToMove = Vector3.zero;

        if (_playerControlsActive)
        {
            //going forward
            if (playerMovement.y > 0)
            {
                transform.rotation = _cameraHolder.transform.localRotation;
                transform.rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0);
                transform.position += transform.forward * _movementSpeed * playerMovement.y * Time.deltaTime;
            }
            else
            //going back
            if (playerMovement.y < 0)
            {
                transform.position += transform.forward * _movementSpeed * playerMovement.y * Time.deltaTime;
            }
            //going left
            if (playerMovement.x < 0)
            {
                transform.position += _cameraHolder.transform.right * _movementSpeed * playerMovement.x * Time.deltaTime;
            }
            else
            //going right
            if (playerMovement.x > 0)
            {
                transform.position += _cameraHolder.transform.right * _movementSpeed * playerMovement.x * Time.deltaTime;
            }
            #region Old Input Movement
            //if (Input.GetKey(KeyCode.W))
            //{
            //    transform.rotation = cameraFollower.transform.localRotation;
            //    transform.rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0);
            //    //transform.rotation = Quaternion.Euler(0, cameraFollower.transform.rotation.y, 0);
            //    transform.position += transform.forward * speed * Time.deltaTime;
            //}
            //if (Input.GetKey(KeyCode.S))
            //{
            //    transform.position -= transform.forward * speed * Time.deltaTime;
            //}
            //if (Input.GetKey(KeyCode.A))
            //{
            //    transform.position -= cameraFollower.transform.right * speed * Time.deltaTime;
            //    //transform.rotation *= Quaternion.Euler(-Vector3.up);
            //}
            //if (Input.GetKey(KeyCode.D))
            //{
            //    //transform.rotation *= Quaternion.Euler(Vector3.up);
            //    transform.position += cameraFollower.transform.right * speed * Time.deltaTime;
            //}
            #endregion
        }
        // do camera rotation

        Vector2 cameraMovement = controls.Player.Camera.ReadValue<Vector2>();

        //        if(InputControlScheme.)


        // Debug.Log(playerInput.currentControlScheme);

        float sensativity = 1;
        // if(playerInput.currentControlScheme == "Keyboard&Mouse")
        // {
        //     print("Using keyboard&mouse");
        //     sensativity = _mouseSensativity;
        // } else if(playerInput.currentControlScheme == "Controller")
        // {
        //     print("Using controller");
        //     sensativity = _controllerSensitivity;
        // }

        //if (Gamepad.current.IsActuated()) sensativity = _controllerSensitivity;
        //else sensativity = _mouseSensativity;

        //if (Gamepad.current.IsActuated()) print("Using joystick");

        _cameraY += sensativity * Time.deltaTime * cameraMovement.y;
        _cameraX += sensativity * Time.deltaTime * cameraMovement.x;
        //cameraY += sensitivity * Time.deltaTime * _cameraTurn.x;
        //cameraX += sensitivity * Time.deltaTime * _cameraTurn.y;

        // if (invert)
        // {
        //     cameraY += sensitivity * Time.deltaTime * Input.GetAxis("Mouse Y");
        // }
        // else
        // {
        //     cameraY += sensitivity * Time.deltaTime * Input.GetAxis("Mouse Y");
        // }
        // cameraX += sensitivity * Time.deltaTime * Input.GetAxis("Mouse X");

        


        if (_playerControlsActive)
        {
            if (_cameraY > cameraClamp) _cameraY = cameraClamp;
            if (_cameraY < -cameraClamp) _cameraY = -cameraClamp;



            _cameraTarget.transform.eulerAngles = new Vector3(_cameraY, _cameraX, 0);

            _cameraHolder.transform.position = _cameraTarget.transform.position;
            _cameraHolder.transform.rotation = _cameraTarget.transform.rotation;
        }
    }
    private void DisablePlayerMovement()
    {
        _playerControlsActive = false;
        Debug.Log("Player controls: " + _playerControlsActive);
    }
    private void EnablePlayerMovement()
    {
        _playerControlsActive = true;
        Debug.Log("Player controls: " + _playerControlsActive);
    }
}
