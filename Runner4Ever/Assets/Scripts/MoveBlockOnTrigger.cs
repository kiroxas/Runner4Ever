using UnityEngine;
using System.Collections;
using Lean.Touch;
using System.Collections.Generic;

public class MoveBlockOnTrigger : MonoBehaviour
{
	public enum MovementStyle
	{
		Constant,
		Acceleration
	}

	// Public
	public MovementStyle movement = MovementStyle.Constant;
	public float raycastDistance = 10.0f;
	public float speed = 6.0f;
	public float accelerationSmooth = 1.5f;

	public bool triggerBelow = true;
	public bool triggerRight = false;
	public bool triggerLeft = false;
	public bool triggerAbove = true;

	public LayerMask triggerMask;
	public float maxDistance = 10.0f;

	// Private

	private CharacterController2D player;
	private Vector2 offsetByUpdate;
	private Vector2 originalPos;
	private Transform transform;
	private Collider2D myCollider;
	private Rigidbody2D rb;
	private float currentSpeed = 0;
	private bool triggered = false;
	private Vector2 _vel;
	private bool comingBack = false;

	public void Start()
	{
		myCollider = GetComponent<Collider2D>();
		transform = GetComponent<Transform>();
		rb = GetComponent<Rigidbody2D>();
		originalPos = transform.position;
		offsetByUpdate = new Vector2(0,0);
	}

	public bool TriggerRight()
	{
		float x = myCollider.bounds.max.x;
		float y = myCollider.bounds.max.y - (myCollider.bounds.size.y / 2.0f);

		Vector2 rayVector = new Vector2(x,y);
		Vector2 rayDirection = Vector2.right;

		var raycastHit = Physics2D.Raycast(rayVector, rayDirection, raycastDistance, triggerMask);
		Debug.DrawRay(rayVector, rayDirection * raycastDistance, Color.red);

		return raycastHit;
	}

	public bool TriggerLeft()
	{
		float x = myCollider.bounds.min.x;
		float y = myCollider.bounds.max.y - (myCollider.bounds.size.y / 2.0f);

		Vector2 rayVector = new Vector2(x,y);
		Vector2 rayDirection = Vector2.left;

		var raycastHit = Physics2D.Raycast(rayVector, rayDirection, raycastDistance, triggerMask);
		Debug.DrawRay(rayVector, rayDirection * raycastDistance, Color.blue);

		return raycastHit;
	}

	public bool TriggerUp()
	{
		float x = myCollider.bounds.min.x + (myCollider.bounds.size.x / 2.0f);
		float y = myCollider.bounds.max.y;

		Vector2 rayVector = new Vector2(x,y);
		Vector2 rayDirection = Vector2.up;

		var raycastHit = Physics2D.Raycast(rayVector, rayDirection, raycastDistance, triggerMask);
		Debug.DrawRay(rayVector, rayDirection * raycastDistance, Color.green);

		return raycastHit;
	}

	public bool TriggerDown()
	{
		float x = myCollider.bounds.min.x + (myCollider.bounds.size.x / 2.0f);
		float y = myCollider.bounds.min.y;

		Vector2 rayVector = new Vector2(x,y);
		Vector2 rayDirection = Vector2.down;

		var raycastHit = Physics2D.Raycast(rayVector, rayDirection, raycastDistance, triggerMask);
		Debug.DrawRay(rayVector, rayDirection * raycastDistance, Color.yellow);

		return raycastHit;
	}

	private void move(Vector2 dir)
	{
		_vel = dir * speed;
		if(movement == MovementStyle.Constant)
		{
			rb.velocity = _vel;
		}
	}

	private void goBack()
	{
		comingBack = true;
		_vel *= -1;
	}

	void OnCollisionEnterCustom(RaycastCollision other) 
	{
		var state = other.other.GetComponent<CharacterController2D>();
        if(state != null && other.other.gameObject.tag == "Player")
        {
        	player = state;
        }
	}

	void OnCollisionExitCustom(RaycastCollision notHitted) 
	{
		player = null;
	}

	public void FixedUpdate()
	{
		if(triggered)
		{
			rb.velocity = Vector2.Lerp(rb.velocity, _vel, Time.deltaTime * accelerationSmooth);
			int FPS =  UnityUtils.getFPS();
			offsetByUpdate = new Vector2(rb.velocity.x / FPS, rb.velocity.y / FPS);

			if(player)
			{
				Debug.Log("Add Outside " + offsetByUpdate);
				player.addOutsideForce(offsetByUpdate);
			}

			if(!comingBack && Vector2.Distance(transform.position, originalPos) > maxDistance) // STAPH
			{
				goBack();
			}
			else if(comingBack&& Vector2.Distance(transform.position, originalPos) < 0.1f)
			{
				triggered = false;
				comingBack = false;
				rb.velocity = new Vector2(0.0f, 0.0f);
			}
		}
		else
		{
			if(triggerAbove)
			{
				if(TriggerUp())
				{
					originalPos = transform.position;
					triggered = true;
					move(Vector2.up);
				}
			}

			if(triggerLeft)
			{
				if(TriggerLeft())
				{
					originalPos = transform.position;
					triggered = true;
					move(Vector2.left);
				}
			}

			if(triggerRight)
			{
				if(TriggerRight())
				{
					originalPos = transform.position;
					triggered = true;
					move(Vector2.right);
				}
			}

			if(triggerBelow)
			{
				if(TriggerDown())
				{
					originalPos = transform.position;
					triggered = true;
					move(Vector2.down);
				}
			}
		}
	}
}