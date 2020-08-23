using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class SetterClass : MonoBehaviour
{
    InputMaster controls;
    public bool isOverlapping = false;

    private void Awake()
    {
        controls = new InputMaster();
        controls.Player.Interact.performed += ctx => Interact();
    }

    private void OnEnable()
    {
        controls.Player.Interact.Enable();
    }
    private void OnDisable()
    {
        controls.Player.Interact.Disable();
    }

    //TODO: Create variable to hold the fishEnum.type we want to set
    private void OnTriggerEnter(Collider other)
    {
        print("I hit a thing!:" + other.name);
        isOverlapping = true;
    }
    private void OnTriggerExit(Collider other)
    {
        isOverlapping = false;
    }

    private void Interact()
    {
        if (isOverlapping)
        {
            print("trigger: setting something");
            TriggerEvent();
        }
    }
    void Update()
    {
       // if (Input.GetKeyDown(KeyCode.Space) && isOverlapping)
       // {
       //     // print("Setting water: " + bodyOfWaterType);
       //     TriggerEvent();
       // }
    }

    //invoke the event associated with the fishEnum type
    public abstract void TriggerEvent();
}
