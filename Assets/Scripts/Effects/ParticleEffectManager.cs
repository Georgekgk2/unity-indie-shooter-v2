using UnityEngine;
using System.Collections.Generic;
using IndieShooter.Core;

namespace IndieShooter.Effects
{
    [System.Serializable]
    public class EffectData
    {
        public string effectName;
        public GameObject effectPrefab;
        public float lifetime = 2f;
        public bool useObjectPool = true;
    }
    
    public class ParticleEffectManager : MonoBehaviour
    {
        public static ParticleEffectManager Instance { get; private set; }
        
        [Header("Effect Prefabs")]
        public List<EffectData> effects = new List<EffectData>();
        
        [Header("Settings")]
        public int poolSize = 20;
        public Transform effectParent;
        
        private Dictionary<string, Queue<GameObject>> effectPools;
        private Dictionary<string, EffectData> effectDatabase;
        
        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeEffectSystem();
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        void Start()
        {
            // Subscribe to events
            EventSystem.Instance?.Subscribe("WeaponFired", OnWeaponFired);
            EventSystem.Instance?.Subscribe("BulletHit", OnBulletHit);
            EventSystem.Instance?.Subscribe("EnemyDied", OnEnemyDied);
            EventSystem.Instance?.Subscribe("PlayerDied", OnPlayerDied);
        }
        
        void InitializeEffectSystem()
        {
            effectPools = new Dictionary<string, Queue<GameObject>>();
            effectDatabase = new Dictionary<string, EffectData>();
            
            if (effectParent == null)
            {
                GameObject parent = new GameObject("Effect Parent");
                effectParent = parent.transform;
                parent.transform.SetParent(transform);
            }
            
            // Initialize pools for each effect
            foreach (EffectData effect in effects)
            {
                effectDatabase[effect.effectName] = effect;
                
                if (effect.useObjectPool)
                {
                    Queue<GameObject> pool = new Queue<GameObject>();
                    
                    for (int i = 0; i < poolSize; i++)
                    {
                        GameObject obj = Instantiate(effect.effectPrefab, effectParent);
                        obj.SetActive(false);
                        pool.Enqueue(obj);
                    }
                    
                    effectPools[effect.effectName] = pool;
                }
            }
        }
        
        public GameObject PlayEffect(string effectName, Vector3 position, Quaternion rotation = default)
        {
            if (!effectDatabase.ContainsKey(effectName))
            {
                Debug.LogWarning($"Effect '{effectName}' not found in database!");
                return null;
            }
            
            EffectData effectData = effectDatabase[effectName];
            GameObject effectObject;
            
            if (effectData.useObjectPool && effectPools.ContainsKey(effectName))
            {
                // Get from pool
                effectObject = GetFromPool(effectName);
                if (effectObject == null)
                {
                    // Pool is empty, create new one
                    effectObject = Instantiate(effectData.effectPrefab, effectParent);
                }
            }
            else
            {
                // Create new instance
                effectObject = Instantiate(effectData.effectPrefab, effectParent);
            }
            
            // Position and activate effect
            effectObject.transform.position = position;
            effectObject.transform.rotation = rotation;
            effectObject.SetActive(true);
            
            // Auto-return to pool after lifetime
            if (effectData.useObjectPool)
            {
                StartCoroutine(ReturnToPoolAfterTime(effectName, effectObject, effectData.lifetime));
            }
            else
            {
                Destroy(effectObject, effectData.lifetime);
            }
            
            return effectObject;
        }
        
        public GameObject PlayEffect(string effectName, Vector3 position, Vector3 normal)
        {
            Quaternion rotation = Quaternion.LookRotation(normal);
            return PlayEffect(effectName, position, rotation);
        }
        
        GameObject GetFromPool(string effectName)
        {
            if (effectPools[effectName].Count > 0)
            {
                return effectPools[effectName].Dequeue();
            }
            return null;
        }
        
        void ReturnToPool(string effectName, GameObject effectObject)
        {
            if (effectPools.ContainsKey(effectName))
            {
                effectObject.SetActive(false);
                effectPools[effectName].Enqueue(effectObject);
            }
        }
        
        System.Collections.IEnumerator ReturnToPoolAfterTime(string effectName, GameObject effectObject, float time)
        {
            yield return new WaitForSeconds(time);
            ReturnToPool(effectName, effectObject);
        }
        
        // Event handlers
        void OnWeaponFired(object data)
        {
            // Play muzzle flash effect
            // Position would come from weapon fire point
            PlayEffect("MuzzleFlash", Vector3.zero);
        }
        
        void OnBulletHit(object data)
        {
            if (data is RaycastHit hit)
            {
                // Play impact effect based on surface type
                string effectName = GetImpactEffectName(hit.collider.tag);
                PlayEffect(effectName, hit.point, hit.normal);
            }
        }
        
        void OnEnemyDied(object data)
        {
            if (data is GameObject enemy)
            {
                PlayEffect("EnemyDeath", enemy.transform.position);
            }
        }
        
        void OnPlayerDied(object data)
        {
            // Play player death effect
            PlayEffect("PlayerDeath", Vector3.zero);
        }
        
        string GetImpactEffectName(string surfaceTag)
        {
            switch (surfaceTag)
            {
                case "Ground":
                    return "DirtImpact";
                case "Wall":
                    return "ConcreteImpact";
                case "Enemy":
                    return "BloodImpact";
                default:
                    return "DefaultImpact";
            }
        }
        
        // Public utility methods
        public void StopEffect(GameObject effectObject)
        {
            if (effectObject != null)
            {
                effectObject.SetActive(false);
            }
        }
        
        public void StopAllEffects()
        {
            foreach (Transform child in effectParent)
            {
                child.gameObject.SetActive(false);
            }
        }
        
        void OnDestroy()
        {
            EventSystem.Instance?.Unsubscribe("WeaponFired", OnWeaponFired);
            EventSystem.Instance?.Unsubscribe("BulletHit", OnBulletHit);
            EventSystem.Instance?.Unsubscribe("EnemyDied", OnEnemyDied);
            EventSystem.Instance?.Unsubscribe("PlayerDied", OnPlayerDied);
        }
    }
}