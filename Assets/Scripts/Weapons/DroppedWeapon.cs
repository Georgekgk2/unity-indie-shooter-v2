using UnityEngine;

public class DroppedWeapon : MonoBehaviour
{
    [Header("Weapon Settings")]
    [Tooltip("²íäåêñ ñëîòó, äî ÿêîãî íàëåæèòü öÿ çáðîÿ. 0 äëÿ ïåðøîãî ñëîòó, 1 äëÿ äðóãîãî ³ ò.ä.")]
    public int weaponSlotIndex = 0; // Öå êëþ÷ äî íîâî¿ ñèñòåìè!

    [Tooltip("×è ìîæíà ï³ä³áðàòè öþ çáðîþ?")]
    public bool canBePickedUp = true;
    
    // Ïîñèëàííÿ íà ïðåôàá "â ðóêàõ" á³ëüøå íå ïîòð³áíå, îñê³ëüêè WeaponSwitching òåïåð âèêîðèñòîâóº ñâ³é ìàñèâ `weaponPrefabs`
    // public GameObject weaponPrefabForPlayer; 

    // Ìåòîä, ÿêèé ñïðàöüîâóº ïðè ç³òêíåíí³
    private void OnCollisionEnter(Collision collision)
    {
        if (!canBePickedUp) return;

        // Ïåðåâ³ðÿºìî, ÷è ç³òêíóëèñÿ ç ãðàâöåì (çà òåãîì "Player")
        if (collision.gameObject.CompareTag("Player"))
        {
            // Øóêàºìî WeaponSwitching êîìïîíåíò íà ãðàâöåâ³ àáî éîãî äî÷³ðí³õ îá'ºêòàõ
            WeaponSwitching weaponSwitching = collision.gameObject.GetComponentInChildren<WeaponSwitching>();
            if (weaponSwitching != null)
            {
                // Âèêëèêàºìî ìåòîä äëÿ ï³äáîðó çáðî¿, ïåðåäàþ÷è ³íäåêñ ñëîòó
                bool pickedUp = weaponSwitching.PickupWeapon(weaponSlotIndex);

                if (pickedUp)
                {
                    // ßêùî çáðîþ óñï³øíî ï³ä³áðàíî, çíèùóºìî öåé îá'ºêò
                    Destroy(gameObject);
                }
            }
            else
            {
                Debug.LogWarning($"Player object '{collision.gameObject.name}' does not have a WeaponSwitching component in its children.", this);
            }
        }
    }
}