using System;
using System.Collections.Generic;
using UnityEngine;

namespace CuriousCity.Analytics.Analyzers
{
    public class MovementAnalyzer : MonoBehaviour
    {
        [Serializable]
        public class MovementData
        {
            public float totalDistance;
            public float averageSpeed;
            public float maxSpeed;
            public float timeMoving;
            public float timeStationary;
            public int directionChanges;
            public List<Vector3> pathPoints;
            
            public MovementData()
            {
                totalDistance = 0f;
                averageSpeed = 0f;
                maxSpeed = 0f;
                timeMoving = 0f;
                timeStationary = 0f;
                directionChanges = 0;
                pathPoints = new List<Vector3>();
            }
        }
        
        [Header("References")]
        [SerializeField] private Transform playerTransform;
        [SerializeField] private LearningStyleTracker learningStyleTracker;
        
        [Header("Configuration")]
        [SerializeField] private float updateInterval = 0.5f;
        [SerializeField] private float movementThreshold = 0.1f;
        [SerializeField] private float stationaryThreshold = 0.01f;
        [SerializeField] private int maxPathPoints = 1000;
        [SerializeField] private bool enableDebugVisualization = false;
        
        // Movement tracking
        private MovementData currentData;
        private Vector3 lastPosition;
        private Vector3 lastDirection;
        private float lastUpdateTime;
        private float currentSpeed;
        private float speedAccumulator;
        private int speedSampleCount;
        
        // Area detection
        private string currentArea = "";
        private float areaEntryTime;
        private Dictionary<string, float> areaTimeSpent;
        
        // Events
        public event Action<MovementData> OnMovementAnalyzed;
        public event Action<string> OnAreaChanged;
        
        private void Awake()
        {
            InitializeAnalyzer();
        }
        
        private void InitializeAnalyzer()
        {
            currentData = new MovementData();
            areaTimeSpent = new Dictionary<string, float>();
            
            // Try to find player transform if not assigned
            if (playerTransform == null)
            {
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                if (player != null)
                {
                    playerTransform = player.transform;
                }
                else
                {
                    // Try to find by common names
                    player = GameObject.Find("Player") ?? 
                            GameObject.Find("FirstPersonController") ?? 
                            GameObject.Find("Character");
                    if (player != null)
                    {
                        playerTransform = player.transform;
                    }
                }
            }
            
            // Try to find LearningStyleTracker if not assigned
            if (learningStyleTracker == null)
            {
                learningStyleTracker = FindObjectOfType<LearningStyleTracker>();
            }
            
            // Initialize tracking variables
            if (playerTransform != null)
            {
                lastPosition = playerTransform.position;
                lastDirection = playerTransform.forward;
                currentData.pathPoints.Add(lastPosition);
            }
            
            lastUpdateTime = Time.time;
            speedAccumulator = 0f;
            speedSampleCount = 0;
            areaEntryTime = Time.time;
        }
        
        private void Start()
        {
            // Final validation check
            if (playerTransform == null)
            {
                Debug.LogError("[MovementAnalyzer] Player transform not found! Please assign it in the inspector.");
                enabled = false;
            }
        }
        
        private void Update()
        {
            // Safety check
            if (playerTransform == null)
            {
                return;
            }
            
            // Update at specified interval
            if (Time.time - lastUpdateTime >= updateInterval)
            {
                AnalyzeMovement();
                lastUpdateTime = Time.time;
            }
            
            // Continuous speed tracking
            TrackSpeed();
        }
        
        private void TrackSpeed()
        {
            if (playerTransform == null || currentData == null)
            {
                return;
            }
            
            Vector3 currentPosition = playerTransform.position;
            float distance = Vector3.Distance(currentPosition, lastPosition);
            float deltaTime = Time.deltaTime;
            
            if (deltaTime > 0)
            {
                currentSpeed = distance / deltaTime;
                speedAccumulator += currentSpeed;
                speedSampleCount++;
                
                // Update max speed
                if (currentSpeed > currentData.maxSpeed)
                {
                    currentData.maxSpeed = currentSpeed;
                }
                
                // Track movement vs stationary time
                if (currentSpeed > stationaryThreshold)
                {
                    currentData.timeMoving += deltaTime;
                }
                else
                {
                    currentData.timeStationary += deltaTime;
                }
            }
            
            lastPosition = currentPosition;
        }
        
