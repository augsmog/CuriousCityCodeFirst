using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using CuriousCity.Core;  // For FirstPersonController and mission manager
using CuriousCity.Data;  // For ArtifactData
using CuriousCity.Analytics;

namespace CuriousCity.Core
{
    /// <summary>
    /// Interactable object for collecting artifacts in historical missions.
    /// Handles artifact collection requirements, visual effects, and integration with mission flow.
    /// </summary>
    public class ArtifactInteractable : InteractableObject
    {
        [Header("Artifact Configuration")]
        public ArtifactData artifactData;
        public bool requiresAllPuzzles = true;
        public List<string> requiredPuzzles = new List<string> { "ChronoCircuits", "ScrollOfSecrets", "PyramidRebuilder" };
        
        [Header("Visual Effects")]
        public GameObject artifactModel;
        public ParticleSystem glowEffect;
        public ParticleSystem collectionEffect;
        public Light artifactLight;
        public AnimationCurve floatCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        public float floatHeight = 0.3f;
        public float floatSpeed = 1f;
        public float rotationSpeed = 30f;
        
        [Header("Interaction Settings")]
        private float interactionRange = 3f;
        
        [Header("Collection Sequence")]
        public float collectionDuration = 3f;
        public AnimationCurve collectionCurve;
        public GameObject collectionBeam;
        public Transform collectionTarget;
        
        [Header("Audio")]
        public AudioClip approachSound;
        public AudioClip collectionSound;
        public AudioClip lockedSound;
        public float approachSoundDistance = 5f;
        
        [Header("UI Feedback")]
        public GameObject lockedIndicator;
        public TextMeshPro requirementText;
        
        // References
        private HistoricalMissionSceneManager missionManager;
        private LearningStyleTracker learningTracker;
        
        // State
        private bool isCollected = false;
        private bool playerNearby = false;
        private float originalY;
        private float floatTimer = 0f;
        private bool approachSoundPlayed = false;
        private Coroutine collectionCoroutine;
        
        protected override void Awake()
        {
            base.Awake();
            
            // Set interaction type
            interactionType = "artifact_collection";
            interactionPrompt = "Press E to collect artifact";
            
            // Cache original position
            if (artifactModel != null)
            {
                originalY = artifactModel.transform.localPosition.y;
            }
            
            // Find references
            missionManager = FindFirstObjectByType<HistoricalMissionSceneManager>();
            learningTracker = FindFirstObjectByType<LearningStyleTracker>();
            
            // Ensure we have artifact data
            if (artifactData == null)
            {
                Debug.LogWarning($"ArtifactInteractable on {gameObject.name} is missing ArtifactData!");
            }
        }
        
        private void Start()
        {
            UpdateVisualState();
            
            // Start floating animation
            if (artifactModel != null)
            {
                StartCoroutine(FloatAnimation());
            }
        }
        
        private void Update()
        {
            // Check for player proximity
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                float distance = Vector3.Distance(transform.position, player.transform.position);
                
                // Play approach sound when player gets close
                if (distance < approachSoundDistance && !approachSoundPlayed && !isCollected)
                {
                    if (audioSource != null && approachSound != null)
                    {
                        audioSource.PlayOneShot(approachSound);
                        approachSoundPlayed = true;
                    }
                }
                else if (distance > approachSoundDistance + 2f)
                {
                    approachSoundPlayed = false;
                }
                
                // Update glow intensity based on distance
                if (artifactLight != null && !isCollected)
                {
                    float normalizedDistance = Mathf.Clamp01(1f - (distance / interactionRange));
                    artifactLight.intensity = Mathf.Lerp(0.5f, 2f, normalizedDistance);
                }
            }
        }
        
