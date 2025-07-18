using UnityEngine;

public class WeaponSwitching : MonoBehaviour
{
    [Header("Weapon Slots")]
    [Tooltip("Масив слотів зброї, які гравець може носити. Ці GameObject'и вже знаходяться в сцені (дочірні до WeaponHolder)")]
    public GameObject[] weaponSlots;
    [Tooltip("Масив префабів зброї 'в руках'. Порядок має відповідати порядку слотів (для підбирання)")]
    public GameObject[] weaponPrefabs; // Префаби для зброї в руках

    [Tooltip("Початковий слот, який буде активований при старті гри (0 - перший слот).")]
    [Range(0, 9)]
    public int startingWeaponIndex = 0;

    [Header("Switching Settings")]
    [Tooltip("Чи можна перемикатися на наступну/попередню зброю колесом миші?")]
    public bool useMouseScroll = true;
    [Tooltip("Чи можна перемикатися на зброю за номерами клавіш (1, 2, 3...)?")]
    public bool useNumberKeys = true;
    [Tooltip("Час (у секундах), який потрібно чекати між перемиканнями зброї.")]
    public float switchCooldown = 0.5f;

    [Tooltip("Індекс слоту, який відповідає 'рукам порожнім' або 'без зброї'. Встановіть -1, якщо такої опції немає.")]
    public int emptyHandsSlotIndex = -1;

    [Header("Drop Settings")]
    [Tooltip("Кнопка для викидання поточної зброї.")]
    public KeyCode dropKey = KeyCode.G;
    [Tooltip("Точка, з якої зброя буде викидатися (зазвичай біля гравця).")]
    public Transform dropSpawnPoint;
    [Tooltip("Сила, з якою зброя буде викидатися.")]
    public float dropForce = 5f;

    // Приватні змінні
    private int selectedWeapon;
    private float nextSwitchTime = 0f;

    void Awake()
    {
        if (weaponSlots == null || weaponSlots.Length == 0)
        {
            Debug.LogError("WeaponSwitching: Масив weaponSlots порожній або не призначений. Скрипт вимкнено.", this);
            enabled = false;
            return;
        }

        if (weaponPrefabs == null || weaponPrefabs.Length != weaponSlots.Length)
        {
            Debug.LogError("WeaponSwitching: Масив weaponPrefabs не призначений або його розмір не відповідає розміру weaponSlots. Підбирання зброї може працювати некоректно.", this);
        }

        if (dropSpawnPoint == null)
        {
            Debug.LogWarning("WeaponSwitching: Drop Spawn Point не призначено. Викидання зброї може працювати некоректно.", this);
            dropSpawnPoint = transform.parent; // Припускаємо, що це об'єкт гравця
        }
        
        // Ініціалізуємо першу зброю
        selectedWeapon = startingWeaponIndex;
        if (selectedWeapon < 0 || selectedWeapon >= weaponSlots.Length)
        {
            selectedWeapon = 0;
        }

        SelectWeapon(selectedWeapon);
    }

    void Update()
    {
        int previousSelectedWeapon = selectedWeapon;

        // --- Викинути зброю ---
        if (Input.GetKeyDown(dropKey))
        {
            DropCurrentWeapon();
            return; // Виходимо з Update, щоб не обробляти інші інпути
        }

        if (Time.time < nextSwitchTime)
        {
            return;
        }

        // --- Перемикання колесом миші ---
        if (useMouseScroll)
        {
            if (Input.GetAxis("Mouse ScrollWheel") > 0f) // Прокрутка вгору
            {
                selectedWeapon = (selectedWeapon + 1) % weaponSlots.Length;
            }
            if (Input.GetAxis("Mouse ScrollWheel") < 0f) // Прокрутка вниз
            {
                selectedWeapon = (selectedWeapon - 1 + weaponSlots.Length) % weaponSlots.Length;
            }
        }

        // --- Перемикання за номерами клавіш ---
        if (useNumberKeys)
        {
            for (int i = 0; i < weaponSlots.Length; i++)
            {
                if (Input.GetKeyDown(KeyCode.Alpha1 + i))
                {
                    selectedWeapon = i;
                    break;
                }
            }
        }

        // --- Клавіша для "покласти зброю" / "руки порожні" ---
        if (emptyHandsSlotIndex != -1 && Input.GetKeyDown(KeyCode.H)) // Приклад: клавіша H для ховання зброї
        {
             if (selectedWeapon == emptyHandsSlotIndex)
             {
                 if (weaponSlots.Length > 0 && emptyHandsSlotIndex != 0) selectedWeapon = 0;
                 else selectedWeapon = previousSelectedWeapon;
             }
             else
             {
                 selectedWeapon = emptyHandsSlotIndex;
             }
        }

        if (selectedWeapon != previousSelectedWeapon)
        {
            SelectWeapon(selectedWeapon);
            nextSwitchTime = Time.time + switchCooldown;
        }
    }

