using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModuleBehaviour : MonoBehaviour
{
    private WheelBehaviour _wheelBehaviour;
    private DriveMotor _driveMotor;
    [HideInInspector] public float targetVelocity = 0;
    [HideInInspector] public float targetModuleAngle = 0;
    [SerializeField] private float wheelDiameter;
    [SerializeField] private float gearRatio;
    [HideInInspector] public Rigidbody rb;

    // Start is called before the first frame update
    void Start()
    {
       _wheelBehaviour = gameObject.AddComponent<WheelBehaviour>();
       _wheelBehaviour.wheelDiameter = wheelDiameter;
       _driveMotor = gameObject.AddComponent<DriveMotor>();
       _driveMotor.gearRatio = gearRatio;
       
       var t = transform;
       while (t.GetComponent<Rigidbody>() == null)
       {
           t = t.parent.transform;
       }
       
       rb = t.GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        var realSpeed = (_wheelBehaviour.transform.InverseTransformDirection(rb.GetPointVelocity(_wheelBehaviour.transform.position)).z / (Mathf.PI * wheelDiameter)) * 60;
        var feedForward = targetVelocity * 13;
        var pValue = ((targetVelocity * 6000) - _driveMotor.motorSpeed) * (12/6000);
        var voltage = Mathf.Clamp(feedForward + pValue, -12, 12);
        var force = ((Mathf.PI * wheelDiameter * (_driveMotor.DriveSimUpdate(voltage, realSpeed*gearRatio)/gearRatio)/60) - _wheelBehaviour.transform.InverseTransformDirection(rb.GetPointVelocity(_wheelBehaviour.transform.position)).z) * rb.mass;
        var friction = _wheelBehaviour.transform.InverseTransformDirection(rb.GetPointVelocity(_wheelBehaviour.transform.position)).x * -1.15f * rb.mass;
        
        for (int i = 0; i < _wheelBehaviour.collisionPoints.Count; i++)
        {
            rb.AddForceAtPosition((_wheelBehaviour.collisionNormals[i]*force)/_wheelBehaviour.collisionPoints.Count, _wheelBehaviour.collisionPoints[i]);
            rb.AddForceAtPosition((_wheelBehaviour.transform.right.normalized*friction)/_wheelBehaviour.collisionPoints.Count, _wheelBehaviour.collisionPoints[i]);
        }
        
        _wheelBehaviour.transform.localEulerAngles = Quaternion.Lerp(_wheelBehaviour.transform.localRotation, Quaternion.Euler(0,targetModuleAngle,0), 360f*Time.deltaTime).eulerAngles;
    }
}
