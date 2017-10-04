using UnityEngine;
using System;
using System.IO;
using System.Xml.Serialization;
using System.Xml;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Random = UnityEngine.Random;


/* Class that represents a portion of the level, can load and unload itself */
public class Segment
{
	private Dictionary<int, List<GameObject>> loaded; // All gameobject loaded in the segment
	private bool enabled = false; // is this segment enabled

	private List<char> layout; // the layout in char format
	private int xSize; // x size of the segment
	private int ySize; // y size of the segment

	private float xBegin; // bottom left x position of the segment
	private float yBegin; // bottom left y position of the segment

	private float tileHeight; // height of a tile
	private float tileWidth; // width of a tile

	public int xGrid; // position in the whole level grid
	public int yGrid; // position in the whole level grid

	private string name; // name, for debug purpose

	private Vector3 bottomRight, bottomLeft, topRight, topLeft; // for debug drawings

	public bool isEnabled()
	{
		return enabled;
	}

	public void enable(PoolCollection statePool)
	{
		if(!enabled)
		{
			load(statePool);
			enabled = true;
		}
	}

	public void disable(PoolCollection statePool)
	{
		if(enabled)
		{
			unload(statePool);
			enabled = false;
		}
	}

	void init(int[] indexes)
	{
		for(int i = 0; i < indexes.Length; ++i)
		{
			loaded[indexes[i]] = new List<GameObject>();
		}
	}

	public Segment(int _xSize, int _ySize, float _xBegin, float _yBegin, List<char> _layout, float _tileWidth, float _tileHeight, int _xGrid, int _yGrid)
	{
		loaded = new Dictionary<int, List<GameObject>>();
		init(PoolIndexes.statelessIndexes);
		init(PoolIndexes.stateIndexes);

		layout = new List<char>(_layout);
		xSize = _xSize;
		ySize = _ySize;
		xBegin = _xBegin;
		yBegin = _yBegin;
		tileWidth = _tileWidth;
		tileHeight = _tileHeight;
		xGrid = _xGrid;
		yGrid = _yGrid;

		bottomLeft = new Vector3(xBegin, yBegin, 0.0f);
		bottomRight = new Vector3(xBegin + xSize * tileWidth, _yBegin, 0.0f);
		topLeft = new Vector3(xBegin, _yBegin + ySize * tileHeight, 0.0f );
		topRight = new Vector3(xBegin + xSize * tileWidth,_yBegin + ySize * tileHeight, 0.0f);

		if(layout.Count != xSize * ySize)
		{
			Debug.LogError("[Segment] Layout size (" + layout.Count + ") should be equal to x (" + xSize + ") * y (" + ySize + ")");
		} 
	}

	private void load(PoolCollection statePool)
	{
		if(layout.Count != xSize * ySize)
		{
			Debug.LogError("[Segment] Layout size (" + layout.Count + ") should be equal to x (" + xSize + ") * y (" + ySize + ")");
		} 

		int index =  (ySize - 1) * xSize;
		for(int y = 0; y < ySize; ++y)
		{
			for(int x = 0; x < xSize; ++x)
			{
				char value = layout[index];
				index++;
				if(value == PoolIndexes.emptyIndex)
				{
					continue;
				}

				int poolIndex = PoolIndexes.fileToPoolMapping[value];
				Vector3 position = new Vector3(xBegin + x * tileWidth, yBegin + y * tileHeight, 0.0f); 

				if(loaded.ContainsKey(poolIndex) == false)
				{
					Debug.LogError("[Segment/Load] Cannot store gameobject of type :" + poolIndex);
				}

				loaded[poolIndex].Add(statePool.getFromPool(poolIndex, position));	
			}
			index -= 2* xSize;
		}
	}

	private void unload(PoolCollection statePool)
	{
		foreach(int index in PoolIndexes.statelessIndexes)
		{
			foreach(GameObject obj in loaded[index])
			{
				statePool.free(obj, index);
			}

			loaded[index].Clear();
		}

		foreach(int index in PoolIndexes.stateIndexes)
		{
			foreach(GameObject obj in loaded[index])
			{
				obj.SetActive(false);
			}

			loaded[index].Clear();
		}
	}

	/* --------------------------------- Debug or Unity Utilities ----------------------------------------------- */

	public void setName(string s)
	{
		name = s;
	}

	public string layoutAsString()
	{
		return SegmentStreamer.layoutAsString(layout, xSize, ySize);
	}

	public string presentation()
	{
		return "I'm segment " + name + " at x:" + xBegin + ",y:" + yBegin + " with size x: " + xSize + " ,y: " + ySize + " /n " + layoutAsString(); 
	}

	public void OnDrawGizmos() 
	{
		UnityUtils.drawGizmoSquare(bottomLeft, bottomRight, topRight, topLeft, Color.yellow);
    }
}