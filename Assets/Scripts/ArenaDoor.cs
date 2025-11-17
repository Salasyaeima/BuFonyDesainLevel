using UnityEngine;
using System.Collections.Generic;
using System;

namespace InfimaGames.LowPolyShooterPack
{
    public class ArenaDoor : MonoBehaviour
    {
        #region SERIALIZED FIELDS
        
        [Header("Door Settings")]
        [Tooltip("Pintu terkunci di awal?")]
        [SerializeField]
        private bool startLocked = false;
        
        [Tooltip("Auto-close door setelah player keluar?")]
        [SerializeField]
        private bool autoClose = true;
        
        [Header("Animation")]
        [Tooltip("Animator component (auto-assigned jika null)")]
        [SerializeField]
        private Animator doorAnimator;
        
        [Tooltip("Nama trigger untuk open animation")]
        [SerializeField]
        private string openTriggerName = "Open";
        
        [Tooltip("Nama trigger untuk close animation")]
        [SerializeField]
        private string closeTriggerName = "Close";
        
        [Header("Visual")]
        [Tooltip("Locked indicator (particle, light, dll)")]
        [SerializeField]
        private GameObject lockedIndicator;
        
        [Tooltip("Material saat locked (optional)")]
        [SerializeField]
        private Material lockedMaterial;
        
        [Tooltip("Material saat unlocked (optional)")]
        [SerializeField]
        private Material unlockedMaterial;
        
        [Tooltip("Door renderer untuk change material (optional)")]
        [SerializeField]
        private Renderer doorRenderer;
        
        [Header("Audio")]
        [SerializeField]
        private AudioClip lockSound;
        
        [SerializeField]
        private AudioClip unlockSound;
        
        [SerializeField]
        private AudioClip openSound;
        
        [SerializeField]
        private AudioClip closeSound;
        
        [SerializeField]
        private AudioClip lockedAttemptSound; // Suara saat coba buka pintu yang locked
        
        [Header("Debug")]
        [SerializeField]
        private bool showDebugLogs = false;
        
        #endregion
        
        #region PRIVATE FIELDS
        
        private bool isLocked;
        private bool isOpen = false;
        private AudioSource audioSource;
        
        #endregion
        
        #region PROPERTIES
        
        public bool IsLocked => isLocked;
        public bool IsOpen => isOpen;
        
        #endregion
        
        #region UNITY LIFECYCLE
        
        private void Awake()
        {
            // Auto-assign animator jika tidak diset
            if (doorAnimator == null)
            {
                doorAnimator = GetComponent<Animator>();
                
                if (doorAnimator == null)
                {
                    Debug.LogWarning($"ArenaDoor {gameObject.name}: No Animator found!");
                }
            }
            
            // Get or add AudioSource
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
                audioSource.playOnAwake = false;
                audioSource.spatialBlend = 1f; // 3D sound
            }
        }
        
        private void Start()
        {
            // Set initial state
            SetLocked(startLocked);
        }
        
        private void OnTriggerEnter(Collider other)
        {
            // Check if player
            if (!other.CompareTag("Player"))
                return;
            
            if (showDebugLogs)
                Debug.Log($"ArenaDoor {gameObject.name}: Player entered trigger");
            
            // Try to open door
            TryOpen();
        }
        
        private void OnTriggerExit(Collider other)
        {
            // Check if player
            if (!other.CompareTag("Player"))
                return;
            
            if (showDebugLogs)
                Debug.Log($"ArenaDoor {gameObject.name}: Player exited trigger");
            
            // Auto-close door when player leaves
            if (autoClose && !isLocked && isOpen)
            {
                Close();
            }
        }
        
        #endregion
        
        #region PUBLIC METHODS
        
        /// <summary>
        /// Set door locked state
        /// Dipanggil dari ArenaManager
        /// </summary>
        public void SetLocked(bool locked)
        {
            isLocked = locked;
            
            // Update visual
            UpdateVisual();
            
            // Play sound
            if (locked)
            {
                PlaySound(lockSound);
                
                // Force close door jika di-lock
                if (isOpen)
                {
                    Close();
                }
                
                if (showDebugLogs)
                    Debug.Log($"ArenaDoor {gameObject.name}: Locked!");
            }
            else
            {
                PlaySound(unlockSound);
                
                if (showDebugLogs)
                    Debug.Log($"ArenaDoor {gameObject.name}: Unlocked!");
            }
        }
        
        /// <summary>
        /// Try to open door
        /// </summary>
        public void TryOpen()
        {
            if (isLocked)
            {
                // Locked! Play feedback
                PlaySound(lockedAttemptSound);
                
                if (showDebugLogs)
                    Debug.Log($"ArenaDoor {gameObject.name}: Cannot open, door is locked!");
                
                // Bisa tambahkan visual feedback (shake animation)
                // doorAnimator.SetTrigger("Shake");
                
                return;
            }
            
            // Open door
            Open();
        }
        
        /// <summary>
        /// Open door (play open animation)
        /// </summary>
        public void Open()
        {
            if (isOpen || isLocked || doorAnimator == null)
                return;
            
            isOpen = true;
            
            // Trigger open animation
            doorAnimator.SetTrigger(openTriggerName);
            
            // Play sound
            PlaySound(openSound);
            
            if (showDebugLogs)
                Debug.Log($"ArenaDoor {gameObject.name}: Opening...");
        }
        
        /// <summary>
        /// Close door (play close animation)
        /// </summary>
        public void Close()
        {
            if (!isOpen || doorAnimator == null)
                return;
            
            isOpen = false;
            
            // Trigger close animation
            doorAnimator.SetTrigger(closeTriggerName);
            
            // Play sound
            PlaySound(closeSound);
            
            if (showDebugLogs)
                Debug.Log($"ArenaDoor {gameObject.name}: Closing...");
        }
        
        /// <summary>
        /// Force open door (ignore lock)
        /// </summary>
        public void ForceOpen()
        {
            isLocked = false;
            Open();
        }
        
        /// <summary>
        /// Force close door (ignore lock)
        /// </summary>
        public void ForceClose()
        {
            Close();
        }
        
        #endregion
        
        #region PRIVATE METHODS
        
        private void UpdateVisual()
        {
            // Update material (optional)
            if (doorRenderer != null)
            {
                if (isLocked && lockedMaterial != null)
                {
                    doorRenderer.material = lockedMaterial;
                }
                else if (!isLocked && unlockedMaterial != null)
                {
                    doorRenderer.material = unlockedMaterial;
                }
            }
            
            // Update locked indicator
            if (lockedIndicator != null)
            {
                lockedIndicator.SetActive(isLocked);
            }
        }
        
        private void PlaySound(AudioClip clip)
        {
            if (clip != null && audioSource != null)
            {
                audioSource.PlayOneShot(clip);
            }
        }
        
        #endregion
    }
    }