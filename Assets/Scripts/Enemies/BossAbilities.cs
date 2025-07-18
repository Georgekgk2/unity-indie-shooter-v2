using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using IndieShooter.Core;

namespace IndieShooter.Enemies
{
    /// <summary>
    /// Система здібностей боса. Управляє всіма атаками та спеціальними здібностями.
    /// </summary>
    public class BossAbilities : MonoBehaviour
    {
        [Header("Ability Settings")]
        [Tooltip("Глобальний кулдаун між здібностями")]
        public float globalCooldown = 1f;
        [Tooltip("Множник кулдауну здібностей")]
        public float cooldownMultiplier = 1f;
        [Tooltip("Чи може використовувати здібності")]
        public bool canUseAbilities = true;
        
        [Header("Basic Abilities")]
        [Tooltip("Базова атака")]
        public BossAbility basicAttack;
        [Tooltip("Атака зарядом")]
        public BossAbility chargeAttack;
        [Tooltip("Атака по площі")]
        public BossAbility areaAttack;
        
        [Header("Special Abilities")]
        [Tooltip("Ультимативна здібність")]
        public BossAbility ultimateAbility;
        [Tooltip("Здібність призову міньйонів")]
        public BossAbility summonMinions;
        [Tooltip("Здібність телепортації")]
        public BossAbility teleportAbility;
        
        [Header("Phase-Specific Abilities")]
        [Tooltip("Здібності для кожної фази")]
        public PhaseAbilitySet[] phaseAbilities;
        
        // Посилання на основний контролер
        private BossController bossController;
        private BossPhases phaseManager;
        
        // Стан здібностей
        private Dictionary<string, float> abilityCooldowns = new Dictionary<string, float>();
        private float lastAbilityTime = 0f;
        private bool isExecutingAbility = false;
        private Queue<BossAbility> abilityQueue = new Queue<BossAbility>();
        
        // Події здібностей
        public System.Action<string> OnAbilityExecuted;
        public System.Action<string> OnAbilityStarted;
        public System.Action<string> OnAbilityEnded;
        public System.Action<string> OnAbilityCooldownStarted;
        
        public void Initialize(BossController controller)
        {
            bossController = controller;
            phaseManager = controller.GetComponent<BossPhases>();
            SetupAbilities();
        }
        
        void Start()
        {
            ValidateAbilities();
        }
        
        void Update()
        {
            if (!canUseAbilities) return;
            
            UpdateCooldowns();
            ProcessAbilityQueue();
        }
        
        /// <summary>
        /// Налаштовує здібності боса
        /// </summary>
        private void SetupAbilities()
        {
            // Ініціалізуємо базові здібності якщо не налаштовані
            if (basicAttack == null)
            {
                basicAttack = CreateBasicAttack();
            }
            
            if (chargeAttack == null)
            {
                chargeAttack = CreateChargeAttack();
            }
            
            if (areaAttack == null)
            {
                areaAttack = CreateAreaAttack();
            }
            
            if (ultimateAbility == null)
            {
                ultimateAbility = CreateUltimateAbility();
            }
            
            // Ініціалізуємо кулдауни
            InitializeCooldowns();
        }
        
        /// <summary>
        /// Створює базову атаку
        /// </summary>
        private BossAbility CreateBasicAttack()
        {
            return new BossAbility
            {
                abilityName = "BasicAttack",
                damage = 50f,
                range = 5f,
                cooldown = 2f,
                castTime = 0.5f,
                abilityType = AbilityType.Melee,
                targetType = TargetType.Single
            };
        }
        
        /// <summary>
        /// Створює атаку зарядом
        /// </summary>
        private BossAbility CreateChargeAttack()
        {
            return new BossAbility
            {
                abilityName = "ChargeAttack",
                damage = 80f,
                range = 15f,
                cooldown = 8f,
                castTime = 1f,
                abilityType = AbilityType.Charge,
                targetType = TargetType.Single
            };
        }
        
