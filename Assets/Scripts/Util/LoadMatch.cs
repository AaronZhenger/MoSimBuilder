using System;
using System.Collections.Generic;
using System.Linq;
using MyBox;
using NUnit.Framework;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using UnityEngine.UIElements;
using Util;

[ExecuteAlways]
public class LoadMatch : MonoBehaviour
{
    [SerializeField] private GameObject[] fieldPrefab;

    [Header("Player 1 (Blue Alliance)")]
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private InspectorDropdown robotSelected;
    [SerializeField] private Cameras view;
    [ConditionalField(true, nameof(isDriverStation))] [SerializeField]
    private StationNum stationNumber;
    [ConditionalField(true, nameof(isDriverStation))] [SerializeField]
    private TrackingType trackingType;

    [Header("Multiplayer")]
    [SerializeField] private bool enableSplitScreen = false;

    [ConditionalField(nameof(enableSplitScreen))] [Header("Player 2 (Red Alliance)")]
    [SerializeField] private Transform spawnPoint2;
    [ConditionalField(nameof(enableSplitScreen))] [SerializeField]
    private InspectorDropdown robotSelected2;
    [ConditionalField(nameof(enableSplitScreen))] [SerializeField]
    private Cameras view2;
    [ConditionalField(true, nameof(isDriverStation2))] [SerializeField]
    private StationNum stationNumber2;
    [ConditionalField(true, nameof(isDriverStation2))] [SerializeField]
    private TrackingType trackingType2;

    private int selectedRobotIndex;
    private string selectedName;
    private int selectedRobotIndex2;
    private string selectedName2;
    private List<GameObject> availableRobots = new List<GameObject>();

    private bool isDriverStation() => view == Cameras.DriverStation;
    private bool isDriverStation2() => enableSplitScreen && view2 == Cameras.DriverStation;

    private GameObject _fieldHolder;
    private GameObject _activeRobot;
    private GameObject _activeRobot2;
    private GameObject _activeCam;
    private GameObject _activeCam2;
    private GameObject _spawnedCamera;
    private GameObject _spawnedCamera2;

    private FMS fms;

    private void OnEnable()
    {
        CheckRobots();
        robotSelected.canBeSelected = availableRobots.Select(x => x.name).ToList();
        if (enableSplitScreen && robotSelected2 != null)
        {
            robotSelected2.canBeSelected = availableRobots.Select(x => x.name).ToList();
        }
    }

    private void LateUpdate()
    {
        CheckRobots();
        robotSelected.canBeSelected = availableRobots.Select(x => x.name).ToList();
        robotSelected.selectedIndex = selectedRobotIndex;
        robotSelected.selectedName = selectedName;

        if (enableSplitScreen && robotSelected2 != null)
        {
            robotSelected2.canBeSelected = availableRobots.Select(x => x.name).ToList();
            robotSelected2.selectedIndex = selectedRobotIndex2;
            robotSelected2.selectedName = selectedName2;
        }
    }

    private void Start()
    {
        selectedName = robotSelected.selectedName;
        selectedRobotIndex = robotSelected.selectedIndex;
        if (enableSplitScreen && robotSelected2 != null)
        {
            selectedName2 = robotSelected2.selectedName;
            selectedRobotIndex2 = robotSelected2.selectedIndex;
        }
        CheckRobots();
        ResetField();
    }

    private void Update()
    {
        selectedName = robotSelected.selectedName;
        selectedRobotIndex = robotSelected.selectedIndex;
        if (enableSplitScreen && robotSelected2 != null)
        {
            selectedName2 = robotSelected2.selectedName;
            selectedRobotIndex2 = robotSelected2.selectedIndex;
        }

        if (!EditorApplication.isPlayingOrWillChangePlaymode && RobotLoaded())
        {
            DeleteRobot();
        }
        if (EditorApplication.isPlaying) return;

        if (!CheckField())
        {
            DestroyField();
            LoadField();
        }

        CheckRobots();
    }

    private void LoadField()
    {
        _fieldHolder = new GameObject
        {
            name = "FieldHolder",
            transform = { position = Vector3.zero, rotation = Quaternion.identity, parent = transform },

        };
        Instantiate(fieldPrefab[0], Vector3.zero, Quaternion.identity, _fieldHolder.transform);
    }

