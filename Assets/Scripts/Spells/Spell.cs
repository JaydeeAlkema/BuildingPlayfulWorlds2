using UnityEngine;

public class Spell : MonoBehaviour
{
	#region Variables
	[SerializeField] private string spellName = default;                    // Name of the spell.
	[SerializeField] private float manaCost = default;                      // How much mana it costs to cast this spell.
	[SerializeField] private Transform target = default;                    // Where to move to.
	[SerializeField] private float lerpTime = default;                     // How fast to move to target.
	[SerializeField] private float damageToDeal = default;                  // How much damage to deal on impact.
	[SerializeField] private float impactRadius = default;                  // The radius of impact. So how close the spell has to come to the target for impact.
	[SerializeField] private GameObject impactParticles = default;          // Particle system to spawn on impact.

	[SerializeField] private Vector3 startingPos = default;                 // the starting position of the enemy.
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
		MoveToTarget();
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
