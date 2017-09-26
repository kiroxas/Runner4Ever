using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumpCharacs : MonoBehaviour
{
	public AnimationCurve jumpShape;
	public float xDistance = 1.0f; // distance we are going to travel on X 
	public float yDistance = 1.0f; // max Y 
	private Transform debugOrigin;

	private float jumpTime;

	private float expectedVelocity;
	private Vector2 currentJumpStart;
	private Vector2 nextFrame;
	private float shapeIndex;
	private List<Vector3> offsetsOrigin;
	private List<Vector3> offsets;

	private int index;
	private float startTime;
	private float timeClock = 0.0f;



	public void Start()
	{
		debugOrigin = GetComponent<Transform>();
		if(jumpShape.length < 2)
		{
			Debug.LogError("Not enough keys in jump Shape");
		}

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

        Debug.Log("Time of jump : " + jumpTime + " expected velocity : " + expectedVelocity + " offsetsOrigin : " + offsetsOrigin.Count);

        shapeIndex = jumpTime;
        timeClock = 0.0f;
        index = 0;
	}

	public void startJump(Vector2 origin)
	{
		timeClock = 0.0f;
		shapeIndex = startTime;
		currentJumpStart = origin;
		index = 0;
	}

	void OnDrawGizmosSelected() 
	{
        if (debugOrigin != null) 
        {
            Gizmos.color = Color.blue;

           // for(int i = 0; i < offsetsOrigin.Count - 1; ++i)
           // {
           // 	Gizmos.DrawLine(debugOrigin.position + offsetsOrigin[i],debugOrigin.position + offsetsOrigin[i + 1]);
           // }
             
            Vector3 from = debugOrigin.position;
            for(int i = 0; i < offsets.Count - 1; ++i)
            {
            	Vector3 to = from + offsets[i];
            	Gizmos.DrawLine(from ,to);
            	from = to;
            }
        }
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

		return offsets[ind];
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
}