using UnityEngine;
using Util;

namespace Generators
{
    [ExecuteAlways]
    public class BuildWheel : GeneratePart
    {
        [SerializeField] private WheelTypes wheelType;

        [SerializeField] private bool shouldCollide = true;
        
        private static GameObject[] loadedWheels;

        private GameObject _wheel;

        [SerializeField] private ColliderDisabler colliderDisabler;

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
            if (colliderDisabler)
            {
                colliderDisabler.SetState(shouldCollide);
            }
            else if (getLoadedPart())
            {
                colliderDisabler = Utils.TryGetComponentOnChild<ColliderDisabler>(getLoadedPart());
            }

            loadedWheels ??= Resources.LoadAll<GameObject>("Parts/Wheel") as GameObject[];

            foreach (var loadedWheel in loadedWheels)
            {
                if (loadedWheel.name == wheelType.ToString())
                {
                    _wheel = loadedWheel;
                }
            }

            if (!Part || Part != _wheel)
            {

                Part = _wheel;

                PartName = "wheel";

                LoadedPartLocation = Vector3.zero;

                LoadedPartRotation = Quaternion.Euler(Vector3.zero);

                LoadedPartScale = Vector3.one;
            }
            else if (Part)
            {
                LoadedPartLocation = Vector3.zero;

                LoadedPartRotation = Quaternion.Euler(Vector3.zero);

                LoadedPartScale = Vector3.one;
            }
        }
    }
}