    /// <summary>
    /// Активовує зброю в обраному слоті та деактивує інші.
    /// </summary>
    void SelectWeapon(int index)
    {
        if (index < 0 || index >= weaponSlots.Length)
        {
            Debug.LogWarning($"WeaponSwitching: Спроба вибрати неіснуючий слот зброї: {index}. Вибір не змінено.", this);
            return;
        }

        for (int i = 0; i < weaponSlots.Length; i++)
        {
            if (weaponSlots[i] != null)
            {
                weaponSlots[i].SetActive(false);
            }
        }

        if (weaponSlots[index] != null)
        {
            weaponSlots[index].SetActive(true);
            selectedWeapon = index;
            Debug.Log($"WeaponSwitched: Активовано зброю в слоті {selectedWeapon} ({weaponSlots[selectedWeapon].name}).");
        }
        else
        {
            selectedWeapon = index;
            Debug.Log($"WeaponSwitched: Слот {index} порожній. Нічого не активовано.", this);
            if (emptyHandsSlotIndex != -1)
            {
                SelectWeapon(emptyHandsSlotIndex);
            }
        }
    }

    /// <summary>
    /// Викидає поточну зброю.
    /// </summary>
    void DropCurrentWeapon()
    {
        GameObject currentWeaponGO = GetCurrentWeaponGameObject();
        if (currentWeaponGO == null || selectedWeapon == emptyHandsSlotIndex)
        {
            Debug.Log("Неможливо викинути: немає зброї або це слот порожніх рук.");
            return;
        }

        WeaponController weaponController = currentWeaponGO.GetComponent<WeaponController>();
        if (weaponController == null || weaponController.weaponWorldPrefab == null)
        {
            Debug.LogError($"Неможливо викинути {currentWeaponGO.name}: немає компонента WeaponController або не призначено weaponWorldPrefab.", this);
            return;
        }

        // Спавним префаб викинутої зброї
        GameObject droppedWeapon = Instantiate(weaponController.weaponWorldPrefab, dropSpawnPoint.position, dropSpawnPoint.rotation);
        
        // Додаємо імпульс, щоб вона викидалася
        Rigidbody droppedRb = droppedWeapon.GetComponent<Rigidbody>();
        if (droppedRb != null)
        {
            droppedRb.AddForce(dropSpawnPoint.forward * dropForce, ForceMode.Impulse);
            droppedRb.AddTorque(Random.insideUnitSphere * dropForce, ForceMode.Impulse);
        }

        // Знищуємо GameObject поточної зброї в руках
        Destroy(currentWeaponGO);

        // "Прибираємо" зброю зі слота, встановлюючи його на null
        weaponSlots[selectedWeapon] = null;
        Debug.Log($"Зброя зі слота {selectedWeapon} викинута.");

        // Перемикаємось на інший доступний слот
        if (emptyHandsSlotIndex != -1)
        {
            SelectWeapon(emptyHandsSlotIndex);
        }
        else
        {
            for (int i = 0; i < weaponSlots.Length; i++)
            {
                if (weaponSlots[i] != null)
                {
                    SelectWeapon(i);
                    return;
                }
            }
            Debug.LogWarning("У гравця не залишилося зброї.", this);
        }
    }

    /// <summary>
    /// Підбирає зброю та розміщує її у відповідному слоті.
    /// </summary>
    /// <returns>Повертає true, якщо зброя була підібрана.</returns>
    public bool PickupWeapon(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= weaponSlots.Length)
        {
            Debug.LogError($"Неможливо підібрати зброю: недійсний індекс слоту {slotIndex}", this);
            return false;
        }
        if (weaponSlots[slotIndex] != null)
        {
            Debug.Log($"Неможливо підібрати: Слот {slotIndex} вже зайнятий.");
            return false;
        }
        
        if (weaponPrefabs == null || slotIndex >= weaponPrefabs.Length || weaponPrefabs[slotIndex] == null)
        {
            Debug.LogError($"Неможливо підібрати: Префаб для слоту {slotIndex} не призначено у weaponPrefabs.", this);
            return false;
        }

