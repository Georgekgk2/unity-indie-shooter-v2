using UnityEngine;
using UnityEngine.SceneManagement;

public class Checkpoint : MonoBehaviour
{
    public enum CheckpointAction { SaveGame, Teleport, LoadNextScene }
    
    [Header("Checkpoint Settings")]
    public CheckpointAction actionType = CheckpointAction.SaveGame;
    
    [Header("Teleport Settings")]
    public Transform teleportDestination;
    
    [Header("Scene Loading Settings")]
    public string nextSceneName;
    
    [Header("Visual & Audio Feedback")]
    public ParticleSystem activationEffect;
    public AudioClip activationSound;
    
    [Header("Behavior")]
    public bool disableAfterUse = true;
    
    private bool hasBeenUsed = false;
    
    private void OnTriggerEnter(Collider other)
    {
        if (hasBeenUsed || !other.CompareTag("Player")) return;
        
        Debug.Log($"Player entered checkpoint '{gameObject.name}' with action: {actionType}");
        
        // Виконуємо дію
        switch (actionType)
        {
            case CheckpointAction.SaveGame:
                // PlayerHealth тепер відповідає за збереження.
                // Перевірка на PlayerHealth вже є в OnTriggerEnter самого PlayerHealth.
                break; // Нічого не робимо, бо PlayerHealth сам збереже гру
            case CheckpointAction.Teleport:
                TeleportPlayer(other.transform);
                break;
            case CheckpointAction.LoadNextScene:
                LoadNextScene();
                break;
        }

        if (activationEffect != null) activationEffect.Play();
        if (activationSound != null) AudioSource.PlayClipAtPoint(activationSound, transform.position);
        
        if (disableAfterUse)
        {
            hasBeenUsed = true;
            GetComponent<Collider>().enabled = false;
        }
    }
    
    private void TeleportPlayer(Transform playerTransform)
    {
        if (teleportDestination != null)
        {
            playerTransform.position = teleportDestination.position;
            playerTransform.rotation = teleportDestination.rotation;
        }
        else
        {
            Debug.LogError($"Checkpoint '{gameObject.name}' is set to Teleport, but destination is not set!", this);
        }
    }
    
    private void LoadNextScene()
    {
        if (!string.IsNullOrEmpty(nextSceneName))
        {
            SceneManager.LoadScene(nextSceneName);
        }
        else
        {
            Debug.LogError($"Checkpoint '{gameObject.name}' is set to LoadNextScene, but scene name is not set!", this);
        }
    }
}