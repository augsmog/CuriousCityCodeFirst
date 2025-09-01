using UnityEngine;
using System.Collections.Generic;
using CuriousCity.Core;  // For FirstPersonController and mission manager
using CuriousCity.Analytics;

namespace CuriousCity.Core
{
/// <summary>
/// Interactable object that triggers puzzle sequences in historical missions.
/// Tracks completion status and communicates with the mission manager.
/// Can be configured from PuzzleTriggerAsset for easy management.
/// </summary>
public class PuzzleTriggerInteractable : InteractableObject
{
    [Header("Puzzle Configuration")]
    public string puzzleType = "ChronoCircuits";
    public bool puzzleCompleted = false;
    public bool requiresPrerequisites = false;
    public List<string> prerequisitePuzzles = new List<string>();
    
    [Header("Visual Feedback")]
    public GameObject completedVisual;
    public ParticleSystem completionParticles;
    public Material completedMaterial;
    
    [Header("Analytics")]
    public bool trackDetailedInteraction = true;
    public float puzzleDifficulty = 1f;
    
    // References
    private HistoricalMissionSceneManager missionManager;
    private LearningStyleTracker learningTracker;
    private Renderer[] renderers;
    
    // State tracking
    private int interactionCount = 0;
    private float firstInteractionTime = 0f;
    
    // Events
    public System.Action OnPuzzleCompleted;
    
    protected override void Awake()
    {
        base.Awake();
        
        // Ensure this object has a collider for interaction detection
        SetupInteractionCollider();
        
        // Find managers
        missionManager = FindFirstObjectByType<HistoricalMissionSceneManager>();
        learningTracker = FindFirstObjectByType<LearningStyleTracker>();
        
        // Cache renderers for material swapping
        renderers = GetComponentsInChildren<Renderer>();
        
        // Set interaction type for analytics
        interactionType = "puzzle_trigger";
        
        // Update prompt based on puzzle type
        UpdateInteractionPrompt();
        
        // Show completed state if already done
        if (puzzleCompleted)
        {
            ShowCompletedState();
        }
    }
    
    /// <summary>
    /// Configures this puzzle trigger from a PuzzleTriggerAsset
    /// </summary>
    public void ConfigureFromAsset(PuzzleTriggerAsset asset)
    {
        if (asset == null) return;
        
        // Basic configuration
        puzzleType = asset.puzzleType;
        requiresPrerequisites = asset.requiresPrerequisites;
        prerequisitePuzzles = new List<string>(asset.prerequisitePuzzles);
        puzzleDifficulty = asset.puzzleDifficulty;
        
        // Visual configuration
        completedMaterial = asset.completedMaterial;
        
        // Interaction configuration
        interactionPrompt = asset.interactionPrompt;
        highlightColor = asset.highlightColor;
        highlightIntensity = asset.highlightIntensity;
        
        // Analytics configuration
        trackDetailedInteraction = asset.trackDetailedInteraction;
        
        // Update prompt
        UpdateInteractionPrompt();
        
        Debug.Log($"[PuzzleTriggerInteractable] Configured from asset: {asset.name}");
    }
    
    /// <summary>
    /// Sets the completed visual GameObject
    /// </summary>
    public void SetCompletedVisual(GameObject visual)
    {
        completedVisual = visual;
    }
    
    /// <summary>
    /// Sets the completion particles
    /// </summary>
    public void SetCompletionParticles(ParticleSystem particles)
    {
        completionParticles = particles;
    }
    
    /// <summary>
    /// Sets the audio source
    /// </summary>
    public void SetAudioSource(AudioSource source)
    {
        audioSource = source;
    }
    
    private void SetupInteractionCollider()
    {
        // Ensure there's a collider for interaction detection
        Collider existingCollider = GetComponent<Collider>();
        if (existingCollider == null)
        {
            // Add a sphere collider as default
            SphereCollider sphereCollider = gameObject.AddComponent<SphereCollider>();
            sphereCollider.radius = 1.5f;
            sphereCollider.isTrigger = true;
            Debug.Log($"[PuzzleTriggerInteractable] Added default SphereCollider to {gameObject.name}");
        }
        else
        {
            // Make sure existing collider is set as trigger
            existingCollider.isTrigger = true;
            Debug.Log($"[PuzzleTriggerInteractable] Using existing collider: {existingCollider.GetType().Name}");
        }
        
        // Ensure this object has the Interactable tag for the player's raycast system
        if (gameObject.tag != "Interactable")
        {
            gameObject.tag = "Interactable";
            Debug.Log($"[PuzzleTriggerInteractable] Set tag to 'Interactable' for {gameObject.name}");
        }
        
        // Ensure the object is on a layer that can be raycast against
        if (gameObject.layer == LayerMask.NameToLayer("Ignore Raycast"))
        {
            gameObject.layer = LayerMask.NameToLayer("Default");
            Debug.Log($"[PuzzleTriggerInteractable] Changed layer from 'Ignore Raycast' to 'Default' for {gameObject.name}");
        }
        
        Debug.Log($"[PuzzleTriggerInteractable] Setup complete for {gameObject.name} - Tag: {gameObject.tag}, Layer: {LayerMask.LayerToName(gameObject.layer)}");
    }
    
