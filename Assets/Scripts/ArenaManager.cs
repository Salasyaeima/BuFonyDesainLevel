using UnityEngine;
using System.Collections.Generic;
using System;

namespace InfimaGames.LowPolyShooterPack
{
    // ========================================
    // ARENA MANAGER - Core System
    // ========================================
    
    /// <summary>
    /// Manager untuk combat arena / boss fight room
    /// Menggunakan Observer Pattern untuk koordinasi
    /// </summary>
    public class ArenaManager : MonoBehaviour
    {
        #region EVENTS
        
        /// <summary>
        /// Event dipanggil saat arena combat dimulai
        /// </summary>
        public event Action OnArenaStarted;
        
        /// <summary>
        /// Event dipanggil saat semua enemy mati
        /// </summary>
        public event Action OnArenaCompleted;
        
        /// <summary>
        /// Event dipanggil saat ada enemy mati
        /// Parameters: (remainingEnemies)
        /// </summary>
        public event Action<int> OnEnemyDefeated;
        
        #endregion
        
        #region SERIALIZED FIELDS
        
        [Header("Arena Setup")]
        [Tooltip("Key/Trigger object yang memulai arena")]
        [SerializeField]
        private ArenaTrigger arenaTrigger;
        
        [Tooltip("Semua enemy di arena ini")]
        [SerializeField]
        private GameObject[] enemies;
        
        [Tooltip("Semua pintu yang akan dikunci")]
        [SerializeField]
        private ArenaDoor[] doors;
        
        [Header("Settings")]
        [Tooltip("Auto-find enemies dengan tag?")]
        [SerializeField]
        private bool autoFindEnemies = true;
        
        [Tooltip("Tag untuk enemy (jika auto-find enabled)")]
        [SerializeField]
        private string enemyTag = "Enemy";
        
        [Tooltip("Auto-find doors dengan tag?")]
        [SerializeField]
        private bool autoFindDoors = true;
        
        [Tooltip("Tag untuk doors (jika auto-find enabled)")]
        [SerializeField]
        private string doorTag = "ArenaDoor";
        
        [Header("Audio")]
        [SerializeField]
        private AudioClip arenaStartSound;
        
        [SerializeField]
        private AudioClip arenaCompleteSound;
        
        [Header("Debug")]
        [SerializeField]
        private bool showDebugLogs = true;
        
        #endregion
        
        #region PRIVATE FIELDS
        
        private List<GameObject> activeEnemies = new List<GameObject>();
        private bool arenaActive = false;
        private bool arenaCompleted = false;
        
        #endregion
        
        #region PROPERTIES
        
        public bool IsArenaActive => arenaActive;
        public bool IsArenaCompleted => arenaCompleted;
        public int RemainingEnemies => activeEnemies.Count;
        
        #endregion
        
        #region UNITY LIFECYCLE
        
        private void Awake()
        {
            // Auto-find enemies jika enabled
            if (autoFindEnemies)
            {
                GameObject[] foundEnemies = GameObject.FindGameObjectsWithTag(enemyTag);
                if (foundEnemies.Length > 0)
                {
                    enemies = foundEnemies;
                    if (showDebugLogs)
                        Debug.Log($"ArenaManager: Auto-found {enemies.Length} enemies");
                }
            }
            
            // Auto-find doors jika enabled
            if (autoFindDoors)
            {
                ArenaDoor[] foundDoors = FindObjectsByType<ArenaDoor>(FindObjectsSortMode.None);
                if (foundDoors.Length > 0)
                {
                    doors = foundDoors;
                    if (showDebugLogs)
                        Debug.Log($"ArenaManager: Auto-found {doors.Length} doors");
                }
            }
            
            // Subscribe to trigger
            if (arenaTrigger != null)
            {
                arenaTrigger.OnTriggered += StartArena;
            }
        }
        
        private void Start()
        {
            // Disable semua enemy di awal
            foreach (GameObject enemy in enemies)
            {
                if (enemy != null)
                {
                    enemy.SetActive(false);
                }
            }
            
            // Ensure doors are open di awal
            foreach (ArenaDoor door in doors)
            {
                if (door != null)
                {
                    door.SetLocked(false);
                }
            }
        }
        
        private void OnDestroy()
        {
            // Unsubscribe
            if (arenaTrigger != null)
            {
                arenaTrigger.OnTriggered -= StartArena;
            }
        }
        
        #endregion
        
