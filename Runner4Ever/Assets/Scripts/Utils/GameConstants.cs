using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

public class GameConstants 
{
	static public string[] charactersNames = { "Human", "HumanFlip" };
	static public string defaultCharac = charactersNames[0];

	static public string[] themes = { "Normal", "Oldie" };
	static public string defaultTheme = themes[0];

	public class  PlayerSpawnArgument 
	{
		public float xPosition;
		public float yPosition;

		public PlayerSpawnArgument(float x, float y)
		{
			xPosition = x;
			yPosition = y;
		}
	}

	public class ResolutionChangedArgument
	{
		public float width;
		public float height;

		public ResolutionChangedArgument(float width, float height)
		{
			width = width;
			height = height;
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

	// Events
	public class PlayerSpawnEvent : UnityEvent<PlayerSpawnArgument>
	{}

	public class LanguageChangedEvent : UnityEvent<LanguageChangedArgument>
	{}

	public class ResolutionChangedEvent : UnityEvent<ResolutionChangedArgument>
	{}

	public class OrientationChangedEvent : UnityEvent<OrientationChangedArgument>
	{}

	
}
