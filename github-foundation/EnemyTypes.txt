using UnityEngine;

/// <summary>
/// Різні типи ворогів з унікальними характеристиками та поведінкою.
/// Кожен тип має свої особливості бою та AI.
/// </summary>

// ================================
// СОЛДАТ (БАЗОВИЙ ВОРОГ)
// ================================

[CreateAssetMenu(fileName = "Soldier_Config", menuName = "Game/Enemies/Soldier")]
public class SoldierEnemy : Enemy
{
    [Header("Soldier Specific")]
    [Tooltip("Зброя солдата")]
    public WeaponConfiguration soldierWeapon;
    [Tooltip("Точність стрільби")]
    [Range(0f, 1f)]
    public float accuracy = 0.7f;

    protected override void OnDamageReceived(Vector3 hitPoint, Vector3 hitDirection)
    {
        base.OnDamageReceived(hitPoint, hitDirection);
        
        // Солдат може відступити при низькому здоров'ї
        if (currentHealth < maxHealth * 0.3f)
        {
            // Логіка відступу
            TryRetreat();
        }
    }

    void TryRetreat()
    {
        // Знаходимо найближчу точку патрулювання для відступу
        if (patrolPoints != null && patrolPoints.Length > 0)
        {
            Transform farthestPoint = null;
            float maxDistance = 0f;

            foreach (var point in patrolPoints)
            {
                if (point != null)
                {
                    float distance = Vector3.Distance(player.position, point.position);
                    if (distance > maxDistance)
                    {
                        maxDistance = distance;
                        farthestPoint = point;
                    }
                }
            }

            if (farthestPoint != null && navAgent != null)
            {
                navAgent.SetDestination(farthestPoint.position);
            }
        }
    }

    public override void AttackPlayer()
    {
        if (player == null || Time.time - lastAttackTime < 1f / attackRate) return;

        lastAttackTime = Time.time;

        // Стрільба з урахуванням точності
        Vector3 targetPosition = player.position + Vector3.up * 1.5f; // Прицілюємося в торс
        
        // Додаємо неточність
        Vector3 inaccuracy = Random.insideUnitSphere * (1f - accuracy) * 2f;
        targetPosition += inaccuracy;

        // Створюємо кулю (якщо є система куль)
        FireBullet(targetPosition);

        // Звук пострілу
        if (audioCollection.attackSound != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySound3D(audioCollection.attackSound, transform.position);
        }

        Debug.Log($"{enemyName} стріляє по гравцю!");
    }

    void FireBullet(Vector3 targetPosition)
    {
        // Тут можна інтегрувати з BulletPool
        Vector3 direction = (targetPosition - transform.position).normalized;
        
        // Raycast для перевірки попадання
        RaycastHit hit;
        if (Physics.Raycast(transform.position + Vector3.up, direction, out hit, attackRange))
        {
            if (hit.transform.CompareTag("Player"))
            {
                var playerHealth = hit.transform.GetComponent<PlayerHealth>();
                if (playerHealth != null)
                {
                    playerHealth.TakeDamage(attackDamage, "Enemy Soldier");
                }
            }
        }
    }
}

// ================================
// ВАЖКИЙ ВОРОГ
// ================================

[CreateAssetMenu(fileName = "Heavy_Config", menuName = "Game/Enemies/Heavy")]
public class HeavyEnemy : Enemy
{
    [Header("Heavy Specific")]
    [Tooltip("Час розкручування кулемета")]
    public float spinUpTime = 2f;
    [Tooltip("Чи розкручений кулемет?")]
    public bool isSpunUp = false;
    [Tooltip("Швидкість стрільби при розкрученому кулеметі")]
    public float spunUpFireRate = 10f;

    private float spinUpTimer = 0f;
    private bool isSpinningUp = false;

    void Reset()
    {
        // Налаштування для важкого ворога
        maxHealth = 300f;
        currentHealth = 300f;
        attackDamage = 40f;
        attackRate = 3f; // Повільно без розкручування
        attackRange = 20f;
        detectionRange = 25f;
        moveSpeed = 2f;
        runSpeed = 3f;
        armor = 0.3f;
        enemyType = EnemyType.Heavy;
    }

