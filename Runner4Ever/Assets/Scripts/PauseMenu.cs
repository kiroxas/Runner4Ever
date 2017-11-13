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

    public void closeGame()
    {
        gameObject.SetActive(false);
        EventManager.TriggerEvent(EventManager.get().quitMainGameEvent, new GameConstants.QuitMainGameArgument());
    }
}