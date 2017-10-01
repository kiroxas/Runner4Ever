using UnityEngine;
using System;
using System.IO;
using System.Xml.Serialization;
using System.Xml;
using System.Collections;
using System.Collections.Generic;
using Random = UnityEngine.Random;


public class PoolCollection
{
	static private int stackDefaultSize = 20;

	Dictionary<int, Pooler> pools;

	public PoolCollection()
	{
		pools = new Dictionary<int, Pooler>();
	}

	public void addPool(GameObject prefabInstance, int key)
	{
		pools[key] = new Pooler(prefabInstance, stackDefaultSize);
	}

	public GameObject getFromPool(int index)
	{
		return pools[index].Get();
	}

	public GameObject getFromPool(int index, Vector3 position)
	{
		return pools[index].Get(position, Quaternion.identity);
	}

	public void free(GameObject prefabInstance, int index)
	{
		pools[index].Free(prefabInstance);
	}
}

/*
 Class that holds pools of objects, and load/unload part of the level dynamicaly 
*/
public class SegmentStreamer : MonoBehaviour 
{
	static int earthIndex = 0;
	static int inverseEarthIndex = 1;
	static int waterIndex = 2;

	/* Origin prefab */
	public GameObject instancePlayer;
	public GameObject checkpoint;
	public GameObject landTiles;
	public GameObject inverseLandTiles;
	public GameObject waterTiles;
	public GameObject objectTiles;
	public GameObject hurtTiles;
	public GameObject enemies;
	public GameObject disapearingTile;
	public GameObject escalator;
	public GameObject movingTile;
	public GameObject killMovingTile;

	/* Poolers */

	PoolCollection tilePool;

	public void Awake()
	{
		tilePool = new PoolCollection();

		tilePool.addPool(landTiles, earthIndex);
		tilePool.addPool(inverseLandTiles, inverseEarthIndex);
		tilePool.addPool(waterTiles, waterIndex);
	}
}