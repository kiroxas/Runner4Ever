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

	InnerState innerState;

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
		colliderHitThisFrame.Clear();
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

	public bool isThisColliding(Vector2 rayDirection, ref float distance)
	{
		Vector2 rayDirectionNorm = rayDirection.normalized;
		Vector2 topLeft = new Vector2(myCollider.bounds.min.x, myCollider.bounds.max.y);
		Vector2 bottomRight = new Vector2(myCollider.bounds.max.x, myCollider.bounds.min.y);

		var raycastHitTL = Physics2D.Raycast(topLeft, rayDirectionNorm, distance, PlatformMask);
		var raycastHitTR = Physics2D.Raycast(myCollider.bounds.max, rayDirectionNorm, distance, PlatformMask);
		var raycastHitBR = Physics2D.Raycast(bottomRight, rayDirectionNorm, distance, PlatformMask);
		var raycastHitBL = Physics2D.Raycast(myCollider.bounds.min, rayDirectionNorm, distance, PlatformMask);
		var raycastHitC = Physics2D.Raycast(myCollider.bounds.center, rayDirectionNorm, distance, PlatformMask);

		Debug.DrawRay(topLeft, rayDirectionNorm * distance, Color.green);
		//Debug.DrawLine(topLeft, topLeftArrival, Color.blue);
		Debug.DrawRay(myCollider.bounds.max, rayDirectionNorm * distance, Color.green);
		Debug.DrawRay(bottomRight, rayDirectionNorm * distance, Color.green);
		Debug.DrawRay(myCollider.bounds.min, rayDirectionNorm * distance, Color.green);
		Debug.DrawRay(myCollider.bounds.center, rayDirectionNorm * distance, Color.green);

		if(raycastHitTL && raycastHitTL.collider.isTrigger == false && rayDirection.y > 0)
		{
			distance = Mathf.Min(distance, Vector2.Distance(topLeft, raycastHitTL.point));
		}

		if(raycastHitTR && raycastHitTR.collider.isTrigger == false && rayDirection.y > 0)
		{
			distance = Mathf.Min(distance, Vector2.Distance(myCollider.bounds.max, raycastHitTR.point));
		}

		if(raycastHitBL && raycastHitBL.collider.isTrigger == false && rayDirection.y < 0)
		{
			distance = Mathf.Min(distance, Vector2.Distance(myCollider.bounds.min, raycastHitBL.point));
		}

		if(raycastHitBR && raycastHitBR.collider.isTrigger == false && rayDirection.y < 0)
		{
			distance = Mathf.Min(distance, Vector2.Distance(bottomRight, raycastHitBR.point));
		}

		if(raycastHitC && raycastHitC.collider.isTrigger == false)
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

	public bool updateGrounded()
	{
		Collider2D myCollider = GetComponent<Collider2D>();
		float step = (float)myCollider.bounds.size.x / (float)groundedRayCasts;
		innerState.isGrounded = false;

		Vector2 rayDirection = Vector2.down;

		/*{
			Vector2 dir = rayDirection;
			//dir.x = -0.0f;
			Vector2 rayVector = myCollider.bounds.min;
			var raycastHit = Physics2D.Raycast(rayVector, dir, rcCastDistance, PlatformMask);
			Debug.DrawRay(rayVector, dir * groundedCastDistance, Color.green);
			if (raycastHit)
			{
				var effector2D = raycastHit.collider.GetComponent<PlatformEffector2D>();
				if(effector2D != null)
				{
					if(effector2D.useOneWay && myCollider.bounds.min.y > raycastHit.collider.bounds.center.y ) // Only allowed if above
					{
						addCollider(raycastHit.collider, raycastHit.point);
						isGrounded = true;
					}
				}
				else
				{
					addCollider(raycastHit.collider, raycastHit.point);
					isGrounded = true;
				}
			}
		
			//dir.x = 0.0f;
			rayVector = new Vector2(myCollider.bounds.max.x , myCollider.bounds.min.y);
			raycastHit = Physics2D.Raycast(rayVector, dir, rcCastDistance, PlatformMask);
			Debug.DrawRay(rayVector, dir * groundedCastDistance, Color.green);
			if (raycastHit)
			{
				var effector2D = raycastHit.collider.GetComponent<PlatformEffector2D>();
				if(effector2D != null)
				{
					if(effector2D.useOneWay && myCollider.bounds.min.y > raycastHit.collider.bounds.center.y ) // Only allowed if above
					{
						addCollider(raycastHit.collider, raycastHit.point);
						isGrounded = true;
					}
				}
				else
				{
					addCollider(raycastHit.collider, raycastHit.point);
					isGrounded = true;
				}
			}
		}*/

		for(int i = 0; i <= groundedRayCasts; ++i)
		{
			Vector2 rayVector = new Vector2(myCollider.bounds.min.x + i * step, myCollider.bounds.min.y);
			var raycastHit = Physics2D.Raycast(rayVector, rayDirection, groundedCastDistance, PlatformMask);
			Debug.DrawRay(rayVector, rayDirection * groundedCastDistance, Color.green);
			if (raycastHit)
			{
				var effector2D = raycastHit.collider.GetComponent<PlatformEffector2D>();
				if(effector2D != null)
				{
					if(effector2D.useOneWay && myCollider.bounds.min.y > raycastHit.collider.bounds.center.y ) // Only allowed if above
					{
						addCollider(raycastHit.collider, raycastHit.point);
						if(raycastHit.collider.isTrigger == false)
						{
							innerState.isGrounded = true;
						}
					}
				}
				else
				{
					addCollider(raycastHit.collider, raycastHit.point);
					if(raycastHit.collider.isTrigger == false)
					{
						innerState.isGrounded = true;
					}
				}
			}
		}

		return isGrounded;
	}

	public bool updateAbove()
	{
		Collider2D myCollider = GetComponent<Collider2D>();
		float step = (float)myCollider.bounds.size.x / (float)groundedRayCasts;
		innerState.isCollidingAbove = false;

		Vector2 rayDirection = Vector2.up;

		{
			Vector2 dir = rayDirection;
			dir.x = -0.0f;
			Vector2 rayVector = new Vector2(myCollider.bounds.min.x , myCollider.bounds.max.y);
			var raycastHit = Physics2D.Raycast(rayVector, dir, rcCastDistance, PlatformMask);
			Debug.DrawRay(rayVector, dir * groundedCastDistance, Color.green);
			if (raycastHit)
			{
				var effector2D = raycastHit.collider.GetComponent<PlatformEffector2D>();
				if(effector2D != null)
				{
					if(effector2D.useOneWay && myCollider.bounds.min.y > raycastHit.collider.bounds.center.y ) // Only allowed if above
					{
						addCollider(raycastHit.collider, raycastHit.point);
						if(raycastHit.collider.isTrigger == false)
						{
							innerState.isCollidingAbove = true;
						}
					}
				}
				else
				{
					addCollider(raycastHit.collider, raycastHit.point);
					if(raycastHit.collider.isTrigger == false)
					{
						innerState.isCollidingAbove = true;
					}
				}
			}
		
			dir.x = 0.0f;
			rayVector = new Vector2(myCollider.bounds.max.x , myCollider.bounds.max.y);
			raycastHit = Physics2D.Raycast(rayVector, dir, rcCastDistance, PlatformMask);
			Debug.DrawRay(rayVector, dir * groundedCastDistance, Color.green);
			if (raycastHit)
			{
				var effector2D = raycastHit.collider.GetComponent<PlatformEffector2D>();
				if(effector2D != null)
				{
					if(effector2D.useOneWay && myCollider.bounds.min.y > raycastHit.collider.bounds.center.y ) // Only allowed if above
					{
						addCollider(raycastHit.collider, raycastHit.point);
						if(raycastHit.collider.isTrigger == false)
						{
							innerState.isCollidingAbove = true;
						}
					}
				}
				else
				{
					addCollider(raycastHit.collider, raycastHit.point);
					if(raycastHit.collider.isTrigger == false)
					{
						innerState.isCollidingAbove = true;
					}
				}
			}
		}

		for(int i = -1; i <= groundedRayCasts; ++i)
		{
			Vector2 rayVector = new Vector2(myCollider.bounds.min.x + i * step, myCollider.bounds.max.y);
			var raycastHit = Physics2D.Raycast(rayVector, rayDirection, groundedCastDistance, PlatformMask);
			Debug.DrawRay(rayVector, rayDirection * groundedCastDistance, Color.green);
			if (raycastHit)
			{
				var effector2D = raycastHit.collider.GetComponent<PlatformEffector2D>();
				if(effector2D != null)
				{
					if(effector2D.useOneWay && myCollider.bounds.min.y > raycastHit.collider.bounds.center.y ) // Only allowed if above
					{
						addCollider(raycastHit.collider, raycastHit.point);
						if(raycastHit.collider.isTrigger == false)
						{
							innerState.isCollidingAbove = true;
						}
					}
				}
				else
				{
					addCollider(raycastHit.collider, raycastHit.point);
					if(raycastHit.collider.isTrigger == false)
					{
						innerState.isCollidingAbove = true;
					}
				}
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
			if (raycastHit)
			{
				addCollider(raycastHit.collider, raycastHit.point);
				if(raycastHit.collider.isTrigger == false)
				{
					innerState.isCollidingLeft = true;
				}
			}
		}

		for(int i = 0; i < rightCollisionRayCasts; ++i)
		{
			Vector2 rayVector = new Vector2(leftX , myCollider.bounds.min.y + yRightColDetDelta + i * step);
			var raycastHit = Physics2D.Raycast(rayVector, rayDirection, rcCastDistance, PlatformMask);
			Debug.DrawRay(rayVector, rayDirection * rcCastDistance, Color.red);
			if (raycastHit)
			{
				if(raycastHit.collider.isTrigger == false)
				{
					innerState.isCollidingLeft = true;
				}
				addCollider(raycastHit.collider, raycastHit.point);
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
			if (raycastHit)
			{
				if(raycastHit.collider.isTrigger == false)
				{
					innerState.isCollidingRight = true;
				}
				addCollider(raycastHit.collider, raycastHit.point);
			}
		}

		for(int i = 0; i < rightCollisionRayCasts; ++i)
		{
			Vector2 rayVector = new Vector2(rightX , myCollider.bounds.min.y + yRightColDetDelta + i * step);
			var raycastHit = Physics2D.Raycast(rayVector, rayDirection, rcCastDistance, PlatformMask);
			Debug.DrawRay(rayVector, rayDirection * rcCastDistance, Color.black);
			if (raycastHit)
			{
				if(raycastHit.collider.isTrigger == false)
				{
					innerState.isCollidingRight = true;
				}
				addCollider(raycastHit.collider, raycastHit.point);
			}
		}

		return isCollidingRight;
	}
}