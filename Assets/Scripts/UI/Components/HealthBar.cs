using UnityEngine;
using UnityEngine.UI;

namespace IndieShooter.UI.Components
{
    public class HealthBar : MonoBehaviour
    {
        [Header("Health Bar Components")]
        public Slider healthSlider;
        public Image fillImage;
        public Text healthText;
        public Image backgroundImage;
        
        [Header("Color Settings")]
        public Color healthColorHigh = Color.green;
        public Color healthColorMid = Color.yellow;
        public Color healthColorLow = Color.red;
        public Color backgroundColor = Color.black;
        
        [Header("Animation")]
        public bool animateChanges = true;
        public float animationSpeed = 2f;
        public bool pulseOnLowHealth = true;
        public float pulseSpeed = 2f;
        
        [Header("Thresholds")]
        [Range(0f, 1f)]
        public float lowHealthThreshold = 0.3f;
        [Range(0f, 1f)]
        public float midHealthThreshold = 0.6f;
        
        private float currentHealth = 1f;
        private float targetHealth = 1f;
        private float maxHealth = 100f;
        private bool isLowHealth = false;
        
        void Start()
        {
            if (healthSlider == null)
                healthSlider = GetComponent<Slider>();
                
            if (fillImage == null && healthSlider != null)
                fillImage = healthSlider.fillRect.GetComponent<Image>();
                
            if (backgroundImage != null)
                backgroundImage.color = backgroundColor;
                
            UpdateHealthBar();
        }
        
        void Update()
        {
            if (animateChanges && Mathf.Abs(currentHealth - targetHealth) > 0.01f)
            {
                currentHealth = Mathf.Lerp(currentHealth, targetHealth, Time.deltaTime * animationSpeed);
                UpdateHealthBar();
            }
            
            if (pulseOnLowHealth && isLowHealth)
            {
                PulseEffect();
            }
        }
        
        public void SetHealth(float health, float maxHealth)
        {
            this.maxHealth = maxHealth;
            targetHealth = Mathf.Clamp01(health / maxHealth);
            
            if (!animateChanges)
            {
                currentHealth = targetHealth;
                UpdateHealthBar();
            }
            
            isLowHealth = targetHealth <= lowHealthThreshold;
        }
        
        public void SetHealthImmediate(float health, float maxHealth)
        {
            this.maxHealth = maxHealth;
            currentHealth = targetHealth = Mathf.Clamp01(health / maxHealth);
            UpdateHealthBar();
            isLowHealth = currentHealth <= lowHealthThreshold;
        }
        
        void UpdateHealthBar()
        {
            if (healthSlider != null)
                healthSlider.value = currentHealth;
                
            if (fillImage != null)
                fillImage.color = GetHealthColor(currentHealth);
                
            if (healthText != null)
                healthText.text = $"{currentHealth * maxHealth:F0}/{maxHealth:F0}";
        }
        
        Color GetHealthColor(float healthPercentage)
        {
            if (healthPercentage > midHealthThreshold)
                return Color.Lerp(healthColorMid, healthColorHigh, (healthPercentage - midHealthThreshold) / (1f - midHealthThreshold));
            else if (healthPercentage > lowHealthThreshold)
                return Color.Lerp(healthColorLow, healthColorMid, (healthPercentage - lowHealthThreshold) / (midHealthThreshold - lowHealthThreshold));
            else
                return healthColorLow;
        }
        
        void PulseEffect()
        {
            if (fillImage != null)
            {
                float pulse = Mathf.Sin(Time.time * pulseSpeed) * 0.3f + 0.7f;
                Color baseColor = GetHealthColor(currentHealth);
                fillImage.color = new Color(baseColor.r, baseColor.g, baseColor.b, pulse);
            }
        }
        
        public void Flash(Color flashColor, float duration = 0.2f)
        {
            StartCoroutine(FlashEffect(flashColor, duration));
        }
        
        System.Collections.IEnumerator FlashEffect(Color flashColor, float duration)
        {
            if (fillImage == null) yield break;
            
            Color originalColor = fillImage.color;
            float elapsed = 0f;
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                fillImage.color = Color.Lerp(flashColor, originalColor, t);
                yield return null;
            }
            
            fillImage.color = originalColor;
        }
    }
}