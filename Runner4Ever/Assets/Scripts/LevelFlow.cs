using UnityEngine;
using System.Collections;
using System;
using UnityEngine.SceneManagement;

/*
	Class that handles input, and triggers actions
*/
public class LevelFlow : MonoBehaviour
{
    private GameObject lastHitCheckpoint;
    public GameObject networkManagerPrefab;
	
 	void Awake()
    {
    	//Debug.Log("StartListening");
        //EventManager.StartListening (GameConstants.playerSpawnEvent, startTrackingPlayer);
    }

    void OnEnable ()
    {
        EventManager.StartListening (EventManager.get().playerSpawnEvent, startTrackingPlayer);
        EventManager.StartListening (EventManager.get().hitFinalCheckpointEvent, lastCheckpointHit);
        EventManager.StartListening (EventManager.get().hitCheckpointEvent, checkpointHit);
        EventManager.StartListening (EventManager.get().playerDeadEvent, playerIsDead);
        EventManager.StartListening (EventManager.get().loadLevelEvent, levelIsLoading);
       
    }

    void OnDisable ()
    {
        EventManager.StopListening (EventManager.get().playerSpawnEvent, startTrackingPlayer);
        EventManager.StopListening (EventManager.get().hitFinalCheckpointEvent, lastCheckpointHit);
        EventManager.StopListening (EventManager.get().hitCheckpointEvent, checkpointHit);
        EventManager.StopListening (EventManager.get().playerDeadEvent, playerIsDead);
        EventManager.StopListening (EventManager.get().loadLevelEvent, levelIsLoading);
    }

    void levelIsLoading( GameConstants.LoadLevelArgument arg)
    {
       if(arg.isNetworkGame) // network related stuff to initialised
       {
           Instantiate(networkManagerPrefab);
       }
    }

    void playerIsDead(GameConstants.PlayerDeadArgument arg)
    {
        arg.player.GetComponent<CharacterController2D>().respawn(lastHitCheckpoint.GetComponent<Transform>().position.x, lastHitCheckpoint.GetComponent<Transform>().position.y);
    }

    void checkpointHit(GameConstants.HitCheckpointArgument arg)
    {
         lastHitCheckpoint = arg.checkpoint;
    }

    void lastCheckpointHit(GameConstants.HitFinalCheckpointArgument arg)
    {
         GameFlow.get().LoadMainMenu();
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