        private void AnalyzeMovement()
        {
            // Safety checks
            if (playerTransform == null || currentData == null)
            {
                return;
            }
            
            Vector3 currentPosition = playerTransform.position;
            Vector3 currentDirection = playerTransform.forward;
            
            // Calculate distance traveled
            float distance = Vector3.Distance(currentPosition, lastPosition);
            if (distance > movementThreshold)
            {
                currentData.totalDistance += distance;
                
                // Add path point
                if (currentData.pathPoints.Count < maxPathPoints)
                {
                    currentData.pathPoints.Add(currentPosition);
                }
                else
                {
                    // Remove oldest point and add new one
                    currentData.pathPoints.RemoveAt(0);
                    currentData.pathPoints.Add(currentPosition);
                }
            }
            
            // Calculate average speed
            if (speedSampleCount > 0)
            {
                currentData.averageSpeed = speedAccumulator / speedSampleCount;
            }
            
            // Detect direction changes
            float angleChange = Vector3.Angle(lastDirection, currentDirection);
            if (angleChange > 30f) // Significant direction change
            {
                currentData.directionChanges++;
                lastDirection = currentDirection;
            }
            
            // Check for area changes
            CheckAreaChange(currentPosition);
            
            // Update learning style tracker if available
            if (learningStyleTracker != null && distance > movementThreshold)
            {
                learningStyleTracker.LogCameraBehavior(currentDirection, updateInterval, GetCurrentTarget());
            }
            
            // Trigger analysis event
            OnMovementAnalyzed?.Invoke(currentData);
        }
        
        private void CheckAreaChange(Vector3 position)
        {
            // Use trigger colliders or raycasts to detect current area
            Collider[] colliders = Physics.OverlapSphere(position, 1f);
            string detectedArea = "";
            
            foreach (Collider col in colliders)
            {
                if (col.CompareTag("Area") || col.CompareTag("Zone"))
                {
                    detectedArea = col.name;
                    break;
                }
            }
            
            // If no area detected, try using position-based naming
            if (string.IsNullOrEmpty(detectedArea))
            {
                detectedArea = GetAreaByPosition(position);
            }
            
            // Handle area change
            if (!string.IsNullOrEmpty(detectedArea) && detectedArea != currentArea)
            {
                // Log time spent in previous area
                if (!string.IsNullOrEmpty(currentArea))
                {
                    float timeInArea = Time.time - areaEntryTime;
                    
                    if (areaTimeSpent.ContainsKey(currentArea))
                    {
                        areaTimeSpent[currentArea] += timeInArea;
                    }
                    else
                    {
                        areaTimeSpent[currentArea] = timeInArea;
                    }
                    
                    // Update learning style tracker
                    if (learningStyleTracker != null)
                    {
                        learningStyleTracker.LogAreaVisit(currentArea, timeInArea);
                    }
                }
                
                // Update current area
                currentArea = detectedArea;
                areaEntryTime = Time.time;
                OnAreaChanged?.Invoke(currentArea);
            }
        }
        
        private string GetAreaByPosition(Vector3 position)
        {
            // Simple grid-based area naming based on position
            int gridX = Mathf.FloorToInt(position.x / 50f);
            int gridZ = Mathf.FloorToInt(position.z / 50f);
            return $"Zone_{gridX}_{gridZ}";
        }
        
        private string GetCurrentTarget()
        {
            // Try to detect what the player is looking at
            if (playerTransform == null)
            {
                return null;
            }
            
            Ray ray = new Ray(playerTransform.position, playerTransform.forward);
            RaycastHit hit;
            
            if (Physics.Raycast(ray, out hit, 50f))
            {
                // Return the name of the object being looked at
                return hit.collider.name;
            }
            
            return null;
        }
        
        public MovementData GetMovementData()
        {
            return currentData ?? new MovementData();
        }
        
        public Dictionary<string, float> GetAreaTimeData()
        {
            return new Dictionary<string, float>(areaTimeSpent);
        }
        
        public void ResetAnalysis()
        {
            InitializeAnalyzer();
        }
        
        private void OnDrawGizmos()
        {
            if (!enableDebugVisualization || currentData == null || currentData.pathPoints == null)
            {
                return;
            }
            
            // Draw movement path
            Gizmos.color = Color.green;
            for (int i = 1; i < currentData.pathPoints.Count; i++)
            {
                Gizmos.DrawLine(currentData.pathPoints[i - 1], currentData.pathPoints[i]);
            }
            
            // Draw current position
            if (playerTransform != null)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(playerTransform.position, 1f);
            }
        }
        
        private void OnDestroy()
        {
            // Clean up
            currentData?.pathPoints?.Clear();
            areaTimeSpent?.Clear();
        }
    }
}