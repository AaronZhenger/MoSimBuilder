using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEditor;
using UnityEngine;
using Util;

/// <summary>
/// Generates a part object can be used multiple times on one object to create complex models
/// </summary>
[ExecuteInEditMode]
public class GeneratePart : MonoBehaviour
{
    [SerializeField] private string partName;
    [SerializeField] public bool ObjectSpawned;
    
    /// <summary>
    /// The name of the Object to use
    /// </summary>
    [HideInInspector] public string PartName;

    /// <summary>
    /// The part(GameObject) to generate
    /// </summary>
    [HideInInspector] public GameObject Part;
    /// <summary>
    /// The transform relative to the scripts object to put the part
    /// </summary>
    [HideInInspector] public Vector3 LoadedPartLocation;
/// <summary>
/// The rotation relative to the scripts object to put the part
/// </summary>
    [HideInInspector] public Quaternion LoadedPartRotation;
/// <summary>
/// The Scale for the object to use.
/// </summary>
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
