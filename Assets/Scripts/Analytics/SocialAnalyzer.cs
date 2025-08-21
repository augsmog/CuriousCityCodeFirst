using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using CuriousCityAutomated.Analytics;

/// <summary>
/// Analyzes social interaction patterns, emotional intelligence, and interpersonal learning preferences
/// </summary>
public class SocialAnalyzer : MonoBehaviour
{
    [Header("Analysis Configuration")]
    public float emotionalStateUpdateInterval = 2f;
    public float socialComfortThreshold = 0.6f;
    public int conversationHistorySize = 50;
    
    [Header("Behavioral Thresholds")]
    public float quickResponseThreshold = 2f;
    public float thoughtfulResponseThreshold = 5f;
    public float empathyIndicatorThreshold = 0.7f;
    
    // Dependencies
    private LearningStyleTracker learningTracker;
    private CrewInteractionManager crewManager;
    
    // Social tracking data
    private List<SocialInteractionEvent> interactionHistory;
    private Dictionary<string, RelationshipMetrics> relationshipData;
    private EmotionalStateProfile currentEmotionalState;
    private Dictionary<string, float> emotionalResponsePatterns;
    
    // Behavioral metrics
    private float socialConfidenceScore;
    private float empathyScore;
    private float collaborationScore;
    private float conflictResolutionScore;
    private float leadershipScore;
    
    // Pattern analysis
    private bool prefersGroupInteraction;
    private bool showsEmotionalAwareness;
    private bool demonstratesActiveLlistening;
    private string dominantCommunicationStyle;
    
    [System.Serializable]
    public class SocialInteractionEvent
    {
        public string characterId;
        public string interactionType;
        public float timestamp;
        public float responseTime;
        public string emotionalContext;
        public Dictionary<string, float> emotionalMarkers;
        public bool initiatedByPlayer;
        public string outcome;
        
        public SocialInteractionEvent()
        {
            emotionalMarkers = new Dictionary<string, float>();
        }
    }
    
    [System.Serializable]
    public class RelationshipMetrics
    {
        public string characterId;
        public float trustLevel;
        public float rapportLevel;
        public int interactionCount;
        public float averageResponseTime;
        public List<string> topicsDiscussed;
        public Dictionary<string, int> emotionalResponses;
        public float conflictFrequency;
        public float positiveInteractionRatio;
        
        public RelationshipMetrics()
        {
            topicsDiscussed = new List<string>();
            emotionalResponses = new Dictionary<string, int>();
        }
    }
    
    [System.Serializable]
    public class EmotionalStateProfile
    {
        public float currentMood; // -1 to 1 (negative to positive)
        public float emotionalStability;
        public float socialEnergy;
        public float empathyLevel;
        public float assertiveness;
        public Dictionary<string, float> emotionalTriggers;
        
        public EmotionalStateProfile()
        {
            emotionalTriggers = new Dictionary<string, float>();
        }
    }
    
    void Start()
    {
        InitializeSocialAnalyzer();
    }
    
    void InitializeSocialAnalyzer()
    {
        learningTracker = FindFirstObjectByType<LearningStyleTracker>();
        crewManager = FindFirstObjectByType<CrewInteractionManager>();
        
        interactionHistory = new List<SocialInteractionEvent>();
        relationshipData = new Dictionary<string, RelationshipMetrics>();
        emotionalResponsePatterns = new Dictionary<string, float>();
        currentEmotionalState = new EmotionalStateProfile();
        
        // Initialize emotional state
        currentEmotionalState.currentMood = 0f;
        currentEmotionalState.emotionalStability = 0.5f;
        currentEmotionalState.socialEnergy = 0.7f;
        currentEmotionalState.empathyLevel = 0.3f;
        currentEmotionalState.assertiveness = 0.5f;
        
        if (crewManager)
        {
            SubscribeToSocialEvents();
        }
        
        InvokeRepeating(nameof(UpdateEmotionalState), emotionalStateUpdateInterval, emotionalStateUpdateInterval);
    }
    
