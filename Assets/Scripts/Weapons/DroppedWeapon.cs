using UnityEngine;

public class DroppedWeapon : MonoBehaviour
{
    [Header("Weapon Settings")]
    [Tooltip("������ �����, �� ����� �������� �� �����. 0 ��� ������� �����, 1 ��� ������� � �.�.")]
    public int weaponSlotIndex = 0; // �� ���� �� ���� �������!

    [Tooltip("�� ����� ������� �� �����?")]
    public bool canBePickedUp = true;
    
    // ��������� �� ������ "� �����" ����� �� �������, ������� WeaponSwitching ����� ����������� ��� ����� `weaponPrefabs`
    // public GameObject weaponPrefabForPlayer; 

    // �����, ���� ��������� ��� �������
    private void OnCollisionEnter(Collision collision)
    {
        if (!canBePickedUp) return;

        // ����������, �� ��������� � ������� (�� ����� "Player")
        if (collision.gameObject.CompareTag("Player"))
        {
            // ������ WeaponSwitching ��������� �� ������� ��� ���� ������� ��'�����
            WeaponSwitching weaponSwitching = collision.gameObject.GetComponentInChildren<WeaponSwitching>();
            if (weaponSwitching != null)
            {
                // ��������� ����� ��� ������ ����, ��������� ������ �����
                bool pickedUp = weaponSwitching.PickupWeapon(weaponSlotIndex);

                if (pickedUp)
                {
                    // ���� ����� ������ �������, ������� ��� ��'���
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