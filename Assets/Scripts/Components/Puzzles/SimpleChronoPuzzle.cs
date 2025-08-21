using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace CuriousCity.Core
{
    public class SimpleChronoPuzzle : BasePuzzle
    {
        public Button testButton;
        
        protected override void InitializePuzzle()
        {
            Debug.Log("Simple Chrono Puzzle Initialized");
        }
    }
}