using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Util;

public class JointController : MonoBehaviour
{
    /// <summary>
    /// Sets the location for the controller to base its targets off of
    /// </summary>
    public float currentPosition; 
    /// <summary>
    /// The joint for the controller to affect controll over
    /// </summary>
    public ConfigurableJoint joint; 
    /// <summary>
    /// Whether or not the joint is moving in a linear or angular axis (true is angular)
    /// </summary>
    public bool angular;

    public bool useNoWrap;
    public float noWrapAngle;
    
    /// <summary>
    /// Specifies the EUler axis to controll. must be (1,0,0) (0,1,0) or (0,0,1)
    /// </summary>
    public Vector3 driveAxis;
    /// <summary>
    /// Sets the home location.
    /// </summary>
    public float home; 
    /// <summary>
    /// Used when another scripts needs to controll the target instead of the passed through setpoints.
    /// </summary>
    public bool follower = false;
    
    private PlayerInput _playerInput;
    public InputActionMap _inputMap;
    public float _targetPosition;

    private GameObject _robotParent;
    
    private PIDController _pidController;
    
    private Dictionary<SetPoint, float> originalPositions = new Dictionary<SetPoint, float>();

    private string _sequencePoint;
    private bool _sequenceActive;
    private float _sequenceTime;
    private bool _delayType;

    [HideInInspector] public float p;
    [HideInInspector] public float i;
    [HideInInspector] public float d;
    [HideInInspector] public float iSat;
    [HideInInspector] public float max;
    
    /// <summary>
    /// The setpoint struct to base the logic around.
    /// </summary>
    [HideInInspector] public SetPoint[] setPoints;
    
    // Start is called before the first frame update
    void Start()
    {
        _sequenceTime = 0;
        _targetPosition = 0;
        _sequenceActive = false;
        _delayType = false;
        _sequencePoint = "";
        _robotParent = Utils.FindParentPlayerInput(gameObject);

        _playerInput = _robotParent.GetComponent<PlayerInput>();
        
        _inputMap = _playerInput.actions.FindActionMap("Robot");
        
        _inputMap.Enable();

        _pidController = new PIDController
        {
            proportionalGain = p,
            derivativeGain = 0,
            integralGain = 0,
            outputMax = max,
            outputMin = -max,
            integralSaturation = 0
        };
    }

    /// <summary>
    /// the overide function for running a joint PID directly instead of through the setpoint object
    /// </summary>
    /// <param name="position"></param>
    public void FollowPosition(float position)
    {  
       this._targetPosition = position; 
    }

    // Update is called once per frame
    void Update()
    {
        noWrapAngle = Mathf.Repeat(noWrapAngle, 360);
        
        if (_sequenceTime > 0)
        {
            _sequenceTime -= Time.deltaTime;
        }
        
        bool alreadyMoved = false;
        
        if (follower) return; 
        
        for (int i = 0; i < setPoints.Length; i++)
        {
            
            var setPoint = setPoints[i];
            var controllerAction = _inputMap.FindAction(setPoint.controllerButton);
            var keyboardAction = _inputMap.FindAction(setPoint.keyboardButton);

            var buttonPressed = controllerAction.triggered || keyboardAction.triggered;
            var buttonHeld = controllerAction.IsPressed() || keyboardAction.IsPressed();

            //I dont even know and I just finished.
            switch (setPoint.controlType)
            {
                
                case ControlType.Hold:
                    
                    if (buttonPressed)
                    {
                        if (!originalPositions.ContainsKey(setPoint))
                        {
                            // Store original position
                            originalPositions.Clear();
                            originalPositions[setPoint] = home;
                            // Apply new position
                            _targetPosition = setPoint.point;
                        }
                    }
                    else if (originalPositions.ContainsKey(setPoint) && !buttonHeld)
                    {
                        // Restore original position
                        _targetPosition = originalPositions[setPoint];
                        originalPositions.Remove(setPoint);
                    }

                    break;

                case ControlType.Sequence:
                    // Implement Sequence logic
                    //its cooked. just dont touch
                    switch (setPoint.sequenceType)
                    {
                        //delay logic
                        case (SequenceType.delay):
                            if (_sequenceActive && _sequencePoint == setPoint.setpointName && _sequenceTime <= 0 && _delayType)
                            {
                                _targetPosition = setPoint.point;
                                _sequencePoint = setPoint.sequenceTo;
                                _sequenceTime = setPoint.delay;
                                _delayType = true;

                                _sequenceActive = _sequencePoint.Length > 0;
                                alreadyMoved = true;
                            }
                            else if (!_sequenceActive && buttonPressed)
                            {
                                bool startPoint = false;
                                for (int j = 0; j < setPoints.Length; j++)
                                {
                                    if (setPoints[j].sequenceTo == setPoint.setpointName)
                                    {
                                        startPoint = true;
                                    }
                                }

                                if (!startPoint)
                                {
                                    _targetPosition = setPoint.point;
                                    _sequencePoint = setPoint.sequenceTo;

                                    _sequenceActive = _sequencePoint.Length > 0;
                                    _sequenceTime = setPoint.delay;
                                    _delayType = true;
                                    alreadyMoved = true;
                                }
                            } else if (!_delayType && buttonPressed && _sequencePoint == setPoint.setpointName && !alreadyMoved)
                            {
                                _targetPosition = setPoint.point;
                                _sequencePoint = setPoint.sequenceTo;
                                _sequenceTime = setPoint.delay;
                                _delayType = true;

                                _sequenceActive = _sequencePoint.Length > 0;
                                alreadyMoved = true;
                            }

                            break;
                        
                        //next press logic
                        case (SequenceType.nextPress):
                            if (_sequenceActive && _sequencePoint == setPoint.setpointName && _sequenceTime <= 0 && _delayType)
                            {
                                _targetPosition = setPoint.point;
                                _sequencePoint = setPoint.sequenceTo;
                                _delayType = false;

                                _sequenceActive = _sequencePoint.Length > 0;
                                alreadyMoved = true;
                            }
                            else if (buttonPressed && !alreadyMoved)
                            {
                                _delayType = false;
                                if (_sequenceActive && _sequencePoint == setPoint.setpointName && !alreadyMoved)
                                {
                                    _targetPosition = setPoint.point;
                                    _sequencePoint = setPoint.sequenceTo;

                                    _sequenceActive = _sequencePoint.Length > 0;
                                    alreadyMoved = true;
                                }
                                else if (!_sequenceActive && !alreadyMoved)
                                {
                                    bool startPoint = false;
                                    for (int j = 0; j < setPoints.Length; j++)
                                    {
                                        if (setPoints[j].sequenceTo == setPoint.setpointName)
                                        {
                                            startPoint = true;
                                        }
                                    }

                                    if (!startPoint)
                                    {
                                        _targetPosition = setPoint.point;
                                        _sequencePoint = setPoint.sequenceTo;

                                        _sequenceActive = _sequencePoint.Length > 0;
                                        alreadyMoved = true;
                                    }
                                }
                            }

                            break;
                    }

                    break;

                case ControlType.Toggle:
                    if (buttonPressed)
                    {
                        if (originalPositions.ContainsKey(setPoint))
                        {
                            _targetPosition = home;
                            originalPositions.Remove(setPoint);
                        }
                        else
                        {
                            originalPositions[setPoint] = setPoint.point;
                            _targetPosition = setPoint.point;
                        }
                    }
                    break;
            }
        }
    }

