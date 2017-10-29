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
        EventManager.StartListening (EventManager.get().playerSpawnEvent, startTrackingPlayer);
        EventManager.StartListening (EventManager.get().hitCheckpointEvent, checkpointHit);
    }

    void OnDisable ()
    {
        EventManager.StopListening (EventManager.get().playerSpawnEvent, startTrackingPlayer);
        EventManager.StopListening (EventManager.get().hitCheckpointEvent, checkpointHit);
    }

    void checkpointHit(GameConstants.HitCheckpointArgument arg)
    {
        if(CheckpointUtils.findLastCheckpoint() == arg.checkpoint)
        {
            GameFlow.get().LoadMainMenu();
        }
    }

    void startTrackingPlayer(GameConstants.PlayerSpawnArgument arg)
    {
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