using UnityEngine;
using UnityEngine.UI;
using IndieShooter.Core;

namespace IndieShooter.UI.HUD
{
    public class GameHUD : MonoBehaviour
    {
        [Header("Health UI")]
        public Slider healthBar;
        public Text healthText;
        public Image healthBarFill;
        public Color healthColorHigh = Color.green;
        public Color healthColorMid = Color.yellow;
        public Color healthColorLow = Color.red;
        
        [Header("Ammo UI")]
        public Text ammoText;
        public Text weaponNameText;
        public Image reloadProgressBar;
        public GameObject lowAmmoWarning;
        
        [Header("Crosshair")]
        public Image crosshair;
        public Color crosshairNormal = Color.white;
        public Color crosshairEnemy = Color.red;
        public float crosshairSize = 20f;
        
        [Header("Minimap")]
        public RawImage minimapImage;
        public Transform minimapPlayer;
        public GameObject[] minimapEnemies;
        
        [Header("Objectives")]
        public Text objectiveText;
        public GameObject objectivePanel;
        
        [Header("Notifications")]
        public Text notificationText;
        public GameObject notificationPanel;
        public float notificationDuration = 3f;
        
        [Header("Score & Stats")]
        public Text scoreText;
        public Text killCountText;
        public Text timeText;
        
        [Header("Damage Indicator")]
        public Image damageOverlay;
        public float damageFlashDuration = 0.5f;
        
        private float currentHealth = 100f;
        private float maxHealth = 100f;
        private int currentAmmo = 30;
        private int maxAmmo = 30;
        private bool isReloading = false;
        private int score = 0;
        private int killCount = 0;
        private float gameTime = 0f;
        
        void Start()
        {
            // Subscribe to game events
            EventSystem.Instance?.Subscribe("HealthUpdated", OnHealthUpdated);
            EventSystem.Instance?.Subscribe("AmmoUpdated", OnAmmoUpdated);
            EventSystem.Instance?.Subscribe("WeaponSwitched", OnWeaponSwitched);
            EventSystem.Instance?.Subscribe("WeaponReloading", OnWeaponReloading);
            EventSystem.Instance?.Subscribe("WeaponReloaded", OnWeaponReloaded);
            EventSystem.Instance?.Subscribe("PlayerTakeDamage", OnPlayerTakeDamage);
            EventSystem.Instance?.Subscribe("EnemyDied", OnEnemyDied);
            EventSystem.Instance?.Subscribe("ObjectiveUpdated", OnObjectiveUpdated);
            EventSystem.Instance?.Subscribe("ScoreUpdated", OnScoreUpdated);
            
            // Initialize UI
            InitializeHUD();
        }
        
        void Update()
        {
            UpdateGameTime();
            UpdateCrosshair();
            UpdateMinimap();
        }
        
        void InitializeHUD()
        {
            UpdateHealthUI();
            UpdateAmmoUI();
            UpdateScoreUI();
            
            // Hide reload progress initially
            if (reloadProgressBar != null)
                reloadProgressBar.gameObject.SetActive(false);
                
            // Hide low ammo warning initially
            if (lowAmmoWarning != null)
                lowAmmoWarning.SetActive(false);
                
            // Hide damage overlay initially
            if (damageOverlay != null)
            {
                damageOverlay.color = new Color(damageOverlay.color.r, damageOverlay.color.g, damageOverlay.color.b, 0);
            }
        }
        
        void UpdateHealthUI()
        {
            if (healthBar != null)
            {
                healthBar.value = currentHealth / maxHealth;
            }
            
            if (healthText != null)
            {
                healthText.text = $"{currentHealth:F0}/{maxHealth:F0}";
            }
            
            if (healthBarFill != null)
            {
                float healthPercentage = currentHealth / maxHealth;
                
                if (healthPercentage > 0.6f)
                    healthBarFill.color = healthColorHigh;
                else if (healthPercentage > 0.3f)
                    healthBarFill.color = healthColorMid;
                else
                    healthBarFill.color = healthColorLow;
            }
        }
        
        void UpdateAmmoUI()
        {
            if (ammoText != null)
            {
                if (isReloading)
                {
                    ammoText.text = "RELOADING...";
                }
                else
                {
                    ammoText.text = $"{currentAmmo}/{maxAmmo}";
                }
            }
            
            // Show low ammo warning
            if (lowAmmoWarning != null)
            {
                bool showWarning = currentAmmo <= maxAmmo * 0.2f && !isReloading;
                lowAmmoWarning.SetActive(showWarning);
            }
        }
        
        void UpdateScoreUI()
        {
            if (scoreText != null)
                scoreText.text = $"Score: {score}";
                
            if (killCountText != null)
                killCountText.text = $"Kills: {killCount}";
        }
        
        void UpdateGameTime()
        {
            gameTime += Time.deltaTime;
            
            if (timeText != null)
            {
                int minutes = Mathf.FloorToInt(gameTime / 60);
                int seconds = Mathf.FloorToInt(gameTime % 60);
                timeText.text = $"{minutes:00}:{seconds:00}";
            }
        }
        
        void UpdateCrosshair()
        {
            if (crosshair == null) return;
            
            // Check if aiming at enemy
            bool aimingAtEnemy = CheckIfAimingAtEnemy();
            crosshair.color = aimingAtEnemy ? crosshairEnemy : crosshairNormal;
            
            // Dynamic crosshair size based on movement/accuracy
            float targetSize = crosshairSize;
            // You can add logic here to change size based on movement, weapon accuracy, etc.
            
            crosshair.rectTransform.sizeDelta = Vector2.one * targetSize;
        }
        
