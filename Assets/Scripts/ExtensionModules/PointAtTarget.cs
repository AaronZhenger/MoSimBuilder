using System;
using System.Collections;
using System.Collections.Generic;
using MyBox;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;
using Util;

public class PointAtTarget : MonoBehaviour
{
    [SerializeField] private TargetType targetType;
    
    [SerializeField] private TargetWhen targetWhen;
    
    [SerializeField] private TargetingMethod targetingMethod;
    
    [Header("Targeting Settings")]
    [ConditionalField(true, nameof(IsPreset))]
    [SerializeField] private Vector3 targetPosition;

    [ConditionalField(true, nameof(WhenAtSetpoint))] [SerializeField]
    private float SetpointName;
    
    [ConditionalField(true, nameof(IsPreset), true)]
    [SerializeField] private Vector3[] extraTargets;

    [Header("Tuning Settings")]
    [ConditionalField(true, nameof(IsInterpolating), true)] 
    [SerializeField] private float heightOffset;
    [ConditionalField(true, nameof(IsInterpolating), true)] 
    [SerializeField] private float angleOffset;
    [ConditionalField(true, nameof(IsInterpolating), true)] 
    [SerializeField] private Transform originOveride;

    [ConditionalField(true, nameof(IsInterpolating))] 
    [SerializeField] private DistanceValue[] interpolationTable;
    private bool IsPreset() => targetType == TargetType.Preset;
    private bool WhenAtSetpoint() => targetWhen == TargetWhen.AtSetpoint;
    private bool IsInterpolating() => targetingMethod == TargetingMethod.Interpolation;
    
    private List<Vector3> _allTargets;
    
    private DistanceValue[] _sortedCache;
    
    private JointController _controller;

    private bool _lateStartup;
    // Start is called before the first frame update
    void Start()
    {
        InitializeCache();
        var foundTargets = Utils.FindGameObjectsOnLayer("AutoAlignNodes");

        _allTargets = new List<Vector3>();
        
        foreach (var target in foundTargets)
        {
            _allTargets.Add(target.transform.position); 
        }

        foreach (var target in extraTargets)
        {
            _allTargets.Add(target);
        }
        
        _lateStartup = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (_lateStartup)
        {
            _controller = GetComponent<BuildMechanism>().GetController();
            _lateStartup = false;
        }
    }
    
    //runs on editor change
    private void OnValidate()
    {
        UpdateTable(interpolationTable);
    }
    
    //Get target
    private Vector3 GetTargetValue()
    {
        switch (targetType)
        {
            case TargetType.Preset:
                return targetPosition;
            case TargetType.Closest:
                return getClosestTarget();
            case TargetType.Furthest:
                return getFurthestTarget();
        }
        
        return Vector3.zero;
    }

    private Vector3 getClosestTarget()
    {
        float closestDistance = float.MaxValue;
        Vector3 closestTarget = Vector3.zero;
        foreach (var target in _allTargets)
        {
            var distance = Vector3.Distance(originOveride.position, transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestTarget = target;
            }
        }
        
        return closestTarget;
    }
    
    private Vector3 getFurthestTarget()
    {
        float furthestDistance = float.MinValue;
        Vector3 furthestTarget = Vector3.zero;
        foreach (var target in _allTargets)
        {
            var distance = Vector3.Distance(originOveride.position, transform.position);
            if (distance > furthestDistance)
            {
                furthestDistance = distance;
                furthestTarget = target;
            }
        }
        
        return furthestTarget;
    }

    //direct calculation stuff
    private float calculateTargetAngle(Transform target)
    {
        Vector3 targetPos = target.position + (Vector3.up * heightOffset);
        Vector3 originPos = originOveride ? originOveride.position : transform.position;

        Vector3 dirToTarget = targetPos - originPos;

        Vector3 localDir = transform.InverseTransformDirection(dirToTarget);

        float angleRad = Mathf.Atan2(localDir.y, localDir.z);
        float angleDeg = angleRad * Mathf.Rad2Deg;

        return angleDeg;
    }
    
    //Interpolation stuff
    private void InitializeCache()
    {
        if (interpolationTable == null || interpolationTable.Length == 0)
        {
            _sortedCache = Array.Empty<DistanceValue>();
            return;
        }

        // Allocate the cache array exactly once
        _sortedCache = new DistanceValue[interpolationTable.Length];
    
        // Copy the serialized data to our working cache
        Array.Copy(interpolationTable, _sortedCache, interpolationTable.Length);

        // Sort the cache immediately to enable Binary Search
        // Using the struct comparer prevents boxing allocations
        Array.Sort(_sortedCache, new DistanceComparer());
    }
    
    private void UpdateTable(DistanceValue[] newData)
    {
        // Avoid re-allocating if the size hasn't changed
        if (_sortedCache == null || _sortedCache.Length != newData.Length)
        {
            _sortedCache = new DistanceValue[newData.Length];
        }
        
        Array.Copy(newData, _sortedCache, newData.Length);
        Array.Sort(_sortedCache, (a, b) => a.distance.CompareTo(b.distance));
    }

    private float Interpolate(float currentDistance)
    {
        if (_sortedCache == null || _sortedCache.Length == 0) return 0f;

        // BinarySearch on a struct array is O(log n) and zero GC
        int index = Array.BinarySearch(_sortedCache, new DistanceValue { distance = currentDistance }, new DistanceComparer());

        if (index >= 0) return _sortedCache[index].value;

        int nextIndex = ~index;

        // Handle bounds
        if (nextIndex == 0) return _sortedCache[0].value;
        if (nextIndex >= _sortedCache.Length) return _sortedCache[_sortedCache.Length - 1].value;
        
        var lower = _sortedCache[nextIndex - 1];
        var upper = _sortedCache[nextIndex];
        float t = (currentDistance - lower.distance) / (upper.distance - lower.distance);
        return Mathf.Lerp(lower.value, upper.value, t);
    }

    [Serializable]
    private struct DistanceValue
    {
        public float distance;
        public float value;
    }
    
    private struct DistanceComparer : System.Collections.Generic.IComparer<DistanceValue>
    {
        public int Compare(DistanceValue x, DistanceValue y) => x.distance.CompareTo(y.distance);
    }
}
