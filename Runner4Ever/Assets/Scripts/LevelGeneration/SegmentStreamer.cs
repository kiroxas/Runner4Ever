using UnityEngine;
using System;
using System.IO;
using System.Xml.Serialization;
using System.Xml;
using System.Collections;
using System.Collections.Generic;
using Random = UnityEngine.Random;

// How will we load the segments
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
	public GameObject landTiles;
	public GameObject inverseLandTiles;
	public GameObject waterTiles;
	public GameObject hurtTiles;
	public GameObject bumper;
	public GameObject standOn;
	public GameObject jumper;

	/* State prefabs */
	public GameObject objectTiles;
	public GameObject hurtTiles;
	public GameObject enemies;
	public GameObject disapearingTile;
	public GameObject escalator;
	public GameObject movingTile;
	public GameObject killMovingTile;

	/* Poolers */

	PoolCollection statelessPool;
	PoolCollection statePool;

	public SegmentStrategy strat = SegmentStrategy.NineGrid;
	private Vector2 oldPlayerPlacement; // player placement at precedent frame, int he segment grid

	private ILayoutGenerator generator; // abstract class to generate the layout

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

	public void createPlayerAndAttachCamera()
	{
    	CameraFollow camera = FindObjectOfType<CameraFollow>();

    	if(camera == null)
    	{
    		Debug.LogError("Could not find the cameraFollow script in the scene");
    	}

    	GameObject firstCheckpoint = CheckpointUtils.findFirstCheckpoint();

    	if(camera && firstCheckpoint)
    	{
    		GameObject player = statePool.getFromPool(PoolIndexes.playerIndex, firstCheckpoint.GetComponent<Transform>().position);
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
		statelessPool = new PoolCollection();

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
		statePool.addPool(bumper, PoolIndexes.bumperIndex, PoolIndexes.smallPoolingStrategy);
		statePool.addPool(standOn, PoolIndexes.standOnIndex, PoolIndexes.smallPoolingStrategy);
		statePool.addPool(jumper, PoolIndexes.jumperIndex, PoolIndexes.smallPoolingStrategy);

		//printSegments();

		loadInitSegments();

		createPlayerAndAttachCamera();
	}

	public void Update()
	{
		updateSegments();
	}

	public void OnDrawGizmos() 
	{
		foreach(Segment s in segments)
		{
			s.OnDrawGizmos();
		}
    }
}