    protected override void OnDamageReceived(Vector3 hitPoint, Vector3 hitDirection)
    {
        base.OnDamageReceived(hitPoint, hitDirection);
        
        // Важкий ворог не відступає, а стає агресивнішим
        if (!isSpinningUp && !isSpunUp)
        {
            StartSpinUp();
        }
    }

    public override void AttackPlayer()
    {
        if (player == null) return;

        // Якщо не розкручений, починаємо розкручування
        if (!isSpunUp && !isSpinningUp)
        {
            StartSpinUp();
            return;
        }

        // Якщо розкручуємося
        if (isSpinningUp)
        {
            spinUpTimer += Time.deltaTime;
            if (spinUpTimer >= spinUpTime)
            {
                isSpunUp = true;
                isSpinningUp = false;
                attackRate = spunUpFireRate;
            }
            return;
        }

        // Стрільба розкрученим кулеметом
        if (isSpunUp && Time.time - lastAttackTime >= 1f / attackRate)
        {
            lastAttackTime = Time.time;
            FireMachineGun();
        }
    }

    void StartSpinUp()
    {
        isSpinningUp = true;
        spinUpTimer = 0f;
        
        // Звук розкручування
        if (audioCollection.alertSound != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySound3D(audioCollection.alertSound, transform.position);
        }
        
        Debug.Log($"{enemyName} розкручує кулемет!");
    }

    void FireMachineGun()
    {
        // Швидка стрільба з розкиду
        for (int i = 0; i < 3; i++)
        {
            Vector3 targetPosition = player.position + Vector3.up * 1.5f;
            Vector3 spread = Random.insideUnitSphere * 1.5f;
            targetPosition += spread;

            FireBullet(targetPosition);
        }

        // Звук кулемета
        if (audioCollection.attackSound != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySound3D(audioCollection.attackSound, transform.position, 1f, Random.Range(0.9f, 1.1f));
        }
    }

    void FireBullet(Vector3 targetPosition)
    {
        Vector3 direction = (targetPosition - transform.position).normalized;
        
        RaycastHit hit;
        if (Physics.Raycast(transform.position + Vector3.up, direction, out hit, attackRange))
        {
            if (hit.transform.CompareTag("Player"))
            {
                var playerHealth = hit.transform.GetComponent<PlayerHealth>();
                if (playerHealth != null)
                {
                    playerHealth.TakeDamage(attackDamage * 0.7f, "Heavy Enemy"); // Менший урон за кулю
                }
            }
        }
    }

    protected override void Die()
    {
        // Вибух при смерті
        if (VisualEffectsManager.Instance != null)
        {
            VisualEffectsManager.Instance.PlayExplosionEffect(transform.position, 2f);
        }

        // Урон навколо
        Collider[] nearbyObjects = Physics.OverlapSphere(transform.position, 5f);
        foreach (var obj in nearbyObjects)
        {
            if (obj.CompareTag("Player"))
            {
                var playerHealth = obj.GetComponent<PlayerHealth>();
                if (playerHealth != null)
                {
                    playerHealth.TakeDamage(50f, "Heavy Enemy Explosion");
                }
            }
        }

        base.Die();
    }
}

// ================================
// РОЗВІДНИК
// ================================

[CreateAssetMenu(fileName = "Scout_Config", menuName = "Game/Enemies/Scout")]
public class ScoutEnemy : Enemy
{
    [Header("Scout Specific")]
    [Tooltip("Швидкість ухилення")]
    public float dodgeSpeed = 8f;
    [Tooltip("Шанс ухилення")]
    [Range(0f, 1f)]
    public float dodgeChance = 0.3f;
    [Tooltip("Час між ухиленнями")]
    public float dodgeCooldown = 3f;

    private float lastDodgeTime = 0f;
    private bool isDodging = false;

    void Reset()
    {
        // Налаштування для розвідника
        maxHealth = 80f;
        currentHealth = 80f;
        attackDamage = 20f;
        attackRate = 2f;
        attackRange = 15f;
        detectionRange = 20f;
        moveSpeed = 5f;
        runSpeed = 8f;
        armor = 0f;
        enemyType = EnemyType.Scout;
    }

