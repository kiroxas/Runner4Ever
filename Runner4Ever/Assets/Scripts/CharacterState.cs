using UnityEngine;
using System.Collections;
using Lean.Touch;
using System.Collections.Generic;
using System.Linq; 

public class RaycastCollision
{
	public Collider2D other;
	public Vector3 point;

	public RaycastCollision(Collider2D o, Vector3 p)
	{
		other = o;
		point = p;
	}
}

public class InnerState
{
	public bool isGrounded { get; set; }
	public bool isCollidingAbove { get; set; }
	public bool isCollidingRight { get; set; }
	public bool isCollidingLeft { get; set; }
	public bool isWallstickingLeft { get; set; }
	public bool isWallstickingRight { get; set; }
	public bool isCollidingSide { get {return isCollidingRight || isCollidingLeft;}}
	public bool isGrabingEdge { get; set; }
	public bool isWallSticking { get{ return isCollidingSide && (isWallstickingLeft || isWallstickingRight);} }	
}


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
	//private Transform transform;

	public InnerState innerState;

	public bool isGrounded { get{ return innerState.isGrounded;} }
	public bool isCollidingAbove {  get{ return innerState.isCollidingAbove;} }
	public bool isCollidingRight {  get{ return innerState.isCollidingRight;}}
	public bool isCollidingLeft { get{ return innerState.isCollidingLeft;}}
	public bool isCollidingSide { get {return innerState.isCollidingSide;}}
	public bool isGrabingEdge { get{ return innerState.isGrabingEdge;} }
	public bool isWallSticking { get{ return innerState.isWallSticking;} }

	public float yRightColDetDelta = 0.02f;

	public float groundedCastDistance = 0.01f;
	public float rcCastDistance = 0.01f;
	public int groundedRayCasts = 8;
	public int rightCollisionRayCasts = 16;
	public LayerMask PlatformMask;
	public float xGroundedOffset = 0.02f;
	public float wallStickingPercent = 0.5f;

	public InnerState deepCopy()
	{
		InnerState state = new InnerState();

		state.isGrounded = isGrounded;
		state.isCollidingAbove = isCollidingAbove;
		state.isCollidingRight = isCollidingRight;
		state.isCollidingLeft = isCollidingLeft;
		state.isGrabingEdge = isGrabingEdge;
		state.isWallstickingLeft = innerState.isWallstickingLeft;
		state.isWallstickingRight = innerState.isWallstickingRight;

		return state;
	} 

	private class ColliderInstances
	{
		public Collider2D instance;
		public int count;
		public Vector3 hitPoint;

		public bool hitCenterAlready = false;

		public ColliderInstances(Collider2D ins, Vector3 hit)
		{
			instance = ins;
			count = 1;
			hitPoint = hit;
		}

		public RaycastCollision get()
		{
			return new RaycastCollision(instance, hitPoint);
		}
	}

	private List<ColliderInstances> colliderHitLastFrame; 
	private List<ColliderInstances> colliderHitThisFrame; 

	public EdgeGrabingStrategy edgeStrategy = EdgeGrabingStrategy.None;

	public void clean()
	{
		colliderHitThisFrame.Clear();
		colliderHitLastFrame.Clear();
	}

	public void Start()
	{
		innerState = new InnerState();
		colliderHitThisFrame = new List<ColliderInstances>();
		colliderHitLastFrame = new List<ColliderInstances>();

		rb = GetComponent<Rigidbody2D>();
		myCollider = GetComponent<Collider2D>();
		
		innerState.isGrounded = false;
		innerState.isCollidingRight = false;
		innerState.isCollidingLeft = false;
		innerState.isGrabingEdge = false;
	}

	void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(myCollider.bounds.min, 0.1f);
        Gizmos.color = Color.black;
        Gizmos.DrawSphere(myCollider.bounds.max, 0.1f);
	}

	public void updateState()
	{
		colliderHitThisFrame.Clear();

		updateGrounded();
		updateAbove();
		updateRightCollision();
		updateLeftCollision();
		updateEdgeGrabing();
		handleCollided();	
	}

	public float boundsXOffset = 0.04f;

	private void drawRaycast(RaycastHit2D raycast, Vector2 rayDirectionNorm, float distance, Vector2 origin)
	{
		Debug.DrawRay(origin, rayDirectionNorm * distance, raycast ? Color.blue : Color.green);
		if(raycast)
		{
			UnityUtils.drawGizmoSquare(raycast.collider.bounds.min,raycast.collider.bounds.max, Color.red);
		}
	}

	private bool isThisOneColliding(Vector2 rayDirectionNorm, ref float distance, Vector2 origin, LayerMask mask)
	{
		Physics2D.queriesHitTriggers = false;
		var raycastHit = Physics2D.Raycast(origin, rayDirectionNorm, distance, mask);
		Physics2D.queriesHitTriggers = true;

		var effector2D = raycastHit.collider ? raycastHit.collider.GetComponent<PlatformEffector2D>() : null;

		drawRaycast(raycastHit, rayDirectionNorm, distance, origin);

		if(raycastHit && (effector2D == null || effector2D.useOneWay == false))
		{
			distance = Mathf.Min(distance, Vector2.Distance(origin, raycastHit.point));
			distance -= 0.01f;
		}

		return raycastHit;
	}

	public bool isThisColliding(Vector2 rayDirection, ref float distance)
	{	
		Vector2 rayDirectionNorm = rayDirection.normalized;
		float halfSize =  (myCollider.bounds.size.x / 2.0f);
		float halfYSize =  (myCollider.bounds.size.y / 2.0f);

		Vector2 topLeft = new Vector2(myCollider.bounds.min.x , myCollider.bounds.max.y );
		Vector2 topMiddle = new Vector2(myCollider.bounds.max.x - halfSize , myCollider.bounds.max.y );
		Vector2 topRight = new Vector2(myCollider.bounds.max.x , myCollider.bounds.max.y );
		Vector2 bottomRight = new Vector2(myCollider.bounds.max.x , myCollider.bounds.min.y );
		Vector2 bottomLeft = new Vector2(myCollider.bounds.min.x , myCollider.bounds.min.y );
		Vector2 bottomCenter = new Vector2(myCollider.bounds.min.x  + halfSize, myCollider.bounds.min.y );
		Vector2 middleLeft = new Vector2(myCollider.bounds.min.x , myCollider.bounds.max.y - halfYSize);
		Vector2 middleRight = new Vector2(myCollider.bounds.max.x , myCollider.bounds.max.y - halfYSize);

		bool colliding = false;

		colliding |= isThisOneColliding(rayDirectionNorm, ref distance, topLeft, PlatformMask);
		colliding |= isThisOneColliding(rayDirectionNorm, ref distance, topMiddle, PlatformMask);
		colliding |= isThisOneColliding(rayDirectionNorm, ref distance, topRight, PlatformMask);
		colliding |= isThisOneColliding(rayDirectionNorm, ref distance, bottomRight, PlatformMask);
		colliding |= isThisOneColliding(rayDirectionNorm, ref distance, bottomLeft, PlatformMask);
		colliding |= isThisOneColliding(rayDirectionNorm, ref distance, bottomCenter, PlatformMask);
		colliding |= isThisOneColliding(rayDirectionNorm, ref distance, middleLeft, PlatformMask);
		colliding |= isThisOneColliding(rayDirectionNorm, ref distance, middleRight, PlatformMask);
		
		return colliding;
	}

	private void addCollider(Collider2D col, Vector3 hitpoint)
	{
		ColliderInstances ins = colliderHitThisFrame.Find(c => col == c.instance);
		if(ins == null)
		{
			colliderHitThisFrame.Add(new ColliderInstances(col, hitpoint));
		}
		else
		{
			ins.count++;
		}

	}

	private bool checkRaycast( RaycastHit2D raycastHit, Collider2D myCollider)
	{
		if (raycastHit)
		{
			var effector2D = raycastHit.collider.GetComponent<PlatformEffector2D>();
			if(effector2D != null)
			{
				if(effector2D.useOneWay && myCollider.bounds.min.y > raycastHit.collider.bounds.max.y ) // Only allowed if above
				{
					addCollider(raycastHit.collider, raycastHit.point);
					return raycastHit.collider.isTrigger == false;
				}
			}
			else
			{
				addCollider(raycastHit.collider, raycastHit.point);
				return raycastHit.collider.isTrigger == false;					
			}
		}

		return false;
	}

	

	public bool updateGrounded()
	{
		float step = (float)(myCollider.bounds.size.x - (2 * xGroundedOffset)) / (float)groundedRayCasts;
		innerState.isGrounded = false;

		Vector2 rayDirection = Vector2.down;

		for(int i = 0; i <= groundedRayCasts; ++i)
		{
			Vector2 rayVector = new Vector2(myCollider.bounds.min.x + xGroundedOffset + i * step, myCollider.bounds.min.y);
			var raycastHit = Physics2D.Raycast(rayVector, rayDirection, groundedCastDistance, PlatformMask);

			drawRaycast(raycastHit, rayDirection, groundedCastDistance, rayVector);

			if (checkRaycast(raycastHit, myCollider))
			{
				innerState.isGrounded = true;
			}

			if(raycastHit.collider && raycastHit.collider.isTrigger) // if it's a trigger, let's recast, ignoring triggers
			{
				Physics2D.queriesHitTriggers = false;
				raycastHit = Physics2D.Raycast(rayVector, rayDirection, groundedCastDistance, PlatformMask);
				if(checkRaycast(raycastHit, myCollider))
				{
					innerState.isGrounded = true;
				}
				Physics2D.queriesHitTriggers = true;
			}			
		}

		return isGrounded;
	}

	public bool updateAbove()
	{
		float step = (float)(myCollider.bounds.size.x - (2 * xGroundedOffset))/ (float)groundedRayCasts;
		innerState.isCollidingAbove = false;

		Vector2 rayDirection = Vector2.up;

		for(int i = 0; i <= groundedRayCasts; ++i)
		{
			Vector2 rayVector = new Vector2(myCollider.bounds.min.x + xGroundedOffset + i * step, myCollider.bounds.max.y);
			var raycastHit = Physics2D.Raycast(rayVector, rayDirection, groundedCastDistance, PlatformMask);

			drawRaycast(raycastHit, rayDirection, groundedCastDistance, rayVector);

			if(checkRaycast(raycastHit, myCollider))
			{
				innerState.isCollidingAbove = true;
			}
		}

		return isCollidingAbove;
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
			innerState.isGrabingEdge = false;
			return isGrabingEdge;
		}

		innerState.isGrabingEdge = false;

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
					innerState.isGrabingEdge = true;
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
					innerState.isGrabingEdge = true;
					break;
				}
			}
		}

		return isGrabingEdge;
	}

	private void handleCollided()
	{
		colliderHitThisFrame = colliderHitThisFrame.OrderBy(c => c.count).ToList(); // most hitted first

		// New collisions
		foreach(ColliderInstances collider in colliderHitThisFrame)
		{
			if(colliderHitLastFrame.Find(c => c.instance == collider.instance) == null) // new collision
			{
				if(collider.instance.isTrigger)
				{
					collider.instance.SendMessage("OnTriggerEnterCustom", new RaycastCollision(myCollider, collider.hitPoint) , SendMessageOptions.DontRequireReceiver); 
				}
				else
				{
					collider.instance.SendMessage("OnCollisionEnterCustom", new RaycastCollision(myCollider, collider.hitPoint), SendMessageOptions.DontRequireReceiver);
				}
			}
			else
			{
				if(collider.instance.isTrigger)
				{
					collider.instance.SendMessage("OnTriggerStayCustom", new RaycastCollision(myCollider, collider.hitPoint) , SendMessageOptions.DontRequireReceiver); 
				}
				else
				{
					collider.instance.SendMessage("OnCollisionStayCustom", new RaycastCollision(myCollider, collider.hitPoint), SendMessageOptions.DontRequireReceiver);
				}
			}
		}

		// Collisions have center aligned
		foreach(ColliderInstances collider in colliderHitThisFrame)
		{
			float epsilon = 0.2f;

			float colCenter = collider.instance.bounds.center.x;
			float myCenter = myCollider.bounds.center.x;

			if( myCenter >= colCenter - epsilon && myCenter <= myCenter + epsilon)
			{
				collider.instance.SendMessage("OnCollisionCenterAlign", new RaycastCollision(myCollider, collider.hitPoint) , SendMessageOptions.DontRequireReceiver); 
			}
		}

		// Collisons not happening
		foreach(ColliderInstances collider in colliderHitLastFrame)
		{
			if(colliderHitThisFrame.Find(c => c.instance == collider.instance) == null) // new collision
			{
				if(collider.instance.isTrigger)
				{
					collider.instance.SendMessage("OnTriggerExitCustom", new RaycastCollision(myCollider, collider.hitPoint), SendMessageOptions.DontRequireReceiver); 
				}
				else
				{
					collider.instance.SendMessage("OnCollisionExitCustom", new RaycastCollision(myCollider, collider.hitPoint), SendMessageOptions.DontRequireReceiver);
				}
			}	
		}

		colliderHitLastFrame.Clear();
		colliderHitLastFrame = new List<ColliderInstances>(colliderHitThisFrame);
	}

	private float getWallThreshold()
	{
		float margin = myCollider.bounds.size.y * wallStickingPercent;
		float threshold = myCollider.bounds.max.y - margin;

		return threshold;
	}

	public bool updateLeftCollision()
	{
		float step = (float)myCollider.bounds.size.y / (float)rightCollisionRayCasts;
		innerState.isCollidingLeft = false;
		innerState.isWallstickingLeft = false;

		Vector2 rayDirection = Vector2.left;
		float leftX = Mathf.Min(myCollider.bounds.max.x , myCollider.bounds.min.x);

		float threshold = getWallThreshold();

		for(int i = 0; i < rightCollisionRayCasts; ++i)
		{
			float y = myCollider.bounds.min.y + yRightColDetDelta + i * step;

			Vector2 rayVector = new Vector2(leftX , y);
			var raycastHit = Physics2D.Raycast(rayVector, rayDirection, rcCastDistance, PlatformMask);

			drawRaycast(raycastHit, rayDirection, rcCastDistance, rayVector);

			if(checkRaycast(raycastHit, myCollider))
			{
				innerState.isCollidingLeft = true;
				if(y >= threshold)
					innerState.isWallstickingLeft = true;
			}
		}

		return isCollidingLeft;
	}

	public bool updateRightCollision()
	{
		float step = (float)myCollider.bounds.size.y / (float)rightCollisionRayCasts;
		innerState.isCollidingRight = false;
		innerState.isWallstickingRight = false;

		Vector2 rayDirection = Vector2.right;
		float rightX = Mathf.Max(myCollider.bounds.max.x , myCollider.bounds.min.x);

		float threshold = getWallThreshold();

		for(int i = 0; i < rightCollisionRayCasts; ++i)
		{
			float y = myCollider.bounds.min.y + yRightColDetDelta + i * step;

			Vector2 rayVector = new Vector2(rightX ,y);
			var raycastHit = Physics2D.Raycast(rayVector, rayDirection, rcCastDistance, PlatformMask);

			drawRaycast(raycastHit, rayDirection, rcCastDistance, rayVector);
			
			if(checkRaycast(raycastHit, myCollider))
			{
				innerState.isCollidingRight = true;
				if(y >= threshold)
					innerState.isWallstickingRight = true;
			}
		}

		return isCollidingRight;
	}
}