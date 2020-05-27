using UnityEngine;

public class Spell : MonoBehaviour
{
	#region Variables
	[SerializeField] private string spellName = default;                    // Name of the spell.
	[SerializeField] private float manaCost = default;						// How much mana it costs to cast this spell.
	[SerializeField] private Transform target = default;                    // Where to move to.
	[SerializeField] private float moveSpeed = default;                     // How fast to move to target.
	[SerializeField] private float damageToDeal = default;                  // How much damage to deal on impact.
	[SerializeField] private float impactRadius = default;                  // The radius of impact. So how close the spell has to come to the target for impact.
	[SerializeField] private GameObject impactParticles = default;          // Particle system to spawn on impact.

	private bool isActive = true;                                           // If the spell is active.
	#endregion

	#region Properties
	public Transform Target { get => target; set => target = value; }
	public float ManaCost { get => manaCost; set => manaCost = value; }
	#endregion

	//private void Initialize(string _spellName, Transform _target, float _movespeed, float _damageToDeal, float _impactRadius)
	//{
	//	spellName = _spellName;
	//	target = _target;
	//	moveSpeed = _movespeed;
	//	damageToDeal = _damageToDeal;
	//	impactRadius = _impactRadius;
	//}

	private void Update()
	{
		MoveToTarget();
	}

	private void MoveToTarget()
	{
		if(target && isActive)
		{
			transform.position = Vector3.Lerp(transform.position, target.position, moveSpeed * Time.deltaTime);
			if(Vector3.Distance(transform.position, target.position) <= impactRadius)
			{
				target.GetComponent<IDamageable>()?.Damage(damageToDeal);
				Instantiate(impactParticles, target.position, Quaternion.identity);
				isActive = false;
				Destroy(gameObject, 5);
			}
		}
	}
}
