using UnityEngine;

public class Bullet : MonoBehaviour
{
    [Tooltip("×àñ (ó ñåêóíäàõ) äî ñàìîçíèùåííÿ êóë³")]
    public float lifeTime = 3f;
    [Tooltip("Åôåêò, ÿêèé ç'ÿâëÿºòüñÿ ïðè óäàð³ êóë³ (íàïðèêëàä, ³ñêðè)")]
    public GameObject hitEffectPrefab; // Îïö³îíàëüíî

    void Start()
    {
        // Çàïóñêàºìî òàéìåð íà çíèùåííÿ êóë³
        Destroy(gameObject, lifeTime);
    }

    // Öåé ìåòîä âèêëèêàºòüñÿ, êîëè êóëÿ ñòèêàºòüñÿ ç ³íøèì Collider
    void OnCollisionEnter(Collision collision)
    {
        // Debug.Log($"Bullet hit: {collision.gameObject.name}");

        // ßêùî º åôåêò óäàðó, ñïàâíèì éîãî
        if (hitEffectPrefab != null)
        {
            // Ñïàâíèìî åôåêò ó ì³ñö³ ç³òêíåííÿ, ç íîðìàëëþ ïîâåðõí³ ÿê íàïðÿìêîì
            GameObject hitEffect = Instantiate(hitEffectPrefab, collision.contacts[0].point, Quaternion.LookRotation(collision.contacts[0].normal));
            Destroy(hitEffect, 1f); // Çíèùóºìî åôåêò ÷åðåç 1 ñåêóíäó
        }

        // Òóò ìîæíà äîäàòè ëîã³êó íàíåñåííÿ øêîäè îá'ºêòó, ç ÿêèì ç³òêíóëèñü
        // Example:
        // if (collision.gameObject.CompareTag("Enemy"))
        // {
        //     EnemyHealth enemyHealth = collision.gameObject.GetComponent<EnemyHealth>();
        //     if (enemyHealth != null)
        //     {
        //         enemyHealth.TakeDamage(10);
        //     }
        // }

        // Çíèùóºìî êóëþ ï³ñëÿ ç³òêíåííÿ (ÿêùî âè õî÷åòå, ùîá âîíà çíèêàëà ï³ñëÿ ïåðøîãî óäàðó)
        Destroy(gameObject);
    }
}