using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class SwerveController : MonoBehaviour
{
    [SerializeField] private ModuleBehaviour[] modules;
    [SerializeField] private bool fieldCentric = false;
    [SerializeField] private bool reversed = false;
    [SerializeField] private bool isRed = false;
    private float velocityMp = 1;
    private float steerMp = 1;
    private Vector2 _translateValue;
    private float _rotateValue;

    private int leftFront = 0;
    private int rightFront = 1;
    private int leftRear = 2;
    private int rightRear = 3;
    
    private PlayerInput _playerInput;
    private InputActionMap _inputActionMap;
    
    private InputAction _translateAction;
    private InputAction _rotateAction;
    // Start is called before the first frame update
    void Start()
    {
        _playerInput = gameObject.GetComponent<PlayerInput>();
        _playerInput.actions.Enable();
        
        _inputActionMap = _playerInput.currentActionMap;
        _inputActionMap.Enable();
        
        _translateAction = _inputActionMap.FindAction("LeftStick");
        _rotateAction = _inputActionMap.FindAction("RightStick");
        _translateAction.Enable();
        _rotateAction.Enable();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        _translateValue = _translateAction.ReadValue<Vector2>();
        _rotateValue = _rotateAction.ReadValue<Vector2>().x;
        
        Vector3 driveInput = new Vector3(_translateValue.y, 0, _translateValue.x);

        float angle;
        if (!isRed)
        {
            angle = transform.localRotation.eulerAngles.y + 270;
        }
        else
        {
            angle = transform.localRotation.eulerAngles.y + 90;
        }
        
        Vector3 fieldRelativeAngle = Quaternion.AngleAxis(angle, Vector3.up) * driveInput;

        float fwd, str;

        if (fieldCentric)
        {
            if (!reversed)
            {
                fwd = fieldRelativeAngle.x * velocityMp;

                str = fieldRelativeAngle.z * velocityMp;
            } else
            {
                fwd = -fieldRelativeAngle.x * velocityMp;

                str = -fieldRelativeAngle.z * velocityMp;
            }
        }
        else
        {
            fwd = driveInput.x * velocityMp;

            str = driveInput.z * velocityMp;
        }

        
        var RCW = -_rotateValue * steerMp;
    
        var L = modules[leftFront].transform.localPosition.z - modules[rightFront].transform.localPosition.z;

        var W = modules[leftFront].transform.localPosition.x - modules[rightFront].transform.localPosition.x;

        var R = Mathf.Sqrt(MathF.Pow(L, 2) + Mathf.Pow(W, 2));

        var A = str - RCW * (L / R);
        var B = str + RCW * (L / R);
        var C = fwd - RCW * (W / R);
        var D = fwd + RCW * (W / R);

        var ws1 = Mathf.Sqrt(Mathf.Pow(B, 2) + Mathf.Pow(C, 2));
        var wa1 = Mathf.Atan2(B, C) * 180 / Mathf.PI;

        var ws2 = Mathf.Sqrt(Mathf.Pow(B, 2) + Mathf.Pow(D, 2));
        var wa2 = Mathf.Atan2(B, D) * 180 / Mathf.PI;

        var ws3 = Mathf.Sqrt(Mathf.Pow(A, 2) + Mathf.Pow(D, 2));
        var wa3 = Mathf.Atan2(A, D) * 180 / Mathf.PI;

        var ws4 = Mathf.Sqrt(Mathf.Pow(A, 2) + Mathf.Pow(C, 2));
        var wa4 = Mathf.Atan2(A, C) * 180 / Mathf.PI;

        
        modules[leftFront].targetVelocity = ws2;
        modules[leftRear].targetVelocity = ws3;
        modules[rightFront].targetVelocity = ws1;
        modules[rightRear].targetVelocity = ws4;
        
        modules[leftFront].targetModuleAngle = wa2;
        modules[leftRear].targetModuleAngle = wa3;
        modules[rightFront].targetModuleAngle = wa1;
        modules[rightRear].targetModuleAngle = wa4;
    }
}
