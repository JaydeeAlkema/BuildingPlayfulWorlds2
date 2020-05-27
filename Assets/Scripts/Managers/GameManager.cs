using System.Collections;
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
	[Space]
	[SerializeField] private Dungeon dungeonPrefab = default;
	[SerializeField] private GameObject playerPrefab = default;
	[SerializeField] private DungeonCell cellToSpawnPlayerOn = default;
	[Space]
	[SerializeField] private Image playerHealthImage = default;                                   // Reference to the Health UI Image Component.
	[SerializeField] private Image playerManaImage = default;                                     // Reference to the Mana UI Image Component.

	private Dungeon dungeonInstance;
	private GameObject playerInstance;
	#endregion

	#region Properties
	public static GameManager Instance { get => instance; set => instance = value; }

	public Dungeon DungeonInstance { get => dungeonInstance; set => dungeonInstance = value; }
	public GameObject PlayerInstance { get => playerInstance; set => playerInstance = value; }
	public GameState GameState { get => gameState; set => gameState = value; }
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
			playerManaImage.fillAmount = playerInstance.GetComponent<PlayerBehaviour>().Mana / playerInstance.GetComponent<PlayerBehaviour>().MaxMana;
		}

		if(playerInstance.GetComponent<PlayerBehaviour>().Health <= 0)
		{
			RestartGame();
		}

		if(Input.GetKeyDown(KeyCode.Space))
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
		playerInstance = Instantiate(playerPrefab) as GameObject;
		playerInstance.SetActive(false);

		dungeonInstance = Instantiate(dungeonPrefab) as Dungeon;
		yield return StartCoroutine(dungeonInstance.Generate());
		cellToSpawnPlayerOn = dungeonInstance.Rooms[0].Cells[Random.Range(0, dungeonInstance.Rooms[0].Cells.Count)];

		playerInstance.transform.position = new Vector3(cellToSpawnPlayerOn.transform.position.x, .25f, cellToSpawnPlayerOn.transform.position.z);
		playerInstance.SetActive(true);
	}
	private void RestartGame()
	{
		SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
	}
	#endregion
}