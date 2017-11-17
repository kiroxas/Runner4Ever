using UnityEngine;
using UnityEngine.Networking;
using System;
using System.IO;
using System.Xml.Serialization;
using System.Collections;
using System.Collections.Generic;
using Random = UnityEngine.Random;

public class UnityUtils 
{
	public enum CollisionDirection
	{
		Above,
		Below,
		Left,
		Right,
		Nowhere
	}

	public static GameConstants.Mode getMode()
	{
		LevelSelectionMode flow = (LevelSelectionMode)UnityEngine.Object.FindObjectOfType(typeof(LevelSelectionMode));
		if(flow)
		{
			return flow.getActiveMode();
		}

		Debug.LogError("Could not find LevelSelectionMode");
		return GameConstants.Mode.Solo;
	}

	public static void Swap<T>(ref List<T> list, int i, int j)
	{
    	var temp = list[i];
   	 	list[i] = list[j];
   		list[j] = temp;
	}


	public static void Shuffle<T>(ref List<T> list)
	{
   		for(var i=0; i < list.Count; i++)
        	Swap(ref list, i, Random.Range(i, list.Count -1));
	}

	static public Vector2 getSpriteSize(GameObject g)
	{
		if(g.GetComponent<SpriteRenderer>() == null)
			return Vector2.zero;

		Bounds b = g.GetComponent<SpriteRenderer>().bounds;

		foreach(var render in g.GetComponentsInChildren<SpriteRenderer>())
		{
			b.Encapsulate(render.bounds);
		}

		return b.size;
	}

	static public void drawGizmoSquare(Vector3 bottomLeft, Vector3 bottomRight, Vector3 topRight, Vector3 topLeft, Color color)
	{
		Debug.DrawLine(bottomLeft, topLeft, color);
		Debug.DrawLine(topLeft, topRight, color);
		Debug.DrawLine(topRight, bottomRight, color);
		Debug.DrawLine(bottomLeft, bottomRight, color);
	}

	static public int getFPS()
	{
		return (int)(1.0f / Time.deltaTime);
	}

	static public CollisionDirection getCollisionDirection(Bounds rectangleHitbox, Vector2 hitpoint)
	{
		if(Mathf.Approximately(hitpoint.y, rectangleHitbox.max.y))  
        {
        	return CollisionDirection.Above;
        }
        else if(Mathf.Approximately(hitpoint.y, rectangleHitbox.min.y))  // below
        {
        	return CollisionDirection.Below;
        }
        else if(Mathf.Approximately(hitpoint.x, rectangleHitbox.min.x)) // left
        {
        	return CollisionDirection.Left;
        }
        else if(Mathf.Approximately(hitpoint.x, rectangleHitbox.max.x))  // right
        {
        	return CollisionDirection.Right;
        }
        else
        {
        	return CollisionDirection.Nowhere;
        }
	}

	static public void disableMultiObjects()
	{
		GameObject[] multiObjects = GameObject.FindGameObjectsWithTag(GameConstants.multiTag);

		foreach(GameObject obj in multiObjects)
		{
			obj.SetActive(false);
		}
	}

	static public bool isNetworkGame()
	{
		LevelFlow flow = (LevelFlow)UnityEngine.Object.FindObjectOfType(typeof(LevelFlow));
		if(flow)
		{
			return flow.isItNetworkGame();
		}

		Debug.LogError("Could not find level flow");
		return false;
	}

	static public NetworkClient getMyClient()
	{
		MyNetworkManager flow = (MyNetworkManager)UnityEngine.Object.FindObjectOfType(typeof(MyNetworkManager));
		if(flow)
		{
			return flow.client;
		}

		Debug.LogError("Could not find MyNetworkManager");
		return null;
	}

	static public List<Vector2> getSpawningLocations()
	{
		SegmentStreamer flow = (SegmentStreamer)UnityEngine.Object.FindObjectOfType(typeof(SegmentStreamer));
		if(flow)
		{
			return flow.getStartPositions();
		}

		Debug.LogError("Could not find SegmentStreamer");
		return null;
	}
}