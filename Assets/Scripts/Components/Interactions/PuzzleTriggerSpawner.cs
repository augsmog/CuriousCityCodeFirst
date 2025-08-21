using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace CuriousCity.Core
{
    /// <summary>
    /// Automatically spawns puzzle triggers from assets at runtime
    /// Manages the lifecycle of puzzle triggers without manual GameObject setup
    /// </summary>
    public class PuzzleTriggerSpawner : MonoBehaviour
    {
        [Header("Spawn Configuration")]
        public bool spawnOnStart = true;
        public bool spawnOnDemand = true;
        public bool cleanupOnDestroy = true;
        
        [Header("Asset References")]
        public List<PuzzleTriggerAsset> puzzleTriggerAssets = new List<PuzzleTriggerAsset>();
        public Transform spawnParent;
        
        [Header("Spawn Locations")]
        public List<Transform> spawnPoints = new List<Transform>();
        public bool useRandomSpawnPoints = true;
        public float spawnRadius = 10f;
        public int maxSpawnAttempts = 100;
        
        [Header("Auto-Spawning")]
        public bool autoSpawnFromFolder = true;
        public string assetFolderPath = "Assets/PuzzleTriggers";
        public bool spawnAllInFolder = true;
        
        // Runtime tracking
        private List<GameObject> spawnedTriggers = new List<GameObject>();
        private Dictionary<string, PuzzleTriggerAsset> assetLookup = new Dictionary<string, PuzzleTriggerAsset>();
        
        private void Start()
        {
            if (spawnOnStart)
            {
                InitializeAndSpawn();
            }
        }
        
        /// <summary>
        /// Initializes the spawner and spawns puzzle triggers
        /// </summary>
        [ContextMenu("Initialize and Spawn")]
        public void InitializeAndSpawn()
        {
            Debug.Log("[PuzzleTriggerSpawner] Initializing puzzle trigger spawner...");
            
            // Load assets if auto-loading is enabled
            if (autoSpawnFromFolder)
            {
                LoadAssetsFromFolder();
            }
            
            // Validate assets
            ValidateAssets();
            
            // Create asset lookup
            CreateAssetLookup();
            
            // Spawn triggers
            SpawnAllTriggers();
            
            Debug.Log($"[PuzzleTriggerSpawner] Spawned {spawnedTriggers.Count} puzzle triggers");
        }
        
        /// <summary>
        /// Loads puzzle trigger assets from the specified folder
        /// </summary>
        private void LoadAssetsFromFolder()
        {
            #if UNITY_EDITOR
            if (string.IsNullOrEmpty(assetFolderPath)) return;
            
            var guids = UnityEditor.AssetDatabase.FindAssets("t:PuzzleTriggerAsset", new[] { assetFolderPath });
            puzzleTriggerAssets.Clear();
            
            foreach (var guid in guids)
            {
                var path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
                var asset = UnityEditor.AssetDatabase.LoadAssetAtPath<PuzzleTriggerAsset>(path);
                if (asset != null)
                {
                    puzzleTriggerAssets.Add(asset);
                    Debug.Log($"[PuzzleTriggerSpawner] Loaded asset: {asset.name}");
                }
            }
            #endif
        }
        
        /// <summary>
        /// Validates all puzzle trigger assets
        /// </summary>
        private void ValidateAssets()
        {
            var invalidAssets = new List<PuzzleTriggerAsset>();
            
            foreach (var asset in puzzleTriggerAssets)
            {
                if (asset == null)
                {
                    invalidAssets.Add(asset);
                    continue;
                }
                
                if (!asset.ValidateConfiguration())
                {
                    invalidAssets.Add(asset);
                }
            }
            
            // Remove invalid assets
            foreach (var invalidAsset in invalidAssets)
            {
                puzzleTriggerAssets.Remove(invalidAsset);
                Debug.LogWarning($"[PuzzleTriggerSpawner] Removed invalid asset: {invalidAsset?.name ?? "null"}");
            }
        }
        
        /// <summary>
        /// Creates a lookup dictionary for quick asset access
        /// </summary>
        private void CreateAssetLookup()
        {
            assetLookup.Clear();
            
            foreach (var asset in puzzleTriggerAssets)
            {
                if (asset != null && !string.IsNullOrEmpty(asset.puzzleType))
                {
                    var key = asset.puzzleType.ToLower();
                    if (!assetLookup.ContainsKey(key))
                    {
                        assetLookup[key] = asset;
                    }
                    else
                    {
                        Debug.LogWarning($"[PuzzleTriggerSpawner] Duplicate puzzle type '{asset.puzzleType}' found. Using first occurrence.");
                    }
                }
            }
        }
        
        /// <summary>
        /// Spawns all puzzle triggers
        /// </summary>
        [ContextMenu("Spawn All Triggers")]
        public void SpawnAllTriggers()
        {
            if (puzzleTriggerAssets.Count == 0)
            {
                Debug.LogWarning("[PuzzleTriggerSpawner] No puzzle trigger assets to spawn!");
                return;
            }
            
            // Clear existing triggers if any
            ClearSpawnedTriggers();
            
            // Spawn each trigger
            foreach (var asset in puzzleTriggerAssets)
            {
                if (asset.autoSpawn)
                {
                    SpawnTrigger(asset);
                }
            }
        }
        
        /// <summary>
        /// Spawns a specific puzzle trigger
        /// </summary>
        public GameObject SpawnTrigger(PuzzleTriggerAsset asset, Vector3? customPosition = null)
        {
            if (asset == null)
            {
                Debug.LogError("[PuzzleTriggerSpawner] Cannot spawn null asset!");
                return null;
            }
            
            Vector3 spawnPosition = customPosition ?? GetSpawnPosition();
            
            if (spawnPosition == Vector3.zero)
            {
                Debug.LogWarning($"[PuzzleTriggerSpawner] Could not find valid spawn position for {asset.name}");
                return null;
            }
            
            GameObject triggerObject = asset.CreatePuzzleTrigger(spawnPosition, spawnParent);
            spawnedTriggers.Add(triggerObject);
            
            Debug.Log($"[PuzzleTriggerSpawner] Spawned {asset.name} at {spawnPosition}");
            
            return triggerObject;
        }
        
        /// <summary>
        /// Spawns a trigger by puzzle type
        /// </summary>
        public GameObject SpawnTriggerByType(string puzzleType, Vector3? customPosition = null)
        {
            if (assetLookup.TryGetValue(puzzleType.ToLower(), out var asset))
            {
                return SpawnTrigger(asset, customPosition);
            }
            
            Debug.LogWarning($"[PuzzleTriggerSpawner] No asset found for puzzle type: {puzzleType}");
            return null;
        }
        
        /// <summary>
        /// Gets a spawn position for a trigger
        /// </summary>
        private Vector3 GetSpawnPosition()
        {
            // Use explicit spawn points if available
            if (spawnPoints.Count > 0)
            {
                if (useRandomSpawnPoints)
                {
                    return spawnPoints[Random.Range(0, spawnPoints.Count)].position;
                }
                else
                {
                    // Find first unused spawn point
                    foreach (var spawnPoint in spawnPoints)
                    {
                        if (IsPositionAvailable(spawnPoint.position))
                        {
                            return spawnPoint.position;
                        }
                    }
                }
            }
            
            // Generate random position within spawn radius
            return GenerateRandomSpawnPosition();
        }
        
        /// <summary>
        /// Generates a random spawn position within the spawn radius
        /// </summary>
        private Vector3 GenerateRandomSpawnPosition()
        {
            Vector3 center = transform.position;
            int attempts = 0;
            
            while (attempts < maxSpawnAttempts)
            {
                Vector2 randomCircle = Random.insideUnitCircle * spawnRadius;
                Vector3 randomPosition = center + new Vector3(randomCircle.x, 0, randomCircle.y);
                
                if (IsPositionAvailable(randomPosition))
                {
                    return randomPosition;
                }
                
                attempts++;
            }
            
            Debug.LogWarning("[PuzzleTriggerSpawner] Could not find available spawn position after max attempts");
            return Vector3.zero;
        }
        
        /// <summary>
        /// Checks if a position is available for spawning
        /// </summary>
        private bool IsPositionAvailable(Vector3 position)
        {
            // Check if position is too close to existing triggers
            foreach (var trigger in spawnedTriggers)
            {
                if (trigger != null && Vector3.Distance(trigger.transform.position, position) < 2f)
                {
                    return false;
                }
            }
            
            // Check if position is on valid ground
            RaycastHit hit;
            if (Physics.Raycast(position + Vector3.up * 10f, Vector3.down, out hit, 20f))
            {
                return true;
            }
            
            return false;
        }
        
        /// <summary>
        /// Clears all spawned triggers
        /// </summary>
        [ContextMenu("Clear All Triggers")]
        public void ClearSpawnedTriggers()
        {
            foreach (var trigger in spawnedTriggers)
            {
                if (trigger != null)
                {
                    DestroyImmediate(trigger);
                }
            }
            
            spawnedTriggers.Clear();
            Debug.Log("[PuzzleTriggerSpawner] Cleared all spawned triggers");
        }
        
        /// <summary>
        /// Gets all spawned triggers
        /// </summary>
        public List<GameObject> GetSpawnedTriggers()
        {
            return spawnedTriggers.Where(t => t != null).ToList();
        }
        
        /// <summary>
        /// Gets a specific spawned trigger by name
        /// </summary>
        public GameObject GetSpawnedTrigger(string triggerName)
        {
            return spawnedTriggers.FirstOrDefault(t => t != null && t.name.Contains(triggerName));
        }
        
        /// <summary>
        /// Respawns all triggers
        /// </summary>
        [ContextMenu("Respawn All Triggers")]
        public void RespawnAllTriggers()
        {
            ClearSpawnedTriggers();
            SpawnAllTriggers();
        }
        
        /// <summary>
        /// Adds a spawn point
        /// </summary>
        public void AddSpawnPoint(Vector3 position)
        {
            var spawnPoint = new GameObject($"SpawnPoint_{spawnPoints.Count}");
            spawnPoint.transform.position = position;
            spawnPoint.transform.SetParent(transform);
            spawnPoints.Add(spawnPoint.transform);
        }
        
        /// <summary>
        /// Removes a spawn point
        /// </summary>
        public void RemoveSpawnPoint(int index)
        {
            if (index >= 0 && index < spawnPoints.Count)
            {
                if (spawnPoints[index] != null)
                {
                    DestroyImmediate(spawnPoints[index].gameObject);
                }
                spawnPoints.RemoveAt(index);
            }
        }
        
        private void OnDestroy()
        {
            if (cleanupOnDestroy)
            {
                ClearSpawnedTriggers();
            }
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
                }
            }
            
            // Draw spawned triggers
            Gizmos.color = Color.blue;
            foreach (var trigger in spawnedTriggers)
            {
                if (trigger != null)
                {
                    Gizmos.DrawWireCube(trigger.transform.position, Vector3.one);
                }
            }
        }
    }
}
