# SSH налаштування для GitHub

## Перевірка існуючих SSH ключів

```bash
# Перевірка наявності SSH ключів
ls -la ~/.ssh/

# Перевірка підключення до GitHub
ssh -T git@github.com
```

## Якщо потрібно налаштувати SSH ключ

```bash
# Додавання ключа до SSH агента
ssh-add ~/.ssh/id_github_unity_shooter

# Перевірка доданих ключів
ssh-add -l

# Тестування підключення з конкретним ключем
ssh -T git@github.com -i ~/.ssh/id_github_unity_shooter
```

## Налаштування Git для використання SSH

```bash
# Перехід до директорії проекту
cd /Users/george/Unity Indie Shooter/Shooter

# Налаштування віддаленого репозиторію через SSH
git remote set-url origin git@github.com:Georgekgk2/unity-indie-shooter-v2.git

# Завантаження
git push -u origin main
```

## Створення нового SSH ключа (якщо потрібно)

```bash
# Генерація нового SSH ключа
ssh-keygen -t ed25519 -C "george@example.com" -f ~/.ssh/id_github_unity_shooter

# Додавання ключа до SSH агента
eval "$(ssh-agent -s)"
ssh-add ~/.ssh/id_github_unity_shooter

# Копіювання публічного ключа для додавання на GitHub
cat ~/.ssh/id_github_unity_shooter.pub | pbcopy
```

## Додавання SSH ключа на GitHub

1. Перейдіть на https://github.com/settings/keys
2. Натисніть "New SSH key"
3. Вставте скопійований публічний ключ
4. Дайте ключу описову назву (наприклад, "Unity Shooter Development")
5. Натисніть "Add SSH key"

## Налаштування SSH конфігурації (опціонально)

Створіть або відредагуйте файл `~/.ssh/config`:

```bash
# Відкрити файл конфігурації
nano ~/.ssh/config
```

Додайте наступний вміст:

```
Host github.com
    HostName github.com
    User git
    IdentityFile ~/.ssh/id_github_unity_shooter
    IdentitiesOnly yes
```

## Тестування налаштування

```bash
# Тестування SSH підключення
ssh -T git@github.com

# Якщо все налаштовано правильно, ви побачите повідомлення:
# Hi username! You've successfully authenticated, but GitHub does not provide shell access.
```

## Вирішення проблем

### Проблема: Permission denied (publickey)

```bash
# Перевірте, чи додано ключ до SSH агента
ssh-add -l

# Якщо ключ не додано, додайте його
ssh-add ~/.ssh/id_github_unity_shooter

# Перевірте права доступу до файлів
chmod 600 ~/.ssh/id_github_unity_shooter
chmod 644 ~/.ssh/id_github_unity_shooter.pub
```

### Проблема: Host key verification failed

```bash
# Видаліть старий ключ хоста
ssh-keygen -R github.com

# Додайте новий ключ хоста
ssh-keyscan github.com >> ~/.ssh/known_hosts
```

### Проблема: Git використовує HTTPS замість SSH

```bash
# Перевірте поточний URL віддаленого репозиторію
git remote -v

# Змініть на SSH URL
git remote set-url origin git@github.com:Georgekgk2/unity-indie-shooter-v2.git
```

## Корисні команди

```bash
# Перевірка поточних віддалених репозиторіїв
git remote -v

# Перевірка статусу Git
git status

# Перевірка історії комітів
git log --oneline -10

# Перевірка гілок
git branch -a
```

## Автоматизація

Для автоматизації процесу використовуйте створені скрипти:

- `deploy_with_ssh.sh` - для завантаження через SSH
- `deploy_to_github_https.sh` - для завантаження через HTTPS

```bash
# Виконання SSH скрипта
./deploy_with_ssh.sh

# Або HTTPS скрипта
./deploy_to_github_https.sh
```