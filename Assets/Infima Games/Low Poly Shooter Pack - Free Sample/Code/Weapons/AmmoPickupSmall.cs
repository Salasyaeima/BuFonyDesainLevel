using UnityEngine;

    namespace InfimaGames.LowPolyShooterPack
{
    
    public class AmmoPickupSmall : AmmoPickup
    {
        private void Reset()
        {
            // Default values untuk small ammo
            GetType().GetField("ammoAmount", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(this, 15);
            GetType().GetField("displayName", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(this, "Small Ammo Box");
        }
    }

 }