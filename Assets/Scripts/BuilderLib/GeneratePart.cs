using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEditor;
using UnityEngine;
using Util;

[ExecuteAlways]
public class GeneratePart : MonoBehaviour
{
    [SerializeField] private string partName;
    [SerializeField] public bool ObjectSpawned;
    
    public string PartName { get; set;}

    public GameObject Part { get; set; }

    public Vector3 LoadedPartLocation { get; set; }
    
    public Quaternion LoadedPartRotation { get; set; }
    
    public Vector3 LoadedPartScale { get; set; }
    
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
                }
            }

            if (_loadedPart == null && Part != null)
            {
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
    }
}
