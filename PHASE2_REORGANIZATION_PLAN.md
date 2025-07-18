# 🔧 ФАЗА 2: РЕОРГАНІЗАЦІЯ CORE/ ДИРЕКТОРІЇ

## 🎯 МЕТА: ОЧИСТИТИ CORE/ ВІД НЕПРАВИЛЬНО РОЗМІЩЕНИХ ФАЙЛІВ

**Поточний стан**: 33 файли в Core/ (занадто багато)  
**Цільовий стан**: 8-10 основних файлів в Core/  
**Принцип**: Core/ тільки для фундаментальних систем  

---

## 📋 ПЛАН ПЕРЕМІЩЕННЯ ФАЙЛІВ

### 🎵 **AUDIO СИСТЕМИ** → `Audio/`
Файли для переміщення з Core/:
- `AudioManager.cs` → `Audio/AudioManager.cs`
- `DynamicMusicManager.cs` → `Audio/MusicManager.cs`
- Інші audio-related файли

### 🎨 **UI СИСТЕМИ** → `UI/Systems/`
Файли для переміщення з Core/:
- `UIThemeSystem.cs` → `UI/Systems/UIThemeSystem.cs`
- `ModernUISystem.cs` → `UI/Systems/ModernUISystem.cs`
- `UIFramework.cs` → `UI/Systems/UIFramework.cs`

### ⚙️ **ЗАГАЛЬНІ СИСТЕМИ** → `Systems/`
Файли для переміщення з Core/:
- `InputManager.cs` → `Systems/InputManager.cs`
- `LevelSystem.cs` → `Systems/LevelSystem.cs`
- `PerkSystem.cs` → `Systems/PerkSystem.cs`
- `SaveSystem.cs` → `Systems/SaveSystem.cs`

### 🏊 **POOL СИСТЕМИ** → Об'єднання
Файли для об'єднання:
- `BulletPool.cs` → видалити (інтегрувати в UniversalObjectPool)
- `UniversalObjectPool.cs` → залишити в Core/
- `Utils/ObjectPooler.cs` → видалити (застаріла версія)

### ✅ **ЗАЛИШИТИ В CORE/**
Тільки фундаментальні системи:
- `GameManager.cs` ✅
- `EventSystem.cs` ✅
- `SceneLoader.cs` ✅
- `ConfigurationSystem.cs` ✅
- `UniversalObjectPool.cs` ✅
- `GameConstants.cs` ✅ (якщо глобальні)

---

## 🚀 ВИКОНАННЯ ПЛАНУ

### КРОК 1: Створити недостатні директорії
### КРОК 2: Перемістити Audio файли
### КРОК 3: Перемістити UI файли  
### КРОК 4: Перемістити System файли
### КРОК 5: Об'єднати Pool системи
### КРОК 6: Оновити namespace та using statements
### КРОК 7: Тестувати компіляцію

**РОЗПОЧИНАЄМО...**