using UnityEngine;
using IndieShooter.Audio;

namespace IndieShooter.Audio
{
    [RequireComponent(typeof(CharacterController))]
    public class FootstepController : MonoBehaviour
    {
        [Header("Footstep Settings")]
        public string[] walkFootstepSounds = { "FootstepWalk1", "FootstepWalk2", "FootstepWalk3" };
        public string[] runFootstepSounds = { "FootstepRun1", "FootstepRun2", "FootstepRun3" };
        public string jumpSound = "FootstepJump";
        public string landSound = "FootstepLand";
        
        [Header("Timing")]
        public float walkStepInterval = 0.5f;
        public float runStepInterval = 0.3f;
        public float minimumVelocity = 0.1f;
        
        [Header("Surface Detection")]
        public LayerMask groundLayerMask = -1;
        public float raycastDistance = 1.1f;
        
        private CharacterController characterController;
        private float stepTimer = 0f;
        private bool wasGrounded = true;
        private bool isMoving = false;
        private string currentSurfaceType = "Default";
        
        void Start()
        {
            characterController = GetComponent<CharacterController>();
        }
        
        void Update()
        {
            HandleFootsteps();
            HandleJumpAndLand();
        }
        
        void HandleFootsteps()
        {
            bool isGrounded = characterController.isGrounded;
            Vector3 velocity = characterController.velocity;
            velocity.y = 0; // Ignore vertical movement
            float speed = velocity.magnitude;
            
            isMoving = speed > minimumVelocity;
            
            if (isGrounded && isMoving)
            {
                // Determine if running or walking
                bool isRunning = Input.GetKey(KeyCode.LeftShift) && speed > 6f;
                float stepInterval = isRunning ? runStepInterval : walkStepInterval;
                
                stepTimer += Time.deltaTime;
                
                if (stepTimer >= stepInterval)
                {
                    PlayFootstepSound(isRunning);
                    stepTimer = 0f;
                }
            }
            else
            {
                stepTimer = 0f;
            }
        }
        
        void HandleJumpAndLand()
        {
            bool isGrounded = characterController.isGrounded;
            
            // Detect jump
            if (wasGrounded && !isGrounded && Input.GetButtonDown("Jump"))
            {
                PlayJumpSound();
            }
            
            // Detect landing
            if (!wasGrounded && isGrounded)
            {
                PlayLandSound();
            }
            
            wasGrounded = isGrounded;
        }
        
        void PlayFootstepSound(bool isRunning)
        {
            DetectSurfaceType();
            
            string[] soundArray = isRunning ? runFootstepSounds : walkFootstepSounds;
            
            if (soundArray.Length > 0)
            {
                string soundName = soundArray[Random.Range(0, soundArray.Length)];
                
                // Add surface type suffix if available
                string fullSoundName = soundName + currentSurfaceType;
                
                if (AudioManager.Instance != null)
                {
                    // Try surface-specific sound first, fallback to default
                    AudioManager.Instance.PlaySFXAtPosition(fullSoundName, transform.position);
                }
            }
        }
        
        void PlayJumpSound()
        {
            if (!string.IsNullOrEmpty(jumpSound) && AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFXAtPosition(jumpSound, transform.position);
            }
        }
        
        void PlayLandSound()
        {
            DetectSurfaceType();
            
            if (!string.IsNullOrEmpty(landSound) && AudioManager.Instance != null)
            {
                string fullSoundName = landSound + currentSurfaceType;
                AudioManager.Instance.PlaySFXAtPosition(fullSoundName, transform.position);
            }
        }
        
        void DetectSurfaceType()
        {
            RaycastHit hit;
            Vector3 rayOrigin = transform.position + Vector3.up * 0.1f;
            
            if (Physics.Raycast(rayOrigin, Vector3.down, out hit, raycastDistance, groundLayerMask))
            {
                // Determine surface type based on tag or material
                switch (hit.collider.tag)
                {
                    case "Ground":
                        currentSurfaceType = "Dirt";
                        break;
                    case "Wall":
                        currentSurfaceType = "Concrete";
                        break;
                    case "Metal":
                        currentSurfaceType = "Metal";
                        break;
                    case "Wood":
                        currentSurfaceType = "Wood";
                        break;
                    case "Water":
                        currentSurfaceType = "Water";
                        break;
                    default:
                        currentSurfaceType = "Default";
                        break;
                }
            }
            else
            {
                currentSurfaceType = "Default";
            }
        }
        
        // Public methods for external control
        public void SetFootstepVolume(float volume)
        {
            // This could be implemented to control footstep volume specifically
        }
        
        public void EnableFootsteps(bool enable)
        {
            enabled = enable;
        }
        
        // Debug visualization
        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Vector3 rayOrigin = transform.position + Vector3.up * 0.1f;
            Gizmos.DrawRay(rayOrigin, Vector3.down * raycastDistance);
        }
    }
}