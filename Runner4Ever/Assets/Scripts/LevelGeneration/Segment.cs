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
	private Dictionary<int, List<GameObject>> stateLoaded; // All gameobject loaded in the segment
	private Dictionary<int, List<GameObject>> bgLoaded; // All gameobject loaded in the segment
	private bool enabled = false; // is this segment enabled
	private bool firstLoad = true; // first time loading ?

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
	private List<Vector2> groundLevels = new List<Vector2>(); //first list is for x (so groundLevels[0] is for grid with x == 0), and it can have multiple grounds on different ys


	private Vector3 bottomRight, bottomLeft, topRight, topLeft; // for debug drawings

	public GameConstants.SegmentEnabledArgument getBounds()
	{
		return new GameConstants.SegmentEnabledArgument(bottomLeft, topRight);
	}

	private bool isItGround(int x, int y)
	{
		int currentIndex = (ySize - y) * xSize + x;
		if(layout[currentIndex] != '1')
			return false;

		if(ySize - y - 1 < 0)
			return false;

		currentIndex = (ySize - y - 1) * xSize + x;
		if(layout[currentIndex] != PoolIndexes.emptyIndex)
			return false;

		if(ySize - y - 2 < 0)
			return true;

		currentIndex = (ySize - y - 2) * xSize + x;
		if(layout[currentIndex] != PoolIndexes.emptyIndex)
			return false;

		return true;
	}

	private void fillGroundLevel()
	{
		for(int x = 0; x < xSize; ++x)
		{
			for(int y = 1; y < ySize; ++y)
			{
				if(isItGround(x,y))
				{
					groundLevels.Add(new Vector2(x,y));
				}
			}
		}

		UnityUtils.Shuffle(ref groundLevels);
	}

	public bool isEnabled()
	{
		return enabled;
	}

	public void enable(PoolCollection statePool, PoolCollection bgPool)
	{
		if(!isEnabled())
		{
			load(statePool, bgPool);
			enabled = true;

			EventManager.TriggerEvent(EventManager.get().segmentEnabledEvent, new GameConstants.SegmentEnabledArgument(bottomRight, topLeft));
		}
	}

	public void disable(PoolCollection statePool, PoolCollection bgPool)
	{
		if(isEnabled())
		{
			unload(statePool, bgPool);
			enabled = false;
		}
	}

	void init(int[] indexes, Dictionary<int, List<GameObject>> loadedList)
	{
		for(int i = 0; i < indexes.Length; ++i)
		{
			loadedList[indexes[i]] = new List<GameObject>();
		}
	}

	public Segment(int _xSize, int _ySize, float _xBegin, float _yBegin, List<char> _layout, float _tileWidth, float _tileHeight, int _xGrid, int _yGrid)
	{
		loaded = new Dictionary<int, List<GameObject>>();
		stateLoaded = new  Dictionary<int, List<GameObject>>();
		bgLoaded = new Dictionary<int, List<GameObject>>();

		init(PoolIndexes.statelessIndexes, loaded);
		init(PoolIndexes.stateIndexes, stateLoaded);

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

		fillGroundLevel();

		if(layout.Count != xSize * ySize)
		{
			Debug.LogError("[Segment] Layout size (" + layout.Count + ") should be equal to x (" + xSize + ") * y (" + ySize + ")");
		} 
	}

	private int getMaxWidthAvailable(Vector2 placement)
	{
		int width = 1;

		while(true) 
		{
			placement.x++;
			var nextLevel = groundLevels.Find(c => c.x == placement.x);
			
			if(placement.x >= xSize || nextLevel.y != placement.y)
			{
				break;
			}
			width++;
		}

		return width;
	}



	private bool isObjValidHere(GameObject obj, Vector2 placement)
	{
		Vector2 size =UnityUtils.getSpriteSize(obj);
		int xSize = (int)Mathf.Ceil(size.x / tileWidth);
		int placeAvailable= getMaxWidthAvailable(placement);

		if(xSize > placeAvailable)
			return false;

		return true;
	}

	private int getIndex(Vector2 vec)
	{
		return getIndex((int)vec.x, (int)vec.y);
	}

	private int getIndex(int x, int y)
	{
		return (ySize - y - 1) * xSize + x;
	}

	private void loadBgProps(PoolCollection bgPool)
	{
		// load a bg prop
		if(bgLoaded.Count == 0 && groundLevels.Count > 0)
		{
			for(int i = 0; i < 2; ++i)
			{
				int ind = bgPool.getRandomIndex();
				Vector2 gridIndex = groundLevels[i % groundLevels.Count];

				float xOffset =  (tileWidth / 2.0f); // bg props have pivot at 0,0, instead of .5/.5 of tiles
				float yOffset =  (tileHeight / 2.0f); // bg props have pivot at 0,0, instead of .5/.5 of tiles

				Vector3 position = new Vector3(xBegin + gridIndex.x * tileWidth - xOffset, yBegin + gridIndex.y * tileHeight - yOffset, 0.0f); 
				if(bgLoaded.ContainsKey(ind) == false)
					bgLoaded[ind] = new List<GameObject>();

				var obj =  bgPool.getFromPool(ind, position);

				if(isObjValidHere(obj, gridIndex))
				{
					bgLoaded[ind].Add(obj);
				}
				else
				{
					bgPool.free(obj, ind);
				}
			}
		}
	}

	private void load(PoolCollection statePool, PoolCollection bgPool)
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

				bool loadedContains = loaded.ContainsKey(poolIndex);
				bool stateLoadedContains = stateLoaded.ContainsKey(poolIndex);

				if(loadedContains == false && stateLoadedContains == false)
				{
					Debug.LogError("[Segment/Load] Cannot store gameobject of type :" + poolIndex);
				}

				if(loadedContains)
				{
					loaded[poolIndex].Add(statePool.getFromPool(poolIndex, position));	
				}
				else if(stateLoadedContains && firstLoad)
				{
					stateLoaded[poolIndex].Add(statePool.getFromPool(poolIndex, position));
				}
			}
			index -= 2* xSize;
		}

		if(!firstLoad) // if not first load, just reactivate the state tiles
		{
			foreach(int ind in PoolIndexes.stateIndexes)
			{
				foreach(GameObject obj in stateLoaded[ind])
				{
					obj.SetActive(true);
				}
			}
		}

		loadBgProps(bgPool);

		firstLoad = false;
	}

	private void unload(PoolCollection statePool, PoolCollection bgPool)
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
			foreach(GameObject obj in stateLoaded[index])
			{
				obj.SetActive(false);
			}
		}

		foreach(var entry in bgLoaded)
		{
			foreach(GameObject g in entry.Value)
			{
				bgPool.free(g, entry.Key);
			}
			entry.Value.Clear();
		}
		bgLoaded.Clear();
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