using UnityEngine;
using System.Collections;
using Lean.Touch;
using System;

public class WalkEnemyController2D : MonoBehaviour
{
	public enum WalkDirection
	{
		Right,
		Left
	}

	private CharacterState state;
	private Rigidbody2D rb;
	private Active active;
	//private Transform transform;

	public WalkDirection startWalkDirection = WalkDirection.Left;
	public float xSpeed = 4;

	private bool facingRight = true;

	public void Awake()
	{
		rb = GetComponent<Rigidbody2D>();
		state = GetComponent<CharacterState>();
		active = GetComponent<Active>();
		active.setActive();
		//transform = GetComponent<Transform>();
	}

	public void Start()
	{
		if(startWalkDirection == WalkDirection.Left)
		{
			changeDirection();
		}
	}

	private void Flip()
	{
		facingRight = !facingRight;
		Vector3 theScale = transform.localScale;
		theScale.x *= -1;
		transform.localScale = theScale;
	}

	private void changeDirection()
	{
		Flip(); // display

		xSpeed *= -1;
	}

	public void Update()
	{
		if(active.isActive() == false)
		{
			rb.velocity = Vector2.zero;
		}
		else
		{
			state.updateState();

			if((state.isCollidingRight && xSpeed > 0)
				|| (state.isCollidingLeft && xSpeed < 0))
			{
				changeDirection();
			}

			rb.velocity = new Vector2(xSpeed, rb.velocity.y);
		}
	}

}