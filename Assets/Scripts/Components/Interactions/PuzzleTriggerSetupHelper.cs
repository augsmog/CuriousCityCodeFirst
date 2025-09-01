using UnityEngine;
using CuriousCity.Core;

namespace CuriousCity.Core
{
    /// <summary>
    /// Helper component to ensure puzzle triggers are properly set up in the scene
    /// </summary>
    public class PuzzleTriggerSetupHelper : MonoBehaviour
    {
        [Header("Setup Configuration")]
        public bool autoSetupOnStart = true;
        public bool createMissingComponents = true;
        public bool validateSceneSetup = true;
        
        [Header("Required Components")]
        public HistoricalMissionSceneManager missionManager;
        public PuzzleManager puzzleManager;
        public Canvas puzzleOverlayCanvas;
        
        private void Start()
        {
            if (autoSetupOnStart)
            {
                SetupPuzzleSystem();
            }
        }
        
        [ContextMenu("Setup Puzzle System")]
        public void SetupPuzzleSystem()
        {
            Debug.Log("[PuzzleTriggerSetupHelper] Setting up puzzle system...");
            
            // Find or create mission manager
            if (missionManager == null)
            {
                missionManager = FindFirstObjectByType<HistoricalMissionSceneManager>();
                if (missionManager == null && createMissingComponents)
                {
                    Debug.Log("[PuzzleTriggerSetupHelper] Creating HistoricalMissionSceneManager...");
                    var missionManagerObj = new GameObject("HistoricalMissionSceneManager");
                    missionManager = missionManagerObj.AddComponent<HistoricalMissionSceneManager>();
                }
            }
            
            // Find or create puzzle manager
            if (puzzleManager == null)
            {
                puzzleManager = FindFirstObjectByType<PuzzleManager>();
                if (puzzleManager == null && createMissingComponents)
                {
                    Debug.Log("[PuzzleTriggerSetupHelper] Creating PuzzleManager...");
                    var puzzleManagerObj = new GameObject("PuzzleManager");
                    puzzleManager = puzzleManagerObj.AddComponent<PuzzleManager>();
                }
            }
            
            // Find or create puzzle overlay canvas
            if (puzzleOverlayCanvas == null)
            {
                puzzleOverlayCanvas = FindFirstObjectByType<Canvas>();
                if (puzzleOverlayCanvas == null && createMissingComponents)
                {
                    Debug.Log("[PuzzleTriggerSetupHelper] Creating Puzzle Overlay Canvas...");
                    var canvasObj = new GameObject("PuzzleOverlayCanvas");
                    puzzleOverlayCanvas = canvasObj.AddComponent<Canvas>();
                    puzzleOverlayCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
                    puzzleOverlayCanvas.sortingOrder = 100; // Ensure it's on top
                    
                    // Add CanvasScaler
                    var scaler = canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
                    scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
                    scaler.referenceResolution = new Vector2(1920, 1080);
                    
                    // Add GraphicRaycaster
                    canvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();
                }
            }
            
            // Connect components
            if (missionManager != null && puzzleManager != null)
            {
                // Initialize puzzle manager with mission manager
                puzzleManager.Initialize(missionManager);
                Debug.Log("[PuzzleTriggerSetupHelper] Connected PuzzleManager to MissionManager");
            }
            
            // Validate scene setup
            if (validateSceneSetup)
            {
                ValidateSceneSetup();
            }
            
            Debug.Log("[PuzzleTriggerSetupHelper] Puzzle system setup complete!");
        }
        
        [ContextMenu("Validate Scene Setup")]
        public void ValidateSceneSetup()
        {
            Debug.Log("=== Puzzle System Validation ===");
            
            // Check mission manager
            if (missionManager != null)
            {
                Debug.Log("✓ HistoricalMissionSceneManager: Found");
                if (missionManager.puzzleManager != null)
                {
                    Debug.Log("✓ Mission Manager has PuzzleManager reference");
                }
                else
                {
                    Debug.LogWarning("✗ Mission Manager missing PuzzleManager reference");
                }
            }
            else
            {
                Debug.LogError("✗ HistoricalMissionSceneManager: Missing");
            }
            
            // Check puzzle manager
            if (puzzleManager != null)
            {
                Debug.Log("✓ PuzzleManager: Found");
                if (puzzleManager.puzzleOverlayCanvas != null)
                {
                    Debug.Log("✓ PuzzleManager has overlay canvas");
                }
                else
                {
                    Debug.LogWarning("✗ PuzzleManager missing overlay canvas");
                }
            }
            else
            {
                Debug.LogError("✗ PuzzleManager: Missing");
            }
            
            // Check puzzle overlay canvas
            if (puzzleOverlayCanvas != null)
            {
                Debug.Log("✓ Puzzle Overlay Canvas: Found");
                Debug.Log($"  - Render Mode: {puzzleOverlayCanvas.renderMode}");
                Debug.Log($"  - Sorting Order: {puzzleOverlayCanvas.sortingOrder}");
            }
            else
            {
                Debug.LogError("✗ Puzzle Overlay Canvas: Missing");
            }
            
            // Check puzzle triggers in scene
            var puzzleTriggers = FindObjectsByType<PuzzleTriggerInteractable>(FindObjectsSortMode.None);
            Debug.Log($"✓ Puzzle Triggers in scene: {puzzleTriggers.Length}");
            
            foreach (var trigger in puzzleTriggers)
            {
                Debug.Log($"  - {trigger.name}: {trigger.puzzleType}");
                if (trigger.GetComponent<Collider>() != null)
                {
                    Debug.Log($"    ✓ Has collider");
                }
                else
                {
                    Debug.LogWarning($"    ✗ Missing collider");
                }
                
                if (trigger.gameObject.tag == "Interactable")
                {
                    Debug.Log($"    ✓ Has Interactable tag");
                }
                else
                {
                    Debug.LogWarning($"    ✗ Wrong tag: {trigger.gameObject.tag}");
                }
            }
            
            Debug.Log("================================");
        }
        
        [ContextMenu("Fix Common Issues")]
        public void FixCommonIssues()
        {
            Debug.Log("[PuzzleTriggerSetupHelper] Fixing common issues...");
            
            // Fix puzzle triggers
            var puzzleTriggers = FindObjectsByType<PuzzleTriggerInteractable>(FindObjectsSortMode.None);
            foreach (var trigger in puzzleTriggers)
            {
                // Ensure proper tag
                if (trigger.gameObject.tag != "Interactable")
                {
                    trigger.gameObject.tag = "Interactable";
                    Debug.Log($"[PuzzleTriggerSetupHelper] Fixed tag for {trigger.name}");
                }
                
                // Ensure proper layer
                if (trigger.gameObject.layer == LayerMask.NameToLayer("Ignore Raycast"))
                {
                    trigger.gameObject.layer = LayerMask.NameToLayer("Default");
                    Debug.Log($"[PuzzleTriggerSetupHelper] Fixed layer for {trigger.name}");
                }
                
                // Ensure collider exists
                if (trigger.GetComponent<Collider>() == null)
                {
                    var collider = trigger.gameObject.AddComponent<SphereCollider>();
                    collider.radius = 1.5f;
                    collider.isTrigger = true;
                    Debug.Log($"[PuzzleTriggerSetupHelper] Added collider to {trigger.name}");
                }
            }
            
            Debug.Log("[PuzzleTriggerSetupHelper] Common issues fixed!");
        }
    }
}
