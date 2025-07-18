using UnityEngine;
using UnityEngine.Playables;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// СЮЖЕТНА КАМПАНІЯ - STORY-DRIVEN CAMPAIGN
/// Епічна кампанія з кінематографічними сценами, діалогами та розгалуженим сюжетом
/// Включає систему вибору, персонажів та прогресію історії
/// </summary>

// ================================
// СТРУКТУРА КАМПАНІЇ
// ================================

public enum CampaignChapter
{
    Prologue,           // Пролог - навчання
    Chapter1_Awakening, // Розділ 1: Пробудження
    Chapter2_Resistance,// Розділ 2: Опір
    Chapter3_Discovery, // Розділ 3: Відкриття
    Chapter4_Betrayal,  // Розділ 4: Зрада
    Chapter5_Revelation,// Розділ 5: Одкровення
    Chapter6_Final,     // Розділ 6: Фінал
    Epilogue           // Епілог
}

public enum MissionType
{
    Story,              // Сюжетна місія
    Optional,           // Додаткова місія
    Stealth,            // Стелс місія
    Survival,           // Виживання
    Escort,             // Супровід
    Sabotage,           // Саботаж
    Rescue,             // Порятунок
    Boss               // Бій з босом
}

public enum DialogueChoice
{
    Aggressive,         // Агресивний вибір
    Diplomatic,         // Дипломатичний вибір
    Neutral,            // Нейтральний вибір
    Heroic,             // Героїчний вибір
    Pragmatic          // Прагматичний вибір
}

// ================================
// МЕНЕДЖЕР КАМПАНІЇ
// ================================

public class CampaignManager : MonoBehaviour
{
    [Header("Campaign Progress")]
    public CampaignChapter currentChapter = CampaignChapter.Prologue;
    public int currentMission = 0;
    public float campaignProgress = 0f;
    
    [Header("Story Data")]
    public CampaignData campaignData;
    public List<Mission> allMissions = new List<Mission>();
    public List<Character> storyCharacters = new List<Character>();
    
    [Header("Player Choices")]
    public List<StoryChoice> playerChoices = new List<StoryChoice>();
    public MoralityAlignment playerAlignment = MoralityAlignment.Neutral;
    public int heroicPoints = 0;
    public int pragmaticPoints = 0;
    
    [Header("Cinematics")]
    public PlayableDirector cinematicDirector;
    public CinematicSequence[] chapterIntros;
    public CinematicSequence[] chapterOutros;
    
    [Header("Audio")]
    public AudioSource narratorVoice;
    public AudioSource characterVoice;
    public AudioSource backgroundMusic;
    public AudioClip[] chapterThemes;
    
    private Mission currentMissionData;
    private bool isInCinematic = false;
    private bool isInDialogue = false;
    private DialogueSystem dialogueSystem;
    
    public static CampaignManager Instance { get; private set; }
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void Start()
    {
        InitializeCampaign();
    }
    
    void InitializeCampaign()
    {
        dialogueSystem = FindObjectOfType<DialogueSystem>();
        if (dialogueSystem == null)
        {
            GameObject dialogueObject = new GameObject("DialogueSystem");
            dialogueSystem = dialogueObject.AddComponent<DialogueSystem>();
        }
        
        LoadCampaignData();
        SetupCharacters();
        
        // Початок кампанії
        if (IsNewCampaign())
        {
            StartCampaign();
        }
        else
        {
            LoadCampaignProgress();
        }
    }
    
    void LoadCampaignData()
    {
        if (campaignData == null)
        {
            campaignData = Resources.Load<CampaignData>("Campaign/MainCampaign");
        }
        
        if (campaignData != null)
        {
            allMissions = campaignData.missions.ToList();
            storyCharacters = campaignData.characters.ToList();
        }
    }
    
    void SetupCharacters()
    {
        foreach (var character in storyCharacters)
        {
            character.Initialize();
        }
    }
    
    bool IsNewCampaign()
    {
        return !PlayerPrefs.HasKey("CampaignProgress");
    }
    
