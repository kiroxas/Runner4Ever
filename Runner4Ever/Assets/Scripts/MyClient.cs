using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;

public class MyClient 
{
	private const short jumpMessage = 140;
	private const short jumpServerMessage = 141;

	private const short dashMessage = 142;
	private const short dashServerMessage = 143;

	NetworkClient client;

	public void OnEnable()
    {
    	EventManager.StartListening(EventManager.get().serverCreatedEvent, startServer);
    	EventManager.StartListening(EventManager.get().clientConnectedEvent, setClient);
        EventManager.StartListening(EventManager.get().networkJumpEvent, receiveJumpEvent);
        EventManager.StartListening(EventManager.get().networkDashEvent, receiveDashEvent);
    }

    public void OnDisable ()
    {
        EventManager.StopListening(EventManager.get().networkJumpEvent, receiveJumpEvent);
        EventManager.StopListening(EventManager.get().clientConnectedEvent, setClient);
        EventManager.StopListening(EventManager.get().serverCreatedEvent, startServer);
        EventManager.StopListening(EventManager.get().networkDashEvent, receiveDashEvent);
    }

    public void receiveJumpEvent(GameConstants.NetworkJumpArgument arg)
    {
    	IntegerMessage msg = new IntegerMessage((int)arg.id);

    	client.Send(jumpServerMessage, msg);
    }

    public void receiveDashEvent(GameConstants.NetworkDashArgument arg)
    {
    	IntegerMessage msg = new IntegerMessage((int)arg.id);

    	client.Send(dashServerMessage, msg);
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
    	client.RegisterHandler(jumpMessage, ReceiveJumpMessage);
    	client.RegisterHandler(dashMessage, ReceiveDashMessage);
    }

    public void registerServer()
    {
     	NetworkServer.RegisterHandler(jumpServerMessage, ServerReceiveJumpMessage);
    	NetworkServer.RegisterHandler(dashServerMessage, ServerReceiveDashMessage);
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

    private void ServerReceiveDashMessage(NetworkMessage message)
    {
     	var stringMessage = message.ReadMessage<IntegerMessage>();

     	NetworkServer.SendToAll (dashMessage, stringMessage);
    }

    private void ReceiveJumpMessage(NetworkMessage message)
    {
    	var NetworkJumpArgument =  message.ReadMessage<IntegerMessage>();
    	
    	EventManager.TriggerEvent(EventManager.get().networkOrdersJumpEvent, new GameConstants.NetworkJumpArgument((uint)NetworkJumpArgument.value));
    }

     private void ReceiveDashMessage(NetworkMessage message)
    {
    	var NetworkJumpArgument =  message.ReadMessage<IntegerMessage>();
    	
    	EventManager.TriggerEvent(EventManager.get().networkOrdersDashEvent, new GameConstants.NetworkDashArgument((uint)NetworkJumpArgument.value));
    }
}