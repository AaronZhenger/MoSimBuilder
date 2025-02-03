using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

[ExecuteAlways]//executes in editor
public class LoadMatch : MonoBehaviour
{
    [SerializeField] private GameObject[] fieldPrefab;
    [SerializeField] private GameObject[] robots;
    [SerializeField] private Transform spawnPoint;
    
    //Robot Selector stuff
    private GameObject[] _robotBuffer;
    private GameObject[] _robotBuffer2;
    private GameObject[] _robotBuffer3;
    private GameObject[] _misisngRobots;
    
    //Field selector Stuff
    private GameObject _fieldHolder;
    
    //Robot Stuff
    private GameObject _activeRobot;

    private GameObject _1StCam;
    
    // Start is called before the first frame update
    private void Start()
    {
        ResetField();
    }

    // Update is called once per frame
    private void Update()
    {
        
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
        _activeRobot = Instantiate(robots[0], spawnPoint.position, spawnPoint.rotation, _fieldHolder.transform);
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
        //load camera options
        _1StCam = Resources.Load("Cameras/1stPerson") as GameObject;
        
        //spawn selected camera
        var cam = Instantiate(_1StCam, Vector3.zero, spawnPoint.rotation, _activeRobot.transform);;
        cam.transform.localPosition = Vector3.zero;
    }

    private void CheckRobots()//its clean and wont brake but I hate this.
    {
        //detect all robot prefabs
        _robotBuffer = Resources.LoadAll<GameObject>("Robots") as GameObject[];

        // create list on initial startup
        if (robots.Length == 0)
        {
            robots = _robotBuffer;
        } 
        else if (_robotBuffer.Length != robots.Length) //detect a change in prefab count
        {
            //creates a blank list for traking robots not on the list
            _misisngRobots = new GameObject[_robotBuffer.Length];
            //stores current state of robot list for use later
            _robotBuffer2 = robots;
            
            robots = new GameObject[_robotBuffer.Length];

            foreach (var t in _robotBuffer) //Detect what robots are not currently in the list
            {
                for (int j = 0; j <= _robotBuffer2.Length; j++)
                {
                    var l = 0;
                    if (j == _robotBuffer2.Length)
                    {
                        while (_misisngRobots[l] != null)
                        {
                            l += 1;
                        }

                        _misisngRobots[l] = t;
                    } 
                    else if (_robotBuffer2[j] == t)
                    {
                        j = _robotBuffer2.Length; //this can NOT be a return
                    }
                }
            }

            //clean the buffer in case a robot was deleted
            int k = 0; 
            for (int i = 0; i < _robotBuffer2.Length; i++) // counts non null robots
            {
                if (_robotBuffer2[i] != null)
                {
                    k += 1;
                }
            }

            _robotBuffer3 = _robotBuffer2; //saves buffer down.
            
            _robotBuffer2 = new GameObject[k];

            var p = 0;
            for (int i = 0; i < _robotBuffer3.Length; i++) //pulls saved buffer back ignoring missing reference robots
            {
                if (_robotBuffer3[i] != null)
                {
                    while (_robotBuffer2[p] != null)
                    {
                        p += 1;
                    }
                    
                    _robotBuffer2[p] = _robotBuffer3[i];
                }
            }

            for (int i = 0; i < _robotBuffer2.Length; i++) //restores the primary buffer to the robot list
            {
                if (i < robots.Length && i < _robotBuffer2.Length)
                {
                    robots[i] = _robotBuffer2[i];
                }
            }

            for (int i = 0; i < _misisngRobots.Length; i++) // adds missing robots to end of the list
            {
                if (_misisngRobots[i] != null)
                {
                    robots[robots.Length - i - 1] = _misisngRobots[i];
                }
            }
        }
    }
}
