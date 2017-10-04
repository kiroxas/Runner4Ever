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

	private float xStart = 0;
	private float yStart = 0;
	private float xEnd = 0;

	private CameraFollow cam;

	public void Start()
	{
		player = GameObject.FindGameObjectWithTag("Player");
		GameObject firstCheckpoint = CheckpointUtils.findFirstCheckpoint();
		GameObject lastCheckpoint = CheckpointUtils.findLastCheckpoint();

		if(firstCheckpoint && lastCheckpoint)
		{
			xStart = firstCheckpoint.GetComponent<Transform>().position.x;
			yStart = firstCheckpoint.GetComponent<Transform>().position.y;
			xEnd = lastCheckpoint.GetComponent<Transform>().position.x;
		}

		cam = FindObjectOfType<CameraFollow>();
	}

	public void Update()
	{	
		int xPlayer = (int)player.GetComponent<Transform>().position.x;
		int yPlayer = (int)player.GetComponent<Transform>().position.y;

		int distance = (int)(xPlayer - xStart);
		int maxDistance = (int)xEnd - (int)xStart;
		float percent =  xStart == xEnd ? 1.0f : (float)distance / (float)maxDistance;

		if(player.GetComponent<CharacterController2D>().isDead() || percent > 1.0f || yPlayer < cam.containingBox.min.y || xPlayer < (cam.containingBox.min.x)|| xPlayer > cam.containingBox.max.x)
		{
			player.GetComponent<CharacterController2D>().respawn(xStart, yStart);
		}
	}
}