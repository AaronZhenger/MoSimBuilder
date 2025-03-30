using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Util;

public class JointController : MonoBehaviour
{
    public float currentPosition;
    public ConfigurableJoint joint;
    public bool angular;
    public Vector3 driveAxis;
    public float home;
    
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
            proportionalGain = 5,
            derivativeGain = 0,
            integralGain = 0,
            outputMax = 1,
            outputMin = -1,
            integralSaturation = 0
        };
    }

    // Update is called once per frame
    void Update()
    {
        if (_sequenceTime > 0)
        {
            _sequenceTime -= Time.deltaTime;
        }
        
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
                    switch (setPoint.sequenceType)
                    {
                        case (SequenceType.delay):
                            if (_sequenceActive && _sequencePoint == setPoint.setpointName && _sequenceTime <= 0)
                            {
                                _targetPosition = setPoint.point;
                                _sequencePoint = setPoint.sequenceTo;
                                _sequenceTime = setPoint.delay;
                                _delayType = true;

                                _sequenceActive = _sequencePoint.Length > 0;
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
                                }
                            }

                            break;
                        case (SequenceType.nextPress):
                            if (_sequenceActive && _sequencePoint == setPoint.setpointName && _sequenceTime <= 0 && _delayType)
                            {
                                _targetPosition = setPoint.point;
                                _sequencePoint = setPoint.sequenceTo;
                                _delayType = false;

                                _sequenceActive = _sequencePoint.Length > 0;
                            }
                            else if (buttonPressed)
                            {
                                if (_sequenceActive && _sequencePoint == setPoint.setpointName)
                                {
                                    _targetPosition = setPoint.point;
                                    _sequencePoint = setPoint.sequenceTo;

                                    _sequenceActive = _sequencePoint.Length > 0;
                                }
                                else if (!_sequenceActive)
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
            rawPID = _pidController.UpdateAngle(Time.deltaTime,currentPosition, _targetPosition);
            joint.targetAngularVelocity = rawPID * driveAxis;
        }
        else
        {
            rawPID = _pidController.Update(Time.deltaTime,currentPosition, _targetPosition);
            joint.targetVelocity = rawPID * driveAxis;
        }
    }
}
