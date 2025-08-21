using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace CuriousCity.Core
{
    public class ChronoCircuitsPuzzle : BasePuzzle
    {
        [Header("UI Elements")]
        public Button[] waterButtons = new Button[4];
        public Button closeButton;
        public TextMeshProUGUI titleText;
        public TextMeshProUGUI instructionText;
        public TextMeshProUGUI progressText;
        
        [Header("Circuit Configuration")]
        public int gridSize = 5;
        public GameObject nodePrefab;
        public Transform gridContainer;
        
        private bool[,] connections;
        private Vector2Int sourceNode = new Vector2Int(0, 2);
        private Vector2Int targetNode = new Vector2Int(4, 2);
        
        // Simple puzzle solution
        private int correctSequence = 0;
        private int[] solution = {0, 2, 1, 3}; // Button sequence
        
        protected override void InitializePuzzle()
        {
            // Set up UI text
            if (titleText)
                titleText.text = "ChronoCircuits - Water Flow Puzzle";
            
            if (instructionText)
                instructionText.text = "Click the water valves in the correct order to restore flow!";
            
            if (progressText)
                progressText.text = "Progress: 0/" + solution.Length;
            
            // Set up close button
            if (closeButton)
            {
                closeButton.onClick.AddListener(() => {
                    var puzzleManager = FindFirstObjectByType<PuzzleManager>();
                    if (puzzleManager)
                        puzzleManager.ExitCurrentPuzzle();
                });
            }
            
            // Set up water buttons
            for (int i = 0; i < waterButtons.Length; i++)
            {
                if (waterButtons[i] != null)
                {
                    int buttonIndex = i; // Capture for closure
                    waterButtons[i].onClick.AddListener(() => OnNodeClicked(buttonIndex, 0));
                }
            }
            
            // Initialize old system
            connections = new bool[gridSize, gridSize];
        }
        
        public void OnNodeClicked(int x, int y)
        {
            LogAttempt();
            LogInteraction("node_click");
            
            // Use the button index as puzzle logic
            if (x == solution[correctSequence])
            {
                correctSequence++;
                
                if (progressText)
                    progressText.text = "Progress: " + correctSequence + "/" + solution.Length;
                
                // Change button color to show it's been activated
                if (waterButtons[x])
                {
                    var colors = waterButtons[x].colors;
                    colors.normalColor = Color.green;
                    waterButtons[x].colors = colors;
                }
                
                if (CheckWinCondition())
                {
                    CompletePuzzle();
                }
            }
            else
            {
                // Wrong button - reset
                correctSequence = 0;
                if (progressText)
                    progressText.text = "Wrong! Try again. Progress: 0/" + solution.Length;
                
                // Reset button colors
                foreach (var btn in waterButtons)
                {
                    if (btn != null)
                    {
                        var colors = btn.colors;
                        colors.normalColor = Color.white;
                        btn.colors = colors;
                    }
                }
            }
        }
        
        private bool CheckWinCondition()
        {
            return correctSequence >= solution.Length;
        }
        
        protected override void CompletePuzzle()
        {
            if (instructionText)
                instructionText.text = "Puzzle Completed! Water flows restored!";
            
            base.CompletePuzzle();
            
            // Close puzzle after a delay
            Invoke(nameof(ClosePuzzleDelayed), 2f);
        }
        
        private void ClosePuzzleDelayed()
        {
            var puzzleManager = FindFirstObjectByType<PuzzleManager>();
            if (puzzleManager)
                puzzleManager.ExitCurrentPuzzle();
        }
        
        private void Update()
        {
            // Allow ESC to close puzzle
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                var puzzleManager = FindFirstObjectByType<PuzzleManager>();
                if (puzzleManager)
                    puzzleManager.ExitCurrentPuzzle();
            }
        }
    }
}