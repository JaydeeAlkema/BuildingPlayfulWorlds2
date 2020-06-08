using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public enum EnemyState
{
	Idle,
	Chasing,
	Attacking,
	Dead
}

public class EnemyBehaviour : MonoBehaviour, IDamageable
{
	#region Variables
	[SerializeField] private float health = default;                                // How much health the enemy has.
	[SerializeField] private float damage = default;                                // How much damage to deal to the player.
	[SerializeField] private int manaWorth = default;                               // How much Mana the player gets back for killing an enemy.

	[Header("Core Properties")]
	[SerializeField] private EnemyState state = EnemyState.Idle;                    // State of the Enemy.
	[SerializeField] private NavMeshAgent agent = default;                          // Reference to the NavMeshAgent component on the Enemy Gameobject.
	[SerializeField] private Animator anim = default;                               // Reference to the animator component.

	[Header("Movement Properties")]
	[SerializeField] private float moveSpeed = 3.5f;                                // Walking speed of the Enemy.
	[SerializeField] private AudioSource audioSource = default;                     // Reference to the audio source component.
	[SerializeField] private AudioClip[] walkSoundAudioClips = default;             // Array with all the walk sound effects.

	[Header("Targeting Properties")]
	[SerializeField] private LayerMask targetMask = default;                        // Which layers to check for target.
	[SerializeField] private Transform target = null;                               // Reference to the target transform.
	[SerializeField] private float targetDestinationUpdateInterval = 0.1f;          // How often the destination will be set for the AI.
	[SerializeField] private float targetDetectionCheckInterval = 0.1f;             // How much time between target detection.
	[SerializeField] private float targetInteractionInterval = 1f;                  // How much time between attacks.
	[SerializeField] private float targetDetectionRadius = 15f;                     // How far the Enemy can "See".
	[SerializeField] private float targetInteractionRadius = 1f;                    // How far the Enemy can "Interact" with the target.

	private float maxMoveSpeed;
	#endregion

	#region Properties
	public float MoveSpeed { get => moveSpeed; set => moveSpeed = value; }
	public EnemyState State { get => state; set => state = value; }
	#endregion

	#region Monobehaviour Callbacks
	private void Start()
	{
		agent.speed = moveSpeed;
		maxMoveSpeed = moveSpeed;

		GetComponent<Outline>().enabled = false;

		StartCoroutine(PlayMovementAudio());
	}

	private void Update()
	{
		if(state != EnemyState.Dead)
		{
			anim.SetFloat("Velocity", agent.velocity.magnitude);
			anim.SetBool("Attacking", state == EnemyState.Attacking ? true : false);
		}
	}

	private void OnEnable()
	{
		StartCoroutine(SearchForTarget());
		StartCoroutine(MoveToTarget());
		StartCoroutine(AttackTarget());
	}
	#endregion

	#region Functions
	/// <summary>
	/// Searches for the nearest target.
	/// As of now it just gets the player from the Game Manager.
	/// </summary>
	/// <returns></returns>
	private IEnumerator SearchForTarget()
	{
		while(true)
		{
			if(state == EnemyState.Idle)
			{
				agent.isStopped = false;
				if(!target) target = GameManager.Instance.PlayerInstance.transform;
				else state = EnemyState.Chasing;
				Debug.Log("[" + gameObject.name + "]" + " Searching for Target");
			}
			yield return new WaitForSeconds(targetDetectionCheckInterval);
		}
	}

	/// <summary>
	/// Moves the enemy to the target at a set interval.
	/// </summary>
	/// <returns></returns>
	private IEnumerator MoveToTarget()
	{
		while(true)
		{
			if(state == EnemyState.Chasing)
			{
				agent.isStopped = false;
				agent.speed = moveSpeed;
				agent.destination = target.position;
				Debug.Log("[" + gameObject.name + "]" + " Moving Towards Target");

				if(Vector3.Distance(transform.position, target.position) < targetInteractionRadius) state = EnemyState.Attacking;
			}
			yield return new WaitForSeconds(targetDestinationUpdateInterval);
		}
	}

	/// <summary>
	/// Attacks the target at a set interval
	/// </summary>
	/// <returns></returns>
	private IEnumerator AttackTarget()
	{
		while(true)
		{
			yield return new WaitForSeconds(targetInteractionInterval);
			if(state == EnemyState.Attacking)
			{
				agent.isStopped = true;
				target.GetComponent<IDamageable>()?.Damage(damage);

				Debug.Log("[" + gameObject.name + "]" + " Attacking Target");
				if(Vector3.Distance(transform.position, target.position) > targetInteractionRadius) state = EnemyState.Chasing;
			}
			yield return null;
		}
	}

	/// <summary>
	/// Plays movement audio depending on the movement speed.
	/// </summary>
	/// <returns></returns>
	private IEnumerator PlayMovementAudio()
	{
		while(true)
		{
			if(state == EnemyState.Chasing)
			{
				int randIndex = Random.Range(0, walkSoundAudioClips.Length);
				audioSource.PlayOneShot(walkSoundAudioClips[randIndex], 0.1f);
				yield return new WaitForSeconds(0.35f);
			}
			yield return null;
		}
	}

	/// <summary>
	/// IDamageable Damage Implementation
	/// Decreases the health and handles what needs to happen when health falls bellow 0.
	/// </summary>
	/// <param name="damage"></param>
	void IDamageable.Damage(float damage)
	{
		health -= damage;
		if(health <= 0)
		{
			state = EnemyState.Dead;
			GameManager.Instance.PlayerInstance.GetComponent<PlayerBehaviour>().Mana += manaWorth;
			anim.SetBool("Dead", true);
			gameObject.name = gameObject.name + " -DEAD-";
			RemoveAllActiveComponents();
		}
	}

	/// <summary>
	/// IDamageable Impact Movement Speed Implementation.
	/// This slows the enemy down to a crawl.
	/// </summary>
	/// <param name="value"></param>
	void IDamageable.ImpactMovementSpeed(float value)
	{
		moveSpeed = value;
		agent.speed = moveSpeed;
	}

	/// <summary>
	/// Removes all the active components once the Enemy is considered Dead.
	/// This is done so the enemy carcass can still exist in the world without having all the code still run on the background.
	/// </summary>
	private void RemoveAllActiveComponents()
	{
		target = null;
		agent.isStopped = true;
		Destroy(GetComponent<Outline>());
		Destroy(GetComponent<CapsuleCollider>());
		Destroy(GetComponent<NavMeshAgent>());
		Destroy(this);
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