        bool CheckIfAimingAtEnemy()
        {
            // Simple raycast to check if crosshair is over enemy
            Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
            RaycastHit hit;
            
            if (Physics.Raycast(ray, out hit, 100f))
            {
                return hit.collider.CompareTag("Enemy");
            }
            
            return false;
        }
        
        void UpdateMinimap()
        {
            // Basic minimap update - you can expand this
            if (minimapPlayer != null && Camera.main != null)
            {
                // Update player position on minimap
                Vector3 playerPos = Camera.main.transform.position;
                minimapPlayer.position = new Vector3(playerPos.x, minimapPlayer.position.y, playerPos.z);
                minimapPlayer.rotation = Quaternion.Euler(0, Camera.main.transform.eulerAngles.y, 0);
            }
        }
        
        public void ShowNotification(string message)
        {
            if (notificationText != null && notificationPanel != null)
            {
                notificationText.text = message;
                notificationPanel.SetActive(true);
                
                // Hide after duration
                Invoke("HideNotification", notificationDuration);
            }
        }
        
        void HideNotification()
        {
            if (notificationPanel != null)
                notificationPanel.SetActive(false);
        }
        
        public void ShowDamageEffect()
        {
            if (damageOverlay != null)
            {
                StartCoroutine(DamageFlashEffect());
            }
        }
        
        System.Collections.IEnumerator DamageFlashEffect()
        {
            float elapsed = 0f;
            Color originalColor = damageOverlay.color;
            Color flashColor = new Color(originalColor.r, originalColor.g, originalColor.b, 0.3f);
            
            while (elapsed < damageFlashDuration)
            {
                elapsed += Time.deltaTime;
                float alpha = Mathf.Lerp(0.3f, 0f, elapsed / damageFlashDuration);
                damageOverlay.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
                yield return null;
            }
            
            damageOverlay.color = new Color(originalColor.r, originalColor.g, originalColor.b, 0);
        }
        
        // Event handlers
        void OnHealthUpdated(object data)
        {
            if (data != null)
            {
                var healthData = (dynamic)data;
                currentHealth = healthData.current;
                maxHealth = healthData.max;
                UpdateHealthUI();
            }
        }
        
        void OnAmmoUpdated(object data)
        {
            if (data != null)
            {
                var ammoData = (dynamic)data;
                currentAmmo = ammoData.current;
                maxAmmo = ammoData.max;
                isReloading = ammoData.isReloading;
                UpdateAmmoUI();
            }
        }
        
        void OnWeaponSwitched(object data)
        {
            if (weaponNameText != null && data != null)
            {
                weaponNameText.text = data.ToString();
            }
        }
        
        void OnWeaponReloading(object data)
        {
            isReloading = true;
            if (reloadProgressBar != null)
            {
                reloadProgressBar.gameObject.SetActive(true);
                StartCoroutine(UpdateReloadProgress());
            }
            UpdateAmmoUI();
        }
        
        void OnWeaponReloaded(object data)
        {
            isReloading = false;
            if (reloadProgressBar != null)
                reloadProgressBar.gameObject.SetActive(false);
            UpdateAmmoUI();
        }
        
        System.Collections.IEnumerator UpdateReloadProgress()
        {
            float reloadTime = 2f; // Get this from weapon
            float elapsed = 0f;
            
            while (elapsed < reloadTime && isReloading)
            {
                elapsed += Time.deltaTime;
                if (reloadProgressBar != null)
                    reloadProgressBar.fillAmount = elapsed / reloadTime;
                yield return null;
            }
        }
        
        void OnPlayerTakeDamage(object data)
        {
            ShowDamageEffect();
        }
        
        void OnEnemyDied(object data)
        {
            killCount++;
            score += 100; // Base score for kill
            UpdateScoreUI();
            ShowNotification("Enemy Eliminated!");
        }
        
        void OnObjectiveUpdated(object data)
        {
            if (objectiveText != null && data != null)
            {
                objectiveText.text = data.ToString();
            }
        }
        
        void OnScoreUpdated(object data)
        {
            if (data != null)
            {
                score = (int)data;
                UpdateScoreUI();
            }
        }
        
        void OnDestroy()
        {
            EventSystem.Instance?.Unsubscribe("HealthUpdated", OnHealthUpdated);
            EventSystem.Instance?.Unsubscribe("AmmoUpdated", OnAmmoUpdated);
            EventSystem.Instance?.Unsubscribe("WeaponSwitched", OnWeaponSwitched);
            EventSystem.Instance?.Unsubscribe("WeaponReloading", OnWeaponReloading);
            EventSystem.Instance?.Unsubscribe("WeaponReloaded", OnWeaponReloaded);
            EventSystem.Instance?.Unsubscribe("PlayerTakeDamage", OnPlayerTakeDamage);
            EventSystem.Instance?.Unsubscribe("EnemyDied", OnEnemyDied);
            EventSystem.Instance?.Unsubscribe("ObjectiveUpdated", OnObjectiveUpdated);
            EventSystem.Instance?.Unsubscribe("ScoreUpdated", OnScoreUpdated);
        }
    }
}