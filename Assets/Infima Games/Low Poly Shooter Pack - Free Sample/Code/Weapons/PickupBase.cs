// Copyright 2021, Infima Games. All Rights Reserved.

using UnityEngine;

namespace InfimaGames.LowPolyShooterPack
{
    /// <summary>
    /// Base class untuk semua pickup items
    /// Menggunakan Trigger-based detection
    /// </summary>
    public abstract class PickupBase : MonoBehaviour
    {
        [Header("Pickup Settings")]
        [Tooltip("Auto destroy setelah dipickup?")]
        [SerializeField]
        protected bool destroyAfterPickup = true;

        [Tooltip("Respawn setelah beberapa detik? (0 = tidak respawn)")]
        [SerializeField]
        protected float respawnTime = 0f;

        [Header("Visual Feedback")]
        [Tooltip("Particle effect saat dipickup")]
        [SerializeField]
        protected GameObject pickupEffect;

        [Tooltip("Rotation speed (cosmetic)")]
        [SerializeField]
        protected float rotationSpeed = 50f;

        [Tooltip("Bob animation (naik-turun)")]
        [SerializeField]
        protected bool enableBobAnimation = true;

        [Tooltip("Bob speed")]
        [SerializeField]
        protected float bobSpeed = 1f;

        [Tooltip("Bob height")]
        [SerializeField]
        protected float bobHeight = 0.3f;

        [Header("Audio")]
        [Tooltip("Sound saat dipickup")]
        [SerializeField]
        protected AudioClip pickupSound;

        [Tooltip("Volume pickup sound")]
        [SerializeField, Range(0f, 1f)]
        protected float soundVolume = 0.7f;

        // Internal state
        protected bool isPickedUp = false;
        private Vector3 startPosition;
        private Renderer[] renderers;
        private Collider triggerCollider;

        #region UNITY LIFECYCLE

        protected virtual void Awake()
        {
            startPosition = transform.position;
            renderers = GetComponentsInChildren<Renderer>();
            triggerCollider = GetComponent<Collider>();

            // Ensure collider is trigger
            if (triggerCollider != null)
                triggerCollider.isTrigger = true;
        }

        protected virtual void Update()
        {
            if (isPickedUp)
                return;

            // Cosmetic rotation
            if (rotationSpeed > 0)
                transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.World);

            // Bob animation
            if (enableBobAnimation)
            {
                float newY = startPosition.y + Mathf.Sin(Time.time * bobSpeed) * bobHeight;
                transform.position = new Vector3(transform.position.x, newY, transform.position.z);
            }
        }

        protected virtual void OnTriggerEnter(Collider other)
        {
            // Sudah dipickup, ignore
            if (isPickedUp)
                return;

            // Cek apakah yang masuk adalah player
            CharacterBehaviour character = other.GetComponent<CharacterBehaviour>();
            if (character == null)
                return;

            // Try pickup
            bool success = TryPickup(character);

            if (success)
            {
                OnPickupSuccess(character);
            }
            else
            {
                OnPickupFailed(character);
            }
        }

        #endregion

        #region ABSTRACT METHODS

        /// <summary>
        /// Override method ini untuk implement logic pickup spesifik
        /// Return true jika pickup berhasil
        /// </summary>
        protected abstract bool TryPickup(CharacterBehaviour character);

        /// <summary>
        /// Override untuk custom message saat pickup failed
        /// </summary>
        protected abstract string GetPickupFailedMessage();

        #endregion

        #region PICKUP HANDLING

        /// <summary>
        /// Called when pickup successful
        /// </summary>
        protected virtual void OnPickupSuccess(CharacterBehaviour character)
        {
            isPickedUp = true;

            // Visual feedback
            PlayPickupEffect();

            // Audio feedback
            PlayPickupSound();

            // Handle destroy or respawn
            if (respawnTime > 0)
            {
                // Respawn mode
                StartRespawn();
            }
            else if (destroyAfterPickup)
            {
                // Destroy mode - LANGSUNG HANCURKAN OBJECT
                Destroy(gameObject);
            }
            else
            {
                // Just disable
                gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// Called when pickup failed (inventory full, etc)
        /// </summary>
        protected virtual void OnPickupFailed(CharacterBehaviour character)
        {
            Debug.Log(GetPickupFailedMessage());
        }

        #endregion

        #region VISUAL & AUDIO

        protected virtual void PlayPickupEffect()
        {
            if (pickupEffect != null)
            {
                GameObject effect = Instantiate(pickupEffect, transform.position, Quaternion.identity);
                Destroy(effect, 2f);
            }
        }

        protected virtual void PlayPickupSound()
        {
            if (pickupSound != null)
            {
                AudioSource.PlayClipAtPoint(pickupSound, transform.position, soundVolume);
            }
        }

        #endregion

        #region RESPAWN

        protected virtual void StartRespawn()
        {
            // Hide visual
            SetRenderersActive(false);

            // Disable collider
            if (triggerCollider != null)
                triggerCollider.enabled = false;

            // Schedule respawn
            Invoke(nameof(Respawn), respawnTime);
        }

        protected virtual void Respawn()
        {
            // Reset state
            isPickedUp = false;

            // Show visual
            SetRenderersActive(true);

            // Enable collider
            if (triggerCollider != null)
                triggerCollider.enabled = true;

            // Reset position
            transform.position = startPosition;

            Debug.Log($"{gameObject.name} respawned!");
        }

        private void SetRenderersActive(bool active)
        {
            foreach (var rend in renderers)
            {
                if (rend != null)
                    rend.enabled = active;
            }
        }

        #endregion
    }
}