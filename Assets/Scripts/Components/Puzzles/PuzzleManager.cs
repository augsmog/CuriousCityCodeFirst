using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using CuriousCityAutomated.Data;
using CuriousCityAutomated.Analytics;
using System.Linq;


namespace CuriousCity.Core
{
    public class PuzzleManager : MonoBehaviour
    {
        /// <summary>
        /// Manages all puzzle types in the historical missions.
        /// Handles puzzle UI overlays, tracks completion, and integrates with learning analytics.
        /// </summary>
        [Header("Puzzle UI Management")]
        public Canvas puzzleOverlayCanvas;
        public GameObject chronoCircuitsPrefab;
        public GameObject scrollOfSecretsPrefab;
        public GameObject pyramidRebuilderPrefab;
        
        [Header("Common UI Elements")]
        public Button exitPuzzleButton;
        public Button hintButton;
        public TextMeshProUGUI puzzleInstructions;
        public TextMeshProUGUI timerText;
        public GameObject completionPanel;
        public TextMeshProUGUI completionMessage;
        
        [Header("Audio")]
        public AudioSource puzzleAudioSource;
        public AudioClip puzzleStartSound;
        public AudioClip puzzleCompleteSound;
        public AudioClip puzzleFailSound;
        public AudioClip hintSound;
        
        // State Management
        private GameObject currentPuzzleInstance;
        private string currentPuzzleType;
        private bool puzzleActive = false;
        private float puzzleStartTime;
        private int hintCount = 0;
        private int attemptCount = 0;
        
        // Enhanced analytics tracking
        private List<PuzzleInteractionEvent> puzzleInteractions;
        private Dictionary<string, float> interactionTimings;
        private Vector3 lastMousePosition;
        private float totalIdleTime;
        private float lastInteractionTime;
        private int mouseClicks;
        private int keyboardInputs;
        
        // Dependencies
        private HistoricalMissionSceneManager missionManager;
        private LearningStyleTracker learningTracker;
        
        // Events
        public static event System.Action<string, PuzzleResults> OnPuzzleCompleted;
        public static event System.Action<string> OnPuzzleStarted;
        public static event System.Action<string> OnPuzzleExited;
        
        public void Initialize(HistoricalMissionSceneManager manager)
        {
            missionManager = manager;
            learningTracker = FindFirstObjectByType<LearningStyleTracker>();
            
            // Initialize analytics tracking
            puzzleInteractions = new List<PuzzleInteractionEvent>();
            interactionTimings = new Dictionary<string, float>();
            
            // Set up UI
            if (exitPuzzleButton)
            {
                exitPuzzleButton.onClick.AddListener(ExitCurrentPuzzle);
            }
            
            if (hintButton)
            {
                hintButton.onClick.AddListener(ShowHint);
            }
            
            // Initially hide puzzle canvas
            if (puzzleOverlayCanvas)
            {
                puzzleOverlayCanvas.gameObject.SetActive(false);
            }
        }
        
        public bool IsPuzzleActive => puzzleActive;

