/* 
    ------------------- Code Monkey -------------------

    Thank you for downloading this package
    I hope you find it useful in your projects
    If you have any questions let me know
    Cheers!

               unitycodemonkey.com
    --------------------------------------------------
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CodeMonkey.KeyDoorSystemCM {

     /// <summary>
     /// Added to Key prefab, holds reference of the Key object
     /// </summary>
    public class DoorKey : MonoBehaviour {

        [Header("Door Key")]
        [Tooltip("The Key Scriptable Object")]
        public Key key;
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
        private Vector3 startPosition;

        public void DestroySelf() {
            // Destroy this Key
            Destroy(gameObject);
        }

        void Awake()
        {
            startPosition = transform.position;
        }
        protected virtual void Update()
        {
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

    }

}