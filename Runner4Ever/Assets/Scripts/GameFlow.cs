using UnityEngine;
using System.Collections;
using System;
using UnityEngine.SceneManagement;

/*
	Class that handles input, and triggers actions
*/
public class GameFlow : MonoBehaviour
{
	static protected GameFlow s_Instance;
	static public GameFlow instance { get { return s_Instance; } }

 	void Awake()
    {
    	if (s_Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        s_Instance = this;
        
    	DontDestroyOnLoad(gameObject);
    }

	public void LoadLevel(string name)
    {
        SceneManager.LoadScene(name);
    }

    public void LoadMainGame()
    {
    	LoadLevel("MainGame");
    }
}