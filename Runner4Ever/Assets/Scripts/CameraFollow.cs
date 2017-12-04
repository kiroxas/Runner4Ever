using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour {

	public Transform target;
	public Vector3 offset;
	public Vector2 smoothing;
	public float panicLinePercent = 0.2f;

	private Bounds containingBox;
	private Vector3 upLeft, downRight;
	private Camera cam;
	private bool followUntilGround = false;


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

    private bool doWeNeedToUpdateY(float y)
    {
    	var charc = target.GetComponent<CharacterController2D>();

    	if(charc == null) // not a player, we follow
    	{
    		return true;
    	}

    	bool ground = charc.grounded();

    	if(followUntilGround)
    	{
    		if(ground)
    		{
    			followUntilGround = false;
    		}

    		return true;
    	}

    	float min, max;
    	getPanicLines(out min, out max);

    	if(y <= min || y >= max)
    	{
    		followUntilGround = true;
    		return true;
    	}

    	return ground;
    }

	public void LateUpdate()
	{
		if(target != null)
		{
			float orthSize =  GetComponent<Camera>().orthographicSize;
			var cameraHalfWidth = orthSize * ((float)Screen.width / Screen.height);
			//float y = Mathf.Lerp(transform.position.y, target.position.y + offset.y, Time.deltaTime * smoothing.y);
			float y = target.position.y + offset.y;
			if(doWeNeedToUpdateY(y))
			{
				y = Mathf.Lerp(transform.position.y, y, Time.deltaTime * smoothing.y);
			}
			else
			{
				y = transform.position.y;
			}

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
		cam = GetComponent<Camera>();
	}
	
	// Update is called once per frame
	void Update () 
	{
		UnityUtils.drawGizmoSquare(containingBox.min, downRight, containingBox.max, upLeft, Color.blue);
	}

	private void getPanicLines(out float min, out float max)
	{
		float minY = cam.ViewportToWorldPoint(new Vector3(0,0,0)).y;
		float maxY = cam.ViewportToWorldPoint(new Vector3(0,1,0)).y;

		float height = maxY - minY;
		float panicHeight = height * panicLinePercent;

		min = minY + panicHeight;
		max = maxY - panicHeight;
	}

	void OnDrawGizmosSelected()
	{
		float min, max;

		getPanicLines(out min, out max);
		Debug.DrawLine(new Vector3(containingBox.min.x, min, 0), new Vector3(containingBox.max.x, min, 0), Color.red);
		Debug.DrawLine(new Vector3(containingBox.min.x, max, 0), new Vector3(containingBox.max.x, max, 0), Color.blue);
	}
}
