using System.Collections.Generic;
using UnityEngine;
using CuriousCityAutomated.Analytics;
using CuriousCityAutomated.Gameplay.Puzzles;

namespace CuriousCity.Data
{
    /// <summary>
    /// ScriptableObject that defines a historical mission's configuration and content.
    /// Used by HistoricalMissionSceneManager to set up mission parameters.
    /// </summary>
    [CreateAssetMenu(fileName = "NewHistoricalMission", menuName = "Curious City/Historical Mission Data", order = 1)]
    public class HistoricalMissionData : ScriptableObject
    {
        [Header("Mission Identity")]
        [SerializeField] private string missionId = "mission_001";
        [SerializeField] private string title = "New Historical Mission";
        [SerializeField] private string historicalPeriod = "Ancient Times";
        [SerializeField] private string location = "Unknown Location";
        
        [Header("Mission Description")]
        [TextArea(3, 5)]
        [SerializeField] private string shortDescription = "A journey through time...";
        [TextArea(5, 10)]
        [SerializeField] private string fullDescription = "Detailed mission description...";
        [TextArea(3, 5)]
        [SerializeField] private string learningObjectives = "Students will learn about...";
        
        [Header("Gameplay Configuration")]
        [SerializeField] private List<string> requiredPuzzleTypes = new List<string>() 
        { 
            "ChronoCircuits", 
            "ScrollOfSecrets", 
            "PyramidRebuilder" 
        };
        [SerializeField] private int difficultyLevel = 1;
        [SerializeField] private float recommendedTimeLimit = 600f; // 10 minutes
        [SerializeField] private List<string> explorationZones = new List<string>()
        {
            "RiverBank",
            "ScribesQuarters",
            "TombChamber"
        };
        
        [Header("Artifact Configuration")]
        [SerializeField] private string artifactType = "Historical";
        [SerializeField] private string artifactName = "Ancient Artifact";
        [SerializeField] private string artifactId = "artifact_001";
        [TextArea(2, 4)]
        [SerializeField] private string artifactDescription = "A mysterious artifact from the past...";
        [SerializeField] private Sprite artifactIcon;
        [SerializeField] private GameObject artifactPrefab;
        
        [Header("Mission Objectives")]
        [SerializeField] private List<MissionObjective> objectives = new List<MissionObjective>();
        
        [Header("Rewards and Unlocks")]
        [SerializeField] private int experienceReward = 100;
        [SerializeField] private int arkMoraleBonus = 10;
        [SerializeField] private int arkPowerBonus = 5;
        [SerializeField] private List<string> unlocksOnCompletion = new List<string>();
        
        [Header("Educational Content")]
        [SerializeField] private List<string> historicalFacts = new List<string>();
        [SerializeField] private List<string> culturalInsights = new List<string>();
        [SerializeField] private string primaryLearningStyle = "Visual";
        
        [Header("Visual and Audio")]
        [SerializeField] private Sprite missionThumbnail;
        [SerializeField] private Color missionThemeColor = Color.white;
        [SerializeField] private AudioClip missionThemeMusic;
        [SerializeField] private AudioClip ambientSoundtrack;
        
        [Header("Scene References")]
        [SerializeField] private string sceneToLoad = "ExplorationScene";
        [SerializeField] private string returnSceneName = "ArkBridgeScene";
        
        // Properties for easy access
        public string MissionId => missionId;
        public string Title => title;
        public string HistoricalPeriod => historicalPeriod;
        public string Location => location;
        public string ShortDescription => shortDescription;
        public string FullDescription => fullDescription;
        public string LearningObjectives => learningObjectives;
        public List<string> RequiredPuzzleTypes => requiredPuzzleTypes;
        public int DifficultyLevel => difficultyLevel;
        public float RecommendedTimeLimit => recommendedTimeLimit;
        public List<string> ExplorationZones => explorationZones;
        public string ArtifactType => artifactType;
        public string ArtifactName => artifactName;
        public string ArtifactId => artifactId;
        public string ArtifactDescription => artifactDescription;
        public Sprite ArtifactIcon => artifactIcon;
        public GameObject ArtifactPrefab => artifactPrefab;
        public List<MissionObjective> Objectives => objectives;
        public int ExperienceReward => experienceReward;
        public int ArkMoraleBonus => arkMoraleBonus;
        public int ArkPowerBonus => arkPowerBonus;
        public List<string> UnlocksOnCompletion => unlocksOnCompletion;
        public List<string> HistoricalFacts => historicalFacts;
        public List<string> CulturalInsights => culturalInsights;
        public string PrimaryLearningStyle => primaryLearningStyle;
        public Sprite MissionThumbnail => missionThumbnail;
        public Color MissionThemeColor => missionThemeColor;
        public AudioClip MissionThemeMusic => missionThemeMusic;
        public AudioClip AmbientSoundtrack => ambientSoundtrack;
        public string SceneToLoad => sceneToLoad;
        public string ReturnSceneName => returnSceneName;
        
        /// <summary>
        /// Get a random historical fact from this mission
        /// </summary>
        public string GetRandomHistoricalFact()
        {
            if (historicalFacts.Count == 0) return "No historical facts available.";
            return historicalFacts[Random.Range(0, historicalFacts.Count)];
        }
        
        /// <summary>
        /// Get a random cultural insight from this mission
        /// </summary>
        public string GetRandomCulturalInsight()
        {
            if (culturalInsights.Count == 0) return "No cultural insights available.";
            return culturalInsights[Random.Range(0, culturalInsights.Count)];
        }
        
        /// <summary>
        /// Check if all required puzzles are in the completed list
        /// </summary>
        public bool AreAllPuzzlesCompleted(List<string> completedPuzzles)
        {
            foreach (string requiredPuzzle in requiredPuzzleTypes)
            {
                if (!completedPuzzles.Contains(requiredPuzzle))
                    return false;
            }
            return true;
        }
        
        /// <summary>
        /// Get objective by index
        /// </summary>
        public MissionObjective GetObjective(int index)
        {
            if (index >= 0 && index < objectives.Count)
                return objectives[index];
            return null;
        }
        
        /// <summary>
        /// Validate the mission data
        /// </summary>
        public bool ValidateMissionData()
        {
            bool isValid = true;
            
            if (string.IsNullOrEmpty(missionId))
            {
                Debug.LogError($"Mission {name} has no ID!");
                isValid = false;
            }
            
            if (requiredPuzzleTypes.Count == 0)
            {
                Debug.LogWarning($"Mission {title} has no required puzzles!");
            }
            
            if (objectives.Count == 0)
            {
                Debug.LogWarning($"Mission {title} has no objectives!");
            }
            
            return isValid;
        }
        
        private void OnValidate()
        {
            // Auto-generate mission ID if empty
            if (string.IsNullOrEmpty(missionId))
            {
                missionId = name.ToLower().Replace(" ", "_");
            }
            
            // Ensure at least one objective exists
            if (objectives.Count == 0)
            {
                objectives.Add(new MissionObjective
                {
                    description = "Complete the mission",
                    isRequired = true
                });
            }
        }
    }
    
    /// <summary>
    /// Individual mission objective data
    /// </summary>
    [System.Serializable]
    public class MissionObjective
    {
        public string objectiveId;
        public string description;
        public bool isRequired = true;
        public bool isCompleted = false;
        public ObjectiveType type = ObjectiveType.Exploration;
        public string targetId; // ID of zone, puzzle, or item to interact with
        
        public enum ObjectiveType
        {
            Exploration,
            PuzzleSolving,
            ArtifactCollection,
            Interaction,
            Discovery
        }
    }
}