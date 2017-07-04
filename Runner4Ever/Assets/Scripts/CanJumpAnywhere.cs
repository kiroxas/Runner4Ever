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

	void OnTriggerEnter2D(Collider2D other) 
	{
        var state = other.GetComponent<CharacterController2D>();
        if(state != null && other.gameObject.tag == "Player")
        {
        	state.jumpState.Push(CharacterController2D.JumpRestrictions.Anywhere);
        }
    }

    void OnTriggerExit2D(Collider2D other) 
	{
        var state = other.GetComponent<CharacterController2D>();
        if(state != null && other.gameObject.tag == "Player")
        {
        	state.jumpState.Pop();
        }
    }
}
