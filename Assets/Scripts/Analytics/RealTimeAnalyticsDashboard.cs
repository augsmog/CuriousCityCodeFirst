using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;
using CuriousCityAutomated.Gameplay.Puzzles;
using CuriousCityAutomated.Analytics;

/// <summary>
/// Real-time analytics dashboard that displays live learning insights, behavioral patterns,
/// and engagement metrics during gameplay. Provides immediate feedback for educational assessment.
/// </summary>
public class RealTimeAnalyticsDashboard : MonoBehaviour
{
    [Header("Dashboard UI Components")]
    public Canvas dashboardCanvas;
    public GameObject dashboardPanel;
    public Button toggleDashboardButton;
    public Button exportDataButton;
    
    [Header("Learning Style Indicators")]
    public Slider visualSpatialIndicator;
    public Slider verbalLinguisticIndicator;
    public Slider logicalMathematicalIndicator;
    public Slider kinestheticIndicator;
    public Slider musicalIndicator;
    public Slider interpersonalIndicator;
    public Slider intrapersonalIndicator;
    public Slider naturalistIndicator;
    
    [Header("Real-Time Metrics")]
    public Slider engagementMeter;
    public Slider frustrationMeter;
    public Slider confidenceMeter;
    public Slider curiosityMeter;
    public Slider focusMeter;
    public TextMeshProUGUI sessionDurationText;
    public TextMeshProUGUI totalInteractionsText;
    
    [Header("Behavioral Insights")]
    public Transform behaviorInsightsContainer;
    public GameObject insightItemPrefab;
    public TextMeshProUGUI dominantLearningStyleText;
    public TextMeshProUGUI currentActivityText;
    public TextMeshProUGUI engagementStatusText;
    
    [Header("Progress Tracking")]
    public Transform progressContainer;
    public Slider puzzleProgressSlider;
    public TextMeshProUGUI puzzleStatsText;
    public Slider explorationProgressSlider;
    public TextMeshProUGUI explorationStatsText;
    
    [Header("Alerts and Notifications")]
    public GameObject alertPanel;
    public TextMeshProUGUI alertText;
    public Image alertIcon;
    public Color normalAlertColor = Color.blue;
    public Color warningAlertColor = new Color(1f, 0.5f, 0f);
    public Color criticalAlertColor = Color.red;
    
    [Header("Data Visualization")]
    public GameObject timeSeriesChartPrefab;
    public Transform chartsContainer;
    public bool showTimeSeriesCharts = true;
    
    // Dependencies
    private CuriousCityAutomated.Analytics.LearningStyleTracker learningTracker;
    private ParentAnalyticsReporter analyticsReporter;
    
    // Dashboard State
    private bool isDashboardVisible = false;
    private List<GameObject> currentInsightItems;
    private List<GameObject> activeCharts;
    private Queue<string> recentAlerts;
    private Dictionary<string, TimeSeriesChart> timeSeriesCharts;
    
    // Update intervals
    private float lastUpdate = 0f;
    private float updateInterval = 1f;
    private float alertDisplayDuration = 5f;
    
    private void Start()
    {
        InitializeDashboard();
    }
    
    private void InitializeDashboard()
    {
        // Get dependencies
        learningTracker = FindFirstObjectByType<CuriousCityAutomated.Analytics.LearningStyleTracker>();
        analyticsReporter = FindFirstObjectByType<ParentAnalyticsReporter>();
        
        // Initialize collections
        currentInsightItems = new List<GameObject>();
        activeCharts = new List<GameObject>();
        recentAlerts = new Queue<string>();
        timeSeriesCharts = new Dictionary<string, TimeSeriesChart>();
        
        // Set up UI
        if (toggleDashboardButton)
        {
            toggleDashboardButton.onClick.AddListener(ToggleDashboard);
        }
        
        if (exportDataButton)
        {
            exportDataButton.onClick.AddListener(ExportAnalyticsData);
        }
        
        // Initially hide dashboard
        if (dashboardPanel)
        {
            dashboardPanel.SetActive(false);
        }
        
        // Subscribe to learning events
        if (learningTracker)
        {
            // Subscribe with explicit namespace qualification
            CuriousCityAutomated.Analytics.LearningStyleTracker.OnRealTimeMetricsUpdated += HandleRealTimeMetricsUpdate;
            CuriousCityAutomated.Analytics.LearningStyleTracker.OnInsightGenerated += DisplayNewInsight;
            CuriousCityAutomated.Analytics.LearningStyleTracker.OnProfileUpdated += UpdateLearningStyleDisplay;
        }
        
        // Initialize time series charts if enabled
        if (showTimeSeriesCharts)
        {
            InitializeTimeSeriesCharts();
        }
        
        // Start dashboard update loop
        StartCoroutine(DashboardUpdateLoop());
    }
    
