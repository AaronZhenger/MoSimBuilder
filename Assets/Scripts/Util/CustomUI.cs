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
        public ControlType controlType;
        public float point;
        public string button;
        [ConditionalField("controlType", false,ControlType.Sequence)]public string sequenceTo;
    }
}
