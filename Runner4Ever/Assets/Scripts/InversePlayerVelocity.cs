using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InversePlayerVelocity : MonoBehaviour {

	public float speedBonus = 2.0f;
	public float maxXVelocity = 20.0f;
	public float maxYVelocity = 20.0f;
    public float timeBeforeRetakeCorrectDirection = 0.6f;

   // private float disableColliderTime = 0.5f;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	bool IsIntersecting(Vector2 a, Vector2 b, Vector2 c, Vector2 d)
    {
        float denominator = ((b.x - a.x) * (d.y - c.y)) - ((b.y - a.y) * (d.x - c.x));
        float numerator1 = ((a.y - c.y) * (d.x - c.x)) - ((a.x - c.x) * (d.y - c.y));
        float numerator2 = ((a.y - c.y) * (b.x - a.x)) - ((a.x - c.x) * (b.y - a.y));

        // Detect coincident lines (has a problem, read below)
        if (denominator == 0) return numerator1 == 0 && numerator2 == 0;

        float r = numerator1 / denominator;
        float s = numerator2 / denominator;

        return (r >= 0 && r <= 1) && (s >= 0 && s <= 1);
    }

	void OnCollisionEnterCustom(RaycastCollision other) 
	{
        var state = other.other.GetComponent<CharacterController2D>();
        if(state != null && other.other.gameObject.tag == "Player")
        {
        	var collider = GetComponent<Collider2D>();

            UnityUtils.CollisionDirection direction = UnityUtils.getCollisionDirection(collider.bounds, other.point);

        	if(direction == UnityUtils.CollisionDirection.Above) 
        	{
        		state.inverseYVelocity(speedBonus, maxYVelocity);
        	}
        	else if(direction == UnityUtils.CollisionDirection.Below) 
        	{
        		state.inverseYVelocity(speedBonus, maxYVelocity);
        	}
        	else if(direction == UnityUtils.CollisionDirection.Left)
        	{
        		state.inverseXVelocity(speedBonus, maxXVelocity);
        	}
        	else if(direction == UnityUtils.CollisionDirection.Right) 
        	{
        		state.inverseXVelocity(speedBonus, maxXVelocity);
        	}
        	else
        	{
        		Debug.Log("Weird, collision from nowhere Point : " + other.point + " min : " + collider.bounds.min +  " max : " + collider.bounds.max);
        	}

            state.doNotRunRight();
            state.popGroundRunDirectionIn(timeBeforeRetakeCorrectDirection);
        }
    }

    private void reactivateCollider()
    {
        GetComponent<BoxCollider2D>().size = new Vector2(1.0f, 1.0f);
    }

    public void disableColliderFor(float time)
    {
         GetComponent<BoxCollider2D>().size = new Vector2(0.0f, 0.0f);
         Invoke("reactivateCollider", time);
    }
}
