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
    public GameConstants.LevelSelectedEvent levelSelectedEvent { get; private set;}
    public GameConstants.LevelInitialisedEvent levelInitialisedEvent { get; private set;}
    public GameConstants.LoadLevelEvent loadLevelEvent { get; private set;}
    public GameConstants.HitCheckpointEvent hitCheckpointEvent { get; private set;}
    public GameConstants.HitFinalCheckpointEvent hitFinalCheckpointEvent { get; private set;}
    public GameConstants.PlayerDeadEvent playerDeadEvent { get; private set;}
    public GameConstants.UnPausePlayerEvent unPausePlayerEvent { get; private set;}
    public GameConstants.PausePlayerEvent pausePlayerEvent { get; private set;}
    public GameConstants.UnPauseAllPlayerEvent unPauseAllPlayerEvent { get; private set;}
    public GameConstants.QuitMainGameEvent quitMainGameEvent { get; private set;}
    public GameConstants.NetworkJumpEvent networkJumpEvent { get; private set;}
    public GameConstants.NetworkOrdersJumpEvent networkOrdersJumpEvent { get; private set;}
    public GameConstants.ClientConnectedEvent clientConnectedEvent { get; private set;}
    public GameConstants.ServerCreatedEvent serverCreatedEvent { get; private set;}
    public GameConstants.NetworkDashEvent networkDashEvent { get; private set;}
    public GameConstants.NetworkOrdersDashEvent networkOrdersDashEvent { get; private set;}
    public GameConstants.AllClientsConnectedEvent allClientsConnectedEvent { get; private set;}
    public GameConstants.InitPlayerEvent initPlayerEvent { get; private set;}
    


    // Triggers
    public GameConstants.TriggerDoorEvent triggerDoorEvent { get; private set;}



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

        DontDestroyOnLoad(gameObject);
    }

    public static EventManager get()
    {
        return instance;
    }

    public void sendTrigger(string key, string message)
    {
        if(key == "P")
        {
            TriggerEvent(triggerDoorEvent, new GameConstants.TriggerDoorArgument(message));
        }
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
            levelSelectedEvent = new GameConstants.LevelSelectedEvent();
            levelInitialisedEvent = new GameConstants.LevelInitialisedEvent();
            loadLevelEvent = new GameConstants.LoadLevelEvent();
            hitCheckpointEvent = new GameConstants.HitCheckpointEvent();
            hitFinalCheckpointEvent = new GameConstants.HitFinalCheckpointEvent();  
            playerDeadEvent = new GameConstants.PlayerDeadEvent();
            unPausePlayerEvent = new GameConstants.UnPausePlayerEvent();
            pausePlayerEvent = new GameConstants.PausePlayerEvent();
            unPauseAllPlayerEvent = new GameConstants.UnPauseAllPlayerEvent();
            quitMainGameEvent = new GameConstants.QuitMainGameEvent();
            networkJumpEvent = new GameConstants.NetworkJumpEvent();
            networkOrdersJumpEvent = new GameConstants.NetworkOrdersJumpEvent();
            clientConnectedEvent = new GameConstants.ClientConnectedEvent();
            serverCreatedEvent = new GameConstants.ServerCreatedEvent();
            networkDashEvent = new GameConstants.NetworkDashEvent();
            networkOrdersDashEvent = new GameConstants.NetworkOrdersDashEvent();
            allClientsConnectedEvent = new GameConstants.AllClientsConnectedEvent();
            triggerDoorEvent = new GameConstants.TriggerDoorEvent();
            initPlayerEvent = new GameConstants.InitPlayerEvent();
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