    void SubscribeToSocialEvents()
    {
        // Subscribe to crew interaction events
        CrewInteractionManager.OnRelationshipChanged += OnRelationshipChanged;
    }
    
    public void AnalyzeSocialInteraction(string characterId, DialogueChoice choice, float responseTime)
    {
        var socialEvent = new SocialInteractionEvent
        {
            characterId = characterId,
            interactionType = DetermineInteractionType(choice),
            timestamp = Time.time,
            responseTime = responseTime,
            emotionalContext = AnalyzeEmotionalContext(choice),
            initiatedByPlayer = true
        };
        
        // Analyze emotional markers
        socialEvent.emotionalMarkers["empathy"] = MeasureEmpathyLevel(choice);
        socialEvent.emotionalMarkers["assertiveness"] = MeasureAssertiveness(choice);
        socialEvent.emotionalMarkers["warmth"] = MeasureWarmth(choice);
        socialEvent.emotionalMarkers["logic"] = MeasureLogicalApproach(choice);
        
        // Determine outcome
        socialEvent.outcome = DetermineInteractionOutcome(socialEvent);
        
        // Update tracking
        interactionHistory.Add(socialEvent);
        UpdateRelationshipMetrics(socialEvent);
        
        // Analyze patterns
        if (interactionHistory.Count % 5 == 0)
        {
            AnalyzeSocialPatterns();
        }
        
        // Log insights
        LogSocialInsights(socialEvent);
    }
    
    string DetermineInteractionType(DialogueChoice choice)
    {
        string choiceText = choice.choiceText.ToLower();
        
        if (choiceText.Contains("help") || choiceText.Contains("support"))
            return "supportive";
        if (choiceText.Contains("question") || choiceText.Contains("curious"))
            return "inquisitive";
        if (choiceText.Contains("agree") || choiceText.Contains("understand"))
            return "agreeable";
        if (choiceText.Contains("disagree") || choiceText.Contains("but"))
            return "challenging";
        if (choiceText.Contains("teach") || choiceText.Contains("explain"))
            return "educational";
        if (choiceText.Contains("joke") || choiceText.Contains("funny"))
            return "humorous";
        
        return "neutral";
    }
    
    string AnalyzeEmotionalContext(DialogueChoice choice)
    {
        float empathy = MeasureEmpathyLevel(choice);
        float logic = MeasureLogicalApproach(choice);
        
        if (empathy > logic && empathy > empathyIndicatorThreshold)
            return "emotionally_supportive";
        if (logic > empathy && logic > 0.7f)
            return "analytically_focused";
        if (choice.choiceText.Contains("!") || choice.choiceText.Contains("excited"))
            return "enthusiastic";
        if (choice.choiceText.Contains("?") && choice.choiceText.Split('?').Length > 2)
            return "highly_curious";
        
        return "balanced";
    }
    
    float MeasureEmpathyLevel(DialogueChoice choice)
    {
        float score = 0f;
        string text = choice.choiceText.ToLower();
        
        // Empathy indicators
        string[] empathyWords = { "understand", "feel", "sorry", "care", "support", 
                                 "help", "comfort", "concern", "worry", "appreciate" };
        
        foreach (string word in empathyWords)
        {
            if (text.Contains(word)) score += 0.2f;
        }
        
        // Relationship impact as empathy indicator
        if (choice.relationshipChange > 0) score += choice.relationshipChange;
        
        return Mathf.Clamp01(score);
    }
    
    float MeasureAssertiveness(DialogueChoice choice)
    {
        float score = 0.5f; // Neutral baseline
        string text = choice.choiceText.ToLower();
        
        // Assertive indicators
        if (text.StartsWith("i think") || text.StartsWith("i believe")) score += 0.3f;
        if (text.Contains("should") || text.Contains("must")) score += 0.2f;
        if (text.Contains("definitely") || text.Contains("certainly")) score += 0.2f;
        
        // Passive indicators
        if (text.Contains("maybe") || text.Contains("perhaps")) score -= 0.2f;
        if (text.Contains("sorry") && !text.Contains("feel sorry for")) score -= 0.2f;
        if (text.EndsWith("?")) score -= 0.1f;
        
        return Mathf.Clamp01(score);
    }
    
