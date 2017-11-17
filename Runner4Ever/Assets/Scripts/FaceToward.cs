using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FaceToward : MonoBehaviour 
{
	private Transform target;

	void OnEnable()
    {
        //EventManager.StartListening(EventManager.get().playerSpawnEvent, attachTarget);
       // EventManager.StartListening(EventManager.get().allClientsConnectedEvent, attachTarget);
        //allClientsConnectedEvent
    }

    void OnDisable ()
    {
       // EventManager.StopListening(EventManager.get().playerSpawnEvent, attachTarget);
    }

    public bool attach(GameObject g)
    {
    	if(g.GetComponent<CharacterController2D>().amILocalPlayer() == false)
		{
			target = g.GetComponent<Transform>();
			Debug.Log("Attached");
			return true;
		}

		return false;
    }

    public void attachTarget(GameConstants.PlayerSpawnArgument arg)
	{
		attach(arg.player);
	}

	void Update()
	{
		if(target)
		{
			transform.right = target.position - transform.position;
			/*Vector3 diff = Camera.main.ScreenToWorldPoint(target.position) - transform.position;
        	diff.Normalize();
 
         	float rot_z = Mathf.Atan2(diff.y, diff.x) * Mathf.Rad2Deg;
         	transform.rotation = Quaternion.Euler(0f, 0f, rot_z - 90);*/
		}
		else
		{
			GameObject[] players = GameObject.FindGameObjectsWithTag(GameConstants.playerTag);

			foreach(GameObject player in players)
			{
				if(attach(player))
				{
					break;
				}
			}
		}
	}
}