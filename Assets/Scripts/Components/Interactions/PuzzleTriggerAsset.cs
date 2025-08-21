using UnityEngine;
using System.Collections.Generic;

namespace CuriousCity.Core
{
    /// <summary>
    /// ScriptableObject asset for configuring puzzle triggers
    /// Can be created and managed through the asset folder for easy adjustment
    /// </summary>
    [CreateAssetMenu(fileName = "NewPuzzleTrigger", menuName = "Curious City/Puzzle Trigger Asset")]
    public class PuzzleTriggerAsset : ScriptableObject
    {
        [Header("Basic Configuration")]
        public string triggerName = "Puzzle Trigger";
        public string puzzleType = "ChronoCircuits";
        public string description = "Triggers a puzzle sequence";
        
        [Header("Puzzle Settings")]
        public bool requiresPrerequisites = false;
        public List<string> prerequisitePuzzles = new List<string>();
        public float puzzleDifficulty = 1f;
        
        [Header("Visual Configuration")]
        public GameObject visualPrefab;
        public Material completedMaterial;
        public GameObject completedVisualPrefab;
        public ParticleSystem completionParticlesPrefab;
        
        [Header("Audio Configuration")]
        public AudioClip interactionSound;
        public AudioClip completionSound;
        public AudioClip lockedSound;
        
        [Header("Spawn Settings")]
        public Vector3 spawnOffset = Vector3.zero;
        public Vector3 spawnRotation = Vector3.zero;
        public Vector3 spawnScale = Vector3.one;
        public bool autoSpawn = true;
        public string spawnTag = "PuzzleTrigger";
        
        [Header("Interaction Settings")]
        public float interactionRange = 2f;
        public string interactionPrompt = "Press E to interact";
        public bool showHighlight = true;
        public Color highlightColor = Color.yellow;
        public float highlightIntensity = 1.5f;
        
        [Header("Advanced Settings")]
        public bool enableAnalytics = true;
        public bool trackDetailedInteraction = true;
        public string customLayer = "Default";
        public bool useCustomCollider = false;
        public Vector3 customColliderSize = Vector3.one;
        
        /// <summary>
        /// Creates a puzzle trigger GameObject from this asset
        /// </summary>
        public GameObject CreatePuzzleTrigger(Vector3 position, Transform parent = null)
        {
            // Create the base GameObject
            GameObject triggerObject = new GameObject($"{triggerName}_{puzzleType}");
            
            // Set position and parent
            triggerObject.transform.position = position + spawnOffset;
            triggerObject.transform.rotation = Quaternion.Euler(spawnRotation);
            triggerObject.transform.localScale = spawnScale;
            
            if (parent != null)
            {
                triggerObject.transform.SetParent(parent);
            }
            
            // Set tag
            triggerObject.tag = spawnTag;
            
            // Set layer
            if (!string.IsNullOrEmpty(customLayer))
            {
                int layerIndex = LayerMask.NameToLayer(customLayer);
                if (layerIndex != -1)
                {
                    triggerObject.layer = layerIndex;
                }
            }
            
            // Add visual representation
            if (visualPrefab != null)
            {
                GameObject visual = Instantiate(visualPrefab, triggerObject.transform);
                visual.transform.localPosition = Vector3.zero;
                visual.transform.localRotation = Quaternion.identity;
                visual.transform.localScale = Vector3.one;
            }
            
            // Add the PuzzleTriggerInteractable component
            var puzzleTrigger = triggerObject.AddComponent<PuzzleTriggerInteractable>();
            
            // Configure the component with asset data
            puzzleTrigger.ConfigureFromAsset(this);
            
            // Add completion visual if specified
            if (completedVisualPrefab != null)
            {
                GameObject completedVisual = Instantiate(completedVisualPrefab, triggerObject.transform);
                completedVisual.transform.localPosition = Vector3.zero;
                completedVisual.transform.localRotation = Quaternion.identity;
                completedVisual.transform.localScale = Vector3.one;
                completedVisual.SetActive(false); // Start hidden
                puzzleTrigger.SetCompletedVisual(completedVisual);
            }
            
            // Add completion particles if specified
            if (completionParticlesPrefab != null)
            {
                var particles = Instantiate(completionParticlesPrefab, triggerObject.transform);
                particles.transform.localPosition = Vector3.zero;
                particles.transform.localRotation = Quaternion.identity;
                puzzleTrigger.SetCompletionParticles(particles);
            }
            
            // Add audio source if we have audio clips
            if (interactionSound != null || completionSound != null || lockedSound != null)
            {
                var audioSource = triggerObject.AddComponent<AudioSource>();
                audioSource.playOnAwake = false;
                audioSource.spatialBlend = 1f; // 3D sound
                puzzleTrigger.SetAudioSource(audioSource);
            }
            
            Debug.Log($"[PuzzleTriggerAsset] Created puzzle trigger '{triggerName}' of type '{puzzleType}' at {position}");
            
            return triggerObject;
        }
        
        /// <summary>
        /// Validates the asset configuration
        /// </summary>
        public bool ValidateConfiguration()
        {
            if (string.IsNullOrEmpty(puzzleType))
            {
                Debug.LogError($"[PuzzleTriggerAsset] {name}: Puzzle type is required!");
                return false;
            }
            
            if (string.IsNullOrEmpty(triggerName))
            {
                Debug.LogError($"[PuzzleTriggerAsset] {name}: Trigger name is required!");
                return false;
            }
            
            if (interactionRange <= 0)
            {
                Debug.LogWarning($"[PuzzleTriggerAsset] {name}: Interaction range should be greater than 0!");
            }
            
            return true;
        }
        
        private void OnValidate()
        {
            // Auto-validate when asset is modified in editor
            ValidateConfiguration();
        }
        
        /// <summary>
        /// Gets a display name for the asset
        /// </summary>
        public string GetDisplayName()
        {
            return $"{triggerName} ({puzzleType})";
        }
        
        /// <summary>
        /// Gets the expected completion time for this puzzle type
        /// </summary>
        public float GetExpectedCompletionTime()
        {
            switch (puzzleType.ToLower())
            {
                case "chronocircuits": return 60f;
                case "scrollofsecrets": return 90f;
                case "pyramidrebuilder": return 120f;
                default: return 60f;
            }
        }
    }
}
