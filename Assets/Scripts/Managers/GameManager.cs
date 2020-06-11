using System.Collections;
using TMPro;

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public enum GameState
{
	Active,
	Paused,
	GameOver
}

public class GameManager : MonoBehaviour
{
	#region Variables
	private static GameManager instance = null;

	[SerializeField] private GameState gameState = GameState.Active;                // The state of the game.
	[Header("Dungeon and Player Properties")]
	[SerializeField] private GameObject minimapCam = default;                       // Reference to the Minimap Camera.
	[SerializeField] private Dungeon dungeonPrefab = default;                       // Reference to the dungeon prefab.
	[SerializeField] private GameObject playerPrefab = default;                     // Reference to the Player prefab.
	[SerializeField] private DungeonCell cellToSpawnPlayerOn = default;             // Which cell the player to spawn on.
	[Header("UI Properties")]
	[SerializeField] private Image playerHealthImage = default;                     // Reference to the Health UI Image Component.
	[SerializeField] private TextMeshProUGUI playerHealthImageText = default;       // Reference to the Health UI Text Component.
	[SerializeField] private Image playerManaImage = default;                       // Reference to the Mana UI Image Component.
	[SerializeField] private TextMeshProUGUI playerManaImageText = default;         // Reference to the Mana UI Text Component.
	[Header("Enemy Properties")]
	[SerializeField] private Transform enemyOnInstantiateParent = default;          // Reference to the parent transform for the newly spawned enemies.

	private Dungeon dungeonInstance = null;
	private GameObject playerInstance = null;
	#endregion

	#region Properties
	public static GameManager Instance { get => instance; set => instance = value; }

	public Dungeon DungeonInstance { get => dungeonInstance; set => dungeonInstance = value; }
	public GameObject PlayerInstance { get => playerInstance; set => playerInstance = value; }
	public GameState GameState { get => gameState; set => gameState = value; }
	public Transform EnemyOnInstantiateParent { get => enemyOnInstantiateParent; set => enemyOnInstantiateParent = value; }
	#endregion

	#region Monobehaviour Callbacks
	private void Awake()
	{
		if(!instance || instance != this) instance = this;
	}

	private void Start()
	{
		StartCoroutine(BeginGame());
	}

	private void Update()
	{
		if(playerInstance != null)
		{
			playerHealthImage.fillAmount = playerInstance.GetComponent<PlayerBehaviour>().Health / 100;
			playerHealthImageText.text = playerInstance.GetComponent<PlayerBehaviour>().Health.ToString();
			playerManaImage.fillAmount = playerInstance.GetComponent<PlayerBehaviour>().Mana / playerInstance.GetComponent<PlayerBehaviour>().MaxMana;
			playerManaImageText.text = playerInstance.GetComponent<PlayerBehaviour>().Mana.ToString();
		}

		if(playerInstance.GetComponent<PlayerBehaviour>().Health <= 0)
		{
			RestartGame();
		}

		if(Input.GetKeyDown(KeyCode.R))
		{
			RestartGame();
		}
	}

	private void OnApplicationQuit()
	{
		PlayerPrefs.SetInt("GridSizeX", 20);
		PlayerPrefs.SetInt("GridSizeY", 20);
		PlayerPrefs.SetFloat("DoorProbability", 0.015f);
	}
	#endregion

	#region Private Functions
	private IEnumerator BeginGame()
	{
		if(playerPrefab != null)
		{
			playerInstance = Instantiate(playerPrefab) as GameObject;
			playerInstance.SetActive(false);
		}

		if(dungeonPrefab != null)
		{
			dungeonInstance = Instantiate(dungeonPrefab) as Dungeon;
			yield return StartCoroutine(dungeonInstance.Generate());
			cellToSpawnPlayerOn = dungeonInstance.Rooms[0].Cells[Random.Range(0, dungeonInstance.Rooms[0].Cells.Count)];
		}

		if(playerInstance != null)
		{
			playerInstance.transform.position = new Vector3(cellToSpawnPlayerOn.transform.position.x, .25f, cellToSpawnPlayerOn.transform.position.z);
			playerInstance.SetActive(true);
			minimapCam.GetComponent<SmoothFollow>().Target = playerInstance.transform;
		}
	}
	private void RestartGame()
	{
		SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
	}
	#endregion
}