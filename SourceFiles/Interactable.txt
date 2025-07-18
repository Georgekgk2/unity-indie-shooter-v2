using UnityEngine;

public abstract class Interactable : MonoBehaviour
{
    [Header("Interactable Settings")]
    [Tooltip("Текст підказки, що відображається, коли гравець дивиться на об'єкт.")]
    public string interactionPrompt = "Взаємодіяти";

    /// <summary>
    /// Цей метод буде викликатися, коли гравець взаємодіє з об'єктом.
    /// `abstract` означає, що кожен дочірній клас (наприклад, DoorController)
    /// зобов'язаний реалізувати цей метод.
    /// </summary>
    /// <returns>True, якщо взаємодія була успішною.</returns>
    public abstract bool Interact(GameObject interactor);
}