    private bool CheckField()
    {
        if (transform.childCount == 0)
        {
            return false;
        }
        else
        {
            return _fieldHolder.transform.Find(fieldPrefab[0].name+"(Clone)");
        }
    }

    private void DestroyField()
    {
        if (transform.Find("FieldHolder"))
        {
            _fieldHolder = transform.Find("FieldHolder").GameObject();
            DestroyImmediate(_fieldHolder);
        }
    }

    public TrackingType GetTrackingType()
    {
        return trackingType;
    }

    public TrackingType GetTrackingType2()
    {
        return trackingType2;
    }

    public void ResetField()
    {
        DestroyField();
        LoadField();
        SpawnRobot();
        addCamera();

        if (enableSplitScreen)
        {
            SpawnRobot2();
            addCamera2();
            SetupSplitScreen();
        }

        Utils.resetParentCache();
        if (fms)
        {
            fms.Restart();
        }
    }

    public void setFMS(FMS fms)
    {
        this.fms = fms;
    }

    public GameObject getFieldHolder()
    {
        return _fieldHolder;
    }

    private void SpawnRobot()
    {
        if (availableRobots.Count > 0 && selectedRobotIndex >= 0 && selectedRobotIndex < availableRobots.Count)
        {
            // Log detected gamepads so you can see what Unity found
            Debug.Log($"[SplitScreen] Detected {Gamepad.all.Count} gamepad(s):");
            for (int i = 0; i < Gamepad.all.Count; i++)
            {
                Debug.Log($"  Gamepad[{i}]: {Gamepad.all[i].displayName} ({Gamepad.all[i].description})");
            }

            GameObject robotToSpawn = availableRobots[selectedRobotIndex];
            _activeRobot = Instantiate(robotToSpawn, spawnPoint.position, spawnPoint.rotation, _fieldHolder.transform);
            var frame = _activeRobot.GetComponent<BuildFrame>();
            frame.playerNumber = "Player1";

            // Pair with first gamepad if split screen is active
            InputDevice pairedDevice = null;
            if (enableSplitScreen && Gamepad.all.Count > 0)
            {
                pairedDevice = Gamepad.all[0];
                Debug.Log($"[SplitScreen] Player 1 paired to: {pairedDevice.displayName}");
            }

            var controller = frame.GetSwerveController(pairedDevice);
            if (controller)
            {
                controller.isRed = false; // Blue alliance
                ConfigureControllerView(controller, view);
            }
        }
    }

    private void SpawnRobot2()
    {
        int robotIdx = (robotSelected2 != null) ? selectedRobotIndex2 : selectedRobotIndex;

        if (availableRobots.Count > 0 && robotIdx >= 0 && robotIdx < availableRobots.Count)
        {
            Transform spawn = spawnPoint2 != null ? spawnPoint2 : spawnPoint;
            GameObject robotToSpawn = availableRobots[robotIdx];
            _activeRobot2 = Instantiate(robotToSpawn, spawn.position, spawn.rotation, _fieldHolder.transform);
            var frame = _activeRobot2.GetComponent<BuildFrame>();
            frame.playerNumber = "Player1"; // Same control scheme, different device

            // Pair with second gamepad
            InputDevice pairedDevice = null;
            if (Gamepad.all.Count > 1)
            {
                pairedDevice = Gamepad.all[1];
                Debug.Log($"[SplitScreen] Player 2 paired to: {pairedDevice.displayName}");
            }
            else if (Gamepad.all.Count > 0)
            {
                // Only one gamepad connected - player 2 gets keyboard only
                pairedDevice = Keyboard.current;
                Debug.Log("[SplitScreen] Only 1 gamepad found. Player 2 using keyboard (WASD/IJKL).");
            }
            else
            {
                Debug.LogWarning("[SplitScreen] No gamepads detected for Player 2!");
            }

            var controller = frame.GetSwerveController(pairedDevice);
            if (controller)
            {
                controller.isRed = true; // Red alliance
                ConfigureControllerView(controller, view2);
            }
        }
    }

