using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.SceneManagement;

/*
	Class that handles input, and triggers actions
*/
public class LevelFlow : MonoBehaviour
{
    private GameObject lastHitCheckpoint;
    public GameObject networkManagerPrefab;

    private bool isNetworkGame = false;
    private List<Run> launched = new List<Run>();
	
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
        EventManager.StartListening (EventManager.get().quitMainGameEvent, quitMainGame);
        EventManager.StartListening (EventManager.get().levelInitialisedEvent, levelInitialised);
    }

    void OnDisable ()
    {
        EventManager.StopListening (EventManager.get().playerSpawnEvent, startTrackingPlayer);
        EventManager.StopListening (EventManager.get().hitFinalCheckpointEvent, lastCheckpointHit);
        EventManager.StopListening (EventManager.get().hitCheckpointEvent, checkpointHit);
        EventManager.StopListening (EventManager.get().playerDeadEvent, playerIsDead);
        EventManager.StopListening (EventManager.get().loadLevelEvent, levelIsLoading);
        EventManager.StopListening (EventManager.get().quitMainGameEvent, quitMainGame);
        EventManager.StopListening (EventManager.get().levelInitialisedEvent, levelInitialised);
    }

    public bool isItNetworkGame()
    {
        return isNetworkGame;
    }

     void levelInitialised( GameConstants.LevelInitialisedArgument arg)
    {
        if(isNetworkGame == false)
        {
            UnityUtils.disableMultiObjects();
        }
    }

    void quitMainGame( GameConstants.QuitMainGameArgument arg)
    {
         GameFlow.get().LoadMainMenu();
    }

    void levelIsLoading( GameConstants.LoadLevelArgument arg)
    {
       isNetworkGame = arg.isNetworkGame();

       if(isNetworkGame) // network related stuff to initialised
       {
           Instantiate(networkManagerPrefab);
       }
    }

    void respawn(GameObject player)
    {   
        if(player == null)
        {
            Debug.LogError("The player is null");
        }
        Vector2 pos;

        if(lastHitCheckpoint == null)
        {
            pos = UnityUtils.getSpawningLocations()[0];
        }
        else
        {
            pos = new Vector2(lastHitCheckpoint.GetComponent<Transform>().position.x, lastHitCheckpoint.GetComponent<Transform>().position.y);
        }

        player.GetComponent<CharacterController2D>().respawn(pos.x, pos.y);
    }


    void playerIsDead(GameConstants.PlayerDeadArgument arg)
    {
        GameObject player = arg.player;
        launched.Add(Run.After(2.5f, ()=>{ respawn(player);}));
    }

    void checkpointHit(GameConstants.HitCheckpointArgument arg)
    {
         lastHitCheckpoint = arg.checkpoint;
    }

    void loadMainGame()
    {
         GameFlow.get().LoadMainMenu();
    }

    void OnDestroy() 
    {
        foreach(Run r in launched)
        {
            if(r.isDone == false)
            {
                r.Abort();
            }
        }
    }

    void lastCheckpointHit(GameConstants.HitFinalCheckpointArgument arg)
    {
        launched.Add(Run.After(2.5f, ()=>{ loadMainGame();}));
    }

    void startTrackingPlayer(GameConstants.PlayerSpawnArgument arg)
    {
        var character = arg.player.GetComponent<CharacterController2D>();
     
        TrackingManager.get().startLevel(character);
    }

	void Start()
    {
        
    }

    void LateUpdate()
    {
        TrackingManager.get().updateMainGame();
    }
}