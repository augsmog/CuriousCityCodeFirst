using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using System.IO;
using CuriousCityAutomated.Analytics;

/// <summary>
/// Generates comprehensive learning analytics reports for parents based on collected behavioral data.
/// Provides actionable insights and recommendations for supporting child's learning at home.
/// </summary>
public class ParentAnalyticsReporter : MonoBehaviour
{
    [Header("Report Configuration")]
    public bool generateReportsAutomatically = true;
    public float reportGenerationInterval = 300f; // 5 minutes
    public int maxSessionsInReport = 10;
    public bool includeVisualCharts = true;
    
    [Header("Report Export Settings")]
    public string exportPath = "";
    public bool emailReports = false;
    public string parentEmail = "";
    
    // Dependencies
    private LearningStyleTracker learningTracker;
    private GameDataManager dataManager;
    
    // Report Generation
    private List<DetailedLearningReport> sessionReports;
    private ParentDashboardData dashboardData;
    
    // Analytical Models
    private LearningStyleClassifier styleClassifier;
    private BehaviorAnalyzer behaviorAnalyzer;
    private RecommendationEngine recommendationEngine;
    
    private void Start()
    {
        InitializeAnalyticsReporter();
        
        if (generateReportsAutomatically)
        {
            StartCoroutine(AutoGenerateReports());
        }
    }
    
    private void InitializeAnalyticsReporter()
    {
        learningTracker = FindFirstObjectByType<LearningStyleTracker>();
        dataManager = GameDataManager.Instance;
        
        sessionReports = new List<DetailedLearningReport>();
        dashboardData = new ParentDashboardData();
        
        // Initialize analytical components
        styleClassifier = new LearningStyleClassifier();
        behaviorAnalyzer = new BehaviorAnalyzer();
        recommendationEngine = new RecommendationEngine();
        
        // Set default export path
        if (string.IsNullOrEmpty(exportPath))
        {
            exportPath = Path.Combine(Application.persistentDataPath, "ParentReports");
        }
        
        if (!Directory.Exists(exportPath))
        {
            Directory.CreateDirectory(exportPath);
        }
        
        // Subscribe to learning events
        LearningStyleTracker.OnInsightGenerated += OnNewInsightGenerated;
        LearningStyleTracker.OnProfileUpdated += OnLearningProfileUpdated;
    }
    
    private IEnumerator AutoGenerateReports()
    {
        while (true)
        {
            yield return new WaitForSeconds(reportGenerationInterval);
            
            if (learningTracker != null)
            {
                GenerateComprehensiveReport();
            }
        }
    }
    
    public void GenerateComprehensiveReport()
    {
        var report = CollectAllLearningData();
        var analysis = AnalyzeLearningProgress(report);
        var recommendations = GenerateRecommendations(analysis);
        
        var parentReport = new ParentLearningReport
        {
            reportId = Guid.NewGuid().ToString(),
            generatedDate = DateTime.Now,
            reportingPeriod = TimeSpan.FromSeconds(reportGenerationInterval),
            childName = GetChildName(),
            
            // Core Analytics
            learningStyleAnalysis = analysis.learningStyleBreakdown,
            behavioralInsights = analysis.behavioralPatterns,
            cognitiveMetrics = analysis.cognitiveMetrics,
            socialLearningPatterns = analysis.socialPatterns,
            
            // Progress Tracking
            skillDevelopment = analysis.skillProgress,
            engagementMetrics = ConvertToFloatDictionary(analysis.engagementAnalysis),
            challengeAreas = analysis.strugglingAreas,
            strengthAreas = analysis.strengthAreas,
            
            // Recommendations
            homeActivities = recommendations.homeActivities,
            learningResourceSuggestions = recommendations.resources,
            parentingTips = recommendations.parentingStrategies,
            
            // Detailed Data
            sessionSummaries = GenerateSessionSummaries(),
            timeSeriesData = GetTimeSeriesAnalysis(),
            comparativeAnalysis = GenerateComparativeAnalysis()
        };
        
        // Generate visual charts if enabled
        if (includeVisualCharts)
        {
            parentReport.visualCharts = GenerateVisualizationData(parentReport);
        }
        
        // Export report
        ExportReport(parentReport);
        
        // Update dashboard
        UpdateParentDashboard(parentReport);
        
        Debug.Log($"Comprehensive parent report generated: {parentReport.reportId}");
    }
    private Dictionary<string, float> ConvertToFloatDictionary(Dictionary<string, object> source)
{
    var result = new Dictionary<string, float>();
    foreach (var kvp in source)
    {
        if (kvp.Value is float floatValue)
        {
            result[kvp.Key] = floatValue;
        }
        else if (kvp.Value is int intValue)
        {
            result[kvp.Key] = (float)intValue;
        }
        else if (kvp.Value is double doubleValue)
        {
            result[kvp.Key] = (float)doubleValue;
        }
        else
        {
            result[kvp.Key] = 0f; // Default value
        }
    }
    return result;
}
    private ComprehensiveLearningData CollectAllLearningData()
    {
        var data = new ComprehensiveLearningData();
        
        if (learningTracker)
        {
            // Get current session data
            var currentReport = learningTracker.GenerateDetailedReport();
            data.currentSessionData = currentReport;
            
            // Collect time series data
            data.engagementTimeSeries = GetEngagementTimeSeries();
            data.difficultyProgressionData = GetDifficultyProgression();
            data.learningStyleEvolution = GetLearningStyleEvolution();
            data.socialInteractionData = GetSocialInteractionAnalysis();
        }
        
        if (dataManager)
        {
            // Get historical data
            var saveData = dataManager.GetCurrentSaveData();
            data.historicalProgress = ExtractHistoricalLearningData(saveData);
        }
        
        return data;
    }
    
