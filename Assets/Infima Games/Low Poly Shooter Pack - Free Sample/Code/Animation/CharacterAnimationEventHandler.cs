// Copyright 2021, Infima Games. All Rights Reserved.

using UnityEngine;

namespace InfimaGames.LowPolyShooterPack
{
	/// <summary>
	/// Handles all the animation events that come from the character in the asset.
	/// </summary>
	public class CharacterAnimationEventHandler : MonoBehaviour
	{
		#region FIELDS
        // BARU: Return ammunition inventory
        
		/// <summary>
        /// Character Component Reference.
        /// </summary>
        private CharacterBehaviour playerCharacter;

		#endregion

		#region UNITY

		private void Awake()
		{
			//Grab a reference to the character component.
			playerCharacter = ServiceLocator.Current.Get<IGameModeService>().GetPlayerCharacter();
		}

		#endregion

		#region ANIMATION

		/// <summary>
		/// Ejects a casing from the character's equipped weapon. This function is called from an Animation Event.
		/// </summary>
		private void OnEjectCasing()
		{
			//Notify the character.
			if(playerCharacter != null)
				playerCharacter.EjectCasing();
		}
        private void OnReloadComplete()
        {
            if(playerCharacter != null)
                playerCharacter.ReloadComplete();
        }
		/// <summary>
		/// Fills the character's equipped weapon's ammunition by a certain amount, or fully if set to 0. This function is called
		/// from a Animation Event.
		/// </summary>
        [System.Obsolete("Use OnReloadComplete() instead")]
        private void OnAmmunitionFill(int amount = 0)
        {
            Debug.LogWarning("OnAmmunitionFill is deprecated. Please update animation to use OnReloadComplete()");
            
            // Fallback untuk compatibility
            if(playerCharacter != null)
                playerCharacter.ReloadComplete();
        }

		/// <summary>
		/// Sets the character's knife active value. This function is called from an Animation Event.
		/// </summary>
		private void OnSetActiveKnife(int active)
		{
		}
		
		/// <summary>
		/// Spawns a grenade at the correct location. This function is called from an Animation Event.
		/// </summary>
		private void OnGrenade()
		{
		}
		/// <summary>
		/// Sets the equipped weapon's magazine to be active or inactive! This function is called from an Animation Event.
		/// </summary>
		private void OnSetActiveMagazine(int active)
		{
			//Notify the character.
			if(playerCharacter != null)
				playerCharacter.SetActiveMagazine(active);
		}

		/// <summary>
		/// Bolt Animation Ended. This function is called from an Animation Event.
		/// </summary>
		private void OnAnimationEndedBolt()
		{
		}
		/// <summary>
		/// Reload Animation Ended. This function is called from an Animation Event.
		/// </summary>
		private void OnAnimationEndedReload()
		{
			//Notify the character.
			if(playerCharacter != null)
				playerCharacter.AnimationEndedReload();
		}

		/// <summary>
		/// Grenade Throw Animation Ended. This function is called from an Animation Event.
		/// </summary>
		private void OnAnimationEndedGrenadeThrow()
		{
		}
		/// <summary>
		/// Melee Animation Ended. This function is called from an Animation Event.
		/// </summary>
		private void OnAnimationEndedMelee()
		{
		}

		/// <summary>
		/// Inspect Animation Ended. This function is called from an Animation Event.
		/// </summary>
		private void OnAnimationEndedInspect()
		{
			//Notify the character.
			if(playerCharacter != null)
				playerCharacter.AnimationEndedInspect();
		}
		/// <summary>
		/// Holster Animation Ended. This function is called from an Animation Event.
		/// </summary>
		private void OnAnimationEndedHolster()
		{
			//Notify the character.
			if(playerCharacter != null)
				playerCharacter.AnimationEndedHolster();
		}

		/// <summary>
		/// Sets the character's equipped weapon's slide back pose. This function is called from an Animation Event.
		/// </summary>
		private void OnSlideBack(int back)
		{
		}

		#endregion
	}   
}