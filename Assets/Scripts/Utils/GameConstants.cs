using UnityEngine;

namespace IndieShooter.Utils
{
    public static class GameConstants
    {
        // Player Settings
        public const float DEFAULT_WALK_SPEED = 6f;
        public const float DEFAULT_RUN_SPEED = 12f;
        public const float DEFAULT_JUMP_SPEED = 8f;
        public const float DEFAULT_GRAVITY = 20f;
        public const float DEFAULT_LOOK_SPEED = 2f;
        public const float DEFAULT_LOOK_X_LIMIT = 45f;
        
        // Weapon Settings
        public const float DEFAULT_WEAPON_DAMAGE = 25f;
        public const float DEFAULT_FIRE_RATE = 0.1f;
        public const float DEFAULT_WEAPON_RANGE = 100f;
        public const int DEFAULT_MAX_AMMO = 30;
        public const float DEFAULT_RELOAD_TIME = 2f;
        
        // Enemy Settings
        public const float DEFAULT_ENEMY_HEALTH = 100f;
        public const float DEFAULT_ENEMY_SPEED = 3f;
        public const float DEFAULT_ENEMY_DAMAGE = 10f;
        
        // Game Settings
        public const int DEFAULT_PLAYER_HEALTH = 100;
        public const float DEFAULT_RESPAWN_TIME = 3f;
        
        // Layer Masks
        public const int PLAYER_LAYER = 8;
        public const int ENEMY_LAYER = 9;
        public const int WEAPON_LAYER = 10;
        public const int PICKUP_LAYER = 11;
        public const int GROUND_LAYER = 12;
        public const int WALL_LAYER = 13;
        public const int BULLET_LAYER = 14;
        public const int INTERACTABLE_LAYER = 15;
        
        // Tags
        public const string PLAYER_TAG = "Player";
        public const string ENEMY_TAG = "Enemy";
        public const string WEAPON_TAG = "Weapon";
        public const string PICKUP_TAG = "Pickup";
        public const string GROUND_TAG = "Ground";
        public const string WALL_TAG = "Wall";
        public const string BULLET_TAG = "Bullet";
        public const string INTERACTABLE_TAG = "Interactable";
        
        // Input
        public const string HORIZONTAL_AXIS = "Horizontal";
        public const string VERTICAL_AXIS = "Vertical";
        public const string MOUSE_X_AXIS = "Mouse X";
        public const string MOUSE_Y_AXIS = "Mouse Y";
        public const string FIRE1_BUTTON = "Fire1";
        public const string FIRE2_BUTTON = "Fire2";
        public const string JUMP_BUTTON = "Jump";
        
        // Audio
        public const float DEFAULT_MASTER_VOLUME = 1f;
        public const float DEFAULT_SFX_VOLUME = 0.8f;
        public const float DEFAULT_MUSIC_VOLUME = 0.6f;
        
        // Performance
        public const int DEFAULT_OBJECT_POOL_SIZE = 50;
        public const float DEFAULT_BULLET_LIFETIME = 5f;
        public const int MAX_PARTICLES = 1000;
        public const float LOD_DISTANCE_1 = 50f;
        public const float LOD_DISTANCE_2 = 100f;
        
        // UI
        public const float DEFAULT_FADE_TIME = 0.5f;
        public const float DEFAULT_MENU_TRANSITION_TIME = 0.3f;
    }
}