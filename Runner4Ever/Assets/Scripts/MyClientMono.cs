using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;

public class MyClientMono : MonoBehaviour
{
	MyClient instance = new MyClient();
	MyServer serverInstance = new MyServer();

	void OnEnable()
    {
       instance.OnEnable();
       serverInstance.OnEnable();
    }

    void OnDisable ()
    {
        instance.OnDisable();
        serverInstance.OnDisable();
    }

	public void Start()
	{
		instance.Start();
		serverInstance.Start();
	}
}