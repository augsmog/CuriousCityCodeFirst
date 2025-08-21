using System.Collections.Generic;
using UnityEngine;
using CuriousCityAutomated.Gameplay.Interactions;
using CuriousCityAutomated.Analytics;

/// <summary>
/// Tracks player's visual attention and gaze patterns for learning analytics
/// </summary>
public class GazeTracker : MonoBehaviour
{
    [Header("Configuration")]
    public float gazeUpdateInterval = 0.1f;
    public float focusThreshold = 2f;
    public float maxGazeDistance = 50f;
    public LayerMask trackableLayers = -1;
    
    [Header("Analytics")]
    public bool enableHeatmapGeneration = true;
    public int heatmapResolution = 128;
    
    // Dependencies
    private Camera playerCamera;
    private LearningStyleTracker learningTracker;
    
    // Tracking data
    private Dictionary<string, float> gazeTargets;
    private Dictionary<string, float> focusDurations;
    private string currentFocusTarget;
    private float currentFocusStartTime;
    private List<Vector3> gazeHeatmapPoints;
    private AttentionMetrics currentMetrics;
    
    // Performance optimization
    private float lastUpdateTime;
    private RaycastHit[] raycastHits;
    
    void Start()
    {
        InitializeGazeTracking();
    }
    
    void InitializeGazeTracking()
    {
        playerCamera = GetComponent<Camera>();
        if (playerCamera == null)
        {
            playerCamera = Camera.main;
        }
        
        learningTracker = FindFirstObjectByType<LearningStyleTracker>();
        
        gazeTargets = new Dictionary<string, float>();
        focusDurations = new Dictionary<string, float>();
        gazeHeatmapPoints = new List<Vector3>();
        currentMetrics = new AttentionMetrics();
        raycastHits = new RaycastHit[10];
        
        lastUpdateTime = Time.time;
    }
    
    void Update()
    {
        if (Time.time - lastUpdateTime >= gazeUpdateInterval)
        {
            UpdateGazeTracking();
            lastUpdateTime = Time.time;
        }
    }
    
    void UpdateGazeTracking()
    {
        // Cast ray from camera center
        Ray gazeRay = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        
        // Use RaycastNonAlloc for performance
        int hitCount = Physics.RaycastNonAlloc(gazeRay, raycastHits, maxGazeDistance, trackableLayers);
        
        if (hitCount > 0)
        {
            // Process the closest hit
            RaycastHit closestHit = GetClosestHit(raycastHits, hitCount);
            ProcessGazeTarget(closestHit);
            
            // Track heatmap point
            if (enableHeatmapGeneration)
            {
                gazeHeatmapPoints.Add(closestHit.point);
            }
        }
        else
        {
            // Looking at empty space
            ProcessEmptyGaze();
        }
    }
    
    RaycastHit GetClosestHit(RaycastHit[] hits, int count)
    {
        RaycastHit closest = hits[0];
        float minDistance = closest.distance;
        
        for (int i = 1; i < count; i++)
        {
            if (hits[i].distance < minDistance)
            {
                closest = hits[i];
                minDistance = hits[i].distance;
            }
        }
        
        return closest;
    }
    
    void ProcessGazeTarget(RaycastHit hit)
    {
        string targetId = GetTargetIdentifier(hit.collider.gameObject);
        
        // Update gaze duration tracking
        if (!gazeTargets.ContainsKey(targetId))
        {
            gazeTargets[targetId] = 0f;
            currentMetrics.attentionSequence.Add(targetId);
        }
        
        gazeTargets[targetId] += gazeUpdateInterval;
        
        // Check for focus change
        if (currentFocusTarget != targetId)
        {
            if (!string.IsNullOrEmpty(currentFocusTarget))
            {
                // Log previous focus
                float focusDuration = Time.time - currentFocusStartTime;
                LogFocusEvent(currentFocusTarget, focusDuration);
            }
            
            currentFocusTarget = targetId;
            currentFocusStartTime = Time.time;
            currentMetrics.distractionCount++;
        }
        
        // Check for extended focus
        float currentFocusDuration = Time.time - currentFocusStartTime;
        if (currentFocusDuration >= focusThreshold && !focusDurations.ContainsKey(targetId))
        {
            focusDurations[targetId] = currentFocusDuration;
            LogExtendedFocus(targetId, hit.collider.gameObject);
        }
    }
    
    void ProcessEmptyGaze()
    {
        if (!string.IsNullOrEmpty(currentFocusTarget))
        {
            float focusDuration = Time.time - currentFocusStartTime;
            LogFocusEvent(currentFocusTarget, focusDuration);
            currentFocusTarget = "";
        }
    }
    
    string GetTargetIdentifier(GameObject target)
    {
        // Priority order for identification
        var interactable = target.GetComponent<IInteractable>();
        if (interactable != null)
        {
            return $"interactable_{interactable.GetInteractionType()}_{target.name}";
        }
        
        if (target.CompareTag("Puzzle"))
        {
            return $"puzzle_{target.name}";
        }
        
        if (target.CompareTag("Character"))
        {
            return $"character_{target.name}";
        }
        
        if (target.name.ToLower().Contains("text") || target.name.ToLower().Contains("sign"))
        {
            return $"text_{target.name}";
        }
        
        if (target.name.ToLower().Contains("art") || target.name.ToLower().Contains("decoration"))
        {
            return $"visual_{target.name}";
        }
        
        return $"object_{target.name}";
    }
    
