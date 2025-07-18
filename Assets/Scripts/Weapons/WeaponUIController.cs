using UnityEngine;
using UnityEngine.UI; // Äëÿ ðîáîòè ç Image êîìïîíåíòàìè
using TMPro; // Äëÿ TextMeshPro, ÿêùî âèêîðèñòîâóºòå

public class WeaponUIController : MonoBehaviour
{
    [Header("UI Elements")]
    [Tooltip("Ìàñèâ Image êîìïîíåíò³â, ùî ïðåäñòàâëÿþòü ³êîíêè ñëîò³â çáðî¿ â UI.")]
    public Image[] weaponSlotIcons; // Ïðèçíà÷òå ñþäè Image êîìïîíåíò³â ç UI
    [Tooltip("Òåêñòîâèé åëåìåíò äëÿ â³äîáðàæåííÿ íàçâè ïîòî÷íî¿ çáðî¿.")]
    public TextMeshProUGUI weaponNameText;
    [Tooltip("Òåêñòîâèé åëåìåíò äëÿ â³äîáðàæåííÿ ê³ëüêîñò³ ïàòðîí³â.")]
    public TextMeshProUGUI ammoText;

    [Header("UI Visuals")]
    [Tooltip("Êîë³ð àêòèâíî¿ ³êîíêè ñëîòó (ïîâíà âèäèì³ñòü).")]
    public Color activeSlotColor = Color.white; // Ïîâíà âèäèì³ñòü, ñòàíäàðòíèé á³ëèé
    [Tooltip("Êîë³ð íåàêòèâíî¿ ³êîíêè ñëîòó (íàï³âïðîçîðèé).")]
    public Color inactiveSlotColor = new Color(1f, 1f, 1f, 0.3f); // Á³ëèé ç 30% ïðîçîð³ñòþ
    [Tooltip("Êîë³ð òåêñòó ïàòðîí³â ï³ä ÷àñ ïåðåçàðÿäêè àáî êîëè ïàòðîíè çàê³í÷èëèñü.")]
    public Color reloadingAmmoTextColor = Color.yellow;
    [Tooltip("Ñòàíäàðòíèé êîë³ð òåêñòó ïàòðîí³â.")]
    public Color normalAmmoTextColor = Color.white;

    [Tooltip("Ìíîæíèê ìàñøòàáó äëÿ àêòèâíî¿ ³êîíêè ñëîòó.")]
    public float activeSlotScale = 1.2f;
    [Tooltip("Øâèäê³ñòü àí³ìàö³¿ çì³íè ðîçì³ðó/êîëüîðó ³êîíêè ñëîòó.")]
    public float iconLerpSpeed = 10f;
    [Tooltip("Øâèäê³ñòü àí³ìàö³¿ çì³íè êîëüîðó òåêñòó.")]
    public float textColorLerpSpeed = 5f;

    [Header("Ammo Display Settings")]
    [Tooltip("Òåêñò, ùî â³äîáðàæàºòüñÿ, ÿêùî íåìàº àêòèâíî¿ çáðî¿ àáî ïîðîæí³ ðóêè.")]
    public string emptyWeaponText = "Ðóêè ïîðîæí³";
    [Tooltip("Ôîðìàò â³äîáðàæåííÿ ïàòðîí³â. Âèêîðèñòîâóéòå {current} òà {max}.")]
    public string ammoFormat = "{current} / {max}";
    [Tooltip("Òåêñò, ùî â³äîáðàæàºòüñÿ ï³ä ÷àñ ïåðåçàðÿäêè.")]
    public string reloadingText = "Ïåðåçàðÿäêà...";
    [Tooltip("Òåêñò, ùî â³äîáðàæàºòüñÿ, ÿêùî ïàòðîí³â íåìàº ³ çáðîÿ íå ïåðåçàðÿäæàºòüñÿ.")]
    public string outOfAmmoText = "ÍÅÌÀª ÏÀÒÐÎÍ²Â!";

