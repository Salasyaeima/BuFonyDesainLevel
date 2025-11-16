// Copyright 2021, Infima Games. All Rights Reserved.

using System;
using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;

namespace InfimaGames.LowPolyShooterPack
{
    /// <summary>
    /// Main Character Component - UPDATED untuk ammunition inventory system
    /// </summary>
    [RequireComponent(typeof(CharacterKinematics))]
    [RequireComponent(typeof(AmmunitionInventory))] // BARU: Required component
    public sealed class Character : CharacterBehaviour
    {
        #region FIELDS SERIALIZED

        [Header("Inventory")]
        [Tooltip("Inventory.")]
        [SerializeField]
        private InventoryBehaviour inventory;

        [Header("Cameras")]
        [Tooltip("Normal Camera.")]
        [SerializeField]
        private Camera cameraWorld;

        [Header("Animation")]
        [Tooltip("Determines how smooth the locomotion blendspace is.")]
        [SerializeField]
        private float dampTimeLocomotion = 0.15f;

        [Tooltip("How smoothly we play aiming transitions.")]
        [SerializeField]
        private float dampTimeAiming = 0.3f;
        
        [Header("Animation Procedural")]
        [Tooltip("Character Animator.")]
        [SerializeField]
        private Animator characterAnimator;

        #endregion

        #region FIELDS

        private bool aiming;
        private bool running;
        private bool holstered;
        private float lastShotTime;
        
        private int layerOverlay;
        private int layerHolster;
        private int layerActions;

        private CharacterKinematics characterKinematics;
        
        private WeaponBehaviour equippedWeapon;
        private WeaponAttachmentManagerBehaviour weaponAttachmentManager;
        private ScopeBehaviour equippedWeaponScope;
        private MagazineBehaviour equippedWeaponMagazine;
        
        // BARU: Ammunition Inventory reference
        private AmmunitionInventory ammunitionInventory;
        
        private bool reloading;
        private bool inspecting;
        private bool holstering;

        private Vector2 axisLook;
        private Vector2 axisMovement;
        
        private bool holdingButtonAim;
        private bool holdingButtonRun;
        private bool holdingButtonFire;

        private bool tutorialTextVisible;
        private bool cursorLocked;

        #endregion

        #region CONSTANTS

        private static readonly int HashAimingAlpha = Animator.StringToHash("Aiming");
        private static readonly int HashMovement = Animator.StringToHash("Movement");

        #endregion

        #region UNITY

        protected override void Awake()
        {
            #region Lock Cursor
            cursorLocked = true;
            UpdateCursorState();
            #endregion

            characterKinematics = GetComponent<CharacterKinematics>();
            
            // BARU: Get ammunition inventory component
            ammunitionInventory = GetComponent<AmmunitionInventory>();
            if (ammunitionInventory == null)
            {
                Debug.LogError("AmmunitionInventory component missing! Adding default...");
                ammunitionInventory = gameObject.AddComponent<AmmunitionInventory>();
            }

            inventory.Init();
            RefreshWeaponSetup();
        }

        protected override void Start()
        {
            layerHolster = characterAnimator.GetLayerIndex("Layer Holster");
            layerActions = characterAnimator.GetLayerIndex("Layer Actions");
            layerOverlay = characterAnimator.GetLayerIndex("Layer Overlay");
        }

        protected override void Update()
        {
            aiming = holdingButtonAim && CanAim();
            running = holdingButtonRun && CanRun();

            if (holdingButtonFire)
            {
                if (CanPlayAnimationFire() && equippedWeapon.HasAmmunition() && equippedWeapon.IsAutomatic())
                {
                    if (Time.time - lastShotTime > 60.0f / equippedWeapon.GetRateOfFire())
                        Fire();
                }   
            }

            UpdateAnimator();
        }

        protected override void LateUpdate()
        {
            if (equippedWeapon == null || equippedWeaponScope == null)
                return;
            
            if(characterKinematics != null)
                characterKinematics.Compute();
        }
        
        #endregion

