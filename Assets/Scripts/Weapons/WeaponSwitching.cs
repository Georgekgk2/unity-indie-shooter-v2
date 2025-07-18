using UnityEngine;

public class WeaponSwitching : MonoBehaviour
{
    [Header("Weapon Slots")]
    [Tooltip("Ìàñèâ ñëîò³â çáðî¿, ÿê³ ãðàâåöü ìîæå íîñèòè. Ö³ GameObject'è âæå çíàõîäÿòüñÿ â ñöåí³ (äî÷³ðí³ äî WeaponHolder)")]
    public GameObject[] weaponSlots;
    [Tooltip("Ìàñèâ ïðåôàá³â çáðî¿ 'â ðóêàõ'. Ïîðÿäîê ìàº â³äïîâ³äàòè ïîðÿäêó ñëîò³â (äëÿ ï³äáèðàííÿ)")]
    public GameObject[] weaponPrefabs; // Ïðåôàáè äëÿ çáðî¿ â ðóêàõ

    [Tooltip("Ïî÷àòêîâèé ñëîò, ÿêèé áóäå àêòèâîâàíèé ïðè ñòàðò³ ãðè (0 - ïåðøèé ñëîò).")]
    [Range(0, 9)]
    public int startingWeaponIndex = 0;

    [Header("Switching Settings")]
    [Tooltip("×è ìîæíà ïåðåìèêàòèñÿ íà íàñòóïíó/ïîïåðåäíþ çáðîþ êîëåñîì ìèø³?")]
    public bool useMouseScroll = true;
    [Tooltip("×è ìîæíà ïåðåìèêàòèñÿ íà çáðîþ çà íîìåðàìè êëàâ³ø (1, 2, 3...)?")]
    public bool useNumberKeys = true;
    [Tooltip("×àñ (ó ñåêóíäàõ), ÿêèé ïîòð³áíî ÷åêàòè ì³æ ïåðåìèêàííÿìè çáðî¿.")]
    public float switchCooldown = 0.5f;

    [Tooltip("²íäåêñ ñëîòó, ÿêèé â³äïîâ³äàº 'ðóêàì ïîðîæí³ì' àáî 'áåç çáðî¿'. Âñòàíîâ³òü -1, ÿêùî òàêî¿ îïö³¿ íåìàº.")]
    public int emptyHandsSlotIndex = -1;

    [Header("Drop Settings")]
    [Tooltip("Êíîïêà äëÿ âèêèäàííÿ ïîòî÷íî¿ çáðî¿.")]
    public KeyCode dropKey = KeyCode.G;
    [Tooltip("Òî÷êà, ç ÿêî¿ çáðîÿ áóäå âèêèäàòèñÿ (çàçâè÷àé á³ëÿ ãðàâöÿ).")]
    public Transform dropSpawnPoint;
    [Tooltip("Ñèëà, ç ÿêîþ çáðîÿ áóäå âèêèäàòèñÿ.")]
    public float dropForce = 5f;

    // Ïðèâàòí³ çì³íí³
    private int selectedWeapon;
    private float nextSwitchTime = 0f;

