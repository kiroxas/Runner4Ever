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

	void OnCollisionEnter2D(Collider2D other) 
	{
        var state = other.GetComponent<CharacterController2D>();
        if(state != null && other.gameObject.tag == "Player")
        {
        	state.doDamage(damage);
        }
    }
}
