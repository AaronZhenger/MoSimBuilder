using System;
using System.Collections.Generic;
using MyBox;
using UnityEditor;
using UnityEngine;
using Util;

public class AutoAim : MonoBehaviour
{
    [SerializeField] private TargetType targetType;

    [SerializeField] private AimAtWhen targetWhen;
    
    [Header("Targeting Settings")]
    [ConditionalField(true, nameof(IsPreset))]
    [SerializeField] private Vector3 targetPosition;

    [ConditionalField(true, nameof(WhenAtSetpoint))] 
    [SerializeField] private BuildMechanism drivingMechanism;
    [ConditionalField(true, nameof(WhenAtSetpoint))] [SerializeField]
    private string SetpointName;
    [ConditionalField(true, nameof(WhenAtSetpoint))]
    [SerializeField] private string connectedTo = "none";

    [ConditionalField(true, nameof(IsPreset), true)]
    [SerializeField] private Vector3[] extraTargets;

    [Header("Tuning Settings")]
    [ConditionalField(true, nameof(IsPlaying))]
    [SerializeField] private float Distance;
    [ConditionalField(true, nameof(IsPlaying))]
    [SerializeField] private float Output;
    private bool IsPreset() => targetType == TargetType.Preset;
    private bool WhenAtSetpoint() => targetWhen == AimAtWhen.AtSetpoint;
    
    private bool IsPlaying() => EditorApplication.isPlaying;

    private SwerveController controller;

    private List<Vector3> _allTargets = new List<Vector3>();
    private void Start()
    {
        if (!EditorApplication.isPlaying) return;
        
        var foundTargets = Utils.FindGameObjectsOnLayer("AutoAngleNodes");
        
        foreach (var target in foundTargets)
        {
            _allTargets.Add(target.transform.position); 
        }

        controller = GetComponent<SwerveController>();
        _allTargets.AddRange(extraTargets);
    }

    private void Update()
    {
        connectedTo = drivingMechanism ? drivingMechanism.name : "none";

        if (!EditorApplication.isPlaying) return;
        
        var currentSetpoint = "";
        if (drivingMechanism && drivingMechanism.GetController())
        {
            currentSetpoint = drivingMechanism.GetController().getActiveSetpoint();
        }
        else if (targetWhen == AimAtWhen.AtSetpoint)
        {
            return;
        }

        bool shouldTarget = targetWhen == AimAtWhen.Always || 
                            (targetWhen == AimAtWhen.AtSetpoint && 
                             String.Equals(
                                 (currentSetpoint ?? "").ToLower().Trim(), 
                                 SetpointName.ToLower().Trim(), 
                                 StringComparison.OrdinalIgnoreCase));
        
        if (!shouldTarget) return;
        
        Vector3 target = GetTargetValue();

        var angle = CalculateTargetAngle(target);

        controller.OverideSteer(angle, true);
    }
    
    private float CalculateTargetAngle(Vector3 targetPos)
    {
        Transform refPoint = transform;

        Vector3 localTarget = refPoint.parent.InverseTransformPoint(targetPos);
      
        float angleRad = Mathf.Atan2(localTarget.z, localTarget.x);
        float angleDeg = angleRad * Mathf.Rad2Deg;

        return angleDeg;
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
        Vector3 originPos = transform.position;
    
        foreach (var target in _allTargets)
        {
            var distance = Vector3.Distance(originPos, target); 
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
        Vector3 originPos = transform.position;
    
        foreach (var target in _allTargets)
        {
            var distance = Vector3.Distance(originPos, target);
            if (distance > furthestDistance)
            {
                furthestDistance = distance;
                furthestTarget = target;
            }
        }
    
        return furthestTarget;
    }
}