using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CastingPulsingStateMachine : CastingStateMachine
{
    //TODO: functionality... starting with initial idea for casting

    [SerializeField]
    private CastingPulsingStateMachineResources _resources;

    private InputMaster _controls;

    private GameObject _fishingTarget;
    private GameObject _fishingProgress;
    private GameObject _castingTarget;

    private float _minimumScale;
    private float _maxScale;

    //TODO: Replace this when get scriptable object reference working
    private float _initialCastingSpeed;

    [Tooltip("How fast the ring pulses, less than 1 for slower, greater than 1 to go faster.")]
    private float _pulseRate;

    [Tooltip("This is how long the player has to finish.")]
    //[SerializeField]
    private float _progressTimerBase = 10;
    private float _progressTimer = 0;

    private bool _hasMoved = false;

    private CastingComponentHolder _objectReference;

    private float _castingMaxDistance = 10;
    private float _castingGamepadRotationMultiplier = 100;
    private float _castingRotationDegreesClamp = 60;

    private Vector2 _prevLocation = Vector2.zero;

    public override void Initialize(Transform location, ToolComponentReferences references)
    {
        _controls = new InputMaster();
        _controls.Fishing.Enable();


        _resources = references.CastingComponentList.PulsingResources;
        //   GameObject g = GameObject.Find("CastingPulsing");
        //_helper = Instantiate(resources.castingPrefab, location.transform.position, location.transform.rotation, this.transform);
        // _helper = Instantiate(g, location.transform.position, location.transform.rotation, this.transform);
        Prefab = Instantiate(_resources.CastingPulsingPrefab, location.transform.position, location.transform.rotation);
        _objectReference = Prefab.GetComponent<CastingComponentHolder>();

        _fishingTarget = _objectReference.FishingTarget;
        _fishingProgress = _objectReference.FishingProgress;
        _castingTarget = _objectReference.CastingTarget;


        _fishingProgress.SetActive(false);
        _castingTarget.SetActive(false);

        SetValues();
        _fishingState = 1;
    }
    private void OnDestroy()
    {
        _controls.Fishing.Disable();

        Destroy(_objectReference.gameObject);
    }


    public override void SetValues()
    {
        _initialCastingSpeed = _resources.InitialCastingSpeed;
        _progressTimerBase = _resources.TimeToFinishCast;
        _minimumScale = _resources.MinimumScale;
        _maxScale = _resources.MaxScale;
        _pulseRate = _resources.PulseRate;


        _castingMaxDistance = _resources.CastingMaxDistance;
        _castingGamepadRotationMultiplier = _resources.JoystickRotationMultiplier;
        _castingRotationDegreesClamp = _resources.MaxRotation;
    }

    public override bool Execute()
    {
        switch (_fishingState)
        {
            //going out / targeting
            case 1:
                //if "holding down the fishing button"
                if (_controls.Fishing.FishingGo.ReadValue<float>() > 0)
                {
                    _hasMoved = true;

                    Vector2 testLocation = _controls.Fishing.AimingControl.ReadValue<Vector2>();

                    //if using mouse/keyboard (mouse delta)
                    if (InputControllerHandler.UsingMK)
                    {
                        //track cumulative total for how far mouse has moved from it's start point
                        _prevLocation += testLocation;
                        // print("delta cumulative: " + _prevLocation);

                        testLocation = _prevLocation;

                        //scale down for mouse movement
                        testLocation.y = testLocation.y * .2f;
                    }
                    else
                    {
                        //modifications for gamepad
                        testLocation.x *= _castingGamepadRotationMultiplier;
                        testLocation.y = testLocation.y * _castingMaxDistance;
                    }

                    //apply modifications


                    //make sure you can't cast backwards in any way
                    if (testLocation.y > 0) testLocation.y = 0;

                    testLocation.x = Mathf.Clamp(testLocation.x, -_castingRotationDegreesClamp, _castingRotationDegreesClamp);

                    //invert input direction
                    testLocation *= -1;

                    //make sure we can't exceed max distance
                    testLocation.y = Mathf.Clamp(testLocation.y, 0, _castingMaxDistance);

                    //apply rotation
                    Prefab.transform.eulerAngles = new Vector3(0, testLocation.x, 0);

                    //apply distance
                    _fishingTarget.transform.localPosition = new Vector3(0, 0, testLocation.y);
                }
                else
                if (_hasMoved && _controls.Fishing.FishingGo.ReadValue<float>() == 0)
                {
                    //Debug.Log("Stopping");
                    //_progressTimer = 0;
                    _fishingProgress.SetActive(true);
                    _fishingProgress.transform.position = _fishingTarget.transform.position;
                    _castingTarget.SetActive(true);
                    _fishingTarget.SetActive(true);
                    _castingTarget.transform.position = _fishingTarget.transform.position;
                    _fishingTarget.transform.localScale = Vector3.one * 2;

                    _progressTimer = -.5f;

                    _fishingState = 2;
                }
                return true;

            //Scaling the orb
            case 2:
                //float size = Mathf.Sin(Time.deltaTime) * _oscilationMultiplier;
                float size = Mathf.PingPong(Time.time * _pulseRate, _maxScale) + _minimumScale;
                if (_progressTimer > 0)
                {
                    _castingTarget.transform.localScale = new Vector3(size, .05f, size);


                    if (_progressTimer >= _progressTimerBase)
                    {
                        _fishingState = 3;
                    }
                }
                //counting time              
                _progressTimer += Time.deltaTime;

                if (_controls.Fishing.FishingGo.ReadValue<float>() > 0)
                {
                    _fishingState = 3;
                }
                return true;
            case 3:
                print("DONE WITH CASTING STEP!!");
                return false;
        }
        return false;
    }
}
