using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicLoader : MonoBehaviour
{
	[SerializeField] AudioClip[] levelMusic;

	AudioSource audioSource;

	void Awake()
	{
		DontDestroyOnLoad( gameObject );
		audioSource = GetComponent<AudioSource> ();
	}
	
	void OnLevelWasLoaded( int level )
	{
		if ( levelMusic [level] )
		{
			audioSource.clip = levelMusic [level];
			audioSource.loop = true;
			audioSource.Play();
		}
	}
}
