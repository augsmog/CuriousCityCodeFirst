// ChronaReactionHelper.cs
using UnityEngine;
using CuriousCityAutomated.Data;
using CuriousCityAutomated.Core;
using CuriousCity.Core;
using System.Linq;

namespace CuriousCity.Core
{
    /// <summary>
    /// Helper to make Chrona react to mission data
    /// </summary>
    public class ChronaReactionHelper : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private ChronaFieldPresence chronaPresence;
        [SerializeField] private MissionDataManager dataManager;

        private void OnEnable()
        {
            // Subscribe to mission completion
            HistoricalMissionSceneManager.OnMissionCompleted += OnMissionCompleted;
        }

        private void OnDisable()
        {
            HistoricalMissionSceneManager.OnMissionCompleted -= OnMissionCompleted;
        }

        private void OnMissionCompleted(MissionResults results)
        {
            if (chronaPresence == null || dataManager == null) return;

            // Update Chrona's evolution based on player choices
            float empathyChange = 0f;
            float logicChange = 0f;

            // Analyze player choices
            foreach (var choice in results.playerChoices)
            {
                switch (choice.GetCategory())
                {
                    case ChoiceCategory.Emotional:
                        empathyChange += 0.1f;
                        break;
                    case ChoiceCategory.Logical:
                        logicChange += 0.1f;
                        break;
                }
            }

            // Determine Chrona's reaction
            string reaction = GenerateChronaReaction(results, empathyChange, logicChange);
            
            // Update mission data with Chrona's evolution
            dataManager.UpdateChronaEvolution(empathyChange, logicChange, reaction);
            
            // Trigger appropriate dialogue
            if (empathyChange > logicChange)
            {
                chronaPresence.TriggerDialogue("mission_complete_empathetic");
            }
            else if (logicChange > empathyChange)
            {
                chronaPresence.TriggerDialogue("mission_complete_logical");
            }
            else
            {
                chronaPresence.TriggerDialogue("mission_complete_balanced");
            }
        }

        private string GenerateChronaReaction(MissionResults results, float empathyChange, float logicChange)
        {
            float score = results.CalculateOverallScore();
            
            if (score > 80)
            {
                return "Impressed by exceptional performance";
            }
            else if (score > 60)
            {
                return "Pleased with solid execution";
            }
            else if (score > 40)
            {
                return "Encouraging despite challenges";
            }
            else
            {
                return "Supportive and offering guidance";
            }
        }
    }
}