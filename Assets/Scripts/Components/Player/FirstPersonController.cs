using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;
using CuriousCityAutomated.Gameplay.Interactions;

namespace CuriousCity.Core
{
    [RequireComponent(typeof(CharacterController))]
    public class FirstPersonController : MonoBehaviour
    {
        [Header("Movement Settings")]
        [SerializeField] private float walkSpeed = 5f;
        [SerializeField] private float runSpeed = 8f;
        [SerializeField] private float jumpHeight = 1.5f;
        [SerializeField] private float gravity = -9.81f;
        [SerializeField] private float groundCheckDistance = 0.4f;
        [SerializeField] private LayerMask groundMask = -1;
        
        [Header("Look Settings")]
        [SerializeField] private float mouseSensitivity = 2f;
        [SerializeField] private float lookXLimit = 85f;
        [SerializeField] private bool invertY = false;
        
        [Header("Camera")]
        [SerializeField] private Camera playerCamera;
        [SerializeField] private float cameraHeight = 1.7f;
        
        [Header("Analytics")]
        [SerializeField] private Analytics.LearningStyleTracker learningStyleTracker;
        [SerializeField] private Analytics.Analyzers.MovementAnalyzer movementAnalyzer;
        [SerializeField] private bool enableAnalytics = true;
        [SerializeField] private float analyticsUpdateInterval = 0.5f;
        
        // Components
        private CharacterController controller;
        private PlayerInput playerInput;
        
        // Movement state
        private Vector3 velocity;
        private Vector2 moveInput;
        private Vector2 lookInput;
        private bool isGrounded;
        private bool isRunning;
        private bool jumpPressed;
        private float currentSpeed;
        
        // Camera state
        private float rotationX = 0f;
        private float lastAnalyticsUpdate;
        
        // Interaction
        private GameObject currentInteractable;
        private float interactionStartTime;
        
        // Control flags for enabling/disabling player controls
        private bool canMove = true;
        private bool canLook = true;
        private bool canJump = true;
        private bool canRun = true;
        
        // UI Elements
        private GameObject interactionPrompt;
        private TMPro.TextMeshProUGUI interactionText;
        private UnityEngine.UI.Image crosshair;
        private Color defaultCrosshairColor = Color.white;
        
        private void Awake()
        {
            // Get required components
            controller = GetComponent<CharacterController>();
            if (controller == null)
            {
                Debug.LogError("[FirstPersonController] CharacterController component is missing!");
                enabled = false;
                return;
            }

            // Setup camera
            if (playerCamera == null)
            {
                playerCamera = GetComponentInChildren<Camera>();
                if (playerCamera == null)
                {
                    playerCamera = Camera.main;
                }
            }
            
            if (playerCamera != null)
            {
                playerCamera.transform.localPosition = new Vector3(0, cameraHeight, 0);
            }
            
            // Setup input
            playerInput = GetComponent<PlayerInput>();
            if (playerInput == null)
            {
                playerInput = gameObject.AddComponent<PlayerInput>();
            }
            
            // Find analytics components if not assigned
            if (enableAnalytics)
            {
                if (learningStyleTracker == null)
                {
                    learningStyleTracker = FindObjectOfType<Analytics.LearningStyleTracker>();
                    if (learningStyleTracker == null)
                    {
                        // Create one if it doesn't exist
                        GameObject analyticsObj = new GameObject("LearningStyleTracker");
                        learningStyleTracker = analyticsObj.AddComponent<Analytics.LearningStyleTracker>();
                    }
                }
                
                if (movementAnalyzer == null)
                {
                    movementAnalyzer = FindObjectOfType<Analytics.Analyzers.MovementAnalyzer>();
                }
            }
            
            // Lock cursor
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            
            lastAnalyticsUpdate = Time.time;
            
            // Final initialization
            currentSpeed = walkSpeed;
        }

        private void CreateRuntimeUI()
        {
            // Create interaction prompt
            if (interactionPrompt == null)
            {
                GameObject canvas = new GameObject("InteractionCanvas");
                Canvas c = canvas.AddComponent<Canvas>();
                c.renderMode = RenderMode.ScreenSpaceOverlay;
                canvas.AddComponent<UnityEngine.UI.CanvasScaler>();
                canvas.AddComponent<UnityEngine.UI.GraphicRaycaster>();
                
                interactionPrompt = new GameObject("InteractionPrompt");
                interactionPrompt.transform.SetParent(canvas.transform);
                
                interactionText = interactionPrompt.AddComponent<TMPro.TextMeshProUGUI>();
                interactionText.text = "Press E to interact";
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
                GameObject crosshairGO = new GameObject("Crosshair");
                crosshairGO.transform.SetParent(GameObject.Find("InteractionCanvas").transform);
                
                crosshair = crosshairGO.AddComponent<UnityEngine.UI.Image>();
                crosshair.color = defaultCrosshairColor;
                
                RectTransform crt = crosshairGO.GetComponent<RectTransform>();
                crt.anchorMin = new Vector2(0.5f, 0.5f);
                crt.anchorMax = new Vector2(0.5f, 0.5f);
                crt.anchoredPosition = Vector2.zero;
                crt.sizeDelta = new Vector2(10, 10);
            }
        }
        
