using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputControllerHandler : MonoBehaviour
{
    public InputMaster controls;

    public static bool UsingMK;

    private void Awake()
    {
        controls = new InputMaster();
        controls.Player.Fish.performed += ctx => FishingStart();

        controls.Player.Camera.performed += ctx => CheckPlayerInputMethod(ctx);
        controls.Player.Movement.performed += ctx => CheckPlayerInputMethod(ctx);
        try
        {
            FishingEventsController.current.onStartFishing += FishingStartControls;

            controls.Fishing.CancelFishing.performed += ctx => FishingStop();
            FishingEventsController.current.onStopFishing += FishingStopControls;
        }
        catch
        {
            Debug.LogError("Error Referencing Fishing Events Controller, " +
                "you may need to check script execution order to ensure it is initialized first!");
        }
    }

    private void CheckPlayerInputMethod(InputAction.CallbackContext ctx)
    {
        if (ctx.control.device is Keyboard) UsingMK = true;
        else if (ctx.control.device is Mouse) UsingMK = true;
        else UsingMK = false;
        //print("mouse and keyboard status: " + UsingMK);
    }

    //when enable, make sure the controls are enabled
    private void OnEnable()
    {
        controls.Player.Movement.Enable();
        controls.Player.Camera.Enable();
        controls.Player.Fish.Enable();
    }

    //when disabled, make sure all fishing controls are disabled
    private void OnDisable()
    {
        controls.Fishing.Disable();
        controls.Player.Disable();
    }
    private void FishingStart()
    {
        FishingEventsController.current.StartFishing();
        FishingStartControls();
    }
    private void FishingStartControls()
    {
        print("Swapping to fishing mode");

        controls.Player.Fish.Disable();
        controls.Fishing.CancelFishing.Enable();
    }

    private void FishingStop()
    {
        FishingEventsController.current.StopFishing();
        FishingStopControls();
    }
    private void FishingStopControls()
    {
        print("Exiting fishing mode");

        controls.Player.Fish.Enable();
        controls.Fishing.Disable();
    }
}
