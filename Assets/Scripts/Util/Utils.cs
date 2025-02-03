using UnityEngine;
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
    }
}