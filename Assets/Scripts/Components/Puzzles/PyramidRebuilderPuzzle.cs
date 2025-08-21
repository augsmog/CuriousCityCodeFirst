using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace CuriousCity.Core
{
    public class PyramidRebuilderPuzzle : BasePuzzle
    {
        [Header("UI Elements")]
        public Button[] pieceButtons = new Button[4];
        public Image[] targetSlots = new Image[4];
        public Button closeButton;
        public TextMeshProUGUI titleText;
        public TextMeshProUGUI instructionText;
        public TextMeshProUGUI progressText;
        
        [Header("Pyramid Configuration")]
        public int puzzleSize = 4;
        public GameObject piecePrefab;
        public Transform puzzleContainer;
        public Transform pieceContainer;
        
        private bool[,] correctPattern;
        private bool[,] currentPattern;
        
        // Simple puzzle solution
        private int[] correctPieceOrder = {3, 1, 4, 2}; // Example pattern
        private int[] currentPieceOrder;
        private int currentPiece = 0;
        
        protected override void InitializePuzzle()
        {
            // Set up UI text
            if (titleText)
                titleText.text = "Pyramid Rebuilder - Restore the Pattern";
            
            if (instructionText)
                instructionText.text = "Click the mural pieces in the correct order to restore the ancient pattern!";
            
            if (progressText)
                progressText.text = "Progress: 0/" + correctPieceOrder.Length;
            
            // Set up close button
            if (closeButton)
            {
                closeButton.onClick.AddListener(() => {
                    var puzzleManager = FindFirstObjectByType<PuzzleManager>();
                    if (puzzleManager)
                        puzzleManager.ExitCurrentPuzzle();
                });
            }
            
            // Initialize arrays
            currentPieceOrder = new int[correctPieceOrder.Length];
            correctPattern = new bool[puzzleSize, puzzleSize];
            currentPattern = new bool[puzzleSize, puzzleSize];
            
            // Set up piece buttons
            for (int i = 0; i < pieceButtons.Length; i++)
            {
                if (pieceButtons[i] != null)
                {
                    int pieceIndex = i; // Capture for closure
                    pieceButtons[i].onClick.AddListener(() => OnPieceClicked(pieceIndex));
                }
            }
            
            // Reset target slots
            foreach (var slot in targetSlots)
            {
                if (slot != null)
                    slot.color = Color.white;
            }
        }
        
        public void OnPieceClicked(int pieceIndex)
        {
            if (currentPiece >= correctPieceOrder.Length) return;
            
            LogAttempt();
            LogInteraction("piece_placement");
            
            currentPieceOrder[currentPiece] = pieceIndex;
            
            // Visual feedback - place piece in slot
            if (targetSlots[currentPiece])
            {
                targetSlots[currentPiece].color = pieceButtons[pieceIndex].image.color;
            }
            
            currentPiece++;
            
            if (progressText)
                progressText.text = "Progress: " + currentPiece + "/" + correctPieceOrder.Length;
            
            if (currentPiece >= correctPieceOrder.Length)
            {
                CheckPatternMatch();
            }
        }
        
        // Keep the old method for compatibility
        public void OnPiecePlaced(int x, int y)
        {
            OnPieceClicked(x);
        }
        
        private bool CheckPatternMatch()
        {
            bool isCorrect = true;
            
            for (int i = 0; i < correctPieceOrder.Length; i++)
            {
                if (currentPieceOrder[i] != correctPieceOrder[i])
                {
                    isCorrect = false;
                    break;
                }
            }
            
            if (isCorrect)
            {
                CompletePuzzle();
                return true;
            }
            else
            {
                // Reset
                currentPiece = 0;
                if (instructionText)
                    instructionText.text = "Pattern incorrect! Try again.";
                
                if (progressText)
                    progressText.text = "Progress: 0/" + correctPieceOrder.Length;
                
                // Reset visual slots
                foreach (var slot in targetSlots)
                {
                    if (slot != null)
                        slot.color = Color.white;
                }
                
                return false;
            }
        }
        
        protected override void CompletePuzzle()
        {
            if (instructionText)
                instructionText.text = "Puzzle Completed! The ancient pattern is restored!";
            
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
        
        private void CreatePuzzlePieces()
        {
            // Legacy method - implementation can be added later
        }
    }
}