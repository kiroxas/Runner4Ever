using UnityEngine;
using System;
using System.IO;
using System.Xml.Serialization;
using System.Collections;
using System.Collections.Generic;
using Random = UnityEngine.Random;


public class CheckpointUtils 
{
	static public GameObject findFirstCheckpoint()
	{
		GameObject[] checkpoints = GameObject.FindGameObjectsWithTag("CheckPoint");
		
		if(checkpoints == null || checkpoints.Length == 0)
			return null;

		float xStart = checkpoints[0].GetComponent<Transform>().position.x;
		GameObject toRet = checkpoints[0];

		foreach(GameObject obj in checkpoints)
		{
			if(obj.GetComponent<Transform>().position.x < xStart)
			{
				xStart = obj.GetComponent<Transform>().position.x;
				toRet = obj;
			}
		}

		return toRet;
	}

	static public GameObject findLastCheckpoint()
	{
		GameObject[] checkpoints = GameObject.FindGameObjectsWithTag("CheckPoint");
		
		if(checkpoints == null || checkpoints.Length == 0)
			return null;

		float xEnd = checkpoints[0].GetComponent<Transform>().position.x;
		GameObject toRet = checkpoints[0];

		foreach(GameObject obj in checkpoints)
		{
			if(obj.GetComponent<Transform>().position.x > xEnd)
			{
				xEnd = obj.GetComponent<Transform>().position.x;
				toRet = obj;
			}
		}

		return toRet;
	}
}