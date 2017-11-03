using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangePlayerMaxSpeed : MonoBehaviour 
{

	public float xSpeedBySecond = 8.0f;

	// Use this for initialization
	void Start () 
	{
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	void OnCollisionEnterCustom(RaycastCollision other) 
	{
        var state = other.other.GetComponent<CharacterController2D>();
        if(state != null && other.other.gameObject.tag == GameConstants.playerTag)
        {
        	state.maxVelocity.Push(xSpeedBySecond, gameObject);
        }
    }

    void OnCollisionExitCustom(RaycastCollision other) 
	{
        var state = other.other.GetComponent<CharacterController2D>();
        if(state != null && other.other.gameObject.tag == GameConstants.playerTag)
        {
        	state.maxVelocity.Remove(gameObject);
        }
    }
}