    float MeasureWarmth(DialogueChoice choice)
    {
        float score = 0.3f;
        string text = choice.choiceText.ToLower();
        
        string[] warmWords = { "thank", "appreciate", "glad", "happy", "wonderful", 
                              "great", "excellent", "proud", "love", "enjoy" };
        
        foreach (string word in warmWords)
        {
            if (text.Contains(word)) score += 0.15f;
        }
        
        return Mathf.Clamp01(score);
    }
    
    float MeasureLogicalApproach(DialogueChoice choice)
    {
        float score = 0f;
        string text = choice.choiceText.ToLower();
        
        string[] logicWords = { "think", "reason", "because", "therefore", "analyze", 
                               "consider", "evaluate", "conclude", "evidence", "fact" };
        
        foreach (string word in logicWords)
        {
            if (text.Contains(word)) score += 0.2f;
        }
        
        // Structured response patterns
        if (text.Contains("first") && text.Contains("second")) score += 0.3f;
        if (text.Contains("if") && text.Contains("then")) score += 0.2f;
        
        return Mathf.Clamp01(score);
    }
    
    string DetermineInteractionOutcome(SocialInteractionEvent socialEvent)
    {
        float positiveMarkers = socialEvent.emotionalMarkers["empathy"] + 
                               socialEvent.emotionalMarkers["warmth"];
        float assertiveMarkers = socialEvent.emotionalMarkers["assertiveness"];
        
        if (positiveMarkers > 1.2f)
            return "strengthened_bond";
        if (assertiveMarkers > 0.8f && socialEvent.emotionalMarkers["logic"] > 0.6f)
            return "established_leadership";
        if (socialEvent.responseTime < quickResponseThreshold)
            return "confident_interaction";
        if (socialEvent.responseTime > thoughtfulResponseThreshold)
            return "thoughtful_exchange";
        
        return "neutral_exchange";
    }
    
    void UpdateRelationshipMetrics(SocialInteractionEvent socialEvent)
    {
        if (!relationshipData.ContainsKey(socialEvent.characterId))
        {
            relationshipData[socialEvent.characterId] = new RelationshipMetrics
            {
                characterId = socialEvent.characterId
            };
        }
        
        var metrics = relationshipData[socialEvent.characterId];
        metrics.interactionCount++;
        
        // Update response time average
        float totalResponseTime = metrics.averageResponseTime * (metrics.interactionCount - 1);
        metrics.averageResponseTime = (totalResponseTime + socialEvent.responseTime) / metrics.interactionCount;
        
        // Track emotional responses
        if (!metrics.emotionalResponses.ContainsKey(socialEvent.emotionalContext))
            metrics.emotionalResponses[socialEvent.emotionalContext] = 0;
        metrics.emotionalResponses[socialEvent.emotionalContext]++;
        
        // Update relationship quality metrics
        if (socialEvent.outcome.Contains("strengthened") || socialEvent.outcome.Contains("positive"))
        {
            metrics.trustLevel = Mathf.Min(1f, metrics.trustLevel + 0.1f);
            metrics.rapportLevel = Mathf.Min(1f, metrics.rapportLevel + 0.1f);
        }
        
        // Calculate positive interaction ratio
        int positiveCount = metrics.emotionalResponses
            .Where(kvp => kvp.Key.Contains("supportive") || kvp.Key.Contains("enthusiastic"))
            .Sum(kvp => kvp.Value);
        metrics.positiveInteractionRatio = positiveCount / (float)metrics.interactionCount;
    }
    
