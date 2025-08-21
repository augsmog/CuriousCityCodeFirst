// MissionDataManager.cs - Updated to work with your existing scripts
using UnityEngine;
using System;
using System.Collections.Generic;
using CuriousCityAutomated.Data;
using CuriousCity.Core;
using System.Linq;  // Required for LINQ methods like .Count() and .Any()
using CuriousCityAutomated.Analytics;
using PuzzleResults = CuriousCity.Core.PuzzleResults;

namespace CuriousCityAutomated.Core
{
    /// <summary>
    /// MonoBehaviour manager that handles MissionResults and PuzzleResults data collection.
    /// Updated to work with existing static events in HistoricalMissionSceneManager and PuzzleManager.
    /// </summary>
    public class MissionDataManager : MonoBehaviour
    {
        private static MissionDataManager instance;
        public static MissionDataManager Instance
        {
            get
            {
                if (instance == null)
                    instance = FindFirstObjectByType<MissionDataManager>();
                return instance;
            }
        }

        [Header("Data Storage")]
        [SerializeField] private MissionResults currentMissionResults;
        [SerializeField] private List<PuzzleResults> currentPuzzleResults = new List<PuzzleResults>();

        [Header("References")]
        [SerializeField] private HistoricalMissionSceneManager missionManager;
        [SerializeField] private PuzzleManager puzzleManager;
        [SerializeField] private LearningStyleTracker learningTracker;

        [Header("Mission Configuration")]
        [SerializeField] private string currentMissionId = "egypt_nile_01";
        [SerializeField] private string currentMissionName = "Ancient Egypt - Nile Exploration";
        [SerializeField] private string historicalPeriod = "Ancient Egypt";
        [SerializeField] private int totalPuzzlesInMission = 3;

        // Track mission start time
        private float missionStartTime;
        
        // Track active puzzle data
        private Dictionary<string, PuzzleResults> activePuzzleData = new Dictionary<string, PuzzleResults>();
        private Dictionary<string, float> puzzleStartTimes = new Dictionary<string, float>();

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }
            instance = this;

