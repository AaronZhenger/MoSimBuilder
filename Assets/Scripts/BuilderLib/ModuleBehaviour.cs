using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModuleBehaviour : MonoBehaviour
{
    //input settigns
    [HideInInspector] public float wheelDiameter;
    [HideInInspector] public float gearRatio;
    [HideInInspector] public float targetVelocity = 0;
    [HideInInspector] public float targetModuleAngle = 0;
    
    private WheelBehaviour _wheelBehaviour;
    private DriveMotor _driveMotor;
    private Rigidbody _rb;
    private float _startingRotation;
    private GameObject _wheelModel;

    // Start is called before the first frame update
    void Start()
    {
        //add wheel behaviour to the correct object
       _wheelBehaviour = transform.Find("Wheel").gameObject.AddComponent<WheelBehaviour>();
       _wheelBehaviour.wheelDiameter = wheelDiameter;
       
       //add drive motor sim to object
       _driveMotor = gameObject.AddComponent<DriveMotor>();
       _driveMotor.gearRatio = gearRatio;
       
       var t = transform;
       while (t.GetComponent<Rigidbody>() == null)
       {
           t = t.parent.transform;
       }
       
       _rb = t.GetComponent<Rigidbody>();
       
       _startingRotation = transform.localRotation.eulerAngles.y;
       
       _wheelModel = _wheelBehaviour.transform.Find("Model").gameObject;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        float targetRotation = Mathf.Repeat(targetModuleAngle-_startingRotation, 360);
        float realSpeed = (_wheelBehaviour.transform.InverseTransformDirection(_rb.GetPointVelocity(_wheelBehaviour.transform.position)).z / (Mathf.PI * wheelDiameter)) * 60;
        
        float feedForward = targetVelocity * 13; //Kv * target = voltage
        float pValue = ((targetVelocity * 6000) - _driveMotor.motorSpeed) * (12/6000); //error * target * p = Perror
        float angleError = targetRotation - _wheelBehaviour.transform.localEulerAngles.y;
        float voltage = Mathf.Clamp(feedForward + pValue * (1 - Mathf.Clamp(Mathf.Abs(angleError),0,90)/90), -12, 12);
        
        
        
        //f = m * a     a = Vtarget - Vreal
        float force = ((Mathf.PI * wheelDiameter * (_driveMotor.DriveSimUpdate(voltage, realSpeed*gearRatio)/gearRatio)/60) - _wheelBehaviour.transform.InverseTransformDirection(_rb.GetPointVelocity(_wheelBehaviour.transform.position)).z) * _rb.mass;
        
        float friction = _wheelBehaviour.transform.InverseTransformDirection(_rb.GetPointVelocity(_wheelBehaviour.transform.position)).x * -1.15f * _rb.mass;
        
        for (int i = 0; i < _wheelBehaviour.collisionPoints.Count; i++)
        {
            //drive wheel force
            _rb.AddForceAtPosition((_wheelBehaviour.collisionNormals[i]*force)/_wheelBehaviour.collisionPoints.Count, _wheelBehaviour.collisionPoints[i]);
            
            //friction force
            _rb.AddForceAtPosition((_wheelBehaviour.transform.right.normalized*friction)/_wheelBehaviour.collisionPoints.Count, _wheelBehaviour.collisionPoints[i]);
        }
        
        
        
        _wheelBehaviour.transform.localEulerAngles = Quaternion.Lerp(_wheelBehaviour.transform.localRotation, Quaternion.Euler(0,targetRotation,0), 360f*Time.deltaTime).eulerAngles;
        
        _wheelModel.transform.Rotate( Vector3.down,(realSpeed*Time.deltaTime));
        
    }
}