    // Wrapper method to handle the metrics update with explicit typing
    private void HandleRealTimeMetricsUpdate(CuriousCityAutomated.Analytics.RealTimeMetrics metrics)
    {
        UpdateRealTimeMetrics(metrics);
    }
    
    private void InitializeTimeSeriesCharts()
    {
        var chartMetrics = new string[]
        {
            "engagement_level", "confidence_level", "curiosity_level",
            "frustration_level", "focus_level"
        };
        
        foreach (var metric in chartMetrics)
        {
            CreateTimeSeriesChart(metric);
        }
    }
    
    private void CreateTimeSeriesChart(string metricName)
    {
        if (timeSeriesChartPrefab && chartsContainer)
        {
            var chartObject = Instantiate(timeSeriesChartPrefab, chartsContainer);
            var chart = chartObject.GetComponent<TimeSeriesChart>();
            
            if (chart)
            {
                chart.Initialize(metricName, GetMetricDisplayName(metricName));
                timeSeriesCharts[metricName] = chart;
                activeCharts.Add(chartObject);
            }
        }
    }
    
    private string GetMetricDisplayName(string metricName)
    {
        switch (metricName)
        {
            case "engagement_level": return "Engagement";
            case "confidence_level": return "Confidence";
            case "curiosity_level": return "Curiosity";
            case "frustration_level": return "Frustration";
            case "focus_level": return "Focus";
            default: return metricName.Replace("_", " ");
        }
    }
    
    private IEnumerator DashboardUpdateLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(updateInterval);
            