        /// <summary>
        /// Створює атаку по площі
        /// </summary>
        private BossAbility CreateAreaAttack()
        {
            return new BossAbility
            {
                abilityName = "AreaAttack",
                damage = 60f,
                range = 8f,
                cooldown = 12f,
                castTime = 2f,
                abilityType = AbilityType.Area,
                targetType = TargetType.Area
            };
        }
        
        /// <summary>
        /// Створює ультимативну здібність
        /// </summary>
        private BossAbility CreateUltimateAbility()
        {
            return new BossAbility
            {
                abilityName = "UltimateAbility",
                damage = 150f,
                range = 20f,
                cooldown = 30f,
                castTime = 3f,
                abilityType = AbilityType.Ultimate,
                targetType = TargetType.All
            };
        }
        
        /// <summary>
        /// Ініціалізує кулдауни здібностей
        /// </summary>
        private void InitializeCooldowns()
        {
            var abilities = GetAllAbilities();
            foreach (var ability in abilities)
            {
                if (ability != null)
                {
                    abilityCooldowns[ability.abilityName] = 0f;
                }
            }
        }
        
        /// <summary>
        /// Перевіряє правильність налаштування здібностей
        /// </summary>
        private void ValidateAbilities()
        {
            var abilities = GetAllAbilities();
            foreach (var ability in abilities)
            {
                if (ability != null && string.IsNullOrEmpty(ability.abilityName))
                {
                    Debug.LogWarning("BossAbilities: Found ability without name!", this);
                }
            }
        }
        
        /// <summary>
        /// Оновлює кулдауни здібностей
        /// </summary>
        private void UpdateCooldowns()
        {
            var keys = new List<string>(abilityCooldowns.Keys);
            foreach (var key in keys)
            {
                if (abilityCooldowns[key] > 0)
                {
                    abilityCooldowns[key] -= Time.deltaTime;
                    abilityCooldowns[key] = Mathf.Max(0, abilityCooldowns[key]);
                }
            }
        }
        
        /// <summary>
        /// Обробляє чергу здібностей
        /// </summary>
        private void ProcessAbilityQueue()
        {
            if (isExecutingAbility || abilityQueue.Count == 0) return;
            
            if (Time.time - lastAbilityTime >= globalCooldown)
            {
                var nextAbility = abilityQueue.Dequeue();
                ExecuteAbilityInternal(nextAbility);
            }
        }
        
        /// <summary>
        /// Використовує здібність
        /// </summary>
        public bool UseAbility(string abilityName, Transform target = null)
        {
            var ability = GetAbilityByName(abilityName);
            if (ability == null)
            {
                Debug.LogWarning($"BossAbilities: Ability '{abilityName}' not found!", this);
                return false;
            }
            
            return UseAbility(ability, target);
        }
        
        /// <summary>
        /// Використовує здібність
        /// </summary>
        public bool UseAbility(BossAbility ability, Transform target = null)
        {
            if (!CanUseAbility(ability))
            {
                return false;
            }
            
            // Додаємо до черги або виконуємо відразу
            if (isExecutingAbility || Time.time - lastAbilityTime < globalCooldown)
            {
                abilityQueue.Enqueue(ability);
                return true;
            }
            
            ExecuteAbilityInternal(ability, target);
            return true;
        }
        
        /// <summary>
        /// Перевіряє чи можна використати здібність
        /// </summary>
        public bool CanUseAbility(BossAbility ability)
        {
            if (!canUseAbilities || ability == null) return false;
            
            // Перевіряємо кулдаун
            if (abilityCooldowns.ContainsKey(ability.abilityName) && 
                abilityCooldowns[ability.abilityName] > 0)
            {
                return false;
            }
            
            // Перевіряємо глобальний кулдаун
            if (Time.time - lastAbilityTime < globalCooldown)
            {
                return false;
            }
            
            // Перевіряємо чи здібність доступна в поточній фазі
            if (!IsAbilityAvailableInCurrentPhase(ability))
            {
                return false;
            }
            
            return true;
        }
        
