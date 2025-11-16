// Copyright 2021, Infima Games. All Rights Reserved.

using System.Globalization;
using TMPro;

namespace InfimaGames.LowPolyShooterPack.Interface
{
    /// <summary>
    /// Total Ammunition Text.
    /// </summary>
    public class TextAmmunitionTotal : ElementText
    {
        #region METHODS
        


protected override void Tick()
{
    // 1. Get ammunition inventory
    if (ammunitionInventory == null)
        ammunitionInventory = playerCharacter.GetAmmunitionInventory();
    
    // 2. Get magazine untuk tahu ammo type
    var magazineBehaviour = equippedWeapon.GetAttachmentManager()?.GetEquippedMagazine();
    
    // 3. Get ammo type dari magazine
    AmmunitionType ammoType = magazineBehaviour.GetAmmunitionType();
    
    // 4. Get RESERVE AMMO dari inventory (bukan magazine capacity!)
    int reserveAmmo = ammunitionInventory.GetReserveAmount(ammoType);
    
    // 5. Display reserve ammo
    textMesh.text = reserveAmmo.ToString(CultureInfo.InvariantCulture);
}
        
        #endregion
    }
}