using UnityEngine;
using System.Collections.Generic;

namespace IndieShooter.Player
{
    /// <summary>
    /// Система звуків кроків гравця. Обробляє відтворення звуків кроків
    /// залежно від поверхні, швидкості руху та стану гравця.
    /// </summary>
    public class PlayerFootsteps : MonoBehaviour
    {
        [Header("Footstep Audio")]
        [Tooltip("Звуки кроків для різних поверхонь")]
        public List<AudioClip> defaultFootsteps = new List<AudioClip>();
        [Tooltip("Звуки кроків по траві")]
        public List<AudioClip> grassFootsteps = new List<AudioClip>();
        [Tooltip("Звуки кроків по металу")]
        public List<AudioClip> metalFootsteps = new List<AudioClip>();
        [Tooltip("Звуки кроків по воді")]
        public List<AudioClip> waterFootsteps = new List<AudioClip>();
        
        [Header("Footstep Settings")]
        [Tooltip("Базовий інтервал між кроками при ходьбі")]
        public float walkStepInterval = 0.5f;
        [Tooltip("Інтервал між кроками при бігу")]
        public float runStepInterval = 0.3f;
        [Tooltip("Гучність звуків кроків")]
        [Range(0f, 1f)]
        public float footstepVolume = 0.7f;
        [Tooltip("Випадкова варіація висоти тону")]
        [Range(0f, 0.5f)]
        public float pitchVariation = 0.1f;
        
        // Посилання на компоненти
        private PlayerMovementCore movementCore;
        private AudioSource audioSource;
        
        // Стан кроків
        private float stepTimer = 0f;
        private bool wasMovingLastFrame = false;
        private string currentSurface = "default";
        
        public void Initialize(PlayerMovementCore core)
        {
            movementCore = core;
            SetupAudioSource();
        }
        
        void Start()
        {
            ValidateSetup();
        }
        
        /// <summary>
        /// Налаштовує AudioSource для відтворення звуків кроків
        /// </summary>
        private void SetupAudioSource()
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
            
            // Налаштування AudioSource
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 1f; // 3D звук
            audioSource.volume = footstepVolume;
            audioSource.minDistance = 1f;
            audioSource.maxDistance = 10f;
        }
        
        /// <summary>
        /// Перевіряє правильність налаштування
        /// </summary>
        private void ValidateSetup()
        {
            if (movementCore == null)
            {
                Debug.LogError("PlayerFootsteps: MovementCore not initialized!", this);
                enabled = false;
                return;
            }
            
            if (defaultFootsteps.Count == 0)
            {
                Debug.LogWarning("PlayerFootsteps: No default footstep sounds assigned!", this);
            }
        }
        
        /// <summary>
        /// Оновлює систему звуків кроків
        /// </summary>
        public void UpdateFootsteps()
        {
            if (!movementCore.IsGrounded()) return;
            
            Vector3 velocity = movementCore.GetCurrentVelocity();
            Vector3 horizontalVelocity = new Vector3(velocity.x, 0, velocity.z);
            float speed = horizontalVelocity.magnitude;
            
            bool isMoving = speed > 0.1f && movementCore.GetMoveInput().magnitude > 0.1f;
            
            if (isMoving)
            {
                UpdateStepTimer(speed);
                
                if (!wasMovingLastFrame)
                {
                    // Почали рухатися - скидаємо таймер
                    stepTimer = 0f;
                }
            }
            else
            {
                // Не рухаємося - скидаємо таймер
                stepTimer = 0f;
            }
            
            wasMovingLastFrame = isMoving;
        }
        
        /// <summary>
        /// Оновлює таймер кроків та відтворює звуки
        /// </summary>
        private void UpdateStepTimer(float speed)
        {
            // Визначаємо інтервал кроків залежно від швидкості
            float currentInterval = GetStepInterval(speed);
            
            stepTimer += Time.deltaTime;
            
            if (stepTimer >= currentInterval)
            {
                PlayFootstepSound();
                stepTimer = 0f;
            }
        }
        