    protected override void OnDamageReceived(Vector3 hitPoint, Vector3 hitDirection)
    {
        // Спроба ухилитися
        if (Time.time - lastDodgeTime >= dodgeCooldown && Random.value < dodgeChance)
        {
            StartCoroutine(DodgeManeuver(hitDirection));
        }

        base.OnDamageReceived(hitPoint, hitDirection);
    }

    System.Collections.IEnumerator DodgeManeuver(Vector3 hitDirection)
    {
        isDodging = true;
        lastDodgeTime = Time.time;

        // Напрямок ухилення (перпендикулярно до удару)
        Vector3 dodgeDirection = Vector3.Cross(hitDirection, Vector3.up).normalized;
        if (Random.value < 0.5f) dodgeDirection = -dodgeDirection;

        // Швидке переміщення
        float dodgeTime = 0.5f;
        float elapsedTime = 0f;

        Vector3 startPosition = transform.position;
        Vector3 targetPosition = startPosition + dodgeDirection * 3f;

        while (elapsedTime < dodgeTime)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / dodgeTime;
            
            if (navAgent != null)
            {
                navAgent.enabled = false;
            }

            transform.position = Vector3.Lerp(startPosition, targetPosition, t);
            yield return null;
        }

        if (navAgent != null)
        {
            navAgent.enabled = true;
        }

        isDodging = false;
    }

    public override void AttackPlayer()
    {
        if (isDodging) return;

        base.AttackPlayer();

        // Після атаки розвідник може відступити
        if (Random.value < 0.4f)
        {
            StartCoroutine(TacticalRetreat());
        }
    }

    System.Collections.IEnumerator TacticalRetreat()
    {
        if (navAgent == null) yield break;

        // Відступаємо на короткий час
        Vector3 retreatDirection = (transform.position - player.position).normalized;
        Vector3 retreatPosition = transform.position + retreatDirection * 5f;

        navAgent.SetDestination(retreatPosition);
        yield return new WaitForSeconds(1f);

        // Повертаємося до атаки
        if (stateMachine != null && !isDead)
        {
            stateMachine.ChangeState<EnemyChaseState>();
        }
    }
}

// ================================
// СНАЙПЕР
// ================================

[CreateAssetMenu(fileName = "Sniper_Config", menuName = "Game/Enemies/Sniper")]
public class SniperEnemy : Enemy
{
    [Header("Sniper Specific")]
    [Tooltip("Час прицілювання")]
    public float aimTime = 2f;
    [Tooltip("Лазерний приціл")]
    public LineRenderer laserSight;
    [Tooltip("Позиції для снайпінгу")]
    public Transform[] snipingPositions;

    private bool isAiming = false;
    private float aimTimer = 0f;
    private Transform currentSnipingPosition;

    void Reset()
    {
        // Налаштування для снайпера
        maxHealth = 120f;
        currentHealth = 120f;
        attackDamage = 80f;
        attackRate = 0.5f; // Повільна стрільба
        attackRange = 50f;
        detectionRange = 40f;
        moveSpeed = 3f;
        runSpeed = 4f;
        armor = 0.1f;
        enemyType = EnemyType.Sniper;
    }

    void Start()
    {
        base.Start();
        
        // Знаходимо найкращу позицію для снайпінгу
        FindBestSnipingPosition();
    }

    void FindBestSnipingPosition()
    {
        if (snipingPositions == null || snipingPositions.Length == 0) return;

        float bestScore = 0f;
        Transform bestPosition = null;

        foreach (var position in snipingPositions)
        {
            if (position == null) continue;

            // Оцінюємо позицію за дистанцією до гравця та висотою
            float distanceScore = Vector3.Distance(position.position, transform.position);
            float heightScore = position.position.y - transform.position.y;
            float totalScore = distanceScore + heightScore * 2f;

            if (totalScore > bestScore)
            {
                bestScore = totalScore;
                bestPosition = position;
            }
        }

        if (bestPosition != null)
        {
            currentSnipingPosition = bestPosition;
            if (navAgent != null)
            {
                navAgent.SetDestination(currentSnipingPosition.position);
            }
        }
    }

