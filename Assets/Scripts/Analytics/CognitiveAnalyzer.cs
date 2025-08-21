using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using CuriousCityAutomated.Gameplay.Puzzles;  // For PuzzleManager
using CuriousCityAutomated.Analytics;

/// <summary>
/// Analyzes cognitive patterns including problem-solving approaches, memory, attention, and thinking styles
/// </summary>
public class CognitiveAnalyzer : MonoBehaviour
{
    [Header("Analysis Configuration")]
    public float cognitiveLoadUpdateInterval = 1f;
    public int workingMemoryCapacity = 7; // Miller's Law
    public float attentionSpanBaseline = 15f; // minutes
    
    [Header("Cognitive Thresholds")]
    public float highCognitiveLoadThreshold = 0.8f;
    public float lowCognitiveLoadThreshold = 0.3f;
    public float optimalChallengeRange = 0.6f;
    
    // Dependencies
    private LearningStyleTracker learningTracker;
    private PuzzleManager puzzleManager;
    
    // Cognitive tracking
    private CognitiveProfile currentProfile;
    private List<ProblemSolvingEvent> problemSolvingHistory;
    private List<MemoryEvent> memoryEvents;
    private AttentionProfile attentionProfile;
    private ThinkingStyleProfile thinkingStyle;
    
    // Real-time metrics
    private float currentCognitiveLoad;
    private float currentAttentionLevel;
    private float currentFrustrationLevel;
    private float currentFlowState;
    
    // Pattern analysis
    private Dictionary<string, ProblemSolvingPattern> solvingPatterns;
    private Dictionary<string, float> cognitiveStrengths;
    private List<LearningBreakthrough> breakthroughs;
    
    [System.Serializable]
    public class CognitiveProfile
    {
        public float processingSpeed;
        public float workingMemoryEfficiency;
        public float cognitiveFlexibility;
        public float abstractThinking;
        public float patternRecognition;
        public float logicalReasoning;
        public float spatialReasoning;
        public float verbalReasoning;
        public float executiveFunction;
        public float metacognition;
        
        public CognitiveProfile()
        {
            // Initialize with baseline values
            processingSpeed = 0.5f;
            workingMemoryEfficiency = 0.5f;
            cognitiveFlexibility = 0.5f;
            abstractThinking = 0.3f;
            patternRecognition = 0.5f;
            logicalReasoning = 0.5f;
            spatialReasoning = 0.5f;
            verbalReasoning = 0.5f;
            executiveFunction = 0.5f;
            metacognition = 0.3f;
        }
    }
    
    [System.Serializable]
    public class ProblemSolvingEvent
    {
        public string problemId;
        public string problemType;
        public float startTime;
        public float solutionTime;
        public int attempts;
        public bool successful;
        public string approach;
        public List<string> strategiesUsed;
        public float cognitiveLoadDuring;
        public Dictionary<string, float> cognitiveMetrics;
        
        public ProblemSolvingEvent()
        {
            strategiesUsed = new List<string>();
            cognitiveMetrics = new Dictionary<string, float>();
        }
    }
    
    [System.Serializable]
    public class MemoryEvent
    {
        public string itemId;
        public string memoryType; // working, short-term, long-term
        public float encodingTime;
        public float retrievalTime;
        public bool successfulRecall;
        public int interferenceCount;
        public float retentionDuration;
    }
    
    [System.Serializable]
    public class AttentionProfile
    {
        public float sustainedAttention;
        public float selectiveAttention;
        public float dividedAttention;
        public float attentionSwitching;
        public int distractionCount;
        public float averageFocusDuration;
        public List<float> attentionSpans;
        
        public AttentionProfile()
        {
            attentionSpans = new List<float>();
        }
    }
    
    [System.Serializable]
    public class ThinkingStyleProfile
    {
        public float convergentThinking; // Single correct answer
        public float divergentThinking; // Multiple creative solutions
        public float analyticalThinking;
        public float holisticThinking;
        public float concreteThinking;
        public float abstractThinking;
        public float sequentialProcessing;
        public float simultaneousProcessing;
        public string dominantStyle;
    }
    
