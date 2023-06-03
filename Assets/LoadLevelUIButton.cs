using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadLevelUIButton : MonoBehaviour
{
	public void LoadLevelByName(string levelName)
	{
		// Load the Scene with the specified name.
		SceneManager.LoadScene(levelName);
	}
}
