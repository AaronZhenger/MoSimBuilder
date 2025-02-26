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
    
    private PlayerInput _playerInput;
    public InputActionMap _inputMap;
    public float _targetPosition;

    private GameObject _robotParent;
    
    private PIDController _pidController;
    
    [HideInInspector] public SetPoint[] setPoints;
    // Start is called before the first frame update
    void Start()
    {
        _targetPosition = 0;
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
        for (int i = 0; i < setPoints.Length; i++)
        {
            if (_inputMap.FindAction(setPoints[i].controllerButton).triggered || _inputMap.FindAction(setPoints[i].keyboardButton).triggered)
            {
                _targetPosition = setPoints[i].point;
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
