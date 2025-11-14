using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace CodeMonkey.KeyDoorSystemCM {
    
    public class DoorKeyHolder : MonoBehaviour {
        
        public event EventHandler OnDoorKeyAdded;
         public event EventHandler OnDoorKeyUsed;
        public event EventHandler<DoorUnlockEventArgs> OnDoorUnlocked;
        public UnityEvent OnEnableGameObject;
        
        [Header("Key Holder")]
        [Tooltip("List of Keys currently being held")]
        public List<Key> doorKeyHoldingList = new List<Key>();
        
        void OnTriggerEnter(Collider collider) {
            DoorKey doorKey = collider.GetComponent<DoorKey>();
            if (doorKey != null) {
                doorKeyHoldingList.Add(doorKey.key);
                doorKey.DestroySelf();
                OnDoorKeyAdded?.Invoke(this, EventArgs.Empty);
                OnEnableGameObject.Invoke();
            }
            
            DoorLock doorLock = collider.GetComponent<DoorLock>();
            if (doorLock != null) {
                if (doorLock.TryOpenDoor(this)) {
                    OnDoorUnlocked?.Invoke(this, new DoorUnlockEventArgs {
                        doorLock = doorLock,
                        keysUsed = doorLock.GetRequiredKeyCount()
                    });
                }
            }
        }
    }
    
    public class DoorUnlockEventArgs : EventArgs {
        public DoorLock doorLock;
        public int keysUsed;
    }
}