        public void StartPuzzle(string puzzleType)
        {
            Debug.Log($"[PuzzleManager] StartPuzzle called for puzzle: {puzzleType}");
            
            if (puzzleActive) 
            {
                Debug.Log($"[PuzzleManager] Puzzle already active, ignoring start request for: {puzzleType}");
                return;
            }
            
            Debug.Log($"[PuzzleManager] Starting puzzle: {puzzleType}");
            
            currentPuzzleType = puzzleType;
            puzzleActive = true;
            puzzleStartTime = Time.time;
            hintCount = 0;
            attemptCount = 0;
            mouseClicks = 0;
            keyboardInputs = 0;
            totalIdleTime = 0f;
            lastInteractionTime = Time.time;
            lastMousePosition = Input.mousePosition;
            
            // Clear previous interaction data
            puzzleInteractions.Clear();
            interactionTimings.Clear();
            
            // Log detailed puzzle start event
            if (learningTracker)
            {
                learningTracker.LogDetailedEvent("puzzle_started", $"Player began {puzzleType} puzzle", "puzzle_engagement",
                    new Dictionary<string, object>
                    {
                        {"puzzle_type", puzzleType},
                        {"start_time", puzzleStartTime},
                        {"player_experience_level", GetPlayerExperienceLevel(puzzleType)},
                        {"previous_attempts_this_session", GetPreviousAttempts(puzzleType)}
                    });
            }
            
            // Show puzzle overlay
            if (puzzleOverlayCanvas)
            {
                puzzleOverlayCanvas.gameObject.SetActive(true);
                Debug.Log($"[PuzzleManager] Puzzle overlay canvas activated");
            }
            else
            {
                Debug.LogWarning($"[PuzzleManager] No puzzle overlay canvas assigned!");
            }
            
            // Create puzzle instance
            CreatePuzzleInstance(puzzleType);
            
            // Play start sound
            if (puzzleAudioSource && puzzleStartSound)
            {
                puzzleAudioSource.PlayOneShot(puzzleStartSound);
            }
            
            // Start timer and interaction tracking
            StartCoroutine(PuzzleTimer());
            StartCoroutine(TrackPuzzleInteractions());
            
            // Track puzzle start
            if (learningTracker)
            {
                learningTracker.LogPuzzleStarted(puzzleType);
            }
            
            OnPuzzleStarted?.Invoke(puzzleType);
            
            Debug.Log($"[PuzzleManager] Puzzle {puzzleType} successfully started");
        }
        
        private float GetExpectedTime(string puzzleType)
        {
            switch (puzzleType.ToLower())
            {
                case "chronocircuits": return 60f; // 1 minute expected
                case "scrollofsecrets": return 90f; // 1.5 minutes expected
                case "pyramidrebuilder": return 120f; // 2 minutes expected
                default: return 60f;
            }
        }
        
        private IEnumerator TrackPuzzleInteractions()
        {
            while (puzzleActive)
            {
                // Track mouse movement and clicks
                Vector3 currentMousePos = Input.mousePosition;
                if (Vector3.Distance(currentMousePos, lastMousePosition) > 5f)
                {
                    RecordInteraction("mouse_movement", Vector3.Distance(currentMousePos, lastMousePosition));
                    lastMousePosition = currentMousePos;
                    lastInteractionTime = Time.time;
                }
                
                // Track clicks
                if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))
                {
                    mouseClicks++;
                    RecordInteraction("mouse_click", 1f);
                    lastInteractionTime = Time.time;
                }
                
                // Track keyboard input
                if (Input.inputString.Length > 0)
                {
                    keyboardInputs++;
                    RecordInteraction("keyboard_input", 1f);
                    lastInteractionTime = Time.time;
                }
                
                // Track idle time
                if (Time.time - lastInteractionTime > 3f)
                {
                    totalIdleTime += Time.deltaTime;
                    
                    // Log extended idle periods
                    if (Time.time - lastInteractionTime > 10f)
                    {
                        if (learningTracker)
                        {
                            learningTracker.LogDetailedEvent("puzzle_idle_period", "Extended period without interaction", "engagement_analysis",
                                new Dictionary<string, object>
                                {
                                    {"idle_duration", Time.time - lastInteractionTime},
                                    {"puzzle_type", currentPuzzleType},
                                    {"possible_frustration", totalIdleTime > 30f}
                                });
                        }
                        lastInteractionTime = Time.time; // Reset to avoid spam
                    }
                }
                
