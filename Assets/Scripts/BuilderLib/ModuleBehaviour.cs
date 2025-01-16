using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModuleBehaviour : MonoBehaviour
{
    private WheelBehaviour _wheelBehaviour;
    private DriveMotor _driveMotor;
    [SerializeField] private float wheelDiameter;
    [HideInInspector] public Rigidbody rb;

    [HideInInspector] public float inputVoltage;
    // Start is called before the first frame update
    void Start()
    {
       _wheelBehaviour = gameObject.AddComponent<WheelBehaviour>();
       _wheelBehaviour.wheelDiameter = wheelDiameter;
       _driveMotor = gameObject.AddComponent<DriveMotor>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        var force = _driveMotor.DriveSimUpdate(inputVoltage) * (2*Mathf.PI*(wheelDiameter/2))/(60 * 10000) * rb.mass;
        
        for (int i = 0; i < _wheelBehaviour.collisionPoints.Count; i++)
        {
            rb.AddForceAtPosition((rb.transform.forward.normalized*force)/_wheelBehaviour.collisionPoints.Count, _wheelBehaviour.collisionPoints[i]);
        }
    }
}
