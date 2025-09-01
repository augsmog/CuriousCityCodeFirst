using UnityEngine;
using System.Collections.Generic;
using CuriousCity.Core;

namespace CuriousCity.Core
{
    /// <summary>
    /// Master manager that coordinates all puzzle trigger systems
    /// Single point of control for the entire puzzle trigger ecosystem
    /// </summary>
    public class PuzzleTriggerMasterManager : MonoBehaviour
    {
        [Header("System Configuration")]
        public bool enableOnStart = true;
        public bool autoSetupSystems = true;
        public bool enableDebugMode = true;
        
        [Header("System References")]
        public PuzzleTriggerSpawner spawner;
        public PuzzleTriggerRegistry registry;
        public HistoricalMissionSceneManager missionManager;
        public PuzzleManager puzzleManager;
        
        [Header("Asset Management")]
        public List<PuzzleTriggerAsset> defaultAssets = new List<PuzzleTriggerAsset>();
        public string assetFolderPath = "Assets/PuzzleTriggers";
        public bool loadAssetsFromFolder = true;
        
        [Header("Spawn Configuration")]
        public Transform spawnParent;
        public List<Transform> spawnPoints = new List<Transform>();
        public float spawnRadius = 15f;
        public bool useRandomSpawning = true;
        
        // Runtime state
        private bool systemsInitialized = false;
        private List<GameObject> managedTriggers = new List<GameObject>();
        
        private void Start()
        {
            if (enableOnStart)
            {
                InitializeSystems();
            }
        }
        
        /// <summary>
        /// Initializes all puzzle trigger systems
        /// </summary>
        [ContextMenu("Initialize Systems")]
        public void InitializeSystems()
        {
            if (systemsInitialized)
            {
                Debug.LogWarning("[PuzzleTriggerMasterManager] Systems already initialized!");
                return;
            }
            
            Debug.Log("[PuzzleTriggerMasterManager] Initializing puzzle trigger systems...");
            
            // Auto-setup systems if enabled
            if (autoSetupSystems)
            {
                SetupSystems();
            }
            
            // Initialize spawner
            if (spawner != null)
            {
                InitializeSpawner();
            }
            
            // Initialize registry
            if (registry != null)
            {
                InitializeRegistry();
            }
            
            // Load and spawn default assets
            if (defaultAssets.Count > 0 || loadAssetsFromFolder)
            {
                LoadAndSpawnAssets();
            }
            
            systemsInitialized = true;
            Debug.Log("[PuzzleTriggerMasterManager] All systems initialized successfully!");
        }
        
        /// <summary>
        /// Sets up all required systems automatically
        /// </summary>
        private void SetupSystems()
        {
            // Find or create mission manager
            if (missionManager == null)
            {
                missionManager = FindFirstObjectByType<HistoricalMissionSceneManager>();
                if (missionManager == null)
                {
                    Debug.Log("[PuzzleTriggerMasterManager] Creating HistoricalMissionSceneManager...");
                    var missionManagerObj = new GameObject("HistoricalMissionSceneManager");
                    missionManager = missionManagerObj.AddComponent<HistoricalMissionSceneManager>();
                }
            }
            
            // Find or create puzzle manager
            if (puzzleManager == null)
            {
                puzzleManager = FindFirstObjectByType<PuzzleManager>();
                if (puzzleManager == null)
                {
                    Debug.Log("[PuzzleTriggerMasterManager] Creating PuzzleManager...");
                    var puzzleManagerObj = new GameObject("PuzzleManager");
                    puzzleManager = puzzleManagerObj.AddComponent<PuzzleManager>();
                }
            }
            
            // Find or create spawner
            if (spawner == null)
            {
                spawner = FindFirstObjectByType<PuzzleTriggerSpawner>();
                if (spawner == null)
                {
                    Debug.Log("[PuzzleTriggerMasterManager] Creating PuzzleTriggerSpawner...");
                    var spawnerObj = new GameObject("PuzzleTriggerSpawner");
                    spawner = spawnerObj.AddComponent<PuzzleTriggerSpawner>();
                    spawner.transform.SetParent(transform);
                }
            }
            
            // Find or create registry
            if (registry == null)
            {
                registry = FindFirstObjectByType<PuzzleTriggerRegistry>();
                if (registry == null)
                {
                    Debug.Log("[PuzzleTriggerMasterManager] Creating PuzzleTriggerRegistry...");
                    var registryObj = new GameObject("PuzzleTriggerRegistry");
                    registry = registryObj.AddComponent<PuzzleTriggerRegistry>();
                    registry.transform.SetParent(transform);
                }
            }
            
            // Set up spawn parent if not specified
            if (spawnParent == null)
            {
                spawnParent = transform;
            }
        }
        
