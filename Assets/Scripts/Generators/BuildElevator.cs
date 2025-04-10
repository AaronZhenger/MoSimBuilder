using System.Collections;
using System.Collections.Generic;
using MyBox;
using UnityEditor;
using UnityEngine;
using Util;

[ExecuteAlways]
public class Buildelevator : MonoBehaviour
{
    [SerializeField] private SetPoint[] setPoints;

    [SerializeField] private bool model;
    
    [ConditionalField(nameof(model), false)]
    [SerializeField] private float width;
    [ConditionalField(nameof(model), false)]
    [SerializeField] private float height;
    [ConditionalField(nameof(model), false)]
    [SerializeField] private int stages;

    private Vector3 _startPose;
    
    private ConfigurableJoint _joint;
    
    private Rigidbody _rigidbody;

    private GameObject _connectedBody;

    private Rigidbody _gRb;

    private JointController _controller;

    private JointDrive _drive;
    
    private GeneratePart[][] _tubings;
    
    private GameObject _tubingObject;
    
    private float scaleFactor;
    
    [SerializeField] private GameObject[] _modelObjects;
    
    private Rigidbody[] _rigidbodies;
    
    private ConfigurableJoint[] _joints;

    private JointDrive[] _drives;
    
    private JointController[] _controllers;

    private bool[] _engaged;
    
    private bool[] wasEngaged;
    
    
    // Start is called before the first frame update
    void Start()
    {
        Startup();
        
        if (EditorApplication.isPlaying)
        {
            Initialize();
        }
    }

    private void Startup()
    {
        var loadedTubes =  Resources.LoadAll<GameObject>("Tubing") as GameObject[];

        foreach (var loadedTube in loadedTubes)
        {
            if (loadedTube.name == "OneXTwoXEighth")
            {
                _tubingObject = loadedTube;
            }
        }

        scaleFactor = 0.0254f;
    }

    private void Initialize()
    {
        GenerateRBs();
        GenerateJoints();
        generateControllers();
    }

    // Update is called once per frame
    void Update()
    {
        if (!EditorApplication.isPlaying)
        {
            if (model)
            {
                BuildModel();
            }
        }
        else
        {
            _controller.setPoints = setPoints;
            _controller.currentPosition = transform.localPosition.y - _startPose.y;
        }
    }

    private void continuousMovement(SetPoint[] setPoints)
    {
        //TODO: add the continuously rigged motion to this function

        if (_engaged == null)
        {
            _engaged = new bool[_rigidbodies.Length];
            wasEngaged = new bool[_rigidbodies.Length];

            for (int i = 0; i < _engaged.Length; i++)
            {
                _engaged[i] = false;   
                wasEngaged[i] = false;
            }
        }
        
        for (int i = 0; i < _rigidbodies.Length; i++)
        {
            if (i == _rigidbodies.Length - 1)
            {
                _controllers[i].setPoints = setPoints;
                _controllers[i].follower = false;
                continue; //skip follower calculations
            }
            else
            {
                _controllers[i].follower = true;
            }

            float combinedHeight = 0;
            for (int j = i+1; j < _rigidbodies.Length; j++)
            { 
                combinedHeight += (height - (1 * ((j < 2) ? 0 : j - 1) - ((stages - j) * -2))) * 0.0254f;
            }

            float setPoint = 0;
            if (combinedHeight > _rigidbodies[i].transform.localPosition.y - (stages))
            {
                _engaged[i] = true;
                setPoint = combinedHeight - _rigidbodies[i].transform.localPosition.y - (stages);;
            }
            else
            {
                _engaged[i] = false;
            }
            
            _controllers[i].FollowPosition(setPoint);
        }
    }

    private void continuousClick()
    {
        for (int i = 0; i < _engaged.Length; i++)
        {
            if (wasEngaged[i] != _engaged[i])
            {
                randomClick();
            }
        }
        
        wasEngaged = _engaged;
    }

    private void randomClick()
    {
        
    }