    void Awake()
    {
        if (weaponSlots == null || weaponSlots.Length == 0)
        {
            Debug.LogError("WeaponSwitching: Ìàñèâ weaponSlots ïîðîæí³é àáî íå ïðèçíà÷åíèé. Ñêðèïò âèìêíåíî.", this);
            enabled = false;
            return;
        }

        if (weaponPrefabs == null || weaponPrefabs.Length != weaponSlots.Length)
        {
            Debug.LogError("WeaponSwitching: Ìàñèâ weaponPrefabs íå ïðèçíà÷åíèé àáî éîãî ðîçì³ð íå â³äïîâ³äàº ðîçì³ðó weaponSlots. Ï³äáèðàííÿ çáðî¿ ìîæå ïðàöþâàòè íåêîðåêòíî.", this);
        }

        if (dropSpawnPoint == null)
        {
            Debug.LogWarning("WeaponSwitching: Drop Spawn Point íå ïðèçíà÷åíî. Âèêèäàííÿ çáðî¿ ìîæå ïðàöþâàòè íåêîðåêòíî.", this);
            dropSpawnPoint = transform.parent; // Ïðèïóñêàºìî, ùî öå îá'ºêò ãðàâöÿ
        }
        
        // ²í³ö³àë³çóºìî ïåðøó çáðîþ
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

        // --- Âèêèíóòè çáðîþ ---
        if (Input.GetKeyDown(dropKey))
        {
            DropCurrentWeapon();
            return; // Âèõîäèìî ç Update, ùîá íå îáðîáëÿòè ³íø³ ³íïóòè
        }

        if (Time.time < nextSwitchTime)
        {
            return;
        }

        // --- Ïåðåìèêàííÿ êîëåñîì ìèø³ ---
        if (useMouseScroll)
        {
            if (Input.GetAxis("Mouse ScrollWheel") > 0f) // Ïðîêðóòêà âãîðó
            {
                selectedWeapon = (selectedWeapon + 1) % weaponSlots.Length;
            }
            if (Input.GetAxis("Mouse ScrollWheel") < 0f) // Ïðîêðóòêà âíèç
            {
                selectedWeapon = (selectedWeapon - 1 + weaponSlots.Length) % weaponSlots.Length;
            }
        }

        // --- Ïåðåìèêàííÿ çà íîìåðàìè êëàâ³ø ---
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

        // --- Êëàâ³øà äëÿ "ïîêëàñòè çáðîþ" / "ðóêè ïîðîæí³" ---
        if (emptyHandsSlotIndex != -1 && Input.GetKeyDown(KeyCode.H)) // Ïðèêëàä: êëàâ³øà H äëÿ õîâàííÿ çáðî¿
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
    /// Àêòèâîâóº çáðîþ â îáðàíîìó ñëîò³ òà äåàêòèâóº ³íø³.
    /// </summary>
    void SelectWeapon(int index)
    {
        if (index < 0 || index >= weaponSlots.Length)
        {
            Debug.LogWarning($"WeaponSwitching: Ñïðîáà âèáðàòè íå³ñíóþ÷èé ñëîò çáðî¿: {index}. Âèá³ð íå çì³íåíî.", this);
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
            Debug.Log($"WeaponSwitched: Àêòèâîâàíî çáðîþ â ñëîò³ {selectedWeapon} ({weaponSlots[selectedWeapon].name}).");
        }
        else
        {
            selectedWeapon = index;
            Debug.Log($"WeaponSwitched: Ñëîò {index} ïîðîæí³é. Í³÷îãî íå àêòèâîâàíî.", this);
            if (emptyHandsSlotIndex != -1)
            {
                SelectWeapon(emptyHandsSlotIndex);
            }
        }
    }

    /// <summary>
    /// Âèêèäàº ïîòî÷íó çáðîþ.
    /// </summary>
    void DropCurrentWeapon()
    {
        GameObject currentWeaponGO = GetCurrentWeaponGameObject();
        if (currentWeaponGO == null || selectedWeapon == emptyHandsSlotIndex)
        {
            Debug.Log("Íåìîæëèâî âèêèíóòè: íåìàº çáðî¿ àáî öå ñëîò ïîðîæí³õ ðóê.");
            return;
        }

        WeaponController weaponController = currentWeaponGO.GetComponent<WeaponController>();
        if (weaponController == null || weaponController.weaponWorldPrefab == null)
        {
            Debug.LogError($"Íåìîæëèâî âèêèíóòè {currentWeaponGO.name}: íåìàº êîìïîíåíòà WeaponController àáî íå ïðèçíà÷åíî weaponWorldPrefab.", this);
            return;
        }

        // Ñïàâíèì ïðåôàá âèêèíóòî¿ çáðî¿
        GameObject droppedWeapon = Instantiate(weaponController.weaponWorldPrefab, dropSpawnPoint.position, dropSpawnPoint.rotation);
        
        // Äîäàºìî ³ìïóëüñ, ùîá âîíà âèêèäàëàñÿ
        Rigidbody droppedRb = droppedWeapon.GetComponent<Rigidbody>();
        if (droppedRb != null)
        {
            droppedRb.AddForce(dropSpawnPoint.forward * dropForce, ForceMode.Impulse);
            droppedRb.AddTorque(Random.insideUnitSphere * dropForce, ForceMode.Impulse);
        }

        // Çíèùóºìî GameObject ïîòî÷íî¿ çáðî¿ â ðóêàõ
        Destroy(currentWeaponGO);

        // "Ïðèáèðàºìî" çáðîþ ç³ ñëîòà, âñòàíîâëþþ÷è éîãî íà null
        weaponSlots[selectedWeapon] = null;
        Debug.Log($"Çáðîÿ ç³ ñëîòà {selectedWeapon} âèêèíóòà.");

        // Ïåðåìèêàºìîñü íà ³íøèé äîñòóïíèé ñëîò
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
            Debug.LogWarning("Ó ãðàâöÿ íå çàëèøèëîñÿ çáðî¿.", this);
        }
    }

    /// <summary>
    /// Ï³äáèðàº çáðîþ òà ðîçì³ùóº ¿¿ ó â³äïîâ³äíîìó ñëîò³.
    /// </summary>
    /// <returns>Ïîâåðòàº true, ÿêùî çáðîÿ áóëà ï³ä³áðàíà.</returns>
    public bool PickupWeapon(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= weaponSlots.Length)
        {
            Debug.LogError($"Íåìîæëèâî ï³ä³áðàòè çáðîþ: íåä³éñíèé ³íäåêñ ñëîòó {slotIndex}", this);
            return false;
        }
        if (weaponSlots[slotIndex] != null)
        {
            Debug.Log($"Íåìîæëèâî ï³ä³áðàòè: Ñëîò {slotIndex} âæå çàéíÿòèé.");
            return false;
        }
        