                yield return new WaitForSeconds(0.1f);
            }
        }
        
        private void RecordInteraction(string interactionType, float value)
        {
            var interaction = new PuzzleInteractionEvent
            {
                interactionType = interactionType,
                timestamp = Time.time - puzzleStartTime,
                value = value,
                mousePosition = Input.mousePosition
            };
            
            puzzleInteractions.Add(interaction);
            
            if (!interactionTimings.ContainsKey(interactionType))
                interactionTimings[interactionType] = 0f;
            interactionTimings[interactionType] += value;
        }
        
        private int GetPlayerExperienceLevel(string puzzleType)
        {
            // Calculate experience based on previous puzzle completions
            // This would integrate with save data in a full implementation
            return PlayerPrefs.GetInt($"{puzzleType}_completions", 0);
        }
        
        private int GetPreviousAttempts(string puzzleType)
        {
            return PlayerPrefs.GetInt($"{puzzleType}_attempts_today", 0);
        }
        
        private void CreatePuzzleInstance(string puzzleType)
        {
            GameObject prefab = null;
            string instructions = "";
            
            switch (puzzleType.ToLower())
            {
                case "chronocircuits":
                    prefab = chronoCircuitsPrefab;
                    instructions = "Connect the water flow from source to destination using logical pathways.";
                    break;
                case "scrollofsecrets":
                    prefab = scrollOfSecretsPrefab;
                    instructions = "Complete the ancient text by filling in the missing hieroglyphic words.";
                    break;
                case "pyramidrebuilder":
                    prefab = pyramidRebuilderPrefab;
                    instructions = "Reconstruct the mural pattern by arranging the scattered pieces.";
                    break;
            }
            
            if (prefab != null)
            {
                currentPuzzleInstance = Instantiate(prefab, puzzleOverlayCanvas.transform);
                
                // Set up puzzle-specific callbacks
                SetupPuzzleCallbacks(currentPuzzleInstance, puzzleType);
            }
            
            // Update instructions
            if (puzzleInstructions)
            {
                puzzleInstructions.text = instructions;
            }
        }
        
        private void SetupPuzzleCallbacks(GameObject puzzleInstance, string puzzleType)
        {
            switch (puzzleType.ToLower())
            {
                case "chronocircuits":
                    var chronoCircuits = puzzleInstance.GetComponent<ChronoCircuitsPuzzle>();
                    if (chronoCircuits != null)
                    {
                        chronoCircuits.OnPuzzleSolved += () => CompletePuzzle(true);
                        chronoCircuits.OnAttemptMade += () => RecordAttempt();
                    }
                    break;
                    
                case "scrollofsecrets":
                    var scrollPuzzle = puzzleInstance.GetComponent<ScrollOfSecretsPuzzle>();
                    if (scrollPuzzle != null)
                    {
                        scrollPuzzle.OnPuzzleSolved += () => CompletePuzzle(true);
                        scrollPuzzle.OnAttemptMade += () => RecordAttempt();
                    }
                    break;
                    
                case "pyramidrebuilder":
                    var pyramidPuzzle = puzzleInstance.GetComponent<PyramidRebuilderPuzzle>();
                    if (pyramidPuzzle != null)
                    {
                        pyramidPuzzle.OnPuzzleSolved += () => CompletePuzzle(true);
                        pyramidPuzzle.OnAttemptMade += () => RecordAttempt();
                    }
                    break;
            }
        }
        
        private void RecordAttempt()
        {
            attemptCount++;
            
            // Track learning behavior
            if (learningTracker)
            {
                // FIX: Use LogDetailedEvent instead of LogPuzzleAttempt
                learningTracker.LogDetailedEvent("puzzle_attempt", $"Attempted {currentPuzzleType}", "puzzle",
                    new Dictionary<string, object> 
                    { 
                        ["puzzle_type"] = currentPuzzleType,
                        ["success"] = false,
                        ["time"] = Time.time - puzzleStartTime,
                        ["attempt_count"] = attemptCount
                    });
            }
        }
        
        public void CompletePuzzle(bool success)
        {
            if (!puzzleActive) return;
            
            puzzleActive = false;
            float completionTime = Time.time - puzzleStartTime;
            
            // Create comprehensive puzzle results with detailed analytics
            var results = new PuzzleResults
            {
                puzzleType = currentPuzzleType,
                wasSolved = success,
                completionTime = completionTime,
                hintsUsed = hintCount,
                attempts = attemptCount,
                learningStyleTags = GetLearningStyleTags(currentPuzzleType),
                interactionMetrics = GenerateInteractionMetrics()
            };
            
            // Enhanced analytics logging
            if (learningTracker)
            {
                learningTracker.LogDetailedEvent("puzzle_completed", $"Puzzle {currentPuzzleType} finished", "puzzle_resolution",
                    new Dictionary<string, object>
                    {
                        {"success", success},
                        {"completion_time", completionTime},
                        {"hints_used", hintCount},
                        {"attempts", attemptCount},
                        {"mouse_clicks", mouseClicks},
                        {"keyboard_inputs", keyboardInputs},
                        {"idle_time_percentage", (totalIdleTime / completionTime) * 100f},
                        {"interaction_frequency", puzzleInteractions.Count / completionTime},
                        {"efficiency_score", CalculateEfficiencyScore(success, completionTime, hintCount, attemptCount)}
                    });
                
                // Log detailed interaction pattern analysis
                AnalyzePuzzleInteractionPatterns();
            }
            
            // Update player experience tracking
            PlayerPrefs.SetInt($"{currentPuzzleType}_attempts_today", 
                PlayerPrefs.GetInt($"{currentPuzzleType}_attempts_today", 0) + 1);
            
            if (success)
            {
                PlayerPrefs.SetInt($"{currentPuzzleType}_completions", 
                    PlayerPrefs.GetInt($"{currentPuzzleType}_completions", 0) + 1);
            }
            
            // Play completion sound
            AudioClip soundToPlay = success ? puzzleCompleteSound : puzzleFailSound;
            if (puzzleAudioSource && soundToPlay)
            {
                puzzleAudioSource.PlayOneShot(soundToPlay);
            }
            
            // Show completion panel
            ShowCompletionPanel(success, results);
            
            // Track completion
            if (learningTracker)
            {
                learningTracker.LogPuzzleCompleted(currentPuzzleType, results);
            }
            
            // Notify mission manager
            if (missionManager)
            {
                missionManager.RegisterPuzzleCompletion(currentPuzzleType, results);
            }
            
            OnPuzzleCompleted?.Invoke(currentPuzzleType, results);
            
            // Auto-close after a delay
            StartCoroutine(AutoClosePuzzle(3f));
        }
        
        private Dictionary<string, float> GenerateInteractionMetrics()
        {
            var metrics = new Dictionary<string, float>
            {
                ["total_interactions"] = puzzleInteractions.Count,
                ["mouse_clicks"] = mouseClicks,
                ["keyboard_inputs"] = keyboardInputs,
                ["idle_time_percentage"] = (totalIdleTime / (Time.time - puzzleStartTime)) * 100f,
                ["interaction_frequency"] = puzzleInteractions.Count / (Time.time - puzzleStartTime),
                ["mouse_movement_total"] = interactionTimings.GetValueOrDefault("mouse_movement", 0f),
                ["average_time_between_interactions"] = CalculateAverageInteractionInterval()
            };
            
            return metrics;
        }
        
        private float CalculateEfficiencyScore(bool success, float time, int hints, int attempts)
        {
            float baseScore = success ? 100f : 0f;
            
            // Deduct points for time taken (normalized to expected time)
            float expectedTime = GetExpectedTime(currentPuzzleType);
            float timeEfficiency = Mathf.Clamp01(expectedTime / time);
            
            // Deduct points for hints and attempts
            float hintPenalty = hints * 10f;
            float attemptPenalty = (attempts - 1) * 5f;
            
            return Mathf.Max(0f, baseScore * timeEfficiency - hintPenalty - attemptPenalty);
        }
        
        private void AnalyzePuzzleInteractionPatterns()
        {
            if (puzzleInteractions.Count < 5 || !learningTracker) return;
            
            // Analyze interaction rhythm and patterns
            var timings = puzzleInteractions.Select(i => i.timestamp).ToList();
            
            // Calculate interaction consistency
            List<float> intervals = new List<float>();
            for (int i = 1; i < timings.Count; i++)
            {
                intervals.Add(timings[i] - timings[i-1]);
            }
            
            float averageInterval = intervals.Average();
            float variance = intervals.Select(x => Mathf.Pow(x - averageInterval, 2)).Average();
            
            // Analyze interaction patterns for learning insights
            bool hasConsistentRhythm = variance < 2f;
            bool hasRapidInteractions = averageInterval < 1f;
            bool hasDeliberateApproach = averageInterval > 3f;
            
            learningTracker.LogDetailedEvent("puzzle_interaction_analysis", "Detailed interaction pattern analysis", "behavioral_analysis",
                new Dictionary<string, object>
                {
                    {"consistent_rhythm", hasConsistentRhythm},
                    {"rapid_interactions", hasRapidInteractions},
                    {"deliberate_approach", hasDeliberateApproach},
                    {"interaction_variance", variance},
                    {"average_interval", averageInterval},
                    {"total_pattern_changes", CountPatternChanges()}
                });
            
            // Update learning style preferences based on interaction patterns
            if (hasConsistentRhythm && hasDeliberateApproach)
            {
                // Suggests methodical, logical approach
                learningTracker.LogDetailedEvent("methodical_approach_detected", "Player shows systematic problem-solving", "cognitive_style");
            }
            else if (hasRapidInteractions)
            {
                // Suggests kinesthetic or trial-and-error approach
                learningTracker.LogDetailedEvent("active_exploration_detected", "Player shows hands-on approach", "cognitive_style");
            }
        }
        
        private float CalculateAverageInteractionInterval()
        {
            if (puzzleInteractions.Count < 2) return 0f;
            
            var timings = puzzleInteractions.Select(i => i.timestamp).ToList();
            float totalTime = timings.Last() - timings.First();
            return totalTime / (timings.Count - 1);
        }
        
        private int CountPatternChanges()
        {
            // Analyze how often the player changes their interaction approach
            // This is a simplified implementation
            if (puzzleInteractions.Count < 10) return 0;
            
            int changes = 0;
            string lastPatternType = puzzleInteractions[0].interactionType;
            
            for (int i = 1; i < puzzleInteractions.Count; i++)
            {
                if (puzzleInteractions[i].interactionType != lastPatternType)
                {
                    changes++;
                    lastPatternType = puzzleInteractions[i].interactionType;
                }
            }
            
            return changes;
        }
        
        private void ShowCompletionPanel(bool success, PuzzleResults results)
        {
            if (completionPanel)
            {
                completionPanel.SetActive(true);
            }
            
            if (completionMessage)
            {
                if (success)
                {
                    completionMessage.text = $"Puzzle Solved!\n" +
                                           $"Time: {results.completionTime:F1}s\n" +
                                           $"Attempts: {results.attempts}\n" +
                                           $"Learning Style: {GetPrimaryLearningStyle(currentPuzzleType)}";
                }
                else
                {
                    completionMessage.text = "Puzzle incomplete. Try again later!";
                }
            }
        }
        
        private IEnumerator AutoClosePuzzle(float delay)
        {
            yield return new WaitForSeconds(delay);
            ClosePuzzleUI();
        }
        
        public void ExitCurrentPuzzle()
        {
            if (!puzzleActive) return;
            
            // Record as incomplete
            CompletePuzzle(false);
            
            OnPuzzleExited?.Invoke(currentPuzzleType);
        }
        
        private void ClosePuzzleUI()
        {
            // Hide completion panel
            if (completionPanel)
            {
                completionPanel.SetActive(false);
            }
            
            // Destroy current puzzle instance
            if (currentPuzzleInstance)
            {
                Destroy(currentPuzzleInstance);
                currentPuzzleInstance = null;
            }
            
            // Hide puzzle canvas
            if (puzzleOverlayCanvas)
            {
                puzzleOverlayCanvas.gameObject.SetActive(false);
            }
            
            currentPuzzleType = "";
        }
        
        private void ShowHint()
        {
            if (!puzzleActive) return;
            
            hintCount++;
            
            // Play hint sound
            if (puzzleAudioSource && hintSound)
            {
                puzzleAudioSource.PlayOneShot(hintSound);
            }
            
            // Get puzzle-specific hint
            string hint = GetPuzzleHint(currentPuzzleType, hintCount);
            
            // Show hint UI (could be a temporary popup)
            StartCoroutine(ShowHintMessage(hint));
            
            // Track hint usage
            if (learningTracker)
            {
                // Simply track that a hint was used with the count
                learningTracker.LogDetailedEvent("hint_used", 
                    $"Used hint #{hintCount} for {currentPuzzleType}", "puzzle",
                    new Dictionary<string, object>
                    {
                        {"puzzle_type", currentPuzzleType},
                        {"hint_number", hintCount},
                        {"time_since_start", Time.time - puzzleStartTime}
                    });
            }
        }
        
        private string GetPuzzleHint(string puzzleType, int hintNumber)
        {
            switch (puzzleType.ToLower())
            {
                case "chronocircuits":
                    switch (hintNumber)
                    {
                        case 1: return "Water flows from high to low. Start with the source.";
                        case 2: return "Connect adjacent nodes to create a continuous path.";
                        case 3: return "Some paths may need to cross over others.";
                        default: return "Think about how ancient irrigation systems worked.";
                    }
                    
                case "scrollofsecrets":
                    switch (hintNumber)
                    {
                        case 1: return "Context clues in surrounding text can help identify missing words.";
                        case 2: return "Consider the grammatical structure of ancient Egyptian writing.";
                        case 3: return "Some symbols represent sounds, others represent concepts.";
                        default: return "Think about what makes sense in this historical context.";
                    }
                    
                case "pyramidrebuilder":
                    switch (hintNumber)
                    {
                        case 1: return "Start with corner and edge pieces to establish the border.";
                        case 2: return "Look for matching colors and patterns between adjacent pieces.";
                        case 3: return "The mural tells a story - pieces should flow logically.";
                        default: return "Ancient art often had symbolic meaning and order.";
                    }
                    
                default:
                    return "Consider how this puzzle relates to ancient knowledge.";
            }
        }
        
        private IEnumerator ShowHintMessage(string hint)
        {
            // Create temporary hint display
            var hintObject = new GameObject("HintDisplay");
            hintObject.transform.SetParent(puzzleOverlayCanvas.transform);
            
            var canvas = hintObject.AddComponent<Canvas>();
            var text = hintObject.AddComponent<TextMeshProUGUI>();
            text.text = hint;
            text.fontSize = 24;
            text.color = Color.yellow;
            text.alignment = TextAlignmentOptions.Center;
            
            var rect = hintObject.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.2f, 0.8f);
            rect.anchorMax = new Vector2(0.8f, 0.9f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            
            // Fade in
            float alpha = 0f;
            while (alpha < 1f)
            {
                alpha += Time.deltaTime * 2f;
                text.color = new Color(text.color.r, text.color.g, text.color.b, alpha);
                yield return null;
            }
            
            yield return new WaitForSeconds(3f);
            
            // Fade out
            while (alpha > 0f)
            {
                alpha -= Time.deltaTime * 2f;
                text.color = new Color(text.color.r, text.color.g, text.color.b, alpha);
                yield return null;
            }
            
            Destroy(hintObject);
        }
        
        private IEnumerator PuzzleTimer()
        {
            while (puzzleActive)
            {
                if (timerText)
                {
                    float elapsedTime = Time.time - puzzleStartTime;
                    timerText.text = $"Time: {elapsedTime:F1}s";
                }
                yield return new WaitForSeconds(0.1f);
            }
        }
        
        private List<string> GetLearningStyleTags(string puzzleType)
        {
            switch (puzzleType.ToLower())
            {
                case "chronocircuits":
                    return new List<string> { "logical-mathematical", "visual-spatial", "systematic" };
                case "scrollofsecrets":
                    return new List<string> { "verbal-linguistic", "reading-writing", "analytical" };
                case "pyramidrebuilder":
                    return new List<string> { "visual-spatial", "kinesthetic", "pattern-recognition" };
                default:
                    return new List<string> { "general" };
            }
        }
        
        private string GetPrimaryLearningStyle(string puzzleType)
        {
            switch (puzzleType.ToLower())
            {
                case "chronocircuits": return "Logical-Mathematical";
                case "scrollofsecrets": return "Verbal-Linguistic";
                case "pyramidrebuilder": return "Visual-Spatial";
                default: return "Mixed";
            }
        }
        
        private void OnDestroy()
        {
            if (exitPuzzleButton)
            {
                exitPuzzleButton.onClick.RemoveAllListeners();
            }
            
            if (hintButton)
            {
                hintButton.onClick.RemoveAllListeners();
            }
        }
    }
}