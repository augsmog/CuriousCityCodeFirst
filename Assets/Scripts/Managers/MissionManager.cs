using System;  // ADD THIS LINE - Required for Action<> delegate
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using CuriousCityAutomated.Data;  // For MissionResults, PuzzleResults
using CuriousCityAutomated.Gameplay.Interactions;  // For InteractableObject
using CuriousCityAutomated.Characters;  // For FirstPersonController
using CuriousCityAutomated.Analytics;

namespace CuriousCity.Core.Puzzles
{
    /// <summary>
    /// Manages historical mission scenes like the Egypt Exploration Scene.
    /// Handles puzzle triggers, exploration zones, artifact recovery, and mission progression.
    /// Template for all future historical missions.
    /// </summary>
    public class MissionManager : MonoBehaviour
    {
        [Header("Mission Configuration")]
        public HistoricalMissionData missionData;
        public Transform player;
        public Camera playerCamera;
        
        [Header("Exploration Zones")]
        public List<ExplorationZone> explorationZones;
        
        [Header("Puzzle System")]
        public PuzzleManager puzzleManager;
        public Transform puzzleUIContainer;
        public Canvas puzzleOverlayCanvas;
        
        [Header("Artifact Recovery")]
        public Transform artifactVault;
        public GameObject artifactObject;
        public ParticleSystem artifactGlow;
        public AudioSource artifactDiscoveryAudio;
        
        [Header("Chrona Integration")]
        public ChronaFieldPresence chronaPresence;
        public AudioSource chronaVoiceSource;
        
        [Header("Mission UI")]
        public TextMeshProUGUI objectiveText;
        public Slider missionProgressSlider;
        public GameObject compassIndicator;
        public TextMeshProUGUI puzzleCountText;
        
        [Header("Audio & Atmosphere")]
        public AudioSource ambientAudioSource;
        public AudioClip explorationMusic;
        public AudioClip puzzleCompleteSound;
        public AudioClip missionCompleteSound;
        
        // Mission State
        private Dictionary<string, bool> puzzleCompletionStatus;
        private Dictionary<string, bool> zoneExplorationStatus;
        private bool missionCompleted = false;
        private int totalPuzzles = 3;
        private int completedPuzzles = 0;
        private LearningStyleTracker learningTracker;
        
        // Events - Now Action<> will be recognized with System namespace
        public static event Action<string> OnPuzzleCompleted;
        public static event Action<string> OnZoneExplored;
        public static event Action<MissionResults> OnMissionCompleted;
        
        // Rest of the code remains the same...
        private void Start()
        {
            InitializeMission();
        }
        
        private void InitializeMission()
        {
            // Initialize mission state
            puzzleCompletionStatus = new Dictionary<string, bool>();
            zoneExplorationStatus = new Dictionary<string, bool>();
            
            // Get learning style tracker
            learningTracker = FindFirstObjectByType<LearningStyleTracker>();
            if (learningTracker == null)
            {
                learningTracker = gameObject.AddComponent<LearningStyleTracker>();
            }
            
            // Initialize puzzle manager
            if (puzzleManager == null)
            {
                puzzleManager = FindFirstObjectByType<PuzzleManager>();
            }
            
            if (puzzleManager != null)
            {
                puzzleManager.Initialize(this);
            }
            
            // Set up exploration zones
            InitializeExplorationZones();
            
            // Set up initial UI
            UpdateMissionUI();
            
            // Start ambient audio
            if (ambientAudioSource && explorationMusic)
            {
                ambientAudioSource.clip = explorationMusic;
                ambientAudioSource.loop = true;
                ambientAudioSource.Play();
            }
            
            // Initial Chrona briefing
            StartCoroutine(InitialMissionBriefing());
        }
        
        private void InitializeExplorationZones()
        {
            foreach (var zone in explorationZones)
            {
                zone.Initialize(this);
                zoneExplorationStatus[zone.zoneName] = false;
                
                // Set up puzzle triggers within zones
                if (zone.puzzleTrigger != null)
                {
                    zone.puzzleTrigger.Initialize(zone.puzzleType, this);
                    puzzleCompletionStatus[zone.puzzleType] = false;
                }
            }
        }
        
