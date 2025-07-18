using UnityEngine;
using IndieShooter.Core;

namespace IndieShooter.Player
{
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(AudioSource))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Movement Settings")]
        public float walkSpeed = 6f;
        public float runSpeed = 12f;
        public float jumpSpeed = 8f;
        public float gravity = 20f;
        public float lookSpeed = 2f;
        public float lookXLimit = 45f;
        
        [Header("Audio")]
        public AudioClip[] footstepSounds;
        public AudioClip jumpSound;
        public AudioClip landSound;
        
        private CharacterController characterController;
        private AudioSource audioSource;
        private Camera playerCamera;
        private Vector3 moveDirection = Vector3.zero;
        private float rotationX = 0;
        private bool canMove = true;
        private bool isGrounded = false;
        private bool wasGrounded = false;
        
        void Start()
        {
            characterController = GetComponent<CharacterController>();
            audioSource = GetComponent<AudioSource>();
            playerCamera = GetComponentInChildren<Camera>();
            
            if (playerCamera == null)
            {
                playerCamera = Camera.main;
            }
            
            // Lock cursor to center of screen
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            
            // Subscribe to events
            EventSystem.Instance?.Subscribe("GamePaused", OnGamePaused);
        }
        
        void Update()
        {
            if (!canMove) return;
            
            HandleMovement();
            HandleMouseLook();
            HandleAudio();
        }
        
        void HandleMovement()
        {
            isGrounded = characterController.isGrounded;
            
            // Handle landing sound
            if (isGrounded && !wasGrounded && landSound != null)
            {
                audioSource.PlayOneShot(landSound);
            }
            wasGrounded = isGrounded;
            
            if (isGrounded)
            {
                // Get input
                float horizontal = Input.GetAxis("Horizontal");
                float vertical = Input.GetAxis("Vertical");
                
                // Calculate movement direction
                Vector3 forward = transform.TransformDirection(Vector3.forward);
                Vector3 right = transform.TransformDirection(Vector3.right);
                
                // Determine speed (walk or run)
                bool isRunning = Input.GetKey(KeyCode.LeftShift);
                float currentSpeed = isRunning ? runSpeed : walkSpeed;
                
                // Apply movement
                moveDirection = (forward * vertical + right * horizontal) * currentSpeed;
                
                // Jump
                if (Input.GetButtonDown("Jump"))
                {
                    moveDirection.y = jumpSpeed;
                    if (jumpSound != null)
                    {
                        audioSource.PlayOneShot(jumpSound);
                    }
                }
            }
            
            // Apply gravity
            moveDirection.y -= gravity * Time.deltaTime;
            
            // Move the character
            characterController.Move(moveDirection * Time.deltaTime);
        }
        
        void HandleMouseLook()
        {
            if (playerCamera == null) return;
            
            // Rotate the camera around the local x axis (look up and down)
            rotationX += -Input.GetAxis("Mouse Y") * lookSpeed;
            rotationX = Mathf.Clamp(rotationX, -lookXLimit, lookXLimit);
            playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);
            
            // Rotate the player around the y axis (look left and right)
            transform.rotation *= Quaternion.Euler(0, Input.GetAxis("Mouse X") * lookSpeed, 0);
        }
        
        void HandleAudio()
        {
            // Play footstep sounds
            if (isGrounded && characterController.velocity.magnitude > 0.1f)
            {
                if (!audioSource.isPlaying && footstepSounds.Length > 0)
                {
                    AudioClip footstep = footstepSounds[Random.Range(0, footstepSounds.Length)];
                    audioSource.clip = footstep;
                    audioSource.Play();
                }
            }
        }
        
        void OnGamePaused(object isPaused)
        {
            canMove = !(bool)isPaused;
            
            if ((bool)isPaused)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }
        
        void OnDestroy()
        {
            EventSystem.Instance?.Unsubscribe("GamePaused", OnGamePaused);
        }
    }
}