# 📁 ПЛАН РЕОРГАНІЗАЦІЇ СТРУКТУРИ ПРОЕКТУ

## 🎯 ПОТОЧНИЙ СТАН АНАЛІЗУ

**Дата**: 18 липня 2025  
**Мета**: Організувати файли по логічним директоріям  

---

## 📊 ФАЙЛИ ДЛЯ ПЕРЕМІЩЕННЯ

### 🎮 **GAME MODES** → `Assets/Scripts/GameModes/`
- `SurvivalMode_EndlessWaves.cs` - режим виживання
- `CampaignMode_StoryDriven.cs` - кампанія
- `CooperativeMode_Multiplayer.cs` - кооператив

### 👤 **PLAYER SYSTEMS** → `Assets/Scripts/Player/`
- `PlayerCommands.cs` - команди гравця
- `PlayerHealth.cs` - здоров'я гравця  
- `PlayerMovement.cs` - рух гравця
- `PlayerInteraction.cs` - взаємодія
- `MouseLook.cs` - управління камерою
- `PlayerMovementStates.cs` - стани руху

### 🎯 **MANAGERS** → `Assets/Scripts/Managers/`
- `GameSettings.cs` - налаштування гри
- `GameConstants.cs` - константи
- `GameEvents.cs` - події гри
- `MusicEvents.cs` - музичні події

### 🏗️ **SYSTEMS** → `Assets/Scripts/Systems/`
- `DoorController.cs` - система дверей
- `Checkpoint.cs` - система чекпоінтів
- `Interactable.cs` - система взаємодії
- `StateMachine.cs` - машина станів

### 🔫 **WEAPONS** → `Assets/Scripts/Weapons/`
- `Bullet.cs` - кулі
- `AmmoPickup.cs` - підбір патронів

### 🎨 **EFFECTS** → `Assets/Scripts/Effects/`
- `CameraEffects.cs` - ефекти камери
- `DamageNumber.cs` - числа пошкоджень

### 🧪 **TESTING** → `Assets/Scripts/Testing/`
- `IntegrationTests_NewContent.cs` - інтеграційні тести

### 📊 **DATA** → `Assets/Scripts/Data/`
- `CharacterClasses_Specialization.cs` - класи персонажів
- `SkillTree_Advanced.cs` - дерево навичок
- `PerkIntegration.cs` - система перків

### 🆕 **CONTENT** → `Assets/Scripts/Content/`
- `NewLevels_ExpansionPack.cs` - нові рівні

---

## 🔧 ПЛАН ВИКОНАННЯ

### Фаза 1: Створення директорій ✅
- GameModes/ ✅
- Managers/ ✅  
- Data/ ✅
- Utilities/ ✅
- Content/ (потрібно створити)

### Фаза 2: Переміщення файлів
1. Game Modes файли
2. Player системи
3. Managers та константи
4. Systems та утиліти
5. Content та тести

### Фаза 3: Перевірка консистентності
1. Аналіз namespace відповідності
2. Перевірка using statements
3. Тестування компіляції
4. Оновлення документації

---

## ⚠️ ПОТЕНЦІЙНІ ПРОБЛЕМИ

### 1. **Namespace Inconsistency**
- Файли можуть мати неправильні namespace
- Потрібно оновити після переміщення

### 2. **Using Statements**
- Можливі проблеми з імпортами
- Потрібна перевірка залежностей

### 3. **Unity References**
- Префаби можуть посилатися на старі шляхи
- Потрібна перевірка .meta файлів

---

## 📋 НАСТУПНІ КРОКИ

1. ✅ Створити недостатні директорії
2. 🔄 Перемістити файли по категоріям
3. 🔍 Проаналізувати namespace consistency
4. 🧪 Перевірити компіляцію
5. 📚 Оновити документацію