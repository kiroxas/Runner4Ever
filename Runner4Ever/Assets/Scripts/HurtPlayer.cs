using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HurtPlayer : MonoBehaviour {

	public int damage = 10;
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
        	state.doDamage(damage);
        }
    }
}