        /// <summary>
        /// Виконує здібність
        /// </summary>
        private void ExecuteAbilityInternal(BossAbility ability, Transform target = null)
        {
            isExecutingAbility = true;
            lastAbilityTime = Time.time;
            
            // Тригеримо події
            OnAbilityStarted?.Invoke(ability.abilityName);
            
            // Запускаємо корутину виконання
            StartCoroutine(ExecuteAbilityCoroutine(ability, target));
        }
        
        /// <summary>
        /// Корутина виконання здібності
        /// </summary>
        private IEnumerator ExecuteAbilityCoroutine(BossAbility ability, Transform target)
        {
            // Фаза підготовки (cast time)
            if (ability.castTime > 0)
            {
                yield return StartCoroutine(CastAbility(ability, target));
            }
            
            // Виконання здібності
            yield return StartCoroutine(PerformAbility(ability, target));
            
            // Завершення
            CompleteAbility(ability);
        }
        
        /// <summary>
        /// Фаза підготовки здібності
        /// </summary>
        private IEnumerator CastAbility(BossAbility ability, Transform target)
        {
            float castTimer = 0f;
            
            // Анімація підготовки
            var animator = bossController.GetComponent<Animator>();
            if (animator != null)
            {
                animator.SetTrigger($"Cast{ability.abilityName}");
            }
            
            // Ефекти підготовки
            ShowCastEffect(ability, target);
            
            while (castTimer < ability.castTime)
            {
                castTimer += Time.deltaTime;
                
                // Можна додати ефекти зарядки
                UpdateCastEffect(ability, castTimer / ability.castTime);
                
                yield return null;
            }
        }
        
        /// <summary>
        /// Виконання здібності
        /// </summary>
        private IEnumerator PerformAbility(BossAbility ability, Transform target)
        {
            switch (ability.abilityType)
            {
                case AbilityType.Melee:
                    yield return StartCoroutine(PerformMeleeAttack(ability, target));
                    break;
                    
                case AbilityType.Ranged:
                    yield return StartCoroutine(PerformRangedAttack(ability, target));
                    break;
                    
                case AbilityType.Area:
                    yield return StartCoroutine(PerformAreaAttack(ability, target));
                    break;
                    
                case AbilityType.Charge:
                    yield return StartCoroutine(PerformChargeAttack(ability, target));
                    break;
                    
                case AbilityType.Ultimate:
                    yield return StartCoroutine(PerformUltimateAbility(ability, target));
                    break;
                    
                case AbilityType.Summon:
                    yield return StartCoroutine(PerformSummonAbility(ability, target));
                    break;
                    
                case AbilityType.Teleport:
                    yield return StartCoroutine(PerformTeleportAbility(ability, target));
                    break;
            }
        }
        
        /// <summary>
        /// Виконує атаку ближнього бою
        /// </summary>
        private IEnumerator PerformMeleeAttack(BossAbility ability, Transform target)
        {
            if (target == null) target = bossController.GetTarget();
            if (target == null) yield break;
            
            // Анімація атаки
            var animator = bossController.GetComponent<Animator>();
            if (animator != null)
            {
                animator.SetTrigger("MeleeAttack");
            }
            
            // Чекаємо момент удару в анімації
            yield return new WaitForSeconds(0.3f);
            
            // Перевіряємо відстань та завдаємо урон
            float distance = Vector3.Distance(transform.position, target.position);
            if (distance <= ability.range)
            {
                DealDamageToTarget(target, ability.damage);
                ShowHitEffect(target.position, ability);
            }
            
            yield return new WaitForSeconds(0.2f);
        }
        
