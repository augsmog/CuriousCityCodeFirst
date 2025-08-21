using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace CuriousCity.Core
{
    /// <summary>
    /// Main AI controller for Chrona character
    /// </summary>
    public class ChronaAI : MonoBehaviour
    {
        [Header("Personality State")]
        public ChronaPersonalityState personality;
        
        [Header("UI Elements")]
        public GameObject hologramContainer;
        public TextMeshProUGUI dialogueText;
        public Image hologramVisual;
        public Slider empathySlider;
        public Slider logicSlider;
        public Button continueButton;
        
        [Header("Visual Representation")]
        public Color empathyColor = Color.cyan;
        public Color logicColor = Color.yellow;
        public ParticleSystem hologramParticles;
        public Animator hologramAnimator;
        
        [Header("Audio")]
        public AudioSource voiceSource;
        public List<AudioClip> empathyVoiceClips;
        public List<AudioClip> logicVoiceClips;
        
        [Header("Dialogue Database")]
        public ChronaDialogueDatabase dialogueDatabase;
        
        // Current state
        private Queue<DialogueEntry> currentDialogueQueue;
        private bool isActivelyTalking = false;
        private string currentMissionContext = "";
        
        // Events
        public static event System.Action<float, float> OnPersonalityChange;
        public static event System.Action<string> OnDialogueComplete;
        
        private void Start()
        {
            InitializeChrona();
        }
        
        private void InitializeChrona()
        {
            if (personality == null)
            {
                personality = new ChronaPersonalityState();
            }
            
            currentDialogueQueue = new Queue<DialogueEntry>();
            
            // Set up UI
            if (continueButton)
            {
                continueButton.onClick.AddListener(AdvanceDialogue);
            }
            
            // Initialize visual representation
            UpdateVisualRepresentation();
            
            // Load dialogue database if not assigned
            if (dialogueDatabase == null)
            {
                dialogueDatabase = Resources.Load<ChronaDialogueDatabase>("ChronaDialogues");
            }
        }
        
        public void ActivateHologram()
        {
            if (hologramContainer)
            {
                hologramContainer.SetActive(true);
            }
            
            // Play activation animation
            if (hologramAnimator)
            {
                hologramAnimator.SetTrigger("Activate");
            }
            
            // Start particle effects
            if (hologramParticles)
            {
                hologramParticles.Play();
            }
            
            // Greeting based on current personality
            TriggerDialogue("greeting");
        }
        
        public void TriggerDialogue(string dialogueId)
        {
            if (dialogueDatabase == null) return;
            
            var dialogue = dialogueDatabase.GetDialogue(dialogueId, personality);
            if (dialogue != null && dialogue.Count > 0)
            {
                currentDialogueQueue.Clear();
                foreach (var entry in dialogue)
                {
                    currentDialogueQueue.Enqueue(entry);
                }
                
                StartDialogue();
            }
        }
        
        private void StartDialogue()
        {
            if (currentDialogueQueue.Count > 0 && !isActivelyTalking)
            {
                isActivelyTalking = true;
                DisplayNextDialogue();
            }
        }
        
        private void DisplayNextDialogue()
        {
            if (currentDialogueQueue.Count == 0)
            {
                EndDialogue();
                return;
            }
            
            var entry = currentDialogueQueue.Dequeue();
            
            if (dialogueText)
            {
                dialogueText.text = entry.text;
            }
            
            // Set visual tone
            SetEmotionalTone(entry.emotionalTone);
            
            // Auto-advance if not waiting for input
            if (!entry.waitForInput)
            {
                Invoke(nameof(DisplayNextDialogue), entry.displayDuration);
            }
        }
        
        private void SetEmotionalTone(EmotionalTone tone)
        {
            Color targetColor = Color.white;
            
            switch (tone)
            {
                case EmotionalTone.Empathetic:
                    targetColor = empathyColor;
                    break;
                case EmotionalTone.Logical:
                    targetColor = logicColor;
                    break;
                case EmotionalTone.Curious:
                    targetColor = Color.green;
                    break;
                case EmotionalTone.Concerned:
                    targetColor = Color.orange;
                    break;
                case EmotionalTone.Excited:
                    targetColor = Color.magenta;
                    break;
            }
            
            if (hologramVisual)
            {
                hologramVisual.color = targetColor;
            }
        }
        
        public void AdvanceDialogue()
        {
            if (isActivelyTalking)
            {
                DisplayNextDialogue();
            }
        }
        
        private void EndDialogue()
        {
            isActivelyTalking = false;
            OnDialogueComplete?.Invoke(currentMissionContext);
        }
        
        private void UpdateVisualRepresentation()
        {
            if (empathySlider && logicSlider && personality != null)
            {
                empathySlider.value = personality.empathyLevel;
                logicSlider.value = personality.logicalCore;
            }
        }
        
        public void UpdatePersonality(bool empatheticAction)
        {
            if (personality != null)
            {
                personality.UpdatePersonality(empatheticAction);
                UpdateVisualRepresentation();
                OnPersonalityChange?.Invoke(personality.empathyLevel, personality.logicalCore);
            }
        }
        
        public void SetMissionContext(string context)
        {
            currentMissionContext = context;
        }
        
        public static GameObject SpawnChrona(Vector3 position)
        {
            GameObject chronaGO = new GameObject("Chrona");
            chronaGO.transform.position = position;
            
            // Add visual representation
            GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            visual.transform.SetParent(chronaGO.transform);
            visual.transform.localScale = new Vector3(0.5f, 1f, 0.5f);
            
            // Make holographic
            Renderer renderer = visual.GetComponent<Renderer>();
            renderer.material.color = new Color(0, 1, 1, 0.5f);
            
            // Add AI component
            chronaGO.AddComponent<ChronaPersonalityState>();
            
            return chronaGO;
        }
    }

    /// <summary>
    /// Represents Chrona's personality state and development
    /// </summary>
    [System.Serializable]
    public class ChronaPersonalityState 
    {
        [Header("Core Traits")]
        public float logicalCore = 1.0f;
        public float empathyLevel = 0.1f;
        
        [Header("Development Tracking")]
        public int totalInteractions = 0;
        public float curiosityLevel = 0.5f;
        public float trustLevel = 0.3f;
        
        public void UpdatePersonality(bool empatheticAction) 
        {
            totalInteractions++;
            
            if (empatheticAction) 
            {
                empathyLevel += 0.1f;
                trustLevel += 0.05f;
            }
            else 
            {
                logicalCore += 0.05f;
            }
            
            // Increase curiosity based on variety of interactions
            curiosityLevel += 0.02f;
            
            // Cap values
            empathyLevel = Mathf.Clamp(empathyLevel, 0f, 2.0f);
            logicalCore = Mathf.Clamp(logicalCore, 0f, 2.0f);
            curiosityLevel = Mathf.Clamp01(curiosityLevel);
            trustLevel = Mathf.Clamp01(trustLevel);
        }
        
        public bool IsEmpathyDominant()
        {
            return empathyLevel > logicalCore;
        }
        
        public float GetPersonalityBalance()
        {
            return empathyLevel / (empathyLevel + logicalCore);
        }
    }

    /// <summary>
    /// Dialogue database for Chrona's responses
    /// </summary>
    [CreateAssetMenu(fileName = "ChronaDialogueDatabase", menuName = "Curious City/Chrona Dialogue Database")]
    public class ChronaDialogueDatabase : ScriptableObject
    {
        public List<DialogueSet> dialogueSets;
        
        public List<DialogueEntry> GetDialogue(string id, ChronaPersonalityState personality)
        {
            var set = dialogueSets.Find(s => s.id == id);
            
            if (set != null)
            {
                // Choose variant based on personality
                if (personality.IsEmpathyDominant() && set.empathyVariant.Count > 0)
                {
                    return set.empathyVariant;
                }
                else if (set.logicVariant.Count > 0)
                {
                    return set.logicVariant;
                }
                else
                {
                    return set.defaultDialogue;
                }
            }
            
            return new List<DialogueEntry>();
        }
    }

    [System.Serializable]
    public class DialogueSet
    {
        public string id;
        public List<DialogueEntry> defaultDialogue;
        public List<DialogueEntry> empathyVariant;
        public List<DialogueEntry> logicVariant;
    }

    [System.Serializable]
    public class DialogueEntry
    {
        [TextArea(3, 5)]
        public string text;
        public EmotionalTone emotionalTone;
        public bool waitForInput = true;
        public float displayDuration = 3f;
    }

    public enum EmotionalTone
    {
        Neutral,
        Empathetic,
        Logical,
        Curious,
        Concerned,
        Excited
    }
}