        // Створюємо екземпляр зброї "в руках" з префабу і робимо його дочірнім до цього об'єкта (WeaponHolder)
        GameObject newWeapon = Instantiate(weaponPrefabs[slotIndex], transform);
        
        // Додаємо його до нашого масиву слотів
        weaponSlots[slotIndex] = newWeapon;
        
        // Перемикаємось на нову зброю
        SelectWeapon(slotIndex);
        
        Debug.Log($"Зброя {newWeapon.name} підібрана у слот {slotIndex}");
        return true;
    }


    /// <summary>
    /// Метод, який повертає поточний активний індекс зброї.
    /// </summary>
    public int GetCurrentWeaponIndex()
    {
        return selectedWeapon;
    }

    /// <summary>
    /// Метод, який повертає GameObject поточної активної зброї.
    /// </summary>
    public GameObject GetCurrentWeaponGameObject()
    {
        if (selectedWeapon >= 0 && selectedWeapon < weaponSlots.Length)
        {
            return weaponSlots[selectedWeapon];
        }
        return null;
    }

    // ================================
    // МЕТОДИ ДЛЯ COMMAND PATTERN
    // ================================

    /// <summary>
    /// Отримує індекс поточної зброї (для Command Pattern)
    /// </summary>
    public int GetCurrentWeaponIndex()
    {
        return selectedWeapon;
    }

    /// <summary>
    /// Перемикає на конкретну зброю за індексом (для Command Pattern)
    /// </summary>
    public bool SwitchToWeapon(int weaponIndex)
    {
        if (Time.time < nextSwitchTime) return false;
        
        if (weaponIndex >= 0 && weaponIndex < weaponSlots.Length)
        {
            int previousWeapon = selectedWeapon;
            SelectWeapon(weaponIndex);
            nextSwitchTime = Time.time + switchCooldown;
            
            // Відправляємо подію перемикання зброї
            string previousWeaponName = GetWeaponName(previousWeapon);
            string newWeaponName = GetWeaponName(weaponIndex);
            Events.Trigger(new WeaponSwitchedEvent(previousWeaponName, newWeaponName, weaponIndex));
            
            return true;
        }
        return false;
    }

    /// <summary>
    /// Перемикає на наступну зброю (для Command Pattern)
    /// </summary>
    public bool SwitchToNextWeapon()
    {
        int nextWeapon = (selectedWeapon + 1) % weaponSlots.Length;
        return SwitchToWeapon(nextWeapon);
    }

    /// <summary>
    /// Перемикає на попередню зброю (для Command Pattern)
    /// </summary>
    public bool SwitchToPreviousWeapon()
    {
        int previousWeapon = (selectedWeapon - 1 + weaponSlots.Length) % weaponSlots.Length;
        return SwitchToWeapon(previousWeapon);
    }

    /// <summary>
    /// Перевіряє, чи можна перемикати зброю (для Command Pattern)
    /// </summary>
    public bool CanSwitchWeapon()
    {
        return Time.time >= nextSwitchTime;
    }

    /// <summary>
    /// Викидає поточну зброю (для Command Pattern)
    /// </summary>
    public bool DropWeapon()
    {
        GameObject currentWeaponGO = GetCurrentWeaponGameObject();
        if (currentWeaponGO == null || selectedWeapon == emptyHandsSlotIndex)
        {
            return false;
        }

        DropCurrentWeapon();
        return true;
    }

    /// <summary>
    /// Отримує назву зброї за індексом
    /// </summary>
    private string GetWeaponName(int weaponIndex)
    {
        if (weaponIndex >= 0 && weaponIndex < weaponSlots.Length && weaponSlots[weaponIndex] != null)
        {
            WeaponController weaponController = weaponSlots[weaponIndex].GetComponent<WeaponController>();
            if (weaponController != null)
            {
                return weaponController.weaponDisplayName;
            }
            return weaponSlots[weaponIndex].name;
        }
        return "Empty Hands";
    }

    /// <summary>
    /// Отримує кількість доступних слотів зброї
    /// </summary>
    public int GetWeaponSlotCount()
    {
        return weaponSlots != null ? weaponSlots.Length : 0;
    }

    /// <summary>
    /// Перевіряє, чи слот зброї порожній
    /// </summary>
    public bool IsWeaponSlotEmpty(int slotIndex)
    {
        if (slotIndex >= 0 && slotIndex < weaponSlots.Length)
        {
            return weaponSlots[slotIndex] == null;
        }
        return true;
    }
}