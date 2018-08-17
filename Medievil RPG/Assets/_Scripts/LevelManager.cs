using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
	[SerializeField] float autoLoadNextLevelAfter = 0;

	public void LoadLevel( string name )
	{
		SceneManager.LoadScene( name );
	}

	public void LoadNextLevel()
	{
		SceneManager.LoadScene( SceneManager.GetActiveScene().buildIndex + 1 );
	}

	public void QuitRequest()
	{
		Application.Quit();
	}

	void Start()
	{
		if( autoLoadNextLevelAfter != 0 )
		{
			Invoke( "LoadNextLevel", autoLoadNextLevelAfter );
		}		
	}
}