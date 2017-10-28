using UnityEngine;
using System;
using System.IO;
using System.Xml.Serialization;
using System.Xml;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Random = UnityEngine.Random;

// How will we load the segments
public enum SegmentStrategy
{
	LoadAll,
	NineGrid,
	LoadOne
}

public class SegmentInfo
{
	public float xBegin;
	public float yBegin;
	public float tileWidth;
	public float tileHeight;
	public int xSize;
	public int ySize;
	public int layoutXGrid;
	public int layoutYGrid;

	public SegmentInfo(float x, float y, float w, float h, int xS, int yS, int lxg, int lyg)
	{
		xBegin = x;
		yBegin = y;
		tileHeight = h;
		tileWidth = w;
		xSize = xS;
		ySize = yS;
		layoutXGrid = lxg;
		layoutYGrid = lyg;
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

	private int xSegments; // number of segments on x
	private int ySegments; // number of segments on y
	private List<Segment> segments; // list of the segments

	public int xTilePerSegment = 6; 
	public int yTilePerSegment = 6;

	public float tileWidth = 1.28f;
	public float tileHeight = 1.28f;

	public float bottomLeftXPos = 0;
	public float bottomLeftYPos = 0;

	/* Origin prefab */
	public GameObject instancePlayer;
	public GameObject checkpoint;

	/* Stateless prefabs */
	public GameObject inverseLandTiles;
	public GameObject waterTiles;
	public GameObject hurtTiles;
	public GameObject bumper;
	public GameObject standOn;
	public GameObject jumper;

	/* State prefabs */
	public GameObject objectTiles;
	public GameObject enemies;
	public GameObject disapearingTile;
	public GameObject escalator;
	public GameObject movingTile;
	public GameObject killMovingTile;

	/* Background */
	public GameObject[] backgroundObjects;

	public GameObject[] topLandTiles;
	public GameObject[] rightLandTiles;
	public GameObject[] leftLandTiles;
	public GameObject[] bottomLandTiles;
	public GameObject[] topInnerLandTiles;
	public GameObject[] rightInnerLandTiles;
	public GameObject[] leftInnerLandTiles;
	public GameObject[] bottomInnerLandTiles;
	public GameObject[] innerLandTiles;
	public GameObject[] floatingTiles;
	public GameObject[] bottomRightTiles;
	public GameObject[] bottomLeftTiles;
	public GameObject[] topRightTiles;
	public GameObject[] topLeftTiles;

	/* Poolers */
	PoolCollection statePool;
	BackgroundPropsHandler bgHandler;
	TilesHandler tilesHandler;

	public Bounds containingBox;
	public int propsPerSegment = 3;

	public SegmentStrategy strat = SegmentStrategy.NineGrid;
	private Vector2 oldPlayerPlacement; // player placement at precedent frame, int he segment grid

	private ILayoutGenerator generator; // abstract class to generate the layout

	private int getSmallestYCell()
	{
		int yCellsBestCase = yTilePerSegment * ySegments;
		int difference = yCellsBestCase - yTotalLevel;

		return yTilePerSegment - difference;
	}

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

		int fullYCells = startY - 1 > 0 ? startY - 1 : 0;
		int smallestYCell = startY > 0 ? 1 : 0;

		if(verbose)
		{
			Debug.Log("fullYCells : " + fullYCells + " smallestYCell  : " + smallestYCell + " smallestSize : " + getSmallestYCell());
		}

		int originY = (smallestYCell * getSmallestYCell()) + (fullYCells * yTilePerSegment) + ySize - 1;
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

				SegmentInfo info = new SegmentInfo(xBegin, yBegin, tileWidth, tileHeight, xSize, ySize, x, y);

				segments.Add(new Segment(info, extractSegmentList(level, x, y, verbose, xSize, ySize)));
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

	public void createPlayer()
	{
    	GameObject firstCheckpoint = CheckpointUtils.findFirstCheckpoint();

    	if(firstCheckpoint)
    	{
    		GameObject player = statePool.getFromPool(PoolIndexes.playerIndex, firstCheckpoint.GetComponent<Transform>().position);

    		EventManager.TriggerEvent(EventManager.get().playerSpawnEvent, new GameConstants.PlayerSpawnArgument(player, 
    																											 player.GetComponent<Transform>().position.x,
    																											 player.GetComponent<Transform>().position.y));
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
			if((s.info.layoutXGrid == gridPos.x || s.info.layoutXGrid == gridPos.x - 1 || s.info.layoutXGrid == gridPos.x + 1) && (s.info.layoutYGrid == gridPos.y || s.info.layoutYGrid == gridPos.y - 1 || s.info.layoutYGrid == gridPos.y + 1))
			{
				seg.Add(s);
			}
		}

		return seg;
	}

	public void loadInitSegments()
	{
		GameConstants.SegmentsUpdatedArgument ev = new GameConstants.SegmentsUpdatedArgument();

		if(strat == SegmentStrategy.LoadAll)
		{
			/* load all */
			foreach(Segment s in segments)
			{
				s.enable(statePool, bgHandler, tilesHandler);
				ev.add(s.getBounds());
			}
		}
		else if(strat == SegmentStrategy.NineGrid)
		{
			oldPlayerPlacement = getPlayerSegment();
			foreach(Segment s in nineGridSegments(oldPlayerPlacement))
			{
				s.enable(statePool, bgHandler, tilesHandler);
				ev.add(s.getBounds());
			}
		}
		else if(strat == SegmentStrategy.LoadOne)
		{
			oldPlayerPlacement = getPlayerSegment();
			foreach(Segment s in segments)
			{
				if(s.info.layoutXGrid == oldPlayerPlacement.x && s.info.layoutYGrid == oldPlayerPlacement.y)
				{
					s.enable(statePool, bgHandler, tilesHandler);
					ev.add(s.getBounds());
					break;
				}
			}
		}

		EventManager.TriggerEvent(EventManager.get().segmentsUpdatedEvent, ev);
	}

	public void updateSegments()
	{
		 if(strat == SegmentStrategy.NineGrid)
		 {	 	
		 	Vector2 gridIndex = getPlayerSegment();

		 	if(gridIndex != oldPlayerPlacement)
		 	{
		 		GameConstants.SegmentsUpdatedArgument ev = new GameConstants.SegmentsUpdatedArgument();
		 		oldPlayerPlacement = gridIndex;
		 		var ngs = nineGridSegments(gridIndex);

		 		// first disable, to liberate space in pool
		 		foreach(Segment s in segments)
		 		{
		 			if(s.isEnabled() && ngs.Contains(s) == false)
		 			{
		 				s.disable(statePool, bgHandler, tilesHandler);
		 			}
		 		}

		 		// then enable
		 		foreach(Segment s in ngs)
		 		{
		 			s.enable(statePool, bgHandler, tilesHandler);
		 			ev.add(s.getBounds());
		 		}

		 		EventManager.TriggerEvent(EventManager.get().segmentsUpdatedEvent, ev);
		 	}

		 }
		else if(strat == SegmentStrategy.LoadOne)
		{
			Vector2 gridIndex = getPlayerSegment();

			if(gridIndex != oldPlayerPlacement)
		 	{
		 		GameConstants.SegmentsUpdatedArgument ev = new GameConstants.SegmentsUpdatedArgument();

		 		// Disable
				foreach(Segment s in segments)
				{
					if(s.info.layoutXGrid == oldPlayerPlacement.x && s.info.layoutYGrid == oldPlayerPlacement.y)
					{
						s.disable(statePool, bgHandler, tilesHandler);
					}
				}

				// Enable
				foreach(Segment s in segments)
				{
					if(s.info.layoutXGrid == gridIndex.x && s.info.layoutYGrid == gridIndex.y)
					{
						s.enable(statePool, bgHandler, tilesHandler);
						ev.add(s.getBounds());
					}
				}

				oldPlayerPlacement = gridIndex;
				EventManager.TriggerEvent(EventManager.get().segmentsUpdatedEvent, ev);
			}
		}
	}

	public void fillContainingBox()
	{
		Vector3 center, size;
		
		float xSize = xTotalLevel * tileWidth;
		float ySize = yTotalLevel * tileHeight;
		size = new Vector3(xSize, ySize, 0);
		float xMiddle = bottomLeftXPos + size.x / 2.0f;
		float yMiddle = bottomLeftYPos + size.y / 2.0f;
		center = new Vector3(xMiddle - tileWidth / 2.0f, yMiddle - tileHeight / 2.0f , 0); // Because the anchor point of tiles are centered

		containingBox = new Bounds(center, size);
	}

	public void Awake()
	{
		segments = new List<Segment>();
		statePool = new PoolCollection();

		//statePool.addPool(landTiles, PoolIndexes.earthIndex, PoolIndexes.bigPoolingStrategy);
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
		statePool.addPool(bumper, PoolIndexes.bumperIndex, PoolIndexes.smallPoolingStrategy);
		statePool.addPool(standOn, PoolIndexes.standOnIndex, PoolIndexes.smallPoolingStrategy);
		statePool.addPool(jumper, PoolIndexes.jumperIndex, PoolIndexes.smallPoolingStrategy);
		
		bgHandler = new BackgroundPropsHandler(propsPerSegment, backgroundObjects, tileWidth);
		tilesHandler = new TilesHandler();

		tilesHandler.addTileType(TilesHandler.TilePlacement.OnTop, topLandTiles);
		tilesHandler.addTileType(TilesHandler.TilePlacement.BelowTop, topInnerLandTiles);
		tilesHandler.addTileType(TilesHandler.TilePlacement.Right, rightLandTiles);
		tilesHandler.addTileType(TilesHandler.TilePlacement.Left, leftLandTiles);
		tilesHandler.addTileType(TilesHandler.TilePlacement.InnerRight, rightInnerLandTiles);
		tilesHandler.addTileType(TilesHandler.TilePlacement.InnerLeft, leftInnerLandTiles);
		tilesHandler.addTileType(TilesHandler.TilePlacement.Inner, innerLandTiles);
		tilesHandler.addTileType(TilesHandler.TilePlacement.Bottom, bottomLandTiles);
		tilesHandler.addTileType(TilesHandler.TilePlacement.InnerBottom, bottomInnerLandTiles);
		tilesHandler.addTileType(TilesHandler.TilePlacement.Floating, floatingTiles);
		tilesHandler.addTileType(TilesHandler.TilePlacement.BottomLeft, bottomLeftTiles);
		tilesHandler.addTileType(TilesHandler.TilePlacement.BottomRight, bottomRightTiles);
		tilesHandler.addTileType(TilesHandler.TilePlacement.TopLeft, topLeftTiles);
		tilesHandler.addTileType(TilesHandler.TilePlacement.TopRight, topRightTiles);
	}

	public void load(GameConstants.LoadLevelArgument arg)
	{
		Debug.Log("Load level " + arg.levelName);

		generator = new BasicFileLevelLoader(arg.levelName);
		generator.generateLayout();

		createSegments();
		fillContainingBox();
		loadInitSegments();
		createPlayer();

		EventManager.TriggerEvent(EventManager.get().levelInitialisedEvent, new GameConstants.LevelInitialisedArgument());
	}

	public void Start()
	{
	}

	void OnEnable()
    {
        EventManager.StartListening(EventManager.get().loadLevelEvent, load);
    }

    void OnDisable ()
    {
        EventManager.StopListening(EventManager.get().loadLevelEvent, load);
    }

	public void Update()
	{
		updateSegments();
	}

	public void OnDrawGizmos() 
	{
		if(segments != null)
		{
			foreach(Segment s in segments)
			{
				s.OnDrawGizmos();
			}
		}
    }
}