using UnityEngine;
using System.Collections;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    [Tooltip("Максимальне здоров'я гравця")]
    public float maxHealth = 100f;
    [Tooltip("Початкове здоров'я гравця при старті або відродженні")]
    public float startingHealth = 100f;
    [Tooltip("Чи потрібно автоматично відновлювати здоров'я з часом?")]
    public bool enableHealthRegen = false;
    [Tooltip("Швидкість відновлення здоров'я за секунду")]
    public float regenRate = 5f;
    [Tooltip("Затримка перед початком відновлення здоров'я після отримання урону")]
    public float regenDelayAfterDamage = 3f;

    [Header("Damage Settings")]
    [Tooltip("Значення урону, який ворог завдає при торканні")]
    public float enemyTouchDamage = 10f;
    [Tooltip("Час невразливості (у секундах) після отримання урону.")]
    public float invulnerabilityTime = 0.5f;

    [Header("Healing Settings")]
    [Tooltip("Кількість здоров'я, що відновлюється при підбиранні MedKit")]
    public float medKitHealAmount = 50f;

    [Header("Death & Respawn Settings")]
    [Tooltip("Час (у секундах) перед відродженням гравця")]
    public float respawnDelay = 3f;
    [Tooltip("Точка відродження, якщо немає збережених даних.")]
    public Transform initialRespawnPoint; // Змінено на "початкову" точку

    // Приватні змінні стану
    [SerializeField] private float currentHealth;
    private bool isDead = false;
    private float lastDamageTime;
    private bool isInvulnerable = false;

    // Посилання на інші скрипти для керування
    private PlayerMovement playerMovement;
    private MouseLook mouseLook;
    private WeaponSwitching weaponSwitching;
    private CameraEffects cameraEffects; // НОВЕ ПОСИЛАННЯ

    void Awake()
    {
        // Отримуємо посилання на інші скрипти гравця
        playerMovement = GetComponent<PlayerMovement>();
        mouseLook = GetComponent<MouseLook>();
        weaponSwitching = GetComponentInChildren<WeaponSwitching>(); // Більш надійний спосіб пошуку
        cameraEffects = GetComponentInChildren<CameraEffects>();   // НОВЕ ПОСИЛАННЯ

        // Перевіряємо, чи всі посилання знайдені
        if (playerMovement == null || mouseLook == null || weaponSwitching == null)
            Debug.LogError("PlayerHealth: Не вдалося знайти один або декілька компонентів гравця (PlayerMovement, MouseLook, WeaponSwitching).", this);
        if (cameraEffects == null)
            Debug.LogWarning("PlayerHealth: CameraEffects не знайдено в дочірніх об'єктах. Ефекти урону не працюватимуть.", this);

        // Завантажуємо гру. Це встановить позицію та здоров'я гравця.
        LoadPlayerState();
    }

    void Update()
    {
        if (isDead) return;

        // Автоматична регенерація здоров'я
        if (enableHealthRegen && currentHealth < maxHealth && Time.time >= lastDamageTime + regenDelayAfterDamage)
        {
            Heal(regenRate * Time.deltaTime);
        }
    }

    /// <summary>
    /// Нанесення урону гравцеві.
    /// </summary>
    public void TakeDamage(float damageAmount)
    {
        if (isDead || isInvulnerable) return;

        float previousHealth = currentHealth;
        currentHealth -= damageAmount;
        currentHealth = Mathf.Max(0, currentHealth);

        lastDamageTime = Time.time;
        Debug.Log($"Player took {damageAmount} damage. Current Health: {currentHealth}");

        // Відправляємо подію зміни здоров'я
        Events.Trigger(new PlayerHealthChangedEvent(currentHealth, maxHealth, previousHealth));

        // Відправляємо подію тряски камери
        Events.Trigger(new CameraShakeEvent(GameConstants.DEFAULT_SHAKE_DURATION, 0.1f, "Damage"));

        // --- ВИКЛИК ЕФЕКТІВ (залишаємо для зворотної сумісності) ---
        if (cameraEffects != null)
        {
            cameraEffects.Shake();
            cameraEffects.FlashDamageEffect();
        }

        StartCoroutine(BecomeInvulnerableCoroutine());

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    /// <summary>
    /// Відновлення здоров'я гравця.
    /// </summary>
    public void Heal(float healAmount)
    {
        if (isDead) return;

        float previousHealth = currentHealth;
        currentHealth += healAmount;
        currentHealth = Mathf.Min(maxHealth, currentHealth);

        Debug.Log($"Player healed {healAmount} health. Current Health: {currentHealth}");

        // Показуємо healing number (Claude покращення)
        if (DamageNumbersManager.Instance != null)
        {
            Vector3 healPosition = transform.position + Vector3.up * 2f;
            DamageNumbersManager.Instance.ShowHealingNumber(healPosition, healAmount);
        }

        // Відправляємо подію зміни здоров'я
        Events.Trigger(new PlayerHealthChangedEvent(currentHealth, maxHealth, previousHealth));

        // Відправляємо ефект лікування через події
        if (cameraEffects != null)
        {
            cameraEffects.PlayHealEffect();
        }
    }

    /// <summary>
    /// Обробляє стан смерті гравця.
    /// </summary>
    private void Die()
    {
        if (isDead) return;

        isDead = true;
        Debug.Log("Player Died!");

        // Відправляємо подію смерті
        Events.Trigger(new PlayerDeathEvent(transform.position, "Health depleted"));

        // Вимикаємо контроль гравця
        SetPlayerControl(false);

        StartCoroutine(RespawnCoroutine());
    }

    /// <summary>
    /// Корутина для затримки відродження.
    /// </summary>
    private IEnumerator RespawnCoroutine()
    {
        yield return new WaitForSeconds(respawnDelay);
        Respawn();
    }

    /// <summary>
    /// Відроджує гравця на останній збереженій точці.
    /// </summary>
    private void Respawn()
    {
        Debug.Log("Player Respawning...");
        
        // Завантажуємо останній збережений стан (позицію та здоров'я)
        LoadPlayerState();

        isDead = false;
        lastDamageTime = Time.time;

        // Відправляємо подію відродження
        Events.Trigger(new PlayerRespawnEvent(transform.position, currentHealth));

        // Увімкнення контролю гравця
        SetPlayerControl(true);

        Debug.Log("Player Respawned!");
    }

    /// <summary>
    /// Зберігає поточний стан гравця (позицію та здоров'я) в PlayerPrefs.
    /// Викликається з Checkpoint.
    /// </summary>
    public void SavePlayerState()
    {
        // Зберігаємо позицію гравця
        PlayerPrefs.SetFloat("PlayerPosX", transform.position.x);
        PlayerPrefs.SetFloat("PlayerPosY", transform.position.y);
        PlayerPrefs.SetFloat("PlayerPosZ", transform.position.z);
        
        // Зберігаємо обертання гравця (по осі Y)
        PlayerPrefs.SetFloat("PlayerRotY", transform.rotation.eulerAngles.y);

        // Зберігаємо здоров'я
        PlayerPrefs.SetFloat("PlayerHealth", currentHealth);
        
        PlayerPrefs.Save(); // Зберігаємо дані на диск
        Debug.Log($"Game Saved! Position: {transform.position}, Health: {currentHealth}");

        // Відправляємо подію збереження гри
        Events.Trigger(new GameSavedEvent("default", true));
    }

    // ================================
    // МЕТОДИ ДЛЯ COMMAND PATTERN
    // ================================

    [Header("Debug Settings")]
    [Tooltip("Режим безсмертя для налагодження")]
    [SerializeField] private bool godModeEnabled = false;

    /// <summary>
    /// Перевіряє, чи увімкнений режим безсмертя (для Command Pattern)
    /// </summary>
    public bool IsGodModeEnabled()
    {
        return godModeEnabled;
    }

    /// <summary>
    /// Вмикає/вимикає режим безсмертя (для Command Pattern)
    /// </summary>
    public void SetGodMode(bool enabled)
    {
        godModeEnabled = enabled;
        Debug.Log($"God Mode: {(enabled ? "Enabled" : "Disabled")}");
        
        // Відправляємо подію для UI
        Events.Trigger(new ShowMessageEvent(
            $"God Mode {(enabled ? "Enabled" : "Disabled")}", 
            ShowMessageEvent.MessageType.Info, 
            2f
        ));
    }

    /// <summary>
    /// Перемикає режим безсмертя (для Command Pattern)
    /// </summary>
    public void ToggleGodMode()
    {
        SetGodMode(!godModeEnabled);
    }

    /// <summary>
    /// Миттєво відновлює здоров'я до максимуму (для Command Pattern)
    /// </summary>
    public void RestoreFullHealth()
    {
        float previousHealth = currentHealth;
        currentHealth = maxHealth;
        
        Debug.Log("Health restored to full!");
        
        // Відправляємо подію зміни здоров'я
        Events.Trigger(new PlayerHealthChangedEvent(currentHealth, maxHealth, previousHealth));
        
        // Відправляємо повідомлення
        Events.Trigger(new ShowMessageEvent("Health Restored!", ShowMessageEvent.MessageType.Success, 2f));
    }

    /// <summary>
    /// Перевіряє, чи можна нанести урон (враховує God Mode)
    /// </summary>
    public bool CanTakeDamage()
    {
        return !isDead && !isInvulnerable && !godModeEnabled;
    }

    /// <summary>
    /// Модифікований метод TakeDamage з підтримкою God Mode
    /// </summary>
    public void TakeDamage(float damageAmount, string damageSource = "Unknown")
    {
        if (!CanTakeDamage()) 
        {
            if (godModeEnabled)
            {
                Debug.Log($"Damage blocked by God Mode: {damageAmount} from {damageSource}");
            }
            return;
        }

        float previousHealth = currentHealth;
        currentHealth -= damageAmount;
        currentHealth = Mathf.Max(0, currentHealth);

        lastDamageTime = Time.time;
        Debug.Log($"Player took {damageAmount} damage from {damageSource}. Current Health: {currentHealth}");

        // Відправляємо подію зміни здоров'я
        Events.Trigger(new PlayerHealthChangedEvent(currentHealth, maxHealth, previousHealth));

        // Відправляємо подію тряски камери
        Events.Trigger(new CameraShakeEvent(GameConstants.DEFAULT_SHAKE_DURATION, 0.1f, $"Damage from {damageSource}"));

        // --- ВИКЛИК ЕФЕКТІВ (залишаємо для зворотної сумісності) ---
        if (cameraEffects != null)
        {
            cameraEffects.Shake();
            cameraEffects.FlashDamageEffect();
        }

        StartCoroutine(BecomeInvulnerableCoroutine());

        if (currentHealth <= 0)
        {
            Die(damageSource);
        }
    }

    /// <summary>
    /// Модифікований метод Die з інформацією про причину смерті
    /// </summary>
    private void Die(string causeOfDeath = "Unknown")
    {
        if (isDead) return;

        isDead = true;
        Debug.Log($"Player Died! Cause: {causeOfDeath}");

        // Відправляємо подію смерті з причиною
        Events.Trigger(new PlayerDeathEvent(transform.position, causeOfDeath));

        // Вимикаємо контроль гравця
        SetPlayerControl(false);

        StartCoroutine(RespawnCoroutine());
    }

    /// <summary>
    /// Завантажує стан гравця з PlayerPrefs.
    /// </summary>
    private void LoadPlayerState()
    {
        // Перевіряємо, чи є збережені дані
        if (PlayerPrefs.HasKey("PlayerPosX"))
        {
            // Завантажуємо позицію
            float x = PlayerPrefs.GetFloat("PlayerPosX");
            float y = PlayerPrefs.GetFloat("PlayerPosY");
            float z = PlayerPrefs.GetFloat("PlayerPosZ");
            transform.position = new Vector3(x, y, z);

            // Завантажуємо обертання
            float rotY = PlayerPrefs.GetFloat("PlayerRotY");
            transform.rotation = Quaternion.Euler(0, rotY, 0);
            if (mouseLook != null) mouseLook.ResetLookToInitial(); // Скидаємо кути MouseLook
            
            // Завантажуємо здоров'я
            currentHealth = PlayerPrefs.GetFloat("PlayerHealth", startingHealth);
            
            Debug.Log($"Game Loaded! Position: {transform.position}, Health: {currentHealth}");
        }
        else
        {
            // Якщо збереження немає, використовуємо початкову точку
            if (initialRespawnPoint != null)
            {
                transform.position = initialRespawnPoint.position;
                transform.rotation = initialRespawnPoint.rotation;
            }
            currentHealth = startingHealth;
        }
    }


    /// <summary>
    /// Робить гравця тимчасово невразливим.
    /// </summary>
    private IEnumerator BecomeInvulnerableCoroutine()
    {
        isInvulnerable = true;
        yield return new WaitForSeconds(invulnerabilityTime);
        isInvulnerable = false;
    }

    /// <summary>
    /// Обробка зіткнень з іншими об'єктами.
    /// </summary>
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            TakeDamage(enemyTouchDamage);
        }
    }

    /// <summary>
    /// Обробка зіткнень з тригерами.
    /// </summary>
    private void OnTriggerEnter(Collider other)
    {
        // Лікування від MedKit
        if (other.CompareTag("MedKit"))
        {
            Heal(medKitHealAmount);
            Destroy(other.gameObject);
            Debug.Log($"MedKit used and destroyed: {other.gameObject.name}");
        }

        // Взаємодія з Checkpoint
        if (other.CompareTag("Checkpoint"))
        {
            Checkpoint checkpoint = other.GetComponent<Checkpoint>();
            if (checkpoint != null)
            {
                // Замість того, щоб Checkpoint зберігав гру, він просить PlayerHealth зробити це.
                // Це кращий дизайн, оскільки PlayerHealth відповідає за свої дані.
                if (checkpoint.actionType == Checkpoint.CheckpointAction.SaveGame)
                {
                    SavePlayerState();
                }
            }
        }
    }

    /// <summary>
    /// Вмикає або вимикає керування гравцем.
    /// </summary>
    private void SetPlayerControl(bool isEnabled)
    {
        if (playerMovement != null) playerMovement.enabled = isEnabled;
        if (mouseLook != null) mouseLook.enabled = isEnabled;
        if (weaponSwitching != null) weaponSwitching.enabled = isEnabled;
    }

    // --- Публічні методи для UI або інших скриптів ---
    public float GetCurrentHealth() { return currentHealth; }
    public float GetMaxHealth() { return maxHealth; }
    public float GetHealthPercentage() { return currentHealth / maxHealth; }
    public bool IsDead() { return isDead; }
}