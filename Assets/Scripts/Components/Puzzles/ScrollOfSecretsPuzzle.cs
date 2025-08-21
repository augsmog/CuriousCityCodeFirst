using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace CuriousCity.Core
{
    public class ScrollOfSecretsPuzzle : BasePuzzle
    {
        [Header("UI Elements")]
        public TMP_InputField[] wordInputs = new TMP_InputField[4];
        public Button checkButton;
        public Button closeButton;
        public TextMeshProUGUI titleText;
        public TextMeshProUGUI instructionText;
        public TextMeshProUGUI progressText;
        
        [Header("Scroll Configuration")]
        public List<string> missingWords = new List<string>();
        public List<string> wordOptions = new List<string>();
        public Transform wordContainer;
        
        private Dictionary<int, string> playerAnswers;
        private string[] correctWords = {"NILE", "PHARAOH", "PYRAMID", "SCRIBE"};
        
        protected override void InitializePuzzle()
        {
            // Set up UI text
            if (titleText)
                titleText.text = "Scroll of Secrets - Ancient Words";
            
            if (instructionText)
                instructionText.text = "Fill in the missing hieroglyphic words to complete the ancient text!";
            
            if (progressText)
                progressText.text = "Words completed: 0/" + correctWords.Length;
            
            // Set up close button
            if (closeButton)
            {
                closeButton.onClick.AddListener(() => {
                    var puzzleManager = FindFirstObjectByType<PuzzleManager>();
                    if (puzzleManager)
                        puzzleManager.ExitCurrentPuzzle();
                });
            }
            
            // Set up check button
            if (checkButton)
                checkButton.onClick.AddListener(CheckAnswers);
            
            // Initialize player answers
            playerAnswers = new Dictionary<int, string>();
            
            // Set up input field listeners
            for (int i = 0; i < wordInputs.Length; i++)
            {
                if (wordInputs[i] != null)
                {
                    int inputIndex = i; // Capture for closure
                    wordInputs[i].onValueChanged.AddListener((value) => OnWordChanged(inputIndex, value));
                }
            }
            
            CreateScrollInterface();
        }
        
        private void OnWordChanged(int position, string word)
        {
            playerAnswers[position] = word.ToUpper();
            
            // Update progress
            int completedWords = 0;
            foreach (var answer in playerAnswers.Values)
            {
                if (!string.IsNullOrEmpty(answer))
                    completedWords++;
            }
            
            if (progressText)
                progressText.text = "Words completed: " + completedWords + "/" + correctWords.Length;
        }
        
        public void OnWordSelected(int position, string word)
        {
            LogAttempt();
            LogInteraction("word_selection");
            
            playerAnswers[position] = word;
            
            if (playerAnswers.Count >= correctWords.Length)
            {
                CheckAnswers();
            }
        }
        
        private void CheckAnswers()
        {
            LogAttempt();
            LogInteraction("answer_check");
            
            bool allCorrect = true;
            int correctCount = 0;
            
            for (int i = 0; i < wordInputs.Length && i < correctWords.Length; i++)
            {
                if (wordInputs[i] != null)
                {
                    string userInput = wordInputs[i].text.ToUpper().Trim();
                    
                    if (userInput == correctWords[i])
                    {
                        correctCount++;
                        wordInputs[i].image.color = Color.green;
                    }
                    else
                    {
                        allCorrect = false;
                        wordInputs[i].image.color = Color.red;
                    }
                }
            }
            
            if (progressText)
                progressText.text = "Correct words: " + correctCount + "/" + correctWords.Length;
            
            if (allCorrect)
            {
                CompletePuzzle();
            }
            else
            {
                if (instructionText)
                    instructionText.text = "Some words are incorrect. Try again!";
                
                // Reset colors after a delay
                Invoke(nameof(ResetInputColors), 2f);
            }
        }
        
        private void ResetInputColors()
        {
            foreach (var input in wordInputs)
            {
                if (input != null)
                    input.image.color = Color.white;
            }
            
            if (instructionText)
                instructionText.text = "Fill in the missing hieroglyphic words to complete the ancient text!";
        }
        
        protected override void CompletePuzzle()
        {
            if (instructionText)
                instructionText.text = "Puzzle Completed! The ancient scroll reveals its secrets!";
            
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
        
        private void CreateScrollInterface()
        {
            // Legacy method - can be expanded later for dynamic UI creation
            // For now, UI elements are assigned in the inspector
        }
    }
}