    [System.Serializable]
    public class ProblemSolvingPattern
    {
        public string patternName;
        public int occurrences;
        public float successRate;
        public float averageTime;
        public List<string> commonStrategies;
        
        public ProblemSolvingPattern()
        {
            commonStrategies = new List<string>();
        }
    }
    
    [System.Serializable]
    public class LearningBreakthrough
    {
        public float timestamp;
        public string breakthroughType;
        public string triggerEvent;
        public float cognitiveLeap;
        public Dictionary<string, float> beforeMetrics;
        public Dictionary<string, float> afterMetrics;
        
        public LearningBreakthrough()
        {
            beforeMetrics = new Dictionary<string, float>();
            afterMetrics = new Dictionary<string, float>();
        }
    }
    
    void Start()
    {
        InitializeCognitiveAnalyzer();
    }
    
    void InitializeCognitiveAnalyzer()
    {
        learningTracker = FindFirstObjectByType<LearningStyleTracker>();
        puzzleManager = FindFirstObjectByType<PuzzleManager>();
        
        currentProfile = new CognitiveProfile();
        problemSolvingHistory = new List<ProblemSolvingEvent>();
        memoryEvents = new List<MemoryEvent>();
        attentionProfile = new AttentionProfile();
        thinkingStyle = new ThinkingStyleProfile();
        solvingPatterns = new Dictionary<string, ProblemSolvingPattern>();
        cognitiveStrengths = new Dictionary<string, float>();
        breakthroughs = new List<LearningBreakthrough>();
        
        InvokeRepeating(nameof(UpdateCognitiveMetrics), cognitiveLoadUpdateInterval, cognitiveLoadUpdateInterval);
    }
    
    public void StartProblemSolving(string problemId, string problemType)
    {
        var solvingEvent = new ProblemSolvingEvent
        {
            problemId = problemId,
            problemType = problemType,
            startTime = Time.time,
            cognitiveLoadDuring = currentCognitiveLoad
        };
        
        // Store as current problem
        problemSolvingHistory.Add(solvingEvent);
        
        // Increase cognitive load
        currentCognitiveLoad = Mathf.Min(1f, currentCognitiveLoad + 0.2f);
    }
    
    public void RecordProblemSolvingAttempt(string problemId, string strategy)
    {
        var currentProblem = problemSolvingHistory.LastOrDefault(p => p.problemId == problemId);
        if (currentProblem != null)
        {
            currentProblem.attempts++;
            if (!currentProblem.strategiesUsed.Contains(strategy))
            {
                currentProblem.strategiesUsed.Add(strategy);
            }
            
            // Analyze strategy type
            AnalyzeStrategyChoice(strategy);
        }
    }
    
    public void CompleteProblemSolving(string problemId, bool successful)
    {
        var problem = problemSolvingHistory.LastOrDefault(p => p.problemId == problemId);
        if (problem == null) return;
        
        problem.successful = successful;
        problem.solutionTime = Time.time - problem.startTime;
        problem.approach = DetermineApproach(problem);
        
        // Record cognitive metrics during solving
        problem.cognitiveMetrics["processing_speed"] = CalculateProcessingSpeed(problem);
        problem.cognitiveMetrics["efficiency"] = CalculateEfficiency(problem);
        problem.cognitiveMetrics["flexibility"] = MeasureFlexibility(problem);
        problem.cognitiveMetrics["persistence"] = problem.attempts / problem.solutionTime;
        
        // Update cognitive profile
        UpdateCognitiveProfile(problem);
        
        // Check for patterns
        IdentifyProblemSolvingPattern(problem);
        
        // Check for breakthrough
        CheckForBreakthrough(problem);
        
        // Log insights
        LogCognitiveInsights(problem);
        
        // Adjust cognitive load
        currentCognitiveLoad = Mathf.Max(0f, currentCognitiveLoad - 0.1f);
    }
    
