// Copyright 2021, Infima Games. All Rights Reserved.

using UnityEngine;

namespace InfimaGames.LowPolyShooterPack
{
    /// <summary>
    /// Weapon. This class handles most of the things that weapons need.
    /// </summary>
    public class Weapon : WeaponBehaviour
    {
        #region FIELDS SERIALIZED - TAMBAHAN

        [Header("Damage Settings")]
        [Tooltip("Base damage per shot")]
        [SerializeField]
        private float damage = 25f;

        #endregion

        #region FIELDS SERIALIZED
        
        [Header("Firing")]
        [Tooltip("Is this weapon automatic?")]
        [SerializeField] 
        private bool automatic;
        
        [Tooltip("How fast the projectiles are.")]
        [SerializeField]
        private float projectileImpulse = 400.0f;

        [Tooltip("Rounds per minute.")]
        [SerializeField] 
        private int roundsPerMinutes = 200;

        [Tooltip("Mask of things recognized when firing.")]
        [SerializeField]
        private LayerMask mask;

        [Tooltip("Maximum firing distance.")]
        [SerializeField]
        private float maximumDistance = 500.0f;

        [Header("Animation")]
        [Tooltip("Ejection port transform.")]
        [SerializeField]
        private Transform socketEjection;

        [Header("Resources")]
        [Tooltip("Casing Prefab.")]
        [SerializeField]
        private GameObject prefabCasing;
        
        [Tooltip("Projectile Prefab.")]
        [SerializeField]
        private GameObject prefabProjectile;
        
        [Tooltip("Animator Controller.")]
        [SerializeField] 
        public RuntimeAnimatorController controller;

        [Tooltip("Weapon Body Texture.")]
        [SerializeField]
        private Sprite spriteBody;
        
        [Header("Audio Clips")]
        [SerializeField] private AudioClip audioClipHolster;
        [SerializeField] private AudioClip audioClipUnholster;
        [SerializeField] private AudioClip audioClipReload;
        [SerializeField] private AudioClip audioClipReloadEmpty;
        [SerializeField] private AudioClip audioClipFireEmpty;
        
        #endregion

        #region FIELDS
        private Animator animator;
        private WeaponAttachmentManagerBehaviour attachmentManager;
        private int ammunitionCurrent;
        
        private MagazineBehaviour magazineBehaviour;
        private MuzzleBehaviour muzzleBehaviour;
        
        private IGameModeService gameModeService;
        private CharacterBehaviour characterBehaviour;
        private Transform playerCamera;
        
        // BARU: Reference ke ammunition inventory
        private AmmunitionInventory ammunitionInventory;
        #endregion

        #region UNITY
        protected override void Awake()
        {
            animator = GetComponent<Animator>();
            attachmentManager = GetComponent<WeaponAttachmentManagerBehaviour>();
            gameModeService = ServiceLocator.Current.Get<IGameModeService>();
            characterBehaviour = gameModeService.GetPlayerCharacter();
            playerCamera = characterBehaviour.GetCameraWorld().transform;
            
            // BARU: Get ammunition inventory dari character
            ammunitionInventory = characterBehaviour.GetComponent<AmmunitionInventory>();
            
            if (ammunitionInventory == null)
            {
                Debug.LogError("AmmunitionInventory component not found on player character!");
            }
        }

        protected override void Start()
        {
            magazineBehaviour = attachmentManager.GetEquippedMagazine();
            muzzleBehaviour = attachmentManager.GetEquippedMuzzle();
            
            ammunitionCurrent = magazineBehaviour.GetAmmunitionTotal();
        }
        #endregion

        #region GETTERS
        public override Animator GetAnimator() => animator;
        public override Sprite GetSpriteBody() => spriteBody;
        public override AudioClip GetAudioClipHolster() => audioClipHolster;
        public override AudioClip GetAudioClipUnholster() => audioClipUnholster;
        public override AudioClip GetAudioClipReload() => audioClipReload;
        public override AudioClip GetAudioClipReloadEmpty() => audioClipReloadEmpty;
        public override AudioClip GetAudioClipFireEmpty() => audioClipFireEmpty;
        public override AudioClip GetAudioClipFire() => muzzleBehaviour.GetAudioClipFire();
        public override int GetAmmunitionCurrent() => ammunitionCurrent;
        public override int GetAmmunitionTotal() => magazineBehaviour.GetAmmunitionTotal();
        public override bool IsAutomatic() => automatic;
        public override float GetRateOfFire() => roundsPerMinutes;
        public override bool IsFull() => ammunitionCurrent == magazineBehaviour.GetAmmunitionTotal();
        public override bool HasAmmunition() => ammunitionCurrent > 0;
        public override RuntimeAnimatorController GetAnimatorController() => controller;
        public override WeaponAttachmentManagerBehaviour GetAttachmentManager() => attachmentManager;
        #endregion

        