    void AnalyzeSocialPatterns()
    {
        if (interactionHistory.Count < 10) return;
        
        var recentInteractions = interactionHistory.TakeLast(20).ToList();
        
        // Calculate social confidence
        socialConfidenceScore = CalculateSocialConfidence(recentInteractions);
        
        // Calculate empathy score
        empathyScore = recentInteractions.Average(i => i.emotionalMarkers.GetValueOrDefault("empathy", 0f));
        
        // Analyze communication style
        dominantCommunicationStyle = DetermineCommunicationStyle(recentInteractions);
        
        // Check for emotional awareness
        showsEmotionalAwareness = AnalyzeEmotionalAwareness(recentInteractions);
        
        // Analyze group interaction preference
        prefersGroupInteraction = AnalyzeGroupPreference();
        
        // Calculate collaboration score
        collaborationScore = CalculateCollaborationScore(recentInteractions);
        
        // Leadership indicators
        leadershipScore = CalculateLeadershipScore(recentInteractions);
        
        LogPatternAnalysis();
    }
    
    float CalculateSocialConfidence(List<SocialInteractionEvent> interactions)
    {
        float quickResponses = interactions.Count(i => i.responseTime < quickResponseThreshold) / (float)interactions.Count;
        float initiatedInteractions = interactions.Count(i => i.initiatedByPlayer) / (float)interactions.Count;
        float assertivenessAverage = interactions.Average(i => i.emotionalMarkers.GetValueOrDefault("assertiveness", 0.5f));
        
        return (quickResponses + initiatedInteractions + assertivenessAverage) / 3f;
    }
    
    string DetermineCommunicationStyle(List<SocialInteractionEvent> interactions)
    {
        var typeCounts = interactions.GroupBy(i => i.interactionType)
            .OrderByDescending(g => g.Count())
            .ToDictionary(g => g.Key, g => g.Count());
        
        if (typeCounts.GetValueOrDefault("supportive", 0) > interactions.Count * 0.4f)
            return "nurturing_supportive";
        if (typeCounts.GetValueOrDefault("inquisitive", 0) > interactions.Count * 0.4f)
            return "curious_learner";
        if (typeCounts.GetValueOrDefault("challenging", 0) > interactions.Count * 0.3f)
            return "analytical_challenger";
        if (typeCounts.GetValueOrDefault("educational", 0) > interactions.Count * 0.3f)
            return "teacher_mentor";
        
        return "balanced_communicator";
    }
    
    bool AnalyzeEmotionalAwareness(List<SocialInteractionEvent> interactions)
    {
        // Check for varied emotional responses
        var emotionalContexts = interactions.Select(i => i.emotionalContext).Distinct().Count();
        
        // Check for appropriate emotional responses
        float appropriateResponses = 0f;
        foreach (var interaction in interactions)
        {
            if (interaction.emotionalContext.Contains("supportive") && 
                interaction.emotionalMarkers["empathy"] > 0.6f)
            {
                appropriateResponses++;
            }
        }
        
        return emotionalContexts > 3 && (appropriateResponses / interactions.Count) > 0.6f;
    }
    
    bool AnalyzeGroupPreference()
    {
        // Analyze interaction patterns across different characters
        if (relationshipData.Count < 3) return false;
        
        // Check for balanced interactions across multiple characters
        var interactionCounts = relationshipData.Values.Select(r => r.interactionCount).ToList();
        float variance = CalculateVariance(interactionCounts.Select(i => (float)i).ToList());
        
        // Low variance suggests balanced group interaction
        return variance < 5f && relationshipData.Count > 2;
    }
    
    float CalculateVariance(List<float> values)
    {
        if (values.Count == 0) return 0f;
        float mean = values.Average();
        return values.Average(v => Mathf.Pow(v - mean, 2));
    }
    
    float CalculateCollaborationScore(List<SocialInteractionEvent> interactions)
    {
        float supportiveRatio = interactions.Count(i => i.interactionType == "supportive") / (float)interactions.Count;
        float agreeableRatio = interactions.Count(i => i.interactionType == "agreeable") / (float)interactions.Count;
        float positiveOutcomes = interactions.Count(i => i.outcome.Contains("positive") || i.outcome.Contains("strengthened")) / (float)interactions.Count;
        
        return (supportiveRatio + agreeableRatio + positiveOutcomes) / 3f;
    }
    
