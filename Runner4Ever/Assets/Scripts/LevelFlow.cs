using UnityEngine;
using System.Collections;
using System;
using UnityEngine.SceneManagement;

/*
	Class that handles input, and triggers actions
*/
public class LevelFlow : MonoBehaviour
{
	
 	void Awake()
    {
    	//Debug.Log("StartListening");
        //EventManager.StartListening (GameConstants.playerSpawnEvent, startTrackingPlayer);
    }

    void OnEnable ()
    {
        Debug.Log("StartListening");
        EventManager.StartListening (GameConstants.playerSpawnEvent, startTrackingPlayer);
    }

    void OnDisable ()
    {
        EventManager.StopListening (GameConstants.playerSpawnEvent, startTrackingPlayer);
    }

    void startTrackingPlayer()
    {
        Debug.Log("startTrackingPlayer");
        var player = FindObjectOfType<CharacterController2D>();
        if(player == null)
        {
            Debug.LogError("Should have a player to start tracking");
        }
        else
        {
            TrackingManager.get().startLevel(player);
        }
    }

	void Start()
    {
        
    }

    void LateUpdate()
    {
        TrackingManager.get().updateMainGame();
    }
}