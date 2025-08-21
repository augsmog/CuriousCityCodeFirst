using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace CuriousCity.Analytics.Analyzers
{
    /// <summary>
    /// Analyzes player interaction patterns for learning insights
    /// </summary>
    public class InteractionAnalyzer : MonoBehaviour
    {
        [Header("Analysis Configuration")]
        public float interactionRange = 5f;
        public LayerMask interactableLayer = -1;
        public float sampleInterval = 0.2f;
        
        // Dependencies
        private CuriousCityAutomated.Analytics.LearningStyleTracker learningTracker;
        private Transform playerTransform;
        private Camera playerCamera;
        
        // Interaction tracking
        private List<InteractionEvent> interactionHistory;
        private GameObject currentTarget;
        private float currentHoverTime;
        private float lastSampleTime;
        
        // Behavioral metrics
        private Dictionary<string, float> interactionTimes;
        private Dictionary<string, int> interactionCounts;
        private float averageResponseTime;
        private int hesitationCount;
        
        [System.Serializable]
        public class InteractionEvent
        {
            public string targetName;
            public string interactionType;
            public float timestamp;
            public float duration;
            public bool completed;
            public float responseTime;
        }
        
        void Start()
        {
            InitializeAnalyzer();
        }
        
        void InitializeAnalyzer()
        {
            learningTracker = FindObjectOfType<CuriousCityAutomated.Analytics.LearningStyleTracker>();
            playerTransform = GetComponent<Transform>();
            playerCamera = GetComponentInChildren<Camera>();
            
            if (playerCamera == null)
                playerCamera = Camera.main;
            
            interactionHistory = new List<InteractionEvent>();
            interactionTimes = new Dictionary<string, float>();
            interactionCounts = new Dictionary<string, int>();
            
            lastSampleTime = Time.time;
        }
        
        void Update()
        {
            if (Time.time - lastSampleTime >= sampleInterval)
            {
                AnalyzeInteractions();
                lastSampleTime = Time.time;
            }
            
            TrackHoverBehavior();
        }
        
        void AnalyzeInteractions()
        {
            if (playerCamera == null) return;
            
            // Cast ray to detect what player is looking at
            Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
            RaycastHit hit;
            
            if (Physics.Raycast(ray, out hit, interactionRange, interactableLayer))
            {
                // Check for the Gameplay.Interactions version (the one we're keeping)
                var interactable = hit.collider.GetComponent<CuriousCityAutomated.Gameplay.Interactions.IInteractable>();
                
                if (interactable != null)
                {
                    ProcessInteractableTarget(hit.collider.gameObject);
                }
            }
            else
            {
                // No target in range
                if (currentTarget != null)
                {
                    EndHoverTracking();
                }
            }
        }
        
        void ProcessInteractableTarget(GameObject target)
        {
            if (currentTarget != target)
            {
                if (currentTarget != null)
                {
                    EndHoverTracking();
                }
                
                currentTarget = target;
                currentHoverTime = 0f;
                
                // Log focus change
                if (learningTracker != null)
                {
                    learningTracker.LogEvent("interaction_focus", $"Focused on {target.name}");
                }
            }
        }
        
        void TrackHoverBehavior()
        {
            if (currentTarget != null)
            {
                currentHoverTime += Time.deltaTime;
                
                // Track hesitation (hovering without interacting)
                if (currentHoverTime > 2f && currentHoverTime < 2.1f)
                {
                    hesitationCount++;
                    
                    if (learningTracker != null)
                    {
                        learningTracker.LogDetailedEvent(
                            "interaction_hesitation",
                            $"Hesitated at {currentTarget.name}",
                            "interaction",
                            new Dictionary<string, object>
                            {
                                {"target", currentTarget.name},
                                {"hover_time", currentHoverTime}
                            }
                        );
                    }
                }
                
                // Check for actual interaction
                if (Input.GetKeyDown(KeyCode.E) || Input.GetMouseButtonDown(0))
                {
                    RecordInteraction();
                }
            }
        }
        
        void RecordInteraction()
        {
            if (currentTarget == null) return;
            
            var interactionEvent = new InteractionEvent
            {
                targetName = currentTarget.name,
                interactionType = DetermineInteractionType(currentTarget),
                timestamp = Time.time,
                duration = currentHoverTime,
                completed = true,
                responseTime = currentHoverTime
            };
            
            interactionHistory.Add(interactionEvent);
            
            // Update metrics
            if (!interactionCounts.ContainsKey(interactionEvent.interactionType))
            {
                interactionCounts[interactionEvent.interactionType] = 0;
                interactionTimes[interactionEvent.interactionType] = 0f;
            }
            
            interactionCounts[interactionEvent.interactionType]++;
            interactionTimes[interactionEvent.interactionType] += interactionEvent.duration;
            
            // Calculate average response time
            if (interactionHistory.Count > 0)
            {
                averageResponseTime = interactionHistory.Average(e => e.responseTime);
            }
            
            // Log to learning tracker
            if (learningTracker != null)
            {
                learningTracker.LogInteraction(interactionEvent.interactionType, interactionEvent.duration);
                
                // Analyze interaction pattern
                AnalyzeInteractionPattern(interactionEvent);
            }
        }
        
        void EndHoverTracking()
        {
            if (currentTarget != null && currentHoverTime > 0.1f)
            {
                // Record incomplete interaction (looked but didn't interact)
                var interactionEvent = new InteractionEvent
                {
                    targetName = currentTarget.name,
                    interactionType = DetermineInteractionType(currentTarget),
                    timestamp = Time.time,
                    duration = currentHoverTime,
                    completed = false,
                    responseTime = currentHoverTime
                };
                
                interactionHistory.Add(interactionEvent);
            }
            
            currentTarget = null;
            currentHoverTime = 0f;
        }
        
        string DetermineInteractionType(GameObject target)
        {
            if (target.CompareTag("Puzzle"))
                return "puzzle";
            if (target.CompareTag("NPC"))
                return "social";
            if (target.CompareTag("Artifact"))
                return "collection";
            if (target.CompareTag("Door"))
                return "navigation";
            
            return "general";
        }
        
        void AnalyzeInteractionPattern(InteractionEvent interaction)
        {
            if (learningTracker == null) return;
            
            // Quick interactions suggest confidence
            if (interaction.responseTime < 1f)
            {
                learningTracker.LogInteractionPattern("quick_decision", 1f);
            }
            // Slow interactions suggest careful consideration
            else if (interaction.responseTime > 3f)
            {
                learningTracker.LogInteractionPattern("careful_consideration", 1f);
            }
            
            // Track interaction type preferences
            string dominantType = GetDominantInteractionType();
            if (!string.IsNullOrEmpty(dominantType))
            {
                learningTracker.LogDetailedEvent(
                    "interaction_preference",
                    $"Shows preference for {dominantType} interactions",
                    "behavioral",
                    new Dictionary<string, object>
                    {
                        {"dominant_type", dominantType},
                        {"interaction_counts", interactionCounts}
                    }
                );
            }
        }
        
        string GetDominantInteractionType()
        {
            if (interactionCounts.Count == 0) return "";
            
            return interactionCounts.OrderByDescending(kvp => kvp.Value).First().Key;
        }
        
        public InteractionMetrics GetMetrics()
        {
            return new InteractionMetrics
            {
                totalInteractions = interactionHistory.Count,
                completedInteractions = interactionHistory.Count(e => e.completed),
                averageResponseTime = averageResponseTime,
                hesitationCount = hesitationCount,
                dominantInteractionType = GetDominantInteractionType()
            };
        }
        
        [System.Serializable]
        public class InteractionMetrics
        {
            public int totalInteractions;
            public int completedInteractions;
            public float averageResponseTime;
            public int hesitationCount;
            public string dominantInteractionType;
        }
    }
}