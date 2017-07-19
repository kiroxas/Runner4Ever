using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InverseJumpWall : MonoBehaviour 
{

	public CharacterController2D.JumpDirectionOnWallOrEdge strat = CharacterController2D.JumpDirectionOnWallOrEdge.Inverse;
	// Use this for initialization
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
        	Debug.Log("Inverse");
        	state.jumpWallStack.Push(strat);
        }
    }

    void OnCollisionExit2D(Collider2D other) 
	{
        var state = other.GetComponent<CharacterController2D>();
        if(state != null && other.gameObject.tag == "Player")
        {
        	state.jumpWallStack.Pop();
        }
    }
}
