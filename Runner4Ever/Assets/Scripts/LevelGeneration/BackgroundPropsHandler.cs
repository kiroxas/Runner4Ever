using UnityEngine;
using System;
using System.IO;
using System.Xml.Serialization;
using System.Xml;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Random = UnityEngine.Random;


public class BackgroundPropsHandler
{
	private PoolCollection bgPool;
	public List<float> sizes;
	public int propsPerSegment;

	public int GetPropsIndexThatFits(int availableWidth)
	{
		List<int> indexes = new List<int>();
		for(int i = 0; i < sizes.Count; ++i)
		{
			if((int)sizes[i] <= availableWidth)
				indexes.Add(i);
		}

		int index = -1;

		if(indexes.Count != 0)
		{
			index = indexes[Random.Range(0, indexes.Count -1)];
		}
	
		return index;
	}

	public BackgroundPropsHandler(int propsPerSeg, GameObject[] g, float tileWidth)
	{
		bgPool = new PoolCollection();
		sizes = new List<float>();
		propsPerSegment = propsPerSeg;

		add(g, tileWidth);
	}

	private void add(GameObject[] g, float tileWidth)
	{
		for(int i = 0; i < g.Length; ++i)
		{
			bgPool.addPool(g[i], i, PoolIndexes.smallPoolingStrategy);
			float size = UnityUtils.getSpriteSize(g[i]).x;
			float gridSize = Mathf.Ceil(size / tileWidth);
			sizes.Add(gridSize);
		}
	}

	public GameObject get(int index, Vector3 position)
	{
		return bgPool.getFromPool(index, position);
	}

	public void free(GameObject g, int index)
	{
		bgPool.free(g, index);
	}

}