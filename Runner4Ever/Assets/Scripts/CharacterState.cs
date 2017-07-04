using UnityEngine;
using System.Collections;
using Lean.Touch;

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

	public bool isGrounded { get; private set; }
	public bool isCollidingRight { get; private set; }
	public bool isGrabingEdge { get; private set; }

	public float yRightColDetDelta = 0.02f;

	public float groundedCastDistance = 0.01f;
	public float rcCastDistance = 0.01f;
	public int groundedRayCasts = 8;
	public int rightCollisionRayCasts = 16;
	public LayerMask PlatformMask;

	public EdgeGrabingStrategy edgeStrategy = EdgeGrabingStrategy.None;

	public void Start()
	{
		rb = GetComponent<Rigidbody2D>();
		isGrounded = false;
		isCollidingRight = false;
		isGrabingEdge = false;
	}

	public void updateState()
	{
		updateGrounded();
		updateRightCollision();
		updateEdgeGrabing();
	}

	public bool updateGrounded()
	{
		Collider2D myCollider = GetComponent<Collider2D>();
		float step = (float)myCollider.bounds.size.x / (float)groundedRayCasts;
		isGrounded = false;

		Vector2 rayDirection = Vector2.down;
		for(int i = 0; i < groundedRayCasts; ++i)
		{
			Vector2 rayVector = new Vector2(myCollider.bounds.min.x + i * step, myCollider.bounds.min.y);
			var raycastHit = Physics2D.Raycast(rayVector, rayDirection, groundedCastDistance, PlatformMask);
			Debug.DrawRay(rayVector, rayDirection * groundedCastDistance, Color.green);
			if (raycastHit)
			{
				isGrounded = true;
				break;
			}
		}

		return isGrounded;
	}

	public bool updateEdgeGrabing()
	{
		float yVelocity = rb.velocity.y;

		if((isGrounded || !isCollidingRight)
			|| (edgeStrategy == EdgeGrabingStrategy.GoingUpOnly && yVelocity < 0)
			|| (edgeStrategy == EdgeGrabingStrategy.GoingDownOnly && yVelocity > 0)
			|| (edgeStrategy == EdgeGrabingStrategy.None))
		{
			isGrabingEdge = false;
			return isGrabingEdge;
		}

		Collider2D myCollider = GetComponent<Collider2D>();
		float step = (float)myCollider.bounds.size.y / (float)rightCollisionRayCasts;
		isGrabingEdge = false;

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

		return isGrabingEdge;
	}



	public bool updateRightCollision()
	{
		Collider2D myCollider = GetComponent<Collider2D>();
		float step = (float)myCollider.bounds.size.y / (float)rightCollisionRayCasts;
		isCollidingRight = false;

		Vector2 rayDirection = Vector2.right;
		for(int i = 0; i < rightCollisionRayCasts; ++i)
		{
			Vector2 rayVector = new Vector2(myCollider.bounds.max.x , myCollider.bounds.min.y + yRightColDetDelta + i * step);
			var raycastHit = Physics2D.Raycast(rayVector, rayDirection, rcCastDistance, PlatformMask);
			Debug.DrawRay(rayVector, rayDirection * rcCastDistance, Color.red);
			if (raycastHit)
			{
				isCollidingRight = true;
				break;
			}
		}

		return isCollidingRight;
	}
}