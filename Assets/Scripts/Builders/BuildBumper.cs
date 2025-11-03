using System.Collections;
using System.Collections.Generic;
using MyBox;
using UnityEngine;
using UnityEngine.Serialization;
using Util;

public class BuildBumper : GeneratePart
{
    [SerializeField] private BumperType bumperType;

    private bool IsAdjustable() => bumperVariant == BumperVariants.Side;
    
    [SerializeField] private BumperVariants bumperVariant;
    
    [ConditionalField(true, nameof(IsAdjustable))]
    [SerializeField] private Units units;

    [ConditionalField(true, nameof(IsAdjustable))] 
    [SerializeField] private float bumperLength = 28;
    
    private static GameObject[] loadedPlates;

    private GameObject _bumper;

    private float _scaleFactor;

    // Start is called before the first frame update
    private void Start()
    {
        Startup();
    }

    private void OnEnable()
    {
        Startup();
    }

    // Update is called once per frame
    void Update()
    {
        switch (units)
        {
            case Units.Inch:
                _scaleFactor = 0.0254f;
                break;
            case Units.Centimeter:
                _scaleFactor = 0.01f;
                break;
            case Units.Meter:
                _scaleFactor = 1.0f;
                break;
            case Units.Millimeter:
                _scaleFactor = 0.001f;
                break;
        }

        loadedPlates ??= Resources.LoadAll<GameObject>("Parts/Bumper") as GameObject[];

        if (bumperType == BumperType.Modern && bumperVariant == BumperVariants.Lift)
        {
            bumperVariant = BumperVariants.Side;
        }

        foreach (var plate in loadedPlates)
        {
            if (plate.name == bumperType.ToString() + bumperVariant.ToString())
            {
                _bumper = plate;
            }
        }

        Vector3 bumperScale = Vector3.one;
        if (IsAdjustable())
        {
            bumperScale.z = bumperLength * _scaleFactor;
        }

        if (!Part || Part != _bumper)
        {

            Part = _bumper;

            PartName = "Bumper";

            LoadedPartLocation = Vector3.zero;

            LoadedPartRotation = Quaternion.Euler(Vector3.zero);

            LoadedPartScale = bumperScale;
        }
        else if (Part)
        {
            LoadedPartLocation = Vector3.zero;

            LoadedPartRotation = Quaternion.Euler(Vector3.zero);

            LoadedPartScale = bumperScale;
        }
    }
}