    // Ïðèâàòí³ ïîñèëàííÿ íà ñêðèïòè
    private WeaponSwitching weaponSwitching;
    private WeaponController activeWeaponController;
    private int lastSelectedWeaponIndex = -1; // Çáåð³ãàºìî ïîïåðåäí³é ³íäåêñ äëÿ îïòèì³çàö³¿

    void Awake()
    {
        weaponSwitching = GetComponent<WeaponSwitching>();
        if (weaponSwitching == null)
        {
            Debug.LogError("WeaponUIController: WeaponSwitching ñêðèïò íå çíàéäåíî íà öüîìó GameObject. Ñêðèïò âèìêíåíî.", this);
            enabled = false;
            return;
        }

        // Ïåðåâ³ðêà, ÷è ïðèçíà÷åí³ âñ³ UI åëåìåíòè (ìîæíà çðîáèòè á³ëüø íàä³éíèì)
        if (weaponSlotIcons == null || weaponSlotIcons.Length == 0) Debug.LogWarning("WeaponUIController: Ìàñèâ weaponSlotIcons ïîðîæí³é àáî íå ïðèçíà÷åíèé.", this);
        if (weaponNameText == null) Debug.LogWarning("WeaponUIController: weaponNameText íå ïðèçíà÷åíî.", this);
        if (ammoText == null) Debug.LogWarning("WeaponUIController: ammoText íå ïðèçíà÷åíî.", this);
    }

    void Start()
    {
        // ²í³ö³àë³çóºìî UI ïðè ñòàðò³, âèêîðèñòîâóþ÷è ïî÷àòêîâèé âèá³ð çáðî¿
        // Öå ãàðàíòóº, ùî UI â³äîáðàçèòü ïðàâèëüíèé ñòàí îäðàçó
        lastSelectedWeaponIndex = weaponSwitching.GetCurrentWeaponIndex(); // Îòðèìóºìî ïî÷àòêîâèé ³íäåêñ
        UpdateWeaponUI(true); // Âèêëèêàºìî Update ç forceUpdate = true äëÿ ïîâíî¿ ³í³ö³àë³çàö³¿
    }

    void Update()
    {
        // Îòðèìóºìî ïîòî÷íèé àêòèâíèé îá'ºêò çáðî¿
        GameObject currentWeaponGO = weaponSwitching.GetCurrentWeaponGameObject();
        if (currentWeaponGO != null)
        {
            activeWeaponController = currentWeaponGO.GetComponent<WeaponController>();
        }
        else
        {
            activeWeaponController = null;
        }

        // Ïåðåâ³ðÿºìî, ÷è çì³íèâñÿ âèáðàíèé ñëîò çáðî¿
        int currentSlotIndex = weaponSwitching.GetCurrentWeaponIndex();
        bool forceUpdate = false;
        if (currentSlotIndex != lastSelectedWeaponIndex)
        {
            forceUpdate = true; // Ïðèìóñîâå îíîâëåííÿ ïðè çì³í³ ñëîòó
            lastSelectedWeaponIndex = currentSlotIndex;
        }

        UpdateWeaponUI(forceUpdate); // Îíîâëþºìî UI. Ìîæåìî îïòèì³çóâàòè, ùîá îíîâëþâàòè ëèøå ïðè çì³í³ ïàòðîí³â àáî ïåðåçàðÿäêè.
    }

