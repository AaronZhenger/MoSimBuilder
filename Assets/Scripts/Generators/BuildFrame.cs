using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
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
    
    private GameObject[] _modules = new GameObject[1]; //holds the module types that could be spawned
    
    private string[] _moduleNames = new string[4]; // array of names for the modules
    
    private Vector3[] _cornerModulePositions = new Vector3[4]; //position for cornerbiasedModules

    private Vector3[] _moduleRotations = new Vector3[4];
    
    private float[] _moduleWheelDiameters = new float[1];
    //
    
    private InputActionAsset inputAsset;
    
    [HideInInspector] public string PlayerNumber = "Player1";
    
    private SwerveController swerve;

    private float UnitValue;
    
    // Start is called before the first frame update
    private void Start()
    {
        Startup();

        if (EditorApplication.isPlaying)
        {
            inputAsset = Resources.Load("Controls/Builder") as InputActionAsset;
            var playerInput = gameObject.AddComponent<PlayerInput>();
            playerInput.actions = inputAsset;
            playerInput.neverAutoSwitchControlSchemes = true;
            playerInput.defaultControlScheme = PlayerNumber;
            playerInput.notificationBehavior = PlayerNotifications.InvokeUnityEvents;
            var rb = gameObject.AddComponent<Rigidbody>();
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.mass = robotWeight;
            rb.drag = 0.5f;
            rb.angularDrag = 0.05f;
            swerve = gameObject.AddComponent<SwerveController>();
            swerve.rb = rb;
            swerve.gearRatio = gearRatio;
            swerve.wheelDiameter = _moduleWheelDiameters[(int)moduleType];
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
        _cornerModulePositions[0] = new Vector3((frameSize.x * -0.5f) + 2.63f, 0, frameSize.y * 0.5f - 2.63f);
        _cornerModulePositions[1] = new Vector3(frameSize.x * 0.5f - 2.63f, 0, frameSize.y * 0.5f - 2.63f);
        _cornerModulePositions[2] = new Vector3(frameSize.x * -0.5f + 2.63f, 0, frameSize.y * -0.5f + 2.63f);
        _cornerModulePositions[3] = new Vector3(frameSize.x * 0.5f - 2.63f, 0, frameSize.y * -0.5f + 2.63f);

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
            else if (_usedModules[i] != null)
            {
                _usedModules[i].LoadedPartLocation = _cornerModulePositions[i] * 0.0254f;

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
            }
        }

        _moduleWheelDiameters[0] = 4;

        //set modules names
        _moduleNames[0] = "lf";
        _moduleNames[1] = "rf";
        _moduleNames[2] = "lr";
        _moduleNames[3] = "rr";

        //set conversion unit (TEMP)
        UnitValue = 0.0254f;

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
            swerve = gameObject.GetComponent<SwerveController>();
        }

        if (swerve != null && _driveTrain != null)
        {
            swerve.gearRatio = gearRatio;
            swerve.wheelDiameter = _moduleWheelDiameters[(int)moduleType];
        }
    }
}
