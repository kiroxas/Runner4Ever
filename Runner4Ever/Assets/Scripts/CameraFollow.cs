using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour 
{

	public Transform target;
	public Vector3 offset;
	public Vector2 smoothing;
	public float panicLineDownPercent = 0.15f;
	public float panicLineUpPercent = 0.1f;

	private Bounds containingBox;
	public Bounds containingBoxBack;
	private Camera cam;
	private bool followUntilGround = false;

	private float zValue = -10;


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
    	if(target == null)
    		return false;

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

    private void getXAndY(ref float x, ref float y)
    {
    	float orthSize =  cam.orthographicSize;
		var cameraHalfWidth = orthSize * ((float)Screen.width / Screen.height);

    	if(doWeNeedToUpdateY(y))
		{
			y = Mathf.Lerp(transform.position.y, y, Time.deltaTime * smoothing.y);
		}
		else
		{
			y = transform.position.y;
		}

		x = Mathf.Clamp(x, containingBox.min.x + cameraHalfWidth, containingBox.max.x - cameraHalfWidth);
		y = Mathf.Clamp(y, containingBox.min.y + orthSize, containingBox.max.y - orthSize);

		x =  Mathf.Lerp(x, transform.position.x, Time.deltaTime * smoothing.x);
    }

	public void LateUpdate()
	{
		if(target != null)
		{
			float x = target.position.x + offset.x;
			float y = target.position.y + offset.y;

			getXAndY(ref x, ref y);

			transform.position = new Vector3(x, y, zValue) ;
		}
	}

	public void attachCamera(GameConstants.PlayerSpawnArgument arg)
	{
		if(arg.player.GetComponent<CharacterController2D>().amILocalPlayer())
		{
			target = arg.player.GetComponent<Transform>();
		}
	}

	public void initBackBox()
	{
		GameObject farCamObj = GameObject.FindGameObjectWithTag("FarCamera");
		if(farCamObj == null)
			return;

		Camera farCam = farCamObj.GetComponent<Camera>();
		if(farCam == null)
		{
			Debug.LogError("The object with tag FarCamera should have a camera");
			return;
		}

		Vector3 camPlace = getBottomLeftOnGamePlane(cam);
		Vector3 farCamPlace = getBottomLeftOnGamePlane(farCam);

		Vector3 difference = camPlace - farCamPlace;
		
		Vector3 size = new Vector3(containingBox.size.x + 2 * difference.x , containingBox.size.y + 2 * difference.y, 0);

		containingBoxBack = new Bounds(containingBox.center, size);
	}

	public void init(GameConstants.LevelInitialisedArgument arg)
	{
		var lg = FindObjectOfType<SegmentStreamer>();
		cam = GetComponent<Camera>();

		if(lg == null)
		{
			Debug.Log("Couldn't find levelGenerator, add one to the scene");
			containingBox = new Bounds(Vector3.zero, Vector3.zero);
		}
		else
		{
			containingBox = lg.containingBox;
		}

		initBackBox();
	}

	// Use this for initialization
	void Start () 
	{	
	}
	
	// Update is called once per frame
	void Update () 
	{
		UnityUtils.drawGizmoSquare(containingBox.min,  containingBox.max, Color.blue);
		UnityUtils.drawGizmoSquare(containingBoxBack.min, containingBoxBack.max, Color.green);	
	}

	private void getPanicLines(out float min, out float max)
	{
		float minY = cam.ViewportToWorldPoint(new Vector3(0,0,0)).y;
		float maxY = cam.ViewportToWorldPoint(new Vector3(0,1,0)).y;

		float height = maxY - minY;
		float panicHeightDown = height * panicLineDownPercent;
		float panicHeightUp = height * panicLineUpPercent;

		min = minY + panicHeightDown;
		max = maxY - panicHeightUp;
	}

	Vector3 getBottomLeftOnGamePlane(Camera cam)
	{
		  // create logical plane perpendicular to Y and at (0,0,0):
   		var plane = new Plane(Vector3.forward, new Vector3(0,0,0));
   		float distance;

   		var ray = cam.ViewportPointToRay(new Vector3(0,0,0));
   		 // bottom left ray
   		if (plane.Raycast(ray, out distance))
   		{
    	    return ray.GetPoint( distance);
   		}

   		return Vector3.zero;
	}

	void OnDrawGizmosSelected()
	{
		float min, max;

		getPanicLines(out min, out max);
		Debug.DrawLine(new Vector3(containingBox.min.x, min, 0), new Vector3(containingBox.max.x, min, 0), Color.red);
		Debug.DrawLine(new Vector3(containingBox.min.x, max, 0), new Vector3(containingBox.max.x, max, 0), Color.blue);

        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(getBottomLeftOnGamePlane(cam), 0.5F); 

        GameObject farCamObj = GameObject.FindGameObjectWithTag("FarCamera");
		if(farCamObj == null)
			return;

		Camera farCam = farCamObj.GetComponent<Camera>();
		
       	Gizmos.color = Color.blue;
       	Gizmos.DrawSphere(getBottomLeftOnGamePlane(farCam), 0.5F); 
	}
}
