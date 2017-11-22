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
    		LeanTween.scale( gameObject, new Vector3(0f, 0f, 1.7f), 1f).setEase(LeanTweenType.easeOutExpo);
    	}
    }
}