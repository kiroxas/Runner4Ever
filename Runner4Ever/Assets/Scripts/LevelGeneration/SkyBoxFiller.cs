using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SkyBoxFiller : MonoBehaviour 
{
    public GameObject skybox;

    //private Bounds containingBox;
    private Vector2 skySize;
    private PoolCollection skies = new PoolCollection();

    private List<GameObject> skiesRendered = new List<GameObject>();
    int skyIndex = 0;

    void OnEnable()
    {
        EventManager.StartListening (EventManager.get().segmentsUpdatedEvent, enableSky);
    }

     void OnDisable ()
    {
        EventManager.StopListening (EventManager.get().segmentsUpdatedEvent, enableSky);
    }

    void enableSky(GameConstants.SegmentsUpdatedArgument arg)
    {
        HashSet<Vector2> skiesNeeded = new HashSet<Vector2>();

        foreach(var seg in arg.segments)
        {
            // containing 
           int xGridMin = (int)(seg.minBound.x / skySize.x);
           int yGridMin = (int)(seg.minBound.y / skySize.y);

           int xGridMax = (int)(seg.maxBound.x / skySize.x);
           int yGridMax = (int)(seg.maxBound.y / skySize.y);

           for(int x = xGridMin; x <= xGridMax; ++x)
           {
               for(int y = yGridMin; y <= yGridMax; ++y)
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
            Vector2 pos = g.GetComponent<Transform>().position;
            Vector2 gridPos = new Vector2(pos.x / skySize.x, pos.y / skySize.y);

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
            skies.free(g, skyIndex);
        }

        // Now let's instantiate the one we need

        foreach(Vector2 g in skiesNeeded)
        {
            Vector2 position = new Vector2(g.x * skySize.x, g.y * skySize.y);
            skiesRendered.Add(skies.getFromPool(skyIndex, position));
        }
    }

    void Awake()
    {
        skySize = skybox.GetComponent<SpriteRenderer>().bounds.size;
        skies.addPool(skybox, 0,  PoolIndexes.smallPoolingStrategy);   
    }

    void Start()
    {}
}