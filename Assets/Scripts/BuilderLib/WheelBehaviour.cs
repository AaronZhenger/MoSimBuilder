using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class WheelBehaviour : MonoBehaviour
{
    [HideInInspector] public float wheelDiameter;
    private bool[] _wheelHits;
    public List<Vector3> collisionPoints = new List<Vector3>();
    public List<Vector3> collisionNormals = new List<Vector3>();

    // Start is called before the first frame update
    private void Start()
    {
        _wheelHits = new bool[8];
    }

    // Fixed Update is called every Physics Tick
    private void FixedUpdate()
    {
        _wheelHits[0] = Physics.Raycast(transform.position, -transform.up.normalized, out RaycastHit hit, wheelDiameter/2); 
        _wheelHits[1] = Physics.Raycast(transform.position, transform.up.normalized, out hit, wheelDiameter/2);
        _wheelHits[2] = Physics.Raycast(transform.position, transform.forward.normalized, out hit, wheelDiameter/2);
        _wheelHits[3] = Physics.Raycast(transform.position, -transform.forward.normalized, out hit, wheelDiameter/2);
        _wheelHits[4] = Physics.Raycast(transform.position, (transform.up.normalized+transform.forward.normalized)/2, out hit, wheelDiameter/2);
        _wheelHits[5] = Physics.Raycast(transform.position, (-transform.up.normalized+transform.forward.normalized)/2, out hit, wheelDiameter/2);
        _wheelHits[6] = Physics.Raycast(transform.position, (transform.up.normalized-transform.forward.normalized)/2, out hit, wheelDiameter/6);
        _wheelHits[7] = Physics.Raycast(transform.position, (-transform.up.normalized-transform.forward.normalized)/2, out hit, wheelDiameter/6);
        
        collisionPoints.Clear();
        collisionNormals.Clear();
        if (_wheelHits[0])
        {
            collisionPoints.Add(transform.position-transform.up.normalized*wheelDiameter/2);
            collisionNormals.Add(transform.forward.normalized);
        }

        if (_wheelHits[1])
        {
            collisionPoints.Add(transform.position+transform.up.normalized*wheelDiameter/2);
            collisionNormals.Add(-transform.forward.normalized);
        }

        if (_wheelHits[2])
        {
            collisionPoints.Add(transform.position+transform.forward.normalized*wheelDiameter/2);
            collisionNormals.Add(transform.up.normalized);
        }

        if (_wheelHits[3])
        {
            collisionPoints.Add(transform.position-transform.forward.normalized*wheelDiameter/2);
            collisionNormals.Add(-transform.up.normalized);
        }

        if (_wheelHits[4])
        {
            collisionPoints.Add(transform.position+((transform.up.normalized+transform.forward.normalized)/2)*wheelDiameter/2);
            collisionNormals.Add(-transform.forward.normalized+transform.up.normalized/2);
        }

        if (_wheelHits[5])
        {
            collisionPoints.Add(transform.position+((-transform.up.normalized+transform.forward.normalized)/2)*wheelDiameter/2);
            collisionNormals.Add(transform.forward.normalized+transform.up.normalized/2);
        }

        if (_wheelHits[6])
        {
            collisionPoints.Add(transform.position+((transform.up.normalized-transform.forward.normalized)/2)*wheelDiameter/2);
            collisionNormals.Add(-transform.forward.normalized-transform.up.normalized/2);
        }

        if (_wheelHits[7])
        {
            collisionPoints.Add(transform.position+((-transform.up.normalized-transform.forward.normalized)/2)*wheelDiameter/2);
            collisionNormals.Add(transform.forward.normalized-transform.up.normalized/2);
        }
        
        
        
    }
}
