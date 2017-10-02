using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour {

	public Transform target;
	public Vector3 offset;
	public Vector2 smoothing;

	public Bounds containingBox;
	private Vector3 upLeft, downRight;

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

	// Use this for initialization
	void Start () 
	{
		var lg = FindObjectOfType<SegmentStreamer>();
		Vector3 center, size;
		if(lg == null)
		{
			Debug.Log("Couldn't find levelGenerator, add one to the scene");
			center = new Vector3(0,0,0);
			size = new Vector3(0,0,0);
		}
		else
		{
			float xSize = lg.xTotalLevel * lg.tileWidth;
			float ySize = lg.yTotalLevel * lg.tileHeight;
			size = new Vector3(xSize, ySize, 0);
			float xMiddle = lg.bottomLeftXPos + size.x / 2.0f;
			float yMiddle = lg.bottomLeftYPos + size.y / 2.0f;
			center = new Vector3(xMiddle - lg.tileWidth / 2.0f, yMiddle + lg.tileHeight / 2.0f, 0); // Because the anchor point of tiles are centered
		}

		containingBox = new Bounds(center, size);
		upLeft = new Vector3(containingBox.min.x, containingBox.max.y);
		downRight = new Vector3(containingBox.max.x, containingBox.min.y);
	}
	
	// Update is called once per frame
	void Update () 
	{
		Debug.DrawLine(containingBox.min, upLeft, Color.blue);
		Debug.DrawLine(upLeft, containingBox.max, Color.blue);
		Debug.DrawLine(containingBox.max, downRight, Color.blue);
		Debug.DrawLine(containingBox.min, downRight, Color.blue);

	}
}
