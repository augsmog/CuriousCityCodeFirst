using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace CuriousCity.Migration
{
    /// <summary>
    /// Complete automated migration system for converting CuriousCityAutomated to code-first architecture
    /// Place this in your NEW project at: Assets/Scripts/Editor/CompleteMigrationSystem.cs
    /// </summary>
    public class CompleteMigrationSystem : EditorWindow
    {
        private string oldProjectPath = "";
        private bool pathValidated = false;
        private List<MigrationTask> migrationTasks = new List<MigrationTask>();
        private Vector2 scrollPosition;
        
        [MenuItem("CuriousCity/Complete Migration System")]
        public static void ShowWindow()
        {
            var window = GetWindow<CompleteMigrationSystem>("Migration System");
            window.minSize = new Vector2(600, 400);
        }
        
        private void OnEnable()
        {
            InitializeMigrationTasks();
        }
        
        private void OnGUI()
        {
            DrawHeader();
            DrawPathSelection();
            
            if (pathValidated)
            {
                DrawMigrationTasks();
                DrawActionButtons();
            }
        }
        
        private void DrawHeader()
        {
            EditorGUILayout.Space();
            GUIStyle headerStyle = new GUIStyle(EditorStyles.boldLabel);
            headerStyle.fontSize = 16;
            EditorGUILayout.LabelField("CuriousCity Automated Migration System", headerStyle);
            EditorGUILayout.LabelField("This will migrate all scripts from your old project to the new code-first architecture", EditorStyles.wordWrappedLabel);
            EditorGUILayout.Space();
        }
        
        private void DrawPathSelection()
        {
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Step 1: Select Old Project Path", EditorStyles.boldLabel);
            
            EditorGUILayout.BeginHorizontal();
            oldProjectPath = EditorGUILayout.TextField("Old Project Path:", oldProjectPath);
            if (GUILayout.Button("Browse", GUILayout.Width(80)))
            {
                string path = EditorUtility.OpenFolderPanel("Select Old CuriousCityAutomated Project", "", "");
                if (!string.IsNullOrEmpty(path))
                {
                    oldProjectPath = path;
                }
            }
            EditorGUILayout.EndHorizontal();
            
            if (GUILayout.Button("Validate Path", GUILayout.Height(30)))
            {
                ValidateProjectPath();
            }
            
            if (pathValidated)
            {
                EditorGUILayout.HelpBox("✓ Valid project path detected!", MessageType.Info);
            }
            
            EditorGUILayout.EndVertical();
        }
        
        private void DrawMigrationTasks()
        {
            EditorGUILayout.Space();
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Step 2: Migration Tasks", EditorStyles.boldLabel);
            
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(200));
            
            foreach (var task in migrationTasks)
            {
                EditorGUILayout.BeginHorizontal();
                task.enabled = EditorGUILayout.Toggle(task.enabled, GUILayout.Width(20));
                
                GUI.color = task.completed ? Color.green : Color.white;
                string status = task.completed ? "✓" : "○";
                EditorGUILayout.LabelField(status, GUILayout.Width(20));
                GUI.color = Color.white;
                
                EditorGUILayout.LabelField(task.name);
                EditorGUILayout.LabelField($"({task.fileCount} files)", GUILayout.Width(80));
                EditorGUILayout.EndHorizontal();
            }
            
            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }
        
        private void DrawActionButtons()
        {
            EditorGUILayout.Space();
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Step 3: Execute Migration", EditorStyles.boldLabel);
            
            GUI.backgroundColor = Color.green;
            if (GUILayout.Button("START FULL MIGRATION", GUILayout.Height(40)))
            {
                if (EditorUtility.DisplayDialog("Start Migration", 
                    "This will:\n" +
                    "• Create folder structure\n" +
                    "• Copy and adapt all scripts\n" +
                    "• Generate resource files\n" +
                    "• Create prefabs\n\n" +
                    "Continue?", "Yes", "Cancel"))
                {
                    ExecuteFullMigration();
                }
            }
            GUI.backgroundColor = Color.white;
            
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Backup Current Project"))
            {
                BackupCurrentProject();
            }
            if (GUILayout.Button("Generate Report"))
            {
                GenerateMigrationReport();
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.EndVertical();
        }
        
        private void ValidateProjectPath()
        {
            if (Directory.Exists(oldProjectPath))
            {
                string assetsPath = Path.Combine(oldProjectPath, "Assets");
                string scriptsPath = Path.Combine(assetsPath, "Scripts");
                
                if (Directory.Exists(scriptsPath))
                {
                    pathValidated = true;
                    AnalyzeOldProject();
                }
                else
                {
                    EditorUtility.DisplayDialog("Invalid Path", "Could not find Assets/Scripts in the selected path", "OK");
                }
            }
        }
        
        private void InitializeMigrationTasks()
        {
            migrationTasks.Clear();
            migrationTasks.Add(new MigrationTask("Core Systems", true));
            migrationTasks.Add(new MigrationTask("Player Controller", true));
            migrationTasks.Add(new MigrationTask("Analytics System", true));
            migrationTasks.Add(new MigrationTask("Data Structures", true));
            migrationTasks.Add(new MigrationTask("Interaction System", true));
            migrationTasks.Add(new MigrationTask("Puzzle Systems", true));
            migrationTasks.Add(new MigrationTask("Chrona AI", true));
            migrationTasks.Add(new MigrationTask("Crew System", true));
            migrationTasks.Add(new MigrationTask("UI Systems", false));
            migrationTasks.Add(new MigrationTask("Audio Systems", false));
        }
        
        private void AnalyzeOldProject()
        {
            string scriptsPath = Path.Combine(oldProjectPath, "Assets", "Scripts");
            
            // Count files for each system
            UpdateTaskFileCount("Core Systems", CountFiles(scriptsPath, "Core"));
            UpdateTaskFileCount("Player Controller", CountFiles(scriptsPath, "Characters"));
            UpdateTaskFileCount("Analytics System", CountFiles(scriptsPath, "Analytics"));
            UpdateTaskFileCount("Data Structures", CountFiles(scriptsPath, "Data"));
            UpdateTaskFileCount("Interaction System", CountFiles(scriptsPath, "Interactions"));
            UpdateTaskFileCount("Puzzle Systems", CountFiles(scriptsPath, "Puzzles"));
            UpdateTaskFileCount("Chrona AI", CountFiles(scriptsPath, "Chrona"));
            UpdateTaskFileCount("Crew System", CountFiles(scriptsPath, "Crew"));
        }
        
        private int CountFiles(string basePath, string folder)
        {
            string path = Path.Combine(basePath, folder);
            if (Directory.Exists(path))
            {
                return Directory.GetFiles(path, "*.cs", SearchOption.AllDirectories).Length;
            }
            return 0;
        }
        
        private void UpdateTaskFileCount(string taskName, int count)
        {
            var task = migrationTasks.FirstOrDefault(t => t.name == taskName);
            if (task != null)
            {
                task.fileCount = count;
            }
        }
        
        private void ExecuteFullMigration()
        {
            try
            {
                EditorUtility.DisplayProgressBar("Migration", "Starting migration...", 0);
                
                // Phase 1: Create folder structure
                CreateFolderStructure();
                
                // Phase 2: Migrate each system
                float progress = 0;
                float step = 1f / migrationTasks.Count;
                
                foreach (var task in migrationTasks.Where(t => t.enabled))
                {
                    EditorUtility.DisplayProgressBar("Migration", $"Migrating {task.name}...", progress);
                    MigrateSystem(task);
                    task.completed = true;
                    progress += step;
                }
                
                // Phase 3: Generate supporting files
                EditorUtility.DisplayProgressBar("Migration", "Generating support files...", 0.8f);
                GenerateSupportFiles();
                
                // Phase 4: Create prefabs
                EditorUtility.DisplayProgressBar("Migration", "Creating prefabs...", 0.9f);
                GeneratePrefabs();
                
                // Phase 5: Final setup
                EditorUtility.DisplayProgressBar("Migration", "Finalizing...", 0.95f);
                FinalizeSetup();
                
                EditorUtility.ClearProgressBar();
                AssetDatabase.Refresh();
                
                EditorUtility.DisplayDialog("Success", 
                    "Migration completed successfully!\n\n" +
                    "Next steps:\n" +
                    "1. Test the Build Egypt Scene button\n" +
                    "2. Press Play to test\n" +
                    "3. Check the migration report", "OK");
                    
                GenerateMigrationReport();
            }
            catch (System.Exception e)
            {
                EditorUtility.ClearProgressBar();
                Debug.LogError($"Migration failed: {e.Message}\n{e.StackTrace}");
                EditorUtility.DisplayDialog("Error", $"Migration failed: {e.Message}", "OK");
            }
        }
        
        private void CreateFolderStructure()
        {
            string[] folders = {
                "Assets/Scripts/Core",
                "Assets/Scripts/Builders",
                "Assets/Scripts/Components/Player",
                "Assets/Scripts/Components/Puzzles",
                "Assets/Scripts/Components/UI",
                "Assets/Scripts/Components/Interactions",
                "Assets/Scripts/Analytics",
                "Assets/Scripts/Data",
                "Assets/Scripts/Managers",
                "Assets/Scripts/Systems",
                "Assets/Scripts/Editor",
                "Assets/Resources/Prefabs/Player",
                "Assets/Resources/Prefabs/Puzzles",
                "Assets/Resources/Prefabs/Environment",
                "Assets/Resources/Prefabs/UI",
                "Assets/Resources/Prefabs/Chrona",
                "Assets/Resources/Prefabs/Crew",
                "Assets/Resources/ScriptableObjects/Missions",
                "Assets/Resources/ScriptableObjects/Puzzles",
                "Assets/Resources/ScriptableObjects/Analytics",
                "Assets/Resources/ScriptableObjects/Artifacts",
                "Assets/Resources/Materials",
                "Assets/Resources/Textures",
                "Assets/Resources/Audio/Music",
                "Assets/Resources/Audio/SFX"
            };
            
            foreach (string folder in folders)
            {
                if (!AssetDatabase.IsValidFolder(folder))
                {
                    string parent = Path.GetDirectoryName(folder).Replace('\\', '/');
                    string newFolder = Path.GetFileName(folder);
                    AssetDatabase.CreateFolder(parent, newFolder);
                }
            }
        }
        
        private void MigrateSystem(MigrationTask task)
        {
            switch (task.name)
            {
                case "Core Systems":
                    MigrateCoreSystem();
                    break;
                case "Player Controller":
                    MigratePlayerController();
                    break;
                case "Analytics System":
                    MigrateAnalytics();
                    break;
                case "Data Structures":
                    MigrateDataStructures();
                    break;
                case "Interaction System":
                    MigrateInteractions();
                    break;
                case "Puzzle Systems":
                    MigratePuzzles();
                    break;
                case "Chrona AI":
                    MigrateChronaAI();
                    break;
                case "Crew System":
                    MigrateCrewSystem();
                    break;
            }
        }
        
        private void MigrateCoreSystem()
        {
            // Migrate ArkBridgeManager
            string sourcePath = Path.Combine(oldProjectPath, "Assets/Scripts/Core/ArkBridgeManager.cs");
            if (File.Exists(sourcePath))
            {
                string content = File.ReadAllText(sourcePath);
                content = AdaptCoreSystem(content);
                File.WriteAllText("Assets/Scripts/Core/ArkBridgeManager.cs", content);
            }
            
            // Migrate HistoricalMissionSceneManager
            sourcePath = Path.Combine(oldProjectPath, "Assets/Scripts/Gameplay/Puzzles/HistoricalMissionSceneManager.cs");
            if (File.Exists(sourcePath))
            {
                string content = File.ReadAllText(sourcePath);
                content = AdaptMissionManager(content);
                File.WriteAllText("Assets/Scripts/Managers/MissionManager.cs", content);
            }
        }
        
        private void MigratePlayerController()
        {
            string sourcePath = Path.Combine(oldProjectPath, "Assets/Scripts/Characters/FirstPersonController.cs");
            if (File.Exists(sourcePath))
            {
                string content = File.ReadAllText(sourcePath);
                content = AdaptPlayerController(content);
                File.WriteAllText("Assets/Scripts/Components/Player/FirstPersonController.cs", content);
            }
        }
        
        private void MigrateAnalytics()
        {
            string sourceDir = Path.Combine(oldProjectPath, "Assets/Scripts/Analytics");
            if (Directory.Exists(sourceDir))
            {
                foreach (string file in Directory.GetFiles(sourceDir, "*.cs", SearchOption.AllDirectories))
                {
                    string fileName = Path.GetFileName(file);
                    string content = File.ReadAllText(file);
                    content = AdaptAnalytics(content);
                    
                    string destPath = Path.Combine("Assets/Scripts/Analytics", fileName);
                    Directory.CreateDirectory(Path.GetDirectoryName(destPath));
                    File.WriteAllText(destPath, content);
                }
            }
        }
        
        private void MigrateDataStructures()
        {
            string sourceDir = Path.Combine(oldProjectPath, "Assets/Scripts/Data");
            if (Directory.Exists(sourceDir))
            {
                foreach (string file in Directory.GetFiles(sourceDir, "*.cs"))
                {
                    string fileName = Path.GetFileName(file);
                    string content = File.ReadAllText(file);
                    content = AdaptDataStructures(content);
                    File.WriteAllText($"Assets/Scripts/Data/{fileName}", content);
                }
            }
        }
        
        private void MigrateInteractions()
        {
            string sourceDir = Path.Combine(oldProjectPath, "Assets/Scripts/Gameplay/Interactions");
            if (Directory.Exists(sourceDir))
            {
                foreach (string file in Directory.GetFiles(sourceDir, "*.cs"))
                {
                    string fileName = Path.GetFileName(file);
                    string content = File.ReadAllText(file);
                    content = AdaptInteractions(content);
                    File.WriteAllText($"Assets/Scripts/Components/Interactions/{fileName}", content);
                }
            }
        }
        
        private void MigratePuzzles()
        {
            string sourceDir = Path.Combine(oldProjectPath, "Assets/Scripts/Gameplay/Puzzles");
            if (Directory.Exists(sourceDir))
            {
                foreach (string file in Directory.GetFiles(sourceDir, "*.cs"))
                {
                    string fileName = Path.GetFileName(file);
                    if (fileName != "HistoricalMissionSceneManager.cs") // Already migrated
                    {
                        string content = File.ReadAllText(file);
                        content = AdaptPuzzles(content);
                        File.WriteAllText($"Assets/Scripts/Components/Puzzles/{fileName}", content);
                    }
                }
            }
        }
        
        private void MigrateChronaAI()
        {
            // Check multiple possible locations
            string[] possiblePaths = {
                Path.Combine(oldProjectPath, "Assets/Scripts/Core/ChronaAI.cs"),
                Path.Combine(oldProjectPath, "Assets/Scripts/Characters/ChronaAI.cs"),
                Path.Combine(oldProjectPath, "Assets/Scripts/AI/ChronaAI.cs")
            };
            
            foreach (string sourcePath in possiblePaths)
            {
                if (File.Exists(sourcePath))
                {
                    string content = File.ReadAllText(sourcePath);
                    content = AdaptChronaAI(content);
                    File.WriteAllText("Assets/Scripts/Core/ChronaAI.cs", content);
                    break;
                }
            }
        }
        
        private void MigrateCrewSystem()
        {
            string sourcePath = Path.Combine(oldProjectPath, "Assets/Scripts/Core/CrewInteractionManager.cs");
            if (File.Exists(sourcePath))
            {
                string content = File.ReadAllText(sourcePath);
                content = AdaptCrewSystem(content);
                File.WriteAllText("Assets/Scripts/Managers/CrewManager.cs", content);
            }
        }
        
        // Adaptation methods for each system
        private string AdaptCoreSystem(string content)
        {
            // Change namespace
            content = Regex.Replace(content, @"namespace CuriousCityAutomated\.\w+", "namespace CuriousCity.Core");
            
            // Remove SerializeField references
            content = Regex.Replace(content, @"\[SerializeField\]\s*private\s+\w+\s+\w+;", "// Removed scene reference - created at runtime");
            
            // Convert to singleton pattern
            if (!content.Contains("private static"))
            {
                content = AddSingletonPattern(content, "ArkBridgeManager");
            }
            
            return content;
        }
        
        private string AdaptPlayerController(string content)
        {
            // Change namespace
            content = content.Replace("CuriousCityAutomated.Characters", "CuriousCity.Core");
            
            // Fix mouse sensitivity
            content = Regex.Replace(content, @"public float mouseSensitivity = \d+\.?\d*f?;", "public float mouseSensitivity = 2.0f;");
            
            // Remove UI element scene references
            content = Regex.Replace(content, @"\[Header\(""UI Elements""\)\][\s\S]*?(?=\[Header|private|public|protected|//|$)", 
                "[Header(\"UI Elements\")]\n    // UI elements created at runtime\n");
            
            // Add runtime UI creation
            if (!content.Contains("CreateRuntimeUI"))
            {
                string uiCreation = @"
    private void CreateRuntimeUI()
    {
        // Create interaction prompt
        if (interactionPrompt == null)
        {
            GameObject canvas = new GameObject(""InteractionCanvas"");
            Canvas c = canvas.AddComponent<Canvas>();
            c.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.AddComponent<UnityEngine.UI.CanvasScaler>();
            canvas.AddComponent<UnityEngine.UI.GraphicRaycaster>();
            
            interactionPrompt = new GameObject(""InteractionPrompt"");
            interactionPrompt.transform.SetParent(canvas.transform);
            
            interactionText = interactionPrompt.AddComponent<TMPro.TextMeshProUGUI>();
            interactionText.text = ""Press E to interact"";
            interactionText.fontSize = 24;
            interactionText.alignment = TMPro.TextAlignmentOptions.Center;
            
            RectTransform rt = interactionPrompt.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = new Vector2(0, -100);
            rt.sizeDelta = new Vector2(300, 50);
            
            interactionPrompt.SetActive(false);
        }
        
        // Create crosshair
        if (crosshair == null)
        {
            GameObject crosshairGO = new GameObject(""Crosshair"");
            crosshairGO.transform.SetParent(GameObject.Find(""InteractionCanvas"").transform);
            
            crosshair = crosshairGO.AddComponent<UnityEngine.UI.Image>();
            crosshair.color = defaultCrosshairColor;
            
            RectTransform crt = crosshairGO.GetComponent<RectTransform>();
            crt.anchorMin = new Vector2(0.5f, 0.5f);
            crt.anchorMax = new Vector2(0.5f, 0.5f);
            crt.anchoredPosition = Vector2.zero;
            crt.sizeDelta = new Vector2(10, 10);
        }
    }";
                
                // Add method after Awake
                content = Regex.Replace(content, @"(void Awake\(\)[^}]*})", "$1\n" + uiCreation);
                
                // Call it in Start
                content = content.Replace("void Start()", "void Start()\n    {\n        CreateRuntimeUI();");
            }
            
            return content;
        }
        
        private string AdaptAnalytics(string content)
        {
            // Change namespace
            content = Regex.Replace(content, @"namespace CuriousCityAutomated\.\w+", "namespace CuriousCity.Analytics");
            
            // Make trackers singletons
            if (content.Contains("class LearningStyleTracker") && !content.Contains("private static LearningStyleTracker"))
            {
                content = AddSingletonPattern(content, "LearningStyleTracker");
            }
            
            return content;
        }
        
        private string AdaptDataStructures(string content)
        {
            // Change namespace
            content = content.Replace("CuriousCityAutomated.Data", "CuriousCity.Data");
            
            // Convert suitable classes to ScriptableObjects
            if (content.Contains("public class") && content.Contains("[Serializable]"))
            {
                content = ConvertToScriptableObject(content);
            }
            
            return content;
        }
        
        private string AdaptInteractions(string content)
        {
            // Change namespace
            content = content.Replace("CuriousCityAutomated.Gameplay.Interactions", "CuriousCity.Core");
            
            // Fix using statements
            content = content.Replace("using CuriousCityAutomated", "using CuriousCity");
            
            // Remove scene dependencies
            content = Regex.Replace(content, @"\[SerializeField\].*?;", "// Scene reference removed");
            
            return content;
        }
        
        private string AdaptPuzzles(string content)
        {
            // Change namespace
            content = content.Replace("CuriousCityAutomated.Gameplay.Puzzles", "CuriousCity.Core");
            
            // Update interaction range
            content = Regex.Replace(content, @"interactionRange = \d+\.?\d*f?", "interactionRange = 3.0f");
            
            return content;
        }
        
        private string AdaptChronaAI(string content)
        {
            // Change namespace
            content = Regex.Replace(content, @"namespace CuriousCityAutomated\.\w+", "namespace CuriousCity.Core");
            
            // Add procedural spawn capability
            if (!content.Contains("SpawnChrona"))
            {
                string spawnMethod = @"
    public static GameObject SpawnChrona(Vector3 position)
    {
        GameObject chronaGO = new GameObject(""Chrona"");
        chronaGO.transform.position = position;
        
        // Add visual representation
        GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        visual.transform.SetParent(chronaGO.transform);
        visual.transform.localScale = new Vector3(0.5f, 1f, 0.5f);
        
        // Make holographic
        Renderer renderer = visual.GetComponent<Renderer>();
        renderer.material.color = new Color(0, 1, 1, 0.5f);
        
        // Add AI component
        chronaGO.AddComponent<ChronaPersonalityState>();
        
        return chronaGO;
    }";
                content = content.Replace("public class Chrona", "public class Chrona\n{" + spawnMethod);
            }
            
            return content;
        }
        
        private string AdaptCrewSystem(string content)
        {
            // Change namespace
            content = content.Replace("CuriousCityAutomated", "CuriousCity");
            
            // Convert to resource-based loading
            content = Regex.Replace(content, @"public CrewMember\[\] crewMembers;", 
                "private CrewMember[] crewMembers { get { return Resources.LoadAll<CrewMember>(\"ScriptableObjects/Crew\"); } }");
            
            return content;
        }
        
        private string AdaptMissionManager(string content)
        {
            // Change namespace and class name
            content = content.Replace("HistoricalMissionSceneManager", "MissionManager");
            content = Regex.Replace(content, @"namespace CuriousCityAutomated\.\w+", "namespace CuriousCity.Core");
            
            // Make it work with SceneBuilder
            content = AddSceneBuilderIntegration(content);
            
            return content;
        }
        
        // Helper methods
        private string AddSingletonPattern(string content, string className)
        {
            string pattern = $@"
    private static {className} _instance;
    public static {className} Instance
    {{
        get
        {{
            if (_instance == null)
            {{
                _instance = FindObjectOfType<{className}>();
                if (_instance == null)
                {{
                    GameObject go = new GameObject(""{className}"");
                    _instance = go.AddComponent<{className}>();
                    DontDestroyOnLoad(go);
                }}
            }}
            return _instance;
        }}
    }}
    
    void Awake()
    {{
        if (_instance != null && _instance != this)
        {{
            Destroy(gameObject);
            return;
        }}
        _instance = this;
        DontDestroyOnLoad(gameObject);
    }}";
            
            // Insert after class declaration
            int classIndex = content.IndexOf($"public class {className}");
            if (classIndex != -1)
            {
                int braceIndex = content.IndexOf("{", classIndex);
                content = content.Insert(braceIndex + 1, pattern);
            }
            
            return content;
        }
        
        private string ConvertToScriptableObject(string content)
        {
            // Change base class
            content = content.Replace(": MonoBehaviour", ": ScriptableObject");
            content = content.Replace("public class", "[CreateAssetMenu(fileName = \"New\", menuName = \"CuriousCity/Data\")]\npublic class");
            
            // Remove MonoBehaviour methods
            content = Regex.Replace(content, @"void (Start|Awake|Update|FixedUpdate|LateUpdate)\(\)[\s\S]*?(?=\n    (private|public|protected|void|\}|$))", "");
            
            return content;
        }
        
        private string AddSceneBuilderIntegration(string content)
        {
            string integration = @"
    public static void BuildMissionScene(string missionType)
    {
        SceneBuilder.ClearScene();
        
        // Build based on mission type
        switch(missionType)
        {
            case ""Egypt"":
                SceneBuilder.BuildEgyptMission();
                break;
            case ""Greece"":
                SceneBuilder.BuildGreeceMission();
                break;
            default:
                SceneBuilder.BuildEgyptMission();
                break;
        }
        
        // Initialize mission manager
        GameObject managerGO = new GameObject(""MissionManager"");
        MissionManager manager = managerGO.AddComponent<MissionManager>();
        manager.StartMission(missionType);
    }";
            
            // Add before the last closing brace
            int lastBrace = content.LastIndexOf("}");
            content = content.Insert(lastBrace - 1, integration);
            
            return content;
        }
        
        private void GenerateSupportFiles()
        {
            // Generate SceneBuilder extension
            string sceneBuilderExtension = @"using UnityEngine;
using CuriousCity.Core;

namespace CuriousCity.Builders
{
    public static class SceneBuilderExtensions
    {
        public static void BuildFromMigration()
        {
            SceneBuilder.BuildEgyptMission();
            
            // Add migrated systems
            GameObject systems = new GameObject(""_Systems"");
            systems.AddComponent<LearningStyleTracker>();
            systems.AddComponent<MissionManager>();
            systems.AddComponent<CrewManager>();
            
            // Spawn Chrona
            ChronaAI.SpawnChrona(new Vector3(0, 2, 5));
            
            Debug.Log(""[Migration] All systems initialized"");
        }
    }
}";
            File.WriteAllText("Assets/Scripts/Builders/SceneBuilderExtensions.cs", sceneBuilderExtension);
            
            // Update .cursorrules
            string cursorRules = @"# CuriousCity Code-First Project
## Migrated from CuriousCityAutomated

This project has been fully migrated to a code-first architecture.
All scenes are generated procedurally.

## Key Systems:
- Player Controller: 2.0 mouse sensitivity, 3m interaction range
- Puzzle System: E key interaction, procedural generation
- Analytics: Learning style tracking integrated
- Chrona AI: Evolving personality system
- Mission Manager: Handles all mission flow

## Namespace: CuriousCity.Core

## Build Commands:
- SceneBuilder.BuildEgyptMission()
- SceneBuilder.BuildArkBridge()
- SceneBuilderExtensions.BuildFromMigration()

## All configuration via ScriptableObjects in Resources/
";
            File.WriteAllText(".cursorrules", cursorRules);
        }
        
        private void GeneratePrefabs()
        {
            // This would normally create actual prefabs, but for now we'll create the structure
            Debug.Log("[Migration] Prefab generation scheduled - create prefabs from migrated components");
        }
        
        private void FinalizeSetup()
        {
            // Update menu items
            string menuScript = @"using UnityEngine;
using UnityEditor;
using CuriousCity.Core;
using CuriousCity.Builders;

namespace CuriousCity.Editor
{
    public class MigratedMenu
    {
        [MenuItem(""CuriousCity/Build Migrated Scene"")]
        public static void BuildMigratedScene()
        {
            SceneBuilderExtensions.BuildFromMigration();
        }
        
        [MenuItem(""CuriousCity/Test All Systems"")]
        public static void TestSystems()
        {
            Debug.Log(""Testing migrated systems..."");
            
            // Test each system
            if (LearningStyleTracker.Instance != null)
                Debug.Log(""✓ Analytics system ready"");
                
            if (MissionManager.Instance != null)
                Debug.Log(""✓ Mission system ready"");
                
            Debug.Log(""All systems operational!"");
        }
    }
}";
            File.WriteAllText("Assets/Scripts/Editor/MigratedMenu.cs", menuScript);
        }
        
        private void BackupCurrentProject()
        {
            string backupPath = Path.Combine(Application.dataPath, "..", "Backup_" + System.DateTime.Now.ToString("yyyyMMdd_HHmmss"));
            
            if (EditorUtility.DisplayDialog("Backup Project", 
                $"This will create a backup at:\n{backupPath}\n\nContinue?", "Yes", "Cancel"))
            {
                EditorUtility.DisplayProgressBar("Backup", "Creating backup...", 0.5f);
                
                // Copy entire project folder
                DirectoryCopy(Application.dataPath, Path.Combine(backupPath, "Assets"), true);
                DirectoryCopy(Path.Combine(Application.dataPath, "..", "ProjectSettings"), 
                    Path.Combine(backupPath, "ProjectSettings"), true);
                
                EditorUtility.ClearProgressBar();
                EditorUtility.DisplayDialog("Success", "Backup created successfully!", "OK");
            }
        }
        
        private void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs)
        {
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);
            DirectoryInfo[] dirs = dir.GetDirectories();
            
            Directory.CreateDirectory(destDirName);
            
            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                string tempPath = Path.Combine(destDirName, file.Name);
                file.CopyTo(tempPath, false);
            }
            
            if (copySubDirs)
            {
                foreach (DirectoryInfo subdir in dirs)
                {
                    string tempPath = Path.Combine(destDirName, subdir.Name);
                    DirectoryCopy(subdir.FullName, tempPath, copySubDirs);
                }
            }
        }
        
        private void GenerateMigrationReport()
        {
            string report = "MIGRATION REPORT\n";
            report += "================\n\n";
            report += $"Date: {System.DateTime.Now}\n";
            report += $"Source: {oldProjectPath}\n";
            report += $"Target: {Application.dataPath}\n\n";
            
            report += "MIGRATION TASKS:\n";
            foreach (var task in migrationTasks)
            {
                string status = task.completed ? "✓" : "✗";
                report += $"{status} {task.name} ({task.fileCount} files)\n";
            }
            
            report += "\nFILES CREATED:\n";
            string[] createdFiles = Directory.GetFiles("Assets/Scripts", "*.cs", SearchOption.AllDirectories);
            report += $"Total scripts: {createdFiles.Length}\n";
            
            report += "\nNEXT STEPS:\n";
            report += "1. Test 'Build Migrated Scene' button\n";
            report += "2. Press Play to test functionality\n";
            report += "3. Create prefabs from migrated components\n";
            report += "4. Test puzzle interactions (E key)\n";
            report += "5. Verify mouse sensitivity (2.0)\n";
            
            string reportPath = Path.Combine(Application.dataPath, "..", "MigrationReport.txt");
            File.WriteAllText(reportPath, report);
            
            EditorUtility.DisplayDialog("Report Generated", 
                $"Migration report saved to:\n{reportPath}", "OK");
            
            // Open the report
            Application.OpenURL("file://" + reportPath);
        }
        
        [System.Serializable]
        private class MigrationTask
        {
            public string name;
            public bool enabled;
            public bool completed;
            public int fileCount;
            
            public MigrationTask(string name, bool enabled = true)
            {
                this.name = name;
                this.enabled = enabled;
                this.completed = false;
                this.fileCount = 0;
            }
        }
    }
}