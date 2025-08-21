using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using CuriousCityAutomated.Data;  // For MissionResults
using CuriousCityAutomated.Analytics;

/// <summary>
/// Core manager for the Ark Bridge Scene - the main hub where players interact with ship systems,
/// Chrona, crew members, and the time machine console. Handles the primary investor demo experience.
/// </summary>
public class ArkBridgeManager : MonoBehaviour 
{
    private static ArkBridgeManager _instance;
    public static ArkBridgeManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<ArkBridgeManager>();
                if (_instance == null)
                {
                    GameObject go = new GameObject("ArkBridgeManager");
                    _instance = go.AddComponent<ArkBridgeManager>();
                    DontDestroyOnLoad(go);
                }
            }
            return _instance;
        }
    }
    
    void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        DontDestroyOnLoad(gameObject);
    }
    [Header("UI Panels")]
    public GameObject commandInterfacePanel;
    public GameObject chronaHologramPanel; 
    public GameObject timeMachinePanel;
    public GameObject crewInteractionPanel;
    public GameObject upgradeStationPanel;
    public GameObject postMissionPanel;

    [Header("Core Systems")]
    public ArkSystemsManager arkSystems;
    public ChronaAI chronaAI;
    public TimeConsoleManager timeConsole;
    public CrewInteractionManager crewManager;
    public ArkUpgradeManager upgradeManager;

    [Header("UI Elements - Command Interface")]
    public Slider moraleMeter;
    public Slider powerMeter;
    public Slider populationMeter;
    public Slider colonyProgressMeter;
    public TextMeshProUGUI statusText;
    public TextMeshProUGUI missionCountText;

    [Header("Audio")]
    public AudioSource ambientSource;
    public AudioClip arkHumSound;
    public AudioClip alertSound;
    public AudioClip upgradeSound;

    [Header("Analytics Integration")]
    public LearningStyleTracker learningTracker;
    
    // Game State
    public bool isFirstTime = true;
    public bool missionInProgress = false;
    public int totalMissionsCompleted = 0;
    
    // Analytics tracking variables
    private float panelSwitchTime;
    private Dictionary<string, float> panelTimeSpent;
    private Dictionary<string, int> panelVisitCount;
    private string currentActivePanel = "";
    private float decisionStartTime;
    
    // Events
    public static event System.Action<string> OnStatusUpdate;
    public static event System.Action<bool> OnMissionStateChange;
    public static event System.Action OnArkUpgrade;

    private void Start()
    {
        InitializeArkBridge();
        
        if (isFirstTime)
        {
            StartCoroutine(FirstTimeIntroSequence());
        }
        else
        {
            RefreshAllSystems();
        }
    }

    private void InitializeArkBridge()
    {
        // Initialize all core systems
        if (arkSystems == null) arkSystems = FindFirstObjectByType<ArkSystemsManager>();
        if (chronaAI == null) chronaAI = FindFirstObjectByType<ChronaAI>();
        if (timeConsole == null) timeConsole = FindFirstObjectByType<TimeConsoleManager>();
        if (crewManager == null) crewManager = FindFirstObjectByType<CrewInteractionManager>();
        if (upgradeManager == null) upgradeManager = FindFirstObjectByType<ArkUpgradeManager>();
        if (learningTracker == null) learningTracker = FindFirstObjectByType<LearningStyleTracker>();
        
        // Initialize analytics tracking
        panelTimeSpent = new Dictionary<string, float>();
        panelVisitCount = new Dictionary<string, int>();
        panelSwitchTime = Time.time;

        // Set up UI
        SetupCommandInterface();
        
        // Subscribe to events
        ArkSystemsManager.OnSystemUpdate += UpdateSystemDisplays;
        TimeConsoleManager.OnMissionSelected += StartMission;
        ChronaAI.OnPersonalityChange += HandleChronaEvolution;
        
        // Start ambient audio
        if (ambientSource && arkHumSound)
        {
            ambientSource.clip = arkHumSound;
            ambientSource.loop = true;
            ambientSource.Play();
        }
    }

    private void SetupCommandInterface()
    {
        // Initialize UI panels - start with command interface active
        ShowPanel("Command");
        
        // Update initial displays
        RefreshSystemMeters();
    }

    private IEnumerator FirstTimeIntroSequence()
    {
        // Investor demo intro sequence
        isFirstTime = false;
        
        yield return new WaitForSeconds(1f);
        
        // Chrona introduction
        if (chronaAI)
        {
            chronaAI.TriggerDialogue("intro_welcome");
        }
        
        yield return new WaitForSeconds(3f);
        
        // Show command interface tutorial
        UpdateStatusText("Welcome aboard the Ark. Your mission: recover knowledge to ensure our survival.");
        
        yield return new WaitForSeconds(2f);
        
        // Highlight time machine for first mission
        if (timeConsole)
        {
            timeConsole.HighlightAvailableMissions();
        }
    }

    public void ShowPanel(string panelName)
    {
        // Track time spent in previous panel
        if (!string.IsNullOrEmpty(currentActivePanel) && learningTracker)
        {
            float timeSpent = Time.time - panelSwitchTime;
            
            if (!panelTimeSpent.ContainsKey(currentActivePanel))
                panelTimeSpent[currentActivePanel] = 0f;
            panelTimeSpent[currentActivePanel] += timeSpent;
            
            learningTracker.LogUIInteraction(currentActivePanel, "panel_exit", timeSpent);
        }
        
        // Hide all panels first
        commandInterfacePanel?.SetActive(false);
        chronaHologramPanel?.SetActive(false);
        timeMachinePanel?.SetActive(false);
        crewInteractionPanel?.SetActive(false);
        upgradeStationPanel?.SetActive(false);
        postMissionPanel?.SetActive(false);

        // Track new panel access
        if (!panelVisitCount.ContainsKey(panelName))
            panelVisitCount[panelName] = 0;
        panelVisitCount[panelName]++;
        
        currentActivePanel = panelName;
        panelSwitchTime = Time.time;
        
        // Log UI navigation decision
        if (learningTracker)
        {
            learningTracker.LogDecision("ui_navigation", panelName, 0.1f, 
                new List<string> {"Command", "Chrona", "TimeMachine", "Crew", "Upgrade"});
            
            learningTracker.LogUIInteraction(panelName, "panel_enter", 0f);
            
            // Analyze UI preferences for learning styles
            AnalyzeUIPreference(panelName);
        }

        // Show requested panel
        switch (panelName.ToLower())
        {
            case "command":
                commandInterfacePanel?.SetActive(true);
                RefreshSystemMeters();
                break;
            case "chrona":
                chronaHologramPanel?.SetActive(true);
                if (chronaAI) chronaAI.ActivateHologram();
                break;
            case "timemachine":
                timeMachinePanel?.SetActive(true);
                if (timeConsole) timeConsole.RefreshMissionList();
                break;
            case "crew":
                crewInteractionPanel?.SetActive(true);
                if (crewManager) crewManager.RefreshCrewStatus();
                break;
            case "upgrade":
                upgradeStationPanel?.SetActive(true);
                if (upgradeManager) upgradeManager.RefreshUpgradeOptions();
                break;
            case "postmission":
                postMissionPanel?.SetActive(true);
                break;
        }
    }
    
    private void AnalyzeUIPreference(string panelName)
    {
        // Analyze what the panel choice indicates about learning preferences
        switch (panelName.ToLower())
        {
            case "chrona":
                // Seeking AI interaction suggests interpersonal or intrapersonal preference
                learningTracker.LogDetailedEvent("ai_interaction_sought", "Player chose to interact with Chrona AI", "social_learning");
                break;
                
            case "command":
                // Data-focused interface suggests logical-mathematical preference
                learningTracker.LogDetailedEvent("data_interface_preferred", "Player chose command/data interface", "analytical_learning");
                break;
                
            case "crew":
                // Human interaction suggests interpersonal learning preference
                learningTracker.LogDetailedEvent("social_interaction_sought", "Player chose crew interaction", "social_learning");
                break;
                
            case "upgrade":
                // System building suggests logical or kinesthetic preference
                learningTracker.LogDetailedEvent("system_building_interest", "Player chose upgrade systems", "constructive_learning");
                break;
                
            case "timemachine":
                // Mission selection suggests goal-oriented, possibly visual-spatial
                learningTracker.LogDetailedEvent("mission_planning_focus", "Player chose mission selection", "strategic_learning");
                break;
        }
    }

    private void RefreshAllSystems()
    {
        RefreshSystemMeters();
        
        if (arkSystems)
        {
            UpdateStatusText($"Systems operational. {totalMissionsCompleted} missions completed.");
        }
        
        if (missionCountText)
        {
            missionCountText.text = $"Missions: {totalMissionsCompleted}";
        }
    }

    private void RefreshSystemMeters()
    {
        if (arkSystems == null) return;

        var systems = arkSystems.GetSystemStates();
        
        if (moraleMeter)
            moraleMeter.value = systems.morale;
        if (powerMeter)
            powerMeter.value = systems.power;
        if (populationMeter)
            populationMeter.value = systems.population;
        if (colonyProgressMeter)
            colonyProgressMeter.value = systems.colonyProgress;
    }

    private void UpdateSystemDisplays(ArkSystemState systems)
    {
        RefreshSystemMeters();
        
        // Update status based on critical systems
        if (systems.power < 0.3f)
        {
            UpdateStatusText("CRITICAL: Power systems failing. Find energy artifacts urgently.");
            PlayAlert();
        }
        else if (systems.morale < 0.4f)
        {
            UpdateStatusText("WARNING: Crew morale low. Consider cultural missions.");
        }
        else
        {
            UpdateStatusText("All systems stable. Ready for time travel missions.");
        }
    }

    private void UpdateStatusText(string message)
    {
        if (statusText)
        {
            statusText.text = message;
        }
        
        OnStatusUpdate?.Invoke(message);
    }

    private void StartMission(string missionId)
    {
        if (missionInProgress) return;
        
        // Track mission selection timing and decision process
        float selectionTime = Time.time - decisionStartTime;
        
        if (learningTracker)
        {
            learningTracker.LogDecision("mission_selection", missionId, selectionTime,
                new List<string> {"egypt_exploration", "delayed_decision", "skip_mission"});
            
            learningTracker.LogDetailedEvent("mission_initiated", $"Player selected mission: {missionId}", "goal_setting",
                new Dictionary<string, object>
                {
                    {"mission_id", missionId},
                    {"selection_time", selectionTime},
                    {"total_missions_completed", totalMissionsCompleted},
                    {"is_first_mission", totalMissionsCompleted == 0}
                });
            
            // Analyze mission selection pattern
            AnalyzeMissionSelectionBehavior(missionId, selectionTime);
        }
        
        missionInProgress = true;
        OnMissionStateChange?.Invoke(true);
        
        // Inform Chrona about mission start
        if (chronaAI)
        {
            chronaAI.OnMissionStarted(missionId);
        }
        
        UpdateStatusText($"Mission {missionId} initiated. Good luck.");
        
        // Load mission scene (implement based on your scene management)
        StartCoroutine(LoadMissionScene(missionId));
    }
    
    private void AnalyzeMissionSelectionBehavior(string missionId, float selectionTime)
    {
        // Quick selection suggests confidence or impulsivity
        if (selectionTime < 5f)
        {
            learningTracker.LogDetailedEvent("quick_mission_selection", "Fast mission choice indicates confidence", "decision_confidence");
        }
        else if (selectionTime > 30f)
        {
            learningTracker.LogDetailedEvent("deliberate_mission_selection", "Extended consideration indicates thoughtful approach", "careful_planning");
        }
        
        // First mission selection is particularly informative
        if (totalMissionsCompleted == 0)
        {
            learningTracker.LogDetailedEvent("first_mission_approach", "Player's first mission selection pattern", "initial_engagement",
                new Dictionary<string, object>
                {
                    {"shows_hesitation", selectionTime > 15f},
                    {"shows_confidence", selectionTime < 10f},
                    {"explored_options", panelVisitCount.GetValueOrDefault("timemachine", 0) > 1}
                });
        }
    }

    private IEnumerator LoadMissionScene(string missionId)
    {
        yield return new WaitForSeconds(1f);
        
        // Add scene transition logic here
        // For MVP, this might just switch to EgyptExplorationScene
        if (missionId == "egypt_exploration")
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("EgyptExplorationScene");
        }
    }

    public void OnMissionCompleted(string missionId, MissionResults results)
    {
        missionInProgress = false;
        totalMissionsCompleted++;
        
        OnMissionStateChange?.Invoke(false);
        
        // Process mission results
        if (arkSystems)
        {
            arkSystems.ProcessMissionResults(results);
        }
        
        // Update Chrona's personality based on mission choices
        if (chronaAI)
        {
            chronaAI.ProcessMissionChoices(results.playerChoices);
        }
        
        // Show post-mission sequence
        StartCoroutine(PostMissionSequence(results));
    }

    private IEnumerator PostMissionSequence(MissionResults results)
    {
        ShowPanel("PostMission");
        
        yield return new WaitForSeconds(1f);
        
        // Artifact integration ceremony
        if (results.artifactRecovered)
        {
            yield return StartCoroutine(ArtifactIntegrationCeremony(results.artifact));
        }
        
        // Crew reactions
        if (crewManager)
        {
            crewManager.TriggerMissionReactions(results);
        }
        
        // Chrona evolution dialogue
        if (chronaAI)
        {
            chronaAI.TriggerMissionReflection(results);
        }
        
        yield return new WaitForSeconds(3f);
        
        // Return to command interface
        ShowPanel("Command");
        RefreshAllSystems();
    }

    private IEnumerator ArtifactIntegrationCeremony(ArtifactData artifact)
    {
        UpdateStatusText($"Integrating {artifact.name} into ship systems...");
        
        // Visual effects for artifact integration
        PlayUpgradeSound();
        
        yield return new WaitForSeconds(2f);
        
        // Apply artifact benefits
        if (upgradeManager)
        {
            upgradeManager.IntegrateArtifact(artifact);
        }
        
        OnArkUpgrade?.Invoke();
        
        UpdateStatusText($"{artifact.name} successfully integrated. {artifact.systemBenefit}");
        
        yield return new WaitForSeconds(2f);
    }

    private void HandleChronaEvolution(float empathy, float logic)
    {
        // Visual updates to Chrona's hologram based on personality changes
        if (chronaAI)
        {
            UpdateStatusText($"Chrona evolving... Empathy: {empathy:F1}, Logic: {logic:F1}");
        }
    }

    private void PlayAlert()
    {
        if (ambientSource && alertSound)
        {
            ambientSource.PlayOneShot(alertSound);
        }
    }

    private void PlayUpgradeSound()
    {
        if (ambientSource && upgradeSound)
        {
            ambientSource.PlayOneShot(upgradeSound);
        }
    }

    // Public API for UI buttons with enhanced analytics
    public void OnCommandInterfaceClicked() 
    {
        if (learningTracker) learningTracker.LogUIInteraction("command_button", "click");
        ShowPanel("Command");
    }
    
    public void OnChronaHologramClicked() 
    {
        if (learningTracker) learningTracker.LogUIInteraction("chrona_button", "click");
        ShowPanel("Chrona");
    }
    
    public void OnTimeMachineClicked() 
    {
        if (learningTracker) learningTracker.LogUIInteraction("timemachine_button", "click");
        decisionStartTime = Time.time; // Start tracking mission selection decision time
        ShowPanel("TimeMachine");
    }
    
    public void OnCrewInteractionClicked() 
    {
        if (learningTracker) learningTracker.LogUIInteraction("crew_button", "click");
        ShowPanel("Crew");
    }
    
    public void OnUpgradeStationClicked() 
    {
        if (learningTracker) learningTracker.LogUIInteraction("upgrade_button", "click");
        ShowPanel("Upgrade");
    }

    private void OnDestroy()
    {
        // Unsubscribe from events
        ArkSystemsManager.OnSystemUpdate -= UpdateSystemDisplays;
        TimeConsoleManager.OnMissionSelected -= StartMission;
        ChronaAI.OnPersonalityChange -= HandleChronaEvolution;
    }
}