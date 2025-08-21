using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace CuriousCity.Data
{
/// <summary>
/// Data structure containing comprehensive results from a completed mission.
/// Tracks player performance, choices made, and learning analytics data.
/// </summary>
[Serializable]
[CreateAssetMenu(fileName = "New", menuName = "CuriousCity/Data")]
public class MissionResults
{
    [Header("Mission Identification")]
    public string missionId;
    public string missionName;
    public string historicalPeriod;
    public DateTime completionDate;
    
    [Header("Core Results")]
    public bool success;
    public bool artifactRecovered;
    public ArtifactData artifact;
    public float completionTime;
    public int puzzlesCompleted;
    public int totalPuzzles;
    
    [Header("Player Choices")]
    public List<PlayerChoice> playerChoices;
    public Dictionary<string, int> choiceTypeCounts;
    
    [Header("Performance Metrics")]
    public float explorationEfficiency;
    public float puzzleSolveSpeed;
    public int hintsUsed;
    public int mistakesMade;
    public float accuracyRate;
    
    [Header("Learning Analytics")]
    public Dictionary<string, float> learningStyleMetrics;
    public string dominantLearningStyle;
    public float engagementLevel;
    public float frustrationLevel;
    public float confidenceLevel;
    
    [Header("Behavioral Insights")]
    public int interactionCount;
    public float averageDecisionTime;
    public bool showedSystematicApproach;
    public bool demonstratedCreativity;
    public float collaborationScore;
    
    [Header("Ark Impact")]
    public float moraleImpact;
    public float powerImpact;
    public float populationImpact;
    public float colonyProgressImpact;
    public List<string> unlockedFeatures;
    
    [Header("Chrona Evolution")]
    public float chronaEmpathyChange;
    public float chronaLogicChange;
    public string chronaReaction;
    
    /// <summary>
    /// Constructor with default initialization
    /// </summary>
    public MissionResults()
    {
        playerChoices = new List<PlayerChoice>();
        choiceTypeCounts = new Dictionary<string, int>();
        learningStyleMetrics = new Dictionary<string, float>();
        unlockedFeatures = new List<string>();
        completionDate = DateTime.Now;
    }
    
    /// <summary>
    /// Calculate overall mission score based on various factors
    /// </summary>
    public float CalculateOverallScore()
    {
        float score = 0f;
        
        // Base score for completion
        if (success) score += 50f;
        if (artifactRecovered) score += 30f;
        
        // Performance bonuses
        score += (puzzlesCompleted / (float)totalPuzzles) * 20f;
        score += Mathf.Clamp01(1f - (completionTime / 3600f)) * 10f; // Time bonus (under 1 hour)
        score += accuracyRate * 10f;
        
        // Deductions
        score -= hintsUsed * 2f;
        score -= mistakesMade * 1f;
        
        // Engagement bonus
        score += engagementLevel * 10f;
        
        return Mathf.Clamp(score, 0f, 100f);
    }
    
    /// <summary>
    /// Get a summary of player choices by type
    /// </summary>
    public Dictionary<string, float> GetChoiceTypePercentages()
    {
        var percentages = new Dictionary<string, float>();
        int totalChoices = playerChoices.Count;
        
        if (totalChoices == 0) return percentages;
        
        // Count choices by type
        foreach (var choice in playerChoices)
        {
            if (!choiceTypeCounts.ContainsKey(choice.choiceType))
                choiceTypeCounts[choice.choiceType] = 0;
            choiceTypeCounts[choice.choiceType]++;
        }
        
        // Calculate percentages
        foreach (var kvp in choiceTypeCounts)
        {
            percentages[kvp.Key] = (kvp.Value / (float)totalChoices) * 100f;
        }
        
        return percentages;
    }
    
    /// <summary>
    /// Analyze choices to determine player tendencies
    /// </summary>
    public PlayerTendencies AnalyzePlayerTendencies()
    {
        var tendencies = new PlayerTendencies();
        var choicePercentages = GetChoiceTypePercentages();
        
        // Analyze choice patterns
        if (choicePercentages.ContainsKey("empathetic") && choicePercentages["empathetic"] > 40f)
            tendencies.isEmpathetic = true;
        
        if (choicePercentages.ContainsKey("logical") && choicePercentages["logical"] > 40f)
            tendencies.isLogical = true;
        
        if (choicePercentages.ContainsKey("creative") && choicePercentages["creative"] > 30f)
            tendencies.isCreative = true;
        
        // Analyze behavioral patterns
        tendencies.isMethodical = showedSystematicApproach;
        tendencies.isExplorative = explorationEfficiency < 0.7f; // Less efficient = more explorative
        tendencies.isCollaborative = collaborationScore > 0.6f;
        tendencies.isCautious = averageDecisionTime > 5f;
        tendencies.isConfident = confidenceLevel > 0.7f;
        
        return tendencies;
    }
    
    /// <summary>
    /// Generate a text summary of the mission results
    /// </summary>
    public string GenerateSummary()
    {
        string summary = $"Mission: {missionName}\n";
        summary += $"Status: {(success ? "Completed" : "Failed")}\n";
        summary += $"Time: {FormatTime(completionTime)}\n";
        summary += $"Puzzles: {puzzlesCompleted}/{totalPuzzles} solved\n";
        summary += $"Learning Style: {dominantLearningStyle}\n";
        summary += $"Score: {CalculateOverallScore():F0}/100";
        
        return summary;
    }
    
    private string FormatTime(float seconds)
    {
        int minutes = (int)(seconds / 60);
        int secs = (int)(seconds % 60);
        return $"{minutes}:{secs:D2}";
    }
}

/// <summary>
/// Helper class for analyzing player tendencies
/// </summary>
[Serializable]
[CreateAssetMenu(fileName = "New", menuName = "CuriousCity/Data")]
public class PlayerTendencies
{
    public bool isEmpathetic;
    public bool isLogical;
    public bool isCreative;
    public bool isMethodical;
    public bool isExplorative;
    public bool isCollaborative;
    public bool isCautious;
    public bool isConfident;
    
    public string GetPrimaryTendency()
    {
        if (isEmpathetic) return "Empathetic";
        if (isLogical) return "Logical";
        if (isCreative) return "Creative";
        if (isMethodical) return "Methodical";
        return "Balanced";
    }
}
}