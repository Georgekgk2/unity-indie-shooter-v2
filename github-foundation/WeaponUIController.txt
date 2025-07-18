using UnityEngine;
using UnityEngine.UI; // Для роботи з Image компонентами
using TMPro; // Для TextMeshPro, якщо використовуєте

public class WeaponUIController : MonoBehaviour
{
    [Header("UI Elements")]
    [Tooltip("Масив Image компонентів, що представляють іконки слотів зброї в UI.")]
    public Image[] weaponSlotIcons; // Призначте сюди Image компонентів з UI
    [Tooltip("Текстовий елемент для відображення назви поточної зброї.")]
    public TextMeshProUGUI weaponNameText;
    [Tooltip("Текстовий елемент для відображення кількості патронів.")]
    public TextMeshProUGUI ammoText;

    [Header("UI Visuals")]
    [Tooltip("Колір активної іконки слоту (повна видимість).")]
    public Color activeSlotColor = Color.white; // Повна видимість, стандартний білий
    [Tooltip("Колір неактивної іконки слоту (напівпрозорий).")]
    public Color inactiveSlotColor = new Color(1f, 1f, 1f, 0.3f); // Білий з 30% прозорістю
    [Tooltip("Колір тексту патронів під час перезарядки або коли патрони закінчились.")]
    public Color reloadingAmmoTextColor = Color.yellow;
    [Tooltip("Стандартний колір тексту патронів.")]
    public Color normalAmmoTextColor = Color.white;

    [Tooltip("Множник масштабу для активної іконки слоту.")]
    public float activeSlotScale = 1.2f;
    [Tooltip("Швидкість анімації зміни розміру/кольору іконки слоту.")]
    public float iconLerpSpeed = 10f;
    [Tooltip("Швидкість анімації зміни кольору тексту.")]
    public float textColorLerpSpeed = 5f;

    [Header("Ammo Display Settings")]
    [Tooltip("Текст, що відображається, якщо немає активної зброї або порожні руки.")]
    public string emptyWeaponText = "Руки порожні";
    [Tooltip("Формат відображення патронів. Використовуйте {current} та {max}.")]
    public string ammoFormat = "{current} / {max}";
    [Tooltip("Текст, що відображається під час перезарядки.")]
    public string reloadingText = "Перезарядка...";
    [Tooltip("Текст, що відображається, якщо патронів немає і зброя не перезаряджається.")]
    public string outOfAmmoText = "НЕМАЄ ПАТРОНІВ!";

    // Приватні посилання на скрипти
    private WeaponSwitching weaponSwitching;
    private WeaponController activeWeaponController;
    private int lastSelectedWeaponIndex = -1; // Зберігаємо попередній індекс для оптимізації

    void Awake()
    {
        weaponSwitching = GetComponent<WeaponSwitching>();
        if (weaponSwitching == null)
        {
            Debug.LogError("WeaponUIController: WeaponSwitching скрипт не знайдено на цьому GameObject. Скрипт вимкнено.", this);
            enabled = false;
            return;
        }

        // Перевірка, чи призначені всі UI елементи (можна зробити більш надійним)
        if (weaponSlotIcons == null || weaponSlotIcons.Length == 0) Debug.LogWarning("WeaponUIController: Масив weaponSlotIcons порожній або не призначений.", this);
        if (weaponNameText == null) Debug.LogWarning("WeaponUIController: weaponNameText не призначено.", this);
        if (ammoText == null) Debug.LogWarning("WeaponUIController: ammoText не призначено.", this);
    }

    void Start()
    {
        // Ініціалізуємо UI при старті, використовуючи початковий вибір зброї
        // Це гарантує, що UI відобразить правильний стан одразу
        lastSelectedWeaponIndex = weaponSwitching.GetCurrentWeaponIndex(); // Отримуємо початковий індекс
        UpdateWeaponUI(true); // Викликаємо Update з forceUpdate = true для повної ініціалізації
    }

    void Update()
    {
        // Отримуємо поточний активний об'єкт зброї
        GameObject currentWeaponGO = weaponSwitching.GetCurrentWeaponGameObject();
        if (currentWeaponGO != null)
        {
            activeWeaponController = currentWeaponGO.GetComponent<WeaponController>();
        }
        else
        {
            activeWeaponController = null;
        }

        // Перевіряємо, чи змінився вибраний слот зброї
        int currentSlotIndex = weaponSwitching.GetCurrentWeaponIndex();
        bool forceUpdate = false;
        if (currentSlotIndex != lastSelectedWeaponIndex)
        {
            forceUpdate = true; // Примусове оновлення при зміні слоту
            lastSelectedWeaponIndex = currentSlotIndex;
        }

        UpdateWeaponUI(forceUpdate); // Оновлюємо UI. Можемо оптимізувати, щоб оновлювати лише при зміні патронів або перезарядки.
    }