    void StartCampaign()
    {
        currentChapter = CampaignChapter.Prologue;
        currentMission = 0;
        campaignProgress = 0f;
        
        PlayChapterIntro(currentChapter);
    }
    
    void LoadCampaignProgress()
    {
        currentChapter = (CampaignChapter)PlayerPrefs.GetInt("CurrentChapter", 0);
        currentMission = PlayerPrefs.GetInt("CurrentMission", 0);
        campaignProgress = PlayerPrefs.GetFloat("CampaignProgress", 0f);
        heroicPoints = PlayerPrefs.GetInt("HeroicPoints", 0);
        pragmaticPoints = PlayerPrefs.GetInt("PragmaticPoints", 0);
        
        UpdatePlayerAlignment();
        LoadPlayerChoices();
    }
    
    void SaveCampaignProgress()
    {
        PlayerPrefs.SetInt("CurrentChapter", (int)currentChapter);
        PlayerPrefs.SetInt("CurrentMission", currentMission);
        PlayerPrefs.SetFloat("CampaignProgress", campaignProgress);
        PlayerPrefs.SetInt("HeroicPoints", heroicPoints);
        PlayerPrefs.SetInt("PragmaticPoints", pragmaticPoints);
        
        SavePlayerChoices();
        PlayerPrefs.Save();
    }
    
    public void StartMission(int missionIndex)
    {
        if (missionIndex >= allMissions.Count) return;
        
        currentMissionData = allMissions[missionIndex];
        currentMission = missionIndex;
        
        // Перевірка передумов
        if (!CheckMissionPrerequisites(currentMissionData))
        {
            UIManager.Instance?.ShowNotification("Місія недоступна", NotificationType.Warning);
            return;
        }
        
        // Запуск pre-mission кінематографа
        if (currentMissionData.preMissionCinematic != null)
        {
            PlayCinematic(currentMissionData.preMissionCinematic);
        }
        else
        {
            StartMissionGameplay();
        }
    }
    
    bool CheckMissionPrerequisites(Mission mission)
    {
        // Перевірка завершених місій
        foreach (int requiredMission in mission.prerequisiteMissions)
        {
            if (!IsMissionCompleted(requiredMission))
            {
                return false;
            }
        }
        
        // Перевірка вибору гравця
        foreach (var requiredChoice in mission.requiredChoices)
        {
            if (!HasPlayerMadeChoice(requiredChoice))
            {
                return false;
            }
        }
        
        return true;
    }
    
    bool IsMissionCompleted(int missionIndex)
    {
        return PlayerPrefs.GetInt($"Mission_{missionIndex}_Completed", 0) == 1;
    }
    
    bool HasPlayerMadeChoice(string choiceId)
    {
        return playerChoices.Any(choice => choice.choiceId == choiceId);
    }
    
    void StartMissionGameplay()
    {
        isInCinematic = false;
        
        // Налаштування місії
        SetupMissionObjectives();
        SetupMissionEnemies();
        SetupMissionEnvironment();
        
        // Запуск місії
        LevelManager.Instance?.LoadLevel(currentMissionData.levelIndex);
        
        // Показати цілі місії
        ShowMissionObjectives();
        
        UIManager.Instance?.ShowNotification($"Місія: {currentMissionData.missionName}", NotificationType.Mission);
    }
    
    void SetupMissionObjectives()
    {
        ObjectiveManager objectiveManager = FindObjectOfType<ObjectiveManager>();
        if (objectiveManager == null)
        {
            GameObject objManager = new GameObject("ObjectiveManager");
            objectiveManager = objManager.AddComponent<ObjectiveManager>();
        }
        
        objectiveManager.SetMissionObjectives(currentMissionData.objectives);
    }
    
    void SetupMissionEnemies()
    {
        // Налаштування ворогів згідно з місією
        EnemySpawner[] spawners = FindObjectsOfType<EnemySpawner>();
        
        foreach (var spawner in spawners)
        {
            spawner.enemyTypes = currentMissionData.enemyTypes;
            spawner.spawnCount = currentMissionData.enemyCount;
            spawner.difficultyMultiplier = GetMissionDifficultyMultiplier();
        }
    }
    
