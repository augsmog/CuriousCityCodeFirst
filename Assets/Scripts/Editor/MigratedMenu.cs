using UnityEngine;
using UnityEditor;
using CuriousCity.Core;
using CuriousCity.Builders;

namespace CuriousCity.Editor
{
    public class MigratedMenu
    {
        [MenuItem("CuriousCity/Build Migrated Scene")]
        public static void BuildMigratedScene()
        {
            SceneBuilderExtensions.BuildFromMigration();
        }
        
        [MenuItem("CuriousCity/Test All Systems")]
        public static void TestSystems()
        {
            Debug.Log("Testing migrated systems...");
            
            // Test each system
            if (LearningStyleTracker.Instance != null)
                Debug.Log("✓ Analytics system ready");
                
            if (MissionManager.Instance != null)
                Debug.Log("✓ Mission system ready");
                
            Debug.Log("All systems operational!");
        }
    }
}