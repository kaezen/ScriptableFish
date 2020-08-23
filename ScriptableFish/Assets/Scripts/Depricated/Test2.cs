using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Test2 : MonoBehaviour
{
    public InputMaster controls;
    Mouse ms;
    Keyboard kb;
    private void Start()
    {
        controls = new InputMaster();
        controls.Player.Interact.performed += ctx => TEST(ctx);
        controls.Player.Interact.Enable();
    }

    void TEST(InputAction.CallbackContext ctx)
    {
        print(ctx.control.device.device);
        if (ctx.control.device.device is Keyboard) print("Using keyboard");
        if (ctx.control.device.device is Gamepad) print("Using Gamepad");


    }
    private void Update()
    {
        InputDevice id = new InputDevice();
        print(controls.controlSchemes);
        print(controls.ControllerScheme);        
        //Debug.Log(id.device) ;
    }

    //used with 'broadcast messages'
    public void OnInteract(InputValue value)
    {
        print("oninteract: " +value);        
    }
    public void OnCamera(InputValue value)
    {
        //print("onCamera: " + value);
    }
    public void OnFish(InputValue value)
    {
        print("Test onfish");
    }
}