    float GetMissionDifficultyMultiplier()
    {
        float baseMultiplier = 1f;
        
        // Збільшення складності з прогресом кампанії
        baseMultiplier += campaignProgress * 0.5f;
        
        // Модифікатор залежно від вибору гравця
        if (playerAlignment == MoralityAlignment.Heroic)
        {
            baseMultiplier *= 1.1f; // Героїчний шлях складніший
        }
        
        return baseMultiplier;
    }
    
    void SetupMissionEnvironment()
    {
        // Налаштування середовища залежно від місії
        switch (currentMissionData.missionType)
        {
            case MissionType.Stealth:
                SetupStealthEnvironment();
                break;
            case MissionType.Survival:
                SetupSurvivalEnvironment();
                break;
            case MissionType.Escort:
                SetupEscortEnvironment();
                break;
        }
    }
    
    void SetupStealthEnvironment()
    {
        // Зменшення освітлення
        RenderSettings.ambientIntensity *= 0.7f;
        
        // Активація стелс механік
        StealthManager stealthManager = FindObjectOfType<StealthManager>();
        if (stealthManager != null)
        {
            stealthManager.EnableStealthMode();
        }
    }
    
    void SetupSurvivalEnvironment()
    {
        // Активація survival елементів
        SurvivalManager survivalManager = FindObjectOfType<SurvivalManager>();
        if (survivalManager != null)
        {
            survivalManager.StartSurvival();
        }
    }
    
    void SetupEscortEnvironment()
    {
        // Створення NPC для супроводу
        if (currentMissionData.escortTarget != null)
        {
            Vector3 spawnPosition = GetEscortSpawnPosition();
            GameObject escort = Instantiate(currentMissionData.escortTarget, spawnPosition, Quaternion.identity);
            
            EscortNPC escortNPC = escort.GetComponent<EscortNPC>();
            if (escortNPC != null)
            {
                escortNPC.Initialize(currentMissionData.escortDestination);
            }
        }
    }
    
    Vector3 GetEscortSpawnPosition()
    {
        GameObject spawnPoint = GameObject.FindGameObjectWithTag("EscortSpawn");
        return spawnPoint != null ? spawnPoint.transform.position : Vector3.zero;
    }
    
    void ShowMissionObjectives()
    {
        MissionUI missionUI = FindObjectOfType<MissionUI>();
        if (missionUI != null)
        {
            missionUI.ShowObjectives(currentMissionData.objectives);
        }
    }
    
    public void CompleteMissionObjective(string objectiveId)
    {
        if (currentMissionData == null) return;
        
        MissionObjective objective = currentMissionData.objectives.FirstOrDefault(obj => obj.objectiveId == objectiveId);
        if (objective != null)
        {
            objective.isCompleted = true;
            
            UIManager.Instance?.ShowNotification($"Ціль виконана: {objective.description}", NotificationType.Success);
            
            // Перевірка завершення місії
            if (AreAllObjectivesCompleted())
            {
                CompleteMission();
            }
        }
    }
    
    bool AreAllObjectivesCompleted()
    {
        return currentMissionData.objectives.All(obj => obj.isCompleted || obj.isOptional);
    }
    
    void CompleteMission()
    {
        // Збереження завершення місії
        PlayerPrefs.SetInt($"Mission_{currentMission}_Completed", 1);
        
        // Нагороди
        GiveMissionRewards();
        
        // Post-mission кінематограф
        if (currentMissionData.postMissionCinematic != null)
        {
            PlayCinematic(currentMissionData.postMissionCinematic);
        }
        else
        {
            AdvanceCampaign();
        }
    }
    
    void GiveMissionRewards()
    {
        // Досвід
        ExperienceManager.Instance?.AddExperience(currentMissionData.experienceReward);
        
        // Валюта
        CurrencyManager.Instance?.AddCurrency(currentMissionData.currencyReward);
        
        // Предмети
        foreach (var item in currentMissionData.itemRewards)
        {
            InventoryManager.Instance?.AddItem(item);
        }
        
        // Розблокування контенту
        foreach (var unlock in currentMissionData.unlocks)
        {
            UnlockContent(unlock);
        }
    }
    
