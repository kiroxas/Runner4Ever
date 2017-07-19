using UnityEngine;
using System.Collections;
using Lean.Touch;
using System.Collections.Generic;

public class CharacterState : MonoBehaviour
{

	public enum EdgeGrabingStrategy
	{
		GoingUpOnly,
		GoingDownOnly,
		Both,
		None
	}

	private Rigidbody2D rb;
	private Collider2D myCollider;

	public bool isGrounded { get; private set; }
	public bool isCollidingRight { get; private set; }
	public bool isCollidingLeft { get; private set; }
	public bool isCollidingSide { get {return isCollidingRight || isCollidingLeft;}}
	public bool isGrabingEdge { get; private set; }
	public bool isWallSticking { get{ return isCollidingSide && ! isGrounded && !isGrabingEdge;} }

	public float yRightColDetDelta = 0.02f;

	public float groundedCastDistance = 0.01f;
	public float rcCastDistance = 0.01f;
	public int groundedRayCasts = 8;
	public int rightCollisionRayCasts = 16;
	public LayerMask PlatformMask;

	private List<Collider2D> colliderHitLastFrame; 
	private List<Collider2D> colliderHitThisFrame; 

	public EdgeGrabingStrategy edgeStrategy = EdgeGrabingStrategy.None;

	public void Start()
	{
		colliderHitThisFrame = new List<Collider2D>();
		colliderHitLastFrame = new List<Collider2D>();

		rb = GetComponent<Rigidbody2D>();
		myCollider = GetComponent<Collider2D>();
		isGrounded = false;
		isCollidingRight = false;
		isCollidingLeft = false;
		isGrabingEdge = false;
	}

	public void updateState()
	{
		colliderHitThisFrame.Clear();

		updateGrounded();
		updateRightCollision();
		updateLeftCollision();
		updateEdgeGrabing();
		handleCollided();
	}

	private void addCollider(Collider2D col)
	{
		if(colliderHitThisFrame.Find(c => col == c) == false)
		{
			colliderHitThisFrame.Add(col);
		}
	}

	public bool updateGrounded()
	{
		Collider2D myCollider = GetComponent<Collider2D>();
		float step = (float)myCollider.bounds.size.x / (float)groundedRayCasts;
		isGrounded = false;

		Vector2 rayDirection = Vector2.down;

		{
			Vector2 dir = rayDirection;
			dir.x = -1;
			Vector2 rayVector = new Vector2(myCollider.bounds.min.x , myCollider.bounds.min.y);
			var raycastHit = Physics2D.Raycast(rayVector, dir, rcCastDistance, PlatformMask);
			Debug.DrawRay(rayVector, dir * groundedCastDistance, Color.green);
			if (raycastHit)
			{
				addCollider(raycastHit.collider);
				isGrounded = true;
			}
		
			dir.x = 1;
			rayVector = new Vector2(myCollider.bounds.max.x , myCollider.bounds.min.y);
			raycastHit = Physics2D.Raycast(rayVector, dir, rcCastDistance, PlatformMask);
			Debug.DrawRay(rayVector, dir * groundedCastDistance, Color.green);
			if (raycastHit)
			{
				addCollider(raycastHit.collider);
				isGrounded = true;
			}
		}

		for(int i = 0; i < groundedRayCasts; ++i)
		{
			Vector2 rayVector = new Vector2(myCollider.bounds.min.x + i * step, myCollider.bounds.min.y);
			var raycastHit = Physics2D.Raycast(rayVector, rayDirection, groundedCastDistance, PlatformMask);
			Debug.DrawRay(rayVector, rayDirection * groundedCastDistance, Color.green);
			if (raycastHit)
			{
				addCollider(raycastHit.collider);
				isGrounded = true;
			}
		}

		return isGrounded;
	}

	public bool updateEdgeGrabing()
	{
		float yVelocity = rb.velocity.y;
		float xVelocity = rb.velocity.x;

		if((isGrounded || !isCollidingSide)
			|| (edgeStrategy == EdgeGrabingStrategy.GoingUpOnly && yVelocity < 0)
			|| (edgeStrategy == EdgeGrabingStrategy.GoingDownOnly && yVelocity > 0)
			|| (edgeStrategy == EdgeGrabingStrategy.None))
		{
			isGrabingEdge = false;
			return isGrabingEdge;
		}

		isGrabingEdge = false;

		// Right
		if(xVelocity >= 0) // if 0, could be grabing ledge, so must check
		{
			Collider2D myCollider = GetComponent<Collider2D>();
			float step = (float)myCollider.bounds.size.y / (float)rightCollisionRayCasts;
			
			bool collided = false; // make sure we already had a collision, maybe it's a collision with a thin platform
			Vector2 rayDirection = Vector2.right;
			for(int i = 0; i < rightCollisionRayCasts + 1; ++i) // +1 to throw one above the character head
			{
				Vector2 rayVector = new Vector2(myCollider.bounds.max.x , myCollider.bounds.min.y + yRightColDetDelta + i * step);
				var raycastHit = Physics2D.Raycast(rayVector, rayDirection, rcCastDistance, PlatformMask);
				Debug.DrawRay(rayVector, rayDirection * rcCastDistance, Color.green);

				collided |= raycastHit;

				if (!raycastHit && collided)
				{
					isGrabingEdge = true;
					break;
				}
			}
		}

		if(!isGrabingEdge && xVelocity <= 0) // left
		{
			Collider2D myCollider = GetComponent<Collider2D>();
			float step = (float)myCollider.bounds.size.y / (float)rightCollisionRayCasts;

			bool collided = false; // make sure we already had a collision, maybe it's a collision with a thin platform
			Vector2 rayDirection = Vector2.left;
			for(int i = 0; i < rightCollisionRayCasts + 1; ++i) // +1 to throw one above the character head
			{
				Vector2 rayVector = new Vector2(myCollider.bounds.min.x , myCollider.bounds.min.y + yRightColDetDelta + i * step);
				var raycastHit = Physics2D.Raycast(rayVector, rayDirection, rcCastDistance, PlatformMask);
				Debug.DrawRay(rayVector, rayDirection * rcCastDistance, Color.green);

				collided |= raycastHit;

				if (!raycastHit && collided)
				{
					isGrabingEdge = true;
					break;
				}
			}
		}

		return isGrabingEdge;
	}

