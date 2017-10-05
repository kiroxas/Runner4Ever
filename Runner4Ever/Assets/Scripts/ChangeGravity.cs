using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeGravity : MonoBehaviour 
{
	public float gravity = 0.25f;

	void Start () 
	{
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	void OnTriggerEnterCustom(RaycastCollision other) 
	{
        var state = other.other.GetComponent<CharacterController2D>();
        if(state != null && other.other.gameObject.tag == "Player")
        {
        	state.gravity.Push(gravity);
        }
    }

    void OnTriggerExitCustom(RaycastCollision other) 
	{
        var state = other.other.GetComponent<CharacterController2D>();
        if(state != null && other.other.gameObject.tag == "Player")
        {
        	state.gravity.Pop();
        }
    }
}
