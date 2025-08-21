using UnityEngine;
using CuriousCity.Gameplay.Puzzles;

namespace CuriousCity.Core
{
    /// <summary>
    /// Debug component to help troubleshoot puzzle trigger interactions
    /// </summary>
    public class PuzzleTriggerDebugger : MonoBehaviour
    {
        [Header("Debug Settings")]
        public bool enableDebugLogging = true;
        public bool showInteractionRange = true;
        public bool logPuzzleManagerStatus = true;
        
        private PuzzleTriggerInteractable puzzleTrigger;
        private HistoricalMissionSceneManager missionManager;
        private PuzzleManager puzzleManager;
        
        private void Start()
        {
            // Find puzzle trigger on this object
            puzzleTrigger = GetComponent<PuzzleTriggerInteractable>();
            if (puzzleTrigger == null)
            {
                Debug.LogError("[PuzzleTriggerDebugger] No PuzzleTriggerInteractable found on this object!");
                return;
            }
            
            // Find mission manager
            missionManager = FindFirstObjectByType<HistoricalMissionSceneManager>();
            if (missionManager == null)
            {
                Debug.LogWarning("[PuzzleTriggerDebugger] No HistoricalMissionSceneManager found in scene!");
            }
            
            // Find puzzle manager
            puzzleManager = FindFirstObjectByType<PuzzleManager>();
            if (puzzleManager == null)
            {
                Debug.LogWarning("[PuzzleTriggerDebugger] No PuzzleManager found in scene!");
            }
            
            if (enableDebugLogging)
            {
                LogInitialStatus();
            }
        }
        
        private void LogInitialStatus()
        {
            Debug.Log($"[PuzzleTriggerDebugger] Initializing debug for {gameObject.name}");
            Debug.Log($"[PuzzleTriggerDebugger] Puzzle Type: {puzzleTrigger.puzzleType}");
            Debug.Log($"[PuzzleTriggerDebugger] Mission Manager Found: {missionManager != null}");
            Debug.Log($"[PuzzleTriggerDebugger] Puzzle Manager Found: {puzzleManager != null}");
            Debug.Log($"[PuzzleTriggerDebugger] Has Collider: {GetComponent<Collider>() != null}");
            Debug.Log($"[PuzzleTriggerDebugger] Tag: {gameObject.tag}");
        }
        
        private void Update()
        {
            if (!enableDebugLogging) return;
            
            // Check for player proximity
            var player = FindFirstObjectByType<CuriousCityAutomated.Characters.FirstPersonController>();
            if (player != null)
            {
                float distance = Vector3.Distance(transform.position, player.transform.position);
                if (distance < 3f)
                {
                    Debug.Log($"[PuzzleTriggerDebugger] Player within range: {distance:F2}m");
                }
            }
        }
        
        private void OnDrawGizmos()
        {
            if (!showInteractionRange) return;
            
            // Draw interaction range
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, 1.5f);
            
            // Draw raycast direction from player camera
            var player = FindFirstObjectByType<CuriousCityAutomated.Characters.FirstPersonController>();
            if (player != null)
            {
                var camera = player.GetComponentInChildren<Camera>();
                if (camera != null)
                {
                    Gizmos.color = Color.red;
                    Gizmos.DrawRay(camera.transform.position, camera.transform.forward * 5f);
                }
            }
        }
        
        [ContextMenu("Test Puzzle Trigger")]
        public void TestPuzzleTrigger()
        {
            if (puzzleTrigger == null)
            {
                Debug.LogError("[PuzzleTriggerDebugger] No puzzle trigger to test!");
                return;
            }
            
            Debug.Log("[PuzzleTriggerDebugger] Testing puzzle trigger manually...");
            
            // Simulate interaction
            var player = FindFirstObjectByType<CuriousCityAutomated.Characters.FirstPersonController>();
            if (player != null)
            {
                puzzleTrigger.Interact(player);
            }
            else
            {
                Debug.LogWarning("[PuzzleTriggerDebugger] No player found to test with!");
            }
        }
        
        [ContextMenu("Check System Status")]
        public void CheckSystemStatus()
        {
            Debug.Log("=== Puzzle Trigger System Status ===");
            Debug.Log($"Puzzle Trigger: {(puzzleTrigger != null ? "Found" : "Missing")}");
            Debug.Log($"Mission Manager: {(missionManager != null ? "Found" : "Missing")}");
            Debug.Log($"Puzzle Manager: {(puzzleManager != null ? "Found" : "Missing")}");
            Debug.Log($"Collider: {(GetComponent<Collider>() != null ? "Found" : "Missing")}");
            Debug.Log($"Tag: {gameObject.tag}");
            Debug.Log($"Layer: {gameObject.layer}");
            
            if (puzzleTrigger != null)
            {
                Debug.Log($"Puzzle Type: {puzzleTrigger.puzzleType}");
                Debug.Log($"Can Interact: {puzzleTrigger.CanInteract()}");
                Debug.Log($"Is Enabled: {puzzleTrigger.isEnabled}");
            }
            
            Debug.Log("================================");
        }
    }
}
