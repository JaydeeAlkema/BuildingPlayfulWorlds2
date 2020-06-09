using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public enum SpellType
{
	TargetBased,
	GroundBased
}

/// <summary>
/// A spell class that handles... Well, being a spell.
/// This really should be a base class where I inherit from. But lazyness got the best of me.
/// </summary>
public class Spell : MonoBehaviour
{
	#region Variables
	[SerializeField] private SpellType spellType = SpellType.TargetBased;   // Type of the spell.
	[SerializeField] private string spellName = default;                    // Name of the spell.
	[SerializeField] private LayerMask targetMask = default;                // The layer for the enemies.

	[Header("Base Variables")]
	[SerializeField] private float damageToDeal = default;                  // How much damage to deal on impact.

	[Header("Target Based Spell")]
	[SerializeField] private Transform target = default;                    // Where to move to.
	[SerializeField] private float lerpTime = default;                      // How fast to move to target.
	[SerializeField] private GameObject impactParticles = default;          // Particle system to spawn on impact.
	[SerializeField] private Vector3 startingPos = default;                 // the starting position of the enemy.

	[Header("Ground Based Spell")]
	[SerializeField] private float spellTick = 1f;                          // How often per second it ticks.
	[SerializeField] private float spellDuration = default;                 // How long the spell lasts untill it stops.
	[SerializeField] private float impactRadius = default;                  // The radius of impact. So how close the spell has to come to the target for impact.
	[SerializeField] private List<GameObject> targetsInRadius = new List<GameObject>();        // Array with all the targets within the radius.

	private float timeLeftToTick;
	private float timeStartedLerping;
	private bool isActive = true;                                           // If the spell is active.
	#endregion

	#region Properties
	public Transform Target { get => target; set => target = value; }
	public SpellType SpellType { get => spellType; set => spellType = value; }
	#endregion

	#region Monobehaviour Callbacks
	private void Start()
	{
		if(spellType == SpellType.TargetBased)
		{
			startingPos = transform.position;
			timeStartedLerping = Time.time;
		}
		else if(spellType == SpellType.GroundBased)
		{
			timeLeftToTick = spellDuration;
			StartCoroutine(GetAllEnemiesWithinSpellRadius());
			StartCoroutine(DamageEnemiesWithinSpellRadius());
		}
	}

	private void Update()
	{
		switch(spellType)
		{
			case SpellType.TargetBased:
				MoveToTarget();
				break;

			case SpellType.GroundBased:
				if(timeLeftToTick <= 0) Destroy(gameObject, 3f);
				break;

			default:
				break;
		}
	}
	#endregion

	#region Functions
	/// <summary>
	/// Gets all the enemies withing a certain radius.
	/// This gets send to the targets in radius list. This list is cleared each time we check for targets.
	/// </summary>
	/// <returns></returns>
	private IEnumerator GetAllEnemiesWithinSpellRadius()
	{
		while(timeLeftToTick > 0)
		{
			targetsInRadius.Clear();
			Collider[] targetsWithinRadius = Physics.OverlapSphere(transform.position, impactRadius, targetMask);

			foreach(Collider target in targetsWithinRadius)
			{
				targetsInRadius.Add(target.gameObject);
			}
			timeLeftToTick -= 1f;
			yield return new WaitForSeconds(spellTick);
		}
		yield return null;
	}

	/// <summary>
	/// Damages all the enemies in the Targets in radius list.
	/// </summary>
	/// <returns></returns>
	private IEnumerator DamageEnemiesWithinSpellRadius()
	{
		while(timeLeftToTick > 0)
		{
			foreach(GameObject enemy in targetsInRadius)
			{
				enemy.GetComponent<IDamageable>()?.Damage(damageToDeal);
				enemy.GetComponent<IDamageable>()?.ImpactMovementSpeed(enemy.GetComponent<AIBehaviour>().MoveSpeed / 3f);
			}
			yield return new WaitForSeconds(spellTick);
		}
		yield return null;
	}

	/// <summary>
	/// Moves cleanly to the target
	/// </summary>
	private void MoveToTarget()
	{
		if(target && isActive)
		{
			transform.position = CleanLerp(startingPos, target.position, timeStartedLerping, lerpTime);

			if(Vector3.Distance(transform.position, target.position) <= impactRadius)
			{
				Instantiate(impactParticles, target.position, Quaternion.identity);
				target.GetComponent<IDamageable>()?.Damage(damageToDeal);
				isActive = false;
				Destroy(gameObject, 5);
			}
		}
	}

	/// <summary>
	/// Cleanly lerps the enemy from the starting point to the end point.
	/// </summary>
	/// <param name="startPos"></param>
	/// <param name="endPos"></param>
	/// <param name="timeStartedLerping"></param>
	/// <param name="lerptime"></param>
	/// <returns></returns>
	private Vector3 CleanLerp(Vector3 startPos, Vector3 endPos, float timeStartedLerping, float lerptime = 1)
	{
		float timeSinceStarted = Time.time - timeStartedLerping;
		float percentageComplete = timeSinceStarted / lerptime;

		Vector3 result = Vector3.Lerp(startPos, endPos, percentageComplete);
		return result;
	}
	#endregion

	#region Debug
	private void OnDrawGizmos()
	{
		Gizmos.color = Color.red;
		Gizmos.DrawWireSphere(transform.position, impactRadius);
	}
	#endregion
}
