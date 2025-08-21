using System;
using UnityEngine;

namespace CuriousCity.Core
{
    public abstract class BasePuzzle : MonoBehaviour
    {
        [Header("Base Puzzle Configuration")]
        public string puzzleName;
        public int difficultyLevel = 1;
        public float timeLimit = 300f;
        
        public System.Action OnPuzzleSolved;
        public System.Action OnAttemptMade;
        public System.Action<string> OnInteractionLogged;
        
        protected bool isCompleted = false;
        protected float startTime;
        
        protected virtual void Start()
        {
            startTime = Time.time;
            InitializePuzzle();
        }
        
        protected abstract void InitializePuzzle();
        
        protected virtual void CompletePuzzle()
        {
            if (isCompleted) return;
            isCompleted = true;
            OnPuzzleSolved?.Invoke();
        }
        
        protected virtual void LogAttempt()
        {
            OnAttemptMade?.Invoke();
        }
        
        protected virtual void LogInteraction(string interactionType)
        {
            OnInteractionLogged?.Invoke(interactionType);
        }
    }
}