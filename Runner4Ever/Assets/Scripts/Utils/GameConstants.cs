using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Events;
using System.Collections.Generic;

public class GameConstants 
{
	public enum Mode
    {
        Solo,
        Multiplayer
    }

    // Multiplayer Message

    public class CustomNetworkMessage : MessageBase
	{
   		public uint netId;
    	public Vector3 position;

    	public CustomNetworkMessage()
    	{}

    	public CustomNetworkMessage(uint n, Vector3 pos)
    	{
    		netId = n;
    		position = pos;
    	}
    }

    // Constants 
    static public string multiTag = "Multi";
	static public string checkpointTag = "CheckPoint";
	static public string playerTag = "Player";
	static public string MainGameName = "MainGame";
	static public string levelFolder = "LevelGeneration/";
	static public string levelListFile = "levelList";
	static public string multiLevelListFile = "multiLevelList";
	static public string[] charactersNames = { "Human", "HumanFlip" };
	static public string defaultCharac = charactersNames[0];

	static public string[] themes = { "Normal", "Oldie" };
	static public string defaultTheme = themes[0];

	public class  PlayerSpawnArgument 
	{
		public float xPosition;
		public float yPosition;
		public GameObject player;

		public PlayerSpawnArgument(GameObject p, float x, float y)
		{
			player = p;
			xPosition = x;
			yPosition = y;
		}
	}

	public class ResolutionChangedArgument
	{
		public float width;
		public float height;

		public ResolutionChangedArgument(float w, float h)
		{
			width = w;
			height = h;
		}
	}

	public class OrientationChangedArgument
	{
		public ScreenOrientation orientation;

		public OrientationChangedArgument(ScreenOrientation or)
		{
			orientation = or;
		}
	}

	public class LanguageChangedArgument
	{
		public LocalizationUtils.Languages lang;

		public LanguageChangedArgument(LocalizationUtils.Languages l)
		{
			lang = l;
		}
	}

	public class SegmentEnabledArgument
	{
		public Vector3 minBound;
		public Vector3 maxBound;

		public SegmentEnabledArgument(Vector3 min, Vector3 max)
		{
			minBound = min;
			maxBound = max;
		}
	}

	public class SegmentsUpdatedArgument
	{
		public List<SegmentEnabledArgument> segments;
		
		public SegmentsUpdatedArgument()
		{
			segments = new List<SegmentEnabledArgument>();
		}

		public void add(SegmentEnabledArgument arg)
		{
			segments.Add(arg);
		}
	}

	public class LevelSelectedArgument
	{
		public string levelName;
		public Mode mode;
		
		public LevelSelectedArgument(string s, Mode m)
		{
			levelName = s;
			mode = m;
		}
	}

	public class LevelInitialisedArgument
	{
		public LevelInitialisedArgument(){}
	}

	public class LoadLevelArgument
	{
		public string levelName;
		public GameConstants.Mode mode;

		public LoadLevelArgument(string ln, GameConstants.Mode m)
		{
			levelName = ln;
			mode = m;
		}

		public bool isNetworkGame()
		{
			return mode == Mode.Multiplayer;
		}
	}

	public class HitCheckpointArgument
	{
		public GameObject checkpoint;
		public GameObject player;

		public HitCheckpointArgument(GameObject c, GameObject p)
		{
			checkpoint = c;
			player = p;
		}
	}

	public class HitFinalCheckpointArgument
	{
		public GameObject checkpoint;
		public GameObject player;

		public HitFinalCheckpointArgument(GameObject c,GameObject p)
		{
			checkpoint = c;
			player = p;
		}
	}

	public class PlayerDeadArgument
	{
		public GameObject player;

		public PlayerDeadArgument(GameObject c)
		{
			player = c;
		}
	}

	public class NetworkArgument
	{
		public uint  id;
		public Vector3 position;

		public NetworkArgument(uint  i, Vector3 pos)
		{
			id = i;
			position = pos;
		}
	}

