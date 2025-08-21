using System.Collections;
using UnityEngine;

namespace CuriousCity.Analytics
{
    /// <summary>
    /// Manages initialization order for analytics components to prevent null reference issues
    /// Place this on a GameObject that loads before other analytics components
    /// </summary>
    [DefaultExecutionOrder(-100)] // Ensures this runs before other components
    public class AnalyticsInitializationManager : MonoBehaviour
    {
        private static AnalyticsInitializationManager instance;
        
        [Header("Analytics Components")]
        [SerializeField] private bool autoCreateComponents = true;
        [SerializeField] private bool validateOnAwake = true;
        
        // Component references
        private LearningStyleTracker learningTracker;
        private GameObject analyticsContainer;
        
        // Status flags
        private bool isInitialized = false;
        public bool IsReady { get; private set; }
        
        public static AnalyticsInitializationManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindObjectOfType<AnalyticsInitializationManager>();
                    if (instance == null)
                    {
                        GameObject go = new GameObject("AnalyticsInitializationManager");
                        instance = go.AddComponent<AnalyticsInitializationManager>();
                    }
                }
                return instance;
            }
        }
        
        private void Awake()
        {
            // Singleton pattern
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            instance = this;
            DontDestroyOnLoad(gameObject);
            
            if (validateOnAwake)
            {
                StartCoroutine(InitializeAnalytics());
            }
        }
        
        private IEnumerator InitializeAnalytics()
        {
            Debug.Log("[AnalyticsInit] Starting analytics initialization...");
            
            // Wait one frame to ensure scene is loaded
            yield return null;
            
            // Step 1: Create analytics container if needed
            if (analyticsContainer == null)
            {
                analyticsContainer = GameObject.Find("AnalyticsSystem");
                if (analyticsContainer == null && autoCreateComponents)
                {
                    analyticsContainer = new GameObject("AnalyticsSystem");
                    DontDestroyOnLoad(analyticsContainer);
                }
            }
            
            // Step 2: Initialize LearningStyleTracker
            learningTracker = FindObjectOfType<LearningStyleTracker>();
            if (learningTracker == null && autoCreateComponents)
            {
                Debug.Log("[AnalyticsInit] Creating LearningStyleTracker...");
                learningTracker = analyticsContainer.AddComponent<LearningStyleTracker>();
            }
            
            // Wait for tracker to initialize
            yield return null;
            
            // Step 3: Find and setup MovementAnalyzer on player
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player == null)
            {
                // Try alternative search methods
                player = GameObject.Find("FirstPersonController") ?? 
                        GameObject.Find("Player") ?? 
                        GameObject.Find("PlayerController");
            }
            
            if (player != null)
            {
                var movementAnalyzer = player.GetComponent<Analyzers.MovementAnalyzer>();
                if (movementAnalyzer == null && autoCreateComponents)
                {
                    Debug.Log("[AnalyticsInit] Adding MovementAnalyzer to player...");
                    movementAnalyzer = player.AddComponent<Analyzers.MovementAnalyzer>();
                }
            }
            else
            {
                Debug.LogWarning("[AnalyticsInit] Player GameObject not found. MovementAnalyzer not initialized.");
            }
            
            // Step 4: Validate all components
            bool allComponentsReady = ValidateComponents();
            
            if (allComponentsReady)
            {
                Debug.Log("[AnalyticsInit] Analytics system initialized successfully!");
                IsReady = true;
                isInitialized = true;
            }
            else
            {
                Debug.LogWarning("[AnalyticsInit] Some analytics components could not be initialized.");
                IsReady = false;
            }
        }
        
        private bool ValidateComponents()
        {
            bool isValid = true;
            
            // Check LearningStyleTracker
            if (learningTracker == null)
            {
                learningTracker = FindObjectOfType<LearningStyleTracker>();
                if (learningTracker == null)
                {
                    Debug.LogError("[AnalyticsInit] LearningStyleTracker is missing!");
                    isValid = false;
                }
            }
            
            // Check for player components
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                var controller = player.GetComponent<Characters.FirstPersonController>();
                if (controller == null)
                {
                    Debug.LogWarning("[AnalyticsInit] FirstPersonController not found on player.");
                }
                
                var movementAnalyzer = player.GetComponent<Analyzers.MovementAnalyzer>();
                if (movementAnalyzer == null)
                {
                    Debug.LogWarning("[AnalyticsInit] MovementAnalyzer not found on player.");
                }
            }
            
            return isValid;
        }
        
        /// <summary>
        /// Force re-initialization of analytics system
        /// </summary>
        [ContextMenu("Reinitialize Analytics")]
        public void ReinitializeAnalytics()
        {
            Debug.Log("[AnalyticsInit] Force reinitializing analytics...");
            isInitialized = false;
            IsReady = false;
            StartCoroutine(InitializeAnalytics());
        }
        
        /// <summary>
        /// Get the learning tracker instance, creating if necessary
        /// </summary>
        public LearningStyleTracker GetLearningTracker()
        {
            if (learningTracker == null)
            {
                learningTracker = FindObjectOfType<LearningStyleTracker>();
                if (learningTracker == null && autoCreateComponents)
                {
                    if (analyticsContainer == null)
                    {
                        analyticsContainer = new GameObject("AnalyticsSystem");
                    }
                    learningTracker = analyticsContainer.AddComponent<LearningStyleTracker>();
                }
            }
            return learningTracker;
        }
        
        /// <summary>
        /// Clean up null references and re-validate
        /// </summary>
        public void CleanupNullReferences()
        {
            // Remove null references
            if (learningTracker == null)
            {
                learningTracker = FindObjectOfType<LearningStyleTracker>();
            }
            
            // Re-validate
            ValidateComponents();
        }
        
        private void OnApplicationPause(bool pauseStatus)
        {
            if (!pauseStatus && isInitialized)
            {
                // Re-validate on unpause
                CleanupNullReferences();
            }
        }
    }
}