        /// <summary>
        /// Initializes the spawner system
        /// </summary>
        private void InitializeSpawner()
        {
            spawner.spawnParent = spawnParent;
            spawner.spawnRadius = spawnRadius;
            spawner.useRandomSpawnPoints = useRandomSpawning;
            
            // Copy spawn points
            spawner.spawnPoints.Clear();
            foreach (var spawnPoint in spawnPoints)
            {
                if (spawnPoint != null)
                {
                    spawner.spawnPoints.Add(spawnPoint);
                }
            }
            
            // Set asset folder path
            spawner.assetFolderPath = assetFolderPath;
            spawner.autoSpawnFromFolder = loadAssetsFromFolder;
            
            Debug.Log("[PuzzleTriggerMasterManager] Spawner initialized");
        }
        
        /// <summary>
        /// Initializes the registry system
        /// </summary>
        private void InitializeRegistry()
        {
            registry.missionManager = missionManager;
            registry.puzzleManager = puzzleManager;
            registry.enableDebugLogging = enableDebugMode;
            
            Debug.Log("[PuzzleTriggerMasterManager] Registry initialized");
        }
        
        /// <summary>
        /// Loads and spawns puzzle trigger assets
        /// </summary>
        [ContextMenu("Load and Spawn Assets")]
        public void LoadAndSpawnAssets()
        {
            if (spawner == null)
            {
                Debug.LogError("[PuzzleTriggerMasterManager] Spawner not available!");
                return;
            }
            
            // Add default assets to spawner
            foreach (var asset in defaultAssets)
            {
                if (asset != null && !spawner.puzzleTriggerAssets.Contains(asset))
                {
                    spawner.puzzleTriggerAssets.Add(asset);
                }
            }
            
            // Trigger spawner to initialize and spawn
            spawner.InitializeAndSpawn();
            
            // Get spawned triggers
            managedTriggers = spawner.GetSpawnedTriggers();
            
            Debug.Log($"[PuzzleTriggerMasterManager] Loaded and spawned {managedTriggers.Count} puzzle triggers");
        }
        
        /// <summary>
        /// Spawns a specific puzzle trigger by type
        /// </summary>
        public GameObject SpawnPuzzleTrigger(string puzzleType, Vector3? position = null)
        {
            if (spawner == null)
            {
                Debug.LogError("[PuzzleTriggerMasterManager] Spawner not available!");
                return null;
            }
            
            GameObject trigger = spawner.SpawnTriggerByType(puzzleType, position);
            if (trigger != null)
            {
                managedTriggers.Add(trigger);
            }
            
            return trigger;
        }
        
        /// <summary>
        /// Gets all managed puzzle triggers
        /// </summary>
        public List<GameObject> GetManagedTriggers()
        {
            // Clean up null references
            managedTriggers.RemoveAll(t => t == null);
            return new List<GameObject>(managedTriggers);
        }
        
        /// <summary>
        /// Gets puzzle triggers by type
        /// </summary>
        public List<GameObject> GetTriggersByType(string puzzleType)
        {
            if (registry == null) return new List<GameObject>();
            
            var triggers = registry.GetPuzzleTriggersByType(puzzleType);
            return triggers.ConvertAll(t => t.gameObject);
        }
        
