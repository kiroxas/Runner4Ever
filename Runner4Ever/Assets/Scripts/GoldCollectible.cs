using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoldCollectible : MonoBehaviour 
{
	public GameObject Effect;
	
	public void OnTriggerEnter2D(Collider2D other)
	{
		if(other.GetComponent<CharacterController2D>() == null)
			return;

		Instantiate(Effect, transform.position, transform.rotation);
		GetComponent<Transform>().gameObject.SetActive(false);
	}
}
