using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
public class GameManager : MonoBehaviour
{
	#region Variables
	[SerializeField] private Dungeon dungeonPrefab = default;
	[SerializeField] private GameObject playerPrefab = default;
	[SerializeField] private DungeonCell cellToSpawnPlayerOn = default;

	private Dungeon dungeonInstance;
	private GameObject playerInstance;
	#endregion

	#region Monobehaviour Callbacks
	private void Start()
	{
		StartCoroutine(BeginGame());
	}

	private void Update()
	{
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
		IntVector2 newGridSize = new IntVector2(PlayerPrefs.GetInt("GridSizeX"), PlayerPrefs.GetInt("GridSizeY"));

		playerInstance = Instantiate(playerPrefab) as GameObject;
		playerInstance.SetActive(false);

		dungeonInstance = Instantiate(dungeonPrefab) as Dungeon;
		dungeonInstance.Size = newGridSize;
		dungeonInstance.DoorProbability = PlayerPrefs.GetFloat("DoorProbability");
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