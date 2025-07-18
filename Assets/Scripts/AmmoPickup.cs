using UnityEngine;

public class AmmoPickup : MonoBehaviour
{
    [Tooltip("Íàçâà çáðî¿ àáî ID, äëÿ ÿêî¿ ïðèçíà÷åíèé öåé íàá³é. ßêùî ïîðîæíüî, ï³äõîäèòü äëÿ áóäü-ÿêî¿ çáðî¿.")]
    public string forWeaponName = ""; // ßêùî âè õî÷åòå, ùîá íàáî¿ áóëè äëÿ êîíêðåòíî¿ çáðî¿
    
    // Ìîæíà äîäàòè â³çóàëüí³ åôåêòè àáî çâóêè ïðè ï³äáèðàíí³
    [Tooltip("Åôåêò, ùî â³äòâîðþºòüñÿ ïðè ï³äáèðàíí³.")]
    public GameObject pickupEffectPrefab;
    [Tooltip("Çâóê, ùî â³äòâîðþºòüñÿ ïðè ï³äáèðàíí³.")]
    public AudioClip pickupSound;
    
    private void OnTriggerEnter(Collider other)
    {
        // Ïåðåâ³ðÿºìî, ÷è öå ãðàâåöü
        if (other.CompareTag("Player"))
        {
            // Íàìàãàºìîñÿ çíàéòè êîìïîíåíò WeaponSwitching
            WeaponSwitching weaponSwitching = other.GetComponentInChildren<WeaponSwitching>();
            if (weaponSwitching != null)
            {
                // Îòðèìóºìî ïîòî÷íó àêòèâíó çáðîþ
                GameObject currentWeaponGO = weaponSwitching.GetCurrentWeaponGameObject();
                if (currentWeaponGO != null)
                {
                    WeaponController weaponController = currentWeaponGO.GetComponent<WeaponController>();
                    if (weaponController != null)
                    {
                        // ßêùî forWeaponName íå âêàçàíî, àáî â³äïîâ³äàº íàçâ³ ïîòî÷íî¿ çáðî¿
                        if (string.IsNullOrEmpty(forWeaponName) || currentWeaponGO.name == forWeaponName)
                        {
                            // Äîäàºìî îäíó ïåðåçàðÿäêó
                            weaponController.AddReloadCharge();
                            
                            // Â³äòâîðþºìî åôåêòè
                            if (pickupEffectPrefab != null)
                            {
                                Instantiate(pickupEffectPrefab, transform.position, Quaternion.identity);
                            }
                            if (pickupSound != null)
                            {
                                // Â³äòâîðþºìî çâóê íå íà öüîìó îá'ºêò³, à â ñâ³ò³, áî öåé îá'ºêò çíèùóºòüñÿ
                                AudioSource.PlayClipAtPoint(pickupSound, transform.position);
                            }
                            
                            // Çíèùóºìî íàá³é
                            Destroy(gameObject);
                        }
                    }
                }
            }
        }
    }
}