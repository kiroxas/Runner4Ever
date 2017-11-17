using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour {

	public Transform target;
	public Vector3 offset;
	public Vector2 smoothing;

	private Bounds containingBox;
	private Vector3 upLeft, downRight;

	void OnEnable()
    {
        EventManager.StartListening(EventManager.get().levelInitialisedEvent, init);
        EventManager.StartListening(EventManager.get().playerSpawnEvent, attachCamera);
    }

    void OnDisable ()
    {
        EventManager.StopListening(EventManager.get().levelInitialisedEvent, init);
        EventManager.StopListening(EventManager.get().playerSpawnEvent, attachCamera);
    }

	public void LateUpdate()
	{
		if(target != null)
		{
			float orthSize =  GetComponent<Camera>().orthographicSize;
			var cameraHalfWidth = orthSize * ((float)Screen.width / Screen.height);
			float y = Mathf.Lerp(transform.position.y, target.position.y + offset.y, Time.deltaTime * smoothing.y);
			float x = target.position.x + offset.x;

			x = Mathf.Clamp(x, containingBox.min.x + cameraHalfWidth, containingBox.max.x - cameraHalfWidth);
			y = Mathf.Clamp(y, containingBox.min.y + orthSize, containingBox.max.y - orthSize);

			x =  Mathf.Lerp(x, transform.position.x, Time.deltaTime * smoothing.x);

			transform.position = new Vector3(x, y, -10) ;
		}
	}

	public void attachCamera(GameConstants.PlayerSpawnArgument arg)
	{
		if(arg.player.GetComponent<CharacterController2D>().amILocalPlayer())
		{
			target = arg.player.GetComponent<Transform>();
		}
	}

	public void init(GameConstants.LevelInitialisedArgument arg)
	{
		var lg = FindObjectOfType<SegmentStreamer>();

		if(lg == null)
		{
			Debug.Log("Couldn't find levelGenerator, add one to the scene");
			containingBox = new Bounds(Vector3.zero, Vector3.zero);
		}
		else
		{
			containingBox = lg.containingBox;
		}

		upLeft = new Vector3(containingBox.min.x, containingBox.max.y);
		downRight = new Vector3(containingBox.max.x, containingBox.min.y);
	}

	// Use this for initialization
	void Start () 
	{
		
	}
	
	// Update is called once per frame
	void Update () 
	{
		UnityUtils.drawGizmoSquare(containingBox.min, downRight, containingBox.max, upLeft, Color.blue);
	}
}
