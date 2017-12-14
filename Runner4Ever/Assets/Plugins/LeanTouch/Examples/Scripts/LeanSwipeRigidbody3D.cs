using UnityEngine;

namespace Lean.Touch
{
	// This script will push a rigidbody around when you swipe
	[RequireComponent(typeof(Rigidbody))]
	public class LeanSwipeRigidbody3D : MonoBehaviour
	{
		// This stores the layers we want the raycast to hit (make sure this GameObject's layer is included!)
		public LayerMask LayerMask = UnityEngine.Physics.DefaultRaycastLayers;
		
		// This allows use to set how powerful the swipe will be
		public float ForceMultiplier = 1.0f;
		
		protected virtual void OnEnable()
		{
			// Hook into the events we need
			LeanTouch.OnFingerSwipe += OnFingerSwipe;
		}
		
		protected virtual void OnDisable()
		{
			// Unhook the events
			LeanTouch.OnFingerSwipe -= OnFingerSwipe;
		}
		
		public void OnFingerSwipe(LeanFinger finger)
		{
			// Raycast information
			var ray = finger.GetStartRay();
			var hit = default(RaycastHit);
			
			// Was this finger pressed down on a collider?
			if (Physics.Raycast(ray, out hit, float.PositiveInfinity, LayerMask) == true)
			{
				// Was that collider this one?
				if (hit.collider.gameObject == gameObject)
				{
					// Get the rigidbody component
					var rigidbody = GetComponent<Rigidbody>();
					
					// Add force to the rigidbody based on the swipe force
					rigidbody.AddForce(finger.SwipeScaledDelta * ForceMultiplier);
				}
			}
		}
	}
}