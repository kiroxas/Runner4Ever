using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StopPlayer : MonoBehaviour 
{
	private bool stopPlayer = true;

	// Use this for initialization
	void Start () 
	{
		stopPlayer = true;
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	void OnCollisionCenterAlign(RaycastCollision other) 
	{
        var state = other.other.GetComponent<CharacterController2D>();
        if(state != null && other.other.gameObject.tag == GameConstants.playerTag && stopPlayer)
        {
        	state.stop();
        	float myX = GetComponent<BoxCollider2D>().bounds.center.x;
        	float otherX = other.other.GetComponent<BoxCollider2D>().bounds.center.x;

        	float diff = myX - otherX;
        	other.other.transform.position = new Vector3(other.other.transform.position.x + diff, other.other.transform.position.y);
        	stopPlayer = false;
        }
    }

    
	void OnCollisionExitCustom(RaycastCollision other) 
	{
        var state = other.other.GetComponent<CharacterController2D>();
        if(state != null && other.other.gameObject.tag == GameConstants.playerTag)
        {
        	stopPlayer = true;
        }
    }
    
}