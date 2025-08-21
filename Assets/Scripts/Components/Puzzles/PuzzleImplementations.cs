using System.Collections.Generic;
using UnityEngine;

namespace CuriousCity.Core
{
    [System.Serializable]
    public class PuzzleInteractionEvent
    {
        public string interactionType;
        public float timestamp;
        public float value;
        public Vector3 mousePosition;
    }
}