        if (weaponPrefabs == null || slotIndex >= weaponPrefabs.Length || weaponPrefabs[slotIndex] == null)
        {
            Debug.LogError($"Íåìîæëèâî ï³ä³áðàòè: Ïðåôàá äëÿ ñëîòó {slotIndex} íå ïðèçíà÷åíî ó weaponPrefabs.", this);
            return false;
        }

        // Ñòâîðþºìî åêçåìïëÿð çáðî¿ "â ðóêàõ" ç ïðåôàáó ³ ðîáèìî éîãî äî÷³ðí³ì äî öüîãî îá'ºêòà (WeaponHolder)
        GameObject newWeapon = Instantiate(weaponPrefabs[slotIndex], transform);
        
        // Äîäàºìî éîãî äî íàøîãî ìàñèâó ñëîò³â
        weaponSlots[slotIndex] = newWeapon;
        
        // Ïåðåìèêàºìîñü íà íîâó çáðîþ
        SelectWeapon(slotIndex);
        
        Debug.Log($"Çáðîÿ {newWeapon.name} ï³ä³áðàíà ó ñëîò {slotIndex}");
        return true;
    }


    /// <summary>
    /// Ìåòîä, ÿêèé ïîâåðòàº ïîòî÷íèé àêòèâíèé ³íäåêñ çáðî¿.
    /// </summary>
    public int GetCurrentWeaponIndex()
    {
        return selectedWeapon;
    }

    /// <summary>
    /// Ìåòîä, ÿêèé ïîâåðòàº GameObject ïîòî÷íî¿ àêòèâíî¿ çáðî¿.
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
    // ÌÅÒÎÄÈ ÄËß COMMAND PATTERN
    // ================================

    /// <summary>
    /// Îòðèìóº ³íäåêñ ïîòî÷íî¿ çáðî¿ (äëÿ Command Pattern)
    /// </summary>
    public int GetCurrentWeaponIndex()
    {
        return selectedWeapon;
    }

    /// <summary>
    /// Ïåðåìèêàº íà êîíêðåòíó çáðîþ çà ³íäåêñîì (äëÿ Command Pattern)
    /// </summary>
    public bool SwitchToWeapon(int weaponIndex)
    {
        if (Time.time < nextSwitchTime) return false;
        
        if (weaponIndex >= 0 && weaponIndex < weaponSlots.Length)
        {
            int previousWeapon = selectedWeapon;
            SelectWeapon(weaponIndex);
            nextSwitchTime = Time.time + switchCooldown;
            
            // Â³äïðàâëÿºìî ïîä³þ ïåðåìèêàííÿ çáðî¿
            string previousWeaponName = GetWeaponName(previousWeapon);
            string newWeaponName = GetWeaponName(weaponIndex);
            Events.Trigger(new WeaponSwitchedEvent(previousWeaponName, newWeaponName, weaponIndex));
            
            return true;
        }
        return false;
    }

    /// <summary>
    /// Ïåðåìèêàº íà íàñòóïíó çáðîþ (äëÿ Command Pattern)
    /// </summary>
    public bool SwitchToNextWeapon()
    {
        int nextWeapon = (selectedWeapon + 1) % weaponSlots.Length;
        return SwitchToWeapon(nextWeapon);
    }

    /// <summary>
    /// Ïåðåìèêàº íà ïîïåðåäíþ çáðîþ (äëÿ Command Pattern)
    /// </summary>
    public bool SwitchToPreviousWeapon()
    {
        int previousWeapon = (selectedWeapon - 1 + weaponSlots.Length) % weaponSlots.Length;
        return SwitchToWeapon(previousWeapon);
    }

    /// <summary>
    /// Ïåðåâ³ðÿº, ÷è ìîæíà ïåðåìèêàòè çáðîþ (äëÿ Command Pattern)
    /// </summary>
    public bool CanSwitchWeapon()
    {
        return Time.time >= nextSwitchTime;
    }

    /// <summary>
    /// Âèêèäàº ïîòî÷íó çáðîþ (äëÿ Command Pattern)
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
    /// Îòðèìóº íàçâó çáðî¿ çà ³íäåêñîì
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
    /// Îòðèìóº ê³ëüê³ñòü äîñòóïíèõ ñëîò³â çáðî¿
    /// </summary>
    public int GetWeaponSlotCount()
    {
        return weaponSlots != null ? weaponSlots.Length : 0;
    }

    /// <summary>
    /// Ïåðåâ³ðÿº, ÷è ñëîò çáðî¿ ïîðîæí³é
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