    /// <summary>
    /// Оновлює всі візуальні елементи UI, пов'язані зі зброєю.
    /// </summary>
    /// <param name="forceUpdate">Примусово оновити всі елементи, навіть якщо вони не змінились (корисно при старті або зміні зброї).</param>
    void UpdateWeaponUI(bool forceUpdate = false)
    {
        // === Оновлення іконок слотів ===
        if (weaponSlotIcons != null && weaponSlotIcons.Length > 0)
        {
            int currentSlotIndex = weaponSwitching.GetCurrentWeaponIndex();
            for (int i = 0; i < weaponSlotIcons.Length; i++)
            {
                if (weaponSlotIcons[i] != null)
                {
                    // Цільові значення для кольору та масштабу
                    Color targetColor = (i == currentSlotIndex) ? activeSlotColor : inactiveSlotColor;
                    float targetScale = (i == currentSlotIndex) ? activeSlotScale : 1f;

                    // Плавна анімація лише, якщо не примусове оновлення або якщо значення вже сильно відрізняються
                    if (forceUpdate || Vector3.Distance(weaponSlotIcons[i].rectTransform.localScale, Vector3.one * targetScale) > 0.01f || Mathf.Abs(weaponSlotIcons[i].color.a - targetColor.a) > 0.01f)
                    {
                        weaponSlotIcons[i].color = Color.Lerp(weaponSlotIcons[i].color, targetColor, Time.deltaTime * iconLerpSpeed);
                        weaponSlotIcons[i].rectTransform.localScale = Vector3.Lerp(weaponSlotIcons[i].rectTransform.localScale, Vector3.one * targetScale, Time.deltaTime * iconLerpSpeed);
                    }
                }
            }
        }

        // === Оновлення тексту назви зброї ===
        if (weaponNameText != null)
        {
            string newWeaponName = "";
            if (activeWeaponController != null)
            {
                newWeaponName = activeWeaponController.gameObject.name; // Відображаємо назву об'єкта зброї
            }
            else
            {
                // Якщо зброї немає (порожні руки)
                if (weaponSwitching.GetCurrentWeaponIndex() == weaponSwitching.emptyHandsSlotIndex)
                {
                    newWeaponName = emptyWeaponText;
                }
                // Якщо слот порожній, але не emptyHandsSlot (наприклад, null-елемент в масиві)
                // Тоді newWeaponName залишиться порожнім рядком
            }
            
            // Оновлюємо текст тільки якщо він змінився або якщо forceUpdate
            if (forceUpdate || weaponNameText.text != newWeaponName)
            {
                weaponNameText.text = newWeaponName;
            }
        }

        // === Оновлення тексту кількості патронів ===
        if (ammoText != null)
        {
            string newAmmoText = "";
            Color targetAmmoTextColor = normalAmmoTextColor;

            if (activeWeaponController != null)
            {
                if (activeWeaponController.IsReloading())
                {
                    newAmmoText = reloadingText;
                    targetAmmoTextColor = reloadingAmmoTextColor;
                }
                else
                {
                    int currentAmmo = activeWeaponController.GetCurrentAmmo();
                    int maxAmmo = activeWeaponController.GetMagazineSize();
                    
                    if (currentAmmo <= 0) // Якщо патронів 0
                    {
                        newAmmoText = outOfAmmoText;
                        targetAmmoTextColor = reloadingAmmoTextColor; // Можна використовувати червоний колір
                    }
                    else
                    {
                        newAmmoText = ammoFormat.Replace("{current}", currentAmmo.ToString()).Replace("{max}", maxAmmo.ToString());
                    }
                }
            }
            // else - newAmmoText залишається порожнім, якщо немає активної зброї

            // Оновлюємо текст лише, якщо він змінився
            if (forceUpdate || ammoText.text != newAmmoText)
            {
                ammoText.text = newAmmoText;
            }
            // Плавно змінюємо колір тексту патронів
            ammoText.color = Color.Lerp(ammoText.color, targetAmmoTextColor, Time.deltaTime * textColorLerpSpeed);
        }
    }
}