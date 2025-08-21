using UnityEngine;
using UnityEditor;
using System.IO;

namespace CuriousCity.Setup
{
    public class ProjectInitializer : EditorWindow
    {
        [MenuItem("CuriousCity/Initialize Project Structure")]
        public static void InitializeProject()
        {
            if (EditorUtility.DisplayDialog("Initialize Project", 
                "This will create the complete folder structure and base scripts for a code-first Unity project. Continue?", 
                "Yes", "Cancel"))
            {
                CreateFolderStructure();
                CreateBaseScripts();
                CreateResourceManager();
                CreateCursorRules();
                CreateGitIgnore();
                SetupProjectSettings();
                
                AssetDatabase.Refresh();
                
                EditorUtility.DisplayDialog("Success", 
                    "Project structure created successfully!\n\n" +
                    "Next steps:\n" +
                    "1. Open the project in Cursor\n" +
                    "2. Review .cursorrules file\n" +
                    "3. Run 'CuriousCity > Build Egypt Scene' to test", 
                    "OK");
            }
        }
        
        static void CreateFolderStructure()
        {
            // Create all necessary folders
            string[] folders = {
                "Assets/Resources",
                "Assets/Resources/Prefabs",
                "Assets/Resources/Prefabs/Player",
                "Assets/Resources/Prefabs/Puzzles",
                "Assets/Resources/Prefabs/Environment",
                "Assets/Resources/Prefabs/Environment/Egypt",
                "Assets/Resources/Prefabs/Environment/Ark",
                "Assets/Resources/Prefabs/UI",
                "Assets/Resources/Materials",
                "Assets/Resources/Textures",
                "Assets/Resources/Audio",
                "Assets/Resources/Audio/Music",
                "Assets/Resources/Audio/SFX",
                "Assets/Resources/ScriptableObjects",
                "Assets/Resources/ScriptableObjects/Puzzles",
                "Assets/Resources/ScriptableObjects/Scenes",
                "Assets/Resources/ScriptableObjects/Missions",
                
                "Assets/Scripts",
                "Assets/Scripts/Core",
                "Assets/Scripts/Builders",
                "Assets/Scripts/Components",
                "Assets/Scripts/Components/Puzzles",
                "Assets/Scripts/Components/Player",
                "Assets/Scripts/Components/UI",
                "Assets/Scripts/Definitions",
                "Assets/Scripts/Managers",
                "Assets/Scripts/CursorCommands",
                "Assets/Scripts/Editor",
                
                "Assets/Scenes",
                "Assets/StreamingAssets",
                "Assets/Settings"
            };
            
            foreach (string folder in folders)
            {
                if (!AssetDatabase.IsValidFolder(folder))
                {
                    string parent = Path.GetDirectoryName(folder).Replace('\\', '/');
                    string newFolder = Path.GetFileName(folder);
                    AssetDatabase.CreateFolder(parent, newFolder);
                    Debug.Log($"Created folder: {folder}");
                }
            }
        }
        
