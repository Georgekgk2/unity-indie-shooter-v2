using UnityEngine;
using IndieShooter.Core;

namespace IndieShooter.AI.Combat
{
    public class AICombatController : MonoBehaviour
    {
        [Header("Weapon Settings")]
        public string weaponType = "AssaultRifle";
        public float weaponDamage = 25f;
        public float weaponRange = 50f;
        public float weaponAccuracy = 0.7f;
        public float fireRate = 3f;
        
        [Header("Combat Behavior")]
        public float burstFireCount = 3;
        public float burstDelay = 0.1f;
        public float reloadTime = 2f;
        public int magazineSize = 30;
        
        [Header("Effects")]
        public GameObject muzzleFlashPrefab;
        public Transform muzzlePoint;
        public AudioClip fireSound;
        public AudioClip reloadSound;
        
        [Header("AI Behavior")]
        public float suppressionRange = 15f;
        public float flankingRange = 8f;
        public bool useAdvancedTactics = true;
        
        private int currentAmmo;
        private bool isReloading = false;
        private bool isFiring = false;
        private float lastFireTime = 0f;
        private int burstCount = 0;
        private AIController aiController;
        
        public bool CanFire => !isReloading && currentAmmo > 0 && Time.time - lastFireTime >= 1f / fireRate;
        public bool NeedsReload => currentAmmo <= 0 || (currentAmmo < magazineSize * 0.3f && !isFiring);
        
        void Start()
        {
            aiController = GetComponent<AIController>();
            currentAmmo = magazineSize;
            
            // Subscribe to events
            EventSystem.Instance?.Subscribe("EnemyWeaponFired", OnWeaponFired);
        }
        
        void Update()
        {
            HandleCombatLogic();
        }
        
        void HandleCombatLogic()
        {
            if (aiController == null || aiController.IsDead) return;
            
            // Handle reloading
            if (isReloading) return;
            
            // Check if need to reload
            if (NeedsReload && !isFiring)
            {
                StartReload();
                return;
            }
            
            // Handle combat based on current state
            if (aiController.canSeeTarget && aiController.IsPlayerInAttackRange)
            {
                HandleAttackBehavior();
            }
        }
        
        void HandleAttackBehavior()
        {
            if (!CanFire) return;
            
            if (useAdvancedTactics)
            {
                HandleAdvancedCombat();
            }
            else
            {
                HandleBasicCombat();
            }
        }
        
        void HandleBasicCombat()
        {
            // Simple continuous fire
            if (CanFire)
            {
                Fire();
            }
        }
        
        void HandleAdvancedCombat()
        {
            // Burst fire pattern
            if (!isFiring && CanFire)
            {
                StartCoroutine(BurstFire());
            }
        }
        
        System.Collections.IEnumerator BurstFire()
        {
            isFiring = true;
            burstCount = 0;
            
            while (burstCount < burstFireCount && currentAmmo > 0)
            {
                Fire();
                burstCount++;
                
                if (burstCount < burstFireCount)
                {
                    yield return new WaitForSeconds(burstDelay);
                }
            }
            
            // Pause between bursts
            yield return new WaitForSeconds(0.5f);
            isFiring = false;
        }
        
        public void Fire()
        {
            if (!CanFire) return;
            
            currentAmmo--;
            lastFireTime = Time.time;
            
            // Calculate fire direction with accuracy
            Vector3 fireOrigin = muzzlePoint != null ? muzzlePoint.position : transform.position + Vector3.up * 1.5f;
            Vector3 targetPosition = aiController.target.position + Vector3.up * 1f; // Aim at torso
            
            // Apply accuracy
            if (Random.value > weaponAccuracy)
            {
                Vector3 inaccuracy = Random.insideUnitSphere * (1f - weaponAccuracy) * 3f;
                inaccuracy.y *= 0.5f; // Less vertical spread
                targetPosition += inaccuracy;
            }
            
            Vector3 fireDirection = (targetPosition - fireOrigin).normalized;
            
            // Perform raycast
            RaycastHit hit;
            if (Physics.Raycast(fireOrigin, fireDirection, out hit, weaponRange))
            {
                ProcessHit(hit);
            }
            
            // Visual and audio effects
            ShowMuzzleFlash();
            PlayFireSound();
            
            // Trigger events
            EventSystem.Instance?.TriggerEvent("EnemyWeaponFired", this);
        }
        
        void ProcessHit(RaycastHit hit)
        {
            // Check what was hit
            if (hit.collider.CompareTag("Player"))
            {
                // Damage player
                var playerHealth = hit.collider.GetComponent<PlayerHealth>();
                if (playerHealth != null)
                {
                    playerHealth.TakeDamage(weaponDamage);
                }
            }
            
            // Trigger hit effects
            EventSystem.Instance?.TriggerEvent("BulletHit", hit);
        }
        
        void ShowMuzzleFlash()
        {
            if (muzzleFlashPrefab != null && muzzlePoint != null)
            {
                GameObject flash = Instantiate(muzzleFlashPrefab, muzzlePoint.position, muzzlePoint.rotation);
                Destroy(flash, 0.1f);
            }
        }
        
        void PlayFireSound()
        {
            if (fireSound != null)
            {
                AudioSource.PlayClipAtPoint(fireSound, transform.position);
            }
        }
        
        void StartReload()
        {
            if (isReloading) return;
            
            StartCoroutine(ReloadCoroutine());
        }
        
        System.Collections.IEnumerator ReloadCoroutine()
        {
            isReloading = true;
            
            // Play reload sound
            if (reloadSound != null)
            {
                AudioSource.PlayClipAtPoint(reloadSound, transform.position);
            }
            
            // Trigger reload animation
            var animator = GetComponent<Animator>();
            if (animator != null)
            {
                animator.SetTrigger("Reload");
            }
            
            yield return new WaitForSeconds(reloadTime);
            
            currentAmmo = magazineSize;
            isReloading = false;
        }
        
        // Tactical behaviors
        public Vector3 GetFlankingPosition()
        {
            if (aiController.target == null) return transform.position;
            
            Vector3 playerPosition = aiController.target.position;
            Vector3 playerForward = aiController.target.forward;
            
            // Try to move to player's side
            Vector3 flankDirection = Vector3.Cross(playerForward, Vector3.up).normalized;
            if (Random.value > 0.5f) flankDirection = -flankDirection;
            
            return playerPosition + flankDirection * flankingRange;
        }
        
        public Vector3 GetCoverPosition()
        {
            // Simple cover seeking - move away from player
            if (aiController.target == null) return transform.position;
            
            Vector3 awayFromPlayer = (transform.position - aiController.target.position).normalized;
            return transform.position + awayFromPlayer * 5f;
        }
        
        public bool ShouldSeekCover()
        {
            // Seek cover when health is low or under heavy fire
            return aiController.currentHealth < aiController.maxHealth * 0.3f;
        }
        
        public bool ShouldSuppressFire()
        {
            // Suppress fire when player is at medium range
            if (aiController.target == null) return false;
            
            float distance = Vector3.Distance(transform.position, aiController.target.position);
            return distance > suppressionRange && distance < weaponRange;
        }
        
        void OnWeaponFired(object data)
        {
            // React to other enemies firing - could implement coordination here
        }
        
        void OnDestroy()
        {
            EventSystem.Instance?.Unsubscribe("EnemyWeaponFired", OnWeaponFired);
        }
    }
}