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
        
        [Header("Visual")]
        [Tooltip("Door mesh/model")]
        [SerializeField]
        private GameObject doorObject;
        
        [Tooltip("Material saat locked")]
        [SerializeField]
        private Material lockedMaterial;
        
        [Tooltip("Material saat unlocked")]
        [SerializeField]
        private Material unlockedMaterial;
        
        [Tooltip("Locked indicator (particle, light, dll)")]
        [SerializeField]
        private GameObject lockedIndicator;
        
        [Header("Animation")]
        [Tooltip("Open position offset")]
        [SerializeField]
        private Vector3 openPositionOffset = new Vector3(0, 3, 0);
        
        [Tooltip("Open speed")]
        [SerializeField]
        private float openSpeed = 2f;
        
        [Header("Audio")]
        [SerializeField]
        private AudioClip lockSound;
        
        [SerializeField]
        private AudioClip unlockSound;
        
        [SerializeField]
        private AudioClip lockedAttemptSound; // Suara saat coba buka pintu yang locked
        
        #endregion
        
        #region PRIVATE FIELDS
        
        private bool isLocked;
        private bool isOpen = false;
        private Vector3 closedPosition;
        private Vector3 openPosition;
        private Renderer doorRenderer;
        
        #endregion
        
        #region PROPERTIES
        
        public bool IsLocked => isLocked;
        public bool IsOpen => isOpen;
        
        #endregion
        
        #region UNITY LIFECYCLE
        
        private void Awake()
        {
            closedPosition = transform.position;
            openPosition = closedPosition + openPositionOffset;
            
            if (doorObject != null)
            {
                doorRenderer = doorObject.GetComponent<Renderer>();
            }
        }
        
        private void Start()
        {
            SetLocked(startLocked);
        }
        
        private void Update()
        {
            // Animate door opening/closing
            if (!isLocked)
            {
                Vector3 targetPosition = isOpen ? openPosition : closedPosition;
                transform.position = Vector3.Lerp(
                    transform.position, 
                    targetPosition, 
                    Time.deltaTime * openSpeed
                );
            }
        }
        
        private void OnTriggerEnter(Collider other)
        {
            // Check if player
            CharacterBehaviour character = other.GetComponent<CharacterBehaviour>();
            if (character == null)
                return;
            
            // Try to open door
            TryOpen();
        }
        
        private void OnTriggerExit(Collider other)
        {
            // Check if player
            CharacterBehaviour character = other.GetComponent<CharacterBehaviour>();
            if (character == null)
                return;
            
            // Close door when player leaves
            if (!isLocked)
            {
                Close();
            }
        }
        
        #endregion
        
        #region PUBLIC METHODS
        
        /// <summary>
        /// Set door locked state
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
                Debug.Log($"ArenaDoor {gameObject.name}: Locked!");
            }
            else
            {
                PlaySound(unlockSound);
                Debug.Log($"ArenaDoor {gameObject.name}: Unlocked!");
            }
            
            // Close door if locked
            if (locked)
            {
                Close();
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
                Debug.Log($"ArenaDoor {gameObject.name}: Cannot open, door is locked!");
                
                // Bisa tambahkan visual feedback (shake, flash, dll)
                return;
            }
            
            // Open door
            Open();
        }
        
        /// <summary>
        /// Force open door
        /// </summary>
        public void Open()
        {
            if (isOpen || isLocked)
                return;
            
            isOpen = true;
            Debug.Log($"ArenaDoor {gameObject.name}: Opening...");
        }
        
        /// <summary>
        /// Close door
        /// </summary>
        public void Close()
        {
            if (!isOpen)
                return;
            
            isOpen = false;
            Debug.Log($"ArenaDoor {gameObject.name}: Closing...");
        }
        
        #endregion
        
        #region PRIVATE METHODS
        
        private void UpdateVisual()
        {
            // Update material
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
            if (clip != null)
            {
                AudioSource.PlayClipAtPoint(clip, transform.position);
            }
        }
        
        #endregion
    }
    }