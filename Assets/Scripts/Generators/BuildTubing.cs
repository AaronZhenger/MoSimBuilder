using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Util;

[ExecuteAlways]
public class BuildTubing : MonoBehaviour
{
    [SerializeField] private TubeType tubeType;

    [SerializeField] private string partName;

    [SerializeField] private float length;

    [SerializeField] private Units units;

    private GeneratePart _generatePart;

    private GameObject _model;

    private GameObject _tube;

    private float _factor;

    // Start is called before the first frame update
    void Start()
    {
        Startup();
    }

    // Update is called once per frame
    void Update()
    {
        
        _factor = units switch
        {
            Units.Inch => 0.0254f,
            Units.Meter => 1,
            Units.Centimeter => 0.01f,
            Units.Millimeter => 0.001f,
            _ => 0.0254f
        };
        
        if (_model == null)
        {
            _model = new GameObject
            {
                name = "tubeModel",
                transform =
                {
                    parent = transform,
                    localPosition = Vector3.zero,
                    localRotation = Quaternion.identity,
                    localScale = Vector3.one
                }
            };
        }

        var loadedTubes = Resources.LoadAll<GameObject>("Tubing") as GameObject[];

        foreach (var loadedTube in loadedTubes)
        {
            if (loadedTube.name == TubeType.OneXTwoXEighth.ToString())
            {
                _tube = loadedTube;
            }
        }

        if (_generatePart == null)
        {
            _generatePart = _model.AddComponent<GeneratePart>();

            _generatePart.Part = _tube;

            _generatePart.PartName = "tube";

            _generatePart.LoadedPartLocation = Vector3.zero;

            _generatePart.LoadedPartRotation = Quaternion.Euler(Vector3.zero);

            _generatePart.LoadedPartScale = Vector3.one;
        }
        else if (_generatePart.Part != _tube)
        {
            _generatePart.Part = _tube;

            _generatePart.PartName = "tube";

            _generatePart.LoadedPartLocation = Vector3.zero;

            _generatePart.LoadedPartRotation = Quaternion.Euler(Vector3.zero);

            _generatePart.LoadedPartScale = new Vector3(1,1,length * _factor);
        }
        else if (_generatePart != null)
        {
            _generatePart.LoadedPartLocation = Vector3.zero;

            _generatePart.LoadedPartRotation = Quaternion.Euler(Vector3.zero);

            _generatePart.LoadedPartScale = new Vector3(1,1,length * _factor);
        }
    }

    private void Startup()
    {
        _model = Utils.FindChild("tubeModel", gameObject);

        if (_model != null)
        {
            var frameParts = _model.GetComponent<GeneratePart>();
            _generatePart = frameParts;
        }
    }
}
