using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

/* Class that handles jumps, keeping which jump we're doing 
*/
[Serializable]
public class JumpCollection
{
	//---------------------------------- Members
	private List<JumpCharacs> jumps; // all jumps
	private int currentIndex; // current jump
	private List<int> jumpsRealised; // keep track of how many jumps we did

	// --------------------------------- Public functions
	public JumpCollection()
	{
		jumps = new List<JumpCharacs>();
		currentIndex = -1;
		jumpsRealised = new List<int>();
	}

	public void addJump(JumpCharacs j)
	{
		jumps.Add(j);
		jumpsRealised.Add(0);
	}

	public void reset()
	{
		endJump();
		currentIndex = -1;
	}

	public void endJump()
	{
		if(currentIndex >= 0)
		{
			jumps[currentIndex].endJump();	
		}
	}

	public void startJump(Vector3 position)
	{
		currentIndex++;
		if(jumps.Count <= currentIndex)
		{
			Debug.LogError("Cannot jump, no more jump registered, jumps : " + jumps.Count + ", index : " + currentIndex);
		}

		cleanPrecedentJump();
		jumps[currentIndex].startJump(position);
		jumpsRealised[currentIndex]++;
	}

	public int getCurrentJumpIndex()
	{
		return currentIndex + 1;
	}

	public bool isJumping()
	{
		return currentIndex >= 0 && jumps[currentIndex].jumpEnded() == false;
	}

	public bool jumpEnded()
	{
		return !isJumping();
	}

	public int getNumberOfFirstJump()
	{
		return jumpsRealised[0];
	}

	public int getNumberOfDoubleJump()
	{
		return jumpsRealised[1];
	}

	public int getNumberOfJumps()
	{
		return getNumberOfFirstJump() + getNumberOfDoubleJump();
	}

	public Vector3 getNext()
	{
		if(currentIndex < 0 || currentIndex >= jumps.Count)
		{
			return Vector3.zero;
		}

		Vector3 ret = jumps[currentIndex].getNext();

		resetIfNecessary();

		return ret;
	}

	// -------------------------------------- Private functions
	private void resetIfNecessary()
	{
		if(currentIndex >= 0 && jumps[currentIndex].jumpEnded())	
		{
			jumps[currentIndex].endJump();
		}
	}

	private void cleanPrecedentJump()
	{
		if(jumps.Count == 0)
		{
			return;
		}

		int ind = currentIndex;
		ind--;
		if(ind < 0)
		{
			ind = 0;
		}

		jumps[ind].endJump();
	}

}