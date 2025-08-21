using UnityEngine;
using CuriousCity.Core;

namespace CuriousCity.Builders
{
    public static class SceneBuilderExtensions
    {
        public static void BuildFromMigration()
        {
            SceneBuilder.BuildEgyptMission();
            
            // Add migrated systems
            GameObject systems = new GameObject("_Systems");
            systems.AddComponent<LearningStyleTracker>();
            systems.AddComponent<MissionManager>();
            systems.AddComponent<CrewManager>();
            
            // Spawn Chrona
            ChronaAI.SpawnChrona(new Vector3(0, 2, 5));
            
            Debug.Log("[Migration] All systems initialized");
        }
    }
}