            if (isDashboardVisible && learningTracker)
            {
                UpdateDashboardDisplays();
            }
        }
    }
    
    private void UpdateDashboardDisplays()
    {
        // Update learning style indicators
        UpdateLearningStyleIndicators();
        
        // Update session statistics
        UpdateSessionStatistics();
        
        // Update behavioral insights
        UpdateBehavioralInsights();
        
        // Update progress tracking
        UpdateProgressTracking();
        
        // Update time series charts
        UpdateTimeSeriesCharts();
        
        // Check for new alerts
        CheckForAlerts();
    }
    
    private void UpdateLearningStyleIndicators()
    {
        if (!learningTracker) return;
        
        // Update sliders with current learning style weights
        if (visualSpatialIndicator) 
            visualSpatialIndicator.value = learningTracker.visualSpatialWeight;
        if (verbalLinguisticIndicator) 
            verbalLinguisticIndicator.value = learningTracker.verbalLinguisticWeight;
        if (logicalMathematicalIndicator) 
            logicalMathematicalIndicator.value = learningTracker.logicalMathematicalWeight;
        if (kinestheticIndicator) 
            kinestheticIndicator.value = learningTracker.kinestheticWeight;
        if (musicalIndicator) 
            musicalIndicator.value = learningTracker.musicalWeight;
        if (interpersonalIndicator) 
            interpersonalIndicator.value = learningTracker.interpersonalWeight;
        if (intrapersonalIndicator) 
            intrapersonalIndicator.value = learningTracker.intrapersonalWeight;
        if (naturalistIndicator) 
            naturalistIndicator.value = learningTracker.naturalistWeight;
    }
    
    private void UpdateRealTimeMetrics(CuriousCityAutomated.Analytics.RealTimeMetrics metrics)
    {
        if (!isDashboardVisible) return;
        
        // Update real-time metric displays
        if (engagementMeter) engagementMeter.value = metrics.engagementLevel;
        if (frustrationMeter) frustrationMeter.value = metrics.frustrationLevel;
        if (confidenceMeter) confidenceMeter.value = 0.5f; // Default value - property doesn't exist in RealTimeMetrics
        if (curiosityMeter) curiosityMeter.value = 0.5f; // Default value - property doesn't exist in RealTimeMetrics
        if (focusMeter) focusMeter.value = metrics.focusLevel;
        
        // Update engagement status text
        if (engagementStatusText)
        {
            string status = GetEngagementStatus(metrics.engagementLevel);
            engagementStatusText.text = $"Engagement: {status}";
            engagementStatusText.color = GetEngagementColor(metrics.engagementLevel);
        }
        
        // Update time series charts
        UpdateTimeSeriesChartsWithMetrics(metrics);
        
        // Check for concerning metrics
        CheckMetricConcerns(metrics);
    }
    
    private void UpdateSessionStatistics()
    {
        if (!learningTracker) return;
        
        var sessionMetrics = learningTracker.GetSessionMetrics();
        
        // Update session duration
        if (sessionDurationText)
        {
            float sessionTime = Time.time - learningTracker.sessionStartTime;
            sessionDurationText.text = $"Session: {FormatTime(sessionTime)}";
        }
        
        // Update total interactions
        if (totalInteractionsText)
        {
            float totalInteractions = sessionMetrics.GetValueOrDefault("total_interactions", 0f);
            totalInteractionsText.text = $"Interactions: {totalInteractions:F0}";
        }
    }
    
    private void UpdateBehavioralInsights()
    {
        // Clear existing insights
        foreach (var item in currentInsightItems)
        {
            if (item) Destroy(item);
        }
        currentInsightItems.Clear();
        
        // Generate current behavioral insights
        var insights = GenerateCurrentInsights();
        
        // Display insights
        foreach (var insight in insights)
        {
            CreateInsightItem(insight);
        }
        
        // Update dominant learning style
        if (dominantLearningStyleText)
        {
            dominantLearningStyleText.text = $"Primary Style: {GetDominantLearningStyle()}";
        }
        
        // Update current activity
        if (currentActivityText)
        {
            currentActivityText.text = $"Activity: {GetCurrentActivity()}";
        }
    }
    
    private void UpdateProgressTracking()
    {
        if (!learningTracker) return;
        
        var sessionMetrics = learningTracker.GetSessionMetrics();
        
        // Update puzzle progress
        if (puzzleProgressSlider && puzzleStatsText)
        {
            float puzzleCompletion = sessionMetrics.GetValueOrDefault("puzzle_completion_rate", 0f);
            puzzleProgressSlider.value = puzzleCompletion;
            
            int completed = (int)sessionMetrics.GetValueOrDefault("puzzles_completed", 0f);
            int attempted = (int)sessionMetrics.GetValueOrDefault("puzzles_attempted", 0f);
            puzzleStatsText.text = $"Puzzles: {completed}/{attempted}";
        }
        
        // Update exploration progress
        if (explorationProgressSlider && explorationStatsText)
        {
            float explorationRate = sessionMetrics.GetValueOrDefault("exploration_rate", 0f);
            explorationProgressSlider.value = explorationRate;
            
            int areasExplored = (int)sessionMetrics.GetValueOrDefault("areas_explored", 0f);
            explorationStatsText.text = $"Areas Explored: {areasExplored}";
        }
    }
    
    private void UpdateTimeSeriesCharts()
    {
        if (!showTimeSeriesCharts || !learningTracker) return;
        
        var sessionMetrics = learningTracker.GetSessionMetrics();
        
        // Update each time series chart with latest data
        foreach (var kvp in timeSeriesCharts)
        {
            string metricName = kvp.Key;
            var chart = kvp.Value;
            
            if (sessionMetrics.ContainsKey(metricName))
            {
                chart.AddDataPoint(sessionMetrics[metricName]);
            }
        }
    }
    
    private void UpdateTimeSeriesChartsWithMetrics(CuriousCityAutomated.Analytics.RealTimeMetrics metrics)
    {
        if (!showTimeSeriesCharts) return;
        
        // Update charts with real-time metrics
        if (timeSeriesCharts.ContainsKey("engagement_level"))
            timeSeriesCharts["engagement_level"].AddDataPoint(metrics.engagementLevel);
        
        // Note: confidence_level and curiosity_level don't exist in RealTimeMetrics
        // Using session metrics or default values for these
        if (timeSeriesCharts.ContainsKey("confidence_level") && learningTracker != null)
        {
            var sessionMetrics = learningTracker.GetSessionMetrics();
            timeSeriesCharts["confidence_level"].AddDataPoint(sessionMetrics.GetValueOrDefault("confidence_level", 0.5f));
        }
        
        if (timeSeriesCharts.ContainsKey("curiosity_level") && learningTracker != null)
        {
            var sessionMetrics = learningTracker.GetSessionMetrics();
            timeSeriesCharts["curiosity_level"].AddDataPoint(sessionMetrics.GetValueOrDefault("curiosity_level", 0.5f));
        }
        
        if (timeSeriesCharts.ContainsKey("frustration_level"))
            timeSeriesCharts["frustration_level"].AddDataPoint(metrics.frustrationLevel);
        
        if (timeSeriesCharts.ContainsKey("focus_level"))
            timeSeriesCharts["focus_level"].AddDataPoint(metrics.focusLevel);
    }
    
    private void CheckForAlerts()
    {
        if (!learningTracker) return;
        
        var sessionMetrics = learningTracker.GetSessionMetrics();
        
        // Check for low engagement
        float engagement = sessionMetrics.GetValueOrDefault("engagement_level", 0.5f);
        if (engagement < 0.3f)
        {
            ShowAlert("Low engagement detected. Consider changing activity or taking a break.", 
                AlertLevel.Warning);
        }
        
        // Check for high frustration
        float frustration = sessionMetrics.GetValueOrDefault("frustration_level", 0f);
        if (frustration > 0.7f)
        {
            ShowAlert("High frustration level. Child may need help or encouragement.", 
                AlertLevel.Critical);
        }
        
        // Check for extended session without break
        float sessionTime = Time.time - learningTracker.sessionStartTime;
        if (sessionTime > 1800f) // 30 minutes
        {
            ShowAlert("Extended session detected. Consider taking a break.", 
                AlertLevel.Normal);
        }
    }
    
    private void CheckMetricConcerns(CuriousCityAutomated.Analytics.RealTimeMetrics metrics)
    {
        // Real-time monitoring for concerning patterns
        
        if (metrics.frustrationLevel > 0.8f && metrics.engagementLevel < 0.3f)
        {
            ShowAlert("Child appears frustrated and disengaged. Intervention recommended.", 
                AlertLevel.Critical);
        }
        
        if (metrics.focusLevel < 0.2f)
        {
            ShowAlert("Low focus detected. Child may be distracted or tired.", 
                AlertLevel.Warning);
        }
        
        // Check confidence from session metrics instead of RealTimeMetrics
        if (learningTracker != null)
        {
            var sessionMetrics = learningTracker.GetSessionMetrics();
            float confidenceLevel = sessionMetrics.GetValueOrDefault("confidence_level", 0.5f);
            
            if (confidenceLevel < 0.2f)
            {
                ShowAlert("Low confidence detected. Child may need encouragement.", 
                    AlertLevel.Warning);
            }
        }
    }
    
    private List<string> GenerateCurrentInsights()
    {
        var insights = new List<string>();
        
        if (!learningTracker) return insights;
        
        // Generate insights based on current behavior patterns
        var sessionMetrics = learningTracker.GetSessionMetrics();
        
        // Learning style insights
        string dominantStyle = GetDominantLearningStyle();
        insights.Add($"Shows strong {dominantStyle} learning preference");
        
        // Behavioral insights
        float persistence = sessionMetrics.GetValueOrDefault("persistence_factor", 0.5f);
        if (persistence > 0.7f)
        {
            insights.Add("Demonstrates high persistence through challenges");
        }
        
        float helpSeeking = sessionMetrics.GetValueOrDefault("help_seeking_behavior", 0.5f);
        if (helpSeeking < 0.3f)
        {
            insights.Add("Prefers independent problem-solving");
        }
        else if (helpSeeking > 0.7f)
        {
            insights.Add("Comfortable seeking help when needed");
        }
        
        float exploration = sessionMetrics.GetValueOrDefault("exploration_tendency", 0.5f);
        if (exploration > 0.7f)
        {
            insights.Add("Shows high curiosity and exploration drive");
        }
        
        return insights;
    }
    
    private void CreateInsightItem(string insight)
    {
        if (insightItemPrefab && behaviorInsightsContainer)
        {
            var insightItem = Instantiate(insightItemPrefab, behaviorInsightsContainer);
            var textComponent = insightItem.GetComponentInChildren<TextMeshProUGUI>();
            
            if (textComponent)
            {
                textComponent.text = $"• {insight}";
            }
            
            currentInsightItems.Add(insightItem);
        }
    }
    
    private string GetDominantLearningStyle()
    {
        if (!learningTracker) return "Unknown";
        
        var weights = new Dictionary<string, float>
        {
            ["Visual-Spatial"] = learningTracker.visualSpatialWeight,
            ["Verbal-Linguistic"] = learningTracker.verbalLinguisticWeight,
            ["Logical-Mathematical"] = learningTracker.logicalMathematicalWeight,
            ["Kinesthetic"] = learningTracker.kinestheticWeight,
            ["Musical"] = learningTracker.musicalWeight,
            ["Interpersonal"] = learningTracker.interpersonalWeight,
            ["Intrapersonal"] = learningTracker.intrapersonalWeight,
            ["Naturalist"] = learningTracker.naturalistWeight
        };
        
        return weights.OrderByDescending(kvp => kvp.Value).First().Key;
    }
    
    private string GetCurrentActivity()
    {
        // Check for puzzle activity
        var puzzleManager = FindFirstObjectByType<PuzzleManager>();
        if (puzzleManager != null && puzzleManager.IsPuzzleActive)
        {
            return "Puzzle Solving";
        }
        
        var crewManager = FindFirstObjectByType<CrewInteractionManager>();
        if (crewManager != null)
        {
            return "Social Interaction";
        }
        
        return "Exploration";
    }
    
    private string GetEngagementStatus(float engagementLevel)
    {
        if (engagementLevel > 0.8f) return "Highly Engaged";
        else if (engagementLevel > 0.6f) return "Engaged";
        else if (engagementLevel > 0.4f) return "Moderately Engaged";
        else if (engagementLevel > 0.2f) return "Low Engagement";
        else return "Disengaged";
    }
    
    private Color GetEngagementColor(float engagementLevel)
    {
        if (engagementLevel > 0.6f) return Color.green;
        else if (engagementLevel > 0.4f) return Color.yellow;
        else return Color.red;
    }
    
    private void ShowAlert(string message, AlertLevel level)
    {
        if (alertPanel && alertText)
        {
            alertPanel.SetActive(true);
            alertText.text = message;
            
            if (alertIcon)
            {
                alertIcon.color = GetAlertColor(level);
            }
            
            // Auto-hide alert after duration
            StartCoroutine(HideAlertAfterDelay());
        }
        
        // Add to recent alerts queue
        recentAlerts.Enqueue($"{System.DateTime.Now:HH:mm:ss} - {message}");
        if (recentAlerts.Count > 10)
        {
            recentAlerts.Dequeue();
        }
    }
    
    private Color GetAlertColor(AlertLevel level)
    {
        switch (level)
        {
            case AlertLevel.Normal: return normalAlertColor;
            case AlertLevel.Warning: return warningAlertColor;
            case AlertLevel.Critical: return criticalAlertColor;
            default: return normalAlertColor;
        }
    }
    
    private IEnumerator HideAlertAfterDelay()
    {
        yield return new WaitForSeconds(alertDisplayDuration);
        
        if (alertPanel)
        {
            alertPanel.SetActive(false);
        }
    }
    
    private string FormatTime(float timeInSeconds)
    {
        int minutes = (int)(timeInSeconds / 60);
        int seconds = (int)(timeInSeconds % 60);
        return $"{minutes:00}:{seconds:00}";
    }
    
    // Event handlers
    private void DisplayNewInsight(CuriousCityAutomated.Analytics.LearningInsight insight)
    {
        ShowAlert($"New Insight: {insight.summary}", AlertLevel.Normal);
    }
    
    private void UpdateLearningStyleDisplay(CuriousCityAutomated.Analytics.LearningStyleProfile profile)
    {
        // Update dashboard when learning style profile changes
        UpdateLearningStyleIndicators();
    }
    
    // Public API
    public void ToggleDashboard()
    {
        isDashboardVisible = !isDashboardVisible;
        
        if (dashboardPanel)
        {
            dashboardPanel.SetActive(isDashboardVisible);
        }
        
        if (isDashboardVisible)
        {
            UpdateDashboardDisplays();
        }
    }
    
    public void ExportAnalyticsData()
    {
        if (analyticsReporter)
        {
            analyticsReporter.GenerateComprehensiveReport();
            ShowAlert("Analytics data exported successfully!", AlertLevel.Normal);
        }
    }
    
    public bool IsDashboardVisible()
    {
        return isDashboardVisible;
    }
    
    private void OnDestroy()
    {
        // Unsubscribe from events
        if (learningTracker)
        {
            // Unsubscribe with explicit namespace qualification
            CuriousCityAutomated.Analytics.LearningStyleTracker.OnRealTimeMetricsUpdated -= HandleRealTimeMetricsUpdate;
            CuriousCityAutomated.Analytics.LearningStyleTracker.OnInsightGenerated -= DisplayNewInsight;
            CuriousCityAutomated.Analytics.LearningStyleTracker.OnProfileUpdated -= UpdateLearningStyleDisplay;
        }
        
        // Clean up UI
        if (toggleDashboardButton)
        {
            toggleDashboardButton.onClick.RemoveAllListeners();
        }
        
        if (exportDataButton)
        {
            exportDataButton.onClick.RemoveAllListeners();
        }
    }
}