    float CalculateLeadershipScore(List<SocialInteractionEvent> interactions)
    {
        float assertiveness = interactions.Average(i => i.emotionalMarkers.GetValueOrDefault("assertiveness", 0.5f));
        float educationalInteractions = interactions.Count(i => i.interactionType == "educational") / (float)interactions.Count;
        float confidenceIndicator = interactions.Count(i => i.responseTime < quickResponseThreshold) / (float)interactions.Count;
        
        return (assertiveness + educationalInteractions + confidenceIndicator) / 3f;
    }
    
    void UpdateEmotionalState()
    {
        if (interactionHistory.Count == 0) return;
        
        var recentInteractions = interactionHistory.TakeLast(10).ToList();
        
        // Update mood based on recent interactions
        float positiveInteractions = recentInteractions.Count(i => 
            i.outcome.Contains("positive") || i.outcome.Contains("strengthened"));
        currentEmotionalState.currentMood = Mathf.Lerp(currentEmotionalState.currentMood, 
            (positiveInteractions / recentInteractions.Count) * 2f - 1f, 0.3f);
        
        // Update emotional stability
        var emotionalVariance = CalculateEmotionalVariance(recentInteractions);
        currentEmotionalState.emotionalStability = Mathf.Lerp(currentEmotionalState.emotionalStability,
            1f - emotionalVariance, 0.2f);
        
        // Update social energy
        float interactionFrequency = recentInteractions.Count / 10f;
        currentEmotionalState.socialEnergy = Mathf.Lerp(currentEmotionalState.socialEnergy,
            interactionFrequency, 0.2f);
        
        // Update empathy level
        currentEmotionalState.empathyLevel = Mathf.Lerp(currentEmotionalState.empathyLevel,
            empathyScore, 0.3f);
        
        // Update assertiveness
        float avgAssertiveness = recentInteractions.Average(i => 
            i.emotionalMarkers.GetValueOrDefault("assertiveness", 0.5f));
        currentEmotionalState.assertiveness = Mathf.Lerp(currentEmotionalState.assertiveness,
            avgAssertiveness, 0.2f);
    }
    
    float CalculateEmotionalVariance(List<SocialInteractionEvent> interactions)
    {
        if (interactions.Count < 2) return 0f;
        
        var moods = new List<float>();
        foreach (var interaction in interactions)
        {
            float mood = interaction.emotionalMarkers.GetValueOrDefault("warmth", 0f) - 
                        interaction.emotionalMarkers.GetValueOrDefault("assertiveness", 0.5f) + 0.5f;
            moods.Add(mood);
        }
        
        return CalculateVariance(moods);
    }
    
    void LogSocialInsights(SocialInteractionEvent socialEvent)
    {
        if (learningTracker == null) return;
        
        learningTracker.LogDetailedEvent("social_interaction_analysis",
            $"Detailed social interaction with {socialEvent.characterId}",
            "social_learning",
            new Dictionary<string, object>
            {
                {"character_id", socialEvent.characterId},
                {"interaction_type", socialEvent.interactionType},
                {"response_time", socialEvent.responseTime},
                {"emotional_context", socialEvent.emotionalContext},
                {"empathy_level", socialEvent.emotionalMarkers["empathy"]},
                {"assertiveness", socialEvent.emotionalMarkers["assertiveness"]},
                {"warmth", socialEvent.emotionalMarkers["warmth"]},
                {"logical_approach", socialEvent.emotionalMarkers["logic"]},
                {"outcome", socialEvent.outcome},
                {"shows_quick_social_processing", socialEvent.responseTime < quickResponseThreshold},
                {"shows_thoughtful_consideration", socialEvent.responseTime > thoughtfulResponseThreshold}
            });
    }
    
