using UnityEngine;

public class GameManager : MonoBehaviour
{
	#region Variables
	[SerializeField] private Maze mazePrefab = default;
	private Maze mazeInstance;
	#endregion

	#region Monobehaviour Callbacks
	private void Start()
	{
		BeginGame();
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
	private void BeginGame()
	{
		mazeInstance = Instantiate(mazePrefab) as Maze;
		mazeInstance.Generate();
	}

	private void RestartGame()
	{
		Destroy(mazeInstance.gameObject);
		BeginGame();
	}
	#endregion
}