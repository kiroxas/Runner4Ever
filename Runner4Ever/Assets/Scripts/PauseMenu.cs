using UnityEngine;
using System.Collections;
using Lean.Touch;
using System;

/*
	Class that handles input, and triggers actions
*/
public class PauseMenu : MonoBehaviour
{
	void OnEnable()
    {
       /* EventManager.StartListening(EventManager.get().unPausePlayerEvent, setActive);
        EventManager.StartListening(EventManager.get().pausePlayerEvent, setInactive);*/
        transform.Find("Checkpoint").gameObject.SetActive(UnityUtils.isNetworkGame() == false);
    }

    void OnDisable ()
    {
       /* EventManager.StopListening(EventManager.get().unPausePlayerEvent, setActive);
        EventManager.StopListening(EventManager.get().pausePlayerEvent, setInactive);*/
    }

    public void open()
    {
    	gameObject.SetActive(true);
        EventManager.TriggerEvent(EventManager.get().pausePlayerEvent, new GameConstants.PausePlayerArgument());
    }

    public void close()
    {
    	gameObject.SetActive(false);
        EventManager.TriggerEvent(EventManager.get().unPausePlayerEvent, new GameConstants.UnPausePlayerArgument());
    }

     public void reloadCheckpoint()
    {
        gameObject.SetActive(false);
        CharacterController2D player = (CharacterController2D)UnityEngine.Object.FindObjectOfType(typeof(CharacterController2D));
        GameObject pl = player ? player.gameObject : null; 
        EventManager.TriggerEvent(EventManager.get().playerDeadEvent, new GameConstants.PlayerDeadArgument(pl));
    }

    public void closeGame()
    {
        gameObject.SetActive(false);
        EventManager.TriggerEvent(EventManager.get().quitMainGameEvent, new GameConstants.QuitMainGameArgument());
    }
}