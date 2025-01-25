using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DriveMotor : MonoBehaviour
{
    private const float StallTorque = 7;
    private float _momentOfInertia = 0.0000105f;

    [HideInInspector] public float gearRatio = 5.85f;

    public float motorSpeed;

    private const float Kv = 6000 / 12;

    // Start is called before the first frame update
    void Start()
    {
        motorSpeed = 0;
        
        _momentOfInertia = _momentOfInertia * ((1/gearRatio) * (1/gearRatio));
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public float DriveSimUpdate(float voltage, float realSpeed)
    {
        //reset drive speed to real speed
        motorSpeed = realSpeed;
        //w = RPM Wm = Max Rpm Ts = stall Torque J = Moi
        //t = -((J*Wm)/Ts) * ln((Wm-w)/Wm)
        //dw = ((Wm-w)/Wm)*(Ts/(J*Wm)) * dt
        //Wm = V*KV
        //V = voltage
        //Kv = RPM per Voltage
        //dw = ((V*Kv-w)/(V*Kv)) * (Ts/(J*V*Kv))
        motorSpeed += ((voltage * Kv - motorSpeed)/(12*Kv)) * (StallTorque/(_momentOfInertia*12*Kv));
        return motorSpeed;
    }

    public void ResetMotor()
    {
        motorSpeed = 0;
    }
}
