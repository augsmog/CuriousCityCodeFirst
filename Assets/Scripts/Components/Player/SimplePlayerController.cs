using UnityEngine;

namespace CuriousCity.Core
{
    public class SimplePlayerController : MonoBehaviour
    {
        [Header("Movement")]
        public float moveSpeed = 5f;
        public float jumpHeight = 2f;
        
        [Header("Mouse Look")]
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
            float horizontal = Input.GetAxis("Horizontal");
            float vertical = Input.GetAxis("Vertical");
            
            // Calculate movement
            Vector3 move = transform.right * horizontal + transform.forward * vertical;
            
            // Apply gravity
            if (controller.isGrounded)
            {
                verticalVelocity = -2f;
                
                // Jump
                if (Input.GetButtonDown("Jump"))
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
            float mouseX = Input.GetAxis("Mouse X") * mouseSensitivityX;
            float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivityY;
            
            // Rotate player body
            transform.Rotate(Vector3.up * mouseX);
            
            // Rotate camera
            xRotation -= mouseY;
            xRotation = Mathf.Clamp(xRotation, -maxLookAngle, maxLookAngle);
            playerCamera.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        }
    }
}