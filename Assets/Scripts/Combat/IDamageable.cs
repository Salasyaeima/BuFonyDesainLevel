
using UnityEngine;
using System;

namespace InfimaGames.LowPolyShooterPack
{
    // ========================================
    // INTERFACE - Damage System
    // ========================================
    
    /// <summary>
    /// Interface untuk entities yang bisa menerima damage
    /// Implementasi Strategy Pattern
    /// </summary>
    public interface IDamageable
    {
        /// <summary>
        /// Apply damage ke entity ini
        /// </summary>
        /// <param name="damage">Jumlah damage</param>
        /// <param name="damageSource">Siapa yang nge-damage (optional)</param>
        void TakeDamage(float damage, GameObject damageSource = null);
        
        /// <summary>
        /// Check apakah entity masih hidup
        /// </summary>
        bool IsAlive();
        
        /// <summary>
        /// Get current health
        /// </summary>
        float GetCurrentHealth();
        
        /// <summary>
        /// Get max health
        /// </summary>
        float GetMaxHealth();
    }

}