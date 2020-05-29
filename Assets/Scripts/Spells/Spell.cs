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
	[SerializeField] private float manaCost = default;                      // How much mana it costs to cast this spell.

	[Header("Base Variables")]
	[SerializeField] private float damageToDeal = default;                  // How much damage to deal on impact.

	[Header("Target Based Spell")]
	[SerializeField] private Transform target = default;                    // Where to move to.
	[SerializeField] private float lerpTime = default;                      // How fast to move to target.
	[SerializeField] private GameObject impactParticles = default;          // Particle system to spawn on impact.
	[SerializeField] private Vector3 startingPos = default;                 // the starting position of the enemy.

	[Header("Ground Based Spell")]
	[SerializeField] private float spellDuration = default;                 // How long the spell lasts untill it stops.
	[SerializeField] private float damageInterval = default;                // How much time between damage ticks.
	[SerializeField] private float impactRadius = default;                  // The radius of impact. So how close the spell has to come to the target for impact.
	[SerializeField] private GameObject[] targetsInRadius = default;        // Array with all the targets within the radius.

	private float timeStartedLerping;
	private bool isActive = true;                                           // If the spell is active.
	#endregion

	#region Properties
	public Transform Target { get => target; set => target = value; }
	public float ManaCost { get => manaCost; set => manaCost = value; }
	#endregion

	private void Start()
	{
		startingPos = transform.position;
		timeStartedLerping = Time.time;
	}

	private void Update()
	{
		if(spellType == SpellType.TargetBased) MoveToTarget();
	}

	private IEnumerator GroundedBasedDamageSpell()
	{
		// Do stuff
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
}
