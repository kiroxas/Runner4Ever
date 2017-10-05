using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CanJumpAnywhere : MonoBehaviour {
	// Use this for initialization
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
        	state.jumpState.Push(CharacterController2D.JumpRestrictions.Anywhere);
        }
    }

    void OnTriggerExitCustom(RaycastCollision other) 
	{
        var state = other.other.GetComponent<CharacterController2D>();
        if(state != null && other.other.gameObject.tag == "Player")
        {
        	if(state.jumpState.Count == 0)
        	{
        		Debug.Log("Trying to pop an empty stack");
        	}
        	else
        	{
        		state.jumpState.Pop();
        	}
        }
    }
}