    private LearningAnalysisResults AnalyzeLearningProgress(ComprehensiveLearningData data)
{
    var results = new LearningAnalysisResults();
    
    // Learning Style Classification
    results.learningStyleBreakdown = styleClassifier.ClassifyLearningStyles(data);
    results.dominantLearningStyle = styleClassifier.GetDominantStyle(data);
    results.learningStyleConfidence = styleClassifier.GetConfidenceLevel(data);
    
    // Behavioral Pattern Analysis
    results.behavioralPatterns = behaviorAnalyzer.AnalyzeBehaviorPatterns(data);
    results.cognitiveMetrics = behaviorAnalyzer.CalculateCognitiveMetrics(data);
    results.socialPatterns = behaviorAnalyzer.AnalyzeSocialPatterns(data);
    
    // Progress and Performance Analysis
    results.skillProgress = AnalyzeSkillDevelopment(data);
    results.engagementAnalysis = AnalyzeEngagementPatterns(data);
    
    // Convert List<string> to List<string> for struggling and strength areas
    results.strugglingAreas = IdentifyChallengingAreas(data);
    results.strengthAreas = IdentifyStrengthAreas(data);
    
    return results;
}
    
    private RecommendationSet GenerateRecommendations(LearningAnalysisResults analysis)
    {
        return recommendationEngine.GeneratePersonalizedRecommendations(analysis);
    }
    
    private Dictionary<string, object> AnalyzeSkillDevelopment(ComprehensiveLearningData data)
    {
        var skillAnalysis = new Dictionary<string, object>();
        
        // Analyze problem-solving skills
        var problemSolvingMetrics = CalculateProblemSolvingMetrics(data);
        skillAnalysis["problem_solving"] = problemSolvingMetrics;
        
        // Analyze critical thinking development
        var criticalThinkingMetrics = CalculateCriticalThinkingMetrics(data);
        skillAnalysis["critical_thinking"] = criticalThinkingMetrics;
        
        // Analyze creativity and exploration
        var creativityMetrics = CalculateCreativityMetrics(data);
        skillAnalysis["creativity"] = creativityMetrics;
        
        // Analyze persistence and resilience
        var persistenceMetrics = CalculatePersistenceMetrics(data);
        skillAnalysis["persistence"] = persistenceMetrics;
        
        return skillAnalysis;
    }
    
    private Dictionary<string, float> CalculateProblemSolvingMetrics(ComprehensiveLearningData data)
    {
        var metrics = new Dictionary<string, float>();
        
        if (data.currentSessionData?.decisionMakingAnalysis != null && 
    data.currentSessionData.decisionMakingAnalysis.ContainsKey("decisions"))
{
    var decisionsData = data.currentSessionData.decisionMakingAnalysis["decisions"];
    List<DecisionEvent> decisions = new List<DecisionEvent>();
            
            // Calculate decision-making speed
            metrics["decision_speed"] = decisions.Any() ? 
                decisions.Average(d => d.decisionTime) : 0f;
            
            // Calculate problem-solving approach consistency
            metrics["approach_consistency"] = CalculateApproachConsistency(decisions);
            
            // Calculate solution effectiveness
            metrics["solution_effectiveness"] = CalculateSolutionEffectiveness(data);
        }
        
        return metrics;
    }
    
