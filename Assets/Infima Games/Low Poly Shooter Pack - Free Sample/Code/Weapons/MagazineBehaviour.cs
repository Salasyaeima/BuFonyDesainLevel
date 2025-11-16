// Copyright 2021, Infima Games. All Rights Reserved.

using UnityEngine;

namespace InfimaGames.LowPolyShooterPack
{
    /// <summary>
    /// Magazine Behaviour.
    /// </summary>
    public abstract class MagazineBehaviour : MonoBehaviour
    {
        #region GETTERS
        public abstract int GetAmmunitionTotal();
        public abstract Sprite GetSprite();
        
        // TAMBAHAN: untuk identifikasi tipe ammo yang digunakan magazine ini
        public abstract AmmunitionType GetAmmunitionType();
        #endregion
    }
}