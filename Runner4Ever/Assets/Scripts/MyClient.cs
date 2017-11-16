using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;

public class MyClient 
{
	private const short jumpMessage = 140;
	private const short jumpServerMessage = 141;

	NetworkClient client;

	public void OnEnable()
    {
    	EventManager.StartListening(EventManager.get().serverCreatedEvent, startServer);
    	EventManager.StartListening(EventManager.get().clientConnectedEvent, setClient);
        EventManager.StartListening(EventManager.get().networkJumpEvent, receiveJumpEvent);
    }

    public void OnDisable ()
    {
        EventManager.StopListening(EventManager.get().networkJumpEvent, receiveJumpEvent);
        EventManager.StopListening(EventManager.get().clientConnectedEvent, setClient);
        EventManager.StopListening(EventManager.get().serverCreatedEvent, startServer);
    }

    public void receiveJumpEvent(GameConstants.NetworkJumpArgument arg)
    {
    	IntegerMessage msg = new IntegerMessage((int)arg.id);

    	client.Send(jumpServerMessage, msg);
    }

    public void startServer(GameConstants.ServerCreatedArgument cl)
    {
    	registerServer();
    }


    public void setClient(GameConstants.ClientConnectedArgument cl)
    {
    	client = cl.client;

    	registerClient();
    }

    public void registerClient()
    {
    	Debug.Log("Registering");
		
    	client.RegisterHandler(jumpMessage, ReceiveJumpMessage);
    }

    public void registerServer()
    {
    	Debug.Log("Registering Server");
    	
      	//registering the server handler
     	NetworkServer.RegisterHandler(jumpServerMessage, ServerReceiveJumpMessage);
    	
    }

	public void Start()
	{
		if(UnityUtils.isNetworkGame() == false)
		{
			Debug.LogError("You should be in a network game to instance myClient");
		}
	}

	private void ServerReceiveJumpMessage(NetworkMessage message)
    {
     	var stringMessage =  message.ReadMessage<IntegerMessage>();

     	NetworkServer.SendToAll (jumpMessage, stringMessage);
    }

    private void ReceiveJumpMessage(NetworkMessage message)
    {
    	var NetworkJumpArgument =  message.ReadMessage<IntegerMessage>();
    	
    	EventManager.TriggerEvent(EventManager.get().networkOrdersJumpEvent, new GameConstants.NetworkJumpArgument((uint)NetworkJumpArgument.value));
    }
}