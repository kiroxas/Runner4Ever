using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerEvent : MonoBehaviour 
{
	public void OnTriggerEnterCustom(RaycastCollision other)
	{
		if(other.other.GetComponent<CharacterController2D>() == null)
			return;

		var infos = GetComponent<AdditionalInformation>();
		if(infos)
		{
			string eventType = infos.minor;
			string message = infos.info;

			EventManager.get().sendTrigger(eventType, message);
		}
	}
}