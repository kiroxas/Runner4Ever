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
		GameObject[] checkpoints = GameObject.FindGameObjectsWithTag("CheckPoint");
		player =  GameObject.FindGameObjectWithTag("Player");

		if(checkpoints == null || checkpoints.Length == 0)
			return;

		yStart = checkpoints[0].GetComponent<Transform>().position.y;
		xEnd = checkpoints[0].GetComponent<Transform>().position.x;
		xStart = xEnd;

		foreach(GameObject obj in checkpoints)
		{
			xStart = Math.Min(xStart, obj.GetComponent<Transform>().position.x);
			xEnd = Math.Max(xEnd, obj.GetComponent<Transform>().position.x);
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

		if(percent > 1.0f || yPlayer < cam.containingBox.min.y || xPlayer < (cam.containingBox.min.x)|| xPlayer > cam.containingBox.max.x)
		{
			Debug.Log("Min y : " + cam.containingBox.min.y + " actual y : " + yPlayer);
			player.GetComponent<Transform>().position = new Vector2(xStart, yStart);
			player.GetComponent<CharacterController2D>().reinit();
		}
	}
}