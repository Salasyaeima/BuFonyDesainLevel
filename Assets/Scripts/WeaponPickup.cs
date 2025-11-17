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
    public class WeaponPickup : MonoBehaviour
    {
        #region SERIALIZED FIELDS
        
        [Header("Weapon Settings")]
        [Tooltip("Weapon prefab yang akan diberikan ke player")]
        [SerializeField]
        private GameObject weaponPrefab;
        
        [Tooltip("Nama weapon untuk display")]
        [SerializeField]
        private string weaponName = "Rifle";
        
        [Tooltip("Ammo yang included saat pickup")]
        [SerializeField]
        private int includedAmmo = 60;
        
        [Tooltip("Ammo type untuk weapon ini")]
        [SerializeField]
        private AmmunitionType ammunitionType = AmmunitionType.Rifle;
        
        [Header("Pickup Settings")]
        [Tooltip("Auto equip setelah pickup?")]
        [SerializeField]
        private bool autoEquipOnPickup = true;
        
        [Tooltip("Destroy pickup setelah diambil?")]
        [SerializeField]
        private bool destroyAfterPickup = true;
        
        [Tooltip("Respawn time (0 = tidak respawn)")]
        [SerializeField]
        private float respawnTime = 0f;
        
        [Header("Visual")]
        [Tooltip("Weapon model untuk display di map")]
        [SerializeField]
        private GameObject weaponModel;
        
        [Tooltip("Rotation speed (cosmetic)")]
        [SerializeField]
        private float rotationSpeed = 50f;
        
        [Tooltip("Bob animation")]
        [SerializeField]
        private bool enableBobAnimation = true;
        
        [SerializeField]
        private float bobSpeed = 1f;
        
        [SerializeField]
        private float bobHeight = 0.3f;
        
        [Tooltip("Pickup effect")]
        [SerializeField]
        private GameObject pickupEffect;
        
        [Header("Audio")]
        [SerializeField]
        private AudioClip pickupSound;
        
        [Header("UI")]
        [Tooltip("Show pickup prompt UI?")]
        [SerializeField]
        private bool showPickupPrompt = true;
        
        [Tooltip("Pickup prompt text")]
        [SerializeField]
        private string pickupPromptText = "Press E to pickup {weaponName}";
        
        #endregion
        
        #region PRIVATE FIELDS
        
        private bool isPickedUp = false;
        private Vector3 startPosition;
        private Renderer[] renderers;
        private Collider pickupCollider;
        private CharacterBehaviour nearbyPlayer;
        
        #endregion
        
        #region UNITY LIFECYCLE
        
        private void Awake()
        {
            startPosition = transform.position;
            renderers = GetComponentsInChildren<Renderer>();
            pickupCollider = GetComponent<Collider>();
            
            if (pickupCollider != null)
                pickupCollider.isTrigger = true;
        }
        
        private void Update()
        {
            if (isPickedUp)
                return;
            
            // Cosmetic rotation
            if (weaponModel != null && rotationSpeed > 0)
            {
                weaponModel.transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.World);
            }
            
            // Bob animation
            if (enableBobAnimation)
            {
                float newY = startPosition.y + Mathf.Sin(Time.time * bobSpeed) * bobHeight;
                transform.position = new Vector3(transform.position.x, newY, transform.position.z);
            }
            
            // Check for pickup input
            if (nearbyPlayer != null && Input.GetKeyDown(KeyCode.E))
            {
                TryPickup(nearbyPlayer);
            }
        }
        
        private void OnTriggerEnter(Collider other)
        {
            if (isPickedUp)
                return;
            
            CharacterBehaviour character = other.GetComponent<CharacterBehaviour>();
            if (character == null)
                return;
            
            nearbyPlayer = character;
            
            // Show pickup prompt
            if (showPickupPrompt)
            {
                ShowPickupPrompt(true);
            }
        }
        
        private void OnTriggerExit(Collider other)
        {
            CharacterBehaviour character = other.GetComponent<CharacterBehaviour>();
            if (character == null)
                return;
            
            if (nearbyPlayer == character)
            {
                nearbyPlayer = null;
                
                // Hide pickup prompt
                if (showPickupPrompt)
                {
                    ShowPickupPrompt(false);
                }
            }
        }
        
        #endregion
        
        #region PUBLIC METHODS
        
        public void TryPickup(CharacterBehaviour character)
        {
            if (isPickedUp || weaponPrefab == null)
                return;
            
            // Get inventory
            InventoryBehaviour inventory = character.GetInventory();
            if (inventory == null)
            {
                Debug.LogError("WeaponPickup: Player doesn't have InventoryBehaviour!");
                return;
            }
            
            // Check if inventory full
            if (inventory.IsFull())
            {
                Debug.Log("WeaponPickup: Inventory is full!");
                // Bisa tampilkan UI message: "Inventory Full!"
                return;
            }
            
            // Add weapon to inventory
            bool success = AddWeaponToInventory(character, inventory);
            
            if (success)
            {
                OnPickupSuccess(character);
            }
        }
        
        #endregion
        
        #region PRIVATE METHODS
        
        private bool AddWeaponToInventory(CharacterBehaviour character, InventoryBehaviour inventory)
        {
            // Instantiate weapon prefab
            GameObject weaponObj = Instantiate(weaponPrefab);
            WeaponBehaviour weapon = weaponObj.GetComponent<WeaponBehaviour>();
            
            if (weapon == null)
            {
                Debug.LogError("WeaponPickup: Weapon prefab doesn't have WeaponBehaviour!");
                Destroy(weaponObj);
                return false;
            }
            
            // Add ammo to inventory
            AmmunitionInventory ammoInventory = character.GetAmmunitionInventory();
            if (ammoInventory != null && includedAmmo > 0)
            {
                ammoInventory.AddAmmo(ammunitionType, includedAmmo);
                Debug.Log($"WeaponPickup: Added {includedAmmo} {ammunitionType} ammo");
            }
            
            // Add weapon to inventory via extension method
            int weaponIndex = inventory.Add(weaponObj);
            
            if (weaponIndex >= 0)
            {
                Debug.Log($"WeaponPickup: Added {weaponName} to inventory at index {weaponIndex}");
                
                // Auto equip jika enabled
                if (autoEquipOnPickup)
                {
                    // Small delay untuk ensure inventory sudah ready
                    StartCoroutine(EquipWeaponDelayed(inventory, weaponIndex));
                }
                
                return true;
            }
            else
            {
                Debug.LogError("WeaponPickup: Failed to add weapon to inventory!");
                Destroy(weaponObj);
                return false;
            }
        }
        
        /// <summary>
        /// Equip weapon dengan delay kecil untuk ensure inventory ready
        /// </summary>
        private System.Collections.IEnumerator EquipWeaponDelayed(InventoryBehaviour inventory, int index)
        {
            // Wait 1 frame
            yield return null;
            
            // Equip weapon
            inventory.Equip(index);
            
            Debug.Log($"WeaponPickup: Equipped weapon at index {index}");
        }
        
        private void OnPickupSuccess(CharacterBehaviour character)
        {
            isPickedUp = true;
            
            // Hide pickup prompt
            if (showPickupPrompt)
            {
                ShowPickupPrompt(false);
            }
            
            // Visual feedback
            if (pickupEffect != null)
            {
                Instantiate(pickupEffect, transform.position, Quaternion.identity);
            }
            
            // Audio feedback
            if (pickupSound != null)
            {
                AudioSource.PlayClipAtPoint(pickupSound, transform.position);
            }
            
            // Show UI message
            // UIManager.ShowMessage($"Picked up {weaponName}");
            
            // Handle respawn or destroy
            if (respawnTime > 0)
            {
                StartRespawn();
            }
            else if (destroyAfterPickup)
            {
                Destroy(gameObject);
            }
            else
            {
                gameObject.SetActive(false);
            }
        }
        
        private void StartRespawn()
        {
            // Hide visual
            SetRenderersActive(false);
            
            // Disable collider
            if (pickupCollider != null)
                pickupCollider.enabled = false;
            
            // Schedule respawn
            Invoke(nameof(Respawn), respawnTime);
        }
        
        private void Respawn()
        {
            isPickedUp = false;
            
            // Show visual
            SetRenderersActive(true);
            
            // Enable collider
            if (pickupCollider != null)
                pickupCollider.enabled = true;
            
            // Reset position
            transform.position = startPosition;
            
            Debug.Log($"WeaponPickup: {weaponName} respawned!");
        }
        
        private void SetRenderersActive(bool active)
        {
            foreach (var rend in renderers)
            {
                if (rend != null)
                    rend.enabled = active;
            }
        }
        
        private void ShowPickupPrompt(bool show)
        {
            if (!show)
            {
                // Hide UI prompt
                // UIManager.HidePickupPrompt();
                return;
            }
            
            // Show UI prompt
            string promptText = pickupPromptText.Replace("{weaponName}", weaponName);
            // UIManager.ShowPickupPrompt(promptText);
            
            Debug.Log($"Pickup Prompt: {promptText}");
        }
        
        #endregion
        
        #region DEBUG
        
        private void OnDrawGizmosSelected()
        {
            // Draw pickup radius
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, 2f);
        }
        
        #endregion
    }
}