    private Dictionary<string, float> CalculateCriticalThinkingMetrics(ComprehensiveLearningData data)
    {
        var metrics = new Dictionary<string, float>();
        
        // Analyze hint usage patterns (critical thinking vs immediate help-seeking)
        var hintUsagePattern = AnalyzeHintUsagePattern(data);
        metrics["independent_thinking"] = hintUsagePattern;
        
        // Analyze error recovery patterns
        var errorRecovery = AnalyzeErrorRecoveryPattern(data);
        metrics["error_learning"] = errorRecovery;
        
        // Analyze hypothesis testing behavior
        var hypothesisTesting = AnalyzeHypothesisTestingBehavior(data);
        metrics["hypothesis_testing"] = hypothesisTesting;
        
        return metrics;
    }
    
    private Dictionary<string, float> CalculateCreativityMetrics(ComprehensiveLearningData data)
    {
        var metrics = new Dictionary<string, float>();
        
        // Analyze exploration patterns
        if (data.currentSessionData?.movementAnalysis != null)
    {
    metrics["exploration_creativity"] = 0.5f; // Default value or calculate differently
    }

        
        // Analyze problem-solving approach variety
        metrics["approach_variety"] = CalculateApproachVariety(data);
        
        // Analyze unconventional solution attempts
        metrics["unconventional_thinking"] = CalculateUnconventionalThinking(data);
        
        return metrics;
    }
    
    private Dictionary<string, float> CalculatePersistenceMetrics(ComprehensiveLearningData data)
    {
        var metrics = new Dictionary<string, float>();
        
        // Analyze persistence through difficult tasks
        if (data.currentSessionData?.cognitiveAnalysis != null)
        {
            metrics["retry_persistence"] = 0.5f; // Default value
        }
        
        // Calculate frustration tolerance
        metrics["frustration_tolerance"] = CalculateFrustrationTolerance(data);
        
        // Calculate goal persistence
        metrics["goal_persistence"] = CalculateGoalPersistence(data);
        
        return metrics;
    }
    
    private List<SessionSummary> GenerateSessionSummaries()
    {
        var summaries = new List<SessionSummary>();
        
        foreach (var report in sessionReports.TakeLast(maxSessionsInReport))
        {
            var summary = new SessionSummary
            {
                sessionDate = report.generatedDate,
                duration = TimeSpan.FromSeconds(report.sessionDuration),
                primaryLearningStyle = GetPrimaryLearningStyleFromReport(report),
                engagementLevel = GetAverageEngagementFromReport(report),
                achievementsUnlocked = GetAchievementsFromReport(report),
                challengesEncountered = GetChallengesFromReport(report),
                keyInsights = GetKeyInsightsFromReport(report)
            };
            
            summaries.Add(summary);
        }
        
        return summaries;
    }
    
    private Dictionary<string, List<float>> GetTimeSeriesAnalysis()
    {
        var timeSeriesData = new Dictionary<string, List<float>>();
        
        if (learningTracker)
        {
            var sessionMetrics = learningTracker.GetSessionMetrics();
            
            // Extract time series for key metrics
            var metricsToTrack = new string[]
            {
                "engagement_level", "confidence_level", "curiosity_level",
                "problem_solving_efficiency", "learning_rate", "focus_duration"
            };
            
            foreach (var metric in metricsToTrack)
            {
                if (sessionMetrics.ContainsKey($"{metric}_history"))
                {
                    // This would be implemented to extract historical time series data
                    timeSeriesData[metric] = ExtractTimeSeriesData(metric);
                }
            }
        }
        
        return timeSeriesData;
    }
    
    private ComparativeAnalysis GenerateComparativeAnalysis()
    {
        var analysis = new ComparativeAnalysis();
        
        // Compare against age group norms (this would require normative data)
        analysis.ageGroupComparison = GenerateAgeGroupComparison();
        
        // Compare progress over time
        analysis.progressComparison = GenerateProgressComparison();
        
        // Compare across different learning contexts
        analysis.contextComparison = GenerateContextComparison();
        
        return analysis;
    }
    
