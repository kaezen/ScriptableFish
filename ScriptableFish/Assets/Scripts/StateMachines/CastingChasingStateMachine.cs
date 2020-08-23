using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class CastingChasingStateMachine : CastingStateMachine
{
    private InputMaster _controls;


    private CastingChasingStateMachineResources _resources;
    private ToolComponentReferences _resourceList;


    private GameObject _fishingTarget;
    private GameObject _fishingProgress;
    private GameObject _castingArc;
    private GameObject _castingTarget;


    private float _initialCastingSpeed = 5;

    [Tooltip("This is how long it takes for the progress circle to catch up to the target")]
    private float _timeToReachTargetBase;
    private float _timeToReachTargetTimer = 0;

    [Tooltip("Minimum amount of force applied")]
    private float _pullForceBase;

    [Tooltip("This is the max amount of force that can pull away (+ or - this value)" +
        " Recommend having this less than player force.")]
    private float _pullForceRange;
    private float _playerForce;

    [Tooltip("How long (in seconds) between when the fighting force triggers")]
    private float _timeBetweenPullsBase;
    private float _timeBetweenPullsTimer = 0;

    [Tooltip("Varience (+ or - this number) from the base time between pulls")]
    private float _timeBetweenPullsOffset;


    [Tooltip("How long the fighting force will pull for.")]
    private float _pullForceDurationTimerBase;
    private float _pullForceDurationTimer = 0;

    [Tooltip("Varience (+ or - this number) from the base duration of a pull")]
    private float _pullForceDurationOffset;

    private Vector3 _pullForce = Vector3.zero;

    private Vector3 _startPos;

    private bool _hasMoved = false;

    private Vector2 _fishingMovementForce;

    private float _castingMaxDistance = 10;
    private float _castingGamepadRotationMultiplier = 100;
    private float _castingRotationDegreesClamp = 60;

    private Vector2 _prevLocation = Vector2.zero;

    public override void Initialize(Transform location, ToolComponentReferences references)
    {
        _controls = new InputMaster();
        _controls.Fishing.Enable();

        _resources = references.CastingComponentList.ChasingResources;

        Prefab = Instantiate(_resources.CastingChasingPrefab, location.transform.position, location.transform.rotation);
        CastingComponentHolder c = Prefab.GetComponent<CastingComponentHolder>();
        _fishingTarget = c.FishingTarget;
        _fishingProgress = c.FishingProgress;
        _castingArc = c.CastingArc;
        _castingTarget = c.CastingTarget;

        _castingTarget.SetActive(false);
        _castingArc.SetActive(false);
        _fishingProgress.SetActive(false);

        SetValues();

        _fishingState = 1;

        _controls.Fishing.FishingMovement.performed += ctx => _fishingMovementForce = ctx.ReadValue<Vector2>();
        _controls.Fishing.FishingMovement.canceled += ctx => _fishingMovementForce = Vector2.zero;
    }

    private void OnDisable()
    {
        _controls.Fishing.Disable();
    }

    public override void SetValues()
    {
        _initialCastingSpeed = _resources.InitialCastingSpeed;
        _timeToReachTargetBase = _resources.TimeToReachTarget;
        _pullForceBase = _resources.PullForceBase;
        _pullForceRange = _resources.PullForceRange;
        _playerForce = _resources.PlayerForce;
        _timeBetweenPullsBase = _resources.TimeBetweenPulls;
        _timeBetweenPullsOffset = _resources.TimeBetweenPullsRange;
        _pullForceDurationTimerBase = _resources.TimeBetweenPulls;
        _pullForceDurationOffset = _resources.PullDurationRange;

        _castingMaxDistance = _resources.CastingMaxDistance;
        _castingGamepadRotationMultiplier = _resources.JoystickRotationMultiplier;
        _castingRotationDegreesClamp = _resources.MaxRotation;

        _castingTarget.transform.localScale = _resources.CastingTargetSize * Vector3.one;
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
                //if fishing button is released
               if (_hasMoved && _controls.Fishing.FishingGo.ReadValue<float>() == 0)
                {
                    _castingTarget.SetActive(true);
                    if (_resources.ShowCastingArc) _castingArc.SetActive(true);
                    _fishingProgress.SetActive(true);

                    _timeBetweenPullsTimer = _timeBetweenPullsBase;
                    _pullForce = Vector3.zero;
                    _startPos = _fishingProgress.transform.position;
                    _fishingState = 2;
                    _timeToReachTargetTimer = -.5f;
                }
                return true;

            //chasing the target
            case 2:
                //moving the target sphere           
                if (_timeToReachTargetTimer > 0)
                {

                    _timeBetweenPullsTimer -= Time.deltaTime;
                    if (_timeBetweenPullsTimer < 0)
                    {
                        if (_pullForce == Vector3.zero)
                        {
                            float r = 0;
                            r = Random.Range(-1, 1);
                            float x = (Random.Range(0, _pullForceRange) + _pullForceBase) * r;
                            r = Random.Range(-1, 1);
                            float z = (Random.Range(0, _pullForceRange) + _pullForceBase) * r;

                            _pullForce = new Vector3(x, 0, z);
                        }
                        _pullForceDurationTimer -= Time.deltaTime;
                        _timeBetweenPullsTimer = 0;
                        //_castingTarget.transform.position += _fightingForce * Time.deltaTime;

                        if (_pullForceDurationTimer <= 0)
                        {
                            _timeBetweenPullsTimer = _timeBetweenPullsBase + Random.Range(-_timeBetweenPullsOffset, _timeBetweenPullsOffset);
                            _pullForceDurationTimer = _pullForceDurationTimerBase + Random.Range(-_pullForceDurationOffset, _pullForceDurationOffset);
                            _pullForce = Vector3.zero;
                        }
                    }

                    //apply player controls
                    Vector3 playerInputForce = new Vector3(_fishingMovementForce.x, 0, _fishingMovementForce.y);
                    // if (Input.GetKey(KeyCode.W)) playerInputForce += _castingTarget.transform.forward;
                    // if (Input.GetKey(KeyCode.A)) playerInputForce += -_castingTarget.transform.right;
                    // if (Input.GetKey(KeyCode.S)) playerInputForce += -_castingTarget.transform.forward;
                    // if (Input.GetKey(KeyCode.D)) playerInputForce += _castingTarget.transform.right;

                    playerInputForce *= _playerForce;
                    playerInputForce += _pullForce;

                    _castingTarget.transform.position += playerInputForce * Time.deltaTime;

                    //moving the progress circle               
                    _fishingProgress.transform.position = Vector3.Lerp(_startPos, _fishingTarget.transform.position, _timeToReachTargetTimer / _timeToReachTargetBase);

                    //scaling the arc
                    float arcSize = _timeToReachTargetTimer / _timeToReachTargetBase * _fishingTarget.transform.localPosition.z;
                    _castingArc.transform.localScale = new Vector3(1, arcSize, arcSize);

                }
                _timeToReachTargetTimer += Time.deltaTime;
                if (_fishingProgress.transform.localPosition.z >= _fishingTarget.transform.localPosition.z)
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