    private void Start()
    {
        // Register with mission manager if available
        if (missionManager != null)
        {
            // Mission manager can track all puzzle triggers in the scene
            // Note: RegisterPuzzleTrigger method needs to be added to HistoricalMissionSceneManager
        }
        
        // Try to register with the registry if it exists
        var registry = FindFirstObjectByType<PuzzleTriggerRegistry>();
        if (registry != null)
        {
            registry.RegisterPuzzleTrigger(this);
        }
    }
    
    protected override void PerformInteraction(FirstPersonController player)
    {
        Debug.Log($"[PuzzleTriggerInteractable] PerformInteraction called on {gameObject.name} for puzzle: {puzzleType}");
        
        if (puzzleCompleted) 
        {
            Debug.Log($"[PuzzleTriggerInteractable] Puzzle {puzzleType} already completed, ignoring interaction");
            return;
        }
        
        // Track interaction attempt
        interactionCount++;
        if (firstInteractionTime == 0f)
        {
            firstInteractionTime = Time.time;
        }
        
        Debug.Log($"[PuzzleTriggerInteractable] Interaction count: {interactionCount}, First interaction time: {firstInteractionTime}");
        
        // Check prerequisites
        if (requiresPrerequisites && !CheckPrerequisites())
        {
            Debug.Log($"[PuzzleTriggerInteractable] Prerequisites not met for {puzzleType}");
            ShowPrerequisiteMessage();
            return;
        }
        
        // Log detailed analytics
        if (trackDetailedInteraction && learningTracker != null)
        {
            learningTracker.LogDetailedEvent("puzzle_trigger_activated", 
                $"Triggered {puzzleType} puzzle", "puzzle_interaction",
                new Dictionary<string, object>
                {
                    {"puzzle_type", puzzleType},
                    {"puzzle_difficulty", puzzleDifficulty},
                    {"interaction_count", interactionCount},
                    {"time_to_first_interaction", firstInteractionTime > 0 ? Time.time - firstInteractionTime : 0f},
                    {"player_position", player.transform.position}
                });
        }
        
        // Trigger the puzzle through mission manager
        if (missionManager != null)
        {
            Debug.Log($"[PuzzleTriggerInteractable] Triggering puzzle {puzzleType} through mission manager");
            missionManager.OnPuzzleTriggered(puzzleType, transform.position);
        }
        else
        {
            Debug.LogError($"[PuzzleTriggerInteractable] No HistoricalMissionSceneManager found to trigger puzzle: {puzzleType}");
        }
    }
    
    public override bool CanInteract()
    {
        return base.CanInteract() && !puzzleCompleted;
    }
    
    public override string GetInteractionPrompt()
    {
        if (puzzleCompleted)
        {
            return "Puzzle already completed";
        }
        
        if (requiresPrerequisites && !CheckPrerequisites())
        {
            int remaining = GetRemainingPrerequisites();
            return $"Complete {remaining} more puzzle{(remaining != 1 ? "s" : "")} first";
        }
        
        return interactionPrompt;
    }
    
    /// <summary>
    /// Marks this puzzle as completed
    /// </summary>
    public void MarkCompleted()
    {
        if (puzzleCompleted) return;
        
        puzzleCompleted = true;
        isEnabled = false;
        
        // Log completion analytics
        if (learningTracker != null)
        {
            float totalTime = firstInteractionTime > 0 ? Time.time - firstInteractionTime : 0f;
            
            learningTracker.LogDetailedEvent("puzzle_completed", 
                $"Completed {puzzleType} puzzle", "achievement",
                new Dictionary<string, object>
                {
                    {"puzzle_type", puzzleType},
                    {"completion_time", totalTime},
                    {"attempt_count", interactionCount},
                    {"puzzle_difficulty", puzzleDifficulty}
                });
        }
        
        // Show visual completion state
        ShowCompletedState();
        
        // Play completion effects
        PlayCompletionEffects();
        
        // Invoke completion event
        OnPuzzleCompleted?.Invoke();
    }
    
