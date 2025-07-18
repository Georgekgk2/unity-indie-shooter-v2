using UnityEngine;

public class AmmoPickup : MonoBehaviour
{
    [Tooltip("Назва зброї або ID, для якої призначений цей набій. Якщо порожньо, підходить для будь-якої зброї.")]
    public string forWeaponName = ""; // Якщо ви хочете, щоб набої були для конкретної зброї
    
    // Можна додати візуальні ефекти або звуки при підбиранні
    [Tooltip("Ефект, що відтворюється при підбиранні.")]
    public GameObject pickupEffectPrefab;
    [Tooltip("Звук, що відтворюється при підбиранні.")]
    public AudioClip pickupSound;
    
    private void OnTriggerEnter(Collider other)
    {
        // Перевіряємо, чи це гравець
        if (other.CompareTag("Player"))
        {
            // Намагаємося знайти компонент WeaponSwitching
            WeaponSwitching weaponSwitching = other.GetComponentInChildren<WeaponSwitching>();
            if (weaponSwitching != null)
            {
                // Отримуємо поточну активну зброю
                GameObject currentWeaponGO = weaponSwitching.GetCurrentWeaponGameObject();
                if (currentWeaponGO != null)
                {
                    WeaponController weaponController = currentWeaponGO.GetComponent<WeaponController>();
                    if (weaponController != null)
                    {
                        // Якщо forWeaponName не вказано, або відповідає назві поточної зброї
                        if (string.IsNullOrEmpty(forWeaponName) || currentWeaponGO.name == forWeaponName)
                        {
                            // Додаємо одну перезарядку
                            weaponController.AddReloadCharge();
                            
                            // Відтворюємо ефекти
                            if (pickupEffectPrefab != null)
                            {
                                Instantiate(pickupEffectPrefab, transform.position, Quaternion.identity);
                            }
                            if (pickupSound != null)
                            {
                                // Відтворюємо звук не на цьому об'єкті, а в світі, бо цей об'єкт знищується
                                AudioSource.PlayClipAtPoint(pickupSound, transform.position);
                            }
                            
                            // Знищуємо набій
                            Destroy(gameObject);
                        }
                    }
                }
            }
        }
    }
}