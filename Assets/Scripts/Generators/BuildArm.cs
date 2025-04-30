using System;
using System.Collections;
using System.Collections.Generic;
using MyBox;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;
using Util;

[ExecuteAlways]
public class BuildArm : MonoBehaviour
{
    [SerializeField] private SetPoint[] setPoints;
    
    [Header("Model Settings")]
    [SerializeField] private ArmModel armModel;

    [ConditionalField(true, nameof(Predicate))] 
    [SerializeField]
    private float length;
    private bool Predicate() => armModel == ArmModel.Single || armModel == ArmModel.SplitParallel;

    [ConditionalField(nameof(armModel), false, ArmModel.SplitParallel)] [SerializeField]
    private float width;
    
    private ConfigurableJoint _joint;
    
    private Rigidbody _rigidbody;

    private GameObject _connectedBody;

    private GeneratePart[] _parts;
    
    private GameObject _modelObject;

    private JointController _controller;

    private JointDrive _drive;
    
    private GameObject _tubingObject;
    // Start is called before the first frame update
    void Start()
    {
        Startup();
        
        if (EditorApplication.isPlaying)
        {
            GenRB();
            GenJoint();
            GenController();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!EditorApplication.isPlaying)
        {
            BuildModel();
        }
        else
        {
            float angle = Quaternion.Angle(transform.rotation, _joint.connectedBody.rotation);

            if (transform.localRotation.eulerAngles.x > 180)
            {
                angle = -angle;
            }

            if (angle < 0)
            {
                angle += 360;
            }

            if (angle >= 360)
            {
                angle -= 360;
            }

            if (angle < 0)
            {
                angle += 360;
            }

            angle = Mathf.Repeat(angle, 360);

            if (!EditorApplication.isPlaying)
            {

            }
            else
            {
                _controller.setPoints = setPoints;
                _controller.currentPosition = angle;
            }
        }
    }

    private void Startup()
    {
        var loadedTubes = Resources.LoadAll<GameObject>("Tubing") as GameObject[];

        foreach (var loadedTube in loadedTubes)
        {
            if (loadedTube.name == "OneXTwoXEighth")
            {
                _tubingObject = loadedTube;
            }
        }

        var detectedPart = Utils.FindChild("Model", gameObject);
        if (detectedPart != null)
        {
            _modelObject = detectedPart.gameObject;
        }
    }

