using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SkyBoxFiller : MonoBehaviour 
{
    public GameObject skybox;
    public GameObject sky;

    //private Bounds containingBox;
    private Vector2 skySize;
    private PoolCollection skies = new PoolCollection();
    private Vector3 beginPos;

    private List<GameObject> skiesRendered = new List<GameObject>();
    int earthIndex = 0;
    int skyIndex = 1;

    void OnEnable()
    {
        EventManager.StartListening (EventManager.get().segmentsUpdatedEvent, enableSky);
    }

     void OnDisable ()
    {
        EventManager.StopListening (EventManager.get().segmentsUpdatedEvent, enableSky);
    }

    private Vector3 segmentToPosition(Vector2 segPos)
	{
		return new Vector2(beginPos.x + segPos.x * skySize.x, beginPos.y + segPos.y * skySize.y);
	}

    private Vector2 getPositionToSegment(Vector3 worldPosition)
	{
		Vector3 position = worldPosition;

		float xSegmentSize = skySize.x;
		int xGridIndex = (int)Mathf.Floor((position.x - beginPos.x) / xSegmentSize);

		float ySegmentSize = skySize.y;
		int yGridIndex = (int)Mathf.Floor((position.y - beginPos.y) / ySegmentSize);

		return new Vector2(xGridIndex, yGridIndex);
	}

    void enableSky(GameConstants.SegmentsUpdatedArgument arg)
    {
    	beginPos = FindObjectOfType<CameraFollow>().containingBoxBack.min;
        HashSet<Vector2> skiesNeeded = new HashSet<Vector2>();

        foreach(var seg in arg.segments)
        {
           Vector2 gridMin = getPositionToSegment(seg.minBound);
           Vector2 gridMax = getPositionToSegment(seg.maxBound);

           for(int x = (int)gridMin.x; x <= (int)gridMax.x; ++x)
           {
               for(int y = (int)gridMin.y; y <= (int)gridMax.y; ++y)
               {
                    Vector2 place = new Vector2(x,y);

                    if(skiesNeeded.Contains(place) == false)
                    {
                        skiesNeeded.Add(place);
                    }
               }
           }
        }

        // Let's free the one that are not needed now

        List<GameObject> toRemove = new List<GameObject>();

        foreach(GameObject g in skiesRendered)
        {
            Vector3 pos = g.GetComponent<Transform>().position;
            Vector2 gridPos = getPositionToSegment(pos);

            if(skiesNeeded.Contains(gridPos) == false)
            {
                toRemove.Add(g);
            }
            else
            {
                skiesNeeded.Remove(gridPos);
            }
        }

        foreach(GameObject g in toRemove)
        {
            skiesRendered.Remove(g);
            Vector3 pos = g.GetComponent<Transform>().position;
            Vector2 gridPos = getPositionToSegment(pos);

            int index = gridPos.y == 0 ? earthIndex : skyIndex;

            skies.free(g, index);
        }

        // Now let's instantiate the one we need

        foreach(Vector2 g in skiesNeeded)
        {
        	int index = g.y == 0 ? earthIndex : skyIndex;
            skiesRendered.Add(skies.getFromPool(index, segmentToPosition(g)));
        }
    }

    void Awake()
    {
        skySize = skybox.GetComponent<SpriteRenderer>().bounds.size;
        skies.addPool(skybox, earthIndex,  PoolIndexes.smallPoolingStrategy);
        skies.addPool(sky, skyIndex,  PoolIndexes.smallPoolingStrategy);
    }

    void Start()
    {}
}