    string DetermineApproach(ProblemSolvingEvent problem)
    {
        if (problem.attempts == 1 && problem.successful)
            return "insight_based";
        if (problem.strategiesUsed.Count > 3)
            return "exploratory";
        if (problem.attempts > 5)
            return "trial_and_error";
        if (problem.strategiesUsed.Any(s => s.Contains("systematic")))
            return "systematic";
        if (problem.solutionTime < 10f)
            return "intuitive";
        
        return "mixed_approach";
    }
    
    float CalculateProcessingSpeed(ProblemSolvingEvent problem)
    {
        // Normalize based on problem complexity and time
        float expectedTime = GetExpectedSolutionTime(problem.problemType);
        return Mathf.Clamp01(expectedTime / problem.solutionTime);
    }
    
    float GetExpectedSolutionTime(string problemType)
    {
        // Define expected times for different problem types
        switch (problemType.ToLower())
        {
            case "chronocircuits": return 120f;
            case "scrollofsecrets": return 90f;
            case "pyramidrebuilder": return 150f;
            default: return 100f;
        }
    }
    
    float CalculateEfficiency(ProblemSolvingEvent problem)
    {
        if (!problem.successful) return 0f;
        
        float timeEfficiency = 1f / (1f + problem.solutionTime / 60f);
        float attemptEfficiency = 1f / (1f + problem.attempts - 1);
        float strategyEfficiency = 1f / (1f + problem.strategiesUsed.Count - 1);
        
        return (timeEfficiency + attemptEfficiency + strategyEfficiency) / 3f;
    }
    
    float MeasureFlexibility(ProblemSolvingEvent problem)
    {
        return problem.strategiesUsed.Count / (float)Mathf.Max(1, problem.attempts);
    }
    
    void AnalyzeStrategyChoice(string strategy)
    {
        // Update thinking style based on strategy
        if (strategy.Contains("logical") || strategy.Contains("sequential"))
        {
            thinkingStyle.analyticalThinking += 0.05f;
            thinkingStyle.sequentialProcessing += 0.05f;
        }
        else if (strategy.Contains("visual") || strategy.Contains("spatial"))
        {
            thinkingStyle.holisticThinking += 0.05f;
            thinkingStyle.simultaneousProcessing += 0.05f;
        }
        else if (strategy.Contains("creative") || strategy.Contains("alternative"))
        {
            thinkingStyle.divergentThinking += 0.05f;
            thinkingStyle.abstractThinking += 0.05f;
        }
        
        // Normalize values
        NormalizeThinkingStyle();
    }
    
    void NormalizeThinkingStyle()
    {
        thinkingStyle.convergentThinking = Mathf.Clamp01(thinkingStyle.convergentThinking);
        thinkingStyle.divergentThinking = Mathf.Clamp01(thinkingStyle.divergentThinking);
        thinkingStyle.analyticalThinking = Mathf.Clamp01(thinkingStyle.analyticalThinking);
        thinkingStyle.holisticThinking = Mathf.Clamp01(thinkingStyle.holisticThinking);
        thinkingStyle.concreteThinking = Mathf.Clamp01(thinkingStyle.concreteThinking);
        thinkingStyle.abstractThinking = Mathf.Clamp01(thinkingStyle.abstractThinking);
        thinkingStyle.sequentialProcessing = Mathf.Clamp01(thinkingStyle.sequentialProcessing);
        thinkingStyle.simultaneousProcessing = Mathf.Clamp01(thinkingStyle.simultaneousProcessing);
    }
    