    private void cascadeMovement()
    {
        //TODO: add the cascade rigged motion to this function
    }
    private void GenerateRBs()
    {
        _rigidbodies = new Rigidbody[_modelObjects.Length - 1]; //stationary stage doesnt have a rb
        for (int i = 1; i < _modelObjects.Length; i++) //skip the stationary stage (0)
        {
            _rigidbodies[i] = _modelObjects[i].AddComponent<Rigidbody>();

            _rigidbodies[i].mass = 1;
            _rigidbodies[i].drag = 0;
            _rigidbodies[i].angularDrag = 0;
            _rigidbodies[i].useGravity = true;
            _rigidbodies[i].isKinematic = false;
            _rigidbodies[i].interpolation = RigidbodyInterpolation.Interpolate;
            _rigidbodies[i].collisionDetectionMode = CollisionDetectionMode.Continuous;
        }
    }
    
    private void GenerateJoints()
    {
        var driveTrain = Utils.FindParentRB(gameObject).GetComponent<Rigidbody>();
        _joints = new ConfigurableJoint[_modelObjects.Length - 1]; //stationary stage doesnt have a rb
        _drives = new JointDrive[_modelObjects.Length - 1];
        for (int i = 1; i < _modelObjects.Length; i++) //skip the stationary stage (0)
        {
            _joints[i] = _modelObjects[i].AddComponent<ConfigurableJoint>();

            _joints[i].connectedBody = driveTrain;
            _joints[i].xMotion = ConfigurableJointMotion.Locked;
            _joints[i].zMotion = ConfigurableJointMotion.Locked;
            _joints[i].angularYMotion = ConfigurableJointMotion.Locked;
            _joints[i].angularZMotion = ConfigurableJointMotion.Locked;
            _joints[i].angularXMotion = ConfigurableJointMotion.Locked;
            _joints[i].yMotion = ConfigurableJointMotion.Free;
            
            _drive.maximumForce = 8000000;
            _drive.positionDamper = 10000;
            _drive.positionSpring = 0;
            _drive.useAcceleration = false;
            
            GenerateDrive(i);
        }
    }

    private void generateControllers()
    {
        _controllers = new JointController[_modelObjects.Length - 1];
        for (int i = 1; i < _modelObjects.Length; i++)
        {
            _controllers[i] = gameObject.AddComponent<JointController>();

            _controllers[i].p = 5;
            _controllers[i].i = 0;
            _controllers[i].d = 0.0005f;
            _controllers[i].iSat = 0;
            _controllers[i].max = 5;
            _controllers[i].angular = false;
            _controllers[i].driveAxis = new Vector3(0, 1, 0);
            _controllers[i].joint = _joint;
        }
    }
    
    private void GenerateDrive(int i)
    {
        _drives[i].maximumForce = 8000000;
        _drives[i].positionDamper = 10000;
        _drives[i].positionSpring = 0;
        _drives[i].useAcceleration = false;
        _joints[i].yDrive = _drive;
    }

