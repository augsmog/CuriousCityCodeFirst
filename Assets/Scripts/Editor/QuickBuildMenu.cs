using UnityEngine;
using UnityEditor;
using CuriousCity.Core;

namespace CuriousCity.Editor
{
    public class QuickBuildMenu
    {
        [MenuItem("CuriousCity/Build Egypt Scene")]
        public static void BuildEgyptScene()
        {
            SceneBuilder.BuildEgyptMission();
        }
        
        [MenuItem("CuriousCity/Build Main Menu")]
        public static void BuildMainMenu()
        {
            SceneBuilder.BuildMainMenu();
        }
        
        [MenuItem("CuriousCity/Clear Scene")]
        public static void ClearScene()
        {
            if (EditorUtility.DisplayDialog("Clear Scene", 
                "This will delete all GameObjects in the scene. Continue?", 
                "Yes", "Cancel"))
            {
                foreach (GameObject go in GameObject.FindObjectsByType<GameObject>(FindObjectsSortMode.None))

                {
                    GameObject.DestroyImmediate(go);
                }
            }
        }
    }
}