    void UpdateCognitiveProfile(ProblemSolvingEvent problem)
    {
        // Update processing speed
        float speedDelta = problem.cognitiveMetrics["processing_speed"] - currentProfile.processingSpeed;
        currentProfile.processingSpeed = Mathf.Lerp(currentProfile.processingSpeed,
            problem.cognitiveMetrics["processing_speed"], 0.1f);
        
        // Update cognitive flexibility
        currentProfile.cognitiveFlexibility = Mathf.Lerp(currentProfile.cognitiveFlexibility,
            problem.cognitiveMetrics["flexibility"], 0.1f);
        
        // Update reasoning based on problem type
        switch (problem.problemType.ToLower())
        {
            case "chronocircuits":
                currentProfile.logicalReasoning = Mathf.Lerp(currentProfile.logicalReasoning,
                    problem.successful ? 0.8f : 0.4f, 0.15f);
                break;
            case "pyramidrebuilder":
                currentProfile.spatialReasoning = Mathf.Lerp(currentProfile.spatialReasoning,
                    problem.successful ? 0.8f : 0.4f, 0.15f);
                break;
            case "scrollofsecrets":
                currentProfile.verbalReasoning = Mathf.Lerp(currentProfile.verbalReasoning,
                    problem.successful ? 0.8f : 0.4f, 0.15f);
                break;
        }
        
        // Update pattern recognition based on attempts
        if (problem.attempts == 1 && problem.successful)
        {
            currentProfile.patternRecognition = Mathf.Min(1f, currentProfile.patternRecognition + 0.1f);
        }
        
        // Update metacognition based on strategy switching
        if (problem.strategiesUsed.Count > 1)
        {
            currentProfile.metacognition = Mathf.Min(1f, currentProfile.metacognition + 0.05f);
        }
    }
    
    void IdentifyProblemSolvingPattern(ProblemSolvingEvent problem)
    {
        string patternKey = $"{problem.problemType}_{problem.approach}";
        
        if (!solvingPatterns.ContainsKey(patternKey))
        {
            solvingPatterns[patternKey] = new ProblemSolvingPattern
            {
                patternName = patternKey
            };
        }
        
        var pattern = solvingPatterns[patternKey];
        pattern.occurrences++;
        
        // Update success rate
        float totalSuccess = pattern.successRate * (pattern.occurrences - 1);
        pattern.successRate = (totalSuccess + (problem.successful ? 1f : 0f)) / pattern.occurrences;
        
        // Update average time
        float totalTime = pattern.averageTime * (pattern.occurrences - 1);
        pattern.averageTime = (totalTime + problem.solutionTime) / pattern.occurrences;
        
        // Update common strategies
        foreach (var strategy in problem.strategiesUsed)
        {
            if (!pattern.commonStrategies.Contains(strategy))
            {
                pattern.commonStrategies.Add(strategy);
            }
        }
    }
    
    void CheckForBreakthrough(ProblemSolvingEvent problem)
    {
        // Check if this represents a significant cognitive leap
        bool isBreakthrough = false;
        string breakthroughType = "";
        
        // Speed breakthrough
        if (problem.cognitiveMetrics["processing_speed"] > currentProfile.processingSpeed + 0.3f)
        {
            isBreakthrough = true;
            breakthroughType = "processing_speed_breakthrough";
        }
        
        // Insight breakthrough (solved quickly with few attempts)
        if (problem.attempts == 1 && problem.solutionTime < GetExpectedSolutionTime(problem.problemType) * 0.5f)
        {
            isBreakthrough = true;
            breakthroughType = "insight_breakthrough";
        }
        
        // Strategy breakthrough (new effective approach)
        if (problem.successful && problem.approach != GetCommonApproach(problem.problemType))
        {
            isBreakthrough = true;
            breakthroughType = "strategy_breakthrough";
        }
        
        if (isBreakthrough)
        {
            var breakthrough = new LearningBreakthrough
            {
                timestamp = Time.time,
                breakthroughType = breakthroughType,
                triggerEvent = problem.problemId,
                cognitiveLeap = CalculateCognitiveLeap(problem)
            };
            
            // Record before/after metrics
            breakthrough.beforeMetrics["processing_speed"] = currentProfile.processingSpeed;
            breakthrough.beforeMetrics["flexibility"] = currentProfile.cognitiveFlexibility;
            breakthrough.afterMetrics["processing_speed"] = problem.cognitiveMetrics["processing_speed"];
            breakthrough.afterMetrics["flexibility"] = problem.cognitiveMetrics["flexibility"];
            
            breakthroughs.Add(breakthrough);
            
            LogBreakthrough(breakthrough);
        }
    }
    