    private void BuildModel()
    {
        if (_modelObjects.Length == 0)
        {
            _modelObjects = new GameObject[stages+1];
            
            _tubings = new GeneratePart[stages+1][];
        }
        else if (_modelObjects.Length != stages + 1)
        {
            foreach (var modelObject in _modelObjects)
            {
                if (_modelObjects != null)
                {
                    DestroyImmediate(modelObject.gameObject);
                }
            }

            _modelObjects = new GameObject[stages+1];
            
            _tubings = new GeneratePart[stages+1][];
        }
        else
        {
            if (_tubings == null || _tubings.Length != stages + 1)
            {
                _tubings = new GeneratePart[stages + 1][];
            }
        }
        
        for (int i = 0; i <= stages; i++) //0 is stationary so 1 stage should generate 2
        {
            if (i <= stages) //all but last two should have a cross brace;
            {
                if (_modelObjects[i] == null)
                {
                    _modelObjects[i] = new GameObject
                    {
                        name = "Stage" + i,
                        transform =
                        {
                            parent = transform,
                            localPosition = Vector3.zero,
                            localRotation = Quaternion.identity,
                            localScale = Vector3.one
                        }
                    };
                }

                
                _modelObjects[i].transform.localPosition = new Vector3(0, i * 1 * 0.0254f, 0); //step the bottom up by 1 inch

                var tubing = CheckTubes(_modelObjects[i], i, 5);
                    
                if (tubing[0] == null)
                {
                    tubing[0] = _modelObjects[i].AddComponent<GeneratePart>();
                    tubing[0].Part = _tubingObject;
                    tubing[0].PartName = "lower crossbar (" + i + ")";
                    tubing[0].LoadedPartLocation =
                        new Vector3(0, 0.5f * 0.0254f, 0); //raise model by an inch so 0,0,0 is the absolute bottom.
                    tubing[0].LoadedPartRotation = Quaternion.Euler(0, 90, 90);
                    tubing[0].LoadedPartScale =
                        new Vector3(1, 1,
                            (width - ((2 + ((i) * 2)) * ((i > 0) ? 1 : 0))) * 0.0254f); //if stationary width goes outside height
                }
                else
                {
                    tubing[0].LoadedPartLocation =
                        new Vector3(0, 0.5f * 0.0254f, 0); //raise model by an inch so 0,0,0 is the absolute bottom.
                    tubing[0].LoadedPartRotation = Quaternion.Euler(0, 90, 90);
                    tubing[0].LoadedPartScale =
                        new Vector3(1, 1,
                            (width - ((2 + ((i) * 2)) * ((i > 0) ? 1 : 0))) * 0.0254f); //if stationary width goes outside height
                }
                
                if (tubing[1] == null)
                {
                    tubing[1] = _modelObjects[i].AddComponent<GeneratePart>();
                    tubing[1].Part = _tubingObject;
                    tubing[1].PartName = "left Upright (" + i + ")";
                    tubing[1].LoadedPartLocation =
                        new Vector3(((width/2.0f) - (0.5f + (1 * (i)))) * -0.0254f, (1.0f + (height/2) - (i >= 1 ? 1 + ((i-1) * 0.5f) : 0) - ((stages - i))) * 0.0254f, 0);
                    tubing[1].LoadedPartRotation = Quaternion.Euler(90, 0, 0);
                    tubing[1].LoadedPartScale =
                        new Vector3(1, 1, (height - (1 * ((i < 2) ? 0 : i-1) - ((stages - i) * -2))) * 0.0254f); 
                }
                else
                {
                    tubing[1].LoadedPartLocation =
                        new Vector3(((width/2.0f) - (0.5f + (1 * (i)))) * -0.0254f, (1.0f + (height/2) - (i >= 1 ? 1 + ((i-1) * 0.5f) : 0) - ((stages - i))) * 0.0254f, 0);
                    tubing[1].LoadedPartRotation = Quaternion.Euler(90, 0, 0);
                    tubing[1].LoadedPartScale =
                        new Vector3(1, 1, (height - (1 * ((i < 2) ? 0 : i-1) - ((stages - i) * -2))) * 0.0254f); 
                }
                
                if (tubing[2] == null)
                {
                    tubing[2] = _modelObjects[i].AddComponent<GeneratePart>();
                    tubing[2].Part = _tubingObject;
                    tubing[2].PartName = "right Upright (" + i + ")";
                    tubing[2].LoadedPartLocation =
                        new Vector3(((width/2.0f) - (0.5f + (1 * (i)))) * 0.0254f, (1.0f + (height/2) - (i >= 1 ? 1 + ((i-1) * 0.5f) : 0) - ((stages - i))) * 0.0254f, 0);
                    tubing[2].LoadedPartRotation = Quaternion.Euler(90, 0, 0);
                    tubing[2].LoadedPartScale =
                        new Vector3(1, 1, (height - (1 * ((i < 2) ? 0 : i-1) - ((stages - i) * -2))) * 0.0254f); 
                }
                else
                {
                    tubing[2].LoadedPartLocation =
                        new Vector3(((width/2.0f) - (0.5f + (1 * (i)))) * 0.0254f, (1.0f + (height/2) - (i >= 1 ? 1 + ((i-1) * 0.5f) : 0) - ((stages - i))) * 0.0254f, 0);
                    tubing[2].LoadedPartRotation = Quaternion.Euler(90, 0, 0);
                    tubing[2].LoadedPartScale =
                        new Vector3(1, 1, (height - (1 * ((i < 2) ? 0 : i-1) - ((stages - i) * -2))) * 0.0254f); 
                }

                if (tubing[3] == null)
                {
                    tubing[3] = _modelObjects[i].AddComponent<GeneratePart>();
                    tubing[3].Part = _tubingObject;
                    tubing[3].PartName = "left cross brace standoff (" + i + ")";
                    tubing[3].LoadedPartLocation =
                        new Vector3(((width/2.0f) - (0.5f + (1 * (i)))) * -0.0254f, (height - (stages) - ((stages - i))) * 0.0254f, -2 * 0.0254f);
                    tubing[3].LoadedPartRotation = Quaternion.Euler(0, 0, 0);
                    tubing[3].LoadedPartScale =
                        new Vector3(1, 1, 2 * 0.0254f); 
                }
                else
                {
                    tubing[3].LoadedPartLocation =
                        new Vector3(((width/2.0f) - (0.5f + (1 * (i)))) * -0.0254f, (height - (stages) - ((stages - i))) * 0.0254f, -2 * 0.0254f);
                    tubing[3].LoadedPartRotation = Quaternion.Euler(0, 0, 0);
                    tubing[3].LoadedPartScale =
                        new Vector3(1, 1, 2 * 0.0254f); 
                }
                
                if (tubing[4] == null)
                {
                    tubing[4] = _modelObjects[i].AddComponent<GeneratePart>();
                    tubing[4].Part = _tubingObject;
                    tubing[4].PartName = "right cross brace standoff (" + i + ")";
                    tubing[4].LoadedPartLocation =
                        new Vector3(((width/2.0f) - (0.5f + (1 * (i)))) * 0.0254f, (height - (stages) - ((stages - i))) * 0.0254f, -2 * 0.0254f);
                    tubing[4].LoadedPartRotation = Quaternion.Euler(0, 0, 0);
                    tubing[4].LoadedPartScale =
                        new Vector3(1, 1, 2 * 0.0254f); 
                }
                else
                {
                    tubing[4].LoadedPartLocation =
                        new Vector3(((width/2.0f) - (0.5f + (1 * (i)))) * 0.0254f, (height - (stages) - ((stages - i))) * 0.0254f, -2 * 0.0254f);
                    tubing[4].LoadedPartRotation = Quaternion.Euler(0, 0, 0);
                    tubing[4].LoadedPartScale =
                        new Vector3(1, 1, 2 * 0.0254f); 
                }

                _tubings[i] = tubing;
            }
        }
    }

    private GeneratePart[] CheckTubes(GameObject parent, float stageNum, int length)
    {
        GeneratePart[] unfiltered = parent.GetComponents<GeneratePart>();
        
        GeneratePart[] filtered = new GeneratePart[length];

        foreach (var t in unfiltered)
        {
            if (t.PartName == "lower crossbar (" + stageNum + ")")
            {
                filtered[0] = t;
            } else if (t.PartName == "left Upright (" + stageNum + ")")
            {
                filtered[1] = t;
            } else if (t.PartName == "right Upright (" + stageNum + ")")
            {
                filtered[2] = t;
            } else if (t.PartName == "left cross brace standoff (" + stageNum + ")")
            {
                filtered[3] = t;
            } else if (t.PartName == "right cross brace standoff (" + stageNum + ")")
            {
                filtered[4] = t;
            }
        }

        return filtered;
    }
}
