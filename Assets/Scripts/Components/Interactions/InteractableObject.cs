using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
using CuriousCity.Characters;  // For FirstPersonController
using CuriousCity.Analytics;

namespace CuriousCity.Core
{
/// <summary>
/// Base implementation for interactable objects in the world.
/// Provides common functionality for highlighting, audio feedback, and interaction events.
/// </summary>
public class InteractableObject : MonoBehaviour, IInteractable
{
    [Header("Interaction Configuration")]
    public string interactionPrompt = "Press E to interact";
    public string interactionType = "examine";
    public bool isEnabled = true;
    
    [Header("Visual Feedback")]
    public GameObject highlightObject;
    public Color highlightColor = Color.yellow;
    public float highlightIntensity = 1.5f;
    
    [Header("Audio")]
    public AudioClip interactionSound;
    public AudioSource audioSource;
    
    [Header("Events")]
    public UnityEvent OnInteracted;
    
    // Cache for original material colors
    private Dictionary<Renderer, Color> originalColors;
    private bool isHighlighted = false;
    
    protected virtual void Awake()
    {
        // Set up audio source if not assigned
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
                audioSource.playOnAwake = false;
                audioSource.spatialBlend = 1f; // 3D sound
            }
        }
        
        // Cache original colors for highlight system
        CacheOriginalColors();
    }
    
    /// <summary>
    /// Main interaction method called by the player
    /// </summary>
    public virtual void Interact(FirstPersonController player)
    {
        if (!CanInteract()) return;
        
        // Perform the interaction
        PerformInteraction(player);
        
        // Play interaction sound
        if (interactionSound && audioSource)
        {
            audioSource.PlayOneShot(interactionSound);
        }
        
        // Invoke Unity event
        OnInteracted?.Invoke();
        
        // Log interaction for analytics
        var learningTracker = FindFirstObjectByType<LearningStyleTracker>();
        if (learningTracker)
        {
            learningTracker.LogDetailedEvent("object_interaction", 
                $"Interacted with {gameObject.name}", "interaction",
                new Dictionary<string, object>
                {
                    {"object_type", interactionType},
                    {"object_name", gameObject.name},
                    {"position", transform.position}
                });
        }
    }
    
    /// <summary>
    /// Override this method in derived classes to implement specific interaction behavior
    /// </summary>
    protected virtual void PerformInteraction(FirstPersonController player)
    {
        // Base implementation - derived classes should override
        Debug.Log($"Interacted with {gameObject.name}");
    }
    
    /// <summary>
    /// Gets the interaction prompt to display
    /// </summary>
    public virtual string GetInteractionPrompt()
    {
        return interactionPrompt;
    }
    
    /// <summary>
    /// Gets the type of interaction for analytics
    /// </summary>
    public virtual string GetInteractionType()
    {
        return interactionType;
    }
    
    /// <summary>
    /// Checks if this object can be interacted with
    /// </summary>
    public virtual bool CanInteract()
    {
        return isEnabled && gameObject.activeInHierarchy;
    }
    
    /// <summary>
    /// Called when player's interaction raycast hits this object
    /// </summary>
    protected virtual void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            var controller = other.GetComponent<FirstPersonController>();
            if (controller != null)
            {
                ShowHighlight();
            }
        }
    }
    
    /// <summary>
    /// Called when player's interaction raycast stops hitting this object
    /// </summary>
    protected virtual void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            var controller = other.GetComponent<FirstPersonController>();
            if (controller != null)
            {
                HideHighlight();
            }
        }
    }
    
    /// <summary>
    /// Shows visual highlight on the object
    /// </summary>
    public virtual void ShowHighlight()
    {
        if (isHighlighted) return;
        isHighlighted = true;
        
        // Enable highlight object if available
        if (highlightObject)
        {
            highlightObject.SetActive(true);
        }
        
        // Apply emission/glow effect to materials
        ApplyHighlightEffect();
    }
    
    /// <summary>
    /// Hides visual highlight on the object
    /// </summary>
    public virtual void HideHighlight()
    {
        if (!isHighlighted) return;
        isHighlighted = false;
        
        // Disable highlight object
        if (highlightObject)
        {
            highlightObject.SetActive(false);
        }
        
        // Remove emission/glow effect
        RemoveHighlightEffect();
    }
    
    private void CacheOriginalColors()
    {
        originalColors = new Dictionary<Renderer, Color>();
        var renderers = GetComponentsInChildren<Renderer>();
        
        foreach (var renderer in renderers)
        {
            foreach (var material in renderer.materials)
            {
                if (material.HasProperty("_EmissionColor"))
                {
                    originalColors[renderer] = material.GetColor("_EmissionColor");
                }
            }
        }
    }
    
    private void ApplyHighlightEffect()
    {
        var renderers = GetComponentsInChildren<Renderer>();
        
        foreach (var renderer in renderers)
        {
            var materials = renderer.materials;
            foreach (var material in materials)
            {
                // Enable emission
                if (material.HasProperty("_EmissionColor"))
                {
                    material.EnableKeyword("_EMISSION");
                    material.SetColor("_EmissionColor", highlightColor * highlightIntensity);
                }
                
                // Alternative: Add outline or rim lighting effect if shader supports it
                if (material.HasProperty("_OutlineColor"))
                {
                    material.SetColor("_OutlineColor", highlightColor);
                    material.SetFloat("_OutlineWidth", 0.02f);
                }
            }
        }
    }
    
    private void RemoveHighlightEffect()
    {
        var renderers = GetComponentsInChildren<Renderer>();
        
        foreach (var renderer in renderers)
        {
            var materials = renderer.materials;
            foreach (var material in materials)
            {
                // Restore original emission
                if (material.HasProperty("_EmissionColor"))
                {
                    if (originalColors.ContainsKey(renderer))
                    {
                        material.SetColor("_EmissionColor", originalColors[renderer]);
                    }
                    else
                    {
                        material.DisableKeyword("_EMISSION");
                        material.SetColor("_EmissionColor", Color.black);
                    }
                }
                
                // Remove outline
                if (material.HasProperty("_OutlineWidth"))
                {
                    material.SetFloat("_OutlineWidth", 0f);
                }
            }
        }
    }
    
    /// <summary>
    /// Enable or disable this interactable
    /// </summary>
    public virtual void SetInteractable(bool enabled)
    {
        isEnabled = enabled;
        
        // Hide highlight if disabling
        if (!enabled && isHighlighted)
        {
            HideHighlight();
        }
    }
    
    protected virtual void OnDestroy()
    {
        // Clean up any remaining highlight effects
        if (isHighlighted)
        {
            HideHighlight();
        }
    }
}
}