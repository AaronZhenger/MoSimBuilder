using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using Util;

[ExecuteAlways]
public class BuildFrame : MonoBehaviour
{
    [Header("Frame Info")]
    [SerializeField] private Vector2 frameSize = new Vector2(29.5f, 29.5f);
    
    [SerializeField] private float robotWeight = 40f;

    [Header("Drive Train Settings")]
    [Tooltip("The simulation is currently hardcoded to Kraken X60s")]
    [SerializeField] private float gearRatio = 5.85f;
    
    [SerializeField] private ModuleType moduleType;

    [Header("Model Settings")] [SerializeField]
    private bool useFrameModel = true;
    
    private GameObject _driveTrain; // the game object all drivetrain spawns are handled under
    
    //Moudle stuff
    private GeneratePart[] _usedModules = new GeneratePart[4]; //caches the modules that are in the world
    
    private GameObject[] _modules = new GameObject[6]; //holds the module types that could be spawned
    
    private float[] _moduleWheelDiameters = new float[6]; //sets the wheel size coresponding to the loaded module num
    
    private string[] _moduleNames = new string[4]; // array of names for the modules
    
    private Vector3[] _cornerModulePositions = new Vector3[4]; //position for cornerbiasedModules
    
    private Vector3[] _standardModulePositions = new Vector3[4];
    
    private Vector3[] _lowProfileModulePositions = new Vector3[4];
    
    private Vector3[] _usedModulePositions = new Vector3[4];

    private Vector3[] _moduleRotations = new Vector3[4];
    
    
    //
    
    private InputActionAsset _inputAsset;
    
    [HideInInspector] public string playerNumber = "Player1";
    
    private SwerveController _swerve;

    private float _unitValue;
    
    // Start is called before the first frame update
    private void Start()
    {
        Startup();

        if (EditorApplication.isPlaying)
        {
            _inputAsset = Resources.Load("Controls/Builder") as InputActionAsset;
            var playerInput = gameObject.AddComponent<PlayerInput>();
            playerInput.actions = _inputAsset;
            playerInput.neverAutoSwitchControlSchemes = true;
            playerInput.defaultControlScheme = playerNumber;
            playerInput.notificationBehavior = PlayerNotifications.InvokeUnityEvents;
            var rb = gameObject.AddComponent<Rigidbody>();
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.mass = robotWeight;
            rb.drag = 0.5f;
            rb.angularDrag = 0.05f;
            _swerve = gameObject.AddComponent<SwerveController>();
            _swerve.rb = rb;
            _swerve.gearRatio = gearRatio;
            _swerve.wheelDiameter = _moduleWheelDiameters[(int)moduleType];
        }
    }

    private void Awake()
    {
        Startup();
    }

