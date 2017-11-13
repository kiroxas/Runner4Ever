using UnityEngine;
using System.Collections;
using Lean.Touch;
using System;

/*
	Class that handles input, and triggers actions
*/
public class Active : MonoBehaviour
{
	private bool _isActive = false;

	void OnEnable()
    {
        EventManager.StartListening(EventManager.get().unPausePlayerEvent, setActive);
        EventManager.StartListening(EventManager.get().pausePlayerEvent, setInactive);
    }

    void OnDisable ()
    {
        EventManager.StopListening(EventManager.get().unPausePlayerEvent, setActive);
        EventManager.StopListening(EventManager.get().pausePlayerEvent, setInactive);
    }

    public void setActive()
    {
    	_isActive = true;
    }

    void setActive(GameConstants.UnPausePlayerArgument arg)
    {
    	setActive();
    }

    void setInactive(GameConstants.PausePlayerArgument arg)
    {
    	_isActive = false;
    }

    public bool isActive()
    {
    	return _isActive;
    }
}