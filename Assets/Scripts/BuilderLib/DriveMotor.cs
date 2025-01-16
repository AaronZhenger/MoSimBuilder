using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DriveMotor : MonoBehaviour
{
    private const float StallTorque = 7;
    private const float MomentOfInertia = 10;

    [HideInInspector] public float motorSpeed;

    private const float Kv = 6000 / 12;

    // Start is called before the first frame update
    void Start()
    {
        motorSpeed = 0;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public float DriveSimUpdate(float voltage)
    {
        //w = RPM Wm = Max Rpm Ts = stall Torque J = Moi
        //t = -((J*Wm)/Ts) * ln((Wm-w)/Wm)
        //dw = ((Wm-w)/Wm)*(Ts/(J*Wm)) * dt
        //Wm = V*KV
        //V = voltage
        //Kv = RPM per Voltage
        //dw = ((V*Kv-w)/(V*Kv)) * (Ts/(J*V*Kv))
        motorSpeed += ((voltage * Kv - motorSpeed)/(voltage*Kv)) * (StallTorque/(MomentOfInertia*voltage*Kv));
        return ((voltage * Kv - motorSpeed)/(voltage*Kv)) * (StallTorque/(MomentOfInertia*voltage*Kv));
    }

    public void ResetMotor()
    {
        motorSpeed = 0;
    }
}
