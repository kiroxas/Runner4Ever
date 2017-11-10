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

	private bool loopJumps = false;

	// --------------------------------- Public functions
	public void loopingJumps(bool b)
	{
		loopJumps = b;
	}

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
			if(loopJumps && (currentIndex == jumps.Count - 1))
			{
				currentIndex = -1;
			}	
		}
	}

	public bool cantJumpReachedMaxJumps()
	{
		return currentIndex >= 0 && jumps[currentIndex].jumpEnded() && (currentIndex >= jumps.Count - 1);
	}

	public void startJump(Vector3 position)
	{
		currentIndex++;
		if(jumps.Count <= currentIndex)
		{
			currentIndex = 0;
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
		if(jumpsRealised.Count > 0)
			return jumpsRealised[0];

		return 0;
	}

	public int getNumberOfDoubleJump()
	{
		if(jumpsRealised.Count > 1)
			return jumpsRealised[1];

		return 0;
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

	public void reinit()
	{
		foreach(JumpCharacs ju in jumps)
		{
			ju.reinit();
		}
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
		if(ind == 0)
		{
			ind = jumps.Count;
		}
		
		ind--;
		if(ind < 0)
		{
			ind = 0;
		}

		jumps[ind].endJump();
	}

}