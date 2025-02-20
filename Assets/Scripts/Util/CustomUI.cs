using System;
using MyBox;
using UnityEditor;
using UnityEngine;

namespace Util
{
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
}
