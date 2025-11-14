using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CodeMonkey.KeyDoorSystemCM {
    
    public class DoorLock : MonoBehaviour {
        
        [Header("Door Lock Configuration")]
        [Tooltip("List of keys required to open this door")]
        public List<Key> requiredKeys = new List<Key>();
        
        [Tooltip("Remove keys from holder after using?")]
        public bool removeKeysOnUse = true;
        
        [Header("Door State")]
        [Tooltip("Keys that have been used on this door")]
        private List<Key> usedKeys = new List<Key>();
        
        private Animator m_Animator;
        private bool isOpen = false;
        
        void Awake() {
            m_Animator = GetComponent<Animator>();
        }
        
        public bool TryOpenDoor(DoorKeyHolder keyHolder) {
            if (isOpen) return false;
            
            // Check apakah semua required keys ada di holder
            foreach (Key requiredKey in requiredKeys) {
                if (!keyHolder.doorKeyHoldingList.Contains(requiredKey)) {
                    return false; // Missing key
                }
            }
            
            // Semua key ada, buka pintu
            if (removeKeysOnUse) {
                foreach (Key key in requiredKeys) {
                    keyHolder.doorKeyHoldingList.Remove(key);
                }
            }
            
            usedKeys.AddRange(requiredKeys);
            OpenDoor();
            return true;
        }
        
        public int GetRequiredKeyCount() {
            return requiredKeys.Count;
        }
        
        public int GetCollectedKeyCount(DoorKeyHolder keyHolder) {
            return requiredKeys.Count(key => keyHolder.doorKeyHoldingList.Contains(key));
        }
        
        private void OpenDoor() {
            isOpen = true;
            if(m_Animator == null) 
            {return;}
            m_Animator.SetTrigger("Open");
        }
        
        public void CloseDoor() {
            isOpen = false;
            if(m_Animator == null) 
            {return;}
            m_Animator.SetTrigger("Close");
        }
    }
}