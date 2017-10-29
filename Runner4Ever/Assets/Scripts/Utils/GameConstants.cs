using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

public class GameConstants 
{
	static public string checkpointTag = "CheckPoint";
	static public string playerTag = "Player";
	static public string MainGameName = "MainGame";
	static public string levelFolder = "LevelGeneration/";
	static public string levelListFile = "levelList";
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
		
		public LevelSelectedArgument(string s)
		{
			levelName = s;
		}
	}

	public class LevelInitialisedArgument
	{
		public LevelInitialisedArgument(){}
	}

	public class LoadLevelArgument
	{
		public string levelName;

		public LoadLevelArgument(string ln)
		{
			levelName = ln;
		}
	}

	public class HitCheckpointArgument
	{
		public GameObject checkpoint;

		public HitCheckpointArgument(GameObject c)
		{
			checkpoint = c;
		}
	}


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
}