        #region GETTERS

        public override Camera GetCameraWorld() => cameraWorld;
        public override InventoryBehaviour GetInventory() => inventory;
        
        // BARU: Return ammunition inventory
        public override AmmunitionInventory GetAmmunitionInventory() => ammunitionInventory;
        
        public override bool IsCrosshairVisible() => !aiming && !holstered;
        public override bool IsRunning() => running;
        public override bool IsAiming() => aiming;
        public override bool IsCursorLocked() => cursorLocked;
        public override bool IsTutorialTextVisible() => tutorialTextVisible;
        public override Vector2 GetInputMovement() => axisMovement;
        public override Vector2 GetInputLook() => axisLook;

        #endregion

        #region METHODS

        private void UpdateAnimator()
        {
            characterAnimator.SetFloat(HashMovement, 
                Mathf.Clamp01(Mathf.Abs(axisMovement.x) + Mathf.Abs(axisMovement.y)), 
                dampTimeLocomotion, Time.deltaTime);
            
            characterAnimator.SetFloat(HashAimingAlpha, 
                Convert.ToSingle(aiming), 
                0.25f / 1.0f * dampTimeAiming, Time.deltaTime);

            characterAnimator.SetBool("Aim", aiming);
            characterAnimator.SetBool("Running", running);
        }
        
        private void Inspect()
        {
            inspecting = true;
            characterAnimator.CrossFade("Inspect", 0.0f, layerActions, 0);
        }
        
        private void Fire()
        {
            lastShotTime = Time.time;
            equippedWeapon.Fire();
            characterAnimator.CrossFade("Fire", 0.05f, layerOverlay, 0);
        }

        /// <summary>
        /// UPDATED: Cek ammunition inventory sebelum reload
        /// </summary>
        private void PlayReloadAnimation()
        {
            // BARU: Validasi dengan ammunition inventory
            if (equippedWeapon == null || equippedWeaponMagazine == null)
                return;

            // Check apakah bisa reload (magazine belum penuh & ada reserve ammo)
            AmmunitionType ammoType = equippedWeaponMagazine.GetAmmunitionType();
            bool hasReserveAmmo = ammunitionInventory.HasReserveAmmo(ammoType);
            bool magazineNotFull = !equippedWeapon.IsFull();

            if (!hasReserveAmmo)
            {
                Debug.Log($"Cannot reload: No reserve ammunition for {ammoType}!");
                // Bisa tambahkan audio feedback di sini
                return;
            }

            if (!magazineNotFull)
            {
                Debug.Log("Cannot reload: Magazine is already full!");
                return;
            }

            #region Animation
            string stateName = equippedWeapon.HasAmmunition() ? "Reload" : "Reload Empty";
            characterAnimator.Play(stateName, layerActions, 0.0f);
            reloading = true;
            #endregion

            // NOTE: Weapon.Reload() hanya trigger animation
            // Actual ammo refill akan dipanggil dari ReloadComplete() via animation event
            equippedWeapon.Reload();
        }



        private IEnumerator Equip(int index = 0)
        {
            if(!holstered)
            {
                SetHolstered(holstering = true);
                yield return new WaitUntil(() => holstering == false);
            }
            
            SetHolstered(false);
            characterAnimator.Play("Unholster", layerHolster, 0);
            
            inventory.Equip(index);
            RefreshWeaponSetup();
        }

        private void RefreshWeaponSetup()
        {
            if ((equippedWeapon = inventory.GetEquipped()) == null)
                return;
            
            characterAnimator.runtimeAnimatorController = equippedWeapon.GetAnimatorController();

            weaponAttachmentManager = equippedWeapon.GetAttachmentManager();
            if (weaponAttachmentManager == null) 
                return;
            
            equippedWeaponScope = weaponAttachmentManager.GetEquippedScope();
            equippedWeaponMagazine = weaponAttachmentManager.GetEquippedMagazine();
        }

        private void FireEmpty()
        {
            lastShotTime = Time.time;
            characterAnimator.CrossFade("Fire Empty", 0.05f, layerOverlay, 0);
        }

