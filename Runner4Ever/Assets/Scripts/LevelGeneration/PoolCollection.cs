using UnityEngine;
using System;
using System.IO;
using System.Xml.Serialization;
using System.Xml;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Random = UnityEngine.Random;


/* Class that will handle all the pools */
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
		addPool(prefabInstance, key, stackDefaultSize);
	}

	public void addPool(GameObject prefabInstance, int key, int stackSize)
	{
		if(pools.ContainsKey(key))
		{
			Debug.LogError("Already assigned a pool to this key : " + key);
		}
		pools[key] = new Pooler(prefabInstance, stackSize);
	}

	public GameObject getFromPool(int index)
	{
		return pools[index].Get();
	}

	public GameObject getFromPool(int index, Vector3 position)
	{
		if(pools.ContainsKey(index) == false)
		{
			Debug.LogError("could not find key : " + index);
		}
		return pools[index].Get(position, Quaternion.identity);
	}

	public void free(GameObject prefabInstance, int index)
	{
		pools[index].Free(prefabInstance);
	}

	public GameObject getUsedFromPool(int index)
	{
		return pools[index].getUsedObject();
	}
}