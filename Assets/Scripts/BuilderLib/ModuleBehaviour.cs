using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Util;

public class ModuleBehaviour : MonoBehaviour
{
    //input settigns
    /// <summary>
    /// The diameter of the swerve wheel
    /// </summary>
    [HideInInspector] public float wheelDiameter;
    /// <summary>
    /// The gear ratio of the motor to drive wheel (positive reduction)
    /// </summary>
    [HideInInspector] public float gearRatio;
    /// <summary>
    /// The target velocity for the drive contorller
    /// </summary>
    [HideInInspector] public float targetVelocity = 0;
    /// <summary>
    /// The target angle to finish the modules at.
    /// </summary>
    [HideInInspector] public float targetModuleAngle = 0;
    
    private WheelBehaviour _wheelBehaviour;
    private DriveMotor _driveMotor;
    [HideInInspector] public Rigidbody _rb;
    private float _startingRotation;
    private GameObject _wheelModel;

    // Start is called before the first frame update
    void Start()
    {
        
        //add wheel behaviour to the correct object
        _wheelBehaviour = Utils.FindChild("Wheel", gameObject).AddComponent<WheelBehaviour>();
        
        _wheelBehaviour.wheelDiameter = wheelDiameter;
       
        //add drive motor sim to object
        _driveMotor = gameObject.AddComponent<DriveMotor>();
        _driveMotor.gearRatio = gearRatio;
       
        _startingRotation = transform.localRotation.eulerAngles.y;

        _wheelModel = Utils.FindChild("Model", _wheelBehaviour.gameObject);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if(_rb == null) return;
        _wheelBehaviour.wheelDiameter = wheelDiameter;
        _driveMotor.gearRatio = gearRatio;
        
        float targetRotation = Mathf.Repeat(targetModuleAngle-_startingRotation, 360);
        float realSpeed = (_wheelBehaviour.transform.InverseTransformDirection(_rb.GetPointVelocity(_wheelBehaviour.transform.position)).z / (Mathf.PI * wheelDiameter)) * 60;
        
        if (FMS.RobotState == RobotState.disabled)
        {
            targetVelocity = 0;
        }
        
        float feedForward = targetVelocity * 18; //Kv * target = voltage
        float pValue = ((targetVelocity * 6000) - _driveMotor.motorSpeed) * (80/6000); //error * target * p = Perror
        float angleError = targetRotation - _wheelBehaviour.transform.localEulerAngles.y;
        float voltage = Mathf.Clamp(feedForward + pValue * ((90 - Mathf.Clamp(Mathf.Abs(angleError),0,90))/90), -12, 12);
        
        //f = m * a     a = Vtarget - Vreal
        float force = ((Mathf.PI * wheelDiameter * (_driveMotor.DriveSimUpdate(voltage, realSpeed*gearRatio)/gearRatio)/60) - _wheelBehaviour.transform.InverseTransformDirection(_rb.GetPointVelocity(_wheelBehaviour.transform.position)).z) * _rb.mass;
        
        float friction = _wheelBehaviour.transform.InverseTransformDirection(_rb.GetPointVelocity(_wheelBehaviour.transform.position)).x * -3f * _rb.mass;
        for (int i = 0; i < _wheelBehaviour.collisionPoints.Count; i++)
        {
            //drive wheel force
                _rb.AddForceAtPosition(
                    (_wheelBehaviour.collisionNormals[i] * force) / _wheelBehaviour.collisionPoints.Count,
                    _wheelBehaviour.collisionPoints[i]);
            

            //friction force
            _rb.AddForceAtPosition((_wheelBehaviour.transform.right.normalized*friction)/_wheelBehaviour.collisionPoints.Count, _wheelBehaviour.collisionPoints[i]);
        }


        if (FMS.RobotState == RobotState.enabled)
        {
            _wheelBehaviour.transform.localEulerAngles = Quaternion.Lerp(_wheelBehaviour.transform.localRotation,
                Quaternion.Euler(0, targetRotation, 0), 360 * Time.deltaTime).eulerAngles;

            _wheelModel.transform.Rotate(Vector3.right, realSpeed * Time.deltaTime);
        }

    }
}