        /// <summary>
        /// Виконує дальню атаку
        /// </summary>
        private IEnumerator PerformRangedAttack(BossAbility ability, Transform target)
        {
            if (target == null) target = bossController.GetTarget();
            if (target == null) yield break;
            
            // Створюємо снаряд
            CreateProjectile(ability, target);
            
            yield return new WaitForSeconds(0.5f);
        }
        
        /// <summary>
        /// Виконує атаку по площі
        /// </summary>
        private IEnumerator PerformAreaAttack(BossAbility ability, Transform target)
        {
            Vector3 attackCenter = target != null ? target.position : transform.position + transform.forward * 5f;
            
            // Показуємо попередження про атаку
            ShowAreaWarning(attackCenter, ability.range);
            
            yield return new WaitForSeconds(1f);
            
            // Виконуємо атаку по площі
            Collider[] hits = Physics.OverlapSphere(attackCenter, ability.range);
            foreach (var hit in hits)
            {
                if (hit.CompareTag("Player"))
                {
                    DealDamageToTarget(hit.transform, ability.damage);
                }
            }
            
            // Ефект вибуху
            ShowExplosionEffect(attackCenter, ability);
            
            yield return new WaitForSeconds(0.5f);
        }
        
        /// <summary>
        /// Виконує атаку зарядом
        /// </summary>
        private IEnumerator PerformChargeAttack(BossAbility ability, Transform target)
        {
            if (target == null) target = bossController.GetTarget();
            if (target == null) yield break;
            
            Vector3 startPosition = transform.position;
            Vector3 targetPosition = target.position;
            Vector3 direction = (targetPosition - startPosition).normalized;
            
            // Рухаємося до цілі
            float chargeSpeed = 15f;
            float chargeDistance = Vector3.Distance(startPosition, targetPosition);
            float chargeTime = chargeDistance / chargeSpeed;
            
            float elapsedTime = 0f;
            while (elapsedTime < chargeTime)
            {
                elapsedTime += Time.deltaTime;
                float t = elapsedTime / chargeTime;
                
                transform.position = Vector3.Lerp(startPosition, targetPosition, t);
                
                // Перевіряємо зіткнення з гравцями
                Collider[] hits = Physics.OverlapSphere(transform.position, 2f);
                foreach (var hit in hits)
                {
                    if (hit.CompareTag("Player"))
                    {
                        DealDamageToTarget(hit.transform, ability.damage);
                        ShowHitEffect(hit.transform.position, ability);
                    }
                }
                
                yield return null;
            }
            
            yield return new WaitForSeconds(0.5f);
        }
        
        /// <summary>
        /// Виконує ультимативну здібність
        /// </summary>
        private IEnumerator PerformUltimateAbility(BossAbility ability, Transform target)
        {
            // Ультимативна здібність - потужна атака по всій арені
            
            // Фаза 1: Підготовка
            ShowUltimateWarning();
            yield return new WaitForSeconds(2f);
            
            // Фаза 2: Виконання
            GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
            foreach (var player in players)
            {
                DealDamageToTarget(player.transform, ability.damage);
                ShowHitEffect(player.transform.position, ability);
            }
            
            // Глобальний ефект
            ShowUltimateEffect();
            
            yield return new WaitForSeconds(1f);
        }
        
        /// <summary>
        /// Виконує призов міньйонів
        /// </summary>
        private IEnumerator PerformSummonAbility(BossAbility ability, Transform target)
        {
            int minionCount = 3;
            
            for (int i = 0; i < minionCount; i++)
            {
                Vector3 spawnPosition = transform.position + Random.insideUnitSphere * 5f;
                spawnPosition.y = transform.position.y;
                
                // Тут буде створення міньйона
                ShowSummonEffect(spawnPosition);
                
                yield return new WaitForSeconds(0.5f);
            }
        }
        
