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

public class PoolIndexes
{
	// -------------- StateLess
	public static int earthIndex = 0;
	public static int inverseEarthIndex = 1;
	public static int waterIndex = 2;
	public static int hurtIndex = 3;

	// -------------- StateFull
	public static int objectIndex = 4;
	public static int enemiesIndex = 5;
	public static int disapearingIndex = 6;
	public static int escalatorIndex = 7; 
	public static int movingIndex = 8;
	public static int killMovingIndex = 9;
}

public class Segment
{
	Dictionary<int, List<GameObject>> loaded;
	bool initialLoad = true;

	void init(int index)
	{
		loaded[index] = new List<GameObject>();
	}

	Segment()
	{
		loaded = new Dictionary<int, List<GameObject>>();
		init(PoolIndexes.earthIndex);
		init(PoolIndexes.inverseEarthIndex);
		init(PoolIndexes.waterIndex);
		init(PoolIndexes.hurtIndex);
		init(PoolIndexes.objectIndex);
		init(PoolIndexes.enemiesIndex);
		init(PoolIndexes.disapearingIndex);
		init(PoolIndexes.escalatorIndex);
		init(PoolIndexes.movingIndex);
		init(PoolIndexes.killMovingIndex);
	}

	void load(PoolCollection statePool, PoolCollection statelessPool)
	{

	}

	void unload(PoolCollection statePool, PoolCollection statelessPool)
	{

	}
}

/*
 Class that holds pools of objects, and load/unload part of the level dynamicaly 
*/
public class SegmentStreamer : MonoBehaviour 
{
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

	PoolCollection statelessPool;
	PoolCollection statePool;

	public void Awake()
	{
		statelessPool = new PoolCollection();

		statelessPool.addPool(landTiles, PoolIndexes.earthIndex);
		statelessPool.addPool(inverseLandTiles, PoolIndexes.inverseEarthIndex);
		statelessPool.addPool(waterTiles, PoolIndexes.waterIndex);
	}
}