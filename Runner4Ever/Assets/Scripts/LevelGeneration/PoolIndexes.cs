using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Linq;

/* Class that stores all the indexes relative to pooling */
public class PoolIndexes
{
	public static int uniquePoolingStrategy = 1;
	public static int smallPoolingStrategy = 5;
	public static int mediumPoolingStrategy = 50;
	public static int bigPoolingStrategy = 250;

	// -------------- StateLess
	public static int emptyLayoutIndex = -1;
	public static int earthIndex = 0;
	public static int inverseEarthIndex = 1;
	public static int waterIndex = 2;
	public static int hurtIndex = 3;

	// -------------- StateFull
	public static int objectIndex = 4;
	public static int enemiesIndex = 5;
	public static int disapearingIndex = 6;
	public static int escalatorIndex = 7; 
	public static int movingIndex = 8;
	public static int killMovingIndex = 9;

	public static int playerIndex = 10;
	public static int checkpointIndex = 11;
	public static int bumperIndex = 12;
	public static int standOnIndex = 13;
	public static int jumperIndex = 14;
	public static int finalCheckpointIndex = 15;
	public static int stopTileIndex = 16;
	public static int accelerateTileIndex = 17;
	public static int decelerateTileIndex = 18;
	public static int startCheckpointIndex = 19;
	public static int triggerIndex = 20;
	public static int doorIndex = 21;
	public static int moveDiagIndex = 22;
	public static int accelerateFasterTileIndex = 23;

	public static int[] statelessIndexes = { earthIndex, inverseEarthIndex, waterIndex, hurtIndex, bumperIndex, standOnIndex, jumperIndex, stopTileIndex, accelerateTileIndex, accelerateFasterTileIndex, decelerateTileIndex };
	public static int[] stateIndexes = { objectIndex, enemiesIndex, disapearingIndex, escalatorIndex, movingIndex, moveDiagIndex, killMovingIndex, checkpointIndex, playerIndex, finalCheckpointIndex, startCheckpointIndex, triggerIndex, doorIndex};

	public static string emptyIndex = "0";
	public static Dictionary<string, int> fileToPoolMapping = new Dictionary<string, int>
	 { {emptyIndex, emptyLayoutIndex},
	   {"1", earthIndex}, 
	   {"2", waterIndex},
	   {"3", objectIndex},
	   {"4", startCheckpointIndex},
	   {"5", checkpointIndex},
	   {"6", inverseEarthIndex},
	   {"7", hurtIndex},
	   {"8", enemiesIndex},
	   {"9", disapearingIndex},
	   {"A", escalatorIndex},
	   {"B", bumperIndex},
	   {"C", movingIndex},
	   {"CD", moveDiagIndex},
	   {"D", killMovingIndex},
	   {"E", standOnIndex},
	   {"F", jumperIndex},
	   {"L", finalCheckpointIndex},
	   {"S", stopTileIndex},
	   {"G", accelerateTileIndex},
	   {"GF", accelerateFasterTileIndex},
	   {"H", decelerateTileIndex},
	   {"T", triggerIndex},
	   {"P", doorIndex} };

	public static string findKey(int value)
	{
		return fileToPoolMapping.FirstOrDefault(x => x.Value == value).Key;
	}

}