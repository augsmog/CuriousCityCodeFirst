using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace CuriousCity.Data
{
/// <summary>
/// Represents a single choice made by the player during gameplay.
/// Tracks decision-making patterns for learning analytics and narrative consequences.
/// </summary>
[Serializable]
[CreateAssetMenu(fileName = "New", menuName = "CuriousCity/Data")]
public class PlayerChoice
{
    [Header("Choice Identification")]
    public string choiceId;
    public string choiceCategory;
    public string sceneName;
    public float timestamp;
    
    [Header("Choice Details")]
    public string choiceType; // "empathetic", "logical", "creative", "aggressive", etc.
    public string choiceDescription;
    public string chosenOption;
    public List<string> availableOptions;
    public bool wasDefaultChoice;
    
    [Header("Decision Metrics")]
    public float decisionTime; // Time taken to make the choice
    public float confidence; // Estimated confidence (based on decision time and behavior)
    public int revisitCount; // Times player changed their mind
    public bool usedHint;
    
    [Header("Context")]
    public string npcInvolved;
    public string puzzleContext;
    public Dictionary<string, object> environmentalFactors;
    public EmotionalContext emotionalContext;
    
    [Header("Consequences")]
    public float impact; // Magnitude of impact (0-1)
    public ImpactType impactType;
    public Dictionary<string, float> systemImpacts; // morale, power, etc.
    public List<string> narrativeConsequences;
    public List<string> unlockedContent;
    
    [Header("Learning Analytics")]
    public Dictionary<string, float> learningIndicators;
    public ProblemSolvingApproach approach;
    public bool alignsWithLearningStyle;
    public float cognitiveLoad;
    
    /// <summary>
    /// Default constructor
    /// </summary>
    public PlayerChoice()
    {
        availableOptions = new List<string>();
        environmentalFactors = new Dictionary<string, object>();
        systemImpacts = new Dictionary<string, float>();
        narrativeConsequences = new List<string>();
        unlockedContent = new List<string>();
        learningIndicators = new Dictionary<string, float>();
        timestamp = Time.time;
        emotionalContext = new EmotionalContext();
    }
    
    /// <summary>
    /// Constructor with basic parameters
    /// </summary>
    public PlayerChoice(string id, string type, string chosen, float impact = 0.5f)
    {
        this.choiceId = id;
        this.choiceType = type;
        this.chosenOption = chosen;
        this.impact = impact;
        
        // Initialize collections
        availableOptions = new List<string>();
        environmentalFactors = new Dictionary<string, object>();
        systemImpacts = new Dictionary<string, float>();
        narrativeConsequences = new List<string>();
        unlockedContent = new List<string>();
        learningIndicators = new Dictionary<string, float>();
        timestamp = Time.time;
        emotionalContext = new EmotionalContext();
    }
    
    /// <summary>
    /// Categorize the choice for analytics
    /// </summary>
    public ChoiceCategory GetCategory()
    {
        switch (choiceType.ToLower())
        {
            case "empathetic":
            case "compassionate":
            case "supportive":
                return ChoiceCategory.Emotional;
                
            case "logical":
            case "analytical":
            case "strategic":
                return ChoiceCategory.Logical;
                
            case "creative":
            case "innovative":
            case "unconventional":
                return ChoiceCategory.Creative;
                
            case "aggressive":
            case "assertive":
            case "confrontational":
                return ChoiceCategory.Aggressive;
                
            case "diplomatic":
            case "negotiating":
            case "mediating":
                return ChoiceCategory.Diplomatic;
                
            default:
                return ChoiceCategory.Neutral;
        }
    }
    
    /// <summary>
    /// Calculate the total system impact of this choice
    /// </summary>
    public float CalculateTotalImpact()
    {
        if (systemImpacts == null || systemImpacts.Count == 0)
            return impact;
        
        float total = 0f;
        foreach (var kvp in systemImpacts)
        {
            total += Mathf.Abs(kvp.Value);
        }
        return total / systemImpacts.Count;
    }
    
    /// <summary>
    /// Determine if this was a considered decision
    /// </summary>
    public bool WasConsideredDecision()
    {
        return decisionTime > 2f && revisitCount > 0;
    }
    
    /// <summary>
    /// Get a description of the decision-making process
    /// </summary>
    public string GetDecisionProcessDescription()
    {
        if (decisionTime < 1f)
            return "Quick instinctive choice";
        else if (decisionTime < 3f)
            return "Brief consideration";
        else if (decisionTime < 10f)
            return "Thoughtful deliberation";
        else
            return "Extended analysis";
    }
}

/// <summary>
/// Emotional context during the choice
/// </summary>
[Serializable]
[CreateAssetMenu(fileName = "New", menuName = "CuriousCity/Data")]
public class EmotionalContext
{
    public float stress;
    public float confidence;
    public float curiosity;
    public float frustration;
    public string dominantEmotion;
    
    public EmotionalContext()
    {
        stress = 0.5f;
        confidence = 0.5f;
        curiosity = 0.5f;
        frustration = 0f;
        dominantEmotion = "neutral";
    }
}

/// <summary>
/// Categories for player choices
/// </summary>
public enum ChoiceCategory
{
    Emotional,
    Logical,
    Creative,
    Aggressive,
    Diplomatic,
    Neutral
}

/// <summary>
/// Types of impact a choice can have
/// </summary>
public enum ImpactType
{
    Immediate,
    Delayed,
    Cascading,
    Persistent,
    Temporary
}

/// <summary>
/// Problem-solving approaches
/// </summary>
public enum ProblemSolvingApproach
{
    Systematic,
    Intuitive,
    TrialAndError,
    Analytical,
    Creative,
    Collaborative,
    Avoidant
}
}