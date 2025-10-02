using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Util;

[ExecuteAlways]
public class LoadMatch : MonoBehaviour
{
    [SerializeField] private GameObject[] fieldPrefab;
    [SerializeField] private Transform spawnPoint;
    [Header("Robot Selection")]
    [SerializeField] private InspectorDropdown robotSelected;
    
     [HideInInspector] public int selectedRobotIndex; 
    [NonSerialized]
    public List<GameObject> availableRobots = new List<GameObject>();
    
    
    private GameObject _fieldHolder;
    private GameObject _activeRobot;
    private GameObject _1StCam;

    private void OnEnable()
    {
        CheckRobots();
        robotSelected.canBeSelected = availableRobots.Select(x => x.name).ToList();
        robotSelected.selectedIndex = selectedRobotIndex;
    }

    private void Start()
    {
        CheckRobots(); 
        ResetField();
    }
    
    private void Update()
    {
        selectedRobotIndex = robotSelected.selectedIndex;
        
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
    
    private void ResetField()
    {
        DestroyField();
        LoadField();
        SpawnRobot();
        addCamera();
    }
    
    private void SpawnRobot()
    {
        if (availableRobots.Count > 0 && selectedRobotIndex >= 0 && selectedRobotIndex < availableRobots.Count)
        {
            GameObject robotToSpawn = availableRobots[selectedRobotIndex];
            _activeRobot = Instantiate(robotToSpawn, spawnPoint.position, spawnPoint.rotation, _fieldHolder.transform);
        }
    }
    
    private bool RobotLoaded()
    {
        return _activeRobot != null;
    }
    
    private void DeleteRobot()
    {
        DestroyImmediate(_activeRobot);
    }
    
    private void addCamera()
    {
        _1StCam = Resources.Load("Cameras/1stPerson") as GameObject;
        
        var cam = Instantiate(_1StCam, Vector3.zero, spawnPoint.rotation, _activeRobot.transform);;
        cam.transform.localPosition = Vector3.zero;
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
    }
}