	private void handleCollided()
	{
		Debug.Log("Collide : " + colliderHitThisFrame.Count );
		// New collisions
		foreach(Collider2D collider in colliderHitThisFrame)
		{
			if(colliderHitLastFrame.Find(c => c == collider) == false) // new collision
			{
				if(collider.isTrigger)
				{
					collider.SendMessage("OnTriggerEnter2D", myCollider , SendMessageOptions.DontRequireReceiver); 
				}
				else
				{
					collider.SendMessage("OnCollisionEnter2D", myCollider, SendMessageOptions.DontRequireReceiver);
				}
			}
			else
			{
				if(collider.isTrigger)
				{
					collider.SendMessage("OnTriggerStay2D", myCollider , SendMessageOptions.DontRequireReceiver); 
				}
				else
				{
					collider.SendMessage("OnCollisionStay2D", myCollider, SendMessageOptions.DontRequireReceiver);
				}
			}
		}

		// Collisons not happening
		foreach(Collider2D collider in colliderHitLastFrame)
		{
			if(colliderHitThisFrame.Find(c => c == collider) == false) // new collision
			{
				if(collider.isTrigger)
				{
					collider.SendMessage("OnTriggerExit2D", myCollider , SendMessageOptions.DontRequireReceiver); 
				}
				else
				{
					collider.SendMessage("OnCollisionExit2D", myCollider, SendMessageOptions.DontRequireReceiver);
				}
			}	
		}

		colliderHitLastFrame.Clear();
		colliderHitLastFrame = new List<Collider2D>(colliderHitThisFrame);
	}


	public bool updateLeftCollision()
	{
		Collider2D myCollider = GetComponent<Collider2D>();
		float step = (float)myCollider.bounds.size.y / (float)rightCollisionRayCasts;
		isCollidingLeft = false;

		Vector2 rayDirection = Vector2.left;
		float leftX = Mathf.Min(myCollider.bounds.max.x , myCollider.bounds.min.x);
		
		{
			Vector2 dir = rayDirection;
			dir.y = -1;
			Vector2 rayVector = new Vector2(leftX , myCollider.bounds.min.y + yRightColDetDelta);
			var raycastHit = Physics2D.Raycast(rayVector, dir, rcCastDistance, PlatformMask);
			Debug.DrawRay(rayVector, dir * rcCastDistance, Color.red);
			if (raycastHit)
			{
				addCollider(raycastHit.collider);
				isCollidingLeft = true;
			}
		}

		for(int i = 0; i < rightCollisionRayCasts; ++i)
		{
			Vector2 rayVector = new Vector2(leftX , myCollider.bounds.min.y + yRightColDetDelta + i * step);
			var raycastHit = Physics2D.Raycast(rayVector, rayDirection, rcCastDistance, PlatformMask);
			Debug.DrawRay(rayVector, rayDirection * rcCastDistance, Color.red);
			if (raycastHit)
			{
				isCollidingLeft = true;
				addCollider(raycastHit.collider);
			}
		}

		return isCollidingLeft;
	}

	public bool updateRightCollision()
	{
		Collider2D myCollider = GetComponent<Collider2D>();
		float step = (float)myCollider.bounds.size.y / (float)rightCollisionRayCasts;
		isCollidingRight = false;

		Vector2 rayDirection = Vector2.right;
		float rightX = Mathf.Max(myCollider.bounds.max.x , myCollider.bounds.min.x);

		{
			Vector2 dir = rayDirection;
			dir.y = -1;
			Vector2 rayVector = new Vector2(rightX , myCollider.bounds.min.y + yRightColDetDelta);
			var raycastHit = Physics2D.Raycast(rayVector, dir, rcCastDistance, PlatformMask);
			Debug.DrawRay(rayVector, dir * rcCastDistance, Color.black);
			if (raycastHit)
			{
				isCollidingRight = true;
				addCollider(raycastHit.collider);
			}
		}

		for(int i = 0; i < rightCollisionRayCasts; ++i)
		{
			Vector2 rayVector = new Vector2(rightX , myCollider.bounds.min.y + yRightColDetDelta + i * step);
			var raycastHit = Physics2D.Raycast(rayVector, rayDirection, rcCastDistance, PlatformMask);
			Debug.DrawRay(rayVector, rayDirection * rcCastDistance, Color.black);
			if (raycastHit)
			{
				isCollidingRight = true;
				addCollider(raycastHit.collider);
			}
		}

		return isCollidingRight;
	}
}