        private IEnumerator InitialMissionBriefing()
        {
            yield return new WaitForSeconds(1f);
            
            if (chronaPresence)
            {
                chronaPresence.TriggerDialogue("mission_start_egypt");
            }
            
            // Set initial objective
            if (objectiveText)
            {
                objectiveText.text = "Explore Ancient Egypt and solve the mysteries to recover the irrigation artifact.";
            }
            
            yield return new WaitForSeconds(3f);
            
            // Enable player movement
            var playerController = player?.GetComponent<FirstPersonController>();
            if (playerController)
            {
                playerController.enabled = true;
            }
        }
        
        public void OnZoneEntered(ExplorationZone zone)
        {
            if (!zoneExplorationStatus[zone.zoneName])
            {
                zoneExplorationStatus[zone.zoneName] = true;
                
                // Track exploration behavior
                if (learningTracker)
                {
                    learningTracker.LogExplorationEvent(zone.zoneName, zone.zoneType);
                }
                
                // Chrona commentary
                if (chronaPresence)
                {
                    chronaPresence.TriggerContextualComment(zone.zoneName);
                }
                
                OnZoneExplored?.Invoke(zone.zoneName);
            }
        }

        public void HandlePuzzleCompleted(string puzzleType, PuzzleResults results)
        {
            if (puzzleCompletionStatus.ContainsKey(puzzleType))
            {
                puzzleCompletionStatus[puzzleType] = true;
                completedPuzzles++;
                
                // Track completion
                if (learningTracker)
                {
                    learningTracker.LogPuzzleCompleted(puzzleType, results);
                }
                
                // Play completion sound
                if (ambientAudioSource && puzzleCompleteSound)
                {
                    ambientAudioSource.PlayOneShot(puzzleCompleteSound);
                }
                
                // Chrona reaction
                if (chronaPresence)
                {
                    chronaPresence.ReactToPuzzleCompletion(puzzleType, results.wasSolved);
                }
                
                // Update UI
                UpdateMissionUI();
                
                // Fire the static event
                OnPuzzleCompleted?.Invoke(puzzleType);
                
                // Check if all puzzles are complete
                CheckMissionCompletion();
            }
        }
        
        private void CheckMissionCompletion()
        {
            if (completedPuzzles >= totalPuzzles && !missionCompleted)
            {
                StartCoroutine(CompleteMission());
            }
        }
        
        private IEnumerator CompleteMission()
        {
            missionCompleted = true;
            
            // Update objective
            if (objectiveText)
            {
                objectiveText.text = "All puzzles solved! Enter the artifact vault.";
            }
            
            // Unlock artifact vault
            if (artifactVault)
            {
                var vaultTrigger = artifactVault.GetComponent<ArtifactVaultTrigger>();
                if (vaultTrigger == null)
                {
                    vaultTrigger = artifactVault.gameObject.AddComponent<ArtifactVaultTrigger>();
                }
                vaultTrigger.Initialize(this);
            }
            
            // Visual effects for vault unlock
            if (artifactGlow)
            {
                artifactGlow.Play();
            }
            
            // Chrona final commentary
            if (chronaPresence)
            {
                chronaPresence.TriggerDialogue("vault_unlocked");
            }
            
            yield return new WaitForSeconds(2f);
            
            // Update compass to point to vault
            if (compassIndicator && artifactVault)
            {
                StartCoroutine(UpdateCompassToTarget(artifactVault.position));
            }
        }
        
        public void OnArtifactRecovered()
        {
            StartCoroutine(ArtifactRecoverySequence());
        }
        
