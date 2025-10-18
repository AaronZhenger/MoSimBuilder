using System;
using System.Collections.Generic;
using MyBox;
using UnityEngine;

namespace Util
{
    [Serializable]
    public class InspectorDropdown
    {
        [HideInInspector] public List<String> canBeSelected = new List<String>();
        [HideInInspector] public int selectedIndex = 0;
        [HideInInspector] public string selectedName = "";
    }
    [Serializable]
    public class SetPoint
    {
        public string setpointName;
        
        [Header("Behaviour Settings")]
        public ControlType controlType;
        
        [ConditionalField(nameof(controlType), false,ControlType.Sequence)]
        public SequenceType sequenceType;
        
        [ConditionalField(nameof(controlType), false,ControlType.Sequence)]
        public string sequenceTo;
        
        [ConditionalField(true, nameof(Predicate))]
        public float delay;
        private bool Predicate() => controlType == ControlType.Sequence && sequenceType == SequenceType.delay;
        
        
        [Header("Generic")]
        [SerializeField]
        private float point;
        
        [SerializeField]
        private bool shouldScaleToUnits = false;
        [ConditionalField(nameof(shouldScaleToUnits))]
        public Units units;

        public float getPoint()
        {
            return shouldScaleToUnits ? point * units switch
            {
                Units.Inch => 0.0254f,
                Units.Centimeter => 0.01f,
                Units.Meter => 1.0f,
                Units.Millimeter => 0.001f,
                _ => 1.0f
                
            } : point;
        }

        [Header("Control Settings")]
        public ControllerInputs controllerButton;
        public KeyboardInputs keyboardButton; 
    }
    
    [Serializable]
    public struct PID
    {
        public float p;
        public float i;
        public float d;
        public float max;
    }
}
