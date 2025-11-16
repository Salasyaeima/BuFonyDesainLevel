
using UnityEngine;
using System;

namespace InfimaGames.LowPolyShooterPack
{

    public class Hitbox : MonoBehaviour
    {
        [Header("Hitbox Settings")]
        [Tooltip("Damage multiplier untuk hitbox ini")]
        [SerializeField]
        private float damageMultiplier = 1f;
        
        [Tooltip("Apakah ini headshot zone?")]
        [SerializeField]
        private bool isHeadshot = false;
        
        [Tooltip("Reference ke Health component")]
        [SerializeField]
        private Health health;
        
        private void Awake()
        {
            // Auto-find health component jika tidak diset
            if (health == null)
            {
                health = GetComponentInParent<Health>();
            }
        }
        
        /// <summary>
        /// Apply damage dengan multiplier
        /// </summary>
        public void ApplyDamage(float baseDamage, GameObject damageSource, 
            Vector3 hitPoint, Vector3 hitNormal)
        {
            if (health == null)
            {
                Debug.LogError($"Hitbox {gameObject.name} tidak punya Health component!");
                return;
            }
            
            // Calculate final damage dengan multiplier
            float finalDamage = baseDamage * damageMultiplier;
            
            // Apply damage ke health component
            health.TakeDamage(finalDamage, damageSource);
            
            // Log headshot
            if (isHeadshot)
            {
                Debug.Log($"HEADSHOT! Damage: {finalDamage}");
            }
        }
        
        public float GetDamageMultiplier() => damageMultiplier;
        public bool IsHeadshotZone() => isHeadshot;
        public Health GetHealth() => health;
    }

}