    private void BuildModel()
    {
        CreateModelObject();
        
        switch (armModel)
        {
            case ArmModel.Single:
                if (_parts == null)
                {
                    CreateSingleArm();
                }
                else if (_parts.Length != 1)
                {
                    DestroyImmediate(_modelObject);
                    CreateModelObject();
                    CreateSingleArm();
                }
                else
                {
                    _parts[0].LoadedPartLocation = new Vector3(0,0, (length/2) * 0.0254f);
                    _parts[0].LoadedPartRotation = Quaternion.Euler(0, 0, 0);
                    _parts[0].LoadedPartScale = new Vector3(1, 1, length * 0.0254f);
                }
                break;
            case ArmModel.SplitParallel:
                if (_parts == null)
                {
                    CreateDoubleArm();
                }
                else if (_parts.Length != 2)
                {
                    DestroyImmediate(_modelObject);
                    CreateModelObject();
                    CreateDoubleArm();
                }
                else
                {
                    _parts[0].LoadedPartLocation = new Vector3(((width/2) - 0.5f) * 0.0254f,0, (length/2) * 0.0254f);
                    _parts[0].LoadedPartRotation = Quaternion.Euler(0, 0, 0);
                    _parts[0].LoadedPartScale = new Vector3(1, 1, length * 0.0254f);
                    
                    _parts[1].LoadedPartLocation = new Vector3(((-width/2) + 0.5f) * 0.0254f,0, (length/2) * 0.0254f);
                    _parts[1].LoadedPartRotation = Quaternion.Euler(0, 0, 0);
                    _parts[1].LoadedPartScale = new Vector3(1, 1, length * 0.0254f);
                }
                break;
            case ArmModel.None:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
    
    private void CreateDoubleArm()
    {
        _parts = CheckTubes(_modelObject, 2);

        if (_parts[0] == null)
        {
            _parts[0] = _modelObject.AddComponent<GeneratePart>();
            _parts[0].Part = _tubingObject;
            _parts[0].PartName = "DoubleL";
            _parts[0].LoadedPartLocation = new Vector3(((width/2) - 0.5f) * 0.0254f,0, (length/2) * 0.0254f);
            _parts[0].LoadedPartRotation = Quaternion.Euler(0, 0, 0);
            _parts[0].LoadedPartScale = new Vector3(1, 1, length * 0.0254f);
        }

        if (_parts[1] == null)
        {
            _parts[1] = _modelObject.AddComponent<GeneratePart>();
            _parts[1].Part = _tubingObject;
            _parts[1].PartName = "DoubleR";
            _parts[1].LoadedPartLocation = new Vector3(((-width/2) + 0.5f) * 0.0254f,0, (length/2) * 0.0254f);
            _parts[1].LoadedPartRotation = Quaternion.Euler(0, 0, 0);
            _parts[1].LoadedPartScale = new Vector3(1, 1, length * 0.0254f);
        }
    }
    
    private void CreateSingleArm()
    {
        _parts = CheckTubes(_modelObject, 1);

        if (_parts[0] == null)
        {
            _parts[0] = _modelObject.AddComponent<GeneratePart>();
            _parts[0].Part = _tubingObject;
            _parts[0].PartName = "Single";
            _parts[0].LoadedPartLocation = new Vector3(0,0, (length/2) * 0.0254f);
            _parts[0].LoadedPartRotation = Quaternion.Euler(0, 0, 0);
            _parts[0].LoadedPartScale = new Vector3(1, 1, length * 0.0254f);
        }
    }
    
    private GeneratePart[] CheckTubes(GameObject parent, int num)
    {
        GeneratePart[] unfiltered = parent.GetComponents<GeneratePart>();
        
        GeneratePart[] filtered = new GeneratePart[num];

        for (int i = 0; i < num; i++)
        {
            filtered[i] = null;
        }

        foreach (var t in unfiltered)
        {
            switch (armModel)
            {
                case ArmModel.Single:
                    if (t.name == "Single")
                    {
                        filtered[0] = t;
                    }
                    break;
                case ArmModel.SplitParallel:
                    if (t.name == "DoubleL")
                    {
                        filtered[0] = t;
                    } 
                    else if (t.name == "DoubleR")
                    {
                        filtered[1] = t;
                    }
                    break;
                case ArmModel.None:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        return filtered;
    }

    private void CreateModelObject()
    {
        if (_modelObject == null)
        {
            _modelObject = new GameObject
            {
                name = "Model"
            };
            
            _modelObject.transform.SetParent(transform);
            _modelObject.transform.localPosition = Vector3.zero;
            _modelObject.transform.localRotation = Quaternion.identity;
            
            _modelObject.transform.localScale = Vector3.one;
        }
    }

    private void GenController()
    {
        _controller = gameObject.AddComponent<JointController>();
            
        _controller.p = 1;
        _controller.i = 0;
        _controller.d = 0.0005f;
        _controller.iSat = 0;
        _controller.max = 10;
        _controller.angular = true;
        _controller.driveAxis = new Vector3(1, 0, 0);
        _controller.joint = _joint;
    }

    private void GenJoint()
    {
        _joint = gameObject.AddComponent<ConfigurableJoint>();
            
        _connectedBody = Utils.FindParentRB(gameObject);

        _joint.connectedBody = _connectedBody.GetComponent<Rigidbody>();
        _joint.xMotion = ConfigurableJointMotion.Locked;
        _joint.yMotion = ConfigurableJointMotion.Locked;
        _joint.zMotion = ConfigurableJointMotion.Locked;
        _joint.angularYMotion = ConfigurableJointMotion.Locked;
        _joint.angularZMotion = ConfigurableJointMotion.Locked;

        _joint.angularXMotion = ConfigurableJointMotion.Free;
        
        _drive.maximumForce = 8000;
        _drive.positionDamper = 100;
        _drive.positionSpring = 0;
        _drive.useAcceleration = false;
        _joint.angularXDrive = _drive;
    }

    private void GenRB()
    {
        _rigidbody = gameObject.AddComponent<Rigidbody>();
        _rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
        _rigidbody.collisionDetectionMode = CollisionDetectionMode.Continuous;
    }
}