        #region PUBLIC METHODS
        
        /// <summary>
        /// Start arena combat (dipanggil dari trigger)
        /// </summary>
        public void StartArena()
        {
            if (arenaActive || arenaCompleted)
            {
                if (showDebugLogs)
                    Debug.Log("ArenaManager: Arena already active or completed!");
                return;
            }
            
            arenaActive = true;
            
            if (showDebugLogs)
                Debug.Log("ArenaManager: Arena Started!");
            
            // Activate semua enemy
            ActivateEnemies();
            
            // Lock semua pintu
            LockDoors();
            
            // Play sound
            PlaySound(arenaStartSound);
            
            // Trigger event
            OnArenaStarted?.Invoke();
        }
        
        /// <summary>
        /// Notify manager bahwa ada enemy yang mati
        /// Dipanggil dari Health.OnDeath
        /// </summary>
        public void NotifyEnemyDefeated(GameObject enemy)
        {
            if (!arenaActive || arenaCompleted)
                return;
            
            // Remove dari list
            if (activeEnemies.Contains(enemy))
            {
                activeEnemies.Remove(enemy);
                
                if (showDebugLogs)
                    Debug.Log($"ArenaManager: Enemy defeated! Remaining: {activeEnemies.Count}");
                
                // Trigger event
                OnEnemyDefeated?.Invoke(activeEnemies.Count);
                
                // Check jika semua enemy sudah mati
                if (activeEnemies.Count == 0)
                {
                    CompleteArena();
                }
            }
        }
        
        /// <summary>
        /// Force complete arena (untuk testing)
        /// </summary>
        public void ForceCompleteArena()
        {
            CompleteArena();
        }
        
        #endregion
        
        #region PRIVATE METHODS
        
        private void ActivateEnemies()
        {
            activeEnemies.Clear();
            
            foreach (GameObject enemy in enemies)
            {
                if (enemy != null)
                {
                    // Activate enemy
                    enemy.SetActive(true);
                    activeEnemies.Add(enemy);
                    
                    // Subscribe to enemy death
                    Health enemyHealth = enemy.GetComponent<Health>();
                    if (enemyHealth != null)
                    {
                        // Unsubscribe dulu jika sudah subscribe (avoid duplicate)
                        enemyHealth.OnDeath -= OnEnemyDeath;
                        enemyHealth.OnDeath += OnEnemyDeath;
                    }
                    else
                    {
                        Debug.LogWarning($"ArenaManager: Enemy {enemy.name} doesn't have Health component!");
                    }
                }
            }
            
            if (showDebugLogs)
                Debug.Log($"ArenaManager: Activated {activeEnemies.Count} enemies");
        }
        
        private void LockDoors()
        {
            foreach (ArenaDoor door in doors)
            {
                if (door != null)
                {
                    door.SetLocked(true);
                }
            }
            
            if (showDebugLogs)
                Debug.Log($"ArenaManager: Locked {doors.Length} doors");
        }
        
        private void UnlockDoors()
        {
            foreach (ArenaDoor door in doors)
            {
                if (door != null)
                {
                    door.SetLocked(false);
                }
            }
            
            if (showDebugLogs)
                Debug.Log($"ArenaManager: Unlocked {doors.Length} doors");
        }
        
        private void CompleteArena()
        {
            if (arenaCompleted)
                return;
            
            arenaCompleted = true;
            arenaActive = false;
            
            if (showDebugLogs)
                Debug.Log("ArenaManager: Arena Completed!");
            
            // Unlock semua pintu
            UnlockDoors();
            
            // Play sound
            PlaySound(arenaCompleteSound);
            
            // Trigger event
            OnArenaCompleted?.Invoke();
        }
        
        private void OnEnemyDeath(GameObject killer)
        {
            // Find which enemy died
            GameObject deadEnemy = null;
            foreach (GameObject enemy in activeEnemies)
            {
                if (enemy != null)
                {
                    Health health = enemy.GetComponent<Health>();
                    if (health != null && health.IsDead)
                    {
                        deadEnemy = enemy;
                        break;
                    }
                }
            }
            
            if (deadEnemy != null)
            {
                NotifyEnemyDefeated(deadEnemy);
            }
        }
        
        private void PlaySound(AudioClip clip)
        {
            if (clip != null)
            {
                AudioSource.PlayClipAtPoint(clip, transform.position);
            }
        }
        
        #endregion
    }
}