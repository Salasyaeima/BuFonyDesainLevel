using UnityEngine;

    namespace InfimaGames.LowPolyShooterPack
{
    
    public class AmmoPickupLarge : AmmoPickup
    {
        private void Reset()
        {
            // Default values untuk large ammo
            GetType().GetField("ammoAmount", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(this, 60);
            GetType().GetField("displayName", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(this, "Large Ammo Box");
        }
    }
 }