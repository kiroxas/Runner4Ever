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

	void OnCollisionEnter2D(Collider2D other) 
	{
        var state = other.GetComponent<CharacterController2D>();
        if(state != null && other.gameObject.tag == "Player")
        {
        	state.gravity.Push(gravity);
        }
    }

    void OnCollisionExit2D(Collider2D other) 
	{
        var state = other.GetComponent<CharacterController2D>();
        if(state != null && other.gameObject.tag == "Player")
        {
        	state.gravity.Pop();
        }
    }
}
