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

	void OnCollisionEnterCustom(RaycastCollision other) 
	{
        var state = other.other.GetComponent<CharacterController2D>();
        if(state != null && other.other.gameObject.tag == GameConstants.playerTag)
        {
        	state.jumpWallStack.Push(strat);
        }
    }

    void OnCollisionExitCustom(RaycastCollision other) 
	{
        var state = other.other.GetComponent<CharacterController2D>();
        if(state != null && other.other.gameObject.tag == GameConstants.playerTag)
        {
        	state.jumpWallStack.Pop();
        }
    }
}
