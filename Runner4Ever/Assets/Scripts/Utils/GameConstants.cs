using UnityEngine;

public class GameConstants 
{
	static public string[] charactersNames = { "Human", "HumanFlip" };
	static public string defaultCharac = charactersNames[0];

	static public string[] themes = { "Normal", "Oldie" };
	static public string defaultTheme = themes[0];

	// Events

	static public string playerSpawnEvent = "pse";
	static public string languageChangedEvent = "lce";
	static public string resolutionChangedEvent = "rce";
	static public string orientationChangedEvent = "oce";
}
