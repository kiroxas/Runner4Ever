using UnityEngine;
using System;
using System.IO;
using System.Xml.Serialization;
using System.Xml;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Random = UnityEngine.Random;

public class Deepness
{
	public int top;
	public int bottom;
	public int right;
	public int left;

	public Deepness(int t, int b, int r, int l)
	{
		top = t;
		bottom = b;
		right = r;
		left = l;
	}

	public string toString()
	{
		return "t:" + top + " , b:" + bottom + " ,r:" + right + " ,l:" + left;
	}

	public bool empty()
	{
		return top == 0 && bottom == 0 && right == 0 && left == 0;
	}

	static public List<Deepness> calculateDeepness(List<int> indexes, List<char> layout, int xSize, int ySize)
	{
		List<Deepness> deepness = Enumerable.Repeat(new Deepness(0,0,0,0), layout.Count).ToList(); 
		// first pass, from left bottom to top right
		for(int y = 0; y < ySize; ++y)
		{
			for(int x = 0; x < xSize; ++x)
			{
				int index = Segment.getStaticIndex(x,y, xSize, ySize);
				char value = layout[index];
				int poolIndex = PoolIndexes.fileToPoolMapping[value];

				int left = 0;
				int bottom = 0;

				// left component
				if( indexes.Contains(poolIndex) == false )
				{
					left = 0;
				}
				else if(x == 0) // on earth index, but first of the row
				{
					left = 1; // for now deepness inside the segment
				}
				else
				{
					left = deepness[Segment.getStaticIndex(x-1, y, xSize, ySize)].left + 1;
				}

				// bottom component
				if(indexes.Contains(poolIndex) == false )
				{
					bottom = 0;
				}
				else if(y == 0) // on earth index, but first of the row
				{
					bottom = 1; // for now deepness inside the segment
				}
				else
				{
					bottom = deepness[Segment.getStaticIndex(x, y - 1, xSize, ySize)].bottom + 1;
				}

				deepness[index] = new Deepness(0,bottom,0,left); 
			}
		}

		// second pass, from top right to bottom left
		for(int y = ySize - 1; y >= 0; --y)
		{
			for(int x = xSize - 1; x >= 0; --x)
			{
				int index = Segment.getStaticIndex(x,y, xSize, ySize);
				char value = layout[index];
				int poolIndex = PoolIndexes.fileToPoolMapping[value];

				int right = 0;
				int top = 0;

				// right component
				if(indexes.Contains(poolIndex) == false )
				{
					right = 0;
				}
				else if(x == xSize - 1) // on earth index, but first of the row
				{
					right = 1; // for now deepness inside the segment
				}
				else
				{
					right = deepness[Segment.getStaticIndex(x + 1, y, xSize, ySize)].right + 1;
				}

				// top component
				if(indexes.Contains(poolIndex) == false)
				{
					top = 0;
				}
				else if(y == ySize - 1) // on earth index, but first of the row
				{
					top = 1; // for now deepness inside the segment
				}
				else
				{
					top = deepness[Segment.getStaticIndex(x, y + 1, xSize, ySize)].top + 1;
				}

				deepness[index] = new Deepness(top,deepness[index].bottom,right,deepness[index].left); 
			}
		}

		return deepness;
	}
}

public class LoadedTile
	{
		public int tileIndex;
		public int poolIndex;
		public GameObject obj;

		public LoadedTile(int ind, int pi, GameObject o)
		{
			tileIndex = pi;
			obj = o;
			poolIndex = ind;
		}
	}

/* Class that represents a portion of the level, can load and unload itself */
public class Segment
{
	private Dictionary<int, List<GameObject>> loaded; // All gameobject loaded in the segment
	private Dictionary<int, List<GameObject>> stateLoaded; // All gameobject loaded in the segment
	private Dictionary<int, List<GameObject>> bgLoaded; // All gameobject loaded in the segment
	private Dictionary<Vector2, LoadedTile> tilesLoaded; // tiles loaded by position
	private bool enabled = false; // is this segment enabled
	private bool firstLoad = true; // first time loading ?

	private List<char> layout; // the layout in char format
	private Dictionary<int, List<Deepness>> deepness; // deepness of earth tiles
	public SegmentInfo info;

	private string name; // name, for debug purpose
	private List<Vector2> groundLevels = new List<Vector2>(); //first list is for x (so groundLevels[0] is for grid with x == 0), and it can have multiple grounds on different ys

	private Vector3 bottomRight, bottomLeft, topRight, topLeft; // for debug drawings

	public GameConstants.SegmentEnabledArgument getBounds()
	{
		return new GameConstants.SegmentEnabledArgument(bottomLeft, topRight);
	}

	private bool containsIndex(int index)
	{
		return layout.Contains(PoolIndexes.findKey(index));
	}

