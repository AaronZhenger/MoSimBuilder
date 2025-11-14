using System.Diagnostics;
using MyBox;
using UnityEngine;
using Util;

namespace Generators
{
    [ExecuteInEditMode]
    public class BuildSpacer : GeneratePart
    {
        [SerializeField] private spacerType spacerType;

        [SerializeField] private bool shouldCollide = true;

        [SerializeField] private Units units;

        [SerializeField] private float spacerLength = 1;
        
        private static GameObject[] loadedSpacers;

        private GameObject _spacer;

        private ColliderDisabler colliderDisabler;

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
            
            if (colliderDisabler)
            {
                colliderDisabler.SetState(shouldCollide);
            }
            else if (getLoadedPart())
            {
                colliderDisabler = Utils.TryGetComponentOnChild<ColliderDisabler>(getLoadedPart());
            }

            loadedSpacers ??= Resources.LoadAll<GameObject>("Parts/Spacers") as GameObject[];

            foreach (var loadedSpacer in loadedSpacers)
            {
                if (loadedSpacer.name == spacerType.ToString())
                {
                    _spacer = loadedSpacer;
                }
            }

            Vector3 partScale = new Vector3(1, 1, spacerLength * _scaleFactor);

            if (!Part || Part != _spacer)
            {

                Part = _spacer;

                PartName = "spacer";

                LoadedPartLocation = Vector3.zero;

                LoadedPartRotation = Quaternion.Euler(Vector3.zero);

                LoadedPartScale = partScale;
            }
            else if (Part)
            {
                LoadedPartLocation = Vector3.zero;

                LoadedPartRotation = Quaternion.Euler(Vector3.zero);

                LoadedPartScale = partScale;
            }
        }
    }
}