    // Update is called once per frame
    private void Update()
    {

        //load module models
        var loadedModules =  Resources.LoadAll<GameObject>("Swerve") as GameObject[];

        foreach (var loadedModule in loadedModules)
        {
            if (loadedModule.name ==  ModuleType.invertedCorner.ToString())
            {
                _modules[0] = loadedModule;
            } else if (loadedModule.name == ModuleType.standardCorner.ToString())
            {
                _modules[1] = loadedModule;
            } else if (loadedModule.name == ModuleType.inverted.ToString())
            {
                _modules[2] = loadedModule;
            } else if (loadedModule.name == ModuleType.standard.ToString())
            {
                _modules[3] = loadedModule;
            } else if (loadedModule.name == ModuleType.inverted.ToString())
            {
                _modules[4] = loadedModule;
            } else if (loadedModule.name == ModuleType.lowProfile.ToString())
            {
                _modules[5] = loadedModule;
            }
        }
        
        if (_driveTrain == null)
        {
            _driveTrain = new GameObject
            {
                name = "driveTrain",
                transform =
                {
                    parent = transform,
                    localPosition = Vector3.zero,
                    localRotation = Quaternion.identity,
                    localScale = Vector3.one
                }
            };
        }

        //set module locations
        _cornerModulePositions[0] = new Vector3((frameSize.x * -0.5f) + 2.15f, 0, frameSize.y * 0.5f - 2.15f);
        _cornerModulePositions[1] = new Vector3(frameSize.x * 0.5f - 2.15f, 0, frameSize.y * 0.5f - 2.15f);
        _cornerModulePositions[2] = new Vector3(frameSize.x * -0.5f + 2.15f, 0, frameSize.y * -0.5f + 2.15f);
        _cornerModulePositions[3] = new Vector3(frameSize.x * 0.5f - 2.15f, 0, frameSize.y * -0.5f + 2.15f);
        
        _standardModulePositions[0] = new Vector3((frameSize.x * -0.5f) + 3.15f, 0, frameSize.y * 0.5f - 3.15f);
        _standardModulePositions[1] = new Vector3(frameSize.x * 0.5f - 3.15f, 0, frameSize.y * 0.5f - 3.15f);
        _standardModulePositions[2] = new Vector3(frameSize.x * -0.5f + 3.15f, 0, frameSize.y * -0.5f + 3.15f);
        _standardModulePositions[3] = new Vector3(frameSize.x * 0.5f - 3.15f, 0, frameSize.y * -0.5f + 3.15f);
        
        _lowProfileModulePositions[0] = new Vector3((frameSize.x * -0.5f) + 1.25f, 0, frameSize.y * 0.5f - 1.25f);
        _lowProfileModulePositions[1] = new Vector3(frameSize.x * 0.5f - 1.25f, 0, frameSize.y * 0.5f - 1.25f);
        _lowProfileModulePositions[2] = new Vector3(frameSize.x * -0.5f + 1.25f, 0, frameSize.y * -0.5f + 1.25f);
        _lowProfileModulePositions[3] = new Vector3(frameSize.x * 0.5f -1.25f, 0, frameSize.y * -0.5f + 1.25f);

        _usedModulePositions = moduleType switch
        {
            ModuleType.invertedCorner => _cornerModulePositions,
            ModuleType.standardCorner => _cornerModulePositions,
            ModuleType.inverted => _standardModulePositions,
            ModuleType.standard => _standardModulePositions,
            ModuleType.lowProfile => _lowProfileModulePositions,
            _ => _usedModulePositions
        };

        _moduleRotations[0] = new Vector3(0, 0, 0);
        _moduleRotations[1] = new Vector3(0, 90, 0);
        _moduleRotations[2] = new Vector3(0, 270, 0);
        _moduleRotations[3] = new Vector3(0, 180, 0);

        //generate and check modules
        for (int i = 0; i < _usedModules.Length; i++)
        {
            if (_usedModules[i] == null)
            {
                _usedModules[i] = _driveTrain.AddComponent<GeneratePart>();
                
                _usedModules[i].Part = _modules[(int)moduleType];

                _usedModules[i].PartName = _moduleNames[i];

                _usedModules[i].LoadedPartLocation = new Vector3();

                _usedModules[i].LoadedPartRotation = Quaternion.identity;

                _usedModules[i].LoadedPartScale = Vector3.one;
            } 
            else if (_usedModules[i].Part != _modules[(int)moduleType])
            {
                _usedModules[i].Part = _modules[(int)moduleType];

                _usedModules[i].PartName = _moduleNames[i];

                _usedModules[i].LoadedPartLocation = new Vector3();

                _usedModules[i].LoadedPartRotation = Quaternion.identity;

                _usedModules[i].LoadedPartScale = Vector3.one;
            }
            else if (_usedModules[i] != null)
            {
                _usedModules[i].LoadedPartLocation = _usedModulePositions[i] * 0.0254f;

                _usedModules[i].LoadedPartRotation = Quaternion.Euler(_moduleRotations[i]);
                        
                _usedModules[i].LoadedPartScale = Vector3.one;
            }
        }
    }

    private void Startup()
    {
        //load module models
        var loadedModules =  Resources.LoadAll<GameObject>("Swerve") as GameObject[];

        foreach (var loadedModule in loadedModules)
        {
            if (loadedModule.name ==  ModuleType.invertedCorner.ToString())
            {
                _modules[0] = loadedModule;
            } else if (loadedModule.name == ModuleType.standardCorner.ToString())
            {
                _modules[1] = loadedModule;
            } else if (loadedModule.name == ModuleType.inverted.ToString())
            {
                _modules[2] = loadedModule;
            } else if (loadedModule.name == ModuleType.standard.ToString())
            {
                _modules[3] = loadedModule;
            } else if (loadedModule.name == ModuleType.lowProfile.ToString())
            {
                _modules[4] = loadedModule;
            }
        }

        _moduleWheelDiameters[0] = 4;
        _moduleWheelDiameters[1] = 4;
        _moduleWheelDiameters[2] = 4;
        _moduleWheelDiameters[3] = 4;
        _moduleWheelDiameters[4] = 3;

        //set modules names
        _moduleNames[0] = "lf";
        _moduleNames[1] = "rf";
        _moduleNames[2] = "lr";
        _moduleNames[3] = "rr";

        //set conversion unit (TEMP)
        _unitValue = 0.0254f;

        //find generated objects at startup
        _driveTrain = Utils.FindChild("driveTrain", gameObject);
        
        if (_driveTrain != null)
        {
            var generatedParts = _driveTrain.GetComponents<GeneratePart>();
            for (int t = 0; t < _usedModules.Length; t++)
            {
                foreach (var part in generatedParts)
                {
                    if (part.PartName == _moduleNames[t])
                    {
                        _usedModules[t] = part;
                    }
                }
            }
            
        }

        if (gameObject.GetComponent<SwerveController>())
        {
            _swerve = gameObject.GetComponent<SwerveController>();
        }

        if (_swerve != null && _driveTrain != null)
        {
            _swerve.gearRatio = gearRatio;
            _swerve.wheelDiameter = _moduleWheelDiameters[(int)moduleType];
        }
    }
}