    private void ConfigureControllerView(SwerveController controller, Cameras cameraView)
    {
        switch (cameraView)
        {
            case Cameras.FirstPerson:
                controller.reversed = false;
                controller.fieldCentric = false;
                break;
            case Cameras.FirstPersonReversed:
                controller.reversed = true;
                controller.fieldCentric = false;
                break;
            case Cameras.ThirdPerson:
                controller.reversed = false;
                controller.fieldCentric = true;
                break;
            case Cameras.ReversedThirdPerson:
                controller.reversed = true;
                controller.fieldCentric = true;
                break;
            case Cameras.DriverStation:
                controller.reversed = false;
                controller.fieldCentric = true;
                break;
        }
    }

    private bool RobotLoaded()
    {
        return _activeRobot != null;
    }

    public GameObject GetRobotLoaded()
    {
        return _activeRobot;
    }

    public GameObject GetRobot2Loaded()
    {
        return _activeRobot2;
    }

    public bool IsSplitScreenEnabled()
    {
        return enableSplitScreen;
    }

    private void DeleteRobot()
    {
        DestroyImmediate(_spawnedCamera);
        DestroyImmediate(_activeRobot);
        if (enableSplitScreen)
        {
            DestroyImmediate(_spawnedCamera2);
            DestroyImmediate(_activeRobot2);
        }
    }

    private void addCamera()
    {
        string objectToLoad = "Cameras/" + view.ToString();
        _activeCam = Resources.Load(objectToLoad) as GameObject;

        var parent = _activeRobot;
        var spawnRotation = spawnPoint.gameObject;
        if (fms)
        {
            parent = view == Cameras.DriverStation ? fms.blueStationCams[(int)stationNumber] : _activeRobot;
            spawnRotation = view == Cameras.DriverStation ? fms.redStationCams[(int)stationNumber] : spawnPoint.gameObject;
        }

        _spawnedCamera = Instantiate(_activeCam, Vector3.zero, spawnRotation.transform.rotation, parent.transform);
        _spawnedCamera.transform.localPosition = Vector3.zero;
    }

    private void addCamera2()
    {
        Cameras camView = view2;
        string objectToLoad = "Cameras/" + camView.ToString();
        _activeCam2 = Resources.Load(objectToLoad) as GameObject;

        var parent = _activeRobot2;
        Transform spawnTf = spawnPoint2 != null ? spawnPoint2 : spawnPoint;
        var spawnRotation = spawnTf.gameObject;
        if (fms)
        {
            parent = camView == Cameras.DriverStation ? fms.redStationCams[(int)stationNumber2] : _activeRobot2;
            spawnRotation = camView == Cameras.DriverStation ? fms.blueStationCams[(int)stationNumber2] : spawnTf.gameObject;
        }

        _spawnedCamera2 = Instantiate(_activeCam2, Vector3.zero, spawnRotation.transform.rotation, parent.transform);
        _spawnedCamera2.transform.localPosition = Vector3.zero;
    }

    /// <summary>
    /// Configures split-screen viewports: Player 1 top half, Player 2 bottom half
    /// </summary>
    private void SetupSplitScreen()
    {
        // Find cameras in the spawned camera objects
        Camera cam1 = _spawnedCamera.GetComponentInChildren<Camera>();
        Camera cam2 = _spawnedCamera2.GetComponentInChildren<Camera>();

        if (cam1 != null)
        {
            // Left half of screen
            cam1.rect = new Rect(0f, 0f, 0.5f, 1f);
        }

        if (cam2 != null)
        {
            // Right half of screen
            cam2.rect = new Rect(0.5f, 0f, 0.5f, 1f);
        }

        // Disable any AudioListener on the second camera to avoid warnings
        AudioListener listener2 = _spawnedCamera2.GetComponentInChildren<AudioListener>();
        if (listener2 != null)
        {
            listener2.enabled = false;
        }
    }


    public void CheckRobots()
    {
        GameObject[] loadedRobots = Resources.LoadAll<GameObject>("Robots");

        availableRobots.Clear();
        foreach (var robot in loadedRobots)
        {
            availableRobots.Add(robot);
        }

        if (selectedRobotIndex >= availableRobots.Count)
        {
            selectedRobotIndex = availableRobots.Count > 0 ? availableRobots.Count - 1 : 0;
        }
        if (selectedRobotIndex2 >= availableRobots.Count)
        {
            selectedRobotIndex2 = availableRobots.Count > 0 ? availableRobots.Count - 1 : 0;
        }
    }
}