    string GetCommonApproach(string problemType)
    {
        var relevantPatterns = solvingPatterns.Where(kvp => kvp.Key.StartsWith(problemType));
        if (!relevantPatterns.Any()) return "unknown";
        
        return relevantPatterns.OrderByDescending(kvp => kvp.Value.occurrences).First().Key;
    }
    
    float CalculateCognitiveLeap(ProblemSolvingEvent problem)
    {
        float expectedPerformance = 0.5f; // Baseline
        float actualPerformance = problem.cognitiveMetrics["efficiency"];
        return actualPerformance - expectedPerformance;
    }
    
    public void RecordMemoryEvent(string itemId, string memoryType, bool encoding)
    {
        if (encoding)
        {
            var memEvent = new MemoryEvent
            {
                itemId = itemId,
                memoryType = memoryType,
                encodingTime = Time.time
            };
            memoryEvents.Add(memEvent);
        }
        else
        {
            // Retrieval attempt
            var memEvent = memoryEvents.LastOrDefault(m => m.itemId == itemId);
            if (memEvent != null)
            {
                memEvent.retrievalTime = Time.time;
                memEvent.retentionDuration = memEvent.retrievalTime - memEvent.encodingTime;
                // Success would be determined by game logic
            }
        }
    }
    
    public void UpdateAttentionMetrics(float focusDuration, bool distracted = false)
    {
        attentionProfile.attentionSpans.Add(focusDuration);
        
        if (attentionProfile.attentionSpans.Count > 20)
        {
            attentionProfile.attentionSpans.RemoveAt(0);
        }
        
        attentionProfile.averageFocusDuration = attentionProfile.attentionSpans.Average();
        
        if (distracted)
        {
            attentionProfile.distractionCount++;
        }
        
        // Update sustained attention
        attentionProfile.sustainedAttention = Mathf.Clamp01(
            attentionProfile.averageFocusDuration / attentionSpanBaseline);
        
        // Update current attention level
        currentAttentionLevel = CalculateCurrentAttention();
    }
    
    float CalculateCurrentAttention()
    {
        if (attentionProfile.attentionSpans.Count == 0) return 0.5f;
        
        // Weight recent attention spans more heavily
        float weightedSum = 0f;
        float weightTotal = 0f;
        
        for (int i = 0; i < attentionProfile.attentionSpans.Count; i++)
        {
            float weight = (i + 1) / (float)attentionProfile.attentionSpans.Count;
            weightedSum += attentionProfile.attentionSpans[i] * weight;
            weightTotal += weight;
        }
        
        return Mathf.Clamp01((weightedSum / weightTotal) / attentionSpanBaseline);
    }
    
    void UpdateCognitiveMetrics()
    {
        // Calculate current cognitive load
        UpdateCognitiveLoad();
        
        // Update flow state
        UpdateFlowState();
        
        // Detect cognitive fatigue
        DetectCognitiveFatigue();
        
        // Update thinking style dominance
        UpdateThinkingStyleDominance();
        
        // Log periodic insights
        if (Time.frameCount % 300 == 0) // Every 5 seconds
        {
            LogPeriodicCognitiveAnalysis();
        }
    }
    
    void UpdateCognitiveLoad()
    {
        // Base load from current activities
        float baseLoad = 0.3f;
        
        // Add load from active problems
        int activeProblems = problemSolvingHistory.Count(p => Time.time - p.startTime < 300f);
        baseLoad += activeProblems * 0.2f;
        
        // Add load from memory tasks
        int recentMemoryTasks = memoryEvents.Count(m => Time.time - m.encodingTime < 60f);
        baseLoad += recentMemoryTasks * 0.1f;
        
        // Adjust based on attention
        baseLoad *= (2f - currentAttentionLevel);
        
        currentCognitiveLoad = Mathf.Clamp01(baseLoad);
        
        // Update frustration based on load
        if (currentCognitiveLoad > highCognitiveLoadThreshold)
        {
            currentFrustrationLevel = Mathf.Min(1f, currentFrustrationLevel + 0.1f * Time.deltaTime);
        }
        else
        {
            currentFrustrationLevel = Mathf.Max(0f, currentFrustrationLevel - 0.1f * Time.deltaTime);
        }
    }
    