        private void UpdateCursorState()
        {
            Cursor.visible = !cursorLocked;
            Cursor.lockState = cursorLocked ? CursorLockMode.Locked : CursorLockMode.None;
        }

        private void SetHolstered(bool value = true)
        {
            holstered = value;
            characterAnimator.SetBool("Holstered", holstered);    
        }
        
        #region ACTION CHECKS

        private bool CanPlayAnimationFire()
        {
            if (holstered || holstering || reloading || inspecting)
                return false;
            return true;
        }

        private bool CanPlayAnimationReload()
        {
            if (reloading || inspecting)
                return false;
            return true;
        }

        private bool CanPlayAnimationHolster()
        {
            if (reloading || inspecting)
                return false;
            return true;
        }

        private bool CanChangeWeapon()
        {
            if (holstering || reloading || inspecting)
                return false;
            return true;
        }

        private bool CanPlayAnimationInspect()
        {
            if (holstered || holstering || reloading || inspecting)
                return false;
            return true;
        }

        private bool CanAim()
        {
            if (holstered || inspecting || reloading || holstering)
                return false;
            return true;
        }
        
        private bool CanRun()
        {
            if (inspecting || reloading || aiming)
                return false;

            if (holdingButtonFire && equippedWeapon.HasAmmunition())
                return false;

            if (axisMovement.y <= 0 || Math.Abs(Mathf.Abs(axisMovement.x) - 1) < 0.01f)
                return false;
            
            return true;
        }

        #endregion

        #region INPUT

        public void OnTryFire(InputAction.CallbackContext context)
        {
            if (!cursorLocked)
                return;

            switch (context)
            {
                case {phase: InputActionPhase.Started}:
                    holdingButtonFire = true;
                    break;
                case {phase: InputActionPhase.Performed}:
                    if (!CanPlayAnimationFire())
                        break;
                    
                    if (equippedWeapon.HasAmmunition())
                    {
                        if (equippedWeapon.IsAutomatic())
                            break;
                            
                        if (Time.time - lastShotTime > 60.0f / equippedWeapon.GetRateOfFire())
                            Fire();
                    }
                    else
                        FireEmpty();
                    break;
                case {phase: InputActionPhase.Canceled}:
                    holdingButtonFire = false;
                    break;
            }
        }

        public void OnTryPlayReload(InputAction.CallbackContext context)
        {
            if (!cursorLocked)
                return;
            
            if (!CanPlayAnimationReload())
                return;
            
            switch (context)
            {
                case {phase: InputActionPhase.Performed}:
                    PlayReloadAnimation();
                    break;
            }
        }

        public void OnTryInspect(InputAction.CallbackContext context)
        {
            if (!cursorLocked || !CanPlayAnimationInspect())
                return;
            
            switch (context)
            {
                case {phase: InputActionPhase.Performed}:
                    Inspect();
                    break;
            }
        }

        public void OnTryAiming(InputAction.CallbackContext context)
        {
            if (!cursorLocked)
                return;

            switch (context.phase)
            {
                case InputActionPhase.Started:
                    holdingButtonAim = true;
                    break;
                case InputActionPhase.Canceled:
                    holdingButtonAim = false;
                    break;
            }
        }

        public void OnTryHolster(InputAction.CallbackContext context)
        {
            if (!cursorLocked)
                return;
            
            switch (context.phase)
            {
                case InputActionPhase.Performed:
                    if (CanPlayAnimationHolster())
                    {
                        SetHolstered(!holstered);
                        holstering = true;
                    }
                    break;
            }
        }

        public void OnTryRun(InputAction.CallbackContext context)
        {
            if (!cursorLocked)
                return;
            
            switch (context.phase)
            {
                case InputActionPhase.Started:
                    holdingButtonRun = true;
                    break;
                case InputActionPhase.Canceled:
                    holdingButtonRun = false;
                    break;
            }
        }

