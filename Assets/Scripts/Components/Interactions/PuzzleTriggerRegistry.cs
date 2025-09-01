using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using CuriousCity.Core;

namespace CuriousCity.Core
{
    /// <summary>
    /// Central registry for managing all puzzle triggers in the scene
    /// Provides easy access and management without manual GameObject references
    /// </summary>
    public class PuzzleTriggerRegistry : MonoBehaviour
    {
        [Header("Registry Configuration")]
        public bool autoRegister = true;
        public bool autoConnectToMissionManager = true;
        public bool enableDebugLogging = true;
        
        [Header("Mission Integration")]
        public HistoricalMissionSceneManager missionManager;
        public PuzzleManager puzzleManager;
        
        // Registry data
        private Dictionary<string, PuzzleTriggerInteractable> puzzleTriggers = new Dictionary<string, PuzzleTriggerInteractable>();
        private Dictionary<string, List<PuzzleTriggerInteractable>> triggersByType = new Dictionary<string, List<PuzzleTriggerInteractable>>();
        private Dictionary<string, GameObject> triggerGameObjects = new Dictionary<string, GameObject>();
        
        // Events
        public static System.Action<PuzzleTriggerInteractable> OnPuzzleTriggerRegistered;
        public static System.Action<PuzzleTriggerInteractable> OnPuzzleTriggerUnregistered;
        public static System.Action<string> OnPuzzleTypeCompleted;
        
        private void Start()
        {
            if (autoRegister)
            {
                RegisterExistingTriggers();
            }
            
            if (autoConnectToMissionManager)
            {
                ConnectToMissionManager();
            }
        }
        
        /// <summary>
        /// Registers an existing puzzle trigger
        /// </summary>
        public void RegisterPuzzleTrigger(PuzzleTriggerInteractable trigger)
        {
            if (trigger == null) return;
            
            string triggerId = GetTriggerId(trigger);
            
            if (puzzleTriggers.ContainsKey(triggerId))
            {
                Debug.LogWarning($"[PuzzleTriggerRegistry] Trigger {triggerId} already registered!");
                return;
            }
            
            // Register in main dictionary
            puzzleTriggers[triggerId] = trigger;
            triggerGameObjects[triggerId] = trigger.gameObject;
            
            // Register by puzzle type
            string puzzleType = trigger.puzzleType.ToLower();
            if (!triggersByType.ContainsKey(puzzleType))
            {
                triggersByType[puzzleType] = new List<PuzzleTriggerInteractable>();
            }
            triggersByType[puzzleType].Add(trigger);
            
            // Set up completion callback
            trigger.OnPuzzleCompleted += () => OnPuzzleCompleted(trigger);
            
            if (enableDebugLogging)
            {
                Debug.Log($"[PuzzleTriggerRegistry] Registered puzzle trigger: {triggerId} ({trigger.puzzleType})");
            }
            
            OnPuzzleTriggerRegistered?.Invoke(trigger);
        }
        
        /// <summary>
        /// Unregisters a puzzle trigger
        /// </summary>
        public void UnregisterPuzzleTrigger(PuzzleTriggerInteractable trigger)
        {
            if (trigger == null) return;
            
            string triggerId = GetTriggerId(trigger);
            
            if (puzzleTriggers.Remove(triggerId))
            {
                triggerGameObjects.Remove(triggerId);
                
                // Remove from type dictionary
                string puzzleType = trigger.puzzleType.ToLower();
                if (triggersByType.ContainsKey(puzzleType))
                {
                    triggersByType[puzzleType].Remove(trigger);
                    if (triggersByType[puzzleType].Count == 0)
                    {
                        triggersByType.Remove(puzzleType);
                    }
                }
                
                if (enableDebugLogging)
                {
                    Debug.Log($"[PuzzleTriggerRegistry] Unregistered puzzle trigger: {triggerId}");
                }
                
                OnPuzzleTriggerUnregistered?.Invoke(trigger);
            }
        }
        
