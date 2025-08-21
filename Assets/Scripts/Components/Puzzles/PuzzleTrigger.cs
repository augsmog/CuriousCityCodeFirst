using UnityEngine;
using UnityEngine.UI;

namespace CuriousCity.Core
{
    public class PuzzleTrigger : MonoBehaviour
    {
        [Header("Configuration")]
        public string puzzleType = "Generic";
        public float interactionRange = 3f;
        public KeyCode interactionKey = KeyCode.E;
        public bool isCompleted = false;
        
        [Header("UI")]
        public GameObject promptUI;
        
        private bool playerInRange = false;
        private GameObject player;
        
        void Start()
        {
            // Find player
            player = GameObject.FindGameObjectWithTag("Player");
            
            // Create UI prompt
            CreatePromptUI();
        }
        
        void CreatePromptUI()
        {
            // Create world space canvas for prompt
            GameObject canvas = new GameObject("Prompt Canvas");
            canvas.transform.SetParent(transform);
            canvas.transform.localPosition = Vector3.up * 3;
            
            Canvas c = canvas.AddComponent<Canvas>();
            c.renderMode = RenderMode.WorldSpace;
            canvas.AddComponent<CanvasScaler>();
            canvas.AddComponent<GraphicRaycaster>();
            
            RectTransform rt = canvas.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(2, 0.5f);
            
            // Create text
            GameObject textGO = new GameObject("Prompt Text");
            textGO.transform.SetParent(canvas.transform);
            Text text = textGO.AddComponent<Text>();
            text.text = $"Press {interactionKey} to interact";
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
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
            if (other.CompareTag("Player"))
            {
                playerInRange = true;
                if (promptUI != null && !isCompleted)
                    promptUI.SetActive(true);
                Debug.Log($"[{puzzleType}] Player entered range");
            }
        }
        
        void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                playerInRange = false;
                if (promptUI != null)
                    promptUI.SetActive(false);
                Debug.Log($"[{puzzleType}] Player left range");
            }
        }
        
        void CompletePuzzle()
        {
            isCompleted = true;
            if (promptUI != null)
                promptUI.SetActive(false);
                
            // Change color to green
            GetComponentInChildren<Renderer>().sharedMaterial.color = Color.green;

            
            Debug.Log($"[{puzzleType}] Puzzle completed!");
            
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
                Debug.Log("[GameManager] All puzzles completed! Mission success!");
            }
        }
    }
}