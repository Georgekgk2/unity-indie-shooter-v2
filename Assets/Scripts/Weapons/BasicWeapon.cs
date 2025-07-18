using UnityEngine;
using IndieShooter.Core;

namespace IndieShooter.Weapons
{
    public class BasicWeapon : MonoBehaviour
    {
        [Header("Weapon Settings")]
        public string weaponName = "Basic Rifle";
        public float damage = 25f;
        public float fireRate = 0.1f;
        public float range = 100f;
        public int maxAmmo = 30;
        public float reloadTime = 2f;
        
        [Header("Effects")]
        public GameObject muzzleFlash;
        public GameObject bulletHole;
        public AudioClip fireSound;
        public AudioClip reloadSound;
        public AudioClip emptySound;
        
        [Header("References")]
        public Transform firePoint;
        public Camera playerCamera;
        
        private int currentAmmo;
        private float nextTimeToFire = 0f;
        private bool isReloading = false;
        private AudioSource audioSource;
        
        void Start()
        {
            currentAmmo = maxAmmo;
            audioSource = GetComponent<AudioSource>();
            
            if (playerCamera == null)
                playerCamera = Camera.main;
                
            // Subscribe to events
            EventSystem.Instance?.Subscribe("GamePaused", OnGamePaused);
        }
        
        void Update()
        {
            if (isReloading) return;
            
            HandleInput();
            UpdateUI();
        }
        
        void HandleInput()
        {
            // Fire weapon
            if (Input.GetButton("Fire1") && Time.time >= nextTimeToFire)
            {
                Fire();
            }
            
            // Reload weapon
            if (Input.GetKeyDown(KeyCode.R) && currentAmmo < maxAmmo)
            {
                StartCoroutine(Reload());
            }
        }
        
        void Fire()
        {
            if (currentAmmo <= 0)
            {
                // Play empty sound
                if (emptySound != null && audioSource != null)
                {
                    audioSource.PlayOneShot(emptySound);
                }
                return;
            }
            
            nextTimeToFire = Time.time + fireRate;
            currentAmmo--;
            
            // Play fire sound
            if (fireSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(fireSound);
            }
            
            // Show muzzle flash
            if (muzzleFlash != null)
            {
                StartCoroutine(ShowMuzzleFlash());
            }
            
            // Perform raycast
            PerformRaycast();
            
            // Trigger event
            EventSystem.Instance?.TriggerEvent("WeaponFired", weaponName);
        }
        
        void PerformRaycast()
        {
            RaycastHit hit;
            Vector3 rayOrigin = playerCamera.transform.position;
            Vector3 rayDirection = playerCamera.transform.forward;
            
            if (Physics.Raycast(rayOrigin, rayDirection, out hit, range))
            {
                // Check if we hit an enemy
                if (hit.collider.CompareTag("Enemy"))
                {
                    // Apply damage
                    var enemyHealth = hit.collider.GetComponent<EnemyHealth>();
                    if (enemyHealth != null)
                    {
                        enemyHealth.TakeDamage(damage);
                    }
                }
                
                // Create bullet hole
                if (bulletHole != null)
                {
                    GameObject hole = Instantiate(bulletHole, hit.point, Quaternion.LookRotation(hit.normal));
                    Destroy(hole, 10f); // Clean up after 10 seconds
                }
                
                // Trigger hit event
                EventSystem.Instance?.TriggerEvent("BulletHit", hit);
            }
            
            // Debug line in scene view
            Debug.DrawRay(rayOrigin, rayDirection * range, Color.red, 0.1f);
        }
        
        System.Collections.IEnumerator ShowMuzzleFlash()
        {
            muzzleFlash.SetActive(true);
            yield return new WaitForSeconds(0.05f);
            muzzleFlash.SetActive(false);
        }
        
        System.Collections.IEnumerator Reload()
        {
            isReloading = true;
            
            // Play reload sound
            if (reloadSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(reloadSound);
            }
            
            // Trigger reload event
            EventSystem.Instance?.TriggerEvent("WeaponReloading", weaponName);
            
            yield return new WaitForSeconds(reloadTime);
            
            currentAmmo = maxAmmo;
            isReloading = false;
            
            // Trigger reload complete event
            EventSystem.Instance?.TriggerEvent("WeaponReloaded", weaponName);
        }
        
        void UpdateUI()
        {
            // Send ammo info to UI
            var ammoData = new { current = currentAmmo, max = maxAmmo, isReloading = isReloading };
            EventSystem.Instance?.TriggerEvent("AmmoUpdated", ammoData);
        }
        
        void OnGamePaused(object isPaused)
        {
            enabled = !(bool)isPaused;
        }
        
        void OnDestroy()
        {
            EventSystem.Instance?.Unsubscribe("GamePaused", OnGamePaused);
        }
    }
    
    // Simple enemy health component for testing
    public class EnemyHealth : MonoBehaviour
    {
        public float maxHealth = 100f;
        private float currentHealth;
        
        void Start()
        {
            currentHealth = maxHealth;
        }
        
        public void TakeDamage(float damage)
        {
            currentHealth -= damage;
            
            if (currentHealth <= 0)
            {
                Die();
            }
        }
        
        void Die()
        {
            EventSystem.Instance?.TriggerEvent("EnemyDied", gameObject);
            Destroy(gameObject);
        }
    }
}