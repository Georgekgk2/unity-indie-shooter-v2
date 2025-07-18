using UnityEngine;
using IndieShooter.Audio;

namespace IndieShooter.Audio
{
    public class SoundEffectPlayer : MonoBehaviour
    {
        [Header("Sound Effects")]
        public string[] soundsToPlay;
        
        [Header("Trigger Settings")]
        public bool playOnStart = false;
        public bool playOnEnable = false;
        public bool playOnCollision = false;
        public bool playOnTrigger = false;
        
        [Header("Random Settings")]
        public bool playRandomSound = false;
        public float randomPlayChance = 1f;
        
        [Header("Timing")]
        public float delay = 0f;
        public float cooldown = 0f;
        
        private float lastPlayTime = 0f;
        
        void Start()
        {
            if (playOnStart)
            {
                PlaySound();
            }
        }
        
        void OnEnable()
        {
            if (playOnEnable)
            {
                PlaySound();
            }
        }
        
        void OnCollisionEnter(Collision collision)
        {
            if (playOnCollision)
            {
                PlaySound();
            }
        }
        
        void OnTriggerEnter(Collider other)
        {
            if (playOnTrigger)
            {
                PlaySound();
            }
        }
        
        public void PlaySound()
        {
            if (Time.time - lastPlayTime < cooldown)
            {
                return;
            }
            
            if (Random.value > randomPlayChance)
            {
                return;
            }
            
            if (soundsToPlay.Length == 0)
            {
                Debug.LogWarning("No sounds assigned to SoundEffectPlayer!");
                return;
            }
            
            string soundToPlay;
            
            if (playRandomSound)
            {
                soundToPlay = soundsToPlay[Random.Range(0, soundsToPlay.Length)];
            }
            else
            {
                soundToPlay = soundsToPlay[0];
            }
            
            if (delay > 0f)
            {
                Invoke("PlayDelayedSound", delay);
            }
            else
            {
                PlaySoundNow(soundToPlay);
            }
            
            lastPlayTime = Time.time;
        }
        
        void PlayDelayedSound()
        {
            if (soundsToPlay.Length > 0)
            {
                string soundToPlay = playRandomSound ? 
                    soundsToPlay[Random.Range(0, soundsToPlay.Length)] : 
                    soundsToPlay[0];
                PlaySoundNow(soundToPlay);
            }
        }
        
        void PlaySoundNow(string soundName)
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFXAtPosition(soundName, transform.position);
            }
        }
        
        public void PlaySpecificSound(string soundName)
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFXAtPosition(soundName, transform.position);
            }
        }
        
        public void PlaySoundByIndex(int index)
        {
            if (index >= 0 && index < soundsToPlay.Length)
            {
                PlaySoundNow(soundsToPlay[index]);
            }
        }
    }
}