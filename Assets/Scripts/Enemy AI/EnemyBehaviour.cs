using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public enum EnemyState
{
	Idle,
	Chasing,
	Attacking
}

public class EnemyBehaviour : MonoBehaviour, IDamageable
{
	#region Variables
	[SerializeField] private float health = default;                                // How much health the enemy has.
	[SerializeField] private float damage = default;                                // How much damage to deal to the player.
	[SerializeField] private int manaWorth = default;                               // How much Mana the player gets back for killing an enemy.
	[Space]
	[SerializeField] private EnemyState state = EnemyState.Idle;                    // State of the Enemy.
	[SerializeField] private NavMeshAgent agent = default;                          // Reference to the NavMeshAgent component on the Enemy Gameobject.
	[SerializeField] private MeshRenderer renderer = default;                       // Reference to the Mesh Renderer component.
	[Header("Movement Properties")]
	[SerializeField] private float walkSpeed = 3.5f;                                // Walking speed of the Enemy.
	[Header("Targeting Properties")]
	[SerializeField] private LayerMask targetMask = default;                        // Which layers to check for target.
	[SerializeField] private Transform target = null;                               // Reference to the target transform.
	[SerializeField] private float targetDestinationUpdateInterval = 0.1f;          // How often the destination will be set for the AI.
	[SerializeField] private float targetDetectionCheckInterval = 0.1f;             // How much time between target detection.
	[SerializeField] private float targetInteractionInterval = 1f;                  // How much time between attacks.

	[SerializeField] private float targetDetectionRadius = 15f;                     // How far the Enemy can "See".
	[SerializeField] private float targetInteractionRadius = 1f;                    // How far the Enemy can "Interact" with the target.
	[Header("Debugging Materials")]
	[SerializeField] private Material idleMat = default;                            // Which Material to show when Idle.
	[SerializeField] private Material chasingMat = default;                         // Which Material to show when Chasing.
	[SerializeField] private Material attackingMat = default;                       // Which Material to show when Attacking.
	#endregion

	#region Properties

	#endregion

	#region Monobehaviour Callbacks
	private void Start()
	{
		agent.speed = walkSpeed;

		GetComponent<Outline>().enabled = false;
	}

	private void OnEnable()
	{
		StartCoroutine(SearchForTarget());
		StartCoroutine(MoveToTarget());
		StartCoroutine(AttackTarget());
	}
	#endregion

	#region Functions
	private IEnumerator SearchForTarget()
	{
		while(true)
		{
			if(state == EnemyState.Idle)
			{
				agent.isStopped = false;
				renderer.material = idleMat;
				if(!target) target = GameManager.Instance.PlayerInstance.transform;
				else state = EnemyState.Chasing;
				Debug.Log("[" + gameObject.name + "]" + " Searching for Target");
			}
			yield return new WaitForSeconds(targetDetectionCheckInterval);
		}
	}

	private IEnumerator MoveToTarget()
	{
		while(true)
		{
			if(state == EnemyState.Chasing)
			{
				agent.isStopped = false;
				renderer.material = chasingMat;
				agent.destination = target.position;
				Debug.Log("[" + gameObject.name + "]" + " Moving Towards Target");

				if(Vector3.Distance(transform.position, target.position) < targetInteractionRadius) state = EnemyState.Attacking;
			}
			yield return new WaitForSeconds(targetDestinationUpdateInterval);
		}
	}

	private IEnumerator AttackTarget()
	{
		while(true)
		{
			yield return new WaitForSeconds(targetInteractionInterval);
			if(state == EnemyState.Attacking)
			{
				agent.isStopped = true;
				renderer.material = attackingMat;
				target.GetComponent<IDamageable>()?.Damage(damage);

				Debug.Log("[" + gameObject.name + "]" + " Attacking Target");
				if(Vector3.Distance(transform.position, target.position) > targetInteractionRadius) state = EnemyState.Chasing;
			}
		}
	}
	#endregion

	#region Debugging
	private void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.blue;
		Gizmos.DrawWireSphere(transform.position, targetDetectionRadius * transform.localScale.x);

		Gizmos.color = Color.red;
		Gizmos.DrawWireSphere(transform.position, targetInteractionRadius * transform.localScale.x);
	}

	void IDamageable.Damage(float damage)
	{
		health -= damage;
		if(health <= 0)
		{
			GameManager.Instance.PlayerInstance.GetComponent<PlayerBehaviour>().Mana += manaWorth;
			Destroy(gameObject);
		}
		#endregion
	}
}
