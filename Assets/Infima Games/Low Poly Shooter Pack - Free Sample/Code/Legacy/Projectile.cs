using System;
using UnityEngine;
using System.Collections;
using InfimaGames.LowPolyShooterPack;
using Random = UnityEngine.Random;

public class Projectile : MonoBehaviour {


	[Header("Damage Settings")]
    [Tooltip("Damage yang diberikan projectile")]
    [SerializeField]
    private float damage = 25f;

    private GameObject shooter; // Siapa yang menembak
	[Range(5, 100)]
	[Tooltip("After how long time should the bullet prefab be destroyed?")]
	public float destroyAfter;
	[Tooltip("If enabled the bullet destroys on impact")]
	public bool destroyOnImpact = false;
	[Tooltip("Minimum time after impact that the bullet is destroyed")]
	public float minDestroyTime;
	[Tooltip("Maximum time after impact that the bullet is destroyed")]
	public float maxDestroyTime;

	[Header("Impact Effect Prefabs")]
	public Transform [] bloodImpactPrefabs;
	public Transform [] metalImpactPrefabs;
	public Transform [] dirtImpactPrefabs;
	public Transform []	concreteImpactPrefabs;

	private void Start ()
	{
		//Grab the game mode service, we need it to access the player character!
		var gameModeService = ServiceLocator.Current.Get<IGameModeService>();
		//Ignore the main player character's collision. A little hacky, but it should work.
		Physics.IgnoreCollision(gameModeService.GetPlayerCharacter().GetComponent<Collider>(), GetComponent<Collider>());
		
		//Start destroy timer
		StartCoroutine (DestroyAfter ());
	}

	//If the bullet collides with anything
    private void OnCollisionEnter(Collision collision)
    {
        //Ignore collisions with other projectiles
        if (collision.gameObject.GetComponent<Projectile>() != null)
            return;

        // ========================================
        // BARU: APPLY DAMAGE
        // ========================================
        
        Vector3 hitPoint = collision.contacts[0].point;
        Vector3 hitNormal = collision.contacts[0].normal;
        
        // Try to apply damage
        TryApplyDamage(collision.gameObject, hitPoint, hitNormal);

        // ========================================
        // EXISTING: IMPACT EFFECTS
        // ========================================
        
        HandleImpactEffects(collision, hitPoint, hitNormal);

        // ========================================
        // EXISTING: DESTRUCTION
        // ========================================
        
        if (!destroyOnImpact)
        {
            StartCoroutine(DestroyTimer());
        }
        else
        {
            Destroy(gameObject);
        }
    }

    

    /// <summary>
    /// Try to apply damage ke object yang kena hit
    /// </summary>
    private void TryApplyDamage(GameObject target, Vector3 hitPoint, Vector3 hitNormal)
    {
        // 1. Cek Hitbox (untuk headshot, limb shots)
        Hitbox hitbox = target.GetComponent<Hitbox>();
        if (hitbox != null)
        {
            hitbox.ApplyDamage(damage, shooter, hitPoint, hitNormal);
            Debug.Log($"Hit {target.name} via Hitbox! Damage: {damage * hitbox.GetDamageMultiplier()}");
            return;
        }

        // 2. Cek IDamageable langsung
        IDamageable damageable = target.GetComponent<IDamageable>();
        if (damageable != null)
        {
            damageable.TakeDamage(damage, shooter);
            Debug.Log($"Hit {target.name}! Damage: {damage}");
            return;
        }

        // 3. Cek parent (untuk nested colliders)
        damageable = target.GetComponentInParent<IDamageable>();
        if (damageable != null)
        {
            damageable.TakeDamage(damage, shooter);
            Debug.Log($"Hit {target.name} (parent)! Damage: {damage}");
            return;
        }

        // 4. No health component found - ini object biasa (wall, ground, dll)
        // Do nothing, tetap spawn impact effect
    }

    /// <summary>
    /// Handle impact effects berdasarkan tag
    /// </summary>
    private void HandleImpactEffects(Collision collision, Vector3 hitPoint, Vector3 hitNormal)
    {
        string tag = collision.transform.tag;

        // Blood impact
        if (tag == "Blood" && bloodImpactPrefabs.Length > 0)
        {
            Instantiate(
                bloodImpactPrefabs[Random.Range(0, bloodImpactPrefabs.Length)],
                hitPoint,
                Quaternion.LookRotation(hitNormal)
            );
            Destroy(gameObject);
        }
        // Metal impact
        else if (tag == "Metal" && metalImpactPrefabs.Length > 0)
        {
            Instantiate(
                metalImpactPrefabs[Random.Range(0, metalImpactPrefabs.Length)],
                hitPoint,
                Quaternion.LookRotation(hitNormal)
            );
            Destroy(gameObject);
        }
        // Dirt impact
        else if (tag == "Dirt" && dirtImpactPrefabs.Length > 0)
        {
            Instantiate(
                dirtImpactPrefabs[Random.Range(0, dirtImpactPrefabs.Length)],
                hitPoint,
                Quaternion.LookRotation(hitNormal)
            );
            Destroy(gameObject);
        }
        // Concrete impact
        else if (tag == "Concrete" && concreteImpactPrefabs.Length > 0)
        {
            Instantiate(
                concreteImpactPrefabs[Random.Range(0, concreteImpactPrefabs.Length)],
                hitPoint,
                Quaternion.LookRotation(hitNormal)
            );
            Destroy(gameObject);
        }
        // Target
        else if (tag == "Target")
        {
            TargetScript target = collision.transform.GetComponent<TargetScript>();
            if (target != null)
            {
                target.isHit = true;
            }
            Destroy(gameObject);
        }
        // ExplosiveBarrel
        else if (tag == "ExplosiveBarrel")
        {
            ExplosiveBarrelScript barrel = collision.transform.GetComponent<ExplosiveBarrelScript>();
            if (barrel != null)
            {
                barrel.explode = true;
            }
            Destroy(gameObject);
        }
        // GasTank
        else if (tag == "GasTank")
        {
            GasTankScript gasTank = collision.transform.GetComponent<GasTankScript>();
            if (gasTank != null)
            {
                gasTank.isHit = true;
            }
            Destroy(gameObject);
        }
    }

	    /// <summary>
    /// Set siapa yang menembak (dipanggil dari Weapon.Fire())
    /// </summary>
    public void SetShooter(GameObject shooterObject)
    {
        shooter = shooterObject;
    }

    /// <summary>
    /// Set custom damage (optional)
    /// </summary>
    public void SetDamage(float damageAmount)
    {
        damage = damageAmount;
    }


	private IEnumerator DestroyTimer () 
	{
		//Wait random time based on min and max values
		yield return new WaitForSeconds
			(Random.Range(minDestroyTime, maxDestroyTime));
		//Destroy bullet object
		Destroy(gameObject);
	}

	private IEnumerator DestroyAfter () 
	{
		//Wait for set amount of time
		yield return new WaitForSeconds (destroyAfter);
		//Destroy bullet object
		Destroy (gameObject);
	}
}