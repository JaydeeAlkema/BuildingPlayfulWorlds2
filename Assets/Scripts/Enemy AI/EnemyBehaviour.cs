using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public enum EnemyState
{
	Idle,
	Chasing,
	Attacking
}

public class EnemyBehaviour : MonoBehaviour
{
	#region Variables
	[SerializeField] private EnemyState state = EnemyState.Idle;                    // State of the Enemy.
	[SerializeField] private NavMeshAgent agent = default;                          // Reference to the NavMeshAgent component on the Enemy Gameobject.
	[Header("Movement Properties")]
	[SerializeField] private float walkSpeed = 3.5f;                                // Walking speed of the Enemy.
	[Header("Targeting Properties")]
	[SerializeField] private LayerMask targetMask = default;                        // Which layers to check for target.
	[SerializeField] private Transform target = null;                               // Reference to the target transform.
	[SerializeField] private float targetDestinationUpdateInterval = 0.1f;          // How often the destination will be set for the AI.
	[SerializeField] private float targetDetectionRadius = 15f;                     // How far the Enemy can "See".
	[SerializeField] private float targetInteractionRadius = 1f;                    // How far the Enemy can "Interact" with the target.
	#endregion

	#region Properties

	#endregion

	#region Monobehaviour Callbacks
	private void Start()
	{
		OnStateChangeEvent();
	}
	#endregion

	#region Functions
	private void OnStateChangeEvent()
	{
		switch(state)
		{
			case EnemyState.Idle:
				StartCoroutine(SearchForTarget());
				break;
			case EnemyState.Chasing:
				StartCoroutine(MoveToTarget());
				break;
			case EnemyState.Attacking:
				StartCoroutine(AttackTarget());
				break;
			default:
				break;
		}
	}

	private IEnumerator SearchForTarget()
	{
		while(true)
		{
			if(!target)
			{
				target = GameManager.Instance.PlayerInstance.transform;
			}
			else
				state = EnemyState.Chasing;

			OnStateChangeEvent();
			yield return null;
		}
	}

	private IEnumerator MoveToTarget()
	{
		while(true)
		{
			agent.destination = target.position;

			OnStateChangeEvent();
			yield return new WaitForSeconds(targetDestinationUpdateInterval);
		}
	}

	private IEnumerator AttackTarget()
	{
		while(true)
		{

			OnStateChangeEvent();
			yield return null;
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
	#endregion
}
