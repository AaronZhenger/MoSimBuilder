using UnityEngine;
using UnityEngine.InputSystem;
using Vector3 = System.Numerics.Vector3;

namespace Util
{
    public class Utils
    {
        // Start is called before the first frame update
        public Utils()
        {
        
        }

        public static GameObject FindChild(string childName, GameObject parent)
        {
            if (parent == null) return null;
            
            for (int i = 0; i < parent.transform.childCount; i++)
            {
                if (parent.transform.GetChild(i).name == childName)
                {
                    return parent.transform.GetChild(i).gameObject;
                }
            }
            
            return null;
        }

        public static GameObject FindParentRB(GameObject child)
        {
            var t = child.transform.parent;
            while (t.GetComponent<Rigidbody>() == null)
            {
                if (t.parent == null)
                {
                    return null;
                }
                t = t.parent.transform;
            }
            
            return t.gameObject;
        }
        
        public static GameObject FindParentPlayerInput(GameObject child)
        {
            var t = child.transform;
            while (t.GetComponent<PlayerInput>() == null)
            {
                t = t.parent.transform;
            }
            
            return t.gameObject;
        }
    }
}