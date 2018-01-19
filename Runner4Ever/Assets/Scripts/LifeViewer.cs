using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LifeViewer : MonoBehaviour 
{
	public Transform lifeContainer;
	public GameObject lifeFull;
	public GameObject lifeEmpty;

	void OnEnable()
    {
        EventManager.StartListening(EventManager.get().healthRemainingEvent, showLife);
    }

    void OnDisable ()
    {
        EventManager.StopListening(EventManager.get().healthRemainingEvent, showLife);
    }

    void showLife(GameConstants.HealthRemainingArgument arg)
    {
    	foreach (Transform t in lifeContainer)
            Destroy(t.gameObject);

        int full = arg.health;
        int emptyLife = arg.maxHealth - arg.health;

        for(int i = 0; i < full; ++i)
        {
            GameObject entry = Instantiate(lifeFull);

            entry.GetComponent<Transform>().SetParent(lifeContainer, false);
        }

        for(int i = 0; i < emptyLife; ++i)
        {
            GameObject entry = Instantiate(lifeEmpty);

            entry.GetComponent<Transform>().SetParent(lifeContainer, false);
        }
    }

	// Use this for initialization
	void Start () 
	{	
	}
	
	// Update is called once per frame
	void Update () 
	{
	}
}