    void UnlockContent(string contentId)
    {
        PlayerPrefs.SetInt($"Unlocked_{contentId}", 1);
        UIManager.Instance?.ShowNotification($"Розблоковано: {contentId}", NotificationType.Unlock);
    }
    
    void AdvanceCampaign()
    {
        currentMission++;
        campaignProgress = (float)currentMission / allMissions.Count;
        
        // Перевірка переходу до наступного розділу
        if (ShouldAdvanceChapter())
        {
            AdvanceChapter();
        }
        
        SaveCampaignProgress();
        
        // Показати прогрес кампанії
        ShowCampaignProgress();
    }
    
    bool ShouldAdvanceChapter()
    {
        // Логіка переходу до наступного розділу
        int missionsPerChapter = allMissions.Count / 8; // 8 розділів
        return currentMission % missionsPerChapter == 0;
    }
    
    void AdvanceChapter()
    {
        currentChapter++;
        
        if (currentChapter <= CampaignChapter.Epilogue)
        {
            PlayChapterOutro(currentChapter - 1);
            PlayChapterIntro(currentChapter);
        }
        else
        {
            CompleteCampaign();
        }
    }
    
    void CompleteCampaign()
    {
        UIManager.Instance?.ShowNotification("КАМПАНІЮ ЗАВЕРШЕНО!", NotificationType.Victory);
        
        // Фінальні нагороди
        GiveCampaignCompletionRewards();
        
        // Статистика
        ShowCampaignStatistics();
        
        // Розблокування New Game+
        PlayerPrefs.SetInt("NewGamePlusUnlocked", 1);
    }
    
    void GiveCampaignCompletionRewards()
    {
        // Великі нагороди за завершення кампанії
        ExperienceManager.Instance?.AddExperience(5000);
        CurrencyManager.Instance?.AddCurrency(10000);
        
        // Спеціальні досягнення
        AchievementManager.Instance?.UnlockAchievement("CAMPAIGN_COMPLETE");
        
        if (playerAlignment == MoralityAlignment.Heroic)
        {
            AchievementManager.Instance?.UnlockAchievement("HERO_PATH");
        }
        else if (playerAlignment == MoralityAlignment.Pragmatic)
        {
            AchievementManager.Instance?.UnlockAchievement("PRAGMATIC_PATH");
        }
    }
    
    void ShowCampaignStatistics()
    {
        CampaignStatistics stats = new CampaignStatistics
        {
            totalMissions = allMissions.Count,
            completedMissions = currentMission,
            heroicChoices = heroicPoints,
            pragmaticChoices = pragmaticPoints,
            finalAlignment = playerAlignment
        };
        
        CampaignStatsUI.Instance?.ShowStatistics(stats);
    }
    
    void ShowCampaignProgress()
    {
        CampaignProgressUI progressUI = FindObjectOfType<CampaignProgressUI>();
        if (progressUI != null)
        {
            progressUI.UpdateProgress(campaignProgress, currentChapter, currentMission);
        }
    }
    
    public void PlayCinematic(CinematicSequence cinematic)
    {
        if (cinematic == null || cinematicDirector == null) return;
        
        isInCinematic = true;
        
        // Заморозка гравця
        PlayerController.Instance?.SetMovementEnabled(false);
        
        // Запуск кінематографа
        cinematicDirector.playableAsset = cinematic.timeline;
        cinematicDirector.Play();
        
        // Підписка на завершення
        cinematicDirector.stopped += OnCinematicFinished;
        
        // Запуск музики
        if (cinematic.backgroundMusic != null)
        {
            backgroundMusic.clip = cinematic.backgroundMusic;
            backgroundMusic.Play();
        }
    }
    
    void OnCinematicFinished(PlayableDirector director)
    {
        isInCinematic = false;
        director.stopped -= OnCinematicFinished;
        
        // Розморозка гравця
        PlayerController.Instance?.SetMovementEnabled(true);
        
        // Продовження геймплею
        if (currentMissionData != null)
        {
            StartMissionGameplay();
        }
        else
        {
            AdvanceCampaign();
        }
    }
    
