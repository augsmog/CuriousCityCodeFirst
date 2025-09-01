using UnityEngine;
using System.Collections.Generic;

namespace CuriousCity.Core
{
    /// <summary>
    /// Builds scenes procedurally from definitions
    /// </summary>
    public class SceneBuilder : MonoBehaviour
    {
        private static SceneBuilder _instance;
        public static SceneBuilder Instance => _instance;
        
        void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
            }
        }
        
        public static void BuildMainMenu()
        {
            ClearScene();
            
            // Create UI camera
            GameObject cameraGO = new GameObject("UI Camera");
            Camera camera = cameraGO.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.1f, 0.1f, 0.2f);
            
            Debug.Log("[SceneBuilder] Main menu built");
        }
        
        public static void BuildEgyptMission()
        {
            ClearScene();
            
            // Create environment
            GameObject environment = new GameObject("Environment");
            
            // Create sun
            GameObject sun = new GameObject("Directional Light");
            Light sunLight = sun.AddComponent<Light>();
            sunLight.type = LightType.Directional;
            sunLight.color = new Color(1f, 0.95f, 0.8f);
            sunLight.intensity = 1.2f;
            sun.transform.rotation = Quaternion.Euler(45f, -30f, 0);
            
            // Create ground
            GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Desert Floor";
            ground.transform.localScale = new Vector3(50, 1, 50);
            ground.GetComponent<Renderer>().sharedMaterial.color = new Color(0.76f, 0.70f, 0.50f);

            
            // Create pyramids
            for (int i = 0; i < 3; i++)
            {
                CreatePyramid(new Vector3(i * 20 - 20, 0, 20));
            }
            
            // Create puzzle triggers
            CreatePuzzleTrigger("ChronoCircuits", new Vector3(0, 1, 0));
            CreatePuzzleTrigger("ScrollSecrets", new Vector3(10, 1, 0));
            CreatePuzzleTrigger("PyramidRebuilder", new Vector3(-10, 1, 0));
            
            // Spawn player
            SpawnPlayer(Vector3.zero + Vector3.up);
            
            Debug.Log("[SceneBuilder] Egypt mission built");
        }
        
        static void CreatePyramid(Vector3 position)
        {
            GameObject pyramid = GameObject.CreatePrimitive(PrimitiveType.Cube);
            pyramid.name = "Pyramid";
            pyramid.transform.position = position + Vector3.up * 5;
            pyramid.transform.localScale = new Vector3(10, 10, 10);
            pyramid.GetComponent<Renderer>().sharedMaterial.color = new Color(0.8f, 0.75f, 0.6f);

        }
        
        static void CreatePuzzleTrigger(string puzzleType, Vector3 position)
        {
            GameObject trigger = new GameObject($"{puzzleType}_Trigger");
            trigger.transform.position = position;
            
            // Add visual representation
            GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Cube);
            visual.name = "Visual";
            visual.transform.SetParent(trigger.transform);
            visual.transform.localPosition = Vector3.zero;
            visual.transform.localScale = new Vector3(2, 2, 2);
            
            // Add trigger collider
            SphereCollider collider = trigger.AddComponent<SphereCollider>();
            collider.radius = 3f;
            collider.isTrigger = true;
            
            // Add puzzle component
            PuzzleTrigger puzzle = trigger.AddComponent<PuzzleTrigger>();
            puzzle.puzzleType = puzzleType;
            
            // Color code by type
            Color color = puzzleType switch
            {
                "ChronoCircuits" => Color.cyan,
                "ScrollSecrets" => Color.yellow,
                "PyramidRebuilder" => Color.magenta,
                _ => Color.white
            };
            visual.GetComponent<Renderer>().sharedMaterial.color = color;

        }
        
        static void SpawnPlayer(Vector3 position)
        {
            // Create player
            GameObject player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            player.name = "Player";
            player.tag = "Player";
            player.transform.position = position;
            
            // Add camera
            GameObject cameraGO = new GameObject("Main Camera");
            cameraGO.tag = "MainCamera";
            cameraGO.transform.SetParent(player.transform);
            cameraGO.transform.localPosition = new Vector3(0, 0.5f, 0);
            Camera camera = cameraGO.AddComponent<Camera>();
            
            // Add character controller
            CharacterController controller = player.AddComponent<CharacterController>();
            controller.height = 2;
            
            // Add player controller
            player.AddComponent<SimplePlayerController>();
            
            // Add audio listener
            cameraGO.AddComponent<AudioListener>();
        }
        
        public static void ClearScene()
        {
            // Destroy all non-persistent objects
            foreach (GameObject go in FindObjectsOfType<GameObject>())
            {
                if (!go.name.StartsWith("_"))
                {
                    DestroyImmediate(go);
                }
            }
        }

        // Simple placeholder for additional missions
        public static void BuildGreeceMission()
        {
            // For now reuse Egypt mission layout
            BuildEgyptMission();
        }
    }
}