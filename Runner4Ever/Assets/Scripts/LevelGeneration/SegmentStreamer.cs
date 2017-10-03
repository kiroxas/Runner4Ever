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

public class PoolIndexes
{
	public static int uniquePoolingStrategy = 1;
	public static int smallPoolingStrategy = 10;
	public static int mediumPoolingStrategy = 50;
	public static int bigPoolingStrategy = 250;

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

	public static int[] stateIndexes = { objectIndex, enemiesIndex, disapearingIndex, escalatorIndex, movingIndex, killMovingIndex, checkpointIndex, playerIndex};

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
	private bool enabled = false;

	private List<char> layout;
	private int xSize;
	private int ySize;

	private float xBegin;
	private float yBegin;

	private float tileHeight;
	private float tileWidth;

	public int xGrid;
	public int yGrid;

	private string name;

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

public enum SegmentStrategy
{
	LoadAll,
	NineGrid
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
	PoolCollection statePool;

	public SegmentStrategy strat = SegmentStrategy.NineGrid;
	private Vector2 oldPlayerPlacement;

	private ILayoutGenerator generator;

	// Extract the list corresponding to the subblock of the segment
	private List<char> extractSegmentList(List<char> wholeLevel, int xSegment, int ySegment, bool verbose, int xSize, int ySize)
	{
		List<char> subList = new List<char>();

		if(verbose)
		{
			Debug.Log("ySegments : " + ySegments + " ySegment " + ySegment );
			Debug.Log("xSegments : " + xSegments + " xSegment " + xSegment );
		}

		int originX = xSegment * xTilePerSegment;
		
		int startY = (ySegments -1 - ySegment); // inverse the y
		int originY = startY * yTilePerSegment + ySize - 1;
		originY = Mathf.Clamp(originY, 1, yTotalLevel - 1);

		int index = (originY * xTotalLevel) + originX;

		if(verbose)
		{
			Debug.Log("startY " + startY + " originX : " + originX + " originY " + originY + " index " + index + " total : " + wholeLevel.Count);
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

		xSegments = (int)Mathf.Ceil((float)xTotalLevel / (float)xTilePerSegment);
		ySegments = (int)Mathf.Ceil((float)yTotalLevel / (float)yTilePerSegment);

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

				bool verbose = false;

				segments.Add(new Segment(xSize, ySize, xBegin, yBegin, extractSegmentList(level, x, y, verbose, xSize, ySize), tileWidth, tileHeight, x, y));
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

	public void attachPlayerToCamera()
	{
    	GameObject player = statePool.getUsedFromPool(PoolIndexes.playerIndex);

    	if(player == null)
    	{
    		Debug.LogError("Could not find the player instance via the poolCoolection");
    	}

    	CameraFollow camera = FindObjectOfType<CameraFollow>();

    	if(camera == null)
    	{
    		Debug.LogError("Could not find the cameraFollow script in the scene");
    	}

    	if(camera && player)
    	{
    		camera.target = player.GetComponent<Transform>(); 
    	}
	}

	public Vector2 getPlayerSegment()
	{
		GameObject player = statePool.getUsedFromPool(PoolIndexes.playerIndex);
		if(player == null || player.GetComponent<Transform>() == null)
    	{
    		return Vector2.zero;
    	}

		Vector3 position = player.GetComponent<Transform>().position;

		float xSegmentSize = xTilePerSegment * tileWidth;
		int xGridIndex = (int)Mathf.Floor(position.x / xSegmentSize);

		float ySegmentSize = yTilePerSegment * tileHeight;
		int yGridIndex = (int)Mathf.Floor(position.y / ySegmentSize);

		return new Vector2(xGridIndex, yGridIndex);
	}

	public List<Segment> nineGridSegments(Vector2 gridPos)
	{
		List<Segment> seg = new List<Segment>();

		foreach (Segment s in segments)
		{
			if(((s.xGrid == gridPos.x || s.xGrid == gridPos.x - 1 || s.xGrid == gridPos.x + 1) && s.yGrid == gridPos.y)
			|| ((s.yGrid == gridPos.y || s.yGrid == gridPos.y - 1 || s.yGrid == gridPos.y + 1) && s.xGrid == gridPos.x))
			{
				seg.Add(s);
			}
		}

		return seg;
	}

	public void loadInitSegments()
	{
		if(strat == SegmentStrategy.LoadAll)
		{
			/* load all */
			foreach(Segment s in segments)
			{
				s.enable(statePool);
			}
		}
		else if(strat == SegmentStrategy.NineGrid)
		{
			oldPlayerPlacement = getPlayerSegment();
			foreach(Segment s in nineGridSegments(oldPlayerPlacement))
			{
				s.enable(statePool);
			}
		}
	}

	public void updateSegments()
	{
		 if(strat == SegmentStrategy.NineGrid)
		 {
		 	Vector2 gridIndex = getPlayerSegment();

		 	if(gridIndex != oldPlayerPlacement)
		 	{
		 		oldPlayerPlacement = gridIndex;
		 		var ngs = nineGridSegments(gridIndex);

		 		// first disable, to liberate space in pool
		 		foreach(Segment s in segments)
		 		{
		 			if(s.isEnabled() && ngs.Contains(s) == false)
		 			{
		 				s.disable(statePool);
		 			}
		 		}

		 		// then enable
		 		foreach(Segment s in ngs)
		 		{
		 			s.enable(statePool);
		 		}
		 	}

		 }
	}

	public void Awake()
	{
		generator = new BasicLevelGenerator(BasicLevelGenerator.GenerationStyle.InOrder);
		generator.generateLayout();

		segments = new List<Segment>();

		createSegments();

		statePool = new PoolCollection();

		statePool.addPool(landTiles, PoolIndexes.earthIndex, PoolIndexes.bigPoolingStrategy);
		statePool.addPool(inverseLandTiles, PoolIndexes.inverseEarthIndex, PoolIndexes.mediumPoolingStrategy);
		statePool.addPool(waterTiles, PoolIndexes.waterIndex , PoolIndexes.mediumPoolingStrategy);
		statePool.addPool(hurtTiles, PoolIndexes.hurtIndex, PoolIndexes.mediumPoolingStrategy);
		statePool.addPool(objectTiles, PoolIndexes.objectIndex, PoolIndexes.mediumPoolingStrategy);
		statePool.addPool(enemies, PoolIndexes.enemiesIndex, PoolIndexes.mediumPoolingStrategy);
		statePool.addPool(disapearingTile, PoolIndexes.disapearingIndex, PoolIndexes.mediumPoolingStrategy);
		statePool.addPool(instancePlayer, PoolIndexes.playerIndex , PoolIndexes.uniquePoolingStrategy);
		statePool.addPool(checkpoint, PoolIndexes.checkpointIndex, PoolIndexes.smallPoolingStrategy);
		statePool.addPool(escalator, PoolIndexes.escalatorIndex, PoolIndexes.smallPoolingStrategy);
		statePool.addPool(movingTile, PoolIndexes.movingIndex, PoolIndexes.smallPoolingStrategy);
		statePool.addPool(killMovingTile, PoolIndexes.killMovingIndex, PoolIndexes.smallPoolingStrategy);

		//printSegments();

		loadInitSegments();

		attachPlayerToCamera();
	}

	public void Update()
	{
		updateSegments();
	}
}