        public void OnTryInventoryNext(InputAction.CallbackContext context)
        {
            if (!cursorLocked || inventory == null)
                return;
            
            switch (context)
            {
                case {phase: InputActionPhase.Performed}:
                    float scrollValue = context.valueType.IsEquivalentTo(typeof(Vector2)) ? 
                        Mathf.Sign(context.ReadValue<Vector2>().y) : 1.0f;
                    
                    int indexNext = scrollValue > 0 ? inventory.GetNextIndex() : inventory.GetLastIndex();
                    int indexCurrent = inventory.GetEquippedIndex();
                    
                    if (CanChangeWeapon() && (indexCurrent != indexNext))
                        StartCoroutine(nameof(Equip), indexNext);
                    break;
            }
        }
        
        public void OnLockCursor(InputAction.CallbackContext context)
        {
            switch (context)
            {
                case {phase: InputActionPhase.Performed}:
                    cursorLocked = !cursorLocked;
                    UpdateCursorState();
                    break;
            }
        }
        
        public void OnMove(InputAction.CallbackContext context)
        {
            axisMovement = cursorLocked ? context.ReadValue<Vector2>() : default;
        }

        public void OnLook(InputAction.CallbackContext context)
        {
            axisLook = cursorLocked ? context.ReadValue<Vector2>() : default;
        }

        public void OnUpdateTutorial(InputAction.CallbackContext context)
        {
            tutorialTextVisible = context switch
            {
                {phase: InputActionPhase.Started} => true,
                {phase: InputActionPhase.Canceled} => false,
                _ => tutorialTextVisible
            };
        }

        #endregion

        #region ANIMATION EVENTS

        public override void EjectCasing()
        {
            if(equippedWeapon != null)
                equippedWeapon.EjectCasing();
        }

        /// <summary>
        /// DEPRECATED: Kept for backward compatibility with old animations
        /// </summary>
        [System.Obsolete("Use ReloadComplete() instead")]
        public override void FillAmmunition(int amount)
        {
            Debug.LogWarning("FillAmmunition() is deprecated. Animation should call ReloadComplete() instead.");
            // Fallback ke ReloadComplete untuk compatibility
            ReloadComplete();
        }

        /// <summary>
        /// BARU: Dipanggil dari animation event saat reload selesai
        /// Mengambil ammo dari inventory dan mengisi magazine
        /// </summary>
        /// <summary>
        /// BARU: Dipanggil dari animation event saat reload selesai
        /// Mengambil ammo dari inventory dan mengisi magazine
        /// </summary>
        public override void ReloadComplete()
        {
            if (equippedWeapon == null || ammunitionInventory == null || equippedWeaponMagazine == null)
                return;

            // Hitung ammo yang dibutuhkan
            int maxCapacity = equippedWeaponMagazine.GetAmmunitionTotal();
            int currentAmmo = equippedWeapon.GetAmmunitionCurrent();
            int ammoNeeded = maxCapacity - currentAmmo;

            // Ambil ammo dari inventory
            AmmunitionType ammoType = equippedWeaponMagazine.GetAmmunitionType();
            int ammoObtained = ammunitionInventory.ConsumeAmmo(ammoType, ammoNeeded);

            // Fill ammunition via weapon
            if (ammoObtained > 0)
            {
                equippedWeapon.FillAmmunition(ammoObtained);
                
                Debug.Log($"Reload complete: Added {ammoObtained} rounds. " +
                          $"Magazine: {equippedWeapon.GetAmmunitionCurrent()}/{maxCapacity}. " +
                          $"Reserve: {ammunitionInventory.GetReserveAmount(ammoType)}");
            }
        }
        
        public override void SetActiveMagazine(int active)
        {
            equippedWeaponMagazine.gameObject.SetActive(active != 0);
        }
        
        public override void AnimationEndedReload()
        {
            reloading = false;
        }

        public override void AnimationEndedInspect()
        {
            inspecting = false;
        }

        public override void AnimationEndedHolster()
        {
            holstering = false;
        }

        #endregion

        #endregion
    }
}