using System;
using System.Collections.Generic;
using UnityEngine;

namespace CuriousCity.Core
{
    /// <summary>
    /// Data structure for puzzle completion results
    /// </summary>
    [Serializable]
    public class PuzzleResults
    {
        public string puzzleType;
        public bool wasSolved;
        public float completionTime;
        public int hintsUsed;
        public int attempts;
        public float score;
        public DateTime completedAt;
        
        // Additional metrics
        public int mistakesMade;
        public float averageAttemptTime;
        public bool usedSkip;
        
        // Properties needed by MissionDataManager
        public List<string> learningStyleTags;
        public Dictionary<string, float> interactionMetrics;
        
        public PuzzleResults()
        {
            puzzleType = "";
            wasSolved = false;
            completionTime = 0f;
            hintsUsed = 0;
            attempts = 0;
            score = 0f;
            completedAt = DateTime.Now;
            mistakesMade = 0;
            averageAttemptTime = 0f;
            usedSkip = false;
            learningStyleTags = new List<string>();
            interactionMetrics = new Dictionary<string, float>();
        }
        
        public PuzzleResults(string type)
        {
            puzzleType = type;
            wasSolved = false;
            completionTime = 0f;
            hintsUsed = 0;
            attempts = 0;
            score = 0f;
            completedAt = DateTime.Now;
            mistakesMade = 0;
            averageAttemptTime = 0f;
            usedSkip = false;
            learningStyleTags = new List<string>();
            interactionMetrics = new Dictionary<string, float>();
        }
        
        /// <summary>
        /// Calculate a score based on performance metrics
        /// </summary>
        public float CalculateScore()
        {
            float baseScore = wasSolved ? 100f : 0f;
            
            // Deduct for hints used
            baseScore -= hintsUsed * 10f;
            
            // Deduct for excessive attempts
            baseScore -= Math.Max(0, attempts - 1) * 5f;
            
            // Bonus for quick completion (under 60 seconds)
            if (completionTime < 60f && wasSolved)
            {
                baseScore += 10f;
            }
            
            // Ensure score doesn't go below 0
            score = Mathf.Max(0f, baseScore);
            return score;
        }
        
        /// <summary>
        /// Get a performance rating based on the results
        /// </summary>
        public string GetPerformanceRating()
        {
            if (!wasSolved) return "Incomplete";
            
            float score = CalculateScore();
            
            if (score >= 90) return "Excellent";
            if (score >= 75) return "Good";
            if (score >= 60) return "Average";
            if (score >= 40) return "Below Average";
            return "Poor";
        }
        
        public override string ToString()
        {
            return $"PuzzleResults: {puzzleType} - Solved: {wasSolved}, Time: {completionTime:F1}s, Attempts: {attempts}, Hints: {hintsUsed}";
        }
    }
}