    void UpdateFlowState()
    {
        // Flow occurs when challenge matches skill level
        float challengeLevel = currentCognitiveLoad;
        float skillLevel = (currentProfile.processingSpeed + currentProfile.cognitiveFlexibility) / 2f;
        
        float skillChallengeBalance = 1f - Mathf.Abs(challengeLevel - skillLevel);
        
        // Flow also requires attention and low frustration
        currentFlowState = skillChallengeBalance * currentAttentionLevel * (1f - currentFrustrationLevel);
    }
    
    void DetectCognitiveFatigue()
    {
        if (problemSolvingHistory.Count < 5) return;
        
        // Check for declining performance
        var recentProblems = problemSolvingHistory.TakeLast(5).ToList();
        float recentSuccessRate = recentProblems.Count(p => p.successful) / 5f;
        float recentEfficiency = recentProblems.Average(p => p.cognitiveMetrics.GetValueOrDefault("efficiency", 0.5f));
        
        if (recentSuccessRate < 0.4f && recentEfficiency < 0.4f)
        {
            LogCognitiveFatigue();
        }
    }
    
    void UpdateThinkingStyleDominance()
    {
        var styles = new Dictionary<string, float>
        {
            ["analytical_sequential"] = (thinkingStyle.analyticalThinking + thinkingStyle.sequentialProcessing) / 2f,
            ["holistic_simultaneous"] = (thinkingStyle.holisticThinking + thinkingStyle.simultaneousProcessing) / 2f,
            ["convergent_concrete"] = (thinkingStyle.convergentThinking + thinkingStyle.concreteThinking) / 2f,
            ["divergent_abstract"] = (thinkingStyle.divergentThinking + thinkingStyle.abstractThinking) / 2f
        };
        
        thinkingStyle.dominantStyle = styles.OrderByDescending(kvp => kvp.Value).First().Key;
    }
    
    void LogCognitiveInsights(ProblemSolvingEvent problem)
    {
        if (learningTracker == null) return;
        
        learningTracker.LogDetailedEvent("cognitive_problem_solving",
            $"Detailed cognitive analysis of {problem.problemType} solving",
            "cognitive_processing",
            new Dictionary<string, object>
            {
                {"problem_id", problem.problemId},
                {"problem_type", problem.problemType},
                {"solution_time", problem.solutionTime},
                {"attempts", problem.attempts},
                {"successful", problem.successful},
                {"approach_type", problem.approach},
                {"strategies_used", string.Join(",", problem.strategiesUsed)},
                {"processing_speed_score", problem.cognitiveMetrics["processing_speed"]},
                {"efficiency_score", problem.cognitiveMetrics["efficiency"]},
                {"flexibility_score", problem.cognitiveMetrics["flexibility"]},
                {"cognitive_load_during", problem.cognitiveLoadDuring},
                {"shows_systematic_thinking", problem.approach == "systematic"},
                {"shows_creative_problem_solving", problem.strategiesUsed.Count > 3},
                {"shows_persistence", problem.attempts > 3 && problem.successful}
            });
    }
    
    void LogBreakthrough(LearningBreakthrough breakthrough)
    {
        if (learningTracker == null) return;
        
        learningTracker.LogDetailedEvent("cognitive_breakthrough",
            $"Significant learning breakthrough detected: {breakthrough.breakthroughType}",
            "cognitive_development",
            new Dictionary<string, object>
            {
                {"breakthrough_type", breakthrough.breakthroughType},
                {"trigger_event", breakthrough.triggerEvent},
                {"cognitive_leap_magnitude", breakthrough.cognitiveLeap},
                {"timestamp", breakthrough.timestamp},
                {"performance_increase", breakthrough.afterMetrics["processing_speed"] - breakthrough.beforeMetrics["processing_speed"]}
            });
    }
    
