using UnityEngine;

public class Bullet : MonoBehaviour
{
    [Tooltip("Час (у секундах) до самознищення кулі")]
    public float lifeTime = 3f;
    [Tooltip("Ефект, який з'являється при ударі кулі (наприклад, іскри)")]
    public GameObject hitEffectPrefab; // Опціонально

    void Start()
    {
        // Запускаємо таймер на знищення кулі
        Destroy(gameObject, lifeTime);
    }

    // Цей метод викликається, коли куля стикається з іншим Collider
    void OnCollisionEnter(Collision collision)
    {
        // Debug.Log($"Bullet hit: {collision.gameObject.name}");

        // Якщо є ефект удару, спавним його
        if (hitEffectPrefab != null)
        {
            // Спавнимо ефект у місці зіткнення, з нормаллю поверхні як напрямком
            GameObject hitEffect = Instantiate(hitEffectPrefab, collision.contacts[0].point, Quaternion.LookRotation(collision.contacts[0].normal));
            Destroy(hitEffect, 1f); // Знищуємо ефект через 1 секунду
        }

        // Тут можна додати логіку нанесення шкоди об'єкту, з яким зіткнулись
        // Example:
        // if (collision.gameObject.CompareTag("Enemy"))
        // {
        //     EnemyHealth enemyHealth = collision.gameObject.GetComponent<EnemyHealth>();
        //     if (enemyHealth != null)
        //     {
        //         enemyHealth.TakeDamage(10);
        //     }
        // }

        // Знищуємо кулю після зіткнення (якщо ви хочете, щоб вона зникала після першого удару)
        Destroy(gameObject);
    }
}