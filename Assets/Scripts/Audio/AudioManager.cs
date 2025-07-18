using UnityEngine;
using System.Collections.Generic;
using IndieShooter.Core;

namespace IndieShooter.Audio
{
    [System.Serializable]
    public class SoundEffect
    {
        public string name;
        public AudioClip clip;
        [Range(0f, 1f)]
        public float volume = 1f;
        [Range(0.1f, 3f)]
        public float pitch = 1f;
        public bool randomizePitch = false;
        [Range(0f, 0.5f)]
        public float pitchVariation = 0.1f;
        public bool loop = false;
        public float cooldown = 0f;
        
        [HideInInspector]
        public float lastPlayTime = 0f;
    }
    
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }
        
        [Header("Audio Settings")]
        [Range(0f, 1f)]
        public float masterVolume = 1f;
        [Range(0f, 1f)]
        public float sfxVolume = 0.8f;
        [Range(0f, 1f)]
        public float ambientVolume = 0.6f;
        
        [Header("Audio Sources")]
        public int audioSourcePoolSize = 10;
        
        [Header("Sound Effects")]
        public List<SoundEffect> soundEffects = new List<SoundEffect>();
        
        private Queue<AudioSource> audioSourcePool;
        private List<AudioSource> activeAudioSources;
        private Dictionary<string, SoundEffect> soundDatabase;
        private Transform audioSourceParent;
        
        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeAudioSystem();
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        void Start()
        {
            // Subscribe to game events
            EventSystem.Instance?.Subscribe("WeaponFired", OnWeaponFired);
            EventSystem.Instance?.Subscribe("BulletHit", OnBulletHit);
            EventSystem.Instance?.Subscribe("EnemyDied", OnEnemyDied);
            EventSystem.Instance?.Subscribe("PlayerDied", OnPlayerDied);
            EventSystem.Instance?.Subscribe("WeaponReloading", OnWeaponReloading);
            EventSystem.Instance?.Subscribe("WeaponReloaded", OnWeaponReloaded);
            EventSystem.Instance?.Subscribe("PlayerSpawned", OnPlayerSpawned);
        }
        
        void InitializeAudioSystem()
        {
            // Create audio source parent
            GameObject parent = new GameObject("Audio Sources");
            parent.transform.SetParent(transform);
            audioSourceParent = parent.transform;
            
            // Initialize audio source pool
            audioSourcePool = new Queue<AudioSource>();
            activeAudioSources = new List<AudioSource>();
            
            for (int i = 0; i < audioSourcePoolSize; i++)
            {
                CreateAudioSource();
            }
            
            // Initialize sound database
            soundDatabase = new Dictionary<string, SoundEffect>();
            foreach (SoundEffect sfx in soundEffects)
            {
                if (!string.IsNullOrEmpty(sfx.name))
                {
                    soundDatabase[sfx.name] = sfx;
                }
            }
            
            // Load audio settings
            LoadAudioSettings();
        }
        
        AudioSource CreateAudioSource()
        {
            GameObject audioObj = new GameObject("AudioSource");
            audioObj.transform.SetParent(audioSourceParent);
            AudioSource audioSource = audioObj.AddComponent<AudioSource>();
            
            // Configure audio source
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 0f; // 2D sound for now
            
            audioSourcePool.Enqueue(audioSource);
            return audioSource;
        }
        
        AudioSource GetAudioSource()
        {
            if (audioSourcePool.Count > 0)
            {
                AudioSource source = audioSourcePool.Dequeue();
                activeAudioSources.Add(source);
                return source;
            }
            else
            {
                // Create new audio source if pool is empty
                AudioSource newSource = CreateAudioSource();
                activeAudioSources.Add(newSource);
                return newSource;
            }
        }
        
        void ReturnAudioSource(AudioSource source)
        {
            if (source != null)
            {
                source.Stop();
                source.clip = null;
                activeAudioSources.Remove(source);
                audioSourcePool.Enqueue(source);
            }
        }
        
        public void PlaySFX(string soundName, Vector3 position = default)
        {
            if (!soundDatabase.ContainsKey(soundName))
            {
                Debug.LogWarning($"Sound effect '{soundName}' not found!");
                return;
            }
            
            SoundEffect sfx = soundDatabase[soundName];
            
            // Check cooldown
            if (Time.time - sfx.lastPlayTime < sfx.cooldown)
            {
                return;
            }
            
            if (sfx.clip == null)
            {
                Debug.LogWarning($"Audio clip for '{soundName}' is null!");
                return;
            }
            
            AudioSource audioSource = GetAudioSource();
            
            // Configure audio source
            audioSource.clip = sfx.clip;
            audioSource.volume = sfx.volume * sfxVolume * masterVolume;
            
            // Handle pitch variation
            if (sfx.randomizePitch)
            {
                audioSource.pitch = sfx.pitch + Random.Range(-sfx.pitchVariation, sfx.pitchVariation);
            }
            else
            {
                audioSource.pitch = sfx.pitch;
            }
            
            audioSource.loop = sfx.loop;
            
            // Set position (for future 3D audio support)
            if (position != default)
            {
                audioSource.transform.position = position;
            }
            
            // Play sound
            audioSource.Play();
            sfx.lastPlayTime = Time.time;
            
            // Return to pool when finished (if not looping)
            if (!sfx.loop)
            {
                StartCoroutine(ReturnAudioSourceWhenFinished(audioSource, sfx.clip.length / audioSource.pitch));
            }
        }
        
        public void PlaySFXAtPosition(string soundName, Vector3 position)
        {
            PlaySFX(soundName, position);
        }
        
        public void StopSFX(string soundName)
        {
            foreach (AudioSource source in activeAudioSources)
            {
                if (source.clip != null && soundDatabase.ContainsKey(soundName))
                {
                    if (source.clip == soundDatabase[soundName].clip)
                    {
                        ReturnAudioSource(source);
                        break;
                    }
                }
            }
        }
        
        public void StopAllSFX()
        {
            List<AudioSource> sourcesToReturn = new List<AudioSource>(activeAudioSources);
            foreach (AudioSource source in sourcesToReturn)
            {
                ReturnAudioSource(source);
            }
        }
        
        System.Collections.IEnumerator ReturnAudioSourceWhenFinished(AudioSource source, float delay)
        {
            yield return new WaitForSeconds(delay);
            ReturnAudioSource(source);
        }
        
        // Volume control methods
        public void SetMasterVolume(float volume)
        {
            masterVolume = Mathf.Clamp01(volume);
            UpdateAllVolumes();
            SaveAudioSettings();
        }
        
        public void SetSFXVolume(float volume)
        {
            sfxVolume = Mathf.Clamp01(volume);
            UpdateAllVolumes();
            SaveAudioSettings();
        }
        
        public void SetAmbientVolume(float volume)
        {
            ambientVolume = Mathf.Clamp01(volume);
            SaveAudioSettings();
        }
        
        void UpdateAllVolumes()
        {
            foreach (AudioSource source in activeAudioSources)
            {
                if (source.clip != null)
                {
                    // Find the original sound effect to get base volume
                    foreach (SoundEffect sfx in soundEffects)
                    {
                        if (sfx.clip == source.clip)
                        {
                            source.volume = sfx.volume * sfxVolume * masterVolume;
                            break;
                        }
                    }
                }
            }
        }
        
        // Event handlers
        void OnWeaponFired(object data)
        {
            PlaySFX("WeaponFire");
        }
        
        void OnBulletHit(object data)
        {
            if (data is RaycastHit hit)
            {
                string soundName = GetImpactSoundName(hit.collider.tag);
                PlaySFXAtPosition(soundName, hit.point);
            }
        }
        
        void OnEnemyDied(object data)
        {
            PlaySFX("EnemyDeath");
        }
        
        void OnPlayerDied(object data)
        {
            PlaySFX("PlayerDeath");
        }
        
        void OnWeaponReloading(object data)
        {
            PlaySFX("WeaponReload");
        }
        
        void OnWeaponReloaded(object data)
        {
            PlaySFX("WeaponReloadComplete");
        }
        
        void OnPlayerSpawned(object data)
        {
            PlaySFX("PlayerSpawn");
        }
        
        string GetImpactSoundName(string surfaceTag)
        {
            switch (surfaceTag)
            {
                case "Ground":
                    return "ImpactDirt";
                case "Wall":
                    return "ImpactConcrete";
                case "Enemy":
                    return "ImpactFlesh";
                default:
                    return "ImpactDefault";
            }
        }
        
        // Settings persistence
        void SaveAudioSettings()
        {
            PlayerPrefs.SetFloat("MasterVolume", masterVolume);
            PlayerPrefs.SetFloat("SFXVolume", sfxVolume);
            PlayerPrefs.SetFloat("AmbientVolume", ambientVolume);
            PlayerPrefs.Save();
        }
        
        void LoadAudioSettings()
        {
            masterVolume = PlayerPrefs.GetFloat("MasterVolume", 1f);
            sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 0.8f);
            ambientVolume = PlayerPrefs.GetFloat("AmbientVolume", 0.6f);
        }
        
        void OnDestroy()
        {
            EventSystem.Instance?.Unsubscribe("WeaponFired", OnWeaponFired);
            EventSystem.Instance?.Unsubscribe("BulletHit", OnBulletHit);
            EventSystem.Instance?.Unsubscribe("EnemyDied", OnEnemyDied);
            EventSystem.Instance?.Unsubscribe("PlayerDied", OnPlayerDied);
            EventSystem.Instance?.Unsubscribe("WeaponReloading", OnWeaponReloading);
            EventSystem.Instance?.Unsubscribe("WeaponReloaded", OnWeaponReloaded);
            EventSystem.Instance?.Unsubscribe("PlayerSpawned", OnPlayerSpawned);
        }
    }
}