using UnityEngine;
using System.Collections.Generic;
using System;

namespace InfimaGames.LowPolyShooterPack
{
    // ========================================
    // ARENA TRIGGER - Key/Pickup
    // ========================================
    
    /// <summary>
    /// Trigger untuk memulai arena (key, button, dll)
    /// </summary>
    public class ArenaTrigger : MonoBehaviour
    {
        #region EVENTS
        
        public event Action OnTriggered;
        
        #endregion
        
        #region SERIALIZED FIELDS
        
        [Header("Trigger Settings")]
        [Tooltip("Trigger hanya sekali?")]
        [SerializeField]
        private bool triggerOnce = true;
        
        [Tooltip("Destroy object setelah triggered?")]
        [SerializeField]
        private bool destroyAfterTrigger = true;
        
        [Tooltip("Delay sebelum destroy")]
        [SerializeField]
        private float destroyDelay = 0.5f;
        
        [Header("Visual")]
        [Tooltip("Object visual (key model, dll)")]
        [SerializeField]
        private GameObject visualObject;
        
        [Tooltip("Particle effect saat dipickup")]
        [SerializeField]
        private GameObject pickupEffect;
        
        [Tooltip("Rotation speed (cosmetic)")]
        [SerializeField]
        private float rotationSpeed = 50f;
        
        [Header("Audio")]
        [SerializeField]
        private AudioClip pickupSound;
        
        #endregion
        
        #region PRIVATE FIELDS
        
        private bool hasTriggered = false;
        
        #endregion
        
        #region UNITY LIFECYCLE
        
        private void Update()
        {
            // Rotate visual
            if (visualObject != null && rotationSpeed > 0)
            {
                visualObject.transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.World);
            }
        }
        
        private void OnTriggerEnter(Collider other)
        {
            // Check if player
            CharacterBehaviour character = other.GetComponent<CharacterBehaviour>();
            if (character == null)
                return;
            
            // Check if already triggered
            if (triggerOnce && hasTriggered)
                return;
            
            // Trigger!
            Trigger();
        }
        
        #endregion
        
        #region PUBLIC METHODS
        
        /// <summary>
        /// Trigger arena start
        /// </summary>
        public void Trigger()
        {
            hasTriggered = true;
            
            Debug.Log($"ArenaTrigger: Triggered by player!");
            
            // Spawn pickup effect
            if (pickupEffect != null)
            {
                Instantiate(pickupEffect, transform.position, Quaternion.identity);
            }
            
            // Play sound
            if (pickupSound != null)
            {
                AudioSource.PlayClipAtPoint(pickupSound, transform.position);
            }
            
            // Invoke event
            OnTriggered?.Invoke();
            
            // Hide visual immediately
            if (visualObject != null)
            {
                visualObject.SetActive(false);
            }
            
            // Destroy object
            if (destroyAfterTrigger)
            {
                Destroy(gameObject, destroyDelay);
            }
        }
        
        #endregion
    }
 }