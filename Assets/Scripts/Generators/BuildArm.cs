using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Util;

[ExecuteAlways]
public class BuildArm : MonoBehaviour
{
    [SerializeField] private SetPoint[] setPoints;
    
    private ConfigurableJoint _joint;
    
    private Rigidbody _rigidbody;

    private GameObject _connectedBody;

    private Rigidbody _gRb;

    private JointController _controller;

    private JointDrive _drive;
    // Start is called before the first frame update
    void Start()
    {
        if (EditorApplication.isPlaying)
        {
            if (_rigidbody == null)
            {
                _rigidbody = gameObject.AddComponent<Rigidbody>();
            }
            
            if (_joint == null)
            {
                _joint = gameObject.AddComponent<ConfigurableJoint>();
            }
            
            _connectedBody = Utils.FindParentRB(gameObject);

            _gRb = _connectedBody.GetComponent<Rigidbody>();

            _joint.connectedBody = _gRb;
            _joint.xMotion = ConfigurableJointMotion.Locked;
            _joint.yMotion = ConfigurableJointMotion.Locked;
            _joint.zMotion = ConfigurableJointMotion.Locked;
            _joint.angularYMotion = ConfigurableJointMotion.Locked;
            _joint.angularZMotion = ConfigurableJointMotion.Locked;

            _joint.angularXMotion = ConfigurableJointMotion.Free;

            _drive.maximumForce = 8000;
            _drive.positionDamper = 100;
            _drive.positionSpring = 0;
            _drive.useAcceleration = false;
            _joint.angularXDrive = _drive;
            
            _controller = gameObject.AddComponent<JointController>();
            
            _controller.p = 1;
            _controller.i = 0;
            _controller.d = 0.0005f;
            _controller.iSat = 0;
            _controller.max = 10;
            _controller.angular = true;
            _controller.driveAxis = new Vector3(1, 0, 0);
            _controller.joint = _joint;
            
            _rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
            _rigidbody.collisionDetectionMode = CollisionDetectionMode.Continuous;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!EditorApplication.isPlaying) return;
        float angle = Quaternion.Angle(transform.rotation, _joint.connectedBody.rotation);
        
        if (transform.localRotation.eulerAngles.x > 180)
        {
            angle = -angle;
        }
        
        if (angle < 0)
        {
            angle += 360;
        }
        
        if (angle >= 360)
        {
            angle -= 360;
        }

        if (angle < 0)
        {
            angle += 360;
        }
                
        angle = Mathf.Repeat(angle, 360);
        
        if (!EditorApplication.isPlaying)
        {
           
        }
        else
        {
            _controller.setPoints = setPoints;
            _controller.currentPosition = angle;
        }
    }
}
