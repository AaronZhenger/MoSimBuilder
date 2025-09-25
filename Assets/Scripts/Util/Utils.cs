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

        /// <summary>
        /// Finds a child with a given name by only searching the children instead of everything.
        /// </summary>
        /// <param name="childName"></param>
        /// <param name="parent"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Finds the first Parent objcet which contains a rigid body
        /// </summary>
        /// <param name="child"></param>
        /// <returns></returns>
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
        
        /// <summary>
        /// finds the first parent object with a player input object.
        /// </summary>
        /// <param name="child"></param>
        /// <returns></returns>
        public static GameObject FindParentPlayerInput(GameObject child)
        {
            var t = child.transform;
            while (t.GetComponent<PlayerInput>() == null)
            {
                t = t.parent.transform;
            }
            
            return t.gameObject;
        }
        
        /// <summary>
        /// Flips the angle 180
        /// </summary>
        /// <param name="angle"></param>
        /// <returns></returns>
        public static float FlipAngle(float angle)
        {
            angle = -angle;
            angle = Mathf.Repeat(angle, 360); 
            if (angle < 0)
            {
                angle += 360;
            }
            return angle;
        }
        
        /// <summary>
        /// Wraps the angle into -180 180 from 360
        /// </summary>
        /// <param name="angle"></param>
        /// <returns></returns>
        public static float WrapAngle180(float angle)
        {
            angle = Mathf.Repeat(angle, 360);
            if (angle > 180)
            {
                angle -= 360; // Convert to -180 to 180 range
            }
            return angle;
        }

        /// <summary>
        /// wraps angle to 0 to 360
        /// </summary>
        /// <param name="angle"></param>
        /// <returns></returns>
        public static float WrapAngle360(float angle)
        {
            angle = Mathf.Repeat(angle, 360);
            return angle;
        }
        
        /// <summary>
        /// returns the difference between two angles
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static float AngleDifference(float a, float b) {
            return (a - b + 540) % 360 - 180;
        }
    }
}