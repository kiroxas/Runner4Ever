using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OpenDoor : MonoBehaviour 
{
	void OnEnable()
    {
        EventManager.StartListening(EventManager.get().triggerDoorEvent, onDoorOpen);
    }

    void OnDisable ()
    {
        EventManager.StopListening(EventManager.get().triggerDoorEvent, onDoorOpen);
    }

    public void onDoorOpen(GameConstants.TriggerDoorArgument arg)
    {
    	var infos = GetComponent<AdditionalInformation>();
 	
		if(infos && infos.minor == arg.id)
		{
    		Debug.Log("Door Opened");
    	}
    }
}