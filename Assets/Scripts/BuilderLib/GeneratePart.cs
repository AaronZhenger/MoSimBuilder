using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEditor;
using UnityEngine;
using Util;

[ExecuteInEditMode]
public class GeneratePart : MonoBehaviour
{
    [SerializeField] private string partName;
    [SerializeField] public bool ObjectSpawned;
    
    [HideInInspector] public string PartName;

    [HideInInspector] public GameObject Part;

    [HideInInspector] public Vector3 LoadedPartLocation;

    [HideInInspector] public Quaternion LoadedPartRotation;

    [HideInInspector] public Vector3 LoadedPartScale;
    
    private GameObject currentPart;
    
    private GameObject _loadedPart;
    

    // Start is called before the first frame update
    void Start()
    {
        Startup();
    }

    void Awake()
    {
        Startup();
    }

    // Update is called once per frame
    void Update()
    {
        if (PartName != null && Part != null)
        {
            partName = PartName;
            ObjectSpawned = _loadedPart != null;
            if (_loadedPart != null)
            {
                if (_loadedPart.name != PartName)
                {
                    DestroyImmediate(_loadedPart);
                } else if (currentPart != Part)
                {
                    DestroyImmediate(_loadedPart);
                }
            }

            if (_loadedPart == null && Part != null)
            {
                currentPart = Part;
                _loadedPart = Instantiate(Part, LoadedPartLocation, LoadedPartRotation, transform);
                _loadedPart.name = PartName;
            }

            _loadedPart.transform.localPosition = LoadedPartLocation;
            _loadedPart.transform.localRotation = LoadedPartRotation;

            var scaleAdjustedScale = new Vector3(LoadedPartScale.x / transform.localScale.x,
                LoadedPartScale.y / transform.localScale.y, LoadedPartScale.z / transform.localScale.z);
            _loadedPart.transform.localScale = scaleAdjustedScale;
        }
    }

    private void Startup()
    {
        if (partName != null)
        {
            PartName = partName;
        }

        _loadedPart = Utils.FindChild(PartName, gameObject);
        currentPart = Part;
    }
}
