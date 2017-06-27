using UnityEngine;
using System;
using System.IO;
using System.Xml.Serialization;
using System.Collections;
using System.Collections.Generic;
using Random = UnityEngine.Random;


public class ProgressionDisplay : MonoBehaviour 
{
	public GameObject path;
	public GameObject cursor;

	public int pathXSize;

	public float xStart;
	public float xEnd;

	public void Start()
	{
		GameObject[] checkpoints = GameObject.FindGameObjectsWithTag("CheckPoint");

		if(checkpoints == null || checkpoints.Length == 0)
			return;

		xStart = xEnd = checkpoints[0].GetComponent<Transform>().position.x;

		foreach(GameObject obj in checkpoints)
		{
			xStart = Math.Min(xStart, obj.GetComponent<Transform>().position.x);
			xEnd = Math.Max(xStart, obj.GetComponent<Transform>().position.x);
		}

	}
}