        /// <summary>
        /// Визначає інтервал між кроками залежно від швидкості
        /// </summary>
        private float GetStepInterval(float speed)
        {
            // Отримуємо швидкості з movementCore
            float walkSpeed = 5f; // Можна отримати з movementCore
            float runSpeed = 10f;  // Можна отримати з movementCore
            
            if (speed > walkSpeed * 0.8f)
            {
                // Біг
                return runStepInterval;
            }
            else
            {
                // Ходьба
                return walkStepInterval;
            }
        }
        
        /// <summary>
        /// Відтворює звук кроку
        /// </summary>
        private void PlayFootstepSound()
        {
            List<AudioClip> footstepClips = GetFootstepClipsForSurface();
            
            if (footstepClips.Count == 0) return;
            
            // Вибираємо випадковий звук
            AudioClip clipToPlay = footstepClips[Random.Range(0, footstepClips.Count)];
            
            if (clipToPlay != null)
            {
                // Додаємо варіацію висоти тону
                float pitch = 1f + Random.Range(-pitchVariation, pitchVariation);
                audioSource.pitch = pitch;
                
                // Відтворюємо звук
                audioSource.PlayOneShot(clipToPlay, footstepVolume);
            }
        }
        
        /// <summary>
        /// Отримує список звуків кроків для поточної поверхні
        /// </summary>
        private List<AudioClip> GetFootstepClipsForSurface()
        {
            switch (currentSurface.ToLower())
            {
                case "grass":
                    return grassFootsteps.Count > 0 ? grassFootsteps : defaultFootsteps;
                case "metal":
                    return metalFootsteps.Count > 0 ? metalFootsteps : defaultFootsteps;
                case "water":
                    return waterFootsteps.Count > 0 ? waterFootsteps : defaultFootsteps;
                default:
                    return defaultFootsteps;
            }
        }
        
        /// <summary>
        /// Визначає тип поверхні під гравцем
        /// </summary>
        private void DetectSurface()
        {
            if (movementCore == null) return;
            
            Transform groundCheck = movementCore.GetGroundCheck();
            LayerMask groundMask = movementCore.GetGroundMask();
            
            if (groundCheck == null) return;
            
            // Виконуємо raycast вниз для визначення поверхні
            RaycastHit hit;
            if (Physics.Raycast(groundCheck.position, Vector3.down, out hit, 0.5f, groundMask))
            {
                // Визначаємо тип поверхні за тегом або назвою
                string surfaceType = DetermineSurfaceType(hit.collider);
                
                if (surfaceType != currentSurface)
                {
                    currentSurface = surfaceType;
                }
            }
        }
        
        /// <summary>
        /// Визначає тип поверхні за колайдером
        /// </summary>
        private string DetermineSurfaceType(Collider collider)
        {
            // Перевіряємо тег
            switch (collider.tag.ToLower())
            {
                case "grass":
                    return "grass";
                case "metal":
                    return "metal";
                case "water":
                    return "water";
            }
            
            // Перевіряємо назву об'єкта
            string objectName = collider.name.ToLower();
            if (objectName.Contains("grass") || objectName.Contains("terrain"))
                return "grass";
            if (objectName.Contains("metal") || objectName.Contains("steel"))
                return "metal";
            if (objectName.Contains("water") || objectName.Contains("liquid"))
                return "water";
            
            return "default";
        }
        
        // === Публічні методи ===
        
        /// <summary>
        /// Примусово відтворює звук кроку
        /// </summary>
        public void PlayFootstep()
        {
            PlayFootstepSound();
        }
        
        /// <summary>
        /// Встановлює тип поверхні вручну
        /// </summary>
        public void SetSurfaceType(string surfaceType)
        {
            currentSurface = surfaceType;
        }
        
        /// <summary>
        /// Отримує поточний тип поверхні
        /// </summary>
        public string GetCurrentSurface()
        {
            return currentSurface;
        }
        
        /// <summary>
        /// Встановлює гучність кроків
        /// </summary>
        public void SetFootstepVolume(float volume)
        {
            footstepVolume = Mathf.Clamp01(volume);
            if (audioSource != null)
            {
                audioSource.volume = footstepVolume;
            }
        }
    }
}