    void PlayChapterIntro(CampaignChapter chapter)
    {
        int chapterIndex = (int)chapter;
        if (chapterIndex < chapterIntros.Length)
        {
            PlayCinematic(chapterIntros[chapterIndex]);
        }
        
        // Зміна музичної теми
        if (chapterIndex < chapterThemes.Length)
        {
            backgroundMusic.clip = chapterThemes[chapterIndex];
            backgroundMusic.Play();
        }
    }
    
    void PlayChapterOutro(CampaignChapter chapter)
    {
        int chapterIndex = (int)chapter;
        if (chapterIndex < chapterOutros.Length)
        {
            PlayCinematic(chapterOutros[chapterIndex]);
        }
    }
    
    public void StartDialogue(DialogueSequence dialogue)
    {
        if (dialogueSystem != null)
        {
            isInDialogue = true;
            dialogueSystem.StartDialogue(dialogue, OnDialogueChoice, OnDialogueFinished);
        }
    }
    
    void OnDialogueChoice(DialogueChoice choice, string choiceId)
    {
        // Збереження вибору гравця
        StoryChoice storyChoice = new StoryChoice
        {
            choiceId = choiceId,
            choice = choice,
            missionIndex = currentMission,
            timestamp = System.DateTime.Now
        };
        
        playerChoices.Add(storyChoice);
        
        // Оновлення морального вирівнювання
        UpdateMoralityPoints(choice);
        UpdatePlayerAlignment();
        
        // Вплив на сюжет
        ApplyChoiceConsequences(storyChoice);
    }
    
    void UpdateMoralityPoints(DialogueChoice choice)
    {
        switch (choice)
        {
            case DialogueChoice.Heroic:
                heroicPoints += 2;
                break;
            case DialogueChoice.Aggressive:
                pragmaticPoints += 2;
                break;
            case DialogueChoice.Diplomatic:
                heroicPoints += 1;
                break;
            case DialogueChoice.Pragmatic:
                pragmaticPoints += 1;
                break;
        }
    }
    
    void UpdatePlayerAlignment()
    {
        if (heroicPoints > pragmaticPoints + 5)
        {
            playerAlignment = MoralityAlignment.Heroic;
        }
        else if (pragmaticPoints > heroicPoints + 5)
        {
            playerAlignment = MoralityAlignment.Pragmatic;
        }
        else
        {
            playerAlignment = MoralityAlignment.Neutral;
        }
    }
    
    void ApplyChoiceConsequences(StoryChoice choice)
    {
        // Логіка впливу вибору на сюжет
        switch (choice.choiceId)
        {
            case "save_civilians":
                if (choice.choice == DialogueChoice.Heroic)
                {
                    // Розблокування додаткової місії порятунку
                    UnlockContent("rescue_mission");
                }
                break;
            case "alliance_decision":
                if (choice.choice == DialogueChoice.Diplomatic)
                {
                    // Зміна доступних союзників
                    ModifyStoryPath("diplomatic_path");
                }
                break;
        }
    }
    
    void ModifyStoryPath(string pathId)
    {
        PlayerPrefs.SetInt($"StoryPath_{pathId}", 1);
        
        // Модифікація доступних місій
        foreach (var mission in allMissions)
        {
            if (mission.storyPath == pathId)
            {
                mission.isUnlocked = true;
            }
        }
    }
    
    void OnDialogueFinished()
    {
        isInDialogue = false;
        
        // Продовження місії після діалогу
        if (currentMissionData != null)
        {
            ContinueMissionAfterDialogue();
        }
    }
    
    void ContinueMissionAfterDialogue()
    {
        // Логіка продовження місії після діалогу
        // Може включати зміну цілей, спавн нових ворогів, тощо
    }
    
    void SavePlayerChoices()
    {
        string choicesJson = JsonUtility.ToJson(new SerializableList<StoryChoice>(playerChoices));
        PlayerPrefs.SetString("PlayerChoices", choicesJson);
    }
    