        private void Start()
        {
            CreateRuntimeUI();
        }
        
        private void Update()
        {
            if (controller == null)
            {
                return;
            }
            
            // Handle input
            HandleInput();
            
            // Update movement
            UpdateMovement();
            
            // Update camera look
            UpdateLook();
            
            // Track behavior for analytics
            if (enableAnalytics && Time.time - lastAnalyticsUpdate >= analyticsUpdateInterval)
            {
                TrackBehavior();
                lastAnalyticsUpdate = Time.time;
            }
            
            // Check for interactions
            CheckInteractions();
        }
        
        private void HandleInput()
        {
            // Get input from Input System or legacy input
            if (playerInput != null && playerInput.currentActionMap != null)
            {
                // Using new Input System
                var moveAction = playerInput.currentActionMap.FindAction("Move");
                var lookAction = playerInput.currentActionMap.FindAction("Look");
                var jumpAction = playerInput.currentActionMap.FindAction("Jump");
                var runAction = playerInput.currentActionMap.FindAction("Run");
                
                if (moveAction != null) moveInput = canMove ? moveAction.ReadValue<Vector2>() : Vector2.zero;
                if (lookAction != null) lookInput = canLook ? lookAction.ReadValue<Vector2>() : Vector2.zero;
                if (jumpAction != null) jumpPressed = canJump && jumpAction.WasPressedThisFrame();
                if (runAction != null) isRunning = canRun && runAction.IsPressed();
            }
            else
            {
                // Fallback to legacy input
                moveInput = canMove ? new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical")) : Vector2.zero;
                lookInput = canLook ? new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y")) : Vector2.zero;
                jumpPressed = canJump && Input.GetButtonDown("Jump");
                isRunning = canRun && Input.GetKey(KeyCode.LeftShift);
            }
            
            // Update speed based on running state
            currentSpeed = isRunning ? runSpeed : walkSpeed;
            
            // Handle escape key for cursor
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Cursor.lockState = Cursor.lockState == CursorLockMode.Locked ? 
                                  CursorLockMode.None : CursorLockMode.Locked;
                Cursor.visible = !Cursor.visible;
            }
        }
        
        private void UpdateMovement()
        {
            if (!canMove && !canJump)
            {
                return;
            }
            
            // Ground check
            isGrounded = Physics.CheckSphere(
                transform.position - new Vector3(0, controller.height / 2 - controller.radius + 0.1f, 0),
                groundCheckDistance,
                groundMask
            );
            
            if (isGrounded && velocity.y < 0)
            {
                velocity.y = -2f; // Small downward force to keep grounded
            }
            
            // Calculate movement
            if (canMove)
            {
                Vector3 move = transform.right * moveInput.x + transform.forward * moveInput.y;
                move = Vector3.ClampMagnitude(move, 1f); // Normalize diagonal movement
                controller.Move(move * currentSpeed * Time.deltaTime);
            }
            
            // Jump
            if (jumpPressed && isGrounded && canJump)
            {
                velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
                
                // Track jump for analytics
                if (enableAnalytics && learningStyleTracker != null)
                {
                    learningStyleTracker.LogInteraction("Jump", 0f);
                }
            }
            
            // Apply gravity
            velocity.y += gravity * Time.deltaTime;
            controller.Move(velocity * Time.deltaTime);
        }
        
