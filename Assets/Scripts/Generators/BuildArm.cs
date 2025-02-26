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

    private GameObject _connectedBody;

    private Rigidbody _gRb;

    private JointController _controller;

    private JointDrive _drive;
    // Start is called before the first frame update
    void Start()
    {
        if (EditorApplication.isPlaying)
        {
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

            _drive.maximumForce = 800;
            _drive.positionDamper = 10;
            _drive.positionSpring = 0;
            _drive.useAcceleration = false;
            _joint.angularXDrive = _drive;
            
            _controller = gameObject.AddComponent<JointController>();
            
            _controller.angular = true;
            _controller.driveAxis = new Vector3(1, 0, 0);
            _controller.joint = _joint;
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
            _controller.currentPosition = transform.localRotation.eulerAngles.x;
        }
    }
}
