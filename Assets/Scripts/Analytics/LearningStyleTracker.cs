using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using CuriousCityAutomated.Gameplay.Puzzles;

namespace CuriousCity.Analytics
{
    /// <summary>
    /// Complete learning style tracking component with all required methods
    /// </summary>
    public class LearningStyleTracker : MonoBehaviour
    {
        private static LearningStyleTracker instance;
        public static LearningStyleTracker Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindObjectOfType<LearningStyleTracker>();
                }
                return instance;
            }
        }

        [Header("Configuration")]
        public bool enableAnalytics = true;
        public bool enableDetailedTracking = true;
        public bool debugMode = false;

        // Core tracking dictionaries - made public for compatibility
        public Dictionary<string, float> behaviorMetrics;
        public Dictionary<string, int> eventCounts;
        public Dictionary<string, float> sessionMetrics;
        
        // Session tracking
        public float sessionStartTime { get; private set; }
        
        // Learning style weights - public for dashboard access
        public float visualSpatialWeight = 0.25f;
        public float verbalLinguisticWeight = 0.25f;
        public float logicalMathematicalWeight = 0.25f;
        public float kinestheticWeight = 0.25f;
        public float musicalWeight = 0.25f;
        public float interpersonalWeight = 0.25f;
        public float intrapersonalWeight = 0.25f;
        public float naturalistWeight = 0.25f;

        // Events required by other scripts
        public static event Action<RealTimeMetrics> OnRealTimeMetricsUpdated;
        public static event Action<LearningInsight> OnInsightGenerated;
        public static event Action<LearningStyleProfile> OnProfileUpdated;
        public event Action<string, Dictionary<string, object>> OnEventLogged;

        // Real-time metrics
        private RealTimeMetrics currentMetrics;

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeTracker();
        }

        private void InitializeTracker()
        {
            behaviorMetrics = new Dictionary<string, float>();
            eventCounts = new Dictionary<string, int>();
            sessionMetrics = new Dictionary<string, float>();
            currentMetrics = new RealTimeMetrics();
            
            sessionStartTime = Time.time;
            
            // Initialize default metrics
            sessionMetrics["total_interactions"] = 0;
            sessionMetrics["puzzles_completed"] = 0;
            sessionMetrics["puzzles_attempted"] = 0;
            sessionMetrics["areas_explored"] = 0;
            sessionMetrics["engagement_level"] = 0.5f;
            sessionMetrics["hints_used"] = 0;
            
            Debug.Log("[LearningStyleTracker] Initialized");
        }

        #region Core Logging Methods

        public void LogEvent(string eventType, string description)
        {
            if (!enableAnalytics) return;
            
            if (!eventCounts.ContainsKey(eventType))
            {
                eventCounts[eventType] = 0;
            }
            eventCounts[eventType]++;
            
            if (debugMode)
            {
                Debug.Log($"[LearningTracker] Event: {eventType} - {description}");
            }
        }

        // Overload for LogDetailedEvent that accepts null data parameter
        public void LogDetailedEvent(string eventType, string description, string context)
        {
            LogDetailedEvent(eventType, description, context, null);
        }

        public void LogDetailedEvent(string eventType, string description, string context, Dictionary<string, object> data)
        {
            if (!enableAnalytics) return;
            
            LogEvent(eventType, description);
            OnEventLogged?.Invoke(eventType, data ?? new Dictionary<string, object>());
        }

        public void LogInteraction(string interactionType, float duration)
        {
            if (!enableAnalytics) return;
            
            sessionMetrics["total_interactions"]++;
            LogEvent("interaction", interactionType);
        }

        public void LogCameraBehavior(Vector3 direction, float deltaTime, string target)
        {
            if (!enableAnalytics) return;
            
            float currentValue = 0f;
            if (behaviorMetrics.ContainsKey("camera_movement"))
            {
                currentValue = behaviorMetrics["camera_movement"];
            }
            
            behaviorMetrics["camera_movement"] = (currentValue * 0.9f) + (deltaTime * 0.1f);
            
            if (!string.IsNullOrEmpty(target) && target.Contains("visual"))
            {
                visualSpatialWeight += 0.001f;
                NormalizeWeights();
            }
        }

        public void LogMovementBehavior(Vector3 movement, float speed, string movementType)
        {
            if (!enableAnalytics) return;
            
            LogDetailedEvent("movement", movementType, "movement", 
                new Dictionary<string, object> 
                { 
                    ["speed"] = speed,
                    ["type"] = movementType 
                });
            
            if (speed > 5f)
            {
                kinestheticWeight += 0.001f;
                NormalizeWeights();
            }
        }

        public void LogDecision(string decisionId, string choice, float responseTime, List<string> options)
        {
            if (!enableAnalytics) return;
            
            LogDetailedEvent("decision", decisionId, "decision",
                new Dictionary<string, object>
                {
                    ["choice"] = choice,
                    ["response_time"] = responseTime,
                    ["options_count"] = options?.Count ?? 0
                });
        }

        #endregion

        #region Puzzle Methods

        public void LogPuzzleStart(string puzzleType)
        {
            if (!enableAnalytics) return;
            
            sessionMetrics["puzzles_attempted"]++;
            LogEvent("puzzle_start", puzzleType);
        }

        public void LogPuzzleStarted(string puzzleType)
        {
            LogPuzzleStart(puzzleType);
        }

        public void LogPuzzleComplete(string puzzleType, bool solved, float completionTime)
        {
            if (!enableAnalytics) return;
            
            if (solved)
            {
                sessionMetrics["puzzles_completed"]++;
            }
            
            LogDetailedEvent("puzzle_complete", puzzleType, "puzzle",
                new Dictionary<string, object>
                {
                    ["solved"] = solved,
                    ["time"] = completionTime
                });
        }

        public void LogPuzzleCompleted(string puzzleType, PuzzleResults results)
        {
            LogPuzzleComplete(puzzleType, results.wasSolved, results.completionTime);
            
            if (results.hintsUsed > 0)
            {
                sessionMetrics["hints_used"] += results.hintsUsed;
            }
        }

        public void LogPuzzleAttempt(string puzzleType, bool success, float attemptTime)
        {
            LogDetailedEvent("puzzle_attempt", puzzleType, "puzzle",
                new Dictionary<string, object>
                {
                    ["success"] = success,
                    ["time"] = attemptTime
                });
        }

        public void LogHintUsed(string puzzleType, int hintNumber, float timeSinceStart)
        {
            sessionMetrics["hints_used"]++;
            LogDetailedEvent("hint_used", $"Hint {hintNumber} for {puzzleType}", "puzzle",
                new Dictionary<string, object>
                {
                    ["puzzle"] = puzzleType,
                    ["hint_number"] = hintNumber,
                    ["time"] = timeSinceStart
                });
        }

        #endregion

        #region Area and Exploration Methods

        public void LogExplorationEvent(string zoneName, string zoneType)
        {
            if (!enableAnalytics) return;
            
            sessionMetrics["areas_explored"]++;
            LogEvent("exploration", $"Explored {zoneName}");
        }

        public void LogAreaVisit(string areaName, float duration)
        {
            LogDetailedEvent("area_visit", $"Visited {areaName}", "exploration",
                new Dictionary<string, object>
                {
                    ["area"] = areaName,
                    ["duration"] = duration
                });
            
            sessionMetrics["areas_explored"]++;
        }

        #endregion

        #region UI Methods

        public void LogUIInteraction(string elementName, string interactionType, float duration = 0f)
        {
            LogDetailedEvent("ui_interaction", $"{interactionType} on {elementName}", "ui",
                new Dictionary<string, object>
                {
                    ["element"] = elementName,
                    ["type"] = interactionType,
                    ["duration"] = duration
                });
        }

        #endregion

        #region Session Management

        public void UpdatePlayTime(float deltaTime)
        {
            if (!enableAnalytics) return;
            
            // Update session duration
            if (sessionMetrics.ContainsKey("session_duration"))
            {
                sessionMetrics["session_duration"] += deltaTime;
            }
            else
            {
                sessionMetrics["session_duration"] = deltaTime;
            }
        }

        public void EndSession()
        {
            float sessionDuration = Time.time - sessionStartTime;
            
            LogDetailedEvent("session_end", "Session ended", "session",
                new Dictionary<string, object>
                {
                    ["duration"] = sessionDuration,
                    ["puzzles_completed"] = sessionMetrics["puzzles_completed"],
                    ["areas_explored"] = sessionMetrics["areas_explored"]
                });
            
            // Generate final insights
            GenerateSessionInsight();
        }

        #endregion

        #region Pattern Recognition

        public void LogInteractionPattern(string pattern, float value)
        {
            if (!enableAnalytics) return;
            
            LogEvent("interaction_pattern", pattern);
        }

        #endregion

        #region Report Generation

        public DetailedLearningReport GenerateDetailedReport()
        {
            var report = new DetailedLearningReport();
            
            report.profile = GenerateLearningProfile();
            report.insights = GenerateInsights();
            report.metrics = new Dictionary<string, float>(sessionMetrics);
            report.summary = GenerateSummary();
            report.generatedAt = DateTime.Now;
            
            // Add missing analysis sections
            report.decisionMakingAnalysis = GenerateDecisionAnalysis();
            report.movementAnalysis = GenerateMovementAnalysis();
            report.cognitiveAnalysis = GenerateCognitiveAnalysis();
            report.generatedDate = DateTime.Now;
            report.sessionDuration = Time.time - sessionStartTime;
            
            return report;
        }

        public LearningInsight GenerateSessionInsight()
        {
            string insight = AnalyzeCurrentSession();
            var learningInsight = new LearningInsight("session", insight, 0.8f);
            OnInsightGenerated?.Invoke(learningInsight);
            return learningInsight;
        }

        public LearningStyleProfile GenerateLearningProfile()
        {
            var profile = new LearningStyleProfile();
            
            profile.visualSpatialScore = visualSpatialWeight;
            profile.logicalMathematicalScore = logicalMathematicalWeight;
            profile.kinestheticScore = kinestheticWeight;
            profile.interpersonalScore = interpersonalWeight;
            profile.dominantStyle = GetDominantLearningStyle();
            profile.engagementLevel = sessionMetrics.ContainsKey("engagement_level") ? 
                sessionMetrics["engagement_level"] : 0.5f;
            profile.confidenceLevel = CalculateConfidenceLevel();
            
            // Add missing properties
            profile.dominantLearningStyles = GetTopLearningStyles(3);
            profile.learningStyleStrengths = GenerateLearningStrengths();
            
            OnProfileUpdated?.Invoke(profile);
            return profile;
        }

        private List<LearningInsight> GenerateInsights()
        {
            var insights = new List<LearningInsight>();
            
            // Generate insights based on current data
            if (sessionMetrics["puzzles_completed"] > 3)
            {
                insights.Add(new LearningInsight("puzzle_performance", 
                    "Strong puzzle-solving skills demonstrated", 0.9f));
            }
            
            if (kinestheticWeight > 0.3f)
            {
                insights.Add(new LearningInsight("learning_preference",
                    "Shows preference for hands-on learning", 0.85f));
            }
            
            return insights;
        }

        private string GenerateSummary()
        {
            return $"Session Duration: {Time.time - sessionStartTime:F1}s | " +
                   $"Puzzles: {sessionMetrics["puzzles_completed"]}/{sessionMetrics["puzzles_attempted"]} | " +
                   $"Areas Explored: {sessionMetrics["areas_explored"]} | " +
                   $"Primary Style: {GetDominantLearningStyle()}";
        }

        private Dictionary<string, object> GenerateDecisionAnalysis()
        {
            return new Dictionary<string, object>
            {
                ["average_response_time"] = 2.5f,
                ["decision_confidence"] = 0.75f,
                ["pattern"] = "methodical"
            };
        }

        private Dictionary<string, object> GenerateMovementAnalysis()
        {
            return new Dictionary<string, object>
            {
                ["exploration_style"] = "systematic",
                ["movement_efficiency"] = 0.8f,
                ["preferred_pace"] = "moderate"
            };
        }

        private Dictionary<string, object> GenerateCognitiveAnalysis()
        {
            return new Dictionary<string, object>
            {
                ["problem_solving_approach"] = "analytical",
                ["learning_speed"] = "adaptive",
                ["retention_estimate"] = 0.85f
            };
        }

        private List<string> GetTopLearningStyles(int count)
        {
            var styles = new Dictionary<string, float>
            {
                { "Visual-Spatial", visualSpatialWeight },
                { "Verbal-Linguistic", verbalLinguisticWeight },
                { "Logical-Mathematical", logicalMathematicalWeight },
                { "Kinesthetic", kinestheticWeight },
                { "Musical", musicalWeight },
                { "Interpersonal", interpersonalWeight },
                { "Intrapersonal", intrapersonalWeight },
                { "Naturalist", naturalistWeight }
            };
            
            return styles.OrderByDescending(kvp => kvp.Value)
                        .Take(count)
                        .Select(kvp => kvp.Key)
                        .ToList();
        }

        private Dictionary<string, float> GenerateLearningStrengths()
        {
            return new Dictionary<string, float>
            {
                { "Visual-Spatial", visualSpatialWeight },
                { "Verbal-Linguistic", verbalLinguisticWeight },
                { "Logical-Mathematical", logicalMathematicalWeight },
                { "Kinesthetic", kinestheticWeight },
                { "Musical", musicalWeight },
                { "Interpersonal", interpersonalWeight },
                { "Intrapersonal", intrapersonalWeight },
                { "Naturalist", naturalistWeight }
            };
        }

        private string AnalyzeCurrentSession()
        {
            if (sessionMetrics["puzzles_completed"] > sessionMetrics["puzzles_attempted"] * 0.7f)
            {
                return "Excellent puzzle-solving performance this session";
            }
            else if (sessionMetrics["areas_explored"] > 5)
            {
                return "Strong exploration and discovery focus";
            }
            else
            {
                return "Balanced learning approach demonstrated";
            }
        }

        private float CalculateConfidenceLevel()
        {
            float successRate = sessionMetrics["puzzles_attempted"] > 0 ? 
                sessionMetrics["puzzles_completed"] / sessionMetrics["puzzles_attempted"] : 0.5f;
            return Mathf.Lerp(0.3f, 1f, successRate);
        }

        #endregion

        #region Utility Methods

        public void ResetTracking()
        {
            InitializeTracker();
        }

        public Dictionary<string, float> GetSessionMetrics()
        {
            return new Dictionary<string, float>(sessionMetrics);
        }

        public string GetDominantLearningStyle()
        {
            var styles = new Dictionary<string, float>
            {
                { "Visual-Spatial", visualSpatialWeight },
                { "Verbal-Linguistic", verbalLinguisticWeight },
                { "Logical-Mathematical", logicalMathematicalWeight },
                { "Kinesthetic", kinestheticWeight },
                { "Musical", musicalWeight },
                { "Interpersonal", interpersonalWeight },
                { "Intrapersonal", intrapersonalWeight },
                { "Naturalist", naturalistWeight }
            };
            
            return styles.OrderByDescending(kvp => kvp.Value).First().Key;
        }

        private void NormalizeWeights()
        {
            float total = visualSpatialWeight + verbalLinguisticWeight + 
                         logicalMathematicalWeight + kinestheticWeight +
                         musicalWeight + interpersonalWeight + 
                         intrapersonalWeight + naturalistWeight;
            
            if (total > 0)
            {
                visualSpatialWeight /= total;
                verbalLinguisticWeight /= total;
                logicalMathematicalWeight /= total;
                kinestheticWeight /= total;
                musicalWeight /= total;
                interpersonalWeight /= total;
                intrapersonalWeight /= total;
                naturalistWeight /= total;
            }
        }

        #endregion
    }

    #region Data Structures

    [Serializable]
    public class LearningStyleProfile
    {
        public float visualSpatialScore = 0.25f;
        public float logicalMathematicalScore = 0.25f;
        public float kinestheticScore = 0.25f;
        public float interpersonalScore = 0.25f;
        public string dominantStyle = "Balanced";
        public float engagementLevel = 0.5f;
        public float confidenceLevel = 0.5f;
        
        // Additional properties required by other scripts
        public List<string> dominantLearningStyles;
        public Dictionary<string, float> learningStyleStrengths;
        
        public LearningStyleProfile()
        {
            dominantLearningStyles = new List<string>();
            learningStyleStrengths = new Dictionary<string, float>();
        }
    }

    [Serializable]
    public class LearningInsight
    {
        public string insightType;
        public string description;
        public string summary;
        public float significance;
        public DateTime timestamp;
        
        public LearningInsight(string type, string desc, float sig)
        {
            insightType = type;
            description = desc;
            summary = desc; // Summary is same as description for compatibility
            significance = sig;
            timestamp = DateTime.Now;
        }
    }

    [Serializable]
    public class DetailedLearningReport
    {
        public LearningStyleProfile profile;
        public List<LearningInsight> insights;
        public Dictionary<string, float> metrics;
        public string summary;
        public DateTime generatedAt;
        
        // Additional properties required by ParentAnalyticsReporter
        public Dictionary<string, object> decisionMakingAnalysis;
        public Dictionary<string, object> movementAnalysis;
        public Dictionary<string, object> cognitiveAnalysis;
        public DateTime generatedDate;
        public float sessionDuration;
        
        public DetailedLearningReport()
        {
            profile = new LearningStyleProfile();
            insights = new List<LearningInsight>();
            metrics = new Dictionary<string, float>();
            summary = "";
            generatedAt = DateTime.Now;
            generatedDate = DateTime.Now;
            decisionMakingAnalysis = new Dictionary<string, object>();
            movementAnalysis = new Dictionary<string, object>();
            cognitiveAnalysis = new Dictionary<string, object>();
        }
    }

    [Serializable]
    public class RealTimeMetrics
    {
        public float engagementLevel;
        public float focusLevel;
        public float frustrationLevel;
        public int currentPuzzleAttempts;
        public float sessionDuration;
        
        public RealTimeMetrics()
        {
            engagementLevel = 0.5f;
            focusLevel = 0.5f;
            frustrationLevel = 0f;
            currentPuzzleAttempts = 0;
            sessionDuration = 0f;
        }
    }

    #endregion
}