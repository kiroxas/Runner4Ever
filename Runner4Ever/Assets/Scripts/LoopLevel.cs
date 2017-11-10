using UnityEngine;
using System;
using System.IO;
using System.Xml.Serialization;
using System.Collections;
using System.Collections.Generic;
using Random = UnityEngine.Random;


public class LoopLevel : MonoBehaviour 
{
	private GameObject player;
	private SegmentStreamer cam;

	void OnEnable()
    {
    	EventManager.StartListening (EventManager.get().playerSpawnEvent, selectPlayer);
    }

    void OnDisable ()
    {
        EventManager.StopListening (EventManager.get().playerSpawnEvent, selectPlayer);
    }

    public void selectPlayer(GameConstants.PlayerSpawnArgument arg)
    {
		player = GameObject.FindGameObjectWithTag("Player");
		cam = FindObjectOfType<SegmentStreamer>();
    }

	public void Start()
	{}

	public void Update()
	{	

		if(player != null && cam != null)
		{
			int xPlayer = (int)player.GetComponent<Transform>().position.x;
			int yPlayer = (int)player.GetComponent<Transform>().position.y;		

			if(player.GetComponent<CharacterController2D>().isDead() || yPlayer < cam.containingBox.min.y || xPlayer < (cam.containingBox.min.x)|| xPlayer > cam.containingBox.max.x)
			{
				EventManager.TriggerEvent(EventManager.get().playerDeadEvent, new GameConstants.PlayerDeadArgument(player));
				//player.GetComponent<CharacterController2D>().respawn(xStart, yStart);
			}
		}
	}
}