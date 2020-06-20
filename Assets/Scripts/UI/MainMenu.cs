using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// This class is responsible for the Main Menu. It handles all the buttons and transitions between the main menu and other menu's/scenes
/// </summary>
public class MainMenu : MonoBehaviour
{
	#region Functions
	/// <summary>
	/// Starts the main game scene.
	/// </summary>
	public void StartGame()
	{
		SceneManager.LoadScene(1);
	}

	/// <summary>
	/// Quits the game.
	/// </summary>
	public void QuitGame()
	{
		Application.Quit();
	}
	#endregion
}