    void LogPatternAnalysis()
    {
        if (learningTracker == null) return;
        
        learningTracker.LogDetailedEvent("social_pattern_analysis",
            "Comprehensive social behavior pattern analysis",
            "interpersonal_intelligence",
            new Dictionary<string, object>
            {
                {"social_confidence_score", socialConfidenceScore},
                {"empathy_score", empathyScore},
                {"collaboration_score", collaborationScore},
                {"leadership_score", leadershipScore},
                {"dominant_communication_style", dominantCommunicationStyle},
                {"shows_emotional_awareness", showsEmotionalAwareness},
                {"prefers_group_interaction", prefersGroupInteraction},
                {"demonstrates_active_listening", demonstratesActiveLlistening},
                {"current_mood", currentEmotionalState.currentMood},
                {"emotional_stability", currentEmotionalState.emotionalStability},
                {"social_energy", currentEmotionalState.socialEnergy}
            });
        
        // Log specific learning indicators
        if (empathyScore > 0.7f)
        {
            learningTracker.LogDetailedEvent("high_empathy_indicator",
                "Player demonstrates high emotional intelligence",
                "interpersonal_strength");
        }
        
        if (leadershipScore > 0.7f)
        {
            learningTracker.LogDetailedEvent("leadership_potential",
                "Player shows natural leadership tendencies",
                "social_confidence");
        }
        
        if (showsEmotionalAwareness)
        {
            learningTracker.LogDetailedEvent("emotional_awareness",
                "Player demonstrates understanding of emotional nuances",
                "emotional_intelligence");
        }
    }
    
    void OnRelationshipChanged(string characterId, float newValue)
    {
        if (relationshipData.ContainsKey(characterId))
        {
            relationshipData[characterId].trustLevel = newValue;
        }
    }
    
    public Dictionary<string, float> GetSocialMetrics()
    {
        return new Dictionary<string, float>
        {
            ["social_confidence"] = socialConfidenceScore,
            ["empathy_level"] = empathyScore,
            ["collaboration_score"] = collaborationScore,
            ["leadership_score"] = leadershipScore,
            ["emotional_stability"] = currentEmotionalState.emotionalStability,
            ["social_energy"] = currentEmotionalState.socialEnergy,
            ["average_response_time"] = GetAverageResponseTime(),
            ["relationship_diversity"] = GetRelationshipDiversity()
        };
    }
    
    float GetAverageResponseTime()
    {
        if (interactionHistory.Count == 0) return 0f;
        return interactionHistory.Average(i => i.responseTime);
    }
    
    float GetRelationshipDiversity()
    {
        if (relationshipData.Count == 0) return 0f;
        
        // Calculate how evenly distributed interactions are
        var interactionCounts = relationshipData.Values.Select(r => r.interactionCount).ToList();
        float total = interactionCounts.Sum();
        float expectedPerCharacter = total / relationshipData.Count;
        
        float diversity = 0f;
        foreach (var count in interactionCounts)
        {
            diversity += 1f - Mathf.Abs(count - expectedPerCharacter) / expectedPerCharacter;
        }
        
        return diversity / relationshipData.Count;
    }
    
    public EmotionalStateProfile GetEmotionalState()
    {
        return currentEmotionalState;
    }
    
    public string GetDominantSocialLearningStyle()
    {
        if (empathyScore > 0.7f && showsEmotionalAwareness)
            return "empathetic_connector";
        if (leadershipScore > 0.7f && socialConfidenceScore > 0.7f)
            return "natural_leader";
        if (collaborationScore > 0.7f && prefersGroupInteraction)
            return "collaborative_learner";
        if (dominantCommunicationStyle.Contains("curious"))
            return "social_explorer";
        if (dominantCommunicationStyle.Contains("analytical"))
            return "analytical_communicator";
        
        return "balanced_social_learner";
    }
    
    void OnDestroy()
    {
        if (crewManager)
        {
            CrewInteractionManager.OnRelationshipChanged -= OnRelationshipChanged;
        }
    }
}