        static void CreateBaseScripts()
        {
            // Create GameBootstrapper.cs
            string bootstrapperCode = @"using UnityEngine;
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
            Debug.Log(""[GameBootstrapper] Initializing Curious City..."");
            
            // Create persistent managers
            CreatePersistentSystems();
            
            // Load initial scene
            if (SceneManager.GetActiveScene().name == """")
            {
                SceneBuilder.BuildMainMenu();
            }
        }
        
        static void CreatePersistentSystems()
        {
            // Create persistent GameObject
            GameObject persistent = new GameObject(""_PersistentSystems"");
            Object.DontDestroyOnLoad(persistent);
            
            // Add core managers
            persistent.AddComponent<GameManager>();
            persistent.AddComponent<SceneBuilder>();
            persistent.AddComponent<AudioManager>();
            persistent.AddComponent<InputManager>();
            
            Debug.Log(""[GameBootstrapper] Core systems initialized"");
        }
    }
}";
            WriteScript("Assets/Scripts/Core/GameBootstrapper.cs", bootstrapperCode);
            
            // Create SceneBuilder.cs
            string sceneBuilderCode = @"using UnityEngine;
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
            GameObject cameraGO = new GameObject(""UI Camera"");
            Camera camera = cameraGO.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.1f, 0.1f, 0.2f);
            
            Debug.Log(""[SceneBuilder] Main menu built"");
        }
        
        public static void BuildEgyptMission()
        {
            ClearScene();
            
            // Create environment
            GameObject environment = new GameObject(""Environment"");
            
            // Create sun
            GameObject sun = new GameObject(""Directional Light"");
            Light sunLight = sun.AddComponent<Light>();
            sunLight.type = LightType.Directional;
            sunLight.color = new Color(1f, 0.95f, 0.8f);
            sunLight.intensity = 1.2f;
            sun.transform.rotation = Quaternion.Euler(45f, -30f, 0);
            
            // Create ground
            GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = ""Desert Floor"";
            ground.transform.localScale = new Vector3(50, 1, 50);
            ground.GetComponent<Renderer>().material.color = new Color(0.76f, 0.70f, 0.50f);
            
            // Create pyramids
            for (int i = 0; i < 3; i++)
            {
                CreatePyramid(new Vector3(i * 20 - 20, 0, 20));
            }
            
            // Create puzzle triggers
            CreatePuzzleTrigger(""ChronoCircuits"", new Vector3(0, 1, 0));
            CreatePuzzleTrigger(""ScrollSecrets"", new Vector3(10, 1, 0));
            CreatePuzzleTrigger(""PyramidRebuilder"", new Vector3(-10, 1, 0));
            
            // Spawn player
            SpawnPlayer(Vector3.zero + Vector3.up);
            
            Debug.Log(""[SceneBuilder] Egypt mission built"");
        }
        
        static void CreatePyramid(Vector3 position)
        {
            GameObject pyramid = GameObject.CreatePrimitive(PrimitiveType.Cube);
            pyramid.name = ""Pyramid"";
            pyramid.transform.position = position + Vector3.up * 5;
            pyramid.transform.localScale = new Vector3(10, 10, 10);
            pyramid.GetComponent<Renderer>().material.color = new Color(0.8f, 0.75f, 0.6f);
        }
        
        static void CreatePuzzleTrigger(string puzzleType, Vector3 position)
        {
            GameObject trigger = new GameObject($""{puzzleType}_Trigger"");
            trigger.transform.position = position;
            
            // Add visual representation
            GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Cube);
            visual.name = ""Visual"";
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
                ""ChronoCircuits"" => Color.cyan,
                ""ScrollSecrets"" => Color.yellow,
                ""PyramidRebuilder"" => Color.magenta,
                _ => Color.white
            };
            visual.GetComponent<Renderer>().material.color = color;
        }
        
        static void SpawnPlayer(Vector3 position)
        {
            // Create player
            GameObject player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            player.name = ""Player"";
            player.tag = ""Player"";
            player.transform.position = position;
            
            // Add camera
            GameObject cameraGO = new GameObject(""Main Camera"");
            cameraGO.tag = ""MainCamera"";
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
        
        static void ClearScene()
        {
            // Destroy all non-persistent objects
            foreach (GameObject go in FindObjectsOfType<GameObject>())
            {
                if (!go.name.StartsWith(""_""))
                {
                    DestroyImmediate(go);
                }
            }
        }
    }
}";
            WriteScript("Assets/Scripts/Core/SceneBuilder.cs", sceneBuilderCode);
            
            // Create GameManager.cs
            string gameManagerCode = @"using UnityEngine;

namespace CuriousCity.Core
{
    public class GameManager : MonoBehaviour
    {
        private static GameManager _instance;
        public static GameManager Instance => _instance;
        
        void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
            }
        }
        
        void Start()
        {
            Debug.Log(""[GameManager] Ready"");
        }
    }
}";
            WriteScript("Assets/Scripts/Core/GameManager.cs", gameManagerCode);
            
            // Create PuzzleTrigger.cs
            string puzzleTriggerCode = @"using UnityEngine;
using UnityEngine.UI;

namespace CuriousCity.Core
{
    public class PuzzleTrigger : MonoBehaviour
    {
        [Header(""Configuration"")]
        public string puzzleType = ""Generic"";
        public float interactionRange = 3f;
        public KeyCode interactionKey = KeyCode.E;
        public bool isCompleted = false;
        
        [Header(""UI"")]
        public GameObject promptUI;
        
        private bool playerInRange = false;
        private GameObject player;
        
        void Start()
        {
            // Find player
            player = GameObject.FindGameObjectWithTag(""Player"");
            
            // Create UI prompt
            CreatePromptUI();
        }
        
        void CreatePromptUI()
        {
            // Create world space canvas for prompt
            GameObject canvas = new GameObject(""Prompt Canvas"");
            canvas.transform.SetParent(transform);
            canvas.transform.localPosition = Vector3.up * 3;
            
            Canvas c = canvas.AddComponent<Canvas>();
            c.renderMode = RenderMode.WorldSpace;
            canvas.AddComponent<CanvasScaler>();
            canvas.AddComponent<GraphicRaycaster>();
            
            RectTransform rt = canvas.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(2, 0.5f);
            
            // Create text
            GameObject textGO = new GameObject(""Prompt Text"");
            textGO.transform.SetParent(canvas.transform);
            Text text = textGO.AddComponent<Text>();
            text.text = $""Press {interactionKey} to interact"";
            text.font = Resources.GetBuiltinResource<Font>(""LegacyRuntime.ttf"");
            text.fontSize = 50;
            text.color = Color.white;
            text.alignment = TextAnchor.MiddleCenter;
            
            RectTransform textRT = textGO.GetComponent<RectTransform>();
            textRT.sizeDelta = new Vector2(200, 50);
            textRT.localScale = Vector3.one * 0.01f;
            textRT.localPosition = Vector3.zero;
            
            promptUI = canvas;
            promptUI.SetActive(false);
        }
        
        void Update()
        {
            if (playerInRange && !isCompleted)
            {
                // Look at player
                if (promptUI != null && player != null)
                {
                    Vector3 lookPos = player.transform.position - promptUI.transform.position;
                    lookPos.y = 0;
                    promptUI.transform.rotation = Quaternion.LookRotation(lookPos);
                }
                
                // Check for interaction
                if (Input.GetKeyDown(interactionKey))
                {
                    CompletePuzzle();
                }
            }
        }
        
        void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag(""Player""))
            {
                playerInRange = true;
                if (promptUI != null && !isCompleted)
                    promptUI.SetActive(true);
                Debug.Log($""[{puzzleType}] Player entered range"");
            }
        }
        
        void OnTriggerExit(Collider other)
        {
            if (other.CompareTag(""Player""))
            {
                playerInRange = false;
                if (promptUI != null)
                    promptUI.SetActive(false);
                Debug.Log($""[{puzzleType}] Player left range"");
            }
        }
        
        void CompletePuzzle()
        {
            isCompleted = true;
            if (promptUI != null)
                promptUI.SetActive(false);
                
            // Change color to green
            GetComponentInChildren<Renderer>().material.color = Color.green;
            
            Debug.Log($""[{puzzleType}] Puzzle completed!"");
            
            // Check if all puzzles are complete
            PuzzleTrigger[] allPuzzles = FindObjectsOfType<PuzzleTrigger>();
            bool allComplete = true;
            foreach (var puzzle in allPuzzles)
            {
                if (!puzzle.isCompleted)
                {
                    allComplete = false;
                    break;
                }
            }
            
            if (allComplete)
            {
                Debug.Log(""[GameManager] All puzzles completed! Mission success!"");
            }
        }
    }
}";
            WriteScript("Assets/Scripts/Components/Puzzles/PuzzleTrigger.cs", puzzleTriggerCode);
            
            // Create SimplePlayerController.cs
            string playerControllerCode = @"using UnityEngine;

namespace CuriousCity.Core
{
    public class SimplePlayerController : MonoBehaviour
    {
        [Header(""Movement"")]
        public float moveSpeed = 5f;
        public float jumpHeight = 2f;
        
        [Header(""Mouse Look"")]
        public float mouseSensitivityX = 2f;
        public float mouseSensitivityY = 2f;
        public float maxLookAngle = 60f;
        
        private CharacterController controller;
        private Camera playerCamera;
        private float verticalVelocity = 0f;
        private float xRotation = 0f;
        
        void Start()
        {
            controller = GetComponent<CharacterController>();
            playerCamera = GetComponentInChildren<Camera>();
            
            // Lock cursor
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        
        void Update()
        {
            HandleMovement();
            HandleMouseLook();
            
            // Toggle cursor lock
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Cursor.lockState = Cursor.lockState == CursorLockMode.Locked 
                    ? CursorLockMode.None 
                    : CursorLockMode.Locked;
                Cursor.visible = !Cursor.visible;
            }
        }
        
        void HandleMovement()
        {
            // Get input
            float horizontal = Input.GetAxis(""Horizontal"");
            float vertical = Input.GetAxis(""Vertical"");
            
            // Calculate movement
            Vector3 move = transform.right * horizontal + transform.forward * vertical;
            
            // Apply gravity
            if (controller.isGrounded)
            {
                verticalVelocity = -2f;
                
                // Jump
                if (Input.GetButtonDown(""Jump""))
                {
                    verticalVelocity = Mathf.Sqrt(jumpHeight * 2f * 9.81f);
                }
            }
            else
            {
                verticalVelocity -= 9.81f * Time.deltaTime;
            }
            
            move.y = verticalVelocity;
            
            // Move controller
            controller.Move(move * moveSpeed * Time.deltaTime);
        }
        
        void HandleMouseLook()
        {
            // Get mouse input
            float mouseX = Input.GetAxis(""Mouse X"") * mouseSensitivityX;
            float mouseY = Input.GetAxis(""Mouse Y"") * mouseSensitivityY;
            
            // Rotate player body
            transform.Rotate(Vector3.up * mouseX);
            
            // Rotate camera
            xRotation -= mouseY;
            xRotation = Mathf.Clamp(xRotation, -maxLookAngle, maxLookAngle);
            playerCamera.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        }
    }
}";
            WriteScript("Assets/Scripts/Components/Player/SimplePlayerController.cs", playerControllerCode);
            
            // Create AudioManager stub
            string audioManagerCode = @"using UnityEngine;

namespace CuriousCity.Core
{
    public class AudioManager : MonoBehaviour
    {
        private static AudioManager _instance;
        public static AudioManager Instance => _instance;
        
        void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
            }
        }
    }
}";
            WriteScript("Assets/Scripts/Managers/AudioManager.cs", audioManagerCode);
            
            // Create InputManager stub
            string inputManagerCode = @"using UnityEngine;

namespace CuriousCity.Core
{
    public class InputManager : MonoBehaviour
    {
        private static InputManager _instance;
        public static InputManager Instance => _instance;
        
        void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
            }
        }
    }
}";
            WriteScript("Assets/Scripts/Managers/InputManager.cs", inputManagerCode);
        }
        
        static void CreateResourceManager()
        {
            // Create menu items for quick scene building
            string menuScriptCode = @"using UnityEngine;
using UnityEditor;
using CuriousCity.Core;

namespace CuriousCity.Editor
{
    public class QuickBuildMenu
    {
        [MenuItem(""CuriousCity/Build Egypt Scene"")]
        public static void BuildEgyptScene()
        {
            SceneBuilder.BuildEgyptMission();
        }
        
        [MenuItem(""CuriousCity/Build Main Menu"")]
        public static void BuildMainMenu()
        {
            SceneBuilder.BuildMainMenu();
        }
        
        [MenuItem(""CuriousCity/Clear Scene"")]
        public static void ClearScene()
        {
            if (EditorUtility.DisplayDialog(""Clear Scene"", 
                ""This will delete all GameObjects in the scene. Continue?"", 
                ""Yes"", ""Cancel""))
            {
                foreach (GameObject go in GameObject.FindObjectsOfType<GameObject>())
                {
                    GameObject.DestroyImmediate(go);
                }
            }
        }
    }
}";
            WriteScript("Assets/Scripts/Editor/QuickBuildMenu.cs", menuScriptCode);
        }
        
        static void CreateCursorRules()
        {
            string cursorRules = @"# Curious City - Code-First Unity Project

This is a code-first Unity project. All scenes are generated procedurally.

## Project Structure
```
Assets/
├── Resources/          # All runtime-loadable assets
├── Scripts/
│   ├── Core/          # Core systems (GameManager, SceneBuilder)
│   ├── Components/    # MonoBehaviours
│   ├── Builders/      # Procedural builders
│   └── Definitions/   # ScriptableObject definitions
```

## Key Rules
1. **Never edit scenes manually** - Use SceneBuilder
2. **All prefabs go in Resources/** - Load with Resources.Load<>()
3. **All configuration in ScriptableObjects** - No hardcoded values
4. **Use builders for everything** - Procedural generation only

## Common Commands

### Build a scene:
```csharp
SceneBuilder.BuildEgyptMission();
```

### Create a puzzle:
```csharp
CreatePuzzleTrigger(""PuzzleType"", position);
```

### Modify all puzzles:
```csharp
foreach (var puzzle in FindObjectsOfType<PuzzleTrigger>())
{
    puzzle.interactionRange = 3f;
}
```

## When modifying:
- Keep namespace CuriousCity.Core
- Maintain existing interfaces
- All values should be configurable
- Use [Header()] attributes for organization
- E key for interaction, 3m range standard

## File naming:
- Scripts: PascalCase matching class name
- Prefabs: Same as script with .prefab
- ScriptableObjects: SO_Name.asset
";
            
            File.WriteAllText(".cursorrules", cursorRules);
            Debug.Log("Created .cursorrules file");
        }
        
        static void CreateGitIgnore()
        {
            string gitignore = @"# Unity generated
[Ll]ibrary/
[Tt]emp/
[Oo]bj/
[Bb]uild/
[Bb]uilds/
[Ll]ogs/
[Uu]ser[Ss]ettings/
[Mm]emoryCaptures/

# Asset meta data
*.pidb.meta
*.pdb.meta
*.mdb.meta

# Unity3D generated file on crash reports
sysinfo.txt

# Builds
*.apk
*.unitypackage
*.app
*.exe

# Crashlytics
crashlytics-build.properties

# Autogenerated VS/MD/Consulo solution and project files
ExportedObj/
.consulo/
*.csproj
*.unityproj
*.sln
*.suo
*.tmp
*.user
*.userprefs
*.pidb
*.booproj
*.svd
*.pdb
*.mdb
*.opendb
*.VC.db

# OS generated
.DS_Store
.DS_Store?
._*
.Spotlight-V100
.Trashes
ehthumbs.db
Thumbs.db

# Cursor
.cursor/
";
            File.WriteAllText(".gitignore", gitignore);
            Debug.Log("Created .gitignore file");
        }
        
        static void SetupProjectSettings()
        {
            // Set player settings
            PlayerSettings.companyName = "CuriousCity";
            PlayerSettings.productName = "CuriousCity_CodeFirst";
            
            // Set tags
            SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            SerializedProperty tagsProp = tagManager.FindProperty("tags");
            
            // Add Player tag if not exists
            bool hasPlayerTag = false;
            for (int i = 0; i < tagsProp.arraySize; i++)
            {
                if (tagsProp.GetArrayElementAtIndex(i).stringValue == "Player")
                {
                    hasPlayerTag = true;
                    break;
                }
            }
            
            if (!hasPlayerTag)
            {
                tagsProp.InsertArrayElementAtIndex(0);
                tagsProp.GetArrayElementAtIndex(0).stringValue = "Player";
                tagManager.ApplyModifiedProperties();
            }
        }
        
        static void WriteScript(string path, string content)
        {
            string directory = Path.GetDirectoryName(path);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            
            File.WriteAllText(path, content);
            Debug.Log($"Created script: {path}");
        }
    }
}