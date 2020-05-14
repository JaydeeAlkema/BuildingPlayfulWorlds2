using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
public class GameManager : MonoBehaviour
{
	#region Variables
	[SerializeField] private Maze mazePrefab = default;
	[SerializeField] private GameObject playerPrefab = default;
	[SerializeField] private MazeCell cellToSpawnPlayerOn = default;
	[Space]
	[SerializeField] private TextMeshProUGUI gridSizeXTextMesh = default;
	[SerializeField] private TextMeshProUGUI gridSizeYTextMesh = default;
	[SerializeField] private TextMeshProUGUI doorProbabilityTextMesh = default;

	private Maze mazeInstance;
	private GameObject playerInstance;
	#endregion

	#region Monobehaviour Callbacks
	private void Start()
	{
		StartCoroutine(BeginGame());
		StartCoroutine(UpdateUI());
	}

	private void Update()
	{
		if(Input.GetKeyDown(KeyCode.Space))
		{
			RestartGame();
		}

		ChangeMazeSizeOnInput();
	}

	private void OnApplicationQuit()
	{
		PlayerPrefs.SetInt("GridSizeX", 20);
		PlayerPrefs.SetInt("GridSizeY", 20);
		PlayerPrefs.SetFloat("DoorProbability", 0.02f);
	}
	#endregion

	#region Private Functions
	private IEnumerator BeginGame()
	{
		IntVector2 newGridSize = new IntVector2(PlayerPrefs.GetInt("GridSizeX"), PlayerPrefs.GetInt("GridSizeY"));

		playerInstance = Instantiate(playerPrefab) as GameObject;
		playerInstance.SetActive(false);

		mazeInstance = Instantiate(mazePrefab) as Maze;
		mazeInstance.Size = newGridSize;
		mazeInstance.DoorProbability = PlayerPrefs.GetFloat("DoorProbability");
		yield return StartCoroutine(mazeInstance.Generate());
		cellToSpawnPlayerOn = mazeInstance.Rooms[0].Cells[Random.Range(0, mazeInstance.Rooms[0].Cells.Count)];

		playerInstance.transform.position = new Vector3(cellToSpawnPlayerOn.transform.position.x, .25f, cellToSpawnPlayerOn.transform.position.z);
		playerInstance.SetActive(true);
	}
	private void RestartGame()
	{
		SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
	}

	private void ChangeMazeSizeOnInput()
	{
		if(Input.GetKeyDown(KeyCode.O)) PlayerPrefs.SetInt("GridSizeX", PlayerPrefs.GetInt("GridSizeX") - 5);
		if(Input.GetKeyDown(KeyCode.P)) PlayerPrefs.SetInt("GridSizeX", PlayerPrefs.GetInt("GridSizeX") + 5);
		if(Input.GetKeyDown(KeyCode.K)) PlayerPrefs.SetInt("GridSizeY", PlayerPrefs.GetInt("GridSizeY") - 5);
		if(Input.GetKeyDown(KeyCode.L)) PlayerPrefs.SetInt("GridSizeY", PlayerPrefs.GetInt("GridSizeY") + 5);
		if(Input.GetKeyDown(KeyCode.N)) PlayerPrefs.SetFloat("DoorProbability", PlayerPrefs.GetFloat("DoorProbability") - 0.0025f);
		if(Input.GetKeyDown(KeyCode.M)) PlayerPrefs.SetFloat("DoorProbability", PlayerPrefs.GetFloat("DoorProbability") + 0.0025f);

		if(PlayerPrefs.GetInt("GridSizeX") <= 0) PlayerPrefs.SetInt("GridSizeX", 5);
		if(PlayerPrefs.GetInt("GridSizeX") >= 250) PlayerPrefs.SetInt("GridSizeX", 250);
		if(PlayerPrefs.GetInt("GridSizeY") <= 0) PlayerPrefs.SetInt("GridSizeY", 5);
		if(PlayerPrefs.GetInt("GridSizeY") >= 250) PlayerPrefs.SetInt("GridSizeY", 250);
		if(PlayerPrefs.GetFloat("DoorProbability") <= 0.005f) PlayerPrefs.SetFloat("DoorProbability", 0.005f);
		if(PlayerPrefs.GetFloat("DoorProbability") >= 0.1f) PlayerPrefs.SetFloat("DoorProbability", 0.1f);
	}

	private IEnumerator UpdateUI()
	{
		while(true)
		{
			yield return new WaitForSeconds(0.1f);
			gridSizeXTextMesh.text = "Grid Size X: " + PlayerPrefs.GetInt("GridSizeX");
			gridSizeYTextMesh.text = "Grid Size Y: " + PlayerPrefs.GetInt("GridSizeY");
			doorProbabilityTextMesh.text = "Door Probability: " + PlayerPrefs.GetFloat("DoorProbability");
		}
	}
	#endregion
}