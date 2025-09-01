using System;
using System.Collections.Generic;
using UnityEngine;
using CuriousCity.Data;

// Global dialogue choice used by analytics
public class DialogueChoice
{
    public string choiceText;
    public float relationshipChange;
}

namespace CuriousCity.Core
{
    // Simple crew manager placeholder
    public class CrewManager : MonoBehaviour { }

    // Handles crew relationship events with empty implementations
    public class CrewInteractionManager : MonoBehaviour
    {
        public static event Action<string, float> OnRelationshipChanged;
        public void RefreshCrewStatus() { }
        public void TriggerMissionReactions(MissionResults results) { }
    }

    // Central data manager stub
    public class GameDataManager : MonoBehaviour
    {
        private static GameDataManager _instance;
        public static GameDataManager Instance => _instance;
        private void Awake() { if (_instance == null) _instance = this; }
        public object GetCurrentSaveData() => null;
    }

    // Placeholder for Ark systems control
    public class ArkSystemsManager : MonoBehaviour
    {
        public static event Action OnSystemUpdate;
        public Dictionary<string, object> GetSystemStates() => new();
        public void ProcessMissionResults(MissionResults results) { }
    }

    // Bridge console manager stub
    public class TimeConsoleManager : MonoBehaviour
    {
        public static event Action<string> OnMissionSelected;
        public void HighlightAvailableMissions() { }
        public void RefreshMissionList() { }
    }

    // Placeholder for upgrade manager
    public class ArkUpgradeManager : MonoBehaviour
    {
        public void RefreshUpgradeOptions() { }
        public void IntegrateArtifact(string artifactType) { }
    }

    // Basic historical mission scene manager used by many systems
    public class HistoricalMissionSceneManager : MonoBehaviour
    {
        public HistoricalMissionData missionData;

        public static event Action<string> OnPuzzleCompleted;
        public static event Action<string> OnZoneExplored;
        public static event Action<MissionResults> OnMissionCompleted;

        public bool CanAccessArtifact() => true;
        public void RegisterPuzzleCompletion(string puzzleType, PuzzleResults results)
            => OnPuzzleCompleted?.Invoke(puzzleType);
        public void RegisterZoneExploration(string zone)
            => OnZoneExplored?.Invoke(zone);
        public void CompleteMission(MissionResults results)
            => OnMissionCompleted?.Invoke(results);
        public void OnPuzzleTriggered(string puzzleType, Vector3 position) { }
    }
}

// Provide alias namespace used in some scripts
namespace CuriousCity.Gameplay.Puzzles
{
    public class HistoricalMissionSceneManager : CuriousCity.Core.HistoricalMissionSceneManager { }
}