        /// <summary>
        /// Виконує телепортацію
        /// </summary>
        private IEnumerator PerformTeleportAbility(BossAbility ability, Transform target)
        {
            // Ефект зникнення
            ShowTeleportEffect(transform.position, true);
            
            yield return new WaitForSeconds(0.5f);
            
            // Телепортуємося
            Vector3 newPosition = GetRandomTeleportPosition();
            transform.position = newPosition;
            
            // Ефект появи
            ShowTeleportEffect(transform.position, false);
            
            yield return new WaitForSeconds(0.3f);
        }
        
        /// <summary>
        /// Завершує виконання здібності
        /// </summary>
        private void CompleteAbility(BossAbility ability)
        {
            // Встановлюємо кулдаун
            float actualCooldown = ability.cooldown * cooldownMultiplier;
            abilityCooldowns[ability.abilityName] = actualCooldown;
            
            // Тригеримо події
            OnAbilityExecuted?.Invoke(ability.abilityName);
            OnAbilityEnded?.Invoke(ability.abilityName);
            OnAbilityCooldownStarted?.Invoke(ability.abilityName);
            
            isExecutingAbility = false;
            
            Debug.Log($"Boss completed ability: {ability.abilityName}");
        }
        
        /// <summary>
        /// Завдає урон цілі
        /// </summary>
        private void DealDamageToTarget(Transform target, float damage)
        {
            var playerHealth = target.GetComponent<MonoBehaviour>(); // PlayerHealth
            if (playerHealth != null)
            {
                // playerHealth.TakeDamage(damage, "Boss");
            }
            
            // Тригеримо подію
            EventSystem.Instance?.TriggerEvent("BossDamageDealt", new {
                target = target.name,
                damage = damage,
                abilityName = "BossAbility"
            });
        }
        
        /// <summary>
        /// Показує ефект попадання
        /// </summary>
        private void ShowHitEffect(Vector3 position, BossAbility ability)
        {
            // Тут можна додати партикли попадання
        }
        
        /// <summary>
        /// Показує ефект підготовки
        /// </summary>
        private void ShowCastEffect(BossAbility ability, Transform target)
        {
            // Ефекти підготовки здібності
        }
        
        /// <summary>
        /// Оновлює ефект підготовки
        /// </summary>
        private void UpdateCastEffect(BossAbility ability, float progress)
        {
            // Оновлення ефектів під час підготовки
        }
        
        /// <summary>
        /// Показує попередження про атаку по площі
        /// </summary>
        private void ShowAreaWarning(Vector3 center, float radius)
        {
            // Візуальне попередження про атаку
        }
        
        /// <summary>
        /// Показує ефект вибуху
        /// </summary>
        private void ShowExplosionEffect(Vector3 center, BossAbility ability)
        {
            // Ефект вибуху
        }
        
        /// <summary>
        /// Показує попередження про ультимативну здібність
        /// </summary>
        private void ShowUltimateWarning()
        {
            // Глобальне попередження
        }
        
        /// <summary>
        /// Показує ефект ультимативної здібності
        /// </summary>
        private void ShowUltimateEffect()
        {
            // Глобальний ефект
        }
        
        /// <summary>
        /// Показує ефект призову
        /// </summary>
        private void ShowSummonEffect(Vector3 position)
        {
            // Ефект призову міньйона
        }
        
        /// <summary>
        /// Показує ефект телепортації
        /// </summary>
        private void ShowTeleportEffect(Vector3 position, bool isDisappearing)
        {
            // Ефект телепортації
        }
        
        /// <summary>
        /// Створює снаряд
        /// </summary>
        private void CreateProjectile(BossAbility ability, Transform target)
        {
            // Створення та запуск снаряда
        }
        
        /// <summary>
        /// Отримує випадкову позицію для телепортації
        /// </summary>
        private Vector3 GetRandomTeleportPosition()
        {
            // Логіка вибору позиції для телепортації
            return transform.position + Random.insideUnitSphere * 10f;
        }
        
