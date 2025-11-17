using UnityEngine;
using System;
using System.Collections.Generic;

namespace InfimaGames.LowPolyShooterPack
{
    public static class InventoryExtensions
    {
        /// <summary>
        /// Check if inventory is full
        /// </summary>
        public static bool IsFull(this InventoryBehaviour inventory)
        {
            // Get InventoryAdapter
            InventoryAdapter adapter = inventory.GetComponent<InventoryAdapter>();
            if (adapter != null)
            {
                return !adapter.CanPickupWeapon();
            }
            
            // Fallback: count children dengan WeaponBehaviour
            int count = 0;
            foreach (Transform child in inventory.transform)
            {
                if (child.GetComponent<WeaponBehaviour>() != null)
                {
                    count++;
                }
            }
            
            return count >= 5; // Default max
        }
        
        /// <summary>
        /// Add weapon to inventory
        /// Returns: weapon index jika berhasil, -1 jika gagal
        /// </summary>
        public static int Add(this InventoryBehaviour inventory, GameObject weaponPrefab)
        {
            InventoryAdapter adapter = inventory.GetComponent<InventoryAdapter>();
            if (adapter != null)
            {
                if (adapter.TryAddWeapon(weaponPrefab, out int index))
                {
                    return index;
                }
                return -1;
            }
            else
            {
                Debug.LogError("InventoryExtensions: No InventoryAdapter found! Add InventoryAdapter component to Inventory GameObject.");
                return -1;
            }
        }
        
        /// <summary>
        /// Get last added weapon index
        /// </summary>
        public static int GetLastAddedIndex(this InventoryBehaviour inventory)
        {
            int count = 0;
            foreach (Transform child in inventory.transform)
            {
                if (child.GetComponent<WeaponBehaviour>() != null)
                {
                    count++;
                }
            }
            return count - 1; // Return last index
        }
        
        /// <summary>
        /// Get weapon at index
        /// Wrapper untuk compatibility
        /// </summary>
        public static WeaponBehaviour GetAtIndex(this InventoryBehaviour inventory, int index)
        {
            try
            {
                // Ambil semua weapon children
                WeaponBehaviour[] weapons = inventory.GetComponentsInChildren<WeaponBehaviour>(true);
                
                if (index >= 0 && index < weapons.Length)
                {
                    return weapons[index];
                }
            }
            catch
            {
                // Ignored
            }
            
            return null;
        }
    }
}