/// <summary>
/// Alert level enumeration for dashboard notifications
/// </summary>
public enum AlertLevel
{
    Normal,
    Warning,
    Critical
}

/// <summary>
/// Simple time series chart component for analytics visualization
/// </summary>
public class TimeSeriesChart : MonoBehaviour
{
    [Header("Chart Configuration")]
    public int maxDataPoints = 100;
    public float chartWidth = 200f;
    public float chartHeight = 100f;
    public Color chartLineColor = Color.blue;
    public LineRenderer lineRenderer;
    public TextMeshProUGUI chartLabel;
    
    private List<float> dataPoints;
    private string metricName;
    
    public void Initialize(string metric, string displayName)
    {
        metricName = metric;
        dataPoints = new List<float>();
        
        if (chartLabel)
        {
            chartLabel.text = displayName;
        }
        
        if (lineRenderer)
        {
            lineRenderer.startColor = chartLineColor;
            lineRenderer.endColor = chartLineColor;
            lineRenderer.startWidth = 2f;
            lineRenderer.endWidth = 2f;
        }
    }
    
    public void AddDataPoint(float value)
    {
        dataPoints.Add(value);
        
        if (dataPoints.Count > maxDataPoints)
        {
            dataPoints.RemoveAt(0);
        }
        
        UpdateChartVisualization();
    }
    
    private void UpdateChartVisualization()
    {
        if (lineRenderer && dataPoints.Count > 1)
        {
            lineRenderer.positionCount = dataPoints.Count;
            
            for (int i = 0; i < dataPoints.Count; i++)
            {
                float x = (i / (float)(dataPoints.Count - 1)) * chartWidth;
                float y = dataPoints[i] * chartHeight;
                lineRenderer.SetPosition(i, new Vector3(x, y, 0));
            }
        }
    }
}