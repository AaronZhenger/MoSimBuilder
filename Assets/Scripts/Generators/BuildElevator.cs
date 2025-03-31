using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Util;

public class Buildelevator : MonoBehaviour
{
    [SerializeField] private SetPoint[] setPoints;

    private Vector3 _startPose;
    
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

            _startPose = transform.localPosition;
            
            _connectedBody = Utils.FindParentRB(gameObject);

            _gRb = _connectedBody.GetComponent<Rigidbody>();

            _joint.connectedBody = _gRb;
            _joint.xMotion = ConfigurableJointMotion.Locked;
            _joint.zMotion = ConfigurableJointMotion.Locked;
            _joint.angularYMotion = ConfigurableJointMotion.Locked;
            _joint.angularZMotion = ConfigurableJointMotion.Locked;
            _joint.angularXMotion = ConfigurableJointMotion.Locked;
            
            _joint.yMotion = ConfigurableJointMotion.Free;

            _drive.maximumForce = 8000000;
            _drive.positionDamper = 10000;
            _drive.positionSpring = 0;
            _drive.useAcceleration = false;
            _joint.yDrive = _drive;
            
            _controller = gameObject.AddComponent<JointController>();

            _controller.p = 5;
            _controller.i = 0;
            _controller.d = 0.0005f;
            _controller.iSat = 0;
            _controller.max = 5;
            _controller.angular = false;
            _controller.driveAxis = new Vector3(0, 1, 0);
            _controller.joint = _joint;
            
            _rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
            _rigidbody.collisionDetectionMode = CollisionDetectionMode.Continuous;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!EditorApplication.isPlaying)
        {
           
        }
        else
        {
            _controller.setPoints = setPoints;
            _controller.currentPosition = transform.localPosition.y - _startPose.y;
        }
    }
}