    /// <summary>
    /// Îíîâëþº âñ³ â³çóàëüí³ åëåìåíòè UI, ïîâ'ÿçàí³ ç³ çáðîºþ.
    /// </summary>
    /// <param name="forceUpdate">Ïðèìóñîâî îíîâèòè âñ³ åëåìåíòè, íàâ³òü ÿêùî âîíè íå çì³íèëèñü (êîðèñíî ïðè ñòàðò³ àáî çì³í³ çáðî¿).</param>
    void UpdateWeaponUI(bool forceUpdate = false)
    {
        // === Îíîâëåííÿ ³êîíîê ñëîò³â ===
        if (weaponSlotIcons != null && weaponSlotIcons.Length > 0)
        {
            int currentSlotIndex = weaponSwitching.GetCurrentWeaponIndex();
            for (int i = 0; i < weaponSlotIcons.Length; i++)
            {
                if (weaponSlotIcons[i] != null)
                {
                    // Ö³ëüîâ³ çíà÷åííÿ äëÿ êîëüîðó òà ìàñøòàáó
                    Color targetColor = (i == currentSlotIndex) ? activeSlotColor : inactiveSlotColor;
                    float targetScale = (i == currentSlotIndex) ? activeSlotScale : 1f;

                    // Ïëàâíà àí³ìàö³ÿ ëèøå, ÿêùî íå ïðèìóñîâå îíîâëåííÿ àáî ÿêùî çíà÷åííÿ âæå ñèëüíî â³äð³çíÿþòüñÿ
                    if (forceUpdate || Vector3.Distance(weaponSlotIcons[i].rectTransform.localScale, Vector3.one * targetScale) > 0.01f || Mathf.Abs(weaponSlotIcons[i].color.a - targetColor.a) > 0.01f)
                    {
                        weaponSlotIcons[i].color = Color.Lerp(weaponSlotIcons[i].color, targetColor, Time.deltaTime * iconLerpSpeed);
                        weaponSlotIcons[i].rectTransform.localScale = Vector3.Lerp(weaponSlotIcons[i].rectTransform.localScale, Vector3.one * targetScale, Time.deltaTime * iconLerpSpeed);
                    }
                }
            }
        }

        // === Îíîâëåííÿ òåêñòó íàçâè çáðî¿ ===
        if (weaponNameText != null)
        {
            string newWeaponName = "";
            if (activeWeaponController != null)
            {
                newWeaponName = activeWeaponController.gameObject.name; // Â³äîáðàæàºìî íàçâó îá'ºêòà çáðî¿
            }
            else
            {
                // ßêùî çáðî¿ íåìàº (ïîðîæí³ ðóêè)
                if (weaponSwitching.GetCurrentWeaponIndex() == weaponSwitching.emptyHandsSlotIndex)
                {
                    newWeaponName = emptyWeaponText;
                }
                // ßêùî ñëîò ïîðîæí³é, àëå íå emptyHandsSlot (íàïðèêëàä, null-åëåìåíò â ìàñèâ³)
                // Òîä³ newWeaponName çàëèøèòüñÿ ïîðîæí³ì ðÿäêîì
            }
            
            // Îíîâëþºìî òåêñò ò³ëüêè ÿêùî â³í çì³íèâñÿ àáî ÿêùî forceUpdate
            if (forceUpdate || weaponNameText.text != newWeaponName)
            {
                weaponNameText.text = newWeaponName;
            }
        }

        // === Îíîâëåííÿ òåêñòó ê³ëüêîñò³ ïàòðîí³â ===
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
                    
                    if (currentAmmo <= 0) // ßêùî ïàòðîí³â 0
                    {
                        newAmmoText = outOfAmmoText;
                        targetAmmoTextColor = reloadingAmmoTextColor; // Ìîæíà âèêîðèñòîâóâàòè ÷åðâîíèé êîë³ð
                    }
                    else
                    {
                        newAmmoText = ammoFormat.Replace("{current}", currentAmmo.ToString()).Replace("{max}", maxAmmo.ToString());
                    }
                }
            }
            // else - newAmmoText çàëèøàºòüñÿ ïîðîæí³ì, ÿêùî íåìàº àêòèâíî¿ çáðî¿

            // Îíîâëþºìî òåêñò ëèøå, ÿêùî â³í çì³íèâñÿ
            if (forceUpdate || ammoText.text != newAmmoText)
            {
                ammoText.text = newAmmoText;
            }
            // Ïëàâíî çì³íþºìî êîë³ð òåêñòó ïàòðîí³â
            ammoText.color = Color.Lerp(ammoText.color, targetAmmoTextColor, Time.deltaTime * textColorLerpSpeed);
        }
    }
}