	public bool containsCheckpoint()
	{
		return containsIndex(PoolIndexes.startCheckpointIndex);
	}

	private bool isItGround(int x, int y)
	{
		int index = getIndex(x,y);
		int indexBelow = getIndex(x,y - 1);

		return deepness[PoolIndexes.earthIndex][index].top == 0 && indexBelow >= 0 && deepness[PoolIndexes.earthIndex][indexBelow].top == 1;
	}

	public Vector2 getSegmentGridPlacement()
	{
		return new Vector2(info.layoutXGrid, info.layoutYGrid);
	}

	private void fillGroundLevel()
	{
		for(int x = 0; x < info.xSize; ++x)
		{
			for(int y = 1; y < info.ySize; ++y)
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

	public void enable(PoolCollection statePool, BackgroundPropsHandler bgPool, TilesSuperHandler tiles)
	{
		if(!isEnabled())
		{
			load(statePool, bgPool, tiles);
			enabled = true;

			EventManager.TriggerEvent(EventManager.get().segmentEnabledEvent, new GameConstants.SegmentEnabledArgument(bottomRight, topLeft));
		}
	}

	public void disable(PoolCollection statePool, BackgroundPropsHandler bgPool, TilesSuperHandler tiles)
	{
		if(isEnabled())
		{
			unload(statePool, bgPool, tiles);
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

	public Segment(SegmentInfo inf, List<char> _layout, Dictionary<int, List<Deepness>> deep)
	{
		loaded = new Dictionary<int, List<GameObject>>();
		stateLoaded = new  Dictionary<int, List<GameObject>>();
		bgLoaded = new Dictionary<int, List<GameObject>>();
		tilesLoaded = new Dictionary<Vector2, LoadedTile>();

		init(PoolIndexes.statelessIndexes, loaded);
		init(PoolIndexes.stateIndexes, stateLoaded);

		layout = new List<char>(_layout);
		info = inf;
		deepness = new Dictionary<int, List<Deepness>>(deep);

		bottomLeft = new Vector3(info.xBegin, info.yBegin, 0.0f);
		bottomRight = new Vector3(info.xBegin + info.xSize * info.tileWidth, info.yBegin, 0.0f);
		topLeft = new Vector3(info.xBegin, info.yBegin + info.ySize * info.tileHeight, 0.0f );
		topRight = new Vector3(info.xBegin + info.xSize * info.tileWidth, info.yBegin + info.ySize * info.tileHeight, 0.0f);
		
		fillGroundLevel();

		if(layout.Count != info.xSize * info.ySize)
		{
			Debug.LogError("[Segment] Layout size (" + layout.Count + ") should be equal to x (" + info.xSize + ") * y (" + info.ySize + ")");
		} 
	}

	private Vector2 getSpriteSize(GameObject obj)
	{
		Vector2 size = UnityUtils.getSpriteSize(obj);
		size = new Vector2(Mathf.Ceil(size.x / info.tileWidth), Mathf.Ceil(size.y / info.tileHeight));

		return size;
	}

	private int getMaxWidthAvailable(Vector2 placement)
	{
		int width = 1;

		while(true) 
		{
			var nextLevel = groundLevels.Find(c => c.x == placement.x + width);
			
			if(placement.x + width >= info.xSize || nextLevel.y != placement.y)
			{
				break;
			}

			float xOffset = (info.tileWidth / 2.0f); // bg props have pivot at 0,0, instead of .5/.5 of tiles
			
			foreach(var values in bgLoaded.Values)
			{
				foreach(GameObject g in values)
				{
					float place = Mathf.Ceil((g.GetComponent<Transform>().position.x - info.xBegin + xOffset ) / info.tileWidth);
					
					if(place == placement.x + width)
						return width;
				}
			}

			width++;
		}

		return width;
	}


	private bool isObjValidHere(GameObject obj, Vector2 placement)
	{
		Vector2 size = getSpriteSize(obj);
		int xSize = (int)Mathf.Ceil(size.x / info.tileWidth);
		int placeAvailable = getMaxWidthAvailable(placement);

		if(xSize > placeAvailable)
			return false;

		float xOffset = (info.tileWidth / 2.0f); // bg props have pivot at 0,0, instead of .5/.5 of tiles

		foreach(var values in bgLoaded)
		{
			foreach(GameObject g in values.Value)
			{
				int place = (int)((g.GetComponent<Transform>().position.x - info.xBegin + xOffset )/ info.tileWidth);
				if(place > placement.x && place < placement.x + xSize)
					return false;
			}
		}

		return true;
	}

	private int bgLoadedCount()
	{
		int i = 0;
		foreach(var values in bgLoaded)
		{
			foreach(GameObject g in values.Value)
			{
				++i;
			}
		}

		return i;
	}

	private void removeSizeFromGroundAvailable(int xSize, Vector2 placement)
	{
		for(int x = (int)placement.x; x < (int)placement.x + xSize; ++x)
		{
			groundLevels.Remove(new Vector2(x, placement.y));
		}
	}

	static public int getStaticIndex(Vector2 vec, int xSize, int ySize)
	{
		return getStaticIndex((int)vec.x, (int)vec.y, xSize, ySize);
	}

	static public int getStaticIndex(int x, int y, int xSize, int ySize)
	{
		return  y * xSize + x;
	}

	public int getIndex(Vector2 vec)
	{
		return getStaticIndex((int)vec.x, (int)vec.y, info.xSize, info.ySize);
	}

	public int getIndex(int x, int y)
	{
		return getStaticIndex(x, y, info.xSize, info.ySize);
	}

	private void loadBgProps(BackgroundPropsHandler bgPool)
	{
		// load a bg prop
		if(bgLoaded.Count == 0 && groundLevels.Count > 0)
		{
			while(bgLoadedCount() <= bgPool.propsPerSegment)
			{
				if(groundLevels.Count == 0) // if we "ate" ll available space
					break;

				Vector2 gridIndex = groundLevels[Random.Range(0, groundLevels.Count - 1)];

				int size = getMaxWidthAvailable(gridIndex);
				int index = bgPool.GetPropsIndexThatFits(size);

				if(index == -1)
				{
					//Debug.Log("Could not find any size " + size + " at " + gridIndex);
					removeSizeFromGroundAvailable(1, gridIndex);
					continue;
				}

				if(bgLoaded.ContainsKey(index) == false)
					bgLoaded[index] = new List<GameObject>();

				float xOffset = info.tileWidth / 2.0f;
				float yOffset = info.tileHeight / 2.0f;

				Vector3 position = new Vector3( info.xBegin + gridIndex.x * info.tileWidth - xOffset , info.yBegin + gridIndex.y * info.tileHeight - yOffset, 0.0f);

				GameObject props = bgPool.get(index, position);
				bgLoaded[index].Add(props);
				removeSizeFromGroundAvailable((int)getSpriteSize(props).x, gridIndex);
			}

			//Debug.Log("Added " + bgLoadedCount() + " props to segment");
		}
	}

	private Vector3 getPosition(int x, int y)
	{
		return  new Vector3(info.xBegin + x * info.tileWidth - (info.tileWidth / 2.0f), info.yBegin + y * info.tileHeight - (info.tileHeight / 2.0f), 0.0f); 
	}

	private void load(PoolCollection statePool, BackgroundPropsHandler bgPool, TilesSuperHandler tileHandler)
	{
		if(layout.Count != info.xSize * info.ySize)
		{
			Debug.LogError("[Segment] Layout size (" + layout.Count + ") should be equal to x (" + info.xSize + ") * y (" + info.ySize + ")");
		} 

		for(int y = 0; y < info.ySize; ++y)
		{
			for(int x = 0; x < info.xSize; ++x)
			{
				int index = getIndex(x,y);
				char value = layout[index];
				
				if(value == PoolIndexes.emptyIndex)
				{
					continue;
				}

				int poolIndex = PoolIndexes.fileToPoolMapping[value];
				Vector3 position = getPosition(x,y); 

				if(tileHandler.containsKey(poolIndex) == false)
				{
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
				else // tile
				{
					if(deepness.ContainsKey(poolIndex) == false)
					{
						Debug.LogError("You must calculate the deepness of key : " + poolIndex + " in segmentStreamer");
					}
					int tileIndex = tileHandler.getRandomTileIndex(poolIndex, deepness[poolIndex][index]);
					GameObject g = tileHandler.getFromPool(poolIndex, deepness[poolIndex][index], tileIndex, position);
					tilesLoaded[new Vector2(x ,y)] = new LoadedTile(poolIndex, tileIndex, g);
				}
			}
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

	private void unload(PoolCollection statePool, BackgroundPropsHandler bgPool, TilesSuperHandler tileHandler)
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

		foreach(var tileLoaded in tilesLoaded)
		{
			int index = getIndex(tileLoaded.Key);
			
			tileHandler.free(tileLoaded.Value.poolIndex, tileLoaded.Value.obj, deepness[tileLoaded.Value.poolIndex][index], tileLoaded.Value.tileIndex);
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
		return SegmentStreamer.layoutAsString(layout, info.xSize, info.ySize);
	}

	public string presentation()
	{
		return "I'm segment " + name + " at x:" + info.xBegin + ",y:" + info.yBegin + " with size x: " + info.xSize + " ,y: " + info.ySize + " /n " + layoutAsString(); 
	}

	public void OnDrawGizmos() 
	{
		UnityUtils.drawGizmoSquare(bottomLeft, bottomRight, topRight, topLeft, Color.yellow);
    }
}