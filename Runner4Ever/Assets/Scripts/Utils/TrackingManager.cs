using System;
using System.IO;
using UnityEngine;
using Random = UnityEngine.Random;

/*
	Tracks everything in the game session
*/
public class TrackingManager : MonoBehaviour
{
	private static TrackingManager instance;

	// Singleton :(
    void Awake () 
    {
        if (instance == null) 
        {
            instance = this;
        } 
        else if (instance != this)
        {
            Destroy (gameObject);
        }

        DontDestroyOnLoad (gameObject);
    }

    static public TrackingManager get()
    {
        return instance;
    }

	/* Time related */
	private float levelStartTime;

	static public float getCurrentSessionTime()
	{
		return Time.realtimeSinceStartup;
	}

	public void startLevelTime()
	{
		levelStartTime = getCurrentSessionTime();
	}

	public float getRealTimeLevel()
	{
		return getCurrentSessionTime() - levelStartTime;
	}

	/* Main Game gameplay related */
	private CharacterController2D currentPlayer = null;
	private float distanceRun;
	private int jumpsDone;
	private int objectCollected;

	public void startLevel(CharacterController2D player)
	{
		
		startLevelTime();
		currentPlayer = player;
		distanceRun = 0.0f;
		jumpsDone = 0;
		objectCollected = 0;
	} 

	public void updateMainGame()
	{
		if(currentPlayer)
		{
			jumpsDone = currentPlayer.getNumberOfJumps();
			distanceRun = currentPlayer.getRunDistance();
			objectCollected = currentPlayer.getObjectsAcquiredCount();
		}
	}

	public float distanceForThisRun()
	{
		return distanceRun;
	}

	public int jumpsForThisRun()
	{
		return jumpsDone;
	}

	public int objectsCollectedForThisRun()
	{
		return objectCollected;
	}

}