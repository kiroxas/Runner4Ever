using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;

public class MyClientMono : MonoBehaviour
{
	MyClient instance = new MyClient();

	void OnEnable()
    {
       instance.OnEnable();
    }

    void OnDisable ()
    {
        instance.OnDisable();
    }

	public void Start()
	{
		instance.Start();
	}
}