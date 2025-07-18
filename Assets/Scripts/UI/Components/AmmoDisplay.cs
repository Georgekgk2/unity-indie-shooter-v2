using UnityEngine;
using UnityEngine.UI;

namespace IndieShooter.UI.Components
{
    public class AmmoDisplay : MonoBehaviour
    {
        [Header("Ammo Display Components")]
        public Text currentAmmoText;
        public Text maxAmmoText;
        public Text weaponNameText;
        public Image ammoIcon;
        public Slider reloadProgressBar;
        
        [Header("Warning Settings")]
        public GameObject lowAmmoWarning;
        public Color lowAmmoColor = Color.red;
        public Color normalAmmoColor = Color.white;
        [Range(0f, 1f)]
        public float lowAmmoThreshold = 0.2f;
        
        [Header("Animation")]
        public bool animateAmmoChange = true;
        public float ammoChangeAnimationDuration = 0.3f;
        public AnimationCurve ammoChangeAnimationCurve = AnimationCurve.EaseOutBounce(0, 0, 1, 1);
        
        [Header("Reload Animation")]
        public bool showReloadProgress = true;
        public Color reloadProgressColor = Color.yellow;
        
        private int currentAmmo = 30;
        private int maxAmmo = 30;
        private bool isReloading = false;
        private string weaponName = "Rifle";
        
        void Start()
        {
            if (reloadProgressBar != null)
            {
                reloadProgressBar.gameObject.SetActive(false);
                reloadProgressBar.fillRect.GetComponent<Image>().color = reloadProgressColor;
            }
                
            if (lowAmmoWarning != null)
                lowAmmoWarning.SetActive(false);
                
            UpdateAmmoDisplay();
        }
        
        public void SetAmmo(int current, int max, string weapon = null)
        {
            bool ammoChanged = current != currentAmmo;
            
            currentAmmo = current;
            maxAmmo = max;
            
            if (!string.IsNullOrEmpty(weapon))
                weaponName = weapon;
                
            UpdateAmmoDisplay();
            
            if (ammoChanged && animateAmmoChange)
                AnimateAmmoChange();
                
            CheckLowAmmoWarning();
        }
        
        public void SetReloading(bool reloading, float progress = 0f)
        {
            isReloading = reloading;
            
            if (showReloadProgress && reloadProgressBar != null)
            {
                reloadProgressBar.gameObject.SetActive(reloading);
                if (reloading)
                    reloadProgressBar.value = progress;
            }
            
            UpdateAmmoDisplay();
        }
        
        public void UpdateReloadProgress(float progress)
        {
            if (reloadProgressBar != null && isReloading)
                reloadProgressBar.value = progress;
        }
        
        void UpdateAmmoDisplay()
        {
            if (currentAmmoText != null)
            {
                if (isReloading)
                    currentAmmoText.text = "---";
                else
                    currentAmmoText.text = currentAmmo.ToString();
            }
            
            if (maxAmmoText != null)
                maxAmmoText.text = maxAmmo.ToString();
                
            if (weaponNameText != null)
                weaponNameText.text = weaponName;
                
            // Update ammo color based on amount
            Color ammoColor = GetAmmoColor();
            if (currentAmmoText != null)
                currentAmmoText.color = ammoColor;
        }
        
        Color GetAmmoColor()
        {
            if (isReloading)
                return reloadProgressColor;
                
            float ammoPercentage = (float)currentAmmo / maxAmmo;
            return ammoPercentage <= lowAmmoThreshold ? lowAmmoColor : normalAmmoColor;
        }
        
        void CheckLowAmmoWarning()
        {
            if (lowAmmoWarning != null)
            {
                float ammoPercentage = (float)currentAmmo / maxAmmo;
                bool showWarning = ammoPercentage <= lowAmmoThreshold && !isReloading && currentAmmo > 0;
                lowAmmoWarning.SetActive(showWarning);
                
                if (showWarning)
                    StartCoroutine(PulseLowAmmoWarning());
            }
        }
        
        System.Collections.IEnumerator PulseLowAmmoWarning()
        {
            if (lowAmmoWarning == null) yield break;
            
            CanvasGroup canvasGroup = lowAmmoWarning.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
                canvasGroup = lowAmmoWarning.AddComponent<CanvasGroup>();
                
            while (lowAmmoWarning.activeInHierarchy)
            {
                // Fade in
                float elapsed = 0f;
                while (elapsed < 0.5f)
                {
                    elapsed += Time.deltaTime;
                    canvasGroup.alpha = Mathf.Lerp(0.3f, 1f, elapsed / 0.5f);
                    yield return null;
                }
                
                // Fade out
                elapsed = 0f;
                while (elapsed < 0.5f)
                {
                    elapsed += Time.deltaTime;
                    canvasGroup.alpha = Mathf.Lerp(1f, 0.3f, elapsed / 0.5f);
                    yield return null;
                }
            }
        }
        
        void AnimateAmmoChange()
        {
            if (currentAmmoText != null)
                StartCoroutine(AmmoChangeAnimation());
        }
        
        System.Collections.IEnumerator AmmoChangeAnimation()
        {
            if (currentAmmoText == null) yield break;
            
            Vector3 originalScale = currentAmmoText.transform.localScale;
            float elapsed = 0f;
            
            while (elapsed < ammoChangeAnimationDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / ammoChangeAnimationDuration;
                float scaleMultiplier = ammoChangeAnimationCurve.Evaluate(t);
                
                currentAmmoText.transform.localScale = originalScale * (1f + scaleMultiplier * 0.2f);
                yield return null;
            }
            
            currentAmmoText.transform.localScale = originalScale;
        }
        
        public void PlayAmmoPickupEffect()
        {
            StartCoroutine(AmmoPickupEffect());
        }
        
        System.Collections.IEnumerator AmmoPickupEffect()
        {
            if (currentAmmoText == null) yield break;
            
            Color originalColor = currentAmmoText.color;
            Color pickupColor = Color.green;
            
            // Flash green
            float elapsed = 0f;
            while (elapsed < 0.3f)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.PingPong(elapsed * 6f, 1f);
                currentAmmoText.color = Color.Lerp(originalColor, pickupColor, t);
                yield return null;
            }
            
            currentAmmoText.color = originalColor;
        }
    }
}