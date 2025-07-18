using UnityEngine;

public class DroppedWeapon : MonoBehaviour
{
    [Header("Weapon Settings")]
    [Tooltip("Індекс слоту, до якого належить ця зброя. 0 для першого слоту, 1 для другого і т.д.")]
    public int weaponSlotIndex = 0; // Це ключ до нової системи!

    [Tooltip("Чи можна підібрати цю зброю?")]
    public bool canBePickedUp = true;
    
    // Посилання на префаб "в руках" більше не потрібне, оскільки WeaponSwitching тепер використовує свій масив `weaponPrefabs`
    // public GameObject weaponPrefabForPlayer; 

    // Метод, який спрацьовує при зіткненні
    private void OnCollisionEnter(Collision collision)
    {
        if (!canBePickedUp) return;

        // Перевіряємо, чи зіткнулися з гравцем (за тегом "Player")
        if (collision.gameObject.CompareTag("Player"))
        {
            // Шукаємо WeaponSwitching компонент на гравцеві або його дочірніх об'єктах
            WeaponSwitching weaponSwitching = collision.gameObject.GetComponentInChildren<WeaponSwitching>();
            if (weaponSwitching != null)
            {
                // Викликаємо метод для підбору зброї, передаючи індекс слоту
                bool pickedUp = weaponSwitching.PickupWeapon(weaponSlotIndex);

                if (pickedUp)
                {
                    // Якщо зброю успішно підібрано, знищуємо цей об'єкт
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