using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Util;


public class SwerveController : MonoBehaviour
{
    //constant settings
    private int leftFront = 0;
    private int rightFront = 1;
    private int leftRear = 2;
    private int rightRear = 3;

    //used by build frame
    [HideInInspector] private ModuleBehaviour[] _modules;
    [HideInInspector] public float gearRatio;
    [HideInInspector] public Rigidbody rb;

    public float wheelDiameter;
    //-=-=-=-=-=

    //begin visible section
    public bool fieldCentric = false;
    public bool reversed = false;
    public bool isRed = false;
    //end visible section

    //Settings
    private float velocityMp = 1;
    private float steerMp = 1;

    //control stuff
    private Vector2 _translateValue;
    private Vector2 _rotateValue;

    private PlayerInput _playerInput;
    private InputActionMap _inputActionMap;

    private InputAction _translateAction;
    private InputAction _rotateAction;

    private string[] _moduleNames = new string[4];

    private bool inputsOveriden;

    private bool inputsOveridable;

    // Start is called before the first frame update
    void Start()
    {

        _playerInput = gameObject.GetComponent<PlayerInput>();

        _translateAction = _playerInput.actions.FindAction("LeftStick");
        _rotateAction = _playerInput.actions.FindAction("RightStick");
        _translateAction.Enable();
        _rotateAction.Enable();

        _moduleNames[0] = "lf";
        _moduleNames[1] = "rf";
        _moduleNames[2] = "lr";
        _moduleNames[3] = "rr";
        _modules = new ModuleBehaviour[4];

        var driveTrain = Utils.FindChild("driveTrain", gameObject);

        for (int i = 0; i < _modules.Length; i++)
        {
            if (Utils.FindChild(_moduleNames[i], driveTrain).GetComponent<ModuleBehaviour>())
            {
                _modules[i] = Utils.FindChild(_moduleNames[i], driveTrain).GetComponent<ModuleBehaviour>();
                _modules[i].gearRatio = gearRatio;
                _modules[i].wheelDiameter = (wheelDiameter + 0.01f) * 0.0254f;
                _modules[i]._rb = rb;
            }
        }

        inputsOveriden = false;
    }

    public void overideInputs(float x, float y, float angle, bool disruptable = false)
    {
        _translateValue = new Vector2(x, y);
        _rotateValue = new Vector2(angle, 0);
        inputsOveriden = true;
        inputsOveridable = disruptable;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        //update controls
        if (_translateAction.ReadValue<Vector2>().magnitude > 0.05f && inputsOveridable)
        {
            _translateValue = _translateAction.ReadValue<Vector2>();
            _rotateValue = _rotateAction.ReadValue<Vector2>();
            inputsOveriden = false;
        }
        else if (!inputsOveriden)
        {
            _translateValue = _translateAction.ReadValue<Vector2>();
            _rotateValue = _rotateAction.ReadValue<Vector2>();
        }

        //rotate input to match alliance and scheme
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


        if (fieldCentric || inputsOveriden)
        {

            if (!reversed || inputsOveriden)
            {
                inputsOveriden = false;

                fwd = fieldRelativeAngle.x * velocityMp;

                str = fieldRelativeAngle.z * velocityMp;
            }
            else
            {
                fwd = -fieldRelativeAngle.x * velocityMp;

                str = -fieldRelativeAngle.z * velocityMp;
            }
        }
        else
        {
            if (!reversed)
            {
                fwd = driveInput.x * velocityMp;

                str = driveInput.z * velocityMp;
            }
            else
            {
                fwd = -driveInput.x * velocityMp;

                str = -driveInput.z * velocityMp;
            }
        }

        // Swerve Math
        var RCW = -_rotateValue.x * steerMp;

        var L = _modules[leftFront].transform.localPosition.z - _modules[rightFront].transform.localPosition.z;

        var W = _modules[leftFront].transform.localPosition.x - _modules[rightFront].transform.localPosition.x;

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

        //assign outputs
        _modules[leftFront].targetVelocity = ws2;
        _modules[leftRear].targetVelocity = ws3;
        _modules[rightFront].targetVelocity = ws1;
        _modules[rightRear].targetVelocity = ws4;

        _modules[leftFront].targetModuleAngle = wa2;
        _modules[leftRear].targetModuleAngle = wa3;
        _modules[rightFront].targetModuleAngle = wa1;
        _modules[rightRear].targetModuleAngle = wa4;
    }
}
