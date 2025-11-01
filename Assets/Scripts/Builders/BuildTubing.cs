using UnityEngine;
using Util;

namespace Generators
{
    [ExecuteAlways]
    public class BuildTubing : GeneratePart
    {
        [SerializeField] private TubeType tubeType;
        
        [SerializeField] private Units units;
        
        [SerializeField] private float length;

        private static GameObject[] loadedTubes;

        private GameObject _tube;

        private float _factor;

        // Start is called before the first frame update
        void Start()
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
        
            _factor = units switch
            {
                Units.Inch => 0.0254f,
                Units.Meter => 1,
                Units.Centimeter => 0.01f,
                Units.Millimeter => 0.001f,
                _ => 0.0254f
            };

            loadedTubes ??= Resources.LoadAll<GameObject>("Tubing") as GameObject[];

            foreach (var loadedTube in loadedTubes)
            {
                if (loadedTube.name == TubeType.OneXTwoXEighth.ToString() && tubeType == TubeType.OneXTwoXEighth)
                {
                    _tube = loadedTube;
                } else if (loadedTube.name == TubeType.OneXOneXEighth.ToString() && tubeType == TubeType.OneXOneXEighth)
                {
                    _tube = loadedTube;
                } else if (loadedTube.name == TubeType.TwoXTwoXEighth.ToString() && tubeType == TubeType.TwoXTwoXEighth)
                {
                    _tube = loadedTube;
                }
            }
        
        

            if (!Part)
            {

                Part = _tube;

                PartName = "tube";

                LoadedPartLocation = Vector3.zero;

                LoadedPartRotation = Quaternion.Euler(Vector3.zero);

                LoadedPartScale = Vector3.one;
            }
            else if (Part != _tube)
            {
                Part = _tube;

                PartName = "tube";

                LoadedPartLocation = Vector3.zero;

                LoadedPartRotation = Quaternion.Euler(Vector3.zero);

                LoadedPartScale = new Vector3(1,1,length * _factor);
            }
            else if (Part != null)
            {
                LoadedPartLocation = Vector3.zero;

                LoadedPartRotation = Quaternion.Euler(Vector3.zero);

                LoadedPartScale = new Vector3(1,1,length * _factor);
            }
            
            base.run();
        }
    }
}
