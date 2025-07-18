using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

/// <summary>
/// UI компонент для відображення окремого перка в меню перків
/// </summary>
public class PerkUIComponent : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("Кнопка перка")]
    public Button perkButton;
    [Tooltip("Іконка перка")]
    public Image perkIcon;
    [Tooltip("Назва перка")]
    public TextMeshProUGUI perkName;
    [Tooltip("Опис перка")]
    public TextMeshProUGUI perkDescription;
    [Tooltip("Вартість перка")]
    public TextMeshProUGUI perkCost;
    [Tooltip("Поточний ранг")]
    public TextMeshProUGUI currentRank;
    [Tooltip("Вимоги рівня")]
    public TextMeshProUGUI levelRequirement;

    [Header("Visual States")]
    [Tooltip("Колір доступного перка")]
    public Color availableColor = Color.white;
    [Tooltip("Колір недоступного перка")]
    public Color unavailableColor = Color.gray;
    [Tooltip("Колір розблокованого перка")]
    public Color unlockedColor = Color.green;
    [Tooltip("Колір максимального рангу")]
    public Color maxRankColor = Color.gold;

    [Header("Rarity Colors")]
    public Color commonColor = Color.white;
    public Color uncommonColor = Color.green;
    public Color rareColor = Color.blue;
    public Color epicColor = Color.magenta;
    public Color legendaryColor = Color.yellow;

    [Header("Effects")]
    [Tooltip("Ефект частинок для рідкісних перків")]
    public ParticleSystem rarityEffect;
    [Tooltip("Анімація при розблокуванні")]
    public Animator unlockAnimator;

    // Приватні змінні
    private Perk associatedPerk;
    private PerkSystem perkSystem;
    private bool isInitialized = false;

    /// <summary>
    /// Ініціалізує UI компонент з перком
    /// </summary>
    public void Initialize(Perk perk, PerkSystem system)
    {
        associatedPerk = perk;
        perkSystem = system;
        isInitialized = true;

        SetupUI();
        UpdateVisualState();

        // Підписуємося на події
        if (perkButton != null)
        {
            perkButton.onClick.AddListener(OnPerkClicked);
        }

        // Підписуємося на події системи перків
        PerkSystem.OnPerkUnlocked += OnPerkUnlocked;
        PerkSystem.OnLevelUp += OnLevelUp;
    }

    void OnDestroy()
    {
        // Відписуємося від подій
        PerkSystem.OnPerkUnlocked -= OnPerkUnlocked;
        PerkSystem.OnLevelUp -= OnLevelUp;
    }

    /// <summary>
    /// Налаштовує базовий UI
    /// </summary>
    void SetupUI()
    {
        if (!isInitialized) return;

        // Встановлюємо базову інформацію
        if (perkName != null)
            perkName.text = associatedPerk.name;

        if (perkDescription != null)
            perkDescription.text = associatedPerk.description;

        if (perkIcon != null && associatedPerk.icon != null)
            perkIcon.sprite = associatedPerk.icon;

        if (perkCost != null)
            perkCost.text = $"Вартість: {associatedPerk.cost}";

        if (levelRequirement != null)
            levelRequirement.text = $"Рівень: {associatedPerk.requiredLevel}";

        // Встановлюємо колір рідкості
        SetRarityColor();

        // Налаштовуємо ефекти для рідкісних перків
        SetupRarityEffects();
    }

    /// <summary>
    /// Оновлює візуальний стан перка
    /// </summary>
    void UpdateVisualState()
    {
        if (!isInitialized) return;

        // Оновлюємо ранг
        if (currentRank != null)
        {
            if (associatedPerk.maxRank > 1)
            {
                currentRank.text = $"{associatedPerk.currentRank}/{associatedPerk.maxRank}";
            }
            else
            {
                currentRank.text = associatedPerk.IsUnlocked ? "Розблоковано" : "Заблоковано";
            }
        }

        // Визначаємо стан перка
        bool canUnlock = perkSystem.CanUnlockPerk(associatedPerk);
        bool isUnlocked = associatedPerk.IsUnlocked;
        bool isMaxRank = associatedPerk.IsMaxRank;

        // Встановлюємо колір кнопки
        Color targetColor;
        if (isMaxRank)
        {
            targetColor = maxRankColor;
        }
        else if (isUnlocked)
        {
            targetColor = unlockedColor;
        }
        else if (canUnlock)
        {
            targetColor = availableColor;
        }
        else
        {
            targetColor = unavailableColor;
        }

        // Застосовуємо колір
        if (perkButton != null)
        {
            var colors = perkButton.colors;
            colors.normalColor = targetColor;
            colors.highlightedColor = targetColor * 1.2f;
            colors.pressedColor = targetColor * 0.8f;
            perkButton.colors = colors;

            // Встановлюємо інтерактивність
            perkButton.interactable = canUnlock && !isMaxRank;
        }

        // Оновлюємо прозорість іконки
        if (perkIcon != null)
        {
            var iconColor = perkIcon.color;
            iconColor.a = (canUnlock || isUnlocked) ? 1f : 0.5f;
            perkIcon.color = iconColor;
        }
    }

    /// <summary>
    /// Встановлює колір рідкості
    /// </summary>
    void SetRarityColor()
    {
        Color rarityColor = GetRarityColor(associatedPerk.rarity);

        // Застосовуємо колір до рамки або фону
        if (perkIcon != null)
        {
            // Можна додати рамку навколо іконки
            var outline = perkIcon.GetComponent<Outline>();
            if (outline != null)
            {
                outline.effectColor = rarityColor;
            }
        }

        // Застосовуємо колір до назви
        if (perkName != null)
        {
            perkName.color = rarityColor;
        }
    }

    /// <summary>
    /// Отримує колір для рідкості
    /// </summary>
    Color GetRarityColor(PerkRarity rarity)
    {
        switch (rarity)
        {
            case PerkRarity.Common: return commonColor;
            case PerkRarity.Uncommon: return uncommonColor;
            case PerkRarity.Rare: return rareColor;
            case PerkRarity.Epic: return epicColor;
            case PerkRarity.Legendary: return legendaryColor;
            default: return commonColor;
        }
    }

    /// <summary>
    /// Налаштовує ефекти рідкості
    /// </summary>
    void SetupRarityEffects()
    {
        if (rarityEffect == null) return;

        // Увімкнути ефекти тільки для рідкісних перків
        bool shouldShowEffect = associatedPerk.rarity >= PerkRarity.Epic;
        
        if (shouldShowEffect)
        {
            rarityEffect.gameObject.SetActive(true);
            
            // Налаштовуємо колір частинок
            var main = rarityEffect.main;
            main.startColor = GetRarityColor(associatedPerk.rarity);
        }
        else
        {
            rarityEffect.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Обробник натискання на перк
    /// </summary>
    void OnPerkClicked()
    {
        if (!isInitialized || perkSystem == null) return;

        // Спробуємо розблокувати перк
        bool success = perkSystem.UnlockPerk(associatedPerk.id);

        if (success)
        {
            // Запускаємо анімацію розблокування
            PlayUnlockAnimation();
            
            // Відтворюємо звук
            PlayUnlockSound();
        }
        else
        {
            // Показуємо повідомлення про помилку
            ShowErrorMessage();
        }
    }

    /// <summary>
    /// Відтворює анімацію розблокування
    /// </summary>
    void PlayUnlockAnimation()
    {
        if (unlockAnimator != null)
        {
            unlockAnimator.SetTrigger("Unlock");
        }

        // Додаткові ефекти
        StartCoroutine(UnlockEffectCoroutine());
    }

    /// <summary>
    /// Корутина для ефектів розблокування
    /// </summary>
    IEnumerator UnlockEffectCoroutine()
    {
        // Масштабування
        Vector3 originalScale = transform.localScale;
        
        // Збільшення
        float duration = 0.2f;
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            float progress = elapsed / duration;
            float scale = Mathf.Lerp(1f, 1.2f, progress);
            transform.localScale = originalScale * scale;
            
            elapsed += Time.deltaTime;
            yield return null;
        }

        // Повернення до нормального розміру
        elapsed = 0f;
        while (elapsed < duration)
        {
            float progress = elapsed / duration;
            float scale = Mathf.Lerp(1.2f, 1f, progress);
            transform.localScale = originalScale * scale;
            
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.localScale = originalScale;
    }

    /// <summary>
    /// Відтворює звук розблокування
    /// </summary>
    void PlayUnlockSound()
    {
        // Інтеграція з AudioManager
        if (AudioManager.Instance != null)
        {
            string soundName = "perk_unlock";
            
            // Різні звуки для різних рідкостей
            switch (associatedPerk.rarity)
            {
                case PerkRarity.Epic:
                    soundName = "perk_unlock_epic";
                    break;
                case PerkRarity.Legendary:
                    soundName = "perk_unlock_legendary";
                    break;
            }

            Events.Publish(new PlaySoundEvent(soundName, transform.position));
        }
    }

    /// <summary>
    /// Показує повідомлення про помилку
    /// </summary>
    void ShowErrorMessage()
    {
        string message = "Не можна розблокувати цей перк";

        // Визначаємо причину
        if (perkSystem.currentLevel < associatedPerk.requiredLevel)
        {
            message = $"Потрібен {associatedPerk.requiredLevel} рівень";
        }
        else if (perkSystem.availablePerkPoints < associatedPerk.cost)
        {
            message = $"Потрібно {associatedPerk.cost} очок перків";
        }
        else if (associatedPerk.IsMaxRank)
        {
            message = "Перк вже максимального рангу";
        }

        // Показуємо через систему повідомлень
        if (ModernUISystem.Instance != null)
        {
            ModernUISystem.Instance.ShowNotification(message, ModernUISystem.NotificationType.Warning);
        }
    }

    /// <summary>
    /// Обробник події розблокування перка
    /// </summary>
    void OnPerkUnlocked(Perk unlockedPerk)
    {
        if (unlockedPerk.id == associatedPerk.id)
        {
            UpdateVisualState();
        }
    }

    /// <summary>
    /// Обробник події підвищення рівня
    /// </summary>
    void OnLevelUp(int newLevel)
    {
        UpdateVisualState();
    }

    /// <summary>
    /// Показує детальну інформацію про перк (tooltip)
    /// </summary>
    public void ShowTooltip()
    {
        if (!isInitialized) return;

        string tooltipText = BuildTooltipText();
        
        // Показуємо через систему UI
        if (ModernUISystem.Instance?.tooltipSystem != null)
        {
            ModernUISystem.Instance.tooltipSystem.ShowTooltip(tooltipText, transform.position);
        }
    }

    /// <summary>
    /// Приховує tooltip
    /// </summary>
    public void HideTooltip()
    {
        if (ModernUISystem.Instance?.tooltipSystem != null)
        {
            ModernUISystem.Instance.tooltipSystem.HideTooltip();
        }
    }

    /// <summary>
    /// Будує текст для tooltip
    /// </summary>
    string BuildTooltipText()
    {
        string text = $"<b>{associatedPerk.name}</b>\n";
        text += $"{associatedPerk.description}\n\n";
        
        // Додаємо інформацію про ефекти
        if (associatedPerk.effects.Count > 0)
        {
            text += "<b>Ефекти:</b>\n";
            foreach (var effect in associatedPerk.effects)
            {
                float totalValue = effect.value * Mathf.Max(1, associatedPerk.currentRank);
                text += $"• {GetEffectDescription(effect.type, totalValue)}\n";
            }
            text += "\n";
        }

        // Додаємо вимоги
        text += $"<b>Рівень:</b> {associatedPerk.requiredLevel}\n";
        text += $"<b>Вартість:</b> {associatedPerk.cost} очок\n";
        
        if (associatedPerk.prerequisites.Count > 0)
        {
            text += "<b>Вимагає:</b> ";
            for (int i = 0; i < associatedPerk.prerequisites.Count; i++)
            {
                if (i > 0) text += ", ";
                // Знаходимо назву перка-передумови
                var prereqPerk = perkSystem.availablePerks.Find(p => p.id == associatedPerk.prerequisites[i]);
                text += prereqPerk != null ? prereqPerk.name : associatedPerk.prerequisites[i];
            }
            text += "\n";
        }

        // Додаємо інформацію про рідкість
        text += $"<b>Рідкість:</b> <color=#{ColorUtility.ToHtmlStringRGB(GetRarityColor(associatedPerk.rarity))}>{GetRarityName(associatedPerk.rarity)}</color>";

        return text;
    }

    /// <summary>
    /// Отримує опис ефекту
    /// </summary>
    string GetEffectDescription(PerkEffectType effectType, float value)
    {
        switch (effectType)
        {
            case PerkEffectType.DamageMultiplier:
                return $"Урон +{value * 100:F0}%";
            case PerkEffectType.HealthBonus:
                return $"Здоров'я +{value:F0}";
            case PerkEffectType.MovementSpeedMultiplier:
                return $"Швидкість руху +{value * 100:F0}%";
            case PerkEffectType.ReloadSpeedMultiplier:
                return $"Швидкість перезарядки +{value * 100:F0}%";
            case PerkEffectType.CriticalChance:
                return $"Шанс критичного удару +{value * 100:F0}%";
            case PerkEffectType.StaminaBonus:
                return $"Стаміна +{value:F0}";
            case PerkEffectType.BerserkerMode:
                return $"Урон при низькому HP +{value * 100:F0}%";
            case PerkEffectType.HealthRegeneration:
                return $"Регенерація {value:F0} HP/3с";
            default:
                return $"{effectType}: {value}";
        }
    }

    /// <summary>
    /// Отримує назву рідкості
    /// </summary>
    string GetRarityName(PerkRarity rarity)
    {
        switch (rarity)
        {
            case PerkRarity.Common: return "Звичайний";
            case PerkRarity.Uncommon: return "Незвичайний";
            case PerkRarity.Rare: return "Рідкісний";
            case PerkRarity.Epic: return "Епічний";
            case PerkRarity.Legendary: return "Легендарний";
            default: return "Невідомий";
        }
    }
}