        private IEnumerator ArtifactRecoverySequence()
        {
            // Play discovery audio
            if (artifactDiscoveryAudio)
            {
                artifactDiscoveryAudio.Play();
            }
            
            // Play mission complete sound
            if (ambientAudioSource && missionCompleteSound)
            {
                ambientAudioSource.PlayOneShot(missionCompleteSound);
            }
            
            // Update objective
            if (objectiveText)
            {
                objectiveText.text = "Artifact recovered! Returning to the Ark...";
            }
            
            // Chrona final dialogue
            if (chronaPresence)
            {
                chronaPresence.TriggerDialogue("artifact_recovered");
            }
            
            yield return new WaitForSeconds(3f);
            
            // Compile mission results
            var results = CompileMissionResults();
            
            // Fire completion event
            OnMissionCompleted?.Invoke(results);
            
            yield return new WaitForSeconds(2f);
            
            // Return to Ark scene
            UnityEngine.SceneManagement.SceneManager.LoadScene("ArkBridgeScene");
        }
        
        private MissionResults CompileMissionResults()
        {
            var results = new MissionResults
            {
                missionId = missionData?.missionId ?? "egypt_exploration",
                success = true,
                artifactRecovered = true,
                artifact = new ArtifactData(
                    "Ancient Irrigation System", 
                    "technological", 
                    "Advanced water management knowledge from ancient Egypt",
                    "Hydroponics efficiency increased by 25%"
                ),
                completionTime = Time.time,
                playerChoices = new List<PlayerChoice>()
            };
            
            // Get learning style metrics from tracker
            if (learningTracker)
            {
                results.learningStyleMetrics = learningTracker.GetSessionMetrics();
            }
            
            return results;
        }
        
        private void UpdateMissionUI()
        {
            // Update progress slider
            if (missionProgressSlider)
            {
                missionProgressSlider.value = (float)completedPuzzles / totalPuzzles;
            }
            
            // Update puzzle count
            if (puzzleCountText)
            {
                puzzleCountText.text = $"Puzzles: {completedPuzzles}/{totalPuzzles}";
            }
            
            // Update objective based on progress
            if (objectiveText && completedPuzzles < totalPuzzles)
            {
                int remaining = totalPuzzles - completedPuzzles;
                objectiveText.text = $"Solve {remaining} more puzzle{(remaining > 1 ? "s" : "")} to unlock the artifact vault.";
            }
        }
        
        private IEnumerator UpdateCompassToTarget(Vector3 targetPosition)
        {
            if (compassIndicator == null || player == null) yield break;
            
            while (!missionCompleted || Vector3.Distance(player.position, targetPosition) > 2f)
            {
                Vector3 direction = (targetPosition - player.position).normalized;
                float angle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
                
                compassIndicator.transform.rotation = Quaternion.AngleAxis(angle, Vector3.up);
                
                yield return new WaitForSeconds(0.1f);
            }
        }
        
        private string GetPuzzleDisplayName(string puzzleType)
        {
            switch (puzzleType.ToLower())
            {
                case "chronocircuits": return "Chrono Circuits";
                case "scrollofsecrets": return "Scroll of Secrets";
                case "pyramidrebuilder": return "Pyramid Rebuilder";
                default: return puzzleType;
            }
        }
        
        // Public API for puzzle manager
        public void RegisterPuzzleCompletion(string puzzleType, PuzzleResults results)
        {
            HandlePuzzleCompleted(puzzleType, results);
        }
        
        public void OnPuzzleTriggered(string puzzleType, Vector3 position)
        {
            Debug.Log($"[MissionManager] OnPuzzleTriggered called for puzzle: {puzzleType} at position: {position}");
            
            if (puzzleManager)
            {
                Debug.Log($"[MissionManager] Starting puzzle {puzzleType} through puzzle manager");
                puzzleManager.StartPuzzle(puzzleType);
            }
            else
            {
                Debug.LogError($"[MissionManager] No puzzle manager found to start puzzle: {puzzleType}");
            }
            
            // Track puzzle interaction
            if (learningTracker)
            {
                learningTracker.LogPuzzleStarted(puzzleType);
            }
            
            // Update objective
            if (objectiveText)
            {
                objectiveText.text = $"Solve the {GetPuzzleDisplayName(puzzleType)} puzzle";
            }
        }
        
