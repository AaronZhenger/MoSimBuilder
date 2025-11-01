using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Util;

[ExecuteAlways]
public class BuildCollider : MonoBehaviour
{
    [SerializeField] Vector3 ColliderSize;

    [SerializeField] private Units units;
    
    private BoxCollider box;

    private float _scale;
    private 
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (!EditorApplication.isPlaying)
        {
            _scale = units switch
            {
                Units.Inch => 0.0254f,
                Units.Meter => 1,
                Units.Centimeter => 0.01f,
                Units.Millimeter => 0.001f,
                _ => 0.0254f
            };
            
            if (!box)
            {
                var intakeParent = Utils.TryGetAddChild("IntakeBox", gameObject);
                box = Utils.TryGetAddComponent<BoxCollider>(intakeParent);
                box.size = ColliderSize * _scale;
            }
            else
            {
                box.size = ColliderSize * _scale;
                box.transform.localPosition = Vector3.zero;
                box.transform.localRotation = Quaternion.identity;
            }
                
        }
    }
}