        /// <summary>
        /// Registers all existing puzzle triggers in the scene
        /// </summary>
        [ContextMenu("Register Existing Triggers")]
        public void RegisterExistingTriggers()
        {
            var existingTriggers = FindObjectsByType<PuzzleTriggerInteractable>(FindObjectsSortMode.None);
            
            foreach (var trigger in existingTriggers)
            {
                RegisterPuzzleTrigger(trigger);
            }
            
            Debug.Log($"[PuzzleTriggerRegistry] Registered {existingTriggers.Length} existing puzzle triggers");
        }
        
        /// <summary>
        /// Connects to the mission manager and puzzle manager
        /// </summary>
        private void ConnectToMissionManager()
        {
            if (missionManager == null)
            {
                missionManager = FindFirstObjectByType<HistoricalMissionSceneManager>();
            }
            
            if (puzzleManager == null)
            {
                puzzleManager = FindFirstObjectByType<PuzzleManager>();
            }
            
            if (missionManager != null && puzzleManager != null)
            {
                // Ensure puzzle manager is initialized with mission manager
                puzzleManager.Initialize(missionManager);
                Debug.Log("[PuzzleTriggerRegistry] Connected to mission manager and puzzle manager");
            }
            else
            {
                Debug.LogWarning("[PuzzleTriggerRegistry] Could not find mission manager or puzzle manager");
            }
        }
        
        /// <summary>
        /// Gets a puzzle trigger by ID
        /// </summary>
        public PuzzleTriggerInteractable GetPuzzleTrigger(string triggerId)
        {
            puzzleTriggers.TryGetValue(triggerId, out var trigger);
            return trigger;
        }
        
        /// <summary>
        /// Gets all puzzle triggers of a specific type
        /// </summary>
        public List<PuzzleTriggerInteractable> GetPuzzleTriggersByType(string puzzleType)
        {
            string typeKey = puzzleType.ToLower();
            if (triggersByType.TryGetValue(typeKey, out var triggers))
            {
                return new List<PuzzleTriggerInteractable>(triggers);
            }
            return new List<PuzzleTriggerInteractable>();
        }
        
        /// <summary>
        /// Gets all puzzle trigger IDs
        /// </summary>
        public List<string> GetAllTriggerIds()
        {
            return new List<string>(puzzleTriggers.Keys);
        }
        
        /// <summary>
        /// Gets all puzzle types
        /// </summary>
        public List<string> GetAllPuzzleTypes()
        {
            return new List<string>(triggersByType.Keys);
        }
        
        /// <summary>
        /// Checks if a puzzle type is completed
        /// </summary>
        public bool IsPuzzleTypeCompleted(string puzzleType)
        {
            var triggers = GetPuzzleTriggersByType(puzzleType);
            return triggers.All(t => t.puzzleCompleted);
        }
        
        /// <summary>
        /// Gets completion status for all puzzle types
        /// </summary>
        public Dictionary<string, bool> GetPuzzleTypeCompletionStatus()
        {
            var status = new Dictionary<string, bool>();
            
            foreach (var puzzleType in triggersByType.Keys)
            {
                status[puzzleType] = IsPuzzleTypeCompleted(puzzleType);
            }
            
            return status;
        }
        
        /// <summary>
        /// Resets all puzzle triggers
        /// </summary>
        [ContextMenu("Reset All Puzzles")]
        public void ResetAllPuzzles()
        {
            foreach (var trigger in puzzleTriggers.Values)
            {
                if (trigger != null)
                {
                    trigger.ResetPuzzle();
                }
            }
            
            Debug.Log("[PuzzleTriggerRegistry] Reset all puzzle triggers");
        }
        
        /// <summary>
        /// Resets puzzles of a specific type
        /// </summary>
        public void ResetPuzzleType(string puzzleType)
        {
            var triggers = GetPuzzleTriggersByType(puzzleType);
            
            foreach (var trigger in triggers)
            {
                trigger.ResetPuzzle();
            }
            
            Debug.Log($"[PuzzleTriggerRegistry] Reset {triggers.Count} puzzles of type: {puzzleType}");
        }
        
