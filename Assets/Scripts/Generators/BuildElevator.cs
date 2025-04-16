using System.Collections;
using System.Collections.Generic;
using MyBox;
using UnityEditor;
using UnityEngine;
using UnityEngine.Audio;
using Util;
using Random = Unity.Mathematics.Random;

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
    [ConditionalField(nameof(model), false)]
    [SerializeField] private bool carriage = true;
    [ConditionalField(nameof(carriage), false)]
    [SerializeField] private float carriageHeight;

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
    
    private GameObject[] _modelObjects;
    
    private Rigidbody[] _rigidbodies;
    
    private ConfigurableJoint[] _joints;

    private JointDrive[] _drives;
    
    private JointController[] _controllers;

    bool[] _engaged;
    
    private bool[] wasEngaged;
    
    private bool wasCarriage;
    
    private AudioSource _audioSource;
    
    [SerializeField]  AudioResource[] _audioClips;
    
    
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
        
        var loadedSounds =  Resources.LoadAll<AudioResource>("Sounds") as AudioResource[];

        _audioClips = new AudioResource[2];
        foreach (var loadedSound in loadedSounds)
        {
            if (loadedSound.name == "ElevatorClick")
            {
                _audioClips[0] = loadedSound;
            }
            else if (loadedSound.name == "ElevatorClick2")
            {
                _audioClips[1] = loadedSound;
            }
        }
        
        
        _engaged = new bool[stages];
        wasEngaged = new bool[stages];

        for (int i = 0; i < _engaged.Length; i++)
        {
            _engaged[i] = false;   
            wasEngaged[i] = false;
                
        }

        scaleFactor = 0.0254f;
        
        _modelObjects = new GameObject[stages +1];
        for (int i = 0; i <= stages; i++)
        {
            _modelObjects[i] = Utils.FindChild("Stage" + i, gameObject).gameObject;
        }
    }

    private void Initialize()
    { 
        GenerateRBs();
        GenerateJoints();
        generateControllers();

        _audioSource = gameObject.AddComponent<AudioSource>();
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
            ContinuousClick();
            ContinuousMovement();
        }
    }

    /// <summary>
    /// Sets stages to the correct height for a continuous Elevator rigging
    /// </summary>
    private void ContinuousMovement()
    {
        //TODO: add the continuously rigged motion to this function
        
        for (int i = 0; i < _rigidbodies.Length; i++)
        {
            if (i == _rigidbodies.Length - 1)
            {
                _controllers[i].setPoints = setPoints;
                _controllers[i].currentPosition = _rigidbodies[i].transform.localPosition.y - ((i+1) * 0.0254f);
                _controllers[i].follower = false;
                continue; //skip follower calculations
            }
            else
            {
                _controllers[i].follower = true;
            }

            //there are so many things wrong with this but PLEASE just leave it. I have lost so much time trying to 
            //trying to make it not jank.
            float combinedHeight = 0;
            for (int j = i+1; j < _rigidbodies.Length; j++)
            { 
                combinedHeight += ((height) - (1 * ((j < 2) ? 0 : j - 1) - ((stages - j) * -2))) * 0.0254f;
            }

            float heightOffset = 0;
            if (carriage)
            {
                heightOffset = -(carriageHeight + 1) * 0.0254f;
            }

            float setPoint = 0;
            if (combinedHeight < _rigidbodies[stages-1].transform.localPosition.y - (i * 0.0254f) - heightOffset)
            {
                _engaged[i] = true; //audio thingy
                setPoint = combinedHeight - _rigidbodies[stages-1].transform.localPosition.y - ((i+1) * 0.0254f);
                
                setPoint -= 6 * 0.0254f;

                if (carriage && stages - 2 == i)
                {
                    setPoint += (6 + i) * 0.0254f;
                }
            }
            else
            {
                setPoint = 0;
                _engaged[i] = false;
            }
            
            setPoint = -setPoint;

            if (setPoint <= 0)
            {
                setPoint = 0;
            }
            
            _controllers[i].FollowPosition(setPoint);
            _controllers[i].currentPosition = _rigidbodies[i].transform.localPosition.y - ((i+1) * 0.0254f);
        }
    }

    /// <summary>
    /// Ques a random click when stages engage/disengage
    /// </summary>
    private void ContinuousClick()
    {
        for (int i = 0; i < _engaged.Length; i++)
        {
            if (wasEngaged[i] != _engaged[i])
            {
                RandomClick();
            }
            
            wasEngaged[i] = _engaged[i];
        }
    }

    /// <summary>
    /// Plays a random click
    /// </summary>
    private void RandomClick()
    {
        var random = new Random(100);

        // Generate a random integer between 1 (inclusive) and 3 (exclusive).
        int randomNumber = random.NextInt(1, 3);

        if (randomNumber == 1)
        {
            _audioSource.resource = _audioClips[0];
            
        } else if (randomNumber == 2)
        {
            _audioSource.resource = _audioClips[1];
        }

        _audioSource.pitch = 1.25f;
        
        _audioSource.Play();
    }

    /// <summary>
    /// Set stages to correct locations for a cascade rigged elevator
    /// </summary>
    private void cascadeMovement()
    {
        //TODO: add the cascade rigged motion to this function
        for (int i = 0; i < _rigidbodies.Length; i++)
        {
            if (i == _rigidbodies.Length - 1)
            {
                _controllers[i].follower = false;
                _controllers[i].setPoints = setPoints;
            }
            else
            {
                _controllers[i].follower = true;
                
                float target = _rigidbodies[^1].transform.localPosition.y;

                float count = 0;
                for (int j = i + 1; j < _rigidbodies.Length; j++)
                {
                    count += 1;
                }
                target = target / (count * 1.5f);
                _controllers[i].FollowPosition(target);
            }
        }
    }
    
    /// <summary>
    /// Generates Rigidbodies at startup
    /// </summary>
    private void GenerateRBs()
    {
        _rigidbodies = new Rigidbody[_modelObjects.Length - 1]; //stationary stage doesnt have a rb
        for (int i = 0; i < _modelObjects.Length-1; i++) //skip the stationary stage (0)
        {
            _rigidbodies[i] = _modelObjects[i+1].AddComponent<Rigidbody>();

            _rigidbodies[i].mass = 1;
            _rigidbodies[i].drag = 0;
            _rigidbodies[i].angularDrag = 0;
            _rigidbodies[i].useGravity = true;
            _rigidbodies[i].isKinematic = false;
            _rigidbodies[i].interpolation = RigidbodyInterpolation.Interpolate;
            _rigidbodies[i].collisionDetectionMode = CollisionDetectionMode.Continuous;
        }
    }
    
    /// <summary>
    /// Generates the Joints at startup
    /// </summary>
    private void GenerateJoints()
    {
        var driveTrain = Utils.FindParentRB(gameObject).GetComponent<Rigidbody>();
        _joints = new ConfigurableJoint[_modelObjects.Length - 1]; //stationary stage doesnt have a rb
        _drives = new JointDrive[_modelObjects.Length - 1];
        for (int i = 0; i < _modelObjects.Length-1; i++) //skip the stationary stage (0)
        {
            _joints[i] = _modelObjects[i+1].AddComponent<ConfigurableJoint>();

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

    /// <summary>
    /// Generates the joint controllers at startup
    /// </summary>
    private void generateControllers()
    {
        _controllers = new JointController[_modelObjects.Length - 1];
        for (int i = 0; i < _modelObjects.Length -1; i++)
        {
            _controllers[i] = _modelObjects[i+1].AddComponent<JointController>();

            _controllers[i].p = 5;
            _controllers[i].i = 0;
            _controllers[i].d = 0.0005f;
            _controllers[i].iSat = 0;
            _controllers[i].max = 5;
            _controllers[i].angular = false;
            _controllers[i].driveAxis = new Vector3(0, 1, 0);
            _controllers[i].joint = _joints[i];

            if (i != _modelObjects.Length - 2)
            {
                _controllers[i].p = 50;
                _controllers[i].i = 0;
                _controllers[i].d = 0.005f;
                _controllers[i].iSat = 0;
                _controllers[i].max = 50;
            }
        }
    }
    
    /// <summary>
    /// Generates a joint drive
    /// </summary>
    /// <param name="i"></param>
    private void GenerateDrive(int i)
    {
        _drives[i].maximumForce = 8000000;
        _drives[i].positionDamper = 10000;
        _drives[i].positionSpring = 0;
        _drives[i].useAcceleration = false;
        _joints[i].yDrive = _drive;
    }

    //generates the standard elevator model.
    private void BuildModel()
    {
        if (wasCarriage != carriage)
        {
            foreach (var modelObject in _modelObjects)
            {
                if (_modelObjects != null)
                {
                    DestroyImmediate(modelObject.gameObject);
                }
            }
        }
        int nonCrossBraceStages = 1;
        if (carriage)
        {
            nonCrossBraceStages = 2;
        }
        
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
            if (i <= stages-nonCrossBraceStages || i == 0) //determines cross brace stage models
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

                var tubing = CheckTubes(_modelObjects[i], i, 6);
                    
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
                            (width - ((2 + ((i) * 2)) * ((i > 0) ? 1.25f : 0))) * 0.0254f); //if stationary width goes outside height
                }
                else
                {
                    tubing[0].LoadedPartLocation =
                        new Vector3(0, 0.5f * 0.0254f, 0); //raise model by an inch so 0,0,0 is the absolute bottom.
                    tubing[0].LoadedPartRotation = Quaternion.Euler(0, 90, 90);
                    tubing[0].LoadedPartScale =
                        new Vector3(1, 1,
                            (width - ((2 + ((i) * 2)) * ((i > 0) ? 1.25f : 0))) * 0.0254f); //if stationary width goes outside height
                }
                
                if (tubing[1] == null)
                {
                    tubing[1] = _modelObjects[i].AddComponent<GeneratePart>();
                    tubing[1].Part = _tubingObject;
                    tubing[1].PartName = "left Upright (" + i + ")";
                    tubing[1].LoadedPartLocation =
                        new Vector3(((width/2.0f) - (0.5f + (1.5f * (i)))) * -0.0254f, (1.0f + (height/2) - (i >= 1 ? 1 + ((i-1) * 0.5f) : 0) - ((stages - (carriage? 1:0) - i))) * 0.0254f, 0);
                    tubing[1].LoadedPartRotation = Quaternion.Euler(90, 0, 0);
                    tubing[1].LoadedPartScale =
                        new Vector3(1, 1, (height - (1 * ((i < 2) ? 0 : i-1) - ((stages - (carriage? 1:0) - i) * -2))) * 0.0254f); 
                }
                else
                {
                    tubing[1].LoadedPartLocation =
                        new Vector3(((width/2.0f) - (0.5f + (1.5f * (i)))) * -0.0254f, (1.0f + (height/2) - (i >= 1 ? 1 + ((i-1) * 0.5f) : 0) - ((stages - (carriage? 1:0) - i))) * 0.0254f, 0);
                    tubing[1].LoadedPartRotation = Quaternion.Euler(90, 0, 0);
                    tubing[1].LoadedPartScale =
                        new Vector3(1, 1, (height - (1 * ((i < 2) ? 0 : i-1) - ((stages - (carriage? 1:0) - i) * -2))) * 0.0254f); 
                }
                
                if (tubing[2] == null)
                {
                    tubing[2] = _modelObjects[i].AddComponent<GeneratePart>();
                    tubing[2].Part = _tubingObject;
                    tubing[2].PartName = "right Upright (" + i + ")";
                    tubing[2].LoadedPartLocation =
                        new Vector3(((width/2.0f) - (0.5f + (1.5f * (i)))) * 0.0254f, (1.0f + (height/2) - (i >= 1 ? 1 + ((i-1) * 0.5f) : 0) - ((stages - (carriage? 1:0) - i))) * 0.0254f, 0);
                    tubing[2].LoadedPartRotation = Quaternion.Euler(90, 0, 0);
                    tubing[2].LoadedPartScale =
                        new Vector3(1, 1, (height - (1 * ((i < 2) ? 0 : i-1) - ((stages - (carriage? 1:0) - i) * -2))) * 0.0254f); 
                }
                else
                {
                    tubing[2].LoadedPartLocation =
                        new Vector3(((width/2.0f) - (0.5f + (1.5f * (i)))) * 0.0254f, (1.0f + (height/2) - (i >= 1 ? 1 + ((i-1) * 0.5f) : 0) - ((stages - (carriage? 1:0) - i))) * 0.0254f, 0);
                    tubing[2].LoadedPartRotation = Quaternion.Euler(90, 0, 0);
                    tubing[2].LoadedPartScale =
                        new Vector3(1, 1, (height - (1 * ((i < 2) ? 0 : i-1) - ((stages - (carriage? 1:0) - i) * -2))) * 0.0254f); 
                }

                if (tubing[3] == null)
                {
                    tubing[3] = _modelObjects[i].AddComponent<GeneratePart>();
                    tubing[3].Part = _tubingObject;
                    tubing[3].PartName = "left cross brace standoff (" + i + ")";
                    tubing[3].LoadedPartLocation =
                        new Vector3(((width/2.0f) - (0.5f + (1 * (i)))) * -0.0254f, (height - (stages - (carriage? 1:0)) - ((stages - (carriage? 1:0) - i))) * 0.0254f, -2 * 0.0254f);
                    tubing[3].LoadedPartRotation = Quaternion.Euler(0, 0, 0);
                    tubing[3].LoadedPartScale =
                        new Vector3(1, 1, 2 * 0.0254f); 
                }
                else
                {
                    tubing[3].LoadedPartLocation =
                        new Vector3(((width/2.0f) - (0.5f + (1.5f * (i)))) * -0.0254f, (height - (stages - (carriage? 1:0)) - ((stages - (carriage? 1:0) - i))) * 0.0254f, -2 * 0.0254f);
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
                        new Vector3(((width/2.0f) - (0.5f + (1.5f * (i)))) * 0.0254f, (height - (stages - (carriage? 1:0)) - ((stages - (carriage? 1:0) - i))) * 0.0254f, -2 * 0.0254f);
                    tubing[4].LoadedPartRotation = Quaternion.Euler(0, 0, 0);
                    tubing[4].LoadedPartScale =
                        new Vector3(1, 1, 2 * 0.0254f); 
                }
                else
                {
                    tubing[4].LoadedPartLocation =
                        new Vector3(((width/2.0f) - (0.5f + (1.5f * (i)))) * 0.0254f, (height - (stages - (carriage? 1:0)) - ((stages - (carriage? 1:0) - i))) * 0.0254f, -2 * 0.0254f);
                    tubing[4].LoadedPartRotation = Quaternion.Euler(0, 0, 0);
                    tubing[4].LoadedPartScale =
                        new Vector3(1, 1, 2 * 0.0254f); 
                }
                
                if (tubing[5] == null)
                {
                    tubing[5] = _modelObjects[i].AddComponent<GeneratePart>();
                    tubing[5].Part = _tubingObject;
                    tubing[5].PartName = "upper cross brace (" + i + ")";
                    tubing[5].LoadedPartLocation =
                        new Vector3(0, (height - (stages - (carriage? 1:0)) - ((stages - (carriage? 1:0) - i))) * 0.0254f, -3.5f * 0.0254f);
                    tubing[5].LoadedPartRotation = Quaternion.Euler(0, 90, 0);
                    tubing[5].LoadedPartScale =
                        new Vector3(1,1, (width - ((2 + ((i) * 2)) * (1)) + 2) * 0.0254f); 
                }
                else
                {
                    tubing[5].LoadedPartLocation =
                        new Vector3(0, (height - (stages - (carriage? 1:0)) - ((stages - (carriage? 1:0) - i))) * 0.0254f, -3.5f * 0.0254f);
                    tubing[5].LoadedPartRotation = Quaternion.Euler(0, 90, 0);
                    tubing[5].LoadedPartScale =
                        new Vector3(1,1, (width - ((2 + ((i) * 2) * 1.5f) * (1)) + 2) * 0.0254f); 
                }

                _tubings[i] = tubing;
            }
            else if (carriage && i == stages)
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

                var tubing = CheckTubes(_modelObjects[i], i, 4);
                    
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
                            (width - ((2 + ((i) * 2)) * ((i > 0) ? 1.25f : 0))) * 0.0254f); //if stationary width goes outside height
                }
                else
                {
                    tubing[0].LoadedPartLocation =
                        new Vector3(0, 0.5f * 0.0254f, 0); //raise model by an inch so 0,0,0 is the absolute bottom.
                    tubing[0].LoadedPartRotation = Quaternion.Euler(0, 90, 90);
                    tubing[0].LoadedPartScale =
                        new Vector3(1, 1,
                            (width - ((2 + ((i) * 2)) * ((i > 0) ? 1.25f : 0))) * 0.0254f); //if stationary width goes outside height
                }
                
                if (tubing[1] == null)
                {
                    tubing[1] = _modelObjects[i].AddComponent<GeneratePart>();
                    tubing[1].Part = _tubingObject;
                    tubing[1].PartName = "left Upright (" + i + ")";
                    tubing[1].LoadedPartLocation =
                        new Vector3(((width/2.0f) - (0.5f + (1.5f * (i)))) * -0.0254f, (1.0f + (carriageHeight/2) - (i >= 1 ? 1 + ((i-1) * 0.5f) : 0) - ((stages - i))) * 0.0254f, 0);
                    tubing[1].LoadedPartRotation = Quaternion.Euler(90, 0, 0);
                    tubing[1].LoadedPartScale =
                        new Vector3(1, 1, (carriageHeight - (1 * ((i < 2) ? 0 : i-1) - ((stages - i) * -2))) * 0.0254f); 
                }
                else
                {
                    tubing[1].LoadedPartLocation =
                        new Vector3(((width/2.0f) - (0.5f + (1.5f * (i)))) * -0.0254f, (1.0f + (carriageHeight/2) - (i >= 1 ? 1 + ((i-1) * 0.5f) : 0) - ((stages - i))) * 0.0254f, 0);
                    tubing[1].LoadedPartRotation = Quaternion.Euler(90, 0, 0);
                    tubing[1].LoadedPartScale =
                        new Vector3(1, 1, (carriageHeight - (1 * ((i < 2) ? 0 : i-1) - ((stages - i) * -2))) * 0.0254f); 
                }
                
                if (tubing[2] == null)
                {
                    tubing[2] = _modelObjects[i].AddComponent<GeneratePart>();
                    tubing[2].Part = _tubingObject;
                    tubing[2].PartName = "right Upright (" + i + ")";
                    tubing[2].LoadedPartLocation =
                        new Vector3(((width/2.0f) - (0.5f + (1.5f * (i)))) * 0.0254f, (1.0f + (carriageHeight/2) - (i >= 1 ? 1 + ((i-1) * 0.5f) : 0) - ((stages - i))) * 0.0254f, 0);
                    tubing[2].LoadedPartRotation = Quaternion.Euler(90, 0, 0);
                    tubing[2].LoadedPartScale =
                        new Vector3(1, 1, (carriageHeight - (1 * ((i < 2) ? 0 : i-1) - ((stages - i) * -2))) * 0.0254f); 
                }
                else
                {
                    tubing[2].LoadedPartLocation =
                        new Vector3(((width/2.0f) - (0.5f + (1.5f * (i)))) * 0.0254f, (1.0f + (carriageHeight/2) - (i >= 1 ? 1 + ((i-1) * 0.5f) : 0) - ((stages - i))) * 0.0254f, 0);
                    tubing[2].LoadedPartRotation = Quaternion.Euler(90, 0, 0);
                    tubing[2].LoadedPartScale =
                        new Vector3(1, 1, (carriageHeight - (1 * ((i < 2) ? 0 : i-1) - ((stages - i) * -2))) * 0.0254f); 
                }
                
                if (tubing[3] == null)
                {
                    tubing[3] = _modelObjects[i].AddComponent<GeneratePart>();
                    tubing[3].Part = _tubingObject;
                    tubing[3].PartName = "upper cross brace (" + i + ")";
                    tubing[3].LoadedPartLocation =
                        new Vector3(((width/2.0f) - (0.5f + (1 * (i)))) * 0.0254f, (1.0f + (carriageHeight/2) - (i >= 1 ? 1 + ((i-1) * 0.5f) : 0) - ((stages - i))) * 0.0254f, 0);
                    tubing[3].LoadedPartRotation = Quaternion.Euler(90, 0, 0);
                    tubing[3].LoadedPartScale =
                        new Vector3(1, 1, (carriageHeight - (1 * ((i < 2) ? 0 : i-1) - ((stages - i) * -2))) * 0.0254f); 
                }
                else
                {
                    tubing[3].LoadedPartLocation =
                        new Vector3(0, (carriageHeight - (stages) - ((stages - i))+0.5f) * 0.0254f, 0);
                    tubing[3].LoadedPartRotation = Quaternion.Euler(0, 90, 90);
                    tubing[3].LoadedPartScale =
                        new Vector3(1,1, (width - ((2 + ((i) * 2)*1.5f) * (1))) * 0.0254f); 
                }
            }
            else if (stages - i <= nonCrossBraceStages || !carriage)
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

                var tubing = CheckTubes(_modelObjects[i], i, 4);
                    
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
                            (width - ((2 + ((i) * 2)) * ((i > 0) ? 1 : 0) * 1.25f)) * 0.0254f); //if stationary width goes outside height
                }
                else
                {
                    tubing[0].LoadedPartLocation =
                        new Vector3(0, 0.5f * 0.0254f, 0); //raise model by an inch so 0,0,0 is the absolute bottom.
                    tubing[0].LoadedPartRotation = Quaternion.Euler(0, 90, 90);
                    tubing[0].LoadedPartScale =
                        new Vector3(1, 1,
                            (width - ((2 + ((i) * 2)) * ((i > 0) ? 1 : 0) * 1.25f)) * 0.0254f); //if stationary width goes outside height
                }
                
                if (tubing[1] == null)
                {
                    tubing[1] = _modelObjects[i].AddComponent<GeneratePart>();
                    tubing[1].Part = _tubingObject;
                    tubing[1].PartName = "left Upright (" + i + ")";
                    tubing[1].LoadedPartLocation =
                        new Vector3(((width/2.0f) - (0.5f + (1.5f * (i)))) * -0.0254f, (1.0f + (height/2) - (i >= 1 ? 1 + ((i-1) * 0.5f) : 0) - ((stages - (carriage? 1:0) - i))) * 0.0254f, 0);
                    tubing[1].LoadedPartRotation = Quaternion.Euler(90, 0, 0);
                    tubing[1].LoadedPartScale =
                        new Vector3(1, 1, (height - (1 * ((i < 2) ? 0 : i-1) - ((stages - (carriage? 1:0) - i) * -2))) * 0.0254f); 
                }
                else
                {
                    tubing[1].LoadedPartLocation =
                        new Vector3(((width/2.0f) - (0.5f + (1.5f * (i)))) * -0.0254f, (1.0f + (height/2) - (i >= 1 ? 1 + ((i-1) * 0.5f) : 0) - ((stages - (carriage? 1:0) - i))) * 0.0254f, 0);
                    tubing[1].LoadedPartRotation = Quaternion.Euler(90, 0, 0);
                    tubing[1].LoadedPartScale =
                        new Vector3(1, 1, (height - (1 * ((i < 2) ? 0 : i-1) - ((stages - (carriage? 1:0) - i) * -2))) * 0.0254f); 
                }
                
                if (tubing[2] == null)
                {
                    tubing[2] = _modelObjects[i].AddComponent<GeneratePart>();
                    tubing[2].Part = _tubingObject;
                    tubing[2].PartName = "right Upright (" + i + ")";
                    tubing[2].LoadedPartLocation =
                        new Vector3(((width/2.0f) - (0.5f + (1.5f * (i)))) * 0.0254f, (1.0f + (height/2) - (i >= 1 ? 1 + ((i-1) * 0.5f) : 0) - ((stages - (carriage? 1:0) - i))) * 0.0254f, 0);
                    tubing[2].LoadedPartRotation = Quaternion.Euler(90, 0, 0);
                    tubing[2].LoadedPartScale =
                        new Vector3(1, 1, (height - (1 * ((i < 2) ? 0 : i-1) - ((stages - (carriage? 1:0) - i) * -2))) * 0.0254f); 
                }
                else
                {
                    tubing[2].LoadedPartLocation =
                        new Vector3(((width/2.0f) - (0.5f + (1.5f * (i)))) * 0.0254f, (1.0f + (height/2) - (i >= 1 ? 1 + ((i-1) * 0.5f) : 0) - ((stages - (carriage? 1:0) - i))) * 0.0254f, 0);
                    tubing[2].LoadedPartRotation = Quaternion.Euler(90, 0, 0);
                    tubing[2].LoadedPartScale =
                        new Vector3(1, 1, (height - (1 * ((i < 2) ? 0 : i-1) - ((stages - (carriage? 1:0) - i) * -2))) * 0.0254f); 
                }
                
                if (tubing[3] == null)
                {
                    tubing[3] = _modelObjects[i].AddComponent<GeneratePart>();
                    tubing[3].Part = _tubingObject;
                    tubing[3].PartName = "upper cross brace (" + i + ")";
                    tubing[3].LoadedPartLocation =
                        new Vector3(((width/2.0f) - (0.5f + (1.5f * (i)))) * 0.0254f, (1.0f + (height/2) - (i >= 1 ? 1 + ((i-1) * 0.5f) : 0) - ((stages - (carriage? 1:0) - i))) * 0.0254f, 0);
                    tubing[3].LoadedPartRotation = Quaternion.Euler(90, 0, 0);
                    tubing[3].LoadedPartScale =
                        new Vector3(1, 1, (height - (1 * ((i < 2) ? 0 : i-1) - ((stages - (carriage? 1:0) - i) * -2))) * 0.0254f); 
                }
                else
                {
                    tubing[3].LoadedPartLocation =
                        new Vector3(0, (height - (stages - (carriage? 1:0)) - ((stages - (carriage? 1:0) - i))+0.5f) * 0.0254f, 0);
                    tubing[3].LoadedPartRotation = Quaternion.Euler(0, 90, 90);
                    tubing[3].LoadedPartScale =
                        new Vector3(1,1, (width - ((2 + ((i) * 2)) * (1))) * 0.0254f); 
                }
                
            }
        }
        
        wasCarriage = carriage;
    }

    /// <summary>
    /// identifies what models are present on an object and returns the sorted list.
    /// </summary>
    /// <param name="parent"></param>
    /// <param name="stageNum"></param>
    /// <param name="length"></param>
    /// <param name="crossBrace"></param>
    /// <returns></returns>
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
            } else if (t.PartName == "upper cross brace (" + stageNum + ")" && length != 4)
            {
                filtered[5] = t;
            } else if (t.PartName == "upper cross brace (" + stageNum + ")") 
            {
                filtered[3] = t;
            }
        }

        return filtered;
    }
}