        private void UpdateLook()
        {
            if (!canLook || playerCamera == null)
            {
                return;
            }
            
            // Calculate rotation
            float mouseX = lookInput.x * mouseSensitivity;
            float mouseY = lookInput.y * mouseSensitivity * (invertY ? 1f : -1f);
            
            // Rotate the player body horizontally
            transform.Rotate(Vector3.up * mouseX);
            
            // Rotate the camera vertically
            rotationX += mouseY;
            rotationX = Mathf.Clamp(rotationX, -lookXLimit, lookXLimit);
            playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);
        }
        
        private void CheckInteractions()
        {
            if (playerCamera == null)
            {
                return;
            }
            
            // Raycast for interactables
            Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
            RaycastHit hit;
            
            if (Physics.Raycast(ray, out hit, 5f))
            {
                GameObject hitObject = hit.collider.gameObject;
                
                // Check if it's interactable
                if (hitObject.CompareTag("Interactable") || 
                    hitObject.GetComponent<IInteractable>() != null)
                {
                    if (currentInteractable != hitObject)
                    {
                        // New interactable found
                        currentInteractable = hitObject;
                        interactionStartTime = Time.time;
                    }
                    
                    // Check for interaction input
                    if (Input.GetKeyDown(KeyCode.E))
                    {
                        InteractWith(hitObject);
                    }
                }
                else
                {
                    currentInteractable = null;
                }
            }
            else
            {
                currentInteractable = null;
            }
        }
        
        private void InteractWith(GameObject target)
        {
            // Try to get IInteractable interface
            var interactable = target.GetComponent<IInteractable>();
            if (interactable != null)
            {
                // Pass the GameObject that has the FirstPersonController
                interactable.Interact(gameObject);
            }
            
            // Track interaction for analytics
            if (enableAnalytics && learningStyleTracker != null)
            {
                float duration = Time.time - interactionStartTime;
                learningStyleTracker.LogInteraction(target.name, duration);
            }
        }
        
        private void TrackBehavior()
        {
            if (!enableAnalytics)
            {
                return;
            }
            
            TrackCameraBehavior();
            
            // Update play time
            if (learningStyleTracker != null)
            {
                learningStyleTracker.UpdatePlayTime(analyticsUpdateInterval);
            }
        }
        
        private void TrackCameraBehavior()
        {
            if (learningStyleTracker == null || playerCamera == null)
            {
                return;
            }
            
            // Get what the player is looking at
            string target = null;
            Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
            RaycastHit hit;
            
            if (Physics.Raycast(ray, out hit, 50f))
            {
                target = hit.collider.name;
            }
            
            // Log camera behavior with safe null checks
            try
            {
                learningStyleTracker.LogCameraBehavior(
                    playerCamera.transform.forward,
                    analyticsUpdateInterval,
                    target
                );
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[FirstPersonController] Error tracking camera behavior: {e.Message}");
            }
        }
        
        // ===== PUBLIC API FOR EXTERNAL CONTROL =====
        
        /// <summary>
        /// Enable or disable player controls
        /// </summary>
        public void SetControlsEnabled(bool move, bool look, bool jump, bool run)
        {
            canMove = move;
            canLook = look;
            canJump = jump;
            canRun = run;
            
            // If disabling look, reset input to prevent stuck rotation
            if (!look)
            {
                lookInput = Vector2.zero;
            }
            
            // If disabling move, reset input to prevent stuck movement
            if (!move)
            {
                moveInput = Vector2.zero;
            }
        }
        
        /// <summary>
        /// Enable or disable movement only
        /// </summary>
        public void SetMovementEnabled(bool enabled)
        {
            canMove = enabled;
            if (!enabled)
            {
                moveInput = Vector2.zero;
            }
        }
        
        /// <summary>
        /// Enable or disable look controls only
        /// </summary>
        public void SetLookEnabled(bool enabled)
        {
            canLook = enabled;
            if (!enabled)
            {
                lookInput = Vector2.zero;
            }
        }
        
        /// <summary>
        /// Teleport the player to a specific position and rotation
        /// </summary>
        public void TeleportTo(Vector3 position, Vector3 eulerAngles)
        {
            // Disable controller temporarily
            controller.enabled = false;
            
            // Set position and rotation
            transform.position = position;
            transform.eulerAngles = new Vector3(0, eulerAngles.y, 0);
            
            // Set camera rotation
            if (playerCamera != null)
            {
                rotationX = eulerAngles.x;
                playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);
            }
            
            // Re-enable controller
            controller.enabled = true;
            
            // Reset velocity
            velocity = Vector3.zero;
        }
        
        /// <summary>
        /// Lock or unlock the cursor
        /// </summary>
        public void LockCursor(bool locked)
        {
            Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !locked;
        }
        
        /// <summary>
        /// Get the current movement input
        /// </summary>
        public Vector3 GetMovementInput()
        {
            return new Vector3(moveInput.x, 0, moveInput.y);
        }
        
        /// <summary>
        /// Check if the player is currently moving
        /// </summary>
        public bool IsMoving()
        {
            return moveInput.magnitude > 0.1f;
        }
        
        /// <summary>
        /// Check if the player is currently running
        /// </summary>
        public bool IsRunning()
        {
            return isRunning;
        }
        
        /// <summary>
        /// Check if the player is grounded
        /// </summary>
        public bool IsGrounded()
        {
            return isGrounded;
        }
        
        private void OnDrawGizmosSelected()
        {
            // Draw ground check sphere
            Gizmos.color = isGrounded ? Color.green : Color.red;
            if (controller != null)
            {
                Gizmos.DrawWireSphere(
                    transform.position - new Vector3(0, controller.height / 2 - controller.radius + 0.1f, 0),
                    groundCheckDistance
                );
            }
            
            // Draw interaction ray
            if (playerCamera != null)
            {
                Gizmos.color = currentInteractable != null ? Color.yellow : Color.blue;
                Gizmos.DrawRay(playerCamera.transform.position, playerCamera.transform.forward * 5f);
            }
        }
        
        private void OnApplicationFocus(bool hasFocus)
        {
            if (hasFocus && canLook)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }
        
        private void OnDestroy()
        {
            // Unlock cursor when destroyed
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
    
    // Simple interface for interactables
    public interface IInteractable
    {
        void Interact(GameObject interactor);
    }
}