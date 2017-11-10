using UnityEngine;
using UnityEngine.Networking;

public class MyNetworkManager : NetworkManager
{
	int index = 0;
	int playersIn = 0;

	int expectedPlayers = 2;

    public override void OnServerConnect(NetworkConnection conn)
    {
    }

    public override void OnServerAddPlayer(NetworkConnection conn, short playerControllerId)
    {
    	var spawns = UnityUtils.getSpawningLocations();

    	GameObject player = (GameObject)GameObject.Instantiate(playerPrefab, spawns[index], Quaternion.identity);
    	++index;

    	if(!NetworkServer.AddPlayerForConnection(conn, player, playerControllerId))
    	{
    		Debug.LogError("Could not add player instance " + playerControllerId);
    	}

    	++playersIn;
    	if(playersIn == expectedPlayers)
    	{

    	}
    }
}