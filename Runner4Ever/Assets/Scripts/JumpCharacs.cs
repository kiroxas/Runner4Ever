using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* Class that handles a jump, its definition and all the points of the curve 
*/
public class JumpCharacs : MonoBehaviour
{
	// --------------------------------- Public Interface -----------------------------------------------
	public AnimationCurve jumpShape; // the curve
	public float xDistance = 1.0f; // distance we are going to travel on X 
	public float yDistance = 1.0f; // Y for one unit in the curve editor 


	// ---------------------------------- Private Members -----------------------------------------------
	private Transform debugOrigin; // the transform to plot the curve

	private float jumpTime; // time a jump takes

	private float expectedVelocity; // velocity per frame
	private Vector2 currentJumpStart; // origin of the jump
	
	private float shapeIndex; // index to evaluate the jumpShape
	private List<Vector3> offsetsOrigin; // offstes with the origin
	private List<Vector3> offsets; // offset with the previous

	private int index; // index in the offsets list
	private float startTime; // the start time of the first key in animation
	private float timeClock = 0.0f; // current time in the animation curve
	private bool goingRight = true; // Are we going right, true by default

	// --------------------------------- Functions -----------------------------------------------

	public void flip()
	{
		goingRight = !goingRight;
	}

	public void init()
	{
		startTime = jumpShape[0].time;
		jumpTime = jumpShape[jumpShape.length - 1].time - startTime;

		int FPS = (int)(1f / Time.unscaledDeltaTime);
		float time = jumpTime * FPS;

		expectedVelocity = xDistance / time;
		
		index = 0;
		shapeIndex = startTime;

		offsetsOrigin = new List<Vector3>();
		while(!jumpEnded())
        {
            offsetsOrigin.Add(getNextKey());
        }

        offsets = new List<Vector3>();
        offsets.Add(offsetsOrigin[0]);
        for(int i = 1 ; i < offsetsOrigin.Count - 1; ++i)
        {
        	offsets.Add(offsetsOrigin[i] - offsetsOrigin[i-1]);
        }

        shapeIndex = jumpTime;
        timeClock = 0.0f;
        index = 0;
	}

	public void Start()
	{
		debugOrigin = GetComponent<Transform>();
		if(jumpShape.length < 2)
		{
			Debug.LogError("Not enough keys in jump Shape");
		}

		init();
	}

	public void startJump(Vector2 origin)
	{
		timeClock = 0.0f;
		shapeIndex = startTime;
		currentJumpStart = origin;
		index = 0;
	}

	
    public bool jumpEnded()
    {
    	return timeClock >= jumpTime;
    }

    public Vector3 getNext()
	{
		int ind = index;
		index++;
		timeClock += Time.deltaTime;

		if(index >= offsets.Count)
		{
			index = offsets.Count - 1;
		}

		Vector3 ret = offsets[ind];

		if(!goingRight)
		{
			ret = new Vector3(ret.x * -1, ret.y, ret.z);
		}

		return ret;
	}

	// For now let's impose speed
	public Vector3 getNextKey()
	{
		shapeIndex += Time.deltaTime;
		timeClock += Time.deltaTime;

		if(jumpEnded())
		{
			shapeIndex = jumpShape[jumpShape.length - 1].time;
		}

		float expectedY = jumpShape.Evaluate(shapeIndex) * yDistance;
		float expectedX = index * expectedVelocity;

		++index;

		Vector3 offset = new Vector3(expectedX, expectedY, 0.0f);

		return  offset; 
	}

	public void endJump()
	{
		shapeIndex = shapeIndex;
		currentJumpStart = Vector2.zero;
	}

	// --------------------------------- Editor Functions -----------------------------------------------

	void OnDrawGizmosSelected() 
	{
        if (debugOrigin != null) 
        {
            Gizmos.color = Color.blue;

            Vector3 from = debugOrigin.position;
            for(int i = 0; i < offsets.Count - 1; ++i)
            {
            	Vector3 to = from + offsets[i];
            	Gizmos.DrawLine(from ,to);
            	from = to;
            }
        }
    }

}