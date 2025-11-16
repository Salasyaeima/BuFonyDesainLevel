// Copyright 2021, Infima Games. All Rights Reserved.

using UnityEngine;

namespace InfimaGames.LowPolyShooterPack
{
    /// <summary>
    /// Magazine.
    /// </summary>
    public class Magazine : MagazineBehaviour
    {
        #region FIELDS SERIALIZED
        [Header("Settings")]
        [Tooltip("Total Ammunition capacity in magazine.")]
        [SerializeField]
        private int ammunitionTotal = 30;

        [Tooltip("Tipe amunisi yang digunakan magazine ini")]
        [SerializeField]
        private AmmunitionType ammunitionType = AmmunitionType.Rifle;

        [Header("Interface")]
        [Tooltip("Interface Sprite.")]
        [SerializeField]
        private Sprite sprite;
        #endregion

        #region GETTERS
        public override int GetAmmunitionTotal() => ammunitionTotal;
        public override Sprite GetSprite() => sprite;
        public override AmmunitionType GetAmmunitionType() => ammunitionType;
        #endregion
    }
}