    public override void AttackPlayer()
    {
        if (player == null) return;

        // Якщо не прицілюємося, починаємо прицілювання
        if (!isAiming)
        {
            StartAiming();
            return;
        }

        // Процес прицілювання
        aimTimer += Time.deltaTime;
        
        // Показуємо лазерний приціл
        if (laserSight != null)
        {
            laserSight.enabled = true;
            laserSight.SetPosition(0, transform.position + Vector3.up * 1.5f);
            laserSight.SetPosition(1, player.position + Vector3.up * 1.5f);
        }

        // Стріляємо після прицілювання
        if (aimTimer >= aimTime)
        {
            FireSniperShot();
            StopAiming();
        }
    }

    void StartAiming()
    {
        isAiming = true;
        aimTimer = 0f;

        // Звук прицілювання
        if (audioCollection.alertSound != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySound3D(audioCollection.alertSound, transform.position);
        }

        Debug.Log($"{enemyName} прицілюється!");
    }

    void FireSniperShot()
    {
        Vector3 targetPosition = player.position + Vector3.up * 1.5f;
        Vector3 direction = (targetPosition - transform.position).normalized;

        // Точний постріл
        RaycastHit hit;
        if (Physics.Raycast(transform.position + Vector3.up * 1.5f, direction, out hit, attackRange))
        {
            if (hit.transform.CompareTag("Player"))
            {
                var playerHealth = hit.transform.GetComponent<PlayerHealth>();
                if (playerHealth != null)
                {
                    playerHealth.TakeDamage(attackDamage, "Sniper Enemy");
                }

                // Ефект попадання
                if (VisualEffectsManager.Instance != null)
                {
                    VisualEffectsManager.Instance.PlayBloodEffect(hit.point, direction);
                }
            }
        }

        // Звук пострілу
        if (audioCollection.attackSound != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySound3D(audioCollection.attackSound, transform.position);
        }

        Debug.Log($"{enemyName} зробив снайперський постріл!");
    }

    void StopAiming()
    {
        isAiming = false;
        aimTimer = 0f;

        if (laserSight != null)
        {
            laserSight.enabled = false;
        }

        // Після пострілу снайпер може змінити позицію
        if (Random.value < 0.6f)
        {
            FindBestSnipingPosition();
        }
    }

    protected override void OnDamageReceived(Vector3 hitPoint, Vector3 hitDirection)
    {
        base.OnDamageReceived(hitPoint, hitDirection);
        
        // Снайпер переміщується при отриманні урону
        StopAiming();
        FindBestSnipingPosition();
    }
}

// ================================
// МЕДИК
// ================================

[CreateAssetMenu(fileName = "Medic_Config", menuName = "Game/Enemies/Medic")]
public class MedicEnemy : Enemy
{
    [Header("Medic Specific")]
    [Tooltip("Сила лікування")]
    public float healAmount = 50f;
    [Tooltip("Дальність лікування")]
    public float healRange = 10f;
    [Tooltip("Час між лікуваннями")]
    public float healCooldown = 5f;

    private float lastHealTime = 0f;

    void Reset()
    {
        // Налаштування для медика
        maxHealth = 100f;
        currentHealth = 100f;
        attackDamage = 15f;
        attackRate = 1.5f;
        attackRange = 12f;
        detectionRange = 18f;
        moveSpeed = 4f;
        runSpeed = 6f;
        armor = 0.1f;
        enemyType = EnemyType.Medic;
    }

    void Update()
    {
        base.Update();
        
        // Перевіряємо, чи потрібно когось лікувати
        if (Time.time - lastHealTime >= healCooldown)
        {
            TryHealNearbyEnemies();
        }
    }

    void TryHealNearbyEnemies()
    {
        Collider[] nearbyEnemies = Physics.OverlapSphere(transform.position, healRange);
        Enemy bestTarget = null;
        float lowestHealthPercent = 1f;

        foreach (var collider in nearbyEnemies)
        {
            Enemy enemy = collider.GetComponent<Enemy>();
            if (enemy != null && enemy != this && !enemy.IsDead)
            {
                float healthPercent = enemy.currentHealth / enemy.maxHealth;
                if (healthPercent < lowestHealthPercent && healthPercent < 0.7f)
                {
                    lowestHealthPercent = healthPercent;
                    bestTarget = enemy;
                }
            }
        }

        if (bestTarget != null)
        {
            HealEnemy(bestTarget);
        }
    }

