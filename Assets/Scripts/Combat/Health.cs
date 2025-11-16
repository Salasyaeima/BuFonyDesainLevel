
using UnityEngine;
using System;

namespace InfimaGames.LowPolyShooterPack
{
    public class Health : MonoBehaviour, IDamageable
    {
        #region EVENTS
        
        /// <summary>
        /// Event dipanggil saat menerima damage
        /// Parameters: (currentHealth, damage, damageSource)
        /// </summary>
        public event Action<float, float, GameObject> OnDamaged;
        
        /// <summary>
        /// Event dipanggil saat healed
        /// Parameters: (currentHealth, healAmount)
        /// </summary>
        public event Action<float, float> OnHealed;
        
        /// <summary>
        /// Event dipanggil saat mati
        /// Parameters: (killer GameObject)
        /// </summary>
        public event Action<GameObject> OnDeath;
        
        /// <summary>
        /// Event dipanggil saat health berubah (damage atau heal)
        /// Parameters: (currentHealth, maxHealth)
        /// </summary>
        public event Action<float, float> OnHealthChanged;
        
        #endregion
        
        #region SERIALIZED FIELDS
        
        [Header("Health Settings")]
        [Tooltip("Maximum health")]
        [SerializeField]
        private float maxHealth = 100f;
        
        [Tooltip("Start with full health?")]
        [SerializeField]
        private bool startWithFullHealth = true;
        
        [Tooltip("Starting health (if not full)")]
        [SerializeField]
        private float startingHealth = 50f;
        
        [Header("Invincibility")]
        [Tooltip("Invincible? (untuk testing atau god mode)")]
        [SerializeField]
        private bool isInvincible = false;
        
        [Tooltip("Invincibility duration after taking damage (iframe)")]
        [SerializeField]
        private float invincibilityDuration = 0f;
        
        [Header("Regeneration")]
        [Tooltip("Auto regenerate health?")]
        [SerializeField]
        private bool autoRegenerate = false;
        
        [Tooltip("Health regeneration per second")]
        [SerializeField]
        private float regenerationRate = 5f;
        
        [Tooltip("Delay before regeneration starts")]
        [SerializeField]
        private float regenerationDelay = 3f;
        
        [Header("Death Settings")]
        [Tooltip("Destroy GameObject on death?")]
        [SerializeField]
        private bool destroyOnDeath = false;
        
        [Tooltip("Delay before destroying")]
        [SerializeField]
        private float destroyDelay = 2f;
        
        #endregion
        
        #region PRIVATE FIELDS
        
        private float currentHealth;
        private bool isDead = false;
        private float lastDamageTime;
        private float invincibilityTimer;
        
        #endregion
        
        #region PROPERTIES
        
        public bool IsDead => isDead;
        public bool IsInvincible => isInvincible || invincibilityTimer > 0;
        public float HealthPercentage => maxHealth > 0 ? currentHealth / maxHealth : 0;
        
        #endregion
        
        #region UNITY LIFECYCLE
        
        private void Awake()
        {
            // Initialize health
            currentHealth = startWithFullHealth ? maxHealth : startingHealth;
            currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        }
        
        private void Update()
        {
            // Update invincibility timer
            if (invincibilityTimer > 0)
            {
                invincibilityTimer -= Time.deltaTime;
            }
            
            // Auto regeneration
            if (autoRegenerate && !isDead && currentHealth < maxHealth)
            {
                // Check if enough time passed since last damage
                if (Time.time - lastDamageTime >= regenerationDelay)
                {
                    Heal(regenerationRate * Time.deltaTime, false); // silent heal
                }
            }
        }
        
        #endregion
        
        #region IDAMAGEABLE IMPLEMENTATION
        
        public void TakeDamage(float damage, GameObject damageSource = null)
        {
            // Validasi
            if (isDead)
                return;
            
            if (IsInvincible)
            {
                Debug.Log($"{gameObject.name} is invincible!");
                return;
            }
            
            if (damage <= 0)
                return;
            
            // Apply damage
            currentHealth -= damage;
            currentHealth = Mathf.Max(currentHealth, 0);
            
            // Update timers
            lastDamageTime = Time.time;
            invincibilityTimer = invincibilityDuration;
            
            // Trigger events
            OnDamaged?.Invoke(currentHealth, damage, damageSource);
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
            
            Debug.Log($"{gameObject.name} took {damage} damage. HP: {currentHealth}/{maxHealth}");
            
            // Check death
            if (currentHealth <= 0 && !isDead)
            {
                Die(damageSource);
            }
        }
        
        public bool IsAlive()
        {
            return !isDead;
        }
        
        public float GetCurrentHealth()
        {
            return currentHealth;
        }
        
        public float GetMaxHealth()
        {
            return maxHealth;
        }
        
        #endregion
        
        #region PUBLIC METHODS
        
        /// <summary>
        /// Heal entity
        /// </summary>
        public void Heal(float amount, bool triggerEvent = true)
        {
            if (isDead || amount <= 0)
                return;
            
            float oldHealth = currentHealth;
            currentHealth += amount;
            currentHealth = Mathf.Min(currentHealth, maxHealth);
            
            float actualHealed = currentHealth - oldHealth;
            
            if (actualHealed > 0 && triggerEvent)
            {
                OnHealed?.Invoke(currentHealth, actualHealed);
                OnHealthChanged?.Invoke(currentHealth, maxHealth);
                
                Debug.Log($"{gameObject.name} healed {actualHealed}. HP: {currentHealth}/{maxHealth}");
            }
        }
        
        /// <summary>
        /// Set invincibility state
        /// </summary>
        public void SetInvincible(bool invincible)
        {
            isInvincible = invincible;
        }
        
        /// <summary>
        /// Reset health to max
        /// </summary>
        public void ResetHealth()
        {
            currentHealth = maxHealth;
            isDead = false;
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
        }
        
        /// <summary>
        /// Kill instantly
        /// </summary>
        public void Kill(GameObject killer = null)
        {
            if (isDead)
                return;
            
            currentHealth = 0;
            Die(killer);
        }
        
        #endregion
        
        #region PRIVATE METHODS
        
        private void Die(GameObject killer)
        {
            isDead = true;
            
            Debug.Log($"{gameObject.name} died! Killer: {(killer != null ? killer.name : "Unknown")}");
            
            // Trigger death event
            OnDeath?.Invoke(killer);
            
            // Destroy if configured
            if (destroyOnDeath)
            {
                Destroy(gameObject, destroyDelay);
            }
        }
        
        #endregion
    }
    

}