    private void ExportReport(ParentLearningReport report)
    {
        try
        {
            // Generate filename
            string fileName = $"LearningReport_{report.childName}_{DateTime.Now:yyyyMMdd_HHmmss}.json";
            string filePath = Path.Combine(exportPath, fileName);
            
            // Serialize to JSON
            string jsonReport = JsonUtility.ToJson(report, true);
            
            // Write to file
            File.WriteAllText(filePath, jsonReport);
            
            // Generate human-readable summary
            GenerateHumanReadableReport(report);
            
            Debug.Log($"Parent report exported to: {filePath}");
            
            // Email if configured
            if (emailReports && !string.IsNullOrEmpty(parentEmail))
            {
                SendReportByEmail(report, filePath);
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to export parent report: {e.Message}");
        }
    }
    
    private void GenerateHumanReadableReport(ParentLearningReport report)
    {
        var readableReport = new System.Text.StringBuilder();
        
        readableReport.AppendLine($"Learning Progress Report for {report.childName}");
        readableReport.AppendLine($"Generated: {report.generatedDate:F}");
        readableReport.AppendLine();
        
        // Executive Summary
        readableReport.AppendLine("EXECUTIVE SUMMARY");
        readableReport.AppendLine("================");
        readableReport.AppendLine(GenerateExecutiveSummary(report));
        readableReport.AppendLine();
        
        // Learning Style Analysis
        readableReport.AppendLine("LEARNING STYLE ANALYSIS");
        readableReport.AppendLine("======================");
        readableReport.AppendLine(FormatLearningStyleAnalysis(report.learningStyleAnalysis));
        readableReport.AppendLine();
        
        // Key Strengths
        readableReport.AppendLine("KEY STRENGTHS");
        readableReport.AppendLine("=============");
        foreach (var strength in report.strengthAreas)
        {
            readableReport.AppendLine($"• {FormatStrengthDescription(strength)}");
        }
        readableReport.AppendLine();
        
        // Areas for Growth
        readableReport.AppendLine("AREAS FOR GROWTH");
        readableReport.AppendLine("================");
        foreach (var challenge in report.challengeAreas)
        {
            readableReport.AppendLine($"• {FormatChallengeDescription(challenge)}");
        }
        readableReport.AppendLine();
        
        // Recommendations
        readableReport.AppendLine("RECOMMENDATIONS FOR HOME");
        readableReport.AppendLine("=======================");
        foreach (var activity in report.homeActivities)
        {
            readableReport.AppendLine($"• {activity}");
        }
        readableReport.AppendLine();
        
        // Save readable report
        string readableFileName = $"LearningReport_{report.childName}_{DateTime.Now:yyyyMMdd_HHmmss}_Summary.txt";
        string readableFilePath = Path.Combine(exportPath, readableFileName);
        File.WriteAllText(readableFilePath, readableReport.ToString());
    }
    
    private string GenerateExecutiveSummary(ParentLearningReport report)
    {
        var summary = new System.Text.StringBuilder();
        
        // Overall engagement
        var avgEngagement = report.engagementMetrics.GetValueOrDefault("average_engagement", 0.5f);
        string engagementDescription = avgEngagement > 0.7f ? "highly engaged" : 
                                     avgEngagement > 0.4f ? "moderately engaged" : "needs engagement support";
        
        summary.AppendLine($"Your child shows {engagementDescription} learning behavior during play sessions.");
        
        // Dominant learning style
        if (report.learningStyleAnalysis.ContainsKey("dominant_style"))
        {
            string dominantStyle = report.learningStyleAnalysis["dominant_style"].ToString();
            summary.AppendLine($"Their primary learning style appears to be {FormatLearningStyleName(dominantStyle)}.");
        }
        
        // Key insight
        if (report.strengthAreas.Any())
        {
            summary.AppendLine($"Notable strength: {FormatStrengthDescription(report.strengthAreas.First())}");
        }
        
        return summary.ToString();
    }
    
    // Event handlers
    private void OnNewInsightGenerated(LearningInsight insight)
    {
        // Process new insights as they're generated
        UpdateRealTimeInsights(insight);
    }
    
    private void OnLearningProfileUpdated(LearningStyleProfile profile)
    {
        // Update dashboard when profile changes
        UpdateLearningStyleTracking(profile);
    }
    
    // Helper methods for data processing
    private string GetChildName()
    {
        return PlayerPrefs.GetString("child_name", "Child");
    }
    
    private void UpdateParentDashboard(ParentLearningReport report)
    {
        dashboardData.lastUpdated = DateTime.Now;
        dashboardData.currentEngagementLevel = report.engagementMetrics.GetValueOrDefault("current_engagement", 0.5f);
        dashboardData.weeklyProgress = CalculateWeeklyProgress(report);
        dashboardData.recentAchievements = ExtractRecentAchievements(report);
    }
    
    // Placeholder implementations for complex analytical methods
    private List<float> ExtractTimeSeriesData(string metric) => new List<float>();
    private Dictionary<string, object> ExtractHistoricalLearningData(GameSaveData saveData) => new Dictionary<string, object>();
    private List<float> GetEngagementTimeSeries() => new List<float>();
    private Dictionary<string, float> GetDifficultyProgression() => new Dictionary<string, float>();
    private Dictionary<string, float> GetLearningStyleEvolution() => new Dictionary<string, float>();
    private Dictionary<string, object> GetSocialInteractionAnalysis() => new Dictionary<string, object>();
    private Dictionary<string, object> AnalyzeEngagementPatterns(ComprehensiveLearningData data) => new Dictionary<string, object>();
    private List<string> IdentifyChallengingAreas(ComprehensiveLearningData data) => new List<string>();
    private List<string> IdentifyStrengthAreas(ComprehensiveLearningData data) => new List<string>();
    private float CalculateApproachConsistency(List<DecisionEvent> decisions) => 0.5f;
    private float CalculateSolutionEffectiveness(ComprehensiveLearningData data) => 0.5f;
    private float AnalyzeHintUsagePattern(ComprehensiveLearningData data) => 0.5f;
    private float AnalyzeErrorRecoveryPattern(ComprehensiveLearningData data) => 0.5f;
    private float AnalyzeHypothesisTestingBehavior(ComprehensiveLearningData data) => 0.5f;
    private float CalculateExplorationCreativity(MovementAnalytics movement) => 0.5f;
    private float CalculateApproachVariety(ComprehensiveLearningData data) => 0.5f;
    private float CalculateUnconventionalThinking(ComprehensiveLearningData data) => 0.5f;
    private float CalculateRetryPersistence(CognitiveLoadAnalytics cognitive) => 0.5f;
    private float CalculateFrustrationTolerance(ComprehensiveLearningData data) => 0.5f;
    private float CalculateGoalPersistence(ComprehensiveLearningData data) => 0.5f;
    private string GetPrimaryLearningStyleFromReport(DetailedLearningReport report) => "Visual";
    private float GetAverageEngagementFromReport(DetailedLearningReport report) => 0.7f;
    private List<string> GetAchievementsFromReport(DetailedLearningReport report) => new List<string>();
    private List<string> GetChallengesFromReport(DetailedLearningReport report) => new List<string>();
    private List<string> GetKeyInsightsFromReport(DetailedLearningReport report) => new List<string>();
    private Dictionary<string, float> GenerateAgeGroupComparison() => new Dictionary<string, float>();
    private Dictionary<string, float> GenerateProgressComparison() => new Dictionary<string, float>();
    private Dictionary<string, float> GenerateContextComparison() => new Dictionary<string, float>();
    private string FormatLearningStyleAnalysis(Dictionary<string, object> analysis) => "Analysis formatted here";
    private string FormatStrengthDescription(string strength) => strength;
    private string FormatChallengeDescription(string challenge) => challenge;
    private string FormatLearningStyleName(string style) => style.Replace("_", " ");
    private void SendReportByEmail(ParentLearningReport report, string filePath) { }
    private void UpdateRealTimeInsights(LearningInsight insight) { }
    private void UpdateLearningStyleTracking(LearningStyleProfile profile) { }
    private float CalculateWeeklyProgress(ParentLearningReport report) => 0.8f;
    private List<string> ExtractRecentAchievements(ParentLearningReport report) => new List<string>();
    private Dictionary<string, object> GenerateVisualizationData(ParentLearningReport report) => new Dictionary<string, object>();
}

// Comprehensive data structures for parent reporting
[System.Serializable]
public class ParentLearningReport
{
    public string reportId;
    public DateTime generatedDate;
    public TimeSpan reportingPeriod;
    public string childName;
    