        /// <summary>
        /// Gets completion status for all puzzle types
        /// </summary>
        public Dictionary<string, bool> GetPuzzleCompletionStatus()
        {
            if (registry == null) return new Dictionary<string, bool>();
            
            return registry.GetPuzzleTypeCompletionStatus();
        }
        
        /// <summary>
        /// Resets all puzzle triggers
        /// </summary>
        [ContextMenu("Reset All Puzzles")]
        public void ResetAllPuzzles()
        {
            if (registry == null)
            {
                Debug.LogError("[PuzzleTriggerMasterManager] Registry not available!");
                return;
            }
            
            registry.ResetAllPuzzles();
        }
        
        /// <summary>
        /// Resets puzzles of a specific type
        /// </summary>
        public void ResetPuzzleType(string puzzleType)
        {
            if (registry == null)
            {
                Debug.LogError("[PuzzleTriggerMasterManager] Registry not available!");
                return;
            }
            
            registry.ResetPuzzleType(puzzleType);
        }
        
        /// <summary>
        /// Adds a spawn point
        /// </summary>
        [ContextMenu("Add Spawn Point Here")]
        public void AddSpawnPointHere()
        {
            var spawnPoint = new GameObject($"SpawnPoint_{spawnPoints.Count}");
            spawnPoint.transform.position = transform.position;
            spawnPoint.transform.SetParent(transform);
            spawnPoints.Add(spawnPoint.transform);
            
            if (spawner != null)
            {
                spawner.AddSpawnPoint(transform.position);
            }
            
            Debug.Log($"[PuzzleTriggerMasterManager] Added spawn point at {transform.position}");
        }
        
        /// <summary>
        /// Gets system status information
        /// </summary>
        [ContextMenu("Log System Status")]
        public void LogSystemStatus()
        {
            Debug.Log("=== Puzzle Trigger Master Manager Status ===");
            Debug.Log($"Systems Initialized: {systemsInitialized}");
            Debug.Log($"Spawner Available: {spawner != null}");
            Debug.Log($"Registry Available: {registry != null}");
            Debug.Log($"Mission Manager Available: {missionManager != null}");
            Debug.Log($"Puzzle Manager Available: {puzzleManager != null}");
            Debug.Log($"Managed Triggers: {GetManagedTriggers().Count}");
            
            if (registry != null)
            {
                var stats = registry.GetRegistryStats();
                foreach (var stat in stats)
                {
                    Debug.Log($"{stat.Key}: {stat.Value}");
                }
            }
            
            Debug.Log("===========================================");
        }
        
        /// <summary>
        /// Cleans up all systems
        /// </summary>
        [ContextMenu("Cleanup All Systems")]
        public void CleanupAllSystems()
        {
            if (spawner != null)
            {
                spawner.ClearSpawnedTriggers();
            }
            
            managedTriggers.Clear();
            systemsInitialized = false;
            
            Debug.Log("[PuzzleTriggerMasterManager] All systems cleaned up");
        }
        
        /// <summary>
        /// Refreshes the entire system
        /// </summary>
        [ContextMenu("Refresh System")]
        public void RefreshSystem()
        {
            CleanupAllSystems();
            InitializeSystems();
        }
        
        private void OnDrawGizmosSelected()
        {
            // Draw spawn radius
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, spawnRadius);
            
            // Draw spawn points
            Gizmos.color = Color.yellow;
            foreach (var spawnPoint in spawnPoints)
            {
                if (spawnPoint != null)
                {
                    Gizmos.DrawWireSphere(spawnPoint.position, 0.5f);
                    Gizmos.DrawLine(transform.position, spawnPoint.position);
                }
            }
            
            // Draw managed triggers
            Gizmos.color = Color.blue;
            foreach (var trigger in managedTriggers)
            {
                if (trigger != null)
                {
                    Gizmos.DrawWireCube(trigger.transform.position, Vector3.one);
                }
            }
        }
        
        private void OnDestroy()
        {
            CleanupAllSystems();
        }
    }
}
