using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Система пулу для куль. Покращує продуктивність, уникаючи постійного створення/знищення об'єктів.
/// </summary>
public class BulletPool : MonoBehaviour
{
    [Header("Pool Settings")]
    [Tooltip("Префаб кулі для пулу")]
    public GameObject bulletPrefab;
    [Tooltip("Початковий розмір пулу")]
    public int initialPoolSize = 50;
    [Tooltip("Максимальний розмір пулу (0 = без обмежень)")]
    public int maxPoolSize = 100;
    [Tooltip("Автоматично розширювати пул при необхідності?")]
    public bool autoExpand = true;

    // Приватні змінні
    private Queue<GameObject> availableBullets = new Queue<GameObject>();
    private List<GameObject> allBullets = new List<GameObject>();
    private Transform poolParent;

    // Singleton pattern для легкого доступу
    public static BulletPool Instance { get; private set; }

    void Awake()
    {
        // Singleton setup
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializePool();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Ініціалізує пул куль
    /// </summary>
    void InitializePool()
    {
        if (bulletPrefab == null)
        {
            Debug.LogError("BulletPool: bulletPrefab не призначено!", this);
            enabled = false;
            return;
        }

        // Створюємо батьківський об'єкт для організації
        poolParent = new GameObject("Bullet Pool").transform;
        poolParent.SetParent(transform);

        // Створюємо початкові кулі
        for (int i = 0; i < initialPoolSize; i++)
        {
            CreateNewBullet();
        }

        Debug.Log($"BulletPool ініціалізовано з {initialPoolSize} кулями.");
    }

    /// <summary>
    /// Створює нову кулю та додає її до пулу
    /// </summary>
    GameObject CreateNewBullet()
    {
        GameObject bullet = Instantiate(bulletPrefab, poolParent);
        bullet.SetActive(false);
        
        // Додаємо компонент для автоматичного повернення в пул
        PooledBullet pooledComponent = bullet.GetComponent<PooledBullet>();
        if (pooledComponent == null)
        {
            pooledComponent = bullet.AddComponent<PooledBullet>();
        }
        pooledComponent.Initialize(this);

        allBullets.Add(bullet);
        availableBullets.Enqueue(bullet);
        
        return bullet;
    }

    /// <summary>
    /// Отримує кулю з пулу
    /// </summary>
    /// <param name="position">Позиція спавну</param>
    /// <param name="rotation">Обертання спавну</param>
    /// <returns>GameObject кулі або null, якщо пул вичерпано</returns>
    public GameObject GetBullet(Vector3 position, Quaternion rotation)
    {
        GameObject bullet = null;

        // Спробуємо отримати кулю з пулу
        if (availableBullets.Count > 0)
        {
            bullet = availableBullets.Dequeue();
        }
        // Якщо пул порожній і дозволено розширення
        else if (autoExpand && (maxPoolSize == 0 || allBullets.Count < maxPoolSize))
        {
            bullet = CreateNewBullet();
            availableBullets.Dequeue(); // Видаляємо з черги, бо зараз використаємо
        }
        else
        {
            Debug.LogWarning("BulletPool: Пул вичерпано і розширення заборонено!");
            return null;
        }

        // Налаштовуємо кулю
        if (bullet != null)
        {
            bullet.transform.position = position;
            bullet.transform.rotation = rotation;
            bullet.SetActive(true);

            // Скидаємо швидкість, якщо є Rigidbody
            Rigidbody rb = bullet.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
        }

        return bullet;
    }

    /// <summary>
    /// Повертає кулю в пул
    /// </summary>
    /// <param name="bullet">Куля для повернення</param>
    public void ReturnBullet(GameObject bullet)
    {
        if (bullet == null) return;

        // Перевіряємо, чи ця куля належить нашому пулу
        if (!allBullets.Contains(bullet))
        {
            Debug.LogWarning("BulletPool: Спроба повернути кулю, яка не належить цьому пулу!");
            return;
        }

        // Деактивуємо кулю
        bullet.SetActive(false);
        bullet.transform.SetParent(poolParent);

        // Повертаємо в чергу доступних
        availableBullets.Enqueue(bullet);
    }

    /// <summary>
    /// Повертає всі активні кулі в пул (корисно при зміні рівня)
    /// </summary>
    public void ReturnAllBullets()
    {
        foreach (GameObject bullet in allBullets)
        {
            if (bullet != null && bullet.activeInHierarchy)
            {
                ReturnBullet(bullet);
            }
        }
    }

    /// <summary>
    /// Отримує статистику пулу
    /// </summary>
    public void GetPoolStats(out int total, out int available, out int active)
    {
        total = allBullets.Count;
        available = availableBullets.Count;
        active = total - available;
    }

    /// <summary>
    /// Виводить статистику пулу в консоль
    /// </summary>
    [ContextMenu("Print Pool Stats")]
    public void PrintPoolStats()
    {
        GetPoolStats(out int total, out int available, out int active);
        Debug.Log($"BulletPool Stats - Total: {total}, Available: {available}, Active: {active}");
    }

    void OnValidate()
    {
        // Валідація параметрів в редакторі
        initialPoolSize = Mathf.Max(1, initialPoolSize);
        if (maxPoolSize > 0)
        {
            maxPoolSize = Mathf.Max(initialPoolSize, maxPoolSize);
        }
    }
}

/// <summary>
/// Компонент для куль з пулу. Автоматично повертає кулю в пул після закінчення життя.
/// </summary>
public class PooledBullet : MonoBehaviour
{
    private BulletPool parentPool;
    private float lifeTime = 3f;
    private float spawnTime;

    public void Initialize(BulletPool pool)
    {
        parentPool = pool;
        
        // Отримуємо час життя з оригінального компонента Bullet, якщо він є
        Bullet bulletComponent = GetComponent<Bullet>();
        if (bulletComponent != null)
        {
            lifeTime = bulletComponent.lifeTime;
        }
    }

    void OnEnable()
    {
        spawnTime = Time.time;
    }

    void Update()
    {
        // Перевіряємо час життя
        if (Time.time - spawnTime >= lifeTime)
        {
            ReturnToPool();
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        // Викликаємо оригінальну логіку Bullet, якщо вона є
        Bullet bulletComponent = GetComponent<Bullet>();
        if (bulletComponent != null)
        {
            // Відключаємо оригінальний Bullet компонент, щоб уникнути подвійного Destroy
            bulletComponent.enabled = false;
            
            // Виконуємо логіку удару вручну
            if (bulletComponent.hitEffectPrefab != null)
            {
                GameObject hitEffect = Instantiate(bulletComponent.hitEffectPrefab, 
                    collision.contacts[0].point, 
                    Quaternion.LookRotation(collision.contacts[0].normal));
                Destroy(hitEffect, 1f);
            }
        }

        // Повертаємо в пул замість знищення
        ReturnToPool();
    }

    void ReturnToPool()
    {
        if (parentPool != null)
        {
            parentPool.ReturnBullet(gameObject);
        }
        else
        {
            // Якщо пул недоступний, знищуємо як зазвичай
            Destroy(gameObject);
        }
    }
}