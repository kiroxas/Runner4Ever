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

	void unpausePlayers()
	{
    	EventManager.TriggerEvent(EventManager.get().unPauseAllPlayerEvent, new GameConstants.UnPauseAllPlayerArgument());
	}

	public override void OnStartClient(NetworkClient client)
	{
		EventManager.TriggerEvent(EventManager.get().clientConnectedEvent, new GameConstants.ClientConnectedArgument(client));
	}

	public override void OnStartServer()
	{
		EventManager.TriggerEvent(EventManager.get().serverCreatedEvent, new GameConstants.ServerCreatedArgument());
	}

	public override void OnServerRemovePlayer(NetworkConnection conn, PlayerController player)
	{
		index--;
		playersIn--;
	}

    public override void OnServerAddPlayer(NetworkConnection conn, short playerControllerId)
    {
    	var spawns = UnityUtils.getSpawningLocations();

    	if(index >= spawns.Count)
    	{
    		Debug.LogError("Index is too high (index:" + index + " spawns:" + spawns.Count);
    	}

    	GameObject player = (GameObject)GameObject.Instantiate(playerPrefab, spawns[index], Quaternion.identity);
    	++index;

    	if(!NetworkServer.AddPlayerForConnection(conn, player, playerControllerId))
    	{
    		Debug.LogError("Could not add player instance " + playerControllerId);
    	}

    	++playersIn;
    	if(playersIn == expectedPlayers)
    	{
			unpausePlayers();
    	}
    }
}