        public bool IsPuzzleCompleted(string puzzleType)
        {
            return puzzleCompletionStatus.GetValueOrDefault(puzzleType, false);
        }
        
        public int GetCompletedPuzzleCount()
        {
            return completedPuzzles;
        }
        
        public bool CanAccessArtifact()
        {
            return completedPuzzles >= totalPuzzles;
        }
        
        private void OnDestroy()
        {
            // Clean up any coroutines or subscriptions
        }
    }

    /// <summary>
    /// Exploration zone within the historical mission
    /// </summary>
    [System.Serializable]
    public class ExplorationZone : MonoBehaviour
    {
        [Header("Zone Configuration")]
        public string zoneName;
        public string zoneType; // "cultural", "technological", "social", etc.
        public string description;
        
        [Header("Puzzle Integration")]
        public PuzzleTrigger puzzleTrigger;
        public string puzzleType;
        
        [Header("Interactive Elements")]
        public List<InteractableObject> interactables;
        public Transform[] pointsOfInterest;
        
        [Header("Audio")]
        public AudioClip ambientSound;
        public AudioClip explorationSound;
        
        private MissionManager missionManager;
        private bool hasBeenExplored = false;
        
        public void Initialize(MissionManager manager)
        {
            missionManager = manager;
            
            // Set up zone trigger
            var trigger = GetComponent<Collider>();
            if (trigger == null)
            {
                trigger = gameObject.AddComponent<BoxCollider>();
                trigger.isTrigger = true;
            }
        }
        
        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player") && !hasBeenExplored)
            {
                hasBeenExplored = true;
                
                if (missionManager)
                {
                    missionManager.OnZoneEntered(this);
                }
                
                // Play exploration sound
                if (explorationSound)
                {
                    var audioSource = GetComponent<AudioSource>();
                    if (audioSource == null)
                    {
                        audioSource = gameObject.AddComponent<AudioSource>();
                    }
                    audioSource.PlayOneShot(explorationSound);
                }
            }
        }
    }

    /// <summary>
    /// Puzzle trigger component for activating puzzles in the world
    /// </summary>
    public class PuzzleTrigger : MonoBehaviour
    {
        [Header("Trigger Configuration")]
        public string puzzleType;
        public string displayName;
        public GameObject interactionPrompt;
        
        [Header("Visual Feedback")]
        public ParticleSystem glowEffect;
        public Light puzzleLight;
        
        private MissionManager missionManager;
        private bool isActivated = false;
        
        public void Initialize(string type, MissionManager manager)
        {
            puzzleType = type;
            missionManager = manager;
            
            // Set up interaction
            var trigger = GetComponent<Collider>();
            if (trigger == null)
            {
                trigger = gameObject.AddComponent<SphereCollider>();
                trigger.isTrigger = true;
            }
            
            // Visual setup
            if (glowEffect)
            {
                glowEffect.Play();
            }
            
            if (puzzleLight)
            {
                puzzleLight.enabled = true;
            }
        }
        
        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player") && !isActivated)
            {
                if (interactionPrompt)
                {
                    interactionPrompt.SetActive(true);
                }
            }
        }
        
        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                if (interactionPrompt)
                {
                    interactionPrompt.SetActive(false);
                }
            }
        }
        
        private void OnTriggerStay(Collider other)
        {
            if (other.CompareTag("Player") && !isActivated)
            {
                if (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Space))
                {
                    ActivatePuzzle();
                }
            }
        }
        
        private void ActivatePuzzle()
        {
            if (isActivated || missionManager == null) return;
            
            isActivated = true;
            
            // Disable visual effects
            if (glowEffect)
            {
                glowEffect.Stop();
            }
            
            if (puzzleLight)
            {
                puzzleLight.enabled = false;
            }
            
            if (interactionPrompt)
            {
                interactionPrompt.SetActive(false);
            }
            
            // Trigger puzzle in mission manager
            missionManager.OnPuzzleTriggered(puzzleType, transform.position);
        }
    }

    /// <summary>
    /// Artifact vault trigger for final artifact recovery
    /// </summary>
    public class ArtifactVaultTrigger : MonoBehaviour
    {
        private MissionManager missionManager;
        private bool canEnter = false;
        
        public void Initialize(MissionManager manager)
        {
            missionManager = manager;
            canEnter = true;
            
            var trigger = GetComponent<Collider>();
            if (trigger == null)
            {
                trigger = gameObject.AddComponent<BoxCollider>();
                trigger.isTrigger = true;
            }
        }
        
        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player") && canEnter && missionManager != null)
            {
                if (missionManager.CanAccessArtifact())
                {
                    missionManager.OnArtifactRecovered();
                    canEnter = false;
                }
            }
        }
    }

    /// <summary>
    /// Chrona's field presence during missions
    /// </summary>
    [System.Serializable]
    public class ChronaFieldPresence : MonoBehaviour
    {
        [Header("Audio")]
        public AudioSource voiceSource;
        public List<AudioClip> contextualComments;
        
        [Header("Visual")]
        public GameObject hologramProjector;
        public ParticleSystem chronicleParticles;
        
        private Dictionary<string, List<string>> contextualDialogue;
        
        private void Start()
        {
            InitializeDialogue();
        }
        
        private void InitializeDialogue()
        {
            contextualDialogue = new Dictionary<string, List<string>>
            {
                ["Nile Riverbank"] = new List<string>
                {
                    "The Nile was the lifeblood of ancient Egypt. Notice how they managed water flow.",
                    "These irrigation channels show remarkable engineering for their time."
                },
                ["Scribe's Quarters"] = new List<string>
                {
                    "Knowledge was power in ancient Egypt. Scribes were highly respected.",
                    "The papyrus scrolls contain wisdom we still use today."
                },
                ["Tomb Chamber"] = new List<string>
                {
                    "Ancient Egyptians believed in preparation for the afterlife.",
                    "The hieroglyphs tell stories of innovation and survival."
                }
            };
        }
        
        public void TriggerDialogue(string dialogueId)
        {
            // Play appropriate dialogue based on ID
            if (voiceSource && contextualComments.Count > 0)
            {
                var randomClip = contextualComments[UnityEngine.Random.Range(0, contextualComments.Count)];
                voiceSource.PlayOneShot(randomClip);
            }
        }
        
        public void TriggerContextualComment(string zoneName)
        {
            if (contextualDialogue.ContainsKey(zoneName))
            {
                var comments = contextualDialogue[zoneName];
                if (comments.Count > 0)
                {
                    // For now, just play a voice clip
                    // In full implementation, would display text and play voice
                    TriggerDialogue("contextual");
                }
            }
        }
        
        public void ReactToPuzzleCompletion(string puzzleType, bool success)
        {
            if (success)
            {
                TriggerDialogue("puzzle_success");
            }
            else
            {
                TriggerDialogue("puzzle_encourage");
            }
        }
    }

    /// <summary>
    /// Data structure for historical mission configuration
    /// </summary>
    [CreateAssetMenu(fileName = "HistoricalMissionData", menuName = "Curious City/Historical Mission Data")]
    public class HistoricalMissionData : ScriptableObject
    {
        public string missionId;
        public string title;
        public string historicalPeriod;
        public string location;
        public List<string> requiredPuzzleTypes;
        public string artifactType;
        public string artifactName;
        public int difficultyLevel;
    }
    public static void BuildMissionScene(string missionType)
    {
        SceneBuilder.ClearScene();
        
        // Build based on mission type
        switch(missionType)
        {
            case "Egypt":
                SceneBuilder.BuildEgyptMission();
                break;
            case "Greece":
                SceneBuilder.BuildGreeceMission();
                break;
            default:
                SceneBuilder.BuildEgyptMission();
                break;
        }
        
        // Initialize mission manager
        GameObject managerGO = new GameObject("MissionManager");
        MissionManager manager = managerGO.AddComponent<MissionManager>();
        manager.StartMission(missionType);
    }
}