using UnityEngine;
using IndieShooter.Core;

namespace IndieShooter.Animation
{
    [RequireComponent(typeof(Animator))]
    public class PlayerAnimationController : MonoBehaviour
    {
        [Header("Animation Settings")]
        public float walkSpeedThreshold = 0.1f;
        public float runSpeedThreshold = 6f;
        public float smoothTime = 0.1f;
        
        [Header("Weapon Animation")]
        public Transform weaponHolder;
        public Vector3 aimPosition = new Vector3(0, 0, 0.5f);
        public Vector3 hipPosition = new Vector3(0.3f, -0.2f, 0.3f);
        public float weaponSwitchSpeed = 5f;
        
        private Animator animator;
        private CharacterController characterController;
        private float currentSpeed;
        private float targetSpeed;
        private bool isAiming = false;
        private bool isReloading = false;
        private bool isShooting = false;
        
        // Animation parameter hashes for performance
        private int speedHash;
        private int isWalkingHash;
        private int isRunningHash;
        private int isJumpingHash;
        private int isAimingHash;
        private int isReloadingHash;
        private int shootTriggerHash;
        
        void Start()
        {
            animator = GetComponent<Animator>();
            characterController = GetComponent<CharacterController>();
            
            // Cache animation parameter hashes
            speedHash = Animator.StringToHash("Speed");
            isWalkingHash = Animator.StringToHash("IsWalking");
            isRunningHash = Animator.StringToHash("IsRunning");
            isJumpingHash = Animator.StringToHash("IsJumping");
            isAimingHash = Animator.StringToHash("IsAiming");
            isReloadingHash = Animator.StringToHash("IsReloading");
            shootTriggerHash = Animator.StringToHash("Shoot");
            
            // Subscribe to events
            EventSystem.Instance?.Subscribe("WeaponFired", OnWeaponFired);
            EventSystem.Instance?.Subscribe("WeaponReloading", OnWeaponReloading);
            EventSystem.Instance?.Subscribe("WeaponReloaded", OnWeaponReloaded);
        }
        
        void Update()
        {
            UpdateMovementAnimation();
            UpdateWeaponAnimation();
            HandleInput();
        }
        
        void UpdateMovementAnimation()
        {
            if (characterController == null) return;
            
            // Calculate movement speed
            Vector3 velocity = characterController.velocity;
            velocity.y = 0; // Ignore vertical movement for speed calculation
            targetSpeed = velocity.magnitude;
            
            // Smooth speed transition
            currentSpeed = Mathf.Lerp(currentSpeed, targetSpeed, Time.deltaTime / smoothTime);
            
            // Update animator parameters
            animator.SetFloat(speedHash, currentSpeed);
            animator.SetBool(isWalkingHash, currentSpeed > walkSpeedThreshold);
            animator.SetBool(isRunningHash, currentSpeed > runSpeedThreshold);
            animator.SetBool(isJumpingHash, !characterController.isGrounded);
        }
        
        void UpdateWeaponAnimation()
        {
            if (weaponHolder == null) return;
            
            // Animate weapon position based on aiming state
            Vector3 targetPosition = isAiming ? aimPosition : hipPosition;
            weaponHolder.localPosition = Vector3.Lerp(
                weaponHolder.localPosition, 
                targetPosition, 
                Time.deltaTime * weaponSwitchSpeed
            );
            
            // Update aiming animation
            animator.SetBool(isAimingHash, isAiming);
            animator.SetBool(isReloadingHash, isReloading);
        }
        
        void HandleInput()
        {
            // Check for aiming input
            if (Input.GetButton("Fire2"))
            {
                SetAiming(true);
            }
            else
            {
                SetAiming(false);
            }
        }
        
        public void SetAiming(bool aiming)
        {
            isAiming = aiming;
        }
        
        public void PlayShootAnimation()
        {
            animator.SetTrigger(shootTriggerHash);
            isShooting = true;
            
            // Reset shooting flag after a short delay
            Invoke("ResetShooting", 0.1f);
        }
        
        public void PlayReloadAnimation()
        {
            isReloading = true;
        }
        
        public void StopReloadAnimation()
        {
            isReloading = false;
        }
        
        public void PlayJumpAnimation()
        {
            // Jump animation is handled automatically by IsJumping parameter
        }
        
        public void PlayLandAnimation()
        {
            // Landing animation can be triggered here if needed
        }
        
        void ResetShooting()
        {
            isShooting = false;
        }
        
        // Event handlers
        void OnWeaponFired(object data)
        {
            PlayShootAnimation();
        }
        
        void OnWeaponReloading(object data)
        {
            PlayReloadAnimation();
        }
        
        void OnWeaponReloaded(object data)
        {
            StopReloadAnimation();
        }
        
        // Public methods for external control
        public void SetMovementSpeed(float speed)
        {
            targetSpeed = speed;
        }
        
        public void TriggerCustomAnimation(string triggerName)
        {
            animator.SetTrigger(triggerName);
        }
        
        public void SetBoolParameter(string paramName, bool value)
        {
            animator.SetBool(paramName, value);
        }
        
        public void SetFloatParameter(string paramName, float value)
        {
            animator.SetFloat(paramName, value);
        }
        
        void OnDestroy()
        {
            EventSystem.Instance?.Unsubscribe("WeaponFired", OnWeaponFired);
            EventSystem.Instance?.Unsubscribe("WeaponReloading", OnWeaponReloading);
            EventSystem.Instance?.Unsubscribe("WeaponReloaded", OnWeaponReloaded);
        }
    }
}