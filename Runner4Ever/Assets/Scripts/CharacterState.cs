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
	public bool isCollidingSide { get {return isCollidingRight || isCollidingLeft;}}
	public bool isGrabingEdge { get; set; }
	public bool isWallSticking { get{ return isCollidingSide && !isGrabingEdge;} }	
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
	public bool isCollidingSide { get {return isCollidingRight || isCollidingLeft;}}
	public bool isGrabingEdge { get{ return innerState.isGrabingEdge;} }
	public bool isWallSticking { get{ return isCollidingSide && !isGrabingEdge;} }

	public float yRightColDetDelta = 0.02f;

	public float groundedCastDistance = 0.01f;
	public float rcCastDistance = 0.01f;
	public int groundedRayCasts = 8;
	public int rightCollisionRayCasts = 16;
	public LayerMask PlatformMask;
	public float xGroundedOffset = 0.02f;

	public InnerState deepCopy()
	{
		InnerState state = new InnerState();

		state.isGrounded = isGrounded;
		state.isCollidingAbove = isCollidingAbove;
		state.isCollidingRight = isCollidingRight;
		state.isCollidingLeft = isCollidingLeft;
		state.isGrabingEdge = isGrabingEdge;

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
		//transform = GetComponent<Transform>();
		innerState.isGrounded = false;
		innerState.isCollidingRight = false;
		innerState.isCollidingLeft = false;
		innerState.isGrabingEdge = false;
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

	public float boundsXOffset = 0.02f;

	public bool isThisColliding(Vector2 rayDirection, ref float distance)
	{
		distance += boundsXOffset;
		
		Vector2 rayDirectionNorm = rayDirection.normalized;
		Vector2 topLeft = new Vector2(myCollider.bounds.min.x + boundsXOffset, myCollider.bounds.max.y);
		Vector2 topRight = new Vector2(myCollider.bounds.max.x - boundsXOffset, myCollider.bounds.max.y);
		Vector2 bottomRight = new Vector2(myCollider.bounds.max.x - boundsXOffset, myCollider.bounds.min.y);
		Vector2 bottomLeft = new Vector2(myCollider.bounds.min.x + boundsXOffset, myCollider.bounds.min.y);

		Physics2D.queriesHitTriggers = false;
		var raycastHitTL = Physics2D.Raycast(topLeft, rayDirectionNorm, distance, PlatformMask);
		var raycastHitTR = Physics2D.Raycast(topRight, rayDirectionNorm, distance, PlatformMask);
		var raycastHitBR = Physics2D.Raycast(bottomRight, rayDirectionNorm, distance, PlatformMask);
		var raycastHitBL = Physics2D.Raycast(bottomLeft, rayDirectionNorm, distance, PlatformMask);
		var raycastHitC = Physics2D.Raycast(myCollider.bounds.center, rayDirectionNorm, distance, PlatformMask);
		Physics2D.queriesHitTriggers = true;

		var effector2DTL = raycastHitTL.collider ? raycastHitTL.collider.GetComponent<PlatformEffector2D>() : null;
		var effector2DTR = raycastHitTR.collider ? raycastHitTR.collider.GetComponent<PlatformEffector2D>() : null;
		var effector2DBR = raycastHitBR.collider ? raycastHitBR.collider.GetComponent<PlatformEffector2D>() : null;
		var effector2DBL = raycastHitBL.collider ? raycastHitBL.collider.GetComponent<PlatformEffector2D>() : null;
		var effector2DC = raycastHitC.collider ? raycastHitC.collider.GetComponent<PlatformEffector2D>() : null;

		Debug.DrawRay(topLeft, rayDirectionNorm * distance, Color.green);
		Debug.DrawRay(topRight, rayDirectionNorm * distance, Color.green);
		Debug.DrawRay(bottomRight, rayDirectionNorm * distance, Color.green);
		Debug.DrawRay(bottomLeft, rayDirectionNorm * distance, Color.green);
		Debug.DrawRay(myCollider.bounds.center, rayDirectionNorm * distance, Color.green);

		if(raycastHitTL && raycastHitTL.collider.isTrigger == false && rayDirection.y > 0 && (effector2DTL == null || effector2DTL.useOneWay == false))
		{
			distance = Mathf.Min(distance, Vector2.Distance(topLeft, raycastHitTL.point));
		}

		if(raycastHitTR && raycastHitTR.collider.isTrigger == false && rayDirection.y > 0 && (effector2DTR == null || effector2DTR.useOneWay == false))
		{
			distance = Mathf.Min(distance, Vector2.Distance(topRight, raycastHitTR.point));
		}

		if(raycastHitBL && raycastHitBL.collider.isTrigger == false && rayDirection.y < 0 && (effector2DBL == null || effector2DBL.useOneWay == false))
		{
			distance = Mathf.Min(distance, Vector2.Distance(bottomLeft, raycastHitBL.point));
		}

		if(raycastHitBR && raycastHitBR.collider.isTrigger == false && rayDirection.y < 0 && (effector2DBR == null || effector2DBR.useOneWay == false))
		{
			distance = Mathf.Min(distance, Vector2.Distance(bottomRight, raycastHitBR.point));
		}

		if(raycastHitC && raycastHitC.collider.isTrigger == false && (effector2DC == null || effector2DC.useOneWay == false))
		{
			distance = Mathf.Min(distance, Vector2.Distance(myCollider.bounds.center, raycastHitC.point));
		}
		
		
		return raycastHitTL || raycastHitTR || raycastHitBL || raycastHitBR || raycastHitC;
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
		Collider2D myCollider = GetComponent<Collider2D>();
		float step = (float)(myCollider.bounds.size.x - (2 * xGroundedOffset)) / (float)groundedRayCasts;
		innerState.isGrounded = false;

		Vector2 rayDirection = Vector2.down;

		for(int i = 0; i <= groundedRayCasts; ++i)
		{
			Vector2 rayVector = new Vector2(myCollider.bounds.min.x + xGroundedOffset + i * step, myCollider.bounds.min.y);
			var raycastHit = Physics2D.Raycast(rayVector, rayDirection, groundedCastDistance, PlatformMask);
			Debug.DrawRay(rayVector, rayDirection * groundedCastDistance, Color.green);
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
		Collider2D myCollider = GetComponent<Collider2D>();
		float step = (float)(myCollider.bounds.size.x - (2 * xGroundedOffset))/ (float)groundedRayCasts;
		innerState.isCollidingAbove = false;

		Vector2 rayDirection = Vector2.up;

		for(int i = 0; i <= groundedRayCasts; ++i)
		{
			Vector2 rayVector = new Vector2(myCollider.bounds.min.x + xGroundedOffset + i * step, myCollider.bounds.max.y);
			var raycastHit = Physics2D.Raycast(rayVector, rayDirection, groundedCastDistance, PlatformMask);
			Debug.DrawRay(rayVector, rayDirection * groundedCastDistance, Color.green);
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


	public bool updateLeftCollision()
	{
		Collider2D myCollider = GetComponent<Collider2D>();
		float step = (float)myCollider.bounds.size.y / (float)rightCollisionRayCasts;
		innerState.isCollidingLeft = false;

		Vector2 rayDirection = Vector2.left;
		float leftX = Mathf.Min(myCollider.bounds.max.x , myCollider.bounds.min.x);
		
		{
			Vector2 dir = rayDirection;
			dir.y = -0.5f;
			dir.x = -1.3f;
			Vector2 rayVector = new Vector2(leftX , myCollider.bounds.min.y + yRightColDetDelta);
			var raycastHit = Physics2D.Raycast(rayVector, dir, rcCastDistance, PlatformMask);
			Debug.DrawRay(rayVector, dir * rcCastDistance, Color.red);
			if(checkRaycast(raycastHit, myCollider))
			{
				innerState.isCollidingLeft = true;
			}
		}

		for(int i = 0; i < rightCollisionRayCasts; ++i)
		{
			Vector2 rayVector = new Vector2(leftX , myCollider.bounds.min.y + yRightColDetDelta + i * step);
			var raycastHit = Physics2D.Raycast(rayVector, rayDirection, rcCastDistance, PlatformMask);
			Debug.DrawRay(rayVector, rayDirection * rcCastDistance, Color.red);
			if(checkRaycast(raycastHit, myCollider))
			{
				innerState.isCollidingLeft = true;
			}
		}

		return isCollidingLeft;
	}

	public bool updateRightCollision()
	{
		Collider2D myCollider = GetComponent<Collider2D>();
		float step = (float)myCollider.bounds.size.y / (float)rightCollisionRayCasts;
		innerState.isCollidingRight = false;

		Vector2 rayDirection = Vector2.right;
		float rightX = Mathf.Max(myCollider.bounds.max.x , myCollider.bounds.min.x);

		{
			Vector2 dir = rayDirection;
			dir.y = -0.5f;
			dir.x = 1.3f;
			Vector2 rayVector = new Vector2(rightX , myCollider.bounds.min.y + yRightColDetDelta);
			var raycastHit = Physics2D.Raycast(rayVector, dir, rcCastDistance, PlatformMask);
			Debug.DrawRay(rayVector, dir * rcCastDistance, Color.black);
			if(checkRaycast(raycastHit, myCollider))
			{
				innerState.isCollidingRight = true;
			}
		}

		for(int i = 0; i < rightCollisionRayCasts; ++i)
		{
			Vector2 rayVector = new Vector2(rightX , myCollider.bounds.min.y + yRightColDetDelta + i * step);
			var raycastHit = Physics2D.Raycast(rayVector, rayDirection, rcCastDistance, PlatformMask);
			Debug.DrawRay(rayVector, rayDirection * rcCastDistance, Color.black);
			if(checkRaycast(raycastHit, myCollider))
			{
				innerState.isCollidingRight = true;
			}
		}

		return isCollidingRight;
	}
}