            // Initialize mission results
            InitializeMissionResults();
        }

        private void OnEnable()
        {
            // Subscribe to existing static events
            HistoricalMissionSceneManager.OnPuzzleCompleted += OnPuzzleCompletedStatic;
            HistoricalMissionSceneManager.OnZoneExplored += OnZoneExploredStatic;
            HistoricalMissionSceneManager.OnMissionCompleted += OnMissionCompletedStatic;
            
            PuzzleManager.OnPuzzleStarted += OnPuzzleStartedStatic;
            PuzzleManager.OnPuzzleCompleted += OnPuzzleCompletedWithResults;
            PuzzleManager.OnPuzzleExited += OnPuzzleExitedStatic;
        }

        private void OnDisable()
        {
            // Unsubscribe from static events
            HistoricalMissionSceneManager.OnPuzzleCompleted -= OnPuzzleCompletedStatic;
            HistoricalMissionSceneManager.OnZoneExplored -= OnZoneExploredStatic;
            HistoricalMissionSceneManager.OnMissionCompleted -= OnMissionCompletedStatic;
            
            PuzzleManager.OnPuzzleStarted -= OnPuzzleStartedStatic;
            PuzzleManager.OnPuzzleCompleted -= OnPuzzleCompletedWithResults;
            PuzzleManager.OnPuzzleExited -= OnPuzzleExitedStatic;
        }

        private void Start()
        {
            // Record mission start time
            missionStartTime = Time.time;
            
            // Initialize with proper mission data if we have a reference
            if (missionManager != null && missionManager.missionData != null)
            {
                currentMissionId = missionManager.missionData.missionId;
                currentMissionName = missionManager.missionData.title;
                historicalPeriod = missionManager.missionData.historicalPeriod;
                totalPuzzlesInMission = missionManager.missionData.requiredPuzzleTypes.Count;
                
                // Update mission results with correct data
                currentMissionResults.missionId = currentMissionId;
                currentMissionResults.missionName = currentMissionName;
                currentMissionResults.historicalPeriod = historicalPeriod;
                currentMissionResults.totalPuzzles = totalPuzzlesInMission;
            }
            
            Debug.Log($"Mission Data Manager initialized for: {currentMissionName}");
        }

        #region Initialization
        private void InitializeMissionResults()
        {
            currentMissionResults = new MissionResults
            {
                missionId = currentMissionId,
                missionName = currentMissionName,
                historicalPeriod = historicalPeriod,
                totalPuzzles = totalPuzzlesInMission,
                completionDate = DateTime.Now,
                playerChoices = new List<PlayerChoice>(),
                unlockedFeatures = new List<string>()
            };

            // Initialize dictionaries
            currentMissionResults.choiceTypeCounts = new Dictionary<string, int>();
            currentMissionResults.learningStyleMetrics = new Dictionary<string, float>
            {
                ["Visual"] = 0f,
                ["Logical"] = 0f,
                ["Verbal"] = 0f,
                ["Kinesthetic"] = 0f
            };

            Debug.Log($"Initialized Mission: {currentMissionName}");
        }
        #endregion

        #region Static Event Handlers
        private void OnPuzzleStartedStatic(string puzzleType)
        {
            Debug.Log($"Puzzle started: {puzzleType}");
            
            // Create new puzzle result entry
            var puzzleResult = new PuzzleResults
            {
                puzzleType = puzzleType,
                wasSolved = false,
                attempts = 0,
                hintsUsed = 0,
                mistakesMade = 0,
                learningStyleTags = new List<string>(),
                interactionMetrics = new Dictionary<string, float>()
            };

            // Add learning style tags based on puzzle type
            switch (puzzleType.ToLower())
            {
                case "chronocircuits":
                    puzzleResult.learningStyleTags.Add("Logical");
                    puzzleResult.learningStyleTags.Add("Mathematical");
                    break;
                case "scrollofsecrets":
                    puzzleResult.learningStyleTags.Add("Verbal");
                    puzzleResult.learningStyleTags.Add("Linguistic");
                    break;
                case "pyramidrebuilder":
                    puzzleResult.learningStyleTags.Add("Visual");
                    puzzleResult.learningStyleTags.Add("Spatial");
                    break;
            }

            // Track active puzzle
            activePuzzleData[puzzleType] = puzzleResult;
            puzzleStartTimes[puzzleType] = Time.time;
            
            // Track interaction count
            currentMissionResults.interactionCount++;
        }

        private void OnPuzzleCompletedWithResults(string puzzleType, PuzzleResults results)
        {
            Debug.Log($"Puzzle completed with results: {puzzleType}");
            
            // Update our tracked data with the results
            if (activePuzzleData.ContainsKey(puzzleType))
            {
                // Merge the PuzzleManager's results with our tracking
                var trackedResult = activePuzzleData[puzzleType];
                results.attempts = Mathf.Max(results.attempts, trackedResult.attempts);
                
                // Calculate completion time if not provided
                if (results.completionTime <= 0 && puzzleStartTimes.ContainsKey(puzzleType))
                {
                    results.completionTime = Time.time - puzzleStartTimes[puzzleType];
                }
            }
            
            // Add to completed puzzles
            currentPuzzleResults.Add(results);
            
            // Update mission metrics
            currentMissionResults.puzzlesCompleted = currentPuzzleResults.Count;
            currentMissionResults.hintsUsed += results.hintsUsed;
            currentMissionResults.mistakesMade += results.mistakesMade;
            
            // Update learning metrics
            UpdateLearningMetrics(results);
            
            // Clean up tracking
            activePuzzleData.Remove(puzzleType);
            puzzleStartTimes.Remove(puzzleType);
        }

        private void OnPuzzleCompletedStatic(string puzzleType)
        {
            // This is called by HistoricalMissionSceneManager
            // Most work is done in OnPuzzleCompletedWithResults
            Debug.Log($"Puzzle completed (from mission manager): {puzzleType}");
        }

        private void OnPuzzleExitedStatic(string puzzleType)
        {
            Debug.Log($"Puzzle exited: {puzzleType}");
            
            // If puzzle was exited without completion, track as incomplete
            if (activePuzzleData.ContainsKey(puzzleType))
            {
                var puzzleResult = activePuzzleData[puzzleType];
                puzzleResult.wasSolved = false;
                puzzleResult.completionTime = Time.time - puzzleStartTimes[puzzleType];
                
                // Still add to results as incomplete
                currentPuzzleResults.Add(puzzleResult);
                
                activePuzzleData.Remove(puzzleType);
                puzzleStartTimes.Remove(puzzleType);
            }
        }

        private void OnZoneExploredStatic(string zoneName)
        {
            Debug.Log($"Zone explored: {zoneName}");
            
            // Track exploration
            currentMissionResults.explorationEfficiency = CalculateExplorationEfficiency();
            
            // This could trigger a player choice
            RecordPlayerChoice(new PlayerChoice(
                $"explore_{zoneName}",
                "exploration",
                $"Explored {zoneName}",
                0.3f
            ));
        }

        private void OnMissionCompletedStatic(MissionResults results)
        {
            Debug.Log("Mission completed - received results from HistoricalMissionSceneManager");
            
            // Merge the provided results with our tracked data
            currentMissionResults.success = results.success;
            currentMissionResults.artifactRecovered = results.artifactRecovered;
            currentMissionResults.artifact = results.artifact;
            
            // Calculate final metrics
            CalculateFinalMetrics();
            
            // Save the complete results
            SaveMissionResults();
            
            Debug.Log($"Mission Summary:\n{currentMissionResults.GenerateSummary()}");
        }
        #endregion

        #region Metrics Calculation
        private void UpdateLearningMetrics(PuzzleResults puzzleResult)
        {
            // Calculate efficiency based on the puzzle results
            float efficiency = CalculatePuzzleEfficiency(puzzleResult);
            
            // Update learning style metrics based on puzzle tags
            foreach (var tag in puzzleResult.learningStyleTags)
            {
                if (currentMissionResults.learningStyleMetrics.ContainsKey(tag))
                {
                    currentMissionResults.learningStyleMetrics[tag] += efficiency;
                }
            }

            // Update engagement level
            currentMissionResults.engagementLevel = CalculateEngagementLevel();
            
            // Notify learning tracker if available
            if (learningTracker != null)
            {
    foreach (var tag in puzzleResult.learningStyleTags)
            {
        // learningTracker.RecordLearningActivity(tag, efficiency);  // ← COMMENT OUT THIS LINE
            }
            }

        }

        private float CalculatePuzzleEfficiency(PuzzleResults puzzle)
        {
            if (!puzzle.wasSolved) return 0f;
            
            float timeEfficiency = Mathf.Clamp01(60f / puzzle.completionTime);
            float accuracyEfficiency = Mathf.Clamp01(1f - (puzzle.mistakesMade * 0.1f));
            float hintPenalty = Mathf.Clamp01(1f - (puzzle.hintsUsed * 0.15f));
            
            return (timeEfficiency + accuracyEfficiency + hintPenalty) / 3f;
        }

        private float CalculateExplorationEfficiency()
        {
            // Simple calculation based on time and interactions
            float timeElapsed = Time.time - missionStartTime;
            float expectedTime = 600f; // 10 minutes expected
            
            return Mathf.Clamp01(expectedTime / timeElapsed);
        }

        private float CalculateEngagementLevel()
        {
            if (currentMissionResults.totalPuzzles == 0) return 0f;
            
            float completionRate = currentMissionResults.puzzlesCompleted / (float)currentMissionResults.totalPuzzles;
            float successRate = currentPuzzleResults.Count > 0 ? 
                currentPuzzleResults.Count(p => p.wasSolved) / (float)currentPuzzleResults.Count : 0f;
            
            return (completionRate + successRate) / 2f;
        }

        private void CalculateFinalMetrics()
        {
            // Calculate total completion time
            currentMissionResults.completionTime = Time.time - missionStartTime;
            
            // Calculate accuracy rate
            int totalAttempts = 0;
            float totalPuzzleTime = 0f;
            
            foreach (var puzzle in currentPuzzleResults)
            {
                totalAttempts += puzzle.attempts;
                totalPuzzleTime += puzzle.completionTime;
            }
            
            currentMissionResults.accuracyRate = totalAttempts > 0 ? 
                currentPuzzleResults.Count(p => p.wasSolved) / (float)totalAttempts : 0f;
            
            // Calculate puzzle solve speed (average)
            currentMissionResults.puzzleSolveSpeed = currentPuzzleResults.Count > 0 ?
                totalPuzzleTime / currentPuzzleResults.Count : 0f;
            
            // Determine dominant learning style
            float maxScore = 0f;
            string dominantStyle = "Balanced";
            foreach (var kvp in currentMissionResults.learningStyleMetrics)
            {
                if (kvp.Value > maxScore)
                {
                    maxScore = kvp.Value;
                    dominantStyle = kvp.Key;
                }
            }
            currentMissionResults.dominantLearningStyle = dominantStyle;
            
            // Set final engagement and confidence levels
            currentMissionResults.engagementLevel = CalculateEngagementLevel();
            currentMissionResults.frustrationLevel = Mathf.Clamp01((currentMissionResults.mistakesMade + currentMissionResults.hintsUsed) / 20f);
            currentMissionResults.confidenceLevel = 1f - currentMissionResults.frustrationLevel;
            
            // Set behavioral insights
            currentMissionResults.showedSystematicApproach = currentMissionResults.mistakesMade < 5;
            currentMissionResults.demonstratedCreativity = currentMissionResults.playerChoices.Any(c => c.choiceType == "creative");
        }
        #endregion

        #region Public Methods
        public void RecordPlayerChoice(PlayerChoice choice)
        {
            if (choice == null) return;
            
            choice.sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            choice.timestamp = Time.time;
            
            currentMissionResults.playerChoices.Add(choice);
            
            // Update choice type counts
            if (!currentMissionResults.choiceTypeCounts.ContainsKey(choice.choiceType))
                currentMissionResults.choiceTypeCounts[choice.choiceType] = 0;
            currentMissionResults.choiceTypeCounts[choice.choiceType]++;
            
            Debug.Log($"Recorded player choice: {choice.choiceType} - {choice.chosenOption}");
        }

        public void RecordPuzzleHint(string puzzleType)
        {
            if (activePuzzleData.ContainsKey(puzzleType))
            {
                activePuzzleData[puzzleType].hintsUsed++;
                currentMissionResults.hintsUsed++;
            }
        }

        public void RecordPuzzleMistake(string puzzleType)
        {
            if (activePuzzleData.ContainsKey(puzzleType))
            {
                activePuzzleData[puzzleType].mistakesMade++;
                currentMissionResults.mistakesMade++;
            }
        }

        public void UpdateArkImpact(float morale, float power, float population, float colonyProgress)
        {
            currentMissionResults.moraleImpact = morale;
            currentMissionResults.powerImpact = power;
            currentMissionResults.populationImpact = population;
            currentMissionResults.colonyProgressImpact = colonyProgress;
        }

        public void UpdateChronaEvolution(float empathyChange, float logicChange, string reaction)
        {
            currentMissionResults.chronaEmpathyChange = empathyChange;
            currentMissionResults.chronaLogicChange = logicChange;
            currentMissionResults.chronaReaction = reaction;
        }

        public MissionResults GetCurrentMissionResults()
        {
            return currentMissionResults;
        }

        public List<PuzzleResults> GetPuzzleResults()
        {
            return currentPuzzleResults;
        }

        public void ResetMissionData()
        {
            InitializeMissionResults();
            currentPuzzleResults.Clear();
            activePuzzleData.Clear();
            puzzleStartTimes.Clear();
        }
        #endregion

        #region Save/Load
        private void SaveMissionResults()
        {
            // Save to GameDataManager
            var gameData = GameDataManager.Instance;
            if (gameData != null)
            {
                gameData.SaveMissionResults(currentMissionResults);
                
                // Also save individual puzzle results
                foreach (var puzzle in currentPuzzleResults)
                {
                    gameData.SavePuzzleResults(puzzle);
                }
            }

            // Log final statistics
            Debug.Log($"Mission Results Saved:\n" +
                     $"Success: {currentMissionResults.success}\n" +
                     $"Puzzles: {currentMissionResults.puzzlesCompleted}/{currentMissionResults.totalPuzzles}\n" +
                     $"Time: {currentMissionResults.completionTime:F1}s\n" +
                     $"Score: {currentMissionResults.CalculateOverallScore():F0}/100");
        }
        #endregion
    }
}