    void LogCognitiveFatigue()
    {
        if (learningTracker == null) return;
        
        learningTracker.LogDetailedEvent("cognitive_fatigue_detected",
            "Player showing signs of cognitive fatigue",
            "cognitive_wellbeing",
            new Dictionary<string, object>
            {
                {"cognitive_load", currentCognitiveLoad},
                {"frustration_level", currentFrustrationLevel},
                {"attention_level", currentAttentionLevel},
                {"recent_success_rate", problemSolvingHistory.TakeLast(5).Count(p => p.successful) / 5f},
                {"recommendation", "suggest_break"}
            });
    }
    
    void LogPeriodicCognitiveAnalysis()
    {
        if (learningTracker == null) return;
        
        learningTracker.LogDetailedEvent("periodic_cognitive_analysis",
            "Comprehensive cognitive state analysis",
            "cognitive_profile",
            new Dictionary<string, object>
            {
                {"processing_speed", currentProfile.processingSpeed},
                {"working_memory_efficiency", currentProfile.workingMemoryEfficiency},
                {"cognitive_flexibility", currentProfile.cognitiveFlexibility},
                {"abstract_thinking", currentProfile.abstractThinking},
                {"pattern_recognition", currentProfile.patternRecognition},
                {"logical_reasoning", currentProfile.logicalReasoning},
                {"spatial_reasoning", currentProfile.spatialReasoning},
                {"verbal_reasoning", currentProfile.verbalReasoning},
                {"executive_function", currentProfile.executiveFunction},
                {"metacognition", currentProfile.metacognition},
                {"current_cognitive_load", currentCognitiveLoad},
                {"current_flow_state", currentFlowState},
                {"dominant_thinking_style", thinkingStyle.dominantStyle},
                {"attention_stability", attentionProfile.sustainedAttention},
                {"breakthrough_count", breakthroughs.Count}
            });
    }
    
    public CognitiveProfile GetCognitiveProfile()
    {
        return currentProfile;
    }
    
    public Dictionary<string, float> GetCognitiveMetrics()
    {
        return new Dictionary<string, float>
        {
            ["cognitive_load"] = currentCognitiveLoad,
            ["attention_level"] = currentAttentionLevel,
            ["frustration_level"] = currentFrustrationLevel,
            ["flow_state"] = currentFlowState,
            ["processing_efficiency"] = CalculateOverallEfficiency(),
            ["cognitive_flexibility"] = currentProfile.cognitiveFlexibility,
            ["problem_solving_success"] = CalculateProblemSolvingSuccess()
        };
    }
    
    float CalculateOverallEfficiency()
    {
        if (problemSolvingHistory.Count == 0) return 0.5f;
        
        var recentProblems = problemSolvingHistory.TakeLast(10).ToList();
        return recentProblems.Average(p => p.cognitiveMetrics.GetValueOrDefault("efficiency", 0.5f));
    }
    
    float CalculateProblemSolvingSuccess()
    {
        if (problemSolvingHistory.Count == 0) return 0f;
        
        var recentProblems = problemSolvingHistory.TakeLast(10).ToList();
        return recentProblems.Count(p => p.successful) / (float)recentProblems.Count;
    }
    
    public string GetDominantCognitiveStyle()
    {
        // Analyze cognitive strengths
        var strengths = new Dictionary<string, float>
        {
            ["logical_analytical"] = currentProfile.logicalReasoning,
            ["visual_spatial"] = currentProfile.spatialReasoning,
            ["verbal_linguistic"] = currentProfile.verbalReasoning,
            ["pattern_recognition"] = currentProfile.patternRecognition,
            ["abstract_thinking"] = currentProfile.abstractThinking
        };
        
        return strengths.OrderByDescending(kvp => kvp.Value).First().Key;
    }
    
    public bool IsInFlowState()
    {
        return currentFlowState > 0.7f;
    }
    
    public bool NeedsBreak()
    {
        return currentCognitiveLoad > highCognitiveLoadThreshold || 
               currentFrustrationLevel > 0.7f ||
               currentAttentionLevel < 0.3f;
    }
}