    private void UpdateInteractionPrompt()
    {
        switch (puzzleType.ToLower())
        {
            case "chronocircuits":
                interactionPrompt = "Press E to activate Chrono Circuits";
                break;
            case "scrollofsecrets":
                interactionPrompt = "Press E to examine ancient scroll";
                break;
            case "pyramidrebuilder":
                interactionPrompt = "Press E to reconstruct pyramid";
                break;
            default:
                interactionPrompt = $"Press E to start {puzzleType} puzzle";
                break;
        }
    }
    
    private bool CheckPrerequisites()
    {
        if (!requiresPrerequisites || prerequisitePuzzles.Count == 0)
            return true;
        
        if (missionManager == null)
            return false;
        
        foreach (string prereq in prerequisitePuzzles)
        {
            if (!missionManager.IsPuzzleCompleted(prereq))
                return false;
        }
        
        return true;
    }
    
    private int GetRemainingPrerequisites()
    {
        if (!requiresPrerequisites || missionManager == null)
            return 0;
        
        int remaining = 0;
        foreach (string prereq in prerequisitePuzzles)
        {
            if (!missionManager.IsPuzzleCompleted(prereq))
                remaining++;
        }
        
        return remaining;
    }
    
    private void ShowPrerequisiteMessage()
    {
        // Could show a UI message or play a sound
        Debug.Log($"Cannot start {puzzleType} - prerequisites not met");
        
        if (audioSource != null)
        {
            // Play a "locked" sound effect
            audioSource.pitch = 0.8f;
            audioSource.PlayOneShot(interactionSound);
            audioSource.pitch = 1f;
        }
    }
    
    private void ShowCompletedState()
    {
        // Enable completed visual indicator
        if (completedVisual != null)
        {
            completedVisual.SetActive(true);
        }
        
        // Swap to completed material
        if (completedMaterial != null && renderers != null)
        {
            foreach (var renderer in renderers)
            {
                var materials = renderer.materials;
                for (int i = 0; i < materials.Length; i++)
                {
                    materials[i] = completedMaterial;
                }
                renderer.materials = materials;
            }
        }
        
        // Make the object semi-transparent to indicate it's completed
        foreach (var renderer in renderers)
        {
            var materials = renderer.materials;
            foreach (var material in materials)
            {
                if (material.HasProperty("_Color"))
                {
                    var color = material.color;
                    color.a = 0.5f;
                    material.color = color;
                }
                
                // Enable transparency
                material.SetFloat("_Mode", 3);
                material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                material.SetInt("_ZWrite", 0);
                material.DisableKeyword("_ALPHATEST_ON");
                material.EnableKeyword("_ALPHABLEND_ON");
                material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                material.renderQueue = 3000;
            }
        }
    }
    
    private void PlayCompletionEffects()
    {
        // Play particle effect
        if (completionParticles != null)
        {
            completionParticles.Play();
        }
        
        // Play completion sound
        if (audioSource != null && missionManager != null && missionManager.puzzleCompleteSound != null)
        {
            audioSource.PlayOneShot(missionManager.puzzleCompleteSound);
        }
    }
    
    /// <summary>
    /// Reset the puzzle trigger (useful for testing or replay)
    /// </summary>
    public void ResetPuzzle()
    {
        puzzleCompleted = false;
        isEnabled = true;
        interactionCount = 0;
        firstInteractionTime = 0f;
        
        // Hide completed visual
        if (completedVisual != null)
        {
            completedVisual.SetActive(false);
        }
        
        // Reset transparency
        foreach (var renderer in renderers)
        {
            var materials = renderer.materials;
            foreach (var material in materials)
            {
                if (material.HasProperty("_Color"))
                {
                    var color = material.color;
                    color.a = 1f;
                    material.color = color;
                }
            }
        }
    }
    
    private void OnDrawGizmosSelected()
    {
        // Draw puzzle type label in scene view
        Gizmos.color = puzzleCompleted ? Color.green : Color.yellow;
        Gizmos.DrawWireSphere(transform.position, 0.5f);
        
        #if UNITY_EDITOR
        UnityEditor.Handles.Label(transform.position + Vector3.up * 2f, 
            $"Puzzle: {puzzleType}\nCompleted: {puzzleCompleted}");
        #endif
    }
}
}