    void LogFocusEvent(string targetId, float duration)
    {
        if (learningTracker == null) return;
        
        currentMetrics.focusDuration += duration;
        
        if (!currentMetrics.objectAttentionTime.ContainsKey(targetId))
        {
            currentMetrics.objectAttentionTime[targetId] = 0f;
        }
        currentMetrics.objectAttentionTime[targetId] += duration;
        
        // Determine learning style implications
        string learningContext = DetermineLearningContext(targetId);
        
        learningTracker.LogDetailedEvent("visual_focus", 
            $"Player focused on {targetId} for {duration:F1}s", 
            learningContext,
            new Dictionary<string, object>
            {
                {"target_id", targetId},
                {"focus_duration", duration},
                {"gaze_stability", CalculateGazeStability()},
                {"is_extended_focus", duration >= focusThreshold}
            });
    }
    
    void LogExtendedFocus(string targetId, GameObject target)
    {
        if (learningTracker == null) return;
        
        string analysisType = AnalyzeTargetType(target);
        
        learningTracker.LogDetailedEvent("extended_visual_focus", 
            $"Extended focus on {targetId}", 
            "deep_engagement",
            new Dictionary<string, object>
            {
                {"target_type", analysisType},
                {"learning_style_indicator", DetermineLearningStyleFromTarget(target)},
                {"shows_interest", true},
                {"focus_intensity", CalculateFocusIntensity(targetId)}
            });
    }
    
    string DetermineLearningContext(string targetId)
    {
        if (targetId.Contains("text") || targetId.Contains("sign"))
            return "verbal_linguistic_attention";
        if (targetId.Contains("puzzle"))
            return "problem_solving_focus";
        if (targetId.Contains("character"))
            return "social_attention";
        if (targetId.Contains("visual") || targetId.Contains("art"))
            return "visual_spatial_attention";
        
        return "environmental_scanning";
    }
    
    string AnalyzeTargetType(GameObject target)
    {
        var renderer = target.GetComponent<Renderer>();
        if (renderer != null)
        {
            if (renderer.material.mainTexture != null)
                return "textured_object";
        }
        
        if (target.GetComponent<Light>() != null)
            return "light_source";
        
        if (target.GetComponent<ParticleSystem>() != null)
            return "particle_effect";
        
        return "standard_object";
    }
    
    string DetermineLearningStyleFromTarget(GameObject target)
    {
        string targetName = target.name.ToLower();
        
        if (targetName.Contains("text") || targetName.Contains("book") || targetName.Contains("scroll"))
            return "verbal_linguistic";
        if (targetName.Contains("puzzle") || targetName.Contains("mechanism"))
            return "logical_mathematical";
        if (targetName.Contains("art") || targetName.Contains("pattern") || targetName.Contains("visual"))
            return "visual_spatial";
        if (targetName.Contains("character") || targetName.Contains("npc"))
            return "interpersonal";
        
        return "general_exploration";
    }
    
    float CalculateGazeStability()
    {
        if (gazeHeatmapPoints.Count < 10) return 1f;
        
        // Calculate variance in recent gaze points
        int sampleSize = Mathf.Min(20, gazeHeatmapPoints.Count);
        float totalVariance = 0f;
        
        for (int i = gazeHeatmapPoints.Count - sampleSize; i < gazeHeatmapPoints.Count - 1; i++)
        {
            totalVariance += Vector3.Distance(gazeHeatmapPoints[i], gazeHeatmapPoints[i + 1]);
        }
        
        float averageVariance = totalVariance / sampleSize;
        return Mathf.Clamp01(1f - (averageVariance / 2f)); // Normalize to 0-1
    }
    
    float CalculateFocusIntensity(string targetId)
    {
        if (!gazeTargets.ContainsKey(targetId)) return 0f;
        
        float totalGazeTime = 0f;
        foreach (var gaze in gazeTargets.Values)
        {
            totalGazeTime += gaze;
        }
        
        return gazeTargets[targetId] / totalGazeTime;
    }
    
    public AttentionMetrics GetCurrentMetrics()
    {
        currentMetrics.averageGazeStability = CalculateGazeStability();
        return currentMetrics;
    }
    
    public Dictionary<string, float> GetGazeHeatmap()
    {
        return new Dictionary<string, float>(gazeTargets);
    }
    
    public void ResetTracking()
    {
        gazeTargets.Clear();
        focusDurations.Clear();
        gazeHeatmapPoints.Clear();
        currentMetrics = new AttentionMetrics();
        currentFocusTarget = "";
    }
    
    void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying || !enableHeatmapGeneration) return;
        
        // Visualize gaze heatmap
        Gizmos.color = Color.red;
        foreach (var point in gazeHeatmapPoints)
        {
            Gizmos.DrawWireSphere(point, 0.1f);
        }
    }
}