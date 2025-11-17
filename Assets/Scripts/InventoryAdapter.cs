using UnityEngine;
using System;
using System.Collections.Generic;

namespace InfimaGames.LowPolyShooterPack
{
    // ========================================
    // WEAPON PICKUP - Senjata yang bisa diambil
    // ========================================
    
    /// <summary>
    /// Weapon pickup di map
    /// Bisa pickup untuk add ke inventory
    /// </summary>
    public class InventoryAdapter : MonoBehaviour
    {
        #region SERIALIZED FIELDS
        
        [Header("Settings")]
        [Tooltip("Maximum weapons yang bisa dimiliki")]
        [SerializeField]
        private int maxWeapons = 5;
        
        [Tooltip("Auto re-initialize inventory setelah add weapon?")]
        [SerializeField]
        private bool autoReinitialize = true;
        
        [Header("Debug")]
        [SerializeField]
        private bool showDebugLogs = true;
        
        #endregion
        
        #region PRIVATE FIELDS
        
        private InventoryBehaviour inventory;
        
        #endregion
        
        #region UNITY LIFECYCLE
        
        private void Awake()
        {
            inventory = GetComponent<InventoryBehaviour>();
            
            if (inventory == null)
            {
                Debug.LogError("InventoryAdapter: No InventoryBehaviour found!");
            }
        }
        
        #endregion
        
        #region PUBLIC METHODS
        
        /// <summary>
        /// Add weapon to inventory
        /// Returns true jika berhasil, false jika inventory penuh
        /// </summary>
        public bool TryAddWeapon(GameObject weaponPrefab, out int weaponIndex)
        {
            weaponIndex = -1;
            
            if (inventory == null)
            {
                Debug.LogError("InventoryAdapter: No inventory found!");
                return false;
            }
            
            // Check if inventory full
            int currentWeaponCount = GetCurrentWeaponCount();
            if (currentWeaponCount >= maxWeapons)
            {
                if (showDebugLogs)
                    Debug.Log($"InventoryAdapter: Inventory is full! ({currentWeaponCount}/{maxWeapons})");
                return false;
            }
            
            try
            {
                // Instantiate weapon
                GameObject weaponObject = Instantiate(weaponPrefab);
                
                // CRITICAL: Parent weapon ke inventory GameObject
                // Ini penting karena Inventory.Init() menggunakan GetComponentsInChildren
                weaponObject.transform.SetParent(transform);
                
                // Reset local transform
                weaponObject.transform.localPosition = Vector3.zero;
                weaponObject.transform.localRotation = Quaternion.identity;
                weaponObject.transform.localScale = Vector3.one;
                
                // Disable weapon (inventory akan handle activation)
                weaponObject.SetActive(false);
                
                // Re-initialize inventory untuk refresh weapons array
                if (autoReinitialize)
                {
                    // Get currently equipped index sebelum re-init
                    int previousEquippedIndex = inventory.GetEquippedIndex();
                    
                    // Re-init inventory
                    inventory.Init(previousEquippedIndex);
                    
                    // Get new weapon index (last weapon in array)
                    weaponIndex = GetCurrentWeaponCount() - 1;
                }
                else
                {
                    weaponIndex = currentWeaponCount;
                }
                
                if (showDebugLogs)
                    Debug.Log($"InventoryAdapter: Successfully added weapon '{weaponObject.name}' at index {weaponIndex}");
                
                return true;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"InventoryAdapter: Failed to add weapon: {e.Message}\n{e.StackTrace}");
                return false;
            }
        }
        
        /// <summary>
        /// Remove weapon dari inventory
        /// </summary>
        public bool RemoveWeapon(int index)
        {
            if (inventory == null)
                return false;
            
            try
            {
                WeaponBehaviour weapon = inventory.GetAtIndex(index);
                if (weapon != null)
                {
                    // Destroy weapon GameObject
                    Destroy(weapon.gameObject);
                    
                    // Re-initialize inventory
                    if (autoReinitialize)
                    {
                        // Equip previous weapon atau weapon pertama
                        int newEquippedIndex = Mathf.Max(0, index - 1);
                        inventory.Init(newEquippedIndex);
                    }
                    
                    if (showDebugLogs)
                        Debug.Log($"InventoryAdapter: Removed weapon at index {index}");
                    
                    return true;
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"InventoryAdapter: Failed to remove weapon: {e.Message}");
            }
            
            return false;
        }
        
        /// <summary>
        /// Check if can pickup weapon
        /// </summary>
        public bool CanPickupWeapon()
        {
            return GetCurrentWeaponCount() < maxWeapons;
        }
        
        /// <summary>
        /// Get current weapon count
        /// </summary>
        public int GetCurrentWeaponCount()
        {
            if (inventory == null)
                return 0;
            
            int count = 0;
            
            // Count semua children yang punya WeaponBehaviour
            foreach (Transform child in transform)
            {
                if (child.GetComponent<WeaponBehaviour>() != null)
                {
                    count++;
                }
            }
            
            return count;
        }
        
        /// <summary>
        /// Get max weapon slots
        /// </summary>
        public int GetMaxWeaponSlots()
        {
            return maxWeapons;
        }
        
        #endregion
    }
}