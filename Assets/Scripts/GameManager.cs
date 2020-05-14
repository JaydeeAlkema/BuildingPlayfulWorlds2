using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
	#region Variables
	[SerializeField] private Maze mazePrefab = default;
	[SerializeField] private GameObject playerPrefab = default;
	[SerializeField] private MazeCell cellToSpawnPlayerOn = default;

	private Maze mazeInstance;
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
	#endregion

	#region Private Functions
	private IEnumerator BeginGame()
	{
		playerInstance = Instantiate(playerPrefab) as GameObject;
		playerInstance.SetActive(false);

		mazeInstance = Instantiate(mazePrefab) as Maze;
		yield return StartCoroutine(mazeInstance.Generate());
		cellToSpawnPlayerOn = mazeInstance.Rooms[0].Cells[Random.Range(0, mazeInstance.Rooms[0].Cells.Count)];

		playerInstance.transform.position = new Vector3(cellToSpawnPlayerOn.transform.position.x, .25f, cellToSpawnPlayerOn.transform.position.z);
		playerInstance.SetActive(true);
	}

	private void RestartGame()
	{
		SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
	}
	#endregion
}