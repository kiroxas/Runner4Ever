using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
using System;

public class EventManager : MonoBehaviour 
{
    public GameConstants.PlayerSpawnEvent playerSpawnEvent { get; private set;}
    public GameConstants.LanguageChangedEvent languageChangedEvent { get; private set;}
    public GameConstants.ResolutionChangedEvent resolutionChangedEvent { get; private set;}
    public GameConstants.OrientationChangedEvent orientationChangedEvent { get; private set;}
    public GameConstants.SegmentEnabledEvent segmentEnabledEvent { get; private set;}
    public GameConstants.SegmentsUpdatedEvent segmentsUpdatedEvent { get; private set;}

    public static EventManager instance;

    void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        Init (); 

        //DontDestroyOnLoad(gameObject);
    }

    public static EventManager get()
    {
        return instance;
    }

    void Init ()
    {
        if (playerSpawnEvent == null || languageChangedEvent == null || resolutionChangedEvent == null || orientationChangedEvent == null || segmentEnabledEvent == null)
        {
            playerSpawnEvent = new GameConstants.PlayerSpawnEvent();
            languageChangedEvent = new GameConstants.LanguageChangedEvent();
            resolutionChangedEvent = new GameConstants.ResolutionChangedEvent();
            orientationChangedEvent = new GameConstants.OrientationChangedEvent();
            segmentEnabledEvent = new GameConstants.SegmentEnabledEvent();
            segmentsUpdatedEvent = new GameConstants.SegmentsUpdatedEvent();
        }
    }

    public static void StartListening<Event>(UnityEvent<Event> ev, UnityAction<Event> listener)
    {
       ev.AddListener(listener);
    }

    public static void StopListening<Event>(UnityEvent<Event> ev, UnityAction<Event> listener)
    {
        ev.RemoveListener(listener);
    }

    public static void TriggerEvent<Event> (UnityEvent<Event> ev, Event arg)
    {
       ev.Invoke(arg);
    }
}

