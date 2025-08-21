using UnityEngine;
using UnityEngine.SceneManagement;

namespace CuriousCity.Core
{
    /// <summary>
    /// Main entry point for the game. Runs before any scene loads.
    /// </summary>
    public class GameBootstrapper
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void OnGameStart()
        {
            Debug.Log("[GameBootstrapper] Initializing Curious City...");
            
            // Create persistent managers
            CreatePersistentSystems();
            
            // Load initial scene
            if (SceneManager.GetActiveScene().name == "")
            {
                SceneBuilder.BuildMainMenu();
            }
        }
        
        static void CreatePersistentSystems()
        {
            // Create persistent GameObject
            GameObject persistent = new GameObject("_PersistentSystems");
            Object.DontDestroyOnLoad(persistent);
            
            // Add core managers
            persistent.AddComponent<GameManager>();
            persistent.AddComponent<SceneBuilder>();
            persistent.AddComponent<AudioManager>();
            persistent.AddComponent<InputManager>();
            
            Debug.Log("[GameBootstrapper] Core systems initialized");
        }
    }
}