using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Core data structures for the learning analytics system
/// </summary>

[Serializable]
public class RealTimeMetrics
{
    public float engagementLevel;
    public float frustrationLevel;
    public float confidenceLevel;
    public float curiosityLevel;
    public float focusLevel;
    public float timestamp;
    
    public RealTimeMetrics()
    {
        timestamp = Time.time;
    }
}

[Serializable]
public class MovementAnalytics
{
    public float totalDistance;
    public float averageSpeed;
    public int directionChanges;
    public float explorationEfficiency;
    public List<Vector3> pathPoints;
    public Dictionary<string, float> zoneTimeSpent;
    
    public MovementAnalytics()
    {
        pathPoints = new List<Vector3>();
        zoneTimeSpent = new Dictionary<string, float>();
    }
}

[Serializable]
public class InteractionAnalytics
{
    public string objectName;
    public string interactionType;
    public float responseTime;
    public float hoverDuration;
    public int attemptCount;
    public bool successful;
    public Vector3 interactionPosition;
    public Dictionary<string, object> contextData;
    
    public InteractionAnalytics()
    {
        contextData = new Dictionary<string, object>();
    }
}

[Serializable]
public class CognitiveLoadAnalytics
{
    public float taskSwitchingFrequency;
    public float averageResponseTime;
    public float errorRate;
    public int hintsUsed;
    public float problemSolvingEfficiency;
    public List<float> responseTimeHistory;
    
    public CognitiveLoadAnalytics()
    {
        responseTimeHistory = new List<float>();
    }
}

[Serializable]
public class DecisionEvent
{
    public string decisionId;
    public string chosenOption;
    public List<string> availableOptions;
    public float decisionTime;
    public float confidence;
    public Dictionary<string, float> optionConsiderationTime;
    
    public DecisionEvent()
    {
        availableOptions = new List<string>();
        optionConsiderationTime = new Dictionary<string, float>();
    }
}

[Serializable]
public class LearningSession
{
    public string sessionId;
    public DateTime startTime;
    public DateTime endTime;
    public float totalDuration;
    public Dictionary<string, float> learningStyleScores;
    public List<LearningEvent> events;
    public Dictionary<string, object> aggregateMetrics;
    
    public LearningSession()
    {
        sessionId = Guid.NewGuid().ToString();
        startTime = DateTime.Now;
        learningStyleScores = new Dictionary<string, float>();
        events = new List<LearningEvent>();
        aggregateMetrics = new Dictionary<string, object>();
    }
}

[Serializable]
public class LearningEvent
{
    public string eventType;
    public float timestamp;
    public string context;
    public Dictionary<string, object> data;
    
    public LearningEvent(string type, string ctx)
    {
        eventType = type;
        timestamp = Time.time;
        context = ctx;
        data = new Dictionary<string, object>();
    }
}

[Serializable]
public class AttentionMetrics
{
    public float focusDuration;
    public int distractionCount;
    public float averageGazeStability;
    public Dictionary<string, float> objectAttentionTime;
    public List<string> attentionSequence;
    
    public AttentionMetrics()
    {
        objectAttentionTime = new Dictionary<string, float>();
        attentionSequence = new List<string>();
    }
}