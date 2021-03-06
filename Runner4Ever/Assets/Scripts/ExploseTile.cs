using UnityEngine;
using System.Collections;
using Lean.Touch;
using System.Collections.Generic;

public class ExploseTile : MonoBehaviour
{
	public float timeToWait = 0.5f;
	public float timeToReappear = 1.5f;

	void Activate()
	{
		gameObject.SetActive(true);
	}

	IEnumerator setInactive()
    {
        yield return new WaitForSeconds(timeToWait);
       	
       	gameObject.SetActive(false);
       	Invoke("Activate", timeToReappear);
    }

	void OnCollisionEnter2D(Collider2D other) 
	{
        var state = other.GetComponent<CharacterController2D>();
        if(state != null && other.gameObject.tag == "Player")
        {
        	StartCoroutine(setInactive());
        }
    }
}