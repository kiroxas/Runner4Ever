using UnityEngine;
using System;
using System.IO;
using System.Xml.Serialization;
using System.Xml;
using System.Collections;
using System.Collections.Generic;
using System.Text;
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
		if(pools.ContainsKey(key))
		{
			Debug.LogError("Already assigned a pool to this key : " + key);
		}
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

	public static int[] statelessIndexes = { earthIndex, inverseEarthIndex, waterIndex, hurtIndex };

	// -------------- StateFull
	public static int objectIndex = 4;
	public static int enemiesIndex = 5;
	public static int disapearingIndex = 6;
	public static int escalatorIndex = 7; 
	public static int movingIndex = 8;
	public static int killMovingIndex = 9;

	public static int playerIndex = 10;
	public static int checkpointIndex = 11;

	public static int[] stateIndexes = { objectIndex, enemiesIndex, disapearingIndex, escalatorIndex, movingIndex, killMovingIndex};

	public static char emptyIndex = '0';
	public static Dictionary<char, int> fileToPoolMapping = new Dictionary<char, int>
	 { {'1', earthIndex}, 
	   {'2', waterIndex},
	   {'3', objectIndex},
	   {'4', playerIndex},
	   {'5', checkpointIndex},
	   {'6', inverseEarthIndex},
	   {'7', hurtIndex},
	   {'8', enemiesIndex},
	   {'9', disapearingIndex},
	   {'A', escalatorIndex},
	   {'B', movingIndex},
	   {'C', killMovingIndex} };

}

public class Segment
{
	private Dictionary<int, List<GameObject>> loaded;
	private bool initialLoad = true;

	private List<char> layout;
	private int xSize;
	private int ySize;

	private float xBegin;
	private float yBegin;

	private float tileHeight;
	private float tileWidth;

	private string name;

	void init(int[] indexes)
	{
		for(int i =0; i < indexes.Length; ++i)
		{
			loaded[i] = new List<GameObject>();
		}
	}

	public Segment(int _xSize, int _ySize, float _xBegin, float _yBegin, List<char> _layout, float _tileWidth, float _tileHeight)
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

		if(layout.Count != xSize * ySize)
		{
			Debug.LogError("[Segment] Layout size (" + layout.Count + ") should be equal to x (" + xSize + ") * y (" + ySize + ")");
		} 
	}

	public void load(PoolCollection statePool)
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
				Debug.Log("Index : " + index);
				char value = layout[index];
				index++;
				if(value == PoolIndexes.emptyIndex)
				{
					continue;
				}

				int poolIndex = PoolIndexes.fileToPoolMapping[value];
				Vector3 position = new Vector3(xBegin + x * tileWidth, yBegin + y * tileHeight, 0.0f); 

				loaded[poolIndex].Add(statePool.getFromPool(poolIndex, position));	
			}
			index -= 2* xSize;
		}
	}

	public void unload(PoolCollection statePool)
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
}

/*
 Class that holds pools of objects, and load/unload part of the level dynamicaly 
*/
public class SegmentStreamer : MonoBehaviour 
{

	public static string layoutAsString(List<char> layout, int xSize, int ySize)
	{
		StringBuilder sb = new StringBuilder();

		int index = 0;
		for(int y =0; y < ySize; ++y)
		{
			for(int x = 0; x < xSize; ++x)
			{
				sb.Append(layout[index]);
				index++;
			}
			sb.Append("\n");
		}

		return sb.ToString();
	}

	[HideInInspector]
	public int xTotalLevel = 6;
	[HideInInspector]
	public int yTotalLevel = 6;

	private int xSegments;
	private int ySegments;
	private List<Segment> segments;

	public int xTilePerSegment = 6; // 
	public int yTilePerSegment = 6;

	public float tileWidth = 1.28f;
	public float tileHeight = 1.28f;

	public float bottomLeftXPos = 0;
	public float bottomLeftYPos = 0;

	/* Origin prefab */
	public GameObject instancePlayer;
	public GameObject checkpoint;

	/* Stateless prefabs */
	public GameObject landTiles;
	public GameObject inverseLandTiles;
	public GameObject waterTiles;
	public GameObject hurtTiles;

	/* State prefabs */
	public GameObject objectTiles;
	public GameObject enemies;
	public GameObject disapearingTile;
	public GameObject escalator;
	public GameObject movingTile;
	public GameObject killMovingTile;

	/* Poolers */
	PoolCollection statelessPool;
	PoolCollection statePool;

	private ILayoutGenerator generator;