    public Dictionary<string, object> learningStyleAnalysis = new Dictionary<string, object>();
    public Dictionary<string, float> behavioralInsights;
    public Dictionary<string, float> cognitiveMetrics;
    public Dictionary<string, object> socialLearningPatterns;
    
    public Dictionary<string, object> skillDevelopment;
    public Dictionary<string, float> engagementMetrics;
    public List<string> challengeAreas;
    public List<string> strengthAreas;
    
    public List<string> homeActivities;
    public List<string> learningResourceSuggestions;
    public List<string> parentingTips;
    
    public List<SessionSummary> sessionSummaries;
    public Dictionary<string, List<float>> timeSeriesData;
    public ComparativeAnalysis comparativeAnalysis;
    public Dictionary<string, object> visualCharts;
}

[System.Serializable]
public class ComprehensiveLearningData
{
    public DetailedLearningReport currentSessionData;
    public Dictionary<string, object> historicalProgress;
    public List<float> engagementTimeSeries;
    public Dictionary<string, float> difficultyProgressionData;
    public Dictionary<string, float> learningStyleEvolution;
    public Dictionary<string, object> socialInteractionData;
}

[System.Serializable]
public class LearningAnalysisResults
{
    public Dictionary<string, object> learningStyleBreakdown;
    public string dominantLearningStyle;
    public float learningStyleConfidence;
    public Dictionary<string, float> behavioralPatterns;
    public Dictionary<string, float> cognitiveMetrics;
    public Dictionary<string, object> socialPatterns;
    public Dictionary<string, object> skillProgress;
    public Dictionary<string, object> engagementAnalysis;
    public List<string> strugglingAreas;
    public List<string> strengthAreas;
}

[System.Serializable]
public class RecommendationSet
{
    public List<string> homeActivities;
    public List<string> resources;
    public List<string> parentingStrategies;
}

[System.Serializable]
public class SessionSummary
{
    public DateTime sessionDate;
    public TimeSpan duration;
    public string primaryLearningStyle;
    public float engagementLevel;
    public List<string> achievementsUnlocked;
    public List<string> challengesEncountered;
    public List<string> keyInsights;
}

[System.Serializable]
public class ComparativeAnalysis
{
    public Dictionary<string, float> ageGroupComparison;
    public Dictionary<string, float> progressComparison;
    public Dictionary<string, float> contextComparison;
}

[System.Serializable]
public class ParentDashboardData
{
    public DateTime lastUpdated;
    public float currentEngagementLevel;
    public float weeklyProgress;
    public List<string> recentAchievements;
}

// Analytical Engine Classes
public class LearningStyleClassifier
{
    public Dictionary<string, object> ClassifyLearningStyles(ComprehensiveLearningData data)
    {
        return new Dictionary<string, object>();
    }
    
