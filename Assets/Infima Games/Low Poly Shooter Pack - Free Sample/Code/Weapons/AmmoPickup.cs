using UnityEngine;
    namespace InfimaGames.LowPolyShooterPack
{
    public class AmmoPickup : PickupBase
    {
        [Header("Ammo Settings")]
        [Tooltip("Tipe ammunition yang diberikan")]
        [SerializeField]
        private AmmunitionType ammunitionType = AmmunitionType.Rifle;

        [Tooltip("Jumlah ammo yang diberikan")]
        [SerializeField]
        private int ammoAmount = 30;

        [Tooltip("Universal ammo? (isi semua tipe ammunition)")]
        [SerializeField]
        private bool isUniversal = false;

        [Header("UI Display")]
        [Tooltip("Display text (optional, untuk UI label)")]
        [SerializeField]
        private string displayName = "Rifle Ammo";

        protected override bool TryPickup(CharacterBehaviour character)
        {
            // Get ammunition inventory
            AmmunitionInventory inventory = character.GetAmmunitionInventory();
            
            if (inventory == null)
            {
                Debug.LogError("AmmunitionInventory not found on character!");
                return false;
            }

            // Universal ammo pickup
            if (isUniversal)
            {
                return TryPickupUniversal(inventory);
            }

            // Specific ammo type pickup
            return TryPickupSpecific(inventory);
        }

        private bool TryPickupSpecific(AmmunitionInventory inventory)
        {
            // Cek apakah inventory sudah penuh untuk tipe ammo ini
            int currentReserve = inventory.GetReserveAmount(ammunitionType);
            
            if (currentReserve >= 999)
            {
                return false; // Inventory penuh
            }

            // Add ammo
            inventory.AddAmmo(ammunitionType, ammoAmount);
            
            Debug.Log($"Picked up {ammoAmount}x {ammunitionType} ammo! Total: {inventory.GetReserveAmount(ammunitionType)}");
            
            return true;
        }

        private bool TryPickupUniversal(AmmunitionInventory inventory)
        {
            bool anyAdded = false;

            // Add ammo ke semua tipe
            foreach (AmmunitionType type in System.Enum.GetValues(typeof(AmmunitionType)))
            {
                int currentReserve = inventory.GetReserveAmount(type);
                
                if (currentReserve < 999)
                {
                    inventory.AddAmmo(type, ammoAmount);
                    anyAdded = true;
                }
            }

            if (anyAdded)
            {
                Debug.Log($"Picked up Universal Ammo: +{ammoAmount} to all types!");
                return true;
            }

            return false;
        }

        protected override string GetPickupFailedMessage()
        {
            if (isUniversal)
                return "All ammunition types are full!";
            
            return $"{displayName} inventory is full!";
        }

        #region EDITOR HELPER

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, 1f);
        }

        #endregion
    }
}