        /// <summary>
        /// Перевіряє чи здібність доступна в поточній фазі
        /// </summary>
        private bool IsAbilityAvailableInCurrentPhase(BossAbility ability)
        {
            if (phaseManager == null) return true;
            
            var currentPhase = phaseManager.GetCurrentPhase();
            if (currentPhase == null) return true;
            
            // Перевіряємо чи здібність є в наборі поточної фази
            foreach (var abilityName in currentPhase.abilitySet)
            {
                if (abilityName == ability.abilityName)
                {
                    return true;
                }
            }
            
            return false;
        }
        
        /// <summary>
        /// Отримує здібність за назвою
        /// </summary>
        private BossAbility GetAbilityByName(string abilityName)
        {
            var abilities = GetAllAbilities();
            foreach (var ability in abilities)
            {
                if (ability != null && ability.abilityName == abilityName)
                {
                    return ability;
                }
            }
            return null;
        }
        
        /// <summary>
        /// Отримує всі здібності
        /// </summary>
        private BossAbility[] GetAllAbilities()
        {
            return new BossAbility[] 
            { 
                basicAttack, chargeAttack, areaAttack, 
                ultimateAbility, summonMinions, teleportAbility 
            };
        }
        
        // === Публічні методи ===
        
        public void UpdateAbilities()
        {
            // Викликається з BossController.Update()
        }
        
        public bool IsExecutingAbility() => isExecutingAbility;
        public float GetAbilityCooldown(string abilityName) => abilityCooldowns.ContainsKey(abilityName) ? abilityCooldowns[abilityName] : 0f;
        public BossAbility[] GetAvailableAbilities() => GetAllAbilities();
        
        public void SetCooldownMultiplier(float multiplier)
        {
            cooldownMultiplier = multiplier;
        }
        
        public void ResetAllCooldowns()
        {
            var keys = new List<string>(abilityCooldowns.Keys);
            foreach (var key in keys)
            {
                abilityCooldowns[key] = 0f;
            }
        }
    }
    
    /// <summary>
    /// Дані здібності боса
    /// </summary>
    [System.Serializable]
    public class BossAbility
    {
        [Header("Basic Info")]
        public string abilityName = "";
        public string description = "";
        public Sprite icon;
        
        [Header("Combat Stats")]
        public float damage = 50f;
        public float range = 5f;
        public float cooldown = 5f;
        public float castTime = 1f;
        
        [Header("Ability Properties")]
        public AbilityType abilityType = AbilityType.Melee;
        public TargetType targetType = TargetType.Single;
        public bool canBeInterrupted = true;
        public bool requiresLineOfSight = true;
        
        [Header("Effects")]
        public GameObject castEffect;
        public GameObject hitEffect;
        public AudioClip soundEffect;
        
        [Header("Advanced")]
        public int minPhase = 1;
        public int maxPhase = 3;
        public float healthThreshold = 0f; // Мінімальний відсоток здоров'я для використання
    }
    
    /// <summary>
    /// Набір здібностей для фази
    /// </summary>
    [System.Serializable]
    public class PhaseAbilitySet
    {
        public int phaseNumber = 1;
        public string[] availableAbilities;
        public float[] abilityWeights; // Ймовірність використання кожної здібності
    }
    
    /// <summary>
    /// Типи здібностей
    /// </summary>
    public enum AbilityType
    {
        Melee,      // Ближній бій
        Ranged,     // Дальня атака
        Area,       // Атака по площі
        Charge,     // Атака зарядом
        Ultimate,   // Ультимативна здібність
        Summon,     // Призов
        Teleport,   // Телепортація
        Buff,       // Підсилення
        Debuff,     // Ослаблення
        Heal        // Лікування
    }
    
    /// <summary>
    /// Типи цілей
    /// </summary>
    public enum TargetType
    {
        Single,     // Одна ціль
        Multiple,   // Кілька цілей
        Area,       // Область
        All,        // Всі гравці
        Self,       // Сам бос
        Random      // Випадкова ціль
    }
}