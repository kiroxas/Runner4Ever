using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SkyBoxFiller : MonoBehaviour 
{
    public GameObject skybox;

    private Bounds containingBox;
    private Vector2 skySize;
    private PoolCollection skies = new PoolCollection();

    void Start()
    {
       var lg = FindObjectOfType<SegmentStreamer>();

        if(lg == null)
        {
            Debug.Log("Couldn't find levelGenerator, add one to the scene");
            containingBox = new Bounds(Vector3.zero, Vector3.zero);
        }
        else
        {
            containingBox = lg.containingBox;
        }

        skySize = skybox.GetComponent<SpriteRenderer>().bounds.size;
        skies.addPool(skybox, 0,  PoolIndexes.smallPoolingStrategy);

        float xSize = containingBox.size.x / skySize.x;

        for(int i = 0; i < (int)Mathf.Ceil(xSize); ++i)
        {
            Vector3 position = new Vector3(containingBox.min.x + i * skySize.x + (skySize.x / 2.0f), containingBox.min.y + (skySize.y / 2.0f), 0.0f); 
            skies.getFromPool(0, position);
        }
    }
}