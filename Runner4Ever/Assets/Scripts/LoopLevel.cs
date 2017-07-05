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

		
	}

	public void Update()
	{
		if(xStart == xEnd)
			return;

		int xPlayer = (int)player.GetComponent<Transform>().position.x;

		int distance = (int)(xPlayer - xStart);
		int maxDistance = (int)xEnd - (int)xStart;
		float percent = (float)distance / (float)maxDistance;

		if(percent > 1.0f)
		{
			player.GetComponent<Transform>().position = new Vector2(xStart, yStart);
		}
	}
}