        /// <summary>
        /// Gets statistics about puzzle triggers
        /// </summary>
        public Dictionary<string, object> GetRegistryStats()
        {
            var stats = new Dictionary<string, object>
            {
                ["total_triggers"] = puzzleTriggers.Count,
                ["puzzle_types"] = triggersByType.Count,
                ["completed_triggers"] = puzzleTriggers.Values.Count(t => t.puzzleCompleted),
                ["active_triggers"] = puzzleTriggers.Values.Count(t => !t.puzzleCompleted),
                ["mission_manager_connected"] = missionManager != null,
                ["puzzle_manager_connected"] = puzzleManager != null
            };
            
            // Add completion status for each puzzle type
            foreach (var puzzleType in triggersByType.Keys)
            {
                var triggers = triggersByType[puzzleType];
                stats[$"type_{puzzleType}_total"] = triggers.Count;
                stats[$"type_{puzzleType}_completed"] = triggers.Count(t => t.puzzleCompleted);
            }
            
            return stats;
        }
        
        /// <summary>
        /// Called when a puzzle is completed
        /// </summary>
        private void OnPuzzleCompleted(PuzzleTriggerInteractable trigger)
        {
            string puzzleType = trigger.puzzleType;
            
            if (enableDebugLogging)
            {
                Debug.Log($"[PuzzleTriggerRegistry] Puzzle completed: {puzzleType} by {GetTriggerId(trigger)}");
            }
            
            // Check if all puzzles of this type are completed
            if (IsPuzzleTypeCompleted(puzzleType))
            {
                OnPuzzleTypeCompleted?.Invoke(puzzleType);
                
                if (enableDebugLogging)
                {
                    Debug.Log($"[PuzzleTriggerRegistry] All puzzles of type {puzzleType} are now completed!");
                }
            }
        }
        
        /// <summary>
        /// Generates a unique ID for a trigger
        /// </summary>
        private string GetTriggerId(PuzzleTriggerInteractable trigger)
        {
            return $"{trigger.puzzleType}_{trigger.gameObject.name}_{trigger.GetInstanceID()}";
        }
        
        /// <summary>
        /// Logs registry status
        /// </summary>
        [ContextMenu("Log Registry Status")]
        public void LogRegistryStatus()
        {
            var stats = GetRegistryStats();
            
            Debug.Log("=== Puzzle Trigger Registry Status ===");
            foreach (var stat in stats)
            {
                Debug.Log($"{stat.Key}: {stat.Value}");
            }
            Debug.Log("=====================================");
        }
        
        /// <summary>
        /// Finds and registers puzzle triggers in children
        /// </summary>
        [ContextMenu("Register Children Triggers")]
        public void RegisterChildrenTriggers()
        {
            var childTriggers = GetComponentsInChildren<PuzzleTriggerInteractable>();
            
            foreach (var trigger in childTriggers)
            {
                RegisterPuzzleTrigger(trigger);
            }
            
            Debug.Log($"[PuzzleTriggerRegistry] Registered {childTriggers.Length} child puzzle triggers");
        }
        
        private void OnDestroy()
        {
            // Clean up event subscriptions
            foreach (var trigger in puzzleTriggers.Values)
            {
                if (trigger != null)
                {
                    trigger.OnPuzzleCompleted -= () => OnPuzzleCompleted(trigger);
                }
            }
        }
        
        private void OnDrawGizmosSelected()
        {
            // Draw registry bounds
            if (puzzleTriggers.Count > 0)
            {
                var positions = puzzleTriggers.Values
                    .Where(t => t != null)
                    .Select(t => t.transform.position)
                    .ToList();
                
                if (positions.Count > 0)
                {
                    var center = positions.Aggregate(Vector3.zero, (acc, pos) => acc + pos) / positions.Count;
                    var bounds = new Bounds(center, Vector3.zero);
                    
                    foreach (var pos in positions)
                    {
                        bounds.Encapsulate(pos);
                    }
                    
                    Gizmos.color = Color.cyan;
                    Gizmos.DrawWireCube(bounds.center, bounds.size);
                    
                    // Draw individual triggers
                    Gizmos.color = Color.yellow;
                    foreach (var pos in positions)
                    {
                        Gizmos.DrawWireSphere(pos, 0.3f);
                    }
                }
            }
        }
    }
}