    void HealEnemy(Enemy target)
    {
        lastHealTime = Time.time;

        // Лікуємо ціль
        target.currentHealth = Mathf.Min(target.maxHealth, target.currentHealth + healAmount);

        // Ефект лікування
        if (VisualEffectsManager.Instance != null)
        {
            VisualEffectsManager.Instance.PlayHealEffect(target.transform.position);
        }

        // Звук лікування
        if (audioCollection.alertSound != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySound3D(audioCollection.alertSound, transform.position);
        }

        Debug.Log($"{enemyName} вилікував {target.enemyName} на {healAmount} HP!");
    }

    protected override void OnDamageReceived(Vector3 hitPoint, Vector3 hitDirection)
    {
        base.OnDamageReceived(hitPoint, hitDirection);
        
        // Медик намагається втекти при отриманні урону
        if (currentHealth < maxHealth * 0.5f)
        {
            StartCoroutine(EmergencyRetreat());
        }
    }

    System.Collections.IEnumerator EmergencyRetreat()
    {
        if (navAgent == null) yield break;

        // Швидко відступаємо
        Vector3 retreatDirection = (transform.position - player.position).normalized;
        Vector3 retreatPosition = transform.position + retreatDirection * 8f;

        navAgent.speed = runSpeed * 1.5f;
        navAgent.SetDestination(retreatPosition);
        
        yield return new WaitForSeconds(2f);
        
        navAgent.speed = moveSpeed;
    }
}

// ================================
// БОС
// ================================

[CreateAssetMenu(fileName = "Boss_Config", menuName = "Game/Enemies/Boss")]
public class BossEnemy : Enemy
{
    [Header("Boss Specific")]
    [Tooltip("Фази бою")]
    public BossPhase[] phases;
    [Tooltip("Поточна фаза")]
    public int currentPhase = 0;
    [Tooltip("Спеціальні атаки")]
    public BossSpecialAttack[] specialAttacks;

    private float phaseTransitionTimer = 0f;
    private bool isInSpecialAttack = false;

    [System.Serializable]
    public struct BossPhase
    {
        public string phaseName;
        public float healthThreshold; // Відсоток здоров'я для переходу
        public float damageMultiplier;
        public float speedMultiplier;
        public float attackRateMultiplier;
    }

    [System.Serializable]
    public struct BossSpecialAttack
    {
        public string attackName;
        public float damage;
        public float range;
        public float cooldown;
        public float castTime;
    }

    void Reset()
    {
        // Налаштування для боса
        maxHealth = 1000f;
        currentHealth = 1000f;
        attackDamage = 60f;
        attackRate = 1f;
        attackRange = 15f;
        detectionRange = 30f;
        moveSpeed = 3f;
        runSpeed = 5f;
        armor = 0.4f;
        enemyType = EnemyType.Boss;
    }

    void Update()
    {
        base.Update();
        CheckPhaseTransition();
    }

    void CheckPhaseTransition()
    {
        if (phases == null || currentPhase >= phases.Length) return;

        float healthPercent = currentHealth / maxHealth;
        
        if (healthPercent <= phases[currentPhase].healthThreshold)
        {
            TransitionToNextPhase();
        }
    }

    void TransitionToNextPhase()
    {
        if (currentPhase < phases.Length - 1)
        {
            currentPhase++;
            ApplyPhaseEffects();
            
            Debug.Log($"{enemyName} переходить до фази {currentPhase + 1}: {phases[currentPhase].phaseName}");
            
            // Ефект переходу фази
            if (VisualEffectsManager.Instance != null)
            {
                VisualEffectsManager.Instance.PlayExplosionEffect(transform.position, 3f);
            }
        }
    }

