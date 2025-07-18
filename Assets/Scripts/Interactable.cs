using UnityEngine;

public abstract class Interactable : MonoBehaviour
{
    [Header("Interactable Settings")]
    [Tooltip("Òåêñò ï³äêàçêè, ùî â³äîáðàæàºòüñÿ, êîëè ãðàâåöü äèâèòüñÿ íà îá'ºêò.")]
    public string interactionPrompt = "Âçàºìîä³ÿòè";

    /// <summary>
    /// Öåé ìåòîä áóäå âèêëèêàòèñÿ, êîëè ãðàâåöü âçàºìîä³º ç îá'ºêòîì.
    /// `abstract` îçíà÷àº, ùî êîæåí äî÷³ðí³é êëàñ (íàïðèêëàä, DoorController)
    /// çîáîâ'ÿçàíèé ðåàë³çóâàòè öåé ìåòîä.
    /// </summary>
    /// <returns>True, ÿêùî âçàºìîä³ÿ áóëà óñï³øíîþ.</returns>
    public abstract bool Interact(GameObject interactor);
}