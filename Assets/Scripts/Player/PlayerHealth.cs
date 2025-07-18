using UnityEngine;
using IndieShooter.Core;

namespace IndieShooter.Player
{
    public class PlayerHealth : MonoBehaviour
    {
        [Header("Health Settings")]
        public float maxHealth = 100f;
        public float currentHealth;
        
        [Header("Regeneration")]
        public bool enableHealthRegen = true;
        public float regenRate = 5f; // Health per second
        public float regenDelay = 5f; // Delay after taking damage
        
        [Header("Damage Effects")]
        public float damageScreenFlashDuration = 0.2f;
        public Color damageScreenColor = Color.red;
        
        private float lastDamageTime = 0f;
        private bool isDead = false;
        
        public bool IsDead => isDead;
        public float HealthPercentage => currentHealth / maxHealth;
        
        void Start()
        {
            currentHealth = maxHealth;
        }
        
        void Update()
        {
            HandleHealthRegeneration();
            UpdateHealthUI();
        }
        
        void HandleHealthRegeneration()
        {
            if (!enableHealthRegen || isDead || currentHealth >= maxHealth) return;
            
            // Regenerate health after delay
            if (Time.time - lastDamageTime > regenDelay)
            {
                currentHealth += regenRate * Time.deltaTime;
                currentHealth = Mathf.Min(currentHealth, maxHealth);
            }
        }
        
        public void TakeDamage(float damage)
        {
            if (isDead) return;
            
            currentHealth -= damage;
            currentHealth = Mathf.Max(0, currentHealth);
            lastDamageTime = Time.time;
            
            // Trigger damage effects
            EventSystem.Instance?.TriggerEvent("PlayerTakeDamage", damage);
            
            // Check for death
            if (currentHealth <= 0)
            {
                Die();
            }
            else
            {
                // Trigger low health warning
                if (HealthPercentage <= 0.25f)
                {
                    EventSystem.Instance?.TriggerEvent("PlayerHealthLow", HealthPercentage);
                }
            }
        }
        
        public void Heal(float amount)
        {
            if (isDead) return;
            
            currentHealth += amount;
            currentHealth = Mathf.Min(currentHealth, maxHealth);
            
            EventSystem.Instance?.TriggerEvent("PlayerHealed", amount);
        }
        
        public void SetHealth(float health)
        {
            currentHealth = Mathf.Clamp(health, 0, maxHealth);
            
            if (currentHealth <= 0 && !isDead)
            {
                Die();
            }
        }
        
        void Die()
        {
            isDead = true;
            currentHealth = 0;
            
            // Trigger death event
            EventSystem.Instance?.TriggerEvent("PlayerDied", gameObject);
            
            // Disable player controls
            var playerController = GetComponent<PlayerController>();
            if (playerController != null)
            {
                playerController.enabled = false;
            }
            
            // Disable weapon
            var weaponController = GetComponentInChildren<BasicWeapon>();
            if (weaponController != null)
            {
                weaponController.enabled = false;
            }
        }
        
        public void Respawn()
        {
            isDead = false;
            currentHealth = maxHealth;
            lastDamageTime = 0f;
            
            // Re-enable player controls
            var playerController = GetComponent<PlayerController>();
            if (playerController != null)
            {
                playerController.enabled = true;
            }
            
            // Re-enable weapon
            var weaponController = GetComponentInChildren<BasicWeapon>();
            if (weaponController != null)
            {
                weaponController.enabled = true;
            }
            
            EventSystem.Instance?.TriggerEvent("PlayerRespawned", gameObject);
        }
        
        void UpdateHealthUI()
        {
            // Send health info to UI
            var healthData = new { 
                current = currentHealth, 
                max = maxHealth, 
                percentage = HealthPercentage,
                isDead = isDead
            };
            EventSystem.Instance?.TriggerEvent("HealthUpdated", healthData);
        }
    }
}