using UnityEngine;
using System;

namespace InfimaGames.LowPolyShooterPack
{
    /// <summary>
    /// Enum untuk tipe amunisi - memudahkan management berbagai jenis ammo
    /// </summary>
    public enum AmmunitionType
    {
        Pistol,
        Rifle,
        Shotgun,
        Sniper
    }

    /// <summary>
    /// Component untuk manage ammunition inventory player
    /// Mengikuti Single Responsibility Principle
    /// </summary>
    public class AmmunitionInventory : MonoBehaviour
    {
        [System.Serializable]
        public class AmmoData
        {
            public AmmunitionType type;
            [Tooltip("Ammo cadangan yang dibawa player")]
            public int reserveAmount;
            [Tooltip("Maximum ammo yang bisa dibawa")]
            public int maxReserveAmount = 120;
        }

        [Header("Ammunition Reserves")]
        [SerializeField] 
        private AmmoData[] ammunitionReserves;

        // Event untuk UI update
        public event Action<AmmunitionType, int> OnAmmoChanged;

        private void Awake()
        {
            // Initialize dictionary untuk fast lookup
            InitializeAmmunition();
        }

        private void InitializeAmmunition()
        {
            // Bisa tambahkan validation di sini
            foreach (var ammo in ammunitionReserves)
            {
                ammo.reserveAmount = Mathf.Clamp(ammo.reserveAmount, 0, ammo.maxReserveAmount);
            }
        }

        /// <summary>
        /// Cek apakah player punya ammo cadangan untuk reload
        /// </summary>
        public bool HasReserveAmmo(AmmunitionType type)
        {
            AmmoData data = GetAmmoData(type);
            return data != null && data.reserveAmount > 0;
        }

        /// <summary>
        /// Get jumlah ammo cadangan
        /// </summary>
        public int GetReserveAmount(AmmunitionType type)
        {
            AmmoData data = GetAmmoData(type);
            return data?.reserveAmount ?? 0;
        }

        /// <summary>
        /// Ambil ammo dari cadangan untuk reload
        /// Returns: jumlah ammo yang berhasil diambil
        /// </summary>
        public int ConsumeAmmo(AmmunitionType type, int amount)
        {
            AmmoData data = GetAmmoData(type);
            if (data == null) return 0;

            // Hitung berapa yang bisa diambil
            int ammoToConsume = Mathf.Min(amount, data.reserveAmount);
            data.reserveAmount -= ammoToConsume;

            // Trigger event untuk UI update
            OnAmmoChanged?.Invoke(type, data.reserveAmount);

            return ammoToConsume;
        }

        /// <summary>
        /// Tambah ammo ke inventory (pickup ammo box, dll)
        /// </summary>
        public void AddAmmo(AmmunitionType type, int amount)
        {
            AmmoData data = GetAmmoData(type);
            if (data == null) return;

            int previousAmount = data.reserveAmount;
            data.reserveAmount = Mathf.Clamp(
                data.reserveAmount + amount, 
                0, 
                data.maxReserveAmount
            );

            // Only trigger event if amount actually changed
            if (previousAmount != data.reserveAmount)
            {
                OnAmmoChanged?.Invoke(type, data.reserveAmount);
            }
        }

        /// <summary>
        /// Helper method untuk get ammo data
        /// </summary>
        private AmmoData GetAmmoData(AmmunitionType type)
        {
            foreach (var ammo in ammunitionReserves)
            {
                if (ammo.type == type)
                    return ammo;
            }
            
            Debug.LogWarning($"Ammunition type {type} not found in inventory!");
            return null;
        }

        /// <summary>
        /// Debug helper
        /// </summary>
        public void DebugLogAmmo()
        {
            foreach (var ammo in ammunitionReserves)
            {
                Debug.Log($"{ammo.type}: {ammo.reserveAmount}/{ammo.maxReserveAmount}");
            }
        }
    }
}