    public string GetDominantStyle(ComprehensiveLearningData data)
    {
        return "visual_spatial";
    }
    
    public float GetConfidenceLevel(ComprehensiveLearningData data)
    {
        return 0.8f;
    }
}

public class BehaviorAnalyzer
{
    public Dictionary<string, float> AnalyzeBehaviorPatterns(ComprehensiveLearningData data)
    {
        return new Dictionary<string, float>();
    }
    
    public Dictionary<string, float> CalculateCognitiveMetrics(ComprehensiveLearningData data)
    {
        return new Dictionary<string, float>();
    }
    
    public Dictionary<string, object> AnalyzeSocialPatterns(ComprehensiveLearningData data)
    {
        return new Dictionary<string, object>();
    }
}

public class RecommendationEngine
{
    public RecommendationSet GeneratePersonalizedRecommendations(LearningAnalysisResults analysis)
    {
        return new RecommendationSet
        {
            homeActivities = new List<string>
            {
                "Try building puzzles together to support visual-spatial development",
                "Read interactive stories to enhance verbal-linguistic skills",
                "Engage in hands-on science experiments for kinesthetic learning"
            },
            resources = new List<string>
            {
                "Khan Academy Kids for structured learning",
                "Local library storytelling sessions",
                "Science museum interactive exhibits"
            },
            parentingStrategies = new List<string>
            {
                "Provide multiple ways to approach problems",
                "Celebrate effort over immediate success",
                "Create quiet spaces for reflection and independent work"
            }
        };
    }
}