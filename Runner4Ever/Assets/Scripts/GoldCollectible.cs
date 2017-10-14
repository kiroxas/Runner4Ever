using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoldCollectible : MonoBehaviour 
{
	public GameObject Effect;
	
	public void OnTriggerEnterCustom(RaycastCollision other)
	{
		if(other.other.GetComponent<CharacterController2D>() == null)
			return;

		other.other.GetComponent<CharacterController2D>().acquireObject();
		Instantiate(Effect, transform.position, transform.rotation);
		GetComponent<Transform>().gameObject.SetActive(false);
	}
}
