using System;
using System.Collections.Generic;
using MyBox;
using UnityEditor;
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
        public float point;
        
        [Header("Control Settings")]
        public string controllerButton;
        public string keyboardButton; 
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
