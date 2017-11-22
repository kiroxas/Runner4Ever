using UnityEngine;
using System;
using System.IO;
using System.Xml.Serialization;
using System.Xml;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Random = UnityEngine.Random;
using System.Linq;

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

	public static List<T> Rotate<T>(ref List<T> list, int offset)
	{
    	return list.Skip(offset).Concat(list.Take(offset)).ToList();
	}

	public static List<T> Symetry<T>(List<T> list, int xSize, int ySize)
	{
    	List<T> symList = new List<T>();

    	for(int y = ySize - 1; y >= 0; --y)
    	{
    		int begR = y * xSize;
    		symList.AddRange(list.GetRange( begR , xSize));
    	}

    	return symList;
	}

	public static string layoutAsString(List<FileUtils.Glyph> layout, int xSize, int ySize)
	{
		StringBuilder sb = new StringBuilder();

		int index = 0;
		for(int y =0; y < ySize; ++y)
		{
			for(int x = 0; x < xSize; ++x)
			{
				sb.Append(layout[index].getFull());
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
	public GameObject finalCheckpoint;
	public GameObject startCheckpoint;

	/* Stateless prefabs */
	public GameObject hurtTiles;
	public GameObject bumper;
	public GameObject jumper;

	/* State prefabs */
	public GameObject objectTiles;
	public GameObject enemies;
	public GameObject escalator;
	public GameObject movingTile;
	public GameObject killMovingTile;
	public GameObject stopTile;
	public GameObject accelerateTile;
	public GameObject decelerateTile;
	public GameObject triggerTile;
	public GameObject doorTile;

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

	public GameObject[] topWaterTiles;
	public GameObject[] innerWaterTiles;

	public GameObject[] topRightInvertTiles;
	public GameObject[] topLeftInvertTiles;
	public GameObject[] rightInvertTiles;
	public GameObject[] leftInvertTiles;
	public GameObject[] topPillarInvertTiles;
	public GameObject[] innerPillarInvertTiles;

	public GameObject[] standOnTiles;

	public GameObject[] disapearingTiles;

	/* Poolers */
	PoolCollection statePool;
	BackgroundPropsHandler bgHandler;
	TilesSuperHandler tilesHandler;

	public Bounds containingBox;
	public int propsPerSegment = 3;

	public SegmentStrategy strat = SegmentStrategy.NineGrid;
	private List<Vector2> oldPlayerPlacement; // player placement at precedent frame, int he segment grid
	private List<Vector2> startPositions; // start checkpoint Positions

	private ILayoutGenerator generator; // abstract class to generate the layout

	private List<Segment> findAllSegmentWithCheckpoint()
	{
		List<Segment> segmentCheckpoint = new List<Segment>();

		foreach(Segment s in segments)
		{
			if(s.containsCheckpoint())
			{
				segmentCheckpoint.Add(s);
			}
		}

		return segmentCheckpoint;
	}

	private Vector2 findFirstSegmentWithCheckpoint()
	{
		var segs = findAllSegmentWithCheckpoint();

		if(segs.Any() == false)
			return Vector2.zero;

		Vector2 min = new Vector2(float.PositiveInfinity, float.PositiveInfinity);

		for (int i = 0; i < segs.Count; i++)
		{
			Vector2 s = segs[i].getSegmentGridPlacement();
            if (s.x < min.x ) 
            	min = s;
        }

		return min;
	}

	private void fillStartPositions()
	{
		startPositions = new List<Vector2>();

		var segs = findAllSegmentWithCheckpoint();

		foreach(Segment s in segs)
		{
			startPositions.AddRange(s.startCheckpointPositions());
		}
	}

	public List<Vector2> getStartPositions()
	{
		fillStartPositions();
		return startPositions;
	}

	private int getSmallestYCell()
	{
		int yCellsBestCase = yTilePerSegment * ySegments;
		int difference = yCellsBestCase - yTotalLevel;

		return yTilePerSegment - difference;
	}

	// Extract the list corresponding to the subblock of the segment
	private List<FileUtils.Glyph> extractSegmentList(List<FileUtils.Glyph> wholeLevel, int xSegment, int ySegment, bool verbose, int xSize, int ySize)
	{
		List<FileUtils.Glyph> subList = new List<FileUtils.Glyph>();

		int xOrigin = xSegment * xTilePerSegment;
		int yOrigin = ySegment * yTilePerSegment;

		for(int y = 0; y <ySize; ++y)
		{
			for(int x = 0; x < xSize; ++x)
			{
				int index = Segment.getStaticIndex(xOrigin + x, yOrigin + y, xTotalLevel, yTotalLevel);
				subList.Add(wholeLevel[index]);
			}
		}

		return subList;
	}

	private Dictionary<int, List<Deepness>> extractDeepness(Dictionary<int, List<Deepness>> deepness, int xSegment, int ySegment, int xSize, int ySize)
	{
		Dictionary<int, List<Deepness>> deep = new Dictionary<int, List<Deepness>>();

		foreach(int key in deepness.Keys)
		{
			deep[key] = new List<Deepness>();
		}

		int xOrigin = xSegment * xTilePerSegment;
		int yOrigin = ySegment * yTilePerSegment;

		for(int y = 0; y < ySize; ++y)
		{
			for(int x = 0; x < xSize; ++x)
			{
				int index = Segment.getStaticIndex(xOrigin + x, yOrigin + y, xTotalLevel, yTotalLevel);
				foreach(int key in deepness.Keys)
				{
					deep[key].Add(deepness[key][index]);
				}
			}
		}

		return deep;
	}

	private Dictionary<int, List<Deepness>> calculateDeepnesses(List<FileUtils.Glyph> level)
	{
		Dictionary<int, List<Deepness>> deep = new Dictionary<int, List<Deepness>>();

		deep[PoolIndexes.earthIndex] = Deepness.calculateDeepness(new List<int>{PoolIndexes.earthIndex, PoolIndexes.inverseEarthIndex, PoolIndexes.stopTileIndex, PoolIndexes.accelerateTileIndex, PoolIndexes.decelerateTileIndex}, level, xTotalLevel, yTotalLevel);
		deep[PoolIndexes.waterIndex] = Deepness.calculateDeepness(new List<int>{PoolIndexes.waterIndex}, level, xTotalLevel, yTotalLevel);
		deep[PoolIndexes.standOnIndex] = Deepness.calculateDeepness(new List<int>{PoolIndexes.standOnIndex}, level, xTotalLevel, yTotalLevel);
		deep[PoolIndexes.inverseEarthIndex] = Deepness.calculateDeepness(new List<int>{PoolIndexes.earthIndex, PoolIndexes.inverseEarthIndex}, level, xTotalLevel, yTotalLevel);
		deep[PoolIndexes.disapearingIndex] = Deepness.calculateDeepness(new List<int>{PoolIndexes.disapearingIndex}, level, xTotalLevel, yTotalLevel);

		return deep;
	}

	public void createSegments()
	{
		List<FileUtils.Glyph> level = generator.getLayout();
		FileUtils.FileSize levelSize = generator.getLevelSize();
		xTotalLevel = levelSize.xSize;
		yTotalLevel = levelSize.ySize;

		xSegments = (int)Mathf.Ceil((float)xTotalLevel / (float)xTilePerSegment);
		ySegments = (int)Mathf.Ceil((float)yTotalLevel / (float)yTilePerSegment);

		level = Symetry(level, xTotalLevel, yTotalLevel);
	
		var deep = calculateDeepnesses(level);
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
				var segmentDeepness = extractDeepness(deep, x, y, xSize, ySize);

				segments.Add(new Segment(info, extractSegmentList(level, x, y, verbose, xSize, ySize), segmentDeepness));
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
    	if(startPositions.Count > 0)
    	{
    		GameObject player = statePool.getFromPool(PoolIndexes.playerIndex, startPositions[0]);

    		EventManager.TriggerEvent(EventManager.get().playerSpawnEvent, new GameConstants.PlayerSpawnArgument(player, 
    																											 startPositions[0].x,
    																											 startPositions[0].y));
    	}
	}

	private Vector2 getPositionToSegment(Vector3 worldPosition)
	{
		Vector3 position = worldPosition;

		float xSegmentSize = xTilePerSegment * tileWidth;
		int xGridIndex = (int)Mathf.Floor(position.x / xSegmentSize);

		float ySegmentSize = yTilePerSegment * tileHeight;
		int yGridIndex = (int)Mathf.Floor(position.y / ySegmentSize);

		return new Vector2(xGridIndex, yGridIndex);
	}

	public List<Vector2> getPlayerSegment()
	{
		GameObject[] players = GameObject.FindGameObjectsWithTag(GameConstants.playerTag);
		if(players == null)
    	{
    		return new List<Vector2>{findFirstSegmentWithCheckpoint()};
    	}

    	List<Vector2> segmentPositions = new List<Vector2>();

    	foreach(GameObject g in players)
    	{
    		segmentPositions.Add(getPositionToSegment(g.GetComponent<Transform>().position));
    	}

		return segmentPositions;
	}

	public List<Segment> nineGridSegments(List<Vector2> gridPosList)
	{
		List<Segment> seg = new List<Segment>();

		foreach (Segment s in segments)
		{
			foreach(Vector2 gridPos in gridPosList)
			{
				if((s.info.layoutXGrid == gridPos.x || s.info.layoutXGrid == gridPos.x - 1 || s.info.layoutXGrid == gridPos.x + 1) 
				&& (s.info.layoutYGrid == gridPos.y || s.info.layoutYGrid == gridPos.y - 1 || s.info.layoutYGrid == gridPos.y + 1))
				{
					seg.Add(s);
					break;
				}
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
				foreach(Vector2 pos in oldPlayerPlacement)
				{
					if(s.info.layoutXGrid == pos.x && s.info.layoutYGrid == pos.y)
					{
						s.enable(statePool, bgHandler, tilesHandler);
						ev.add(s.getBounds());
						break;
					}
				}
			}
		}

		EventManager.TriggerEvent(EventManager.get().segmentsUpdatedEvent, ev);
	}

	public void updateSegments()
	{
		if(strat == SegmentStrategy.NineGrid)
		{	 	
		 	List<Vector2> gridIndex = getPlayerSegment();

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
			List<Vector2> gridIndex = getPlayerSegment();

			if(gridIndex != oldPlayerPlacement)
		 	{
		 		GameConstants.SegmentsUpdatedArgument ev = new GameConstants.SegmentsUpdatedArgument();

		 		// Disable
				foreach(Segment s in segments)
				{
					if(oldPlayerPlacement.Contains(s.getSegmentGridPlacement()) && gridIndex.Contains(s.getSegmentGridPlacement()) == false)
					{
						s.disable(statePool, bgHandler, tilesHandler);
					}
				}

				// Enable
				foreach(Segment s in segments)
				{
					if(gridIndex.Contains(s.getSegmentGridPlacement()))
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

		statePool.addPool(hurtTiles, PoolIndexes.hurtIndex, PoolIndexes.mediumPoolingStrategy);
		statePool.addPool(objectTiles, PoolIndexes.objectIndex, PoolIndexes.mediumPoolingStrategy);
		statePool.addPool(enemies, PoolIndexes.enemiesIndex, PoolIndexes.mediumPoolingStrategy);
		statePool.addPool(instancePlayer, PoolIndexes.playerIndex , PoolIndexes.uniquePoolingStrategy);
		statePool.addPool(checkpoint, PoolIndexes.checkpointIndex, PoolIndexes.smallPoolingStrategy);
		statePool.addPool(escalator, PoolIndexes.escalatorIndex, PoolIndexes.smallPoolingStrategy);
		statePool.addPool(movingTile, PoolIndexes.movingIndex, PoolIndexes.smallPoolingStrategy);
		statePool.addPool(killMovingTile, PoolIndexes.killMovingIndex, PoolIndexes.smallPoolingStrategy);
		statePool.addPool(bumper, PoolIndexes.bumperIndex, PoolIndexes.smallPoolingStrategy);
		statePool.addPool(jumper, PoolIndexes.jumperIndex, PoolIndexes.smallPoolingStrategy);
		statePool.addPool(finalCheckpoint, PoolIndexes.finalCheckpointIndex, PoolIndexes.smallPoolingStrategy);
		statePool.addPool(stopTile, PoolIndexes.stopTileIndex, PoolIndexes.smallPoolingStrategy);
		statePool.addPool(accelerateTile, PoolIndexes.accelerateTileIndex, PoolIndexes.smallPoolingStrategy);
		statePool.addPool(decelerateTile, PoolIndexes.decelerateTileIndex, PoolIndexes.smallPoolingStrategy);
		statePool.addPool(startCheckpoint, PoolIndexes.startCheckpointIndex, PoolIndexes.smallPoolingStrategy);
		statePool.addPool(triggerTile, PoolIndexes.triggerIndex, PoolIndexes.smallPoolingStrategy);
		statePool.addPool(doorTile, PoolIndexes.doorIndex, PoolIndexes.smallPoolingStrategy);

		bgHandler = new BackgroundPropsHandler(propsPerSegment, backgroundObjects, tileWidth);
		tilesHandler = new TilesSuperHandler();

		// Earth
		tilesHandler.addTileType(PoolIndexes.earthIndex, TilesHandler.TilePlacement.OnTop, topLandTiles);
		tilesHandler.addTileType(PoolIndexes.earthIndex, TilesHandler.TilePlacement.BelowTop, topInnerLandTiles);
		tilesHandler.addTileType(PoolIndexes.earthIndex, TilesHandler.TilePlacement.Right, rightLandTiles);
		tilesHandler.addTileType(PoolIndexes.earthIndex, TilesHandler.TilePlacement.Left, leftLandTiles);
		tilesHandler.addTileType(PoolIndexes.earthIndex, TilesHandler.TilePlacement.InnerRight, rightInnerLandTiles);
		tilesHandler.addTileType(PoolIndexes.earthIndex, TilesHandler.TilePlacement.InnerLeft, leftInnerLandTiles);
		tilesHandler.addTileType(PoolIndexes.earthIndex, TilesHandler.TilePlacement.Inner, innerLandTiles);
		tilesHandler.addTileType(PoolIndexes.earthIndex, TilesHandler.TilePlacement.Bottom, bottomLandTiles);
		tilesHandler.addTileType(PoolIndexes.earthIndex, TilesHandler.TilePlacement.InnerBottom, bottomInnerLandTiles);
		tilesHandler.addTileType(PoolIndexes.earthIndex, TilesHandler.TilePlacement.Floating, floatingTiles);
		tilesHandler.addTileType(PoolIndexes.earthIndex, TilesHandler.TilePlacement.BottomLeft, bottomLeftTiles);
		tilesHandler.addTileType(PoolIndexes.earthIndex, TilesHandler.TilePlacement.BottomRight, bottomRightTiles);
		tilesHandler.addTileType(PoolIndexes.earthIndex, TilesHandler.TilePlacement.TopLeft, topLeftTiles);
		tilesHandler.addTileType(PoolIndexes.earthIndex, TilesHandler.TilePlacement.TopRight, topRightTiles);
		tilesHandler.addTileType(PoolIndexes.earthIndex, TilesHandler.TilePlacement.PillarUp, topLandTiles);
		tilesHandler.addTileType(PoolIndexes.earthIndex, TilesHandler.TilePlacement.PillarInner, innerLandTiles);
		tilesHandler.addTileType(PoolIndexes.earthIndex, TilesHandler.TilePlacement.PillarBottom, bottomLandTiles);

		// Water
		tilesHandler.addTileType(PoolIndexes.waterIndex, TilesHandler.TilePlacement.OnTop, topWaterTiles);
		tilesHandler.addTileType(PoolIndexes.waterIndex, TilesHandler.TilePlacement.BelowTop, innerWaterTiles);
		tilesHandler.addTileType(PoolIndexes.waterIndex, TilesHandler.TilePlacement.Right, innerWaterTiles);
		tilesHandler.addTileType(PoolIndexes.waterIndex, TilesHandler.TilePlacement.Left, innerWaterTiles);
		tilesHandler.addTileType(PoolIndexes.waterIndex, TilesHandler.TilePlacement.InnerRight, innerWaterTiles);
		tilesHandler.addTileType(PoolIndexes.waterIndex, TilesHandler.TilePlacement.InnerLeft, innerWaterTiles);
		tilesHandler.addTileType(PoolIndexes.waterIndex, TilesHandler.TilePlacement.Inner, innerWaterTiles);
		tilesHandler.addTileType(PoolIndexes.waterIndex, TilesHandler.TilePlacement.Bottom, innerWaterTiles);
		tilesHandler.addTileType(PoolIndexes.waterIndex, TilesHandler.TilePlacement.InnerBottom, innerWaterTiles);
		tilesHandler.addTileType(PoolIndexes.waterIndex, TilesHandler.TilePlacement.Floating, innerWaterTiles);
		tilesHandler.addTileType(PoolIndexes.waterIndex, TilesHandler.TilePlacement.BottomLeft, innerWaterTiles);
		tilesHandler.addTileType(PoolIndexes.waterIndex, TilesHandler.TilePlacement.BottomRight, innerWaterTiles);
		tilesHandler.addTileType(PoolIndexes.waterIndex, TilesHandler.TilePlacement.TopLeft, topWaterTiles);
		tilesHandler.addTileType(PoolIndexes.waterIndex, TilesHandler.TilePlacement.TopRight, topWaterTiles);
		tilesHandler.addTileType(PoolIndexes.waterIndex, TilesHandler.TilePlacement.PillarUp, topWaterTiles);
		tilesHandler.addTileType(PoolIndexes.waterIndex, TilesHandler.TilePlacement.PillarInner, innerWaterTiles);
		tilesHandler.addTileType(PoolIndexes.waterIndex, TilesHandler.TilePlacement.PillarBottom, innerWaterTiles);

		// Invert
		tilesHandler.addTileType(PoolIndexes.inverseEarthIndex, TilesHandler.TilePlacement.OnTop, topRightInvertTiles);
		tilesHandler.addTileType(PoolIndexes.inverseEarthIndex, TilesHandler.TilePlacement.BelowTop, rightInvertTiles);
		tilesHandler.addTileType(PoolIndexes.inverseEarthIndex, TilesHandler.TilePlacement.Right, rightInvertTiles);
		tilesHandler.addTileType(PoolIndexes.inverseEarthIndex, TilesHandler.TilePlacement.Left, leftInvertTiles);
		tilesHandler.addTileType(PoolIndexes.inverseEarthIndex, TilesHandler.TilePlacement.InnerRight, topRightInvertTiles);
		tilesHandler.addTileType(PoolIndexes.inverseEarthIndex, TilesHandler.TilePlacement.InnerLeft, topRightInvertTiles);
		tilesHandler.addTileType(PoolIndexes.inverseEarthIndex, TilesHandler.TilePlacement.Inner, topRightInvertTiles);
		tilesHandler.addTileType(PoolIndexes.inverseEarthIndex, TilesHandler.TilePlacement.Bottom, rightInvertTiles);
		tilesHandler.addTileType(PoolIndexes.inverseEarthIndex, TilesHandler.TilePlacement.InnerBottom, topRightInvertTiles);
		tilesHandler.addTileType(PoolIndexes.inverseEarthIndex, TilesHandler.TilePlacement.Floating, topRightInvertTiles);
		tilesHandler.addTileType(PoolIndexes.inverseEarthIndex, TilesHandler.TilePlacement.BottomLeft, leftInvertTiles);
		tilesHandler.addTileType(PoolIndexes.inverseEarthIndex, TilesHandler.TilePlacement.BottomRight, rightInvertTiles);
		tilesHandler.addTileType(PoolIndexes.inverseEarthIndex, TilesHandler.TilePlacement.TopLeft, topLeftInvertTiles);
		tilesHandler.addTileType(PoolIndexes.inverseEarthIndex, TilesHandler.TilePlacement.TopRight, topRightInvertTiles);
		tilesHandler.addTileType(PoolIndexes.inverseEarthIndex, TilesHandler.TilePlacement.PillarUp, topPillarInvertTiles);
		tilesHandler.addTileType(PoolIndexes.inverseEarthIndex, TilesHandler.TilePlacement.PillarInner, innerPillarInvertTiles);
		tilesHandler.addTileType(PoolIndexes.inverseEarthIndex, TilesHandler.TilePlacement.PillarBottom, innerPillarInvertTiles);

		// StandOn
		tilesHandler.addTileType(PoolIndexes.standOnIndex, TilesHandler.TilePlacement.OnTop, standOnTiles);
		tilesHandler.addTileType(PoolIndexes.standOnIndex, TilesHandler.TilePlacement.BelowTop, standOnTiles);
		tilesHandler.addTileType(PoolIndexes.standOnIndex, TilesHandler.TilePlacement.Right, standOnTiles);
		tilesHandler.addTileType(PoolIndexes.standOnIndex, TilesHandler.TilePlacement.Left, standOnTiles);
		tilesHandler.addTileType(PoolIndexes.standOnIndex, TilesHandler.TilePlacement.InnerRight, standOnTiles);
		tilesHandler.addTileType(PoolIndexes.standOnIndex, TilesHandler.TilePlacement.InnerLeft, standOnTiles);
		tilesHandler.addTileType(PoolIndexes.standOnIndex, TilesHandler.TilePlacement.Inner, standOnTiles);
		tilesHandler.addTileType(PoolIndexes.standOnIndex, TilesHandler.TilePlacement.Bottom, standOnTiles);
		tilesHandler.addTileType(PoolIndexes.standOnIndex, TilesHandler.TilePlacement.InnerBottom, standOnTiles);
		tilesHandler.addTileType(PoolIndexes.standOnIndex, TilesHandler.TilePlacement.Floating, standOnTiles);
		tilesHandler.addTileType(PoolIndexes.standOnIndex, TilesHandler.TilePlacement.BottomLeft, standOnTiles);
		tilesHandler.addTileType(PoolIndexes.standOnIndex, TilesHandler.TilePlacement.BottomRight, standOnTiles);
		tilesHandler.addTileType(PoolIndexes.standOnIndex, TilesHandler.TilePlacement.TopLeft, standOnTiles);
		tilesHandler.addTileType(PoolIndexes.standOnIndex, TilesHandler.TilePlacement.TopRight, standOnTiles);
		tilesHandler.addTileType(PoolIndexes.standOnIndex, TilesHandler.TilePlacement.PillarUp, standOnTiles);
		tilesHandler.addTileType(PoolIndexes.standOnIndex, TilesHandler.TilePlacement.PillarInner, standOnTiles);
		tilesHandler.addTileType(PoolIndexes.standOnIndex, TilesHandler.TilePlacement.PillarBottom, standOnTiles);

		// Disapearing
		tilesHandler.addTileType(PoolIndexes.disapearingIndex, TilesHandler.TilePlacement.OnTop, disapearingTiles);
		tilesHandler.addTileType(PoolIndexes.disapearingIndex, TilesHandler.TilePlacement.BelowTop, disapearingTiles);
		tilesHandler.addTileType(PoolIndexes.disapearingIndex, TilesHandler.TilePlacement.Right, disapearingTiles);
		tilesHandler.addTileType(PoolIndexes.disapearingIndex, TilesHandler.TilePlacement.Left, disapearingTiles);
		tilesHandler.addTileType(PoolIndexes.disapearingIndex, TilesHandler.TilePlacement.InnerRight, disapearingTiles);
		tilesHandler.addTileType(PoolIndexes.disapearingIndex, TilesHandler.TilePlacement.InnerLeft, disapearingTiles);
		tilesHandler.addTileType(PoolIndexes.disapearingIndex, TilesHandler.TilePlacement.Inner, disapearingTiles);
		tilesHandler.addTileType(PoolIndexes.disapearingIndex, TilesHandler.TilePlacement.Bottom, disapearingTiles);
		tilesHandler.addTileType(PoolIndexes.disapearingIndex, TilesHandler.TilePlacement.InnerBottom, disapearingTiles);
		tilesHandler.addTileType(PoolIndexes.disapearingIndex, TilesHandler.TilePlacement.Floating, disapearingTiles);
		tilesHandler.addTileType(PoolIndexes.disapearingIndex, TilesHandler.TilePlacement.BottomLeft, disapearingTiles);
		tilesHandler.addTileType(PoolIndexes.disapearingIndex, TilesHandler.TilePlacement.BottomRight, disapearingTiles);
		tilesHandler.addTileType(PoolIndexes.disapearingIndex, TilesHandler.TilePlacement.TopLeft, disapearingTiles);
		tilesHandler.addTileType(PoolIndexes.disapearingIndex, TilesHandler.TilePlacement.TopRight, disapearingTiles);
		tilesHandler.addTileType(PoolIndexes.disapearingIndex, TilesHandler.TilePlacement.PillarUp, disapearingTiles);
		tilesHandler.addTileType(PoolIndexes.disapearingIndex, TilesHandler.TilePlacement.PillarInner, disapearingTiles);
		tilesHandler.addTileType(PoolIndexes.disapearingIndex, TilesHandler.TilePlacement.PillarBottom, disapearingTiles);
		
	}

	private void unpausePlayer()
	{
		EventManager.TriggerEvent(EventManager.get().unPausePlayerEvent, new GameConstants.UnPausePlayerArgument());
	}

	private void pausePlayer()
	{
		EventManager.TriggerEvent(EventManager.get().pausePlayerEvent, new GameConstants.PausePlayerArgument());
	}

	public void load(GameConstants.LoadLevelArgument arg)
	{
		generator = new BasicFileLevelLoader(arg.levelName);
		generator.generateLayout();

		createSegments();
		fillContainingBox();
		fillStartPositions();
		loadInitSegments();

		if(arg.isNetworkGame() == false)
		{
			createPlayer();
			Invoke("unpausePlayer", 2.5f);
		}

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