    void ApplyPhaseEffects()
    {
        if (currentPhase >= phases.Length) return;

        var phase = phases[currentPhase];
        
        // Застосовуємо модифікатори фази
        attackDamage *= phase.damageMultiplier;
        attackRate *= phase.attackRateMultiplier;
        
        if (navAgent != null)
        {
            navAgent.speed = moveSpeed * phase.speedMultiplier;
        }
    }

    public override void AttackPlayer()
    {
        if (isInSpecialAttack) return;

        // Шанс використати спеціальну атаку
        if (specialAttacks != null && specialAttacks.Length > 0 && Random.value < 0.3f)
        {
            StartCoroutine(PerformSpecialAttack());
            return;
        }

        base.AttackPlayer();
    }

    System.Collections.IEnumerator PerformSpecialAttack()
    {
        isInSpecialAttack = true;
        
        // Вибираємо випадкову спеціальну атаку
        var attack = specialAttacks[Random.Range(0, specialAttacks.Length)];
        
        Debug.Log($"{enemyName} використовує {attack.attackName}!");
        
        // Час каста
        yield return new WaitForSeconds(attack.castTime);
        
        // Виконуємо атаку
        ExecuteSpecialAttack(attack);
        
        isInSpecialAttack = false;
    }

    void ExecuteSpecialAttack(BossSpecialAttack attack)
    {
        switch (attack.attackName)
        {
            case "Area Blast":
                AreaBlastAttack(attack);
                break;
            case "Charge Attack":
                ChargeAttack(attack);
                break;
            case "Summon Minions":
                SummonMinions();
                break;
        }
    }

    void AreaBlastAttack(BossSpecialAttack attack)
    {
        // Вибух навколо боса
        Collider[] targets = Physics.OverlapSphere(transform.position, attack.range);
        
        foreach (var target in targets)
        {
            if (target.CompareTag("Player"))
            {
                var playerHealth = target.GetComponent<PlayerHealth>();
                if (playerHealth != null)
                {
                    playerHealth.TakeDamage(attack.damage, $"Boss {attack.attackName}");
                }
            }
        }

        // Ефект вибуху
        if (VisualEffectsManager.Instance != null)
        {
            VisualEffectsManager.Instance.PlayExplosionEffect(transform.position, attack.range);
        }
    }

    void ChargeAttack(BossSpecialAttack attack)
    {
        if (player == null) return;

        // Швидкий рух до гравця
        StartCoroutine(ChargeAtPlayer(attack));
    }

    System.Collections.IEnumerator ChargeAtPlayer(BossSpecialAttack attack)
    {
        Vector3 startPosition = transform.position;
        Vector3 targetPosition = player.position;
        
        float chargeTime = 1f;
        float elapsedTime = 0f;

        while (elapsedTime < chargeTime)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / chargeTime;
            
            transform.position = Vector3.Lerp(startPosition, targetPosition, t);
            
            // Перевіряємо зіткнення з гравцем
            if (Vector3.Distance(transform.position, player.position) < 2f)
            {
                var playerHealth = player.GetComponent<PlayerHealth>();
                if (playerHealth != null)
                {
                    playerHealth.TakeDamage(attack.damage, $"Boss {attack.attackName}");
                }
                break;
            }
            
            yield return null;
        }
    }

    void SummonMinions()
    {
        // Спавнимо кілька простих ворогів
        for (int i = 0; i < 3; i++)
        {
            Vector3 spawnPosition = transform.position + Random.insideUnitSphere * 5f;
            spawnPosition.y = transform.position.y;
            
            // Тут можна створити простих ворогів
            Debug.Log($"{enemyName} викликає міньйона в позиції {spawnPosition}");
        }
    }

    protected override void Die()
    {
        // Особлива смерть боса
        Debug.Log($"БОС {enemyName} ПЕРЕМОЖЕНИЙ!");
        
        // Великий вибух
        if (VisualEffectsManager.Instance != null)
        {
            VisualEffectsManager.Instance.PlayExplosionEffect(transform.position, 10f);
        }

        // Нагорода за перемогу
        Events.Trigger(new ShowMessageEvent("BOSS DEFEATED!", ShowMessageEvent.MessageType.Success, 5f));
        
        base.Die();
    }
}