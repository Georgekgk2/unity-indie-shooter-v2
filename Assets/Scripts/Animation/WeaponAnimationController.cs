using UnityEngine;
using IndieShooter.Core;

namespace IndieShooter.Animation
{
    public class WeaponAnimationController : MonoBehaviour
    {
        [Header("Weapon Sway")]
        public float swayAmount = 0.02f;
        public float swaySpeed = 2f;
        public float swayResetSpeed = 2f;
        public bool enableSway = true;
        
        [Header("Weapon Bob")]
        public float bobAmount = 0.05f;
        public float bobSpeed = 10f;
        public bool enableBob = true;
        
        [Header("Recoil Settings")]
        public Vector3 recoilRotation = new Vector3(-2f, 0.5f, 0f);
        public Vector3 recoilPosition = new Vector3(0f, 0f, -0.1f);
        public float recoilSpeed = 10f;
        public float recoilReturnSpeed = 5f;
        
        [Header("Reload Animation")]
        public Vector3 reloadRotation = new Vector3(30f, -20f, 0f);
        public Vector3 reloadPosition = new Vector3(-0.2f, -0.3f, 0.1f);
        public float reloadAnimationSpeed = 2f;
        
        private Vector3 originalPosition;
        private Vector3 originalRotation;
        private Vector3 currentRecoil;
        private Vector3 targetRecoil;
        private bool isReloading = false;
        private bool isAiming = false;
        private float bobTimer = 0f;
        private CharacterController characterController;
        
        void Start()
        {
            originalPosition = transform.localPosition;
            originalRotation = transform.localEulerAngles;
            characterController = GetComponentInParent<CharacterController>();
            
            // Subscribe to weapon events
            EventSystem.Instance?.Subscribe("WeaponFired", OnWeaponFired);
            EventSystem.Instance?.Subscribe("WeaponReloading", OnWeaponReloading);
            EventSystem.Instance?.Subscribe("WeaponReloaded", OnWeaponReloaded);
        }
        
        void Update()
        {
            if (isReloading)
            {
                HandleReloadAnimation();
            }
            else
            {
                HandleWeaponSway();
                HandleWeaponBob();
                HandleRecoil();
            }
        }
        
        void HandleWeaponSway()
        {
            if (!enableSway) return;
            
            // Get mouse input
            float mouseX = Input.GetAxis("Mouse X");
            float mouseY = Input.GetAxis("Mouse Y");
            
            // Calculate sway
            Vector3 targetSway = new Vector3(-mouseY, mouseX, 0) * swayAmount;
            
            // Apply sway with smoothing
            Vector3 currentSway = Vector3.Lerp(transform.localPosition, originalPosition + targetSway, Time.deltaTime * swaySpeed);
            
            // Reset to original position when not moving mouse
            if (Mathf.Abs(mouseX) < 0.01f && Mathf.Abs(mouseY) < 0.01f)
            {
                currentSway = Vector3.Lerp(transform.localPosition, originalPosition, Time.deltaTime * swayResetSpeed);
            }
            
            transform.localPosition = currentSway;
        }
        
        void HandleWeaponBob()
        {
            if (!enableBob || characterController == null) return;
            
            // Check if player is moving
            Vector3 velocity = characterController.velocity;
            velocity.y = 0;
            float speed = velocity.magnitude;
            
            if (speed > 0.1f && characterController.isGrounded)
            {
                bobTimer += Time.deltaTime * bobSpeed * speed;
                
                // Calculate bob offset
                float bobX = Mathf.Sin(bobTimer) * bobAmount;
                float bobY = Mathf.Sin(bobTimer * 2) * bobAmount * 0.5f;
                
                Vector3 bobOffset = new Vector3(bobX, bobY, 0);
                transform.localPosition = originalPosition + bobOffset + currentRecoil;
            }
            else
            {
                bobTimer = 0f;
                transform.localPosition = Vector3.Lerp(transform.localPosition, originalPosition + currentRecoil, Time.deltaTime * swayResetSpeed);
            }
        }
        
        void HandleRecoil()
        {
            // Smoothly return recoil to zero
            currentRecoil = Vector3.Lerp(currentRecoil, targetRecoil, Time.deltaTime * recoilSpeed);
            targetRecoil = Vector3.Lerp(targetRecoil, Vector3.zero, Time.deltaTime * recoilReturnSpeed);
            
            // Apply recoil to rotation
            Vector3 recoilRot = Vector3.Lerp(Vector3.zero, recoilRotation, currentRecoil.magnitude);
            transform.localEulerAngles = originalRotation + recoilRot;
        }
        
        void HandleReloadAnimation()
        {
            // Animate weapon during reload
            Vector3 targetPos = originalPosition + reloadPosition;
            Vector3 targetRot = originalRotation + reloadRotation;
            
            transform.localPosition = Vector3.Lerp(transform.localPosition, targetPos, Time.deltaTime * reloadAnimationSpeed);
            transform.localEulerAngles = Vector3.Lerp(transform.localEulerAngles, targetRot, Time.deltaTime * reloadAnimationSpeed);
        }
        
        public void TriggerRecoil()
        {
            targetRecoil += recoilPosition;
            currentRecoil += recoilPosition * 0.5f;
        }
        
        public void SetAiming(bool aiming)
        {
            isAiming = aiming;
            
            // Reduce sway and bob when aiming
            if (aiming)
            {
                swayAmount *= 0.3f;
                bobAmount *= 0.2f;
            }
            else
            {
                swayAmount = 0.02f;
                bobAmount = 0.05f;
            }
        }
        
        // Event handlers
        void OnWeaponFired(object data)
        {
            TriggerRecoil();
        }
        
        void OnWeaponReloading(object data)
        {
            isReloading = true;
        }
        
        void OnWeaponReloaded(object data)
        {
            isReloading = false;
            
            // Return to original position
            StartCoroutine(ReturnToOriginalPosition());
        }
        
        System.Collections.IEnumerator ReturnToOriginalPosition()
        {
            float timer = 0f;
            Vector3 startPos = transform.localPosition;
            Vector3 startRot = transform.localEulerAngles;
            
            while (timer < 1f)
            {
                timer += Time.deltaTime * reloadAnimationSpeed;
                
                transform.localPosition = Vector3.Lerp(startPos, originalPosition, timer);
                transform.localEulerAngles = Vector3.Lerp(startRot, originalRotation, timer);
                
                yield return null;
            }
            
            transform.localPosition = originalPosition;
            transform.localEulerAngles = originalRotation;
        }
        
        void OnDestroy()
        {
            EventSystem.Instance?.Unsubscribe("WeaponFired", OnWeaponFired);
            EventSystem.Instance?.Unsubscribe("WeaponReloading", OnWeaponReloading);
            EventSystem.Instance?.Unsubscribe("WeaponReloaded", OnWeaponReloaded);
        }
    }
}