	public class NetworkJumpArgument : NetworkArgument
	{
		public NetworkJumpArgument(uint  i, Vector3 pos) : base(i, pos)
		{}
	}

	public class NetworkDashArgument : NetworkArgument
	{
		public NetworkDashArgument(uint  i, Vector3 pos) : base(i, pos)
		{}
	}

	public class ClientConnectedArgument
	{
		public NetworkClient client;

		public ClientConnectedArgument(NetworkClient c) 
		{
			client = c;
		}
	}

	public class TriggerDoorArgument
	{
		public string id;

		public TriggerDoorArgument(string c) 
		{
			id = c;
		}
	}

	public class HealthRemainingArgument
	{
		public int health;
		public int maxHealth;

		public HealthRemainingArgument(int h, int mH) 
		{
			health = h;
			maxHealth = mH;
		}
	}

	public class InitPlayerArgument
	{}

	public class AllClientsConnectedArgument
	{}

	public class ServerCreatedArgument
	{}

	public class UnPausePlayerArgument
	{}

	public class PausePlayerArgument
	{}

	public class UnPauseAllPlayerArgument
	{}

	public class QuitMainGameArgument
	{}

	public class StopAnimationsArgument
	{}

	public class PlayAnimationsArgument
	{}

	// Events
	public class PlayerSpawnEvent : UnityEvent<PlayerSpawnArgument>
	{}

	public class LanguageChangedEvent : UnityEvent<LanguageChangedArgument>
	{}

	public class ResolutionChangedEvent : UnityEvent<ResolutionChangedArgument>
	{}

	public class OrientationChangedEvent : UnityEvent<OrientationChangedArgument>
	{}

	public class SegmentEnabledEvent : UnityEvent<SegmentEnabledArgument>
	{}

	public class SegmentsUpdatedEvent : UnityEvent<SegmentsUpdatedArgument>
	{}

	public class LevelSelectedEvent : UnityEvent<LevelSelectedArgument>
	{}

	public class LevelInitialisedEvent : UnityEvent<LevelInitialisedArgument>
	{}

	public class LoadLevelEvent : UnityEvent<LoadLevelArgument>
	{}

	public class HitCheckpointEvent : UnityEvent<HitCheckpointArgument>
	{}

	public class HitFinalCheckpointEvent : UnityEvent<HitFinalCheckpointArgument>
	{}

	public class PlayerDeadEvent : UnityEvent<PlayerDeadArgument>
	{}

	public class UnPausePlayerEvent : UnityEvent<UnPausePlayerArgument>
	{}

	public class PausePlayerEvent : UnityEvent<PausePlayerArgument>
	{}

	public class UnPauseAllPlayerEvent : UnityEvent<UnPauseAllPlayerArgument>
	{}	

	public class QuitMainGameEvent : UnityEvent<QuitMainGameArgument>
	{}

	public class NetworkJumpEvent : UnityEvent<NetworkJumpArgument>
	{}	

	public class NetworkOrdersJumpEvent : UnityEvent<NetworkJumpArgument>
	{}	
	
	public class ClientConnectedEvent : UnityEvent<ClientConnectedArgument>
	{}

	public class ServerCreatedEvent : UnityEvent<ServerCreatedArgument>
	{}

	public class NetworkDashEvent : UnityEvent<NetworkDashArgument>
	{}	
	
	public class NetworkOrdersDashEvent : UnityEvent<NetworkDashArgument>
	{}

	public class AllClientsConnectedEvent : UnityEvent<AllClientsConnectedArgument>
	{}	

	public class TriggerDoorEvent : UnityEvent<TriggerDoorArgument>
	{}

	public class InitPlayerEvent : UnityEvent<InitPlayerArgument>
	{}

	public class StopAnimationsEvent : UnityEvent<StopAnimationsArgument>
	{}

	public class PlayAnimationsEvent : UnityEvent<PlayAnimationsArgument>
	{}

	public class HealthRemainingEvent : UnityEvent<HealthRemainingArgument>
	{}

	

	
}
