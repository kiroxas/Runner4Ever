using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumpCharacs : MonoBehaviour
{
	public AnimationCurve jumpShape;
	public float xDistance = 1.0f; // distance we are going to travel on X 
	public float yDistance = 1.0f; // max Y 
	public Transform debugOrigin;

	private float jumpTime;
	private float speed;
	private float expectedVelocity;
	private Vector2 currentJumpStart;
	private Vector2 nextFrame;
	private float timeJumping;
	private List<Vector3> offsets;

	public void Start()
	{
		if(jumpShape.length < 2)
		{
			Debug.LogError("Not enough keys in jump Shape");
		}

		jumpTime = jumpShape[jumpShape.length - 1].time - jumpShape[0].time;
		expectedVelocity = xDistance / jumpTime;
		

		offsets = new List<Vector3>();
		while(!jumpEnded())
        {
            offsets.Add(getNextKey());
        }

        Debug.Log("Time of jump : " + jumpTime + " expected velocity : " + expectedVelocity + " offsets : " + offsets.Count);

	}

	public void startJump(Vector2 origin, float speed)
	{
		currentJumpStart = origin;
	}

	void OnDrawGizmosSelected() 
	{
        if (debugOrigin != null) 
        {
            Gizmos.color = Color.blue;

            for(int i = 0; i < offsets.Count - 1; ++i)
            {
            	Gizmos.DrawLine(debugOrigin.position + offsets[i],debugOrigin.position + offsets[i + 1]);
            }
        }
    }

    public bool jumpEnded()
    {
    	return timeJumping >= jumpTime;
    }

	// For now let's impose speed
	public Vector3 getNextKey()
	{
		timeJumping += Time.deltaTime;
		if(jumpEnded())
		{
			timeJumping = jumpTime;
		}

		float expectedY = jumpShape.Evaluate(timeJumping) * yDistance;
		float expectedX = timeJumping * expectedVelocity;

		Vector3 offset = new Vector3(expectedX, expectedY, 0.0f);

		return  offset; 
	}

	public void endJump()
	{
		timeJumping = 0.0f;
		currentJumpStart = Vector2.zero;
	}
}