    private void FixedUpdate()
    {
        
        float rawPID;

        if (angular)
        {
            float targetForPid = -_targetPosition;
            var wrapAngle = noWrapAngle;
            wrapAngle = Utils.FlipAngle(wrapAngle);
            wrapAngle = Mathf.Repeat(wrapAngle, 360);
            if (useNoWrap)
            {
                if (PassesThroughWrapAngle(currentPosition, targetForPid, wrapAngle))
                {
                    // Force the long way by adding/subtracting 360 to the target
                    float difference = Utils.AngleDifference(_targetPosition, currentPosition);
        
                    if (difference > 0)
                    {
                        // Would normally go counter-clockwise, force clockwise
                        targetForPid = wrapAngle + 180;
                    }
                    else
                    {
                        // Would normally go clockwise, force counter-clockwise
                        targetForPid = wrapAngle - 180;
                    }
                }
                else
                {
                    //this case is redundant for my sanity
                    // Normal case - shortest path doesn't pass through wrap angle
                    targetForPid = -_targetPosition;
                }
            }
            
            rawPID = _pidController.UpdateAngle(Time.fixedDeltaTime,currentPosition, targetForPid);
            joint.targetAngularVelocity = rawPID * driveAxis;
        }
        else
        {
            rawPID = _pidController.UpdateLinear(Time.fixedDeltaTime,currentPosition, _targetPosition);
            joint.targetVelocity = -rawPID * driveAxis;
        }
    }
    
    bool PassesThroughWrapAngle(float currentAngle, float targetAngle, float wrapAngle)
    {
        // Normalize all angles to [0, 360)
        currentAngle = ((currentAngle % 360) + 360) % 360;
        targetAngle = ((targetAngle % 360) + 360) % 360;
        wrapAngle = ((wrapAngle % 360) + 360) % 360;
    
        // Calculate the shortest angular difference
        float diff = targetAngle - currentAngle;
        if (diff > 180.0f) diff -= 360.0f;
        if (diff < -180.0f) diff += 360.0f;
    
        // Determine the angular span we're traversing
        float endAngle = currentAngle + diff;
        if (endAngle < 0) endAngle += 360.0f;
        if (endAngle >= 360.0f) endAngle -= 360.0f;
    
        // Check if wrapAngle is between start and end on the shortest path
        if (diff > 0) {
            // Moving counter-clockwise
            if (currentAngle <= endAngle) {
                return (wrapAngle > currentAngle && wrapAngle < endAngle);
            } else {
                return (wrapAngle > currentAngle || wrapAngle < endAngle);
            }
        } else {
            // Moving clockwise  
            if (currentAngle >= endAngle) {
                return (wrapAngle < currentAngle && wrapAngle > endAngle);
            } else {
                return (wrapAngle < currentAngle || wrapAngle > endAngle);
            }
        }
    }
}