        #region METHODS

        

        /// <summary>
        /// BARU: Check apakah bisa reload
        /// Conditions: 1) Magazine belum penuh, 2) Ada ammo di inventory
        /// </summary>
        public override bool CanReload()
        {
            if (ammunitionInventory == null || magazineBehaviour == null)
                return false;

            // Cek magazine belum penuh
            bool magazineNotFull = !IsFull();
            
            // Cek ada reserve ammo
            AmmunitionType ammoType = magazineBehaviour.GetAmmunitionType();
            bool hasReserveAmmo = ammunitionInventory.HasReserveAmmo(ammoType);

            return magazineNotFull && hasReserveAmmo;
        }

        /// <summary>
        /// UPDATED: Reload dengan inventory system
        /// </summary>
        public override void Reload()
        {
            // Validasi dulu
            if (!CanReload())
            {
                Debug.Log("Cannot reload: Magazine full or no reserve ammo!");
                return;
            }

            // Play animation
            animator.Play(HasAmmunition() ? "Reload" : "Reload Empty", 0, 0.0f);
            
            // NOTE: Actual ammo refill dilakukan di animation event
            // atau bisa call ReloadComplete() dari animation
        }

        /// <summary>
        /// BARU: Method ini dipanggil dari Animation Event saat reload selesai
        /// </summary>
        public void ReloadComplete()
        {
            if (ammunitionInventory == null || magazineBehaviour == null)
                return;

            // Hitung berapa ammo yang dibutuhkan untuk isi magazine
            int maxCapacity = magazineBehaviour.GetAmmunitionTotal();
            int ammoNeeded = maxCapacity - ammunitionCurrent;

            // Ambil ammo dari inventory
            AmmunitionType ammoType = magazineBehaviour.GetAmmunitionType();
            int ammoObtained = ammunitionInventory.ConsumeAmmo(ammoType, ammoNeeded);

            // Isi magazine dengan ammo yang berhasil diambil
            ammunitionCurrent += ammoObtained;

            Debug.Log($"Reload complete. Magazine: {ammunitionCurrent}/{maxCapacity}. " +
                      $"Reserve: {ammunitionInventory.GetReserveAmount(ammoType)}");
        }

        public override void Fire(float spreadMultiplier = 1.0f)
        {
            if (muzzleBehaviour == null || playerCamera == null)
                return;

            Transform muzzleSocket = muzzleBehaviour.GetSocket();
            
            // Play animation
            const string stateName = "Fire";
            animator.Play(stateName, 0, 0.0f);
            
            // Decrease ammo
            ammunitionCurrent = Mathf.Clamp(
                ammunitionCurrent - 1, 
                0, 
                magazineBehaviour.GetAmmunitionTotal()
            );
            
            // Muzzle effect
            muzzleBehaviour.Effect();
            
            // Calculate target point dengan raycast
            Quaternion rotation = Quaternion.LookRotation(
                playerCamera.forward * 1000.0f - muzzleSocket.position
            );
            
            if (Physics.Raycast(
                new Ray(playerCamera.position, playerCamera.forward),
                out RaycastHit hit, maximumDistance, mask))
            {
                rotation = Quaternion.LookRotation(hit.point - muzzleSocket.position);
            }
            
            // Spawn projectile
            GameObject projectileObj = Instantiate(
                prefabProjectile, 
                muzzleSocket.position, 
                rotation
            );
            
            // ========================================
            // BARU: Setup projectile dengan damage info
            // ========================================
            Projectile projectile = projectileObj.GetComponent<Projectile>();
            if (projectile != null)
            {
                projectile.SetDamage(damage);
                projectile.SetShooter(characterBehaviour.gameObject);
            }
            
            // Apply velocity
            Rigidbody rb = projectileObj.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = projectileObj.transform.forward * projectileImpulse;
            }
        }

        /// <summary>
        /// DEPRECATED: Tidak digunakan lagi karena sekarang pakai inventory system
        /// Kept for backward compatibility
        /// </summary>
        public override void FillAmmunition(int amount)
        {
            Debug.LogWarning("FillAmmunition() is deprecated. Use AmmunitionInventory.AddAmmo() instead.");
            ammunitionCurrent = amount != 0 ? 
                Mathf.Clamp(ammunitionCurrent + amount, 0, GetAmmunitionTotal()) : 
                magazineBehaviour.GetAmmunitionTotal();
        }

        public override void EjectCasing()
        {
            if(prefabCasing != null && socketEjection != null)
                Instantiate(prefabCasing, socketEjection.position, socketEjection.rotation);
        }

        #endregion
    }
}