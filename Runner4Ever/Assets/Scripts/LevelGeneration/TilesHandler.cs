using UnityEngine;
using System;
using System.IO;
using System.Xml.Serialization;
using System.Xml;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Random = UnityEngine.Random;

public class TilesHandler
{
	public enum TilePlacement
	{
		OnTop,
		BelowTop,
		Right,
		Left,
		InnerRight,
		InnerLeft,
		Inner,
		Bottom,
		InnerBottom,
		Floating,
		None
	}

	static public TilePlacement getPlacement(Deepness tileDeepness)
	{
		if(tileDeepness.empty())
		{
			return TilePlacement.None;
		}
		else if(tileDeepness.top == 1 && tileDeepness.bottom == 1)
		{
			return TilePlacement.Floating;
		}
		else if(tileDeepness.top == 1) // on top
		{
			return TilePlacement.OnTop;
		}
		else if(tileDeepness.right == 1)
		{
			return TilePlacement.Right;
		}
		else if(tileDeepness.left == 1)
		{
			return TilePlacement.Left;
		}
		else if(tileDeepness.bottom == 1)
		{
			return TilePlacement.Bottom;
		}
		else if(tileDeepness.top == 2)
		{
			return TilePlacement.BelowTop;
		}
		else if(tileDeepness.right == 2)
		{
			return TilePlacement.InnerRight;
		}
		else if(tileDeepness.left == 2)
		{
			return TilePlacement.InnerLeft;
		}
		else if(tileDeepness.bottom == 2)
		{
			return TilePlacement.InnerBottom;
		}
		else
		{
			return TilePlacement.Inner;
		}
	}

	private Dictionary<TilePlacement, PoolCollection> myPools;

	public TilesHandler()
	{
		myPools = new Dictionary<TilePlacement, PoolCollection>();
	}

	public void addTileType(TilePlacement placement, GameObject[] tiles)
	{
		if(myPools.ContainsKey(placement))
		{
			Debug.LogError("Already added a pool of placement " + placement);
		}

		myPools[placement] = new PoolCollection();

		for(int i = 0; i < tiles.Length; ++i)
		{
			myPools[placement].addPool(tiles[i], i, PoolIndexes.smallPoolingStrategy);
		}
	}

	public int getRandomTileIndex(Deepness tileDeepness)
	{
		return myPools[getPlacement(tileDeepness)].getRandomIndex();
	}

	public GameObject getFromPool(Deepness tileDeepness, int index, Vector3 position)
	{
		return myPools[getPlacement(tileDeepness)].getFromPool(index, position);
	}

	public void free(GameObject g, Deepness tileDeepness, int index)
	{
		myPools[getPlacement(tileDeepness)].free(g, index);
	}

}