	// Extract the list corresponding to the subblock of the segment
	private List<char> extractSegmentList(List<char> wholeLevel, int xSegment, int ySegment, bool verbose, int xSize, int ySize)
	{
		List<char> subList = new List<char>();

		if(verbose)
		{
			Debug.Log("ySegments : " + ySegments + " ySegment " + ySegment );
		}

		int originX = xSegment * xTilePerSegment;
		
		int startY = (ySegments -1 - ySegment); // inverse the y
		int originY = startY * yTilePerSegment + ySize - 1;

		int index = (originY * xTotalLevel) + originX;

		if(verbose)
		{
			Debug.Log("originX : " + originX + " originY " + originY + "index " + index + "total : " + wholeLevel.Count);
			Debug.Log("xSize : " + xSize + " ySize " + ySize );
		}

		List<List<char>> lists = new List<List<char>>();
		int subListIndex = 0;

		for(int y = 0; y < ySize; ++y)
		{
			lists.Add(new List<char>());

			for(int x = 0; x < xSize; ++x)
			{
				if(verbose)
				{
					Debug.Log("index : " + index + " value " + wholeLevel[index]);
				}

				if(index >= wholeLevel.Count)
				{
					Debug.LogError("Index : " + index + " origin : " + originX + "," + originY);
				}
				lists[subListIndex].Add(wholeLevel[index]);
				++index;
			}
			index--; // remove the ++ of the end loop above
			index -= (xSize - 1); // Should realign on the first of this line
			index -= xTotalLevel; 
			subListIndex++;
		}
		//subList.Reverse();
		lists.Reverse();
		foreach(List<char> lc in lists)
		{
			subList.AddRange(lc);
		}

		return subList;
	}

	public void createSegments()
	{
		List<char> level = generator.getLayout();
		FileUtils.FileSize levelSize = generator.getLevelSize();
		xTotalLevel = levelSize.xSize;
		yTotalLevel = levelSize.ySize;

		Debug.Log(SegmentStreamer.layoutAsString(level, xTotalLevel, yTotalLevel));

		xSegments = (int)Mathf.Ceil((float)xTotalLevel / (float)xTilePerSegment);
		ySegments = (int)Mathf.Ceil((float)yTotalLevel / (float)yTilePerSegment);

		Debug.Log("Level Size : " + xTotalLevel + ", " + yTotalLevel + " segments :" + xSegments + ", " + ySegments);

		int segmentNumber = 1;

		for(int x = 0; x < xSegments; ++x)
		{
			for(int y = 0; y < ySegments; ++y)
			{
				int xSize = xTotalLevel - x * xTilePerSegment;
				xSize = Mathf.Clamp(xSize, 1, xTilePerSegment); 

				int ySize = yTotalLevel - y * yTilePerSegment;
				ySize = Mathf.Clamp(ySize, 1, yTilePerSegment);

				float xBegin = x * xTilePerSegment * tileWidth + bottomLeftXPos;
				float yBegin = y * yTilePerSegment * tileHeight + bottomLeftYPos;

				bool verbose = x == 0 && y ==0;

				segments.Add(new Segment(xSize, ySize, xBegin, yBegin, extractSegmentList(level, x, y, verbose, xSize, ySize), tileWidth, tileHeight));
				segments[segments.Count -1].setName(segmentNumber.ToString());

				if(verbose)
				{
					Debug.Log(segments[segments.Count -1].layoutAsString());
				}
				segmentNumber++;
			}
		}

	}

	public void printSegments()
	{
		foreach(Segment s in segments)
		{
			Debug.Log(s.presentation());
		}
	}

	public void Awake()
	{
		generator = new BasicLevelGenerator(BasicLevelGenerator.GenerationStyle.Random);
		generator.generateLayout();

		segments = new List<Segment>();

		createSegments();

		statePool = new PoolCollection();

		statePool.addPool(landTiles, PoolIndexes.earthIndex);
		statePool.addPool(inverseLandTiles, PoolIndexes.inverseEarthIndex);
		statePool.addPool(waterTiles, PoolIndexes.waterIndex);
		statePool.addPool(hurtTiles, PoolIndexes.hurtIndex);
		statePool.addPool(objectTiles, PoolIndexes.objectIndex);
		statePool.addPool(enemies, PoolIndexes.enemiesIndex);
		statePool.addPool(disapearingTile, PoolIndexes.disapearingIndex);
		statePool.addPool(instancePlayer, PoolIndexes.playerIndex);
		statePool.addPool(checkpoint, PoolIndexes.checkpointIndex);
		statePool.addPool(escalator, PoolIndexes.escalatorIndex);
		statePool.addPool(movingTile, PoolIndexes.movingIndex);
		statePool.addPool(killMovingTile, PoolIndexes.killMovingIndex);

		printSegments();

		foreach(Segment s in segments)
		{
			s.load(statePool);
		}

	}
}