        protected override void PerformInteraction(FirstPersonController player)
        {
            if (isCollected) return;
            
            bool canCollect = CanCollectArtifact();
            
            // Log interaction attempt
            if (learningTracker != null)
            {
                learningTracker.LogDetailedEvent("artifact_interaction_attempt", 
                    $"Attempted to collect {artifactData?.artifactName ?? "artifact"}", "collection",
                    new Dictionary<string, object>
                    {
                        {"artifact_id", artifactData?.id ?? "unknown"},
                        {"can_collect", canCollect},
                        {"puzzles_completed", GetCompletedPuzzleCount()},
                        {"time_in_mission", Time.timeSinceLevelLoad}
                    });
            }
            
            if (canCollect)
            {
                CollectArtifact(player);
            }
            else
            {
                ShowLockedFeedback();
            }
        }
        
        public override bool CanInteract()
        {
            return base.CanInteract() && !isCollected;
        }
        
        public override string GetInteractionPrompt()
        {
            if (isCollected)
            {
                return "Artifact already collected";
            }
            
            if (!CanCollectArtifact())
            {
                if (missionManager != null)
                {
                    int remaining = requiredPuzzles.Count - GetCompletedPuzzleCount();
                    return $"Complete {remaining} more puzzle{(remaining != 1 ? "s" : "")} to unlock";
                }
                return "Complete all puzzles to unlock";
            }
            
            return $"Press E to collect {artifactData?.artifactName ?? "artifact"}";
        }
        
        private bool CanCollectArtifact()
        {
            if (!requiresAllPuzzles) return true;
            
            if (missionManager != null)
            {
                return missionManager.CanAccessArtifact();
            }
            
            // If no mission manager, check individual puzzles
            return GetCompletedPuzzleCount() >= requiredPuzzles.Count;
        }
        
        private int GetCompletedPuzzleCount()
        {
            if (missionManager == null) return 0;
            
            int completed = 0;
            foreach (string puzzle in requiredPuzzles)
            {
                if (missionManager.IsPuzzleCompleted(puzzle))
                {
                    completed++;
                }
            }
            return completed;
        }
        
        private void CollectArtifact(FirstPersonController player)
        {
            if (isCollected || collectionCoroutine != null) return;
            
            isCollected = true;
            isEnabled = false;
            
            // Log collection analytics
            if (learningTracker != null)
            {
                learningTracker.LogDetailedEvent("artifact_collected", 
                    $"Collected {artifactData?.artifactName ?? "artifact"}", "achievement",
                    new Dictionary<string, object>
                    {
                        {"artifact_id", artifactData?.id ?? "unknown"},
                        {"artifact_power", artifactData?.artifactPower ?? 0},
                        {"time_to_collect", Time.timeSinceLevelLoad},
                        {"player_position", player.transform.position}
                    });
            }
            
            // Start collection sequence
            collectionCoroutine = StartCoroutine(CollectionSequence(player));
        }
        
        private IEnumerator CollectionSequence(FirstPersonController player)
        {
            // Disable player movement during collection
            // NOTE: SetControlsEnabled exists in FirstPersonController based on the search results
            player.SetControlsEnabled(false, false, false, false);
            
            // Play collection sound
            if (audioSource != null && collectionSound != null)
            {
                audioSource.PlayOneShot(collectionSound);
            }
            
            // Enable collection effects
            if (collectionEffect != null)
            {
                collectionEffect.Play();
            }
            
            if (collectionBeam != null)
            {
                collectionBeam.SetActive(true);
            }
            
            // Animate artifact to player or collection point
            float elapsed = 0f;
            Vector3 startPos = artifactModel.transform.position;
            Vector3 endPos = collectionTarget != null ? collectionTarget.position : player.transform.position + Vector3.up;
            
            while (elapsed < collectionDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / collectionDuration;
                
                if (collectionCurve != null && collectionCurve.length > 0)
                {
                    t = collectionCurve.Evaluate(t);
                }
                
                // Move and scale artifact
                if (artifactModel != null)
                {
                    artifactModel.transform.position = Vector3.Lerp(startPos, endPos, t);
                    artifactModel.transform.localScale = Vector3.one * (1f - t * 0.8f);
                    
                    // Increase rotation speed
                    artifactModel.transform.Rotate(Vector3.up, rotationSpeed * 3f * Time.deltaTime);
                }
                
                // Fade out glow
                if (glowEffect != null)
                {
                    var main = glowEffect.main;
                    main.startLifetime = Mathf.Lerp(2f, 0f, t);
                }
                
                yield return null;
            }
            
            // Hide artifact
            if (artifactModel != null)
            {
                artifactModel.SetActive(false);
            }
            
            // Notify mission manager
            if (missionManager != null)
            {
                missionManager.OnArtifactRecovered();
            }
            
            // Re-enable player movement
            player.SetControlsEnabled(true, true, true, true);
            
            // Disable this game object after a short delay
            yield return new WaitForSeconds(1f);
            gameObject.SetActive(false);
        }
        
        private void ShowLockedFeedback()
        {
            // Play locked sound
            if (audioSource != null && lockedSound != null)
            {
                audioSource.PlayOneShot(lockedSound);
            }
            
            // Show visual feedback
            if (lockedIndicator != null)
            {
                StartCoroutine(FlashLockedIndicator());
            }
            
            // Update requirement text
            if (requirementText != null)
            {
                int remaining = requiredPuzzles.Count - GetCompletedPuzzleCount();
                requirementText.text = $"{remaining} puzzles remaining";
                requirementText.gameObject.SetActive(true);
                StartCoroutine(HideRequirementText());
            }
        }
        
        private IEnumerator FlashLockedIndicator()
        {
            lockedIndicator.SetActive(true);
            yield return new WaitForSeconds(2f);
            lockedIndicator.SetActive(false);
        }
        
        private IEnumerator HideRequirementText()
        {
            yield return new WaitForSeconds(3f);
            if (requirementText != null)
            {
                requirementText.gameObject.SetActive(false);
            }
        }
        
        private IEnumerator FloatAnimation()
        {
            while (!isCollected)
            {
                floatTimer += Time.deltaTime * floatSpeed;
                
                // Float up and down
                float yOffset = floatCurve.Evaluate(Mathf.PingPong(floatTimer, 1f)) * floatHeight;
                artifactModel.transform.localPosition = new Vector3(
                    artifactModel.transform.localPosition.x,
                    originalY + yOffset,
                    artifactModel.transform.localPosition.z
                );
                
                // Rotate
                artifactModel.transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
                
                yield return null;
            }
        }
        
        private void UpdateVisualState()
        {
            bool locked = !CanCollectArtifact();
            
            // Update glow effect
            if (glowEffect != null)
            {
                var main = glowEffect.main;
                main.startColor = locked ? Color.red : Color.yellow;
            }
            
            // Update light color
            if (artifactLight != null)
            {
                artifactLight.color = locked ? Color.red : Color.yellow;
            }
            
            // Show/hide locked indicator
            if (lockedIndicator != null)
            {
                lockedIndicator.SetActive(locked);
            }
        }
        
        public override void ShowHighlight()
        {
            base.ShowHighlight();
            
            // Intensify glow when highlighted
            if (glowEffect != null && !isCollected)
            {
                var emission = glowEffect.emission;
                emission.rateOverTime = 50f;
            }
        }
        
        public override void HideHighlight()
        {
            base.HideHighlight();
            
            // Return glow to normal
            if (glowEffect != null && !isCollected)
            {
                var emission = glowEffect.emission;
                emission.rateOverTime = 20f;
            }
        }
        
        private void OnDrawGizmosSelected()
        {
            // Draw collection range
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, interactionRange);
            
            // Draw approach sound range
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, approachSoundDistance);
            
            #if UNITY_EDITOR
            string status = isCollected ? "Collected" : (CanCollectArtifact() ? "Available" : "Locked");
            UnityEditor.Handles.Label(transform.position + Vector3.up * 2f, 
                $"Artifact: {artifactData?.artifactName ?? "Unknown"}\nStatus: {status}");
            #endif
        }
    }
}