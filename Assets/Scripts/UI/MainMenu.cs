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
	public void StartGame()
	{
		SceneManager.LoadScene(1);
	}

	public void ToggleOptionsMenu()
	{
		// To be implemented
	}

	public void QuitGame()
	{
		Application.Quit();
	}
	#endregion
}