    void LoadPlayerChoices()
    {
        string choicesJson = PlayerPrefs.GetString("PlayerChoices", "");
        if (!string.IsNullOrEmpty(choicesJson))
        {
            var choicesList = JsonUtility.FromJson<SerializableList<StoryChoice>>(choicesJson);
            playerChoices = choicesList.items;
        }
    }
}

// ================================
// СТРУКТУРИ ДАНИХ
// ================================

[System.Serializable]
public class Mission
{
    public string missionName;
    public string description;
    public MissionType missionType;
    public int levelIndex;
    public bool isUnlocked = false;
    public string storyPath = "";
    
    [Header("Prerequisites")]
    public List<int> prerequisiteMissions = new List<int>();
    public List<string> requiredChoices = new List<string>();
    
    [Header("Objectives")]
    public List<MissionObjective> objectives = new List<MissionObjective>();
    
    [Header("Enemies")]
    public List<EnemyType> enemyTypes = new List<EnemyType>();
    public int enemyCount = 10;
    
    [Header("Cinematics")]
    public CinematicSequence preMissionCinematic;
    public CinematicSequence postMissionCinematic;
    public DialogueSequence missionDialogue;
    
    [Header("Rewards")]
    public int experienceReward = 500;
    public int currencyReward = 200;
    public List<Item> itemRewards = new List<Item>();
    public List<string> unlocks = new List<string>();
    
    [Header("Special")]
    public GameObject escortTarget;
    public Vector3 escortDestination;
}

[System.Serializable]
public class MissionObjective
{
    public string objectiveId;
    public string description;
    public bool isOptional = false;
    public bool isCompleted = false;
    public ObjectiveType type;
    public int targetCount = 1;
    public int currentCount = 0;
}

public enum ObjectiveType
{
    KillEnemies,        // Вбити ворогів
    ReachLocation,      // Дістатися локації
    CollectItems,       // Зібрати предмети
    DefendArea,         // Захистити зону
    EscortNPC,          // Супроводити NPC
    ActivateSwitch,     // Активувати перемикач
    SurviveTime,        // Вижити час
    StealthKill         // Стелс вбивство
}

[System.Serializable]
public class Character
{
    public string characterName;
    public string description;
    public Sprite portrait;
    public GameObject characterModel;
    public AudioClip voiceClip;
    public CharacterRole role;
    public bool isAlive = true;
    public MoralityAlignment alignment;
    
    public void Initialize()
    {
        // Ініціалізація персонажа
    }
}

public enum CharacterRole
{
    Protagonist,        // Головний герой
    Ally,              // Союзник
    Mentor,            // Наставник
    Antagonist,        // Антагоніст
    Neutral,           // Нейтральний
    Informant          // Інформатор
}

public enum MoralityAlignment
{
    Heroic,            // Героїчний
    Neutral,           // Нейтральний
    Pragmatic          // Прагматичний
}

[System.Serializable]
public class StoryChoice
{
    public string choiceId;
    public DialogueChoice choice;
    public int missionIndex;
    public System.DateTime timestamp;
}

[System.Serializable]
public class CinematicSequence
{
    public string sequenceName;
    public PlayableAsset timeline;
    public AudioClip backgroundMusic;
    public bool skipable = true;
    public float duration;
}

[System.Serializable]
public class CampaignStatistics
{
    public int totalMissions;
    public int completedMissions;
    public int heroicChoices;
    public int pragmaticChoices;
    public MoralityAlignment finalAlignment;
    public float completionTime;
}

[System.Serializable]
public class SerializableList<T>
{
    public List<T> items;
    
    public SerializableList(List<T> list)
    {
        items = list;
    }
}

// ================================
// SCRIPTABLE OBJECTS
// ================================

[CreateAssetMenu(fileName = "CampaignData", menuName = "Game/Campaign/Campaign Data")]
public class CampaignData : ScriptableObject
{
    public string campaignName;
    public string description;
    public Mission[] missions;
    public Character[] characters;
    public CinematicSequence[] cinematics;
}