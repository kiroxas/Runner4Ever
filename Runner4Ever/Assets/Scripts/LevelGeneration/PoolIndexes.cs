using System.Collections;
using System.Collections.Generic;
using System.Text;

/* Class that stores all the indexes relative to pooling */
public class PoolIndexes
{
	public static int uniquePoolingStrategy = 1;
	public static int smallPoolingStrategy = 10;
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


	public static int[] statelessIndexes = { earthIndex, inverseEarthIndex, waterIndex, hurtIndex, bumperIndex, standOnIndex, jumperIndex };
	public static int[] stateIndexes = { objectIndex, enemiesIndex, disapearingIndex, escalatorIndex, movingIndex, killMovingIndex, checkpointIndex, playerIndex, finalCheckpointIndex};

	public static char emptyIndex = '0';
	public static Dictionary<char, int> fileToPoolMapping = new Dictionary<char, int>
	 { {emptyIndex, emptyLayoutIndex},
	   {'1', earthIndex}, 
	   {'2', waterIndex},
	   {'3', objectIndex},
	   {'4', checkpointIndex},
	   {'5', checkpointIndex},
	   {'6', inverseEarthIndex},
	   {'7', hurtIndex},
	   {'8', enemiesIndex},
	   {'9', disapearingIndex},
	   {'A', escalatorIndex},
	   {'B', bumperIndex},
	   {'C', movingIndex},
	   {'D', killMovingIndex},
	   {'E', standOnIndex},
	   {'F', jumperIndex},
	   {'L', finalCheckpointIndex} };

}