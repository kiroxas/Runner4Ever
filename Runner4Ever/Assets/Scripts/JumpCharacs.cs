using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

/* Class that handles a jump, its definition and all the points of the curve 
*/
[Serializable]
public class JumpCharacs
{
	// --------------------------------- Public Interface -----------------------------------------------
	public AnimationCurve jumpShape; // the curve
	public float xDistance = 1.0f; // distance we are going to travel on X 
	public float yDistance = 1.0f; // Y for one unit in the curve editor 


	// ---------------------------------- Private Members -----------------------------------------------
	public Transform debugOrigin; // the transform to plot the curve

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

	private bool inJump = false;
	private string name; // can name it for log

	// --------------------------------- Functions -----------------------------------------------

	public JumpCharacs()
	{
		jumpShape = new AnimationCurve();
	}

	public void flip()
	{
		goingRight = !goingRight;
	}

	public void init()
	{
		if(jumpShape.length < 2)
		{
			Debug.LogError("Not enough keys in jump Shape : " + name);
		}

		startTime = jumpShape[0].time;
		jumpTime = jumpShape[jumpShape.length - 1].time - startTime;

		int FPS = (int)(1f / Time.unscaledDeltaTime);
		float time = jumpTime * FPS;

		expectedVelocity = xDistance / time;
		
		index = 0;
		shapeIndex = startTime;

		offsetsOrigin = new List<Vector3>();
		inJump = true;

		while(!jumpEnded())
        {
            offsetsOrigin.Add(getNextKey());
        }

        offsets = new List<Vector3>();

        if(offsetsOrigin.Count > 0)
        {
        	offsets.Add(offsetsOrigin[0]);
        	for(int i = 1 ; i < offsetsOrigin.Count - 1; ++i)
       		{
        		offsets.Add(offsetsOrigin[i] - offsetsOrigin[i-1]);
        	}
    	}

        shapeIndex = jumpTime;
        timeClock = 0.0f;
        index = 0;
        inJump = false;
	}

	public void startJump(Vector2 origin)
	{
		//Debug.Log("Start jump for : " + name);
		timeClock = 0.0f;
		shapeIndex = startTime;
		currentJumpStart = origin;
		index = 0;
		inJump = true;
	}

	
    public bool jumpEnded()
    {
    	return  !inJump; // timeClock >= jumpTime;
    }

    public Vector3 getHighestPoint()
    {
    	Vector3 highest = offsetsOrigin[0];

    	foreach(Vector3 v in offsetsOrigin)
    	{
    		if(v.y > highest.y)
    		{
    			highest = v;
    		}
    	}

    	return highest;
    }

    public Vector3 getNext()
	{
		int ind = index;
		index++;
		timeClock += Time.deltaTime;

		if(index >= offsets.Count)
		{
			endJump();
			return Vector3.zero;
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

		if(timeClock >= jumpTime)
		{
			shapeIndex = jumpShape[jumpShape.length - 1].time;
			endJump();
		}

		float expectedY = jumpShape.Evaluate(shapeIndex) * yDistance;
		float expectedX = index * expectedVelocity;

		++index;

		Vector3 offset = new Vector3(expectedX, expectedY, 0.0f);

		return  offset; 
	}

	public void endJump()
	{
		//Debug.Log("End jump for : " + name);
		shapeIndex = jumpTime;
		currentJumpStart = Vector2.zero;
		index = 0;
		inJump = false;
	}

	public void reinit()
	{
		goingRight = true;
	}

	// --------------------------------- Editor Functions -----------------------------------------------

	public void setName(string n)
	{
		name = n;
	}

	public void setDebugTransform(Transform t)
	{
		debugOrigin = t;
	}

	public Vector3 getDebugPosition()
	{
		if(debugOrigin == null)
			return Vector3.zero;

		return debugOrigin.position;
	}

	public void OnDrawGizmosSelected(Vector3 position) 
	{      
        Vector3 from = position;
        for(int i = 0; i < offsets.Count - 1; ++i)
        {
            Vector3 to = from + offsets[i];
            Gizmos.DrawLine(from ,to);
            from = to;
        }
    }

	public void OnDrawGizmosSelected() 
	{
        if (debugOrigin != null) 
        {
            OnDrawGizmosSelected(debugOrigin.position);
        }
    }

}