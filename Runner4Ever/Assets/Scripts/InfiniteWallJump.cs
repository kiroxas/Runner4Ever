using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InfiniteWallJump : MonoBehaviour 
{
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
        	state.loopingJumps(true);
        	state.wallJumpStrat.Push(CharacterController2D.WallJumpStrategy.Infinite, gameObject);
        }
    }

    void OnCollisionExitCustom(RaycastCollision other) 
	{
        var state = other.other.GetComponent<CharacterController2D>();
        if(state != null && other.other.gameObject.tag == GameConstants.playerTag)
        {
        	state.loopingJumps(false);
        	state.wallJumpStrat.Remove(gameObject);
        }
    }
}
