using UnityEngine;
using System.Collections;

public class CameraMovement : MonoBehaviour 
{
	// ----------------------------------------------- Data members ----------------------------------------------
	[Tooltip("What the camera will look at.")]
	public Transform target;			// What the camera will be looking at
	[Tooltip("How far the camera currently is from the target.")]
	public float distance = 10.0f;		// How far the camera is from the target
	[Tooltip("How fast the camera moves horizontally.")]
	public float xSpeed = 10.0f;		// Longtitudal speed of camera
	[Tooltip("How fast the camera moves vertically.")]
	public float ySpeed = 10.0f;		// Latitudal speed of camera

	// yMinLimit and yMaxLimit are used to clamp the y angle of the camera.
	[Tooltip("Minimum angle of the camera on the y axis.")]
	public float yMinLimit = 10f;
	[Tooltip("Maximum angle of the camera on the y axis.")]
	public float yMaxLimit = 80f;

	[Tooltip("Minimum angle of the camera on the x axis.")]
	public float xMinLimit = -360f;
	[Tooltip("Maximum angle of the camera on the x axis.")]
	public float xMaxLimit = 360f;

	[Tooltip("Minimum allowed distance between camera and target.")]
	public float distanceMin = 0.5f;			// Minimum distance camera can be from target
	[Tooltip("Maximum allowed distance between camera and target")]
	public float distanceMax = 10f;				// Maximum distance camera can be from target

	[Tooltip("Radius of the thin SphereCast, used to detect camera collisions.")]
	public float thinRadius = 0.15f;
	[Tooltip("Radius of the thick SphereCast, used to detect camera collisions.")]
	public float thickRadius = 0.3f;
	[Tooltip("LayerMask used for detecting camera collisions. Camera will not avoid objects if this is not set correctly.")]
	public LayerMask layerMask;

	private Quaternion rotation;		// Local reference to rotation
	private Vector3 position;			// Local reference to position
	float x = 0.0f;						// x angle of camera
	float y = 0.0f;						// y angle of camera
	// ----------------------------------------------- End Data members -------------------------------------------

	// --------------------------------------------------- Methods ------------------------------------------------
	// --------------------------------------------------------------------
	// Use this for initialization
	void Start () 
	{
		// Going to use a default Unity functionality for this.
		Vector3 angles = this.transform.eulerAngles;
		x = angles.y;
		y = angles.x;
	}
	// --------------------------------------------------------------------
	void LateUpdate () 
	{
		if (target)		// Does target exist? (Not Null)
		{
			CameraMove();							// Move the camera, using the mouse.
			rotation = Quaternion.Euler(y, x, 0);	// Returns a rotation that rotates z degrees around the z axis, x degrees around the x axis, and y degrees around the y axis (in that order).

			// Zoom out if we're not at maximum zoom distance.
			if(distance < distanceMax)
			{
				distance = Mathf.Lerp(distance, distanceMax, Time.deltaTime * 2f);
			}
			// We'll declare a new Vector3 storing -distance. This will be 0 if we can see Bip, so nothing will happen.
			// However, if we can't see Bip, position.z will be equal to distance * -1, flipping it.
			Vector3 distanceVector = new Vector3(0.0f, 0.0f, -distance);
			// Camera follows target
			Vector3 position = rotation * distanceVector + target.position;
			transform.rotation = rotation;
			transform.position = position;
			CameraCollision();
		}
	}
	// --------------------------------------------------------------------
	public void CameraMove()
	{
		// Offset the angles by the mouse, when the mouse is moved.
		x += Input.GetAxis("Mouse X") * xSpeed;
		y -= Input.GetAxis("Mouse Y") * ySpeed;

		x = ClampAngle(x, xMinLimit, xMaxLimit);
		y = ClampAngle(y, yMinLimit, yMaxLimit);
	}
	// --------------------------------------------------------------------
	public float ClampAngle(float angle, float min, float max)
	{
		// Ensure that angle is between -360 and 360, because it is a float and can be any size.
		// If you rotate the camera 360 degrees in either direction, angle will snap back to 0.
		if (angle < -360F)
		{
			angle += 360F;
		}

		if (angle > 360F)
		{
			angle -= 360F;
		}
		// Then call Mathf.Clamp to actually clamp the angle.
		return Mathf.Clamp(angle, min, max);
	}
	// -------------------------------------------------------------------
	void CameraCollision ()
	{
		Vector3 normal, thickNormal;						// Normal of the cast collisions.
		Vector3 ray = transform.position - target.position;	// Direction for the SphereCasts.

		// The position of the thin SphereCast collision.
		Vector3 CollisionPoint = GetDoubleSphereCastCollision (transform.position, thinRadius, out normal, true);
		// The position of the thick SphereCast collision.
		Vector3 CollisionPointThick = GetDoubleSphereCastCollision (transform.position, thickRadius, out thickNormal, false);
		// The position of the RayCast collision.
		Vector3 CollisionPointRay = GetRayCollisionPoint (transform.position);

		// Collision Position from thick SphereCast, projected onto the RayCast.
		Vector3 CollisionPointProjectedOnRay = Vector3.Project (CollisionPointThick - target.position, ray.normalized) + target.position;
		// Direction to push the camera.
		Vector3 VectorToProject = (CollisionPointProjectedOnRay - CollisionPointThick).normalized;
		// Thick Collision Position projected onto thin SphereCast.
		Vector3 CollisionPointThickProjectedOnThin = CollisionPointProjectedOnRay - VectorToProject * thinRadius;
		// Distance between thick sphere and thin sphere collisions. Used to calculate where to push Camera.
		float ThinToThickDistance = Vector3.Distance (CollisionPointThickProjectedOnThin, CollisionPointThick);
		float ThinToThickDistanceNormal = ThinToThickDistance / (thickRadius - thinRadius);

		// Distance between target and thin sphere collision.
		float CollisionDistanceThin = Vector3.Distance (target.position, CollisionPoint);
		// Distance between target and Thick sphere collision.
		float CollisionDistanceThick = Vector3.Distance (target.position, CollisionPointProjectedOnRay);
		// Smoothly interpolating between distance and new distance.
		float CollisionDistance = Mathf.Lerp (CollisionDistanceThick, CollisionDistanceThin, ThinToThickDistanceNormal);

		// Thick point can be actually projected IN FRONT of the character due to double projection to avoid sphere moving through the walls
		// In this case we should only use thin point
		bool isThickPointIncorrect = transform.InverseTransformDirection (CollisionPointThick - target.position).z > 0;
		isThickPointIncorrect = isThickPointIncorrect || (CollisionDistanceThin < CollisionDistanceThick);
		if (isThickPointIncorrect) 
		{
			CollisionDistance = CollisionDistanceThin;
		}

		// if CollisionDistance is smaller than distance, zoom in
		if (CollisionDistance < distance) 
		{
			distance = CollisionDistance;
		}
		else
		{
			// Otherwise, zoom out.
			distance = Mathf.SmoothStep (distance, CollisionDistance, Time.deltaTime * 100 * Mathf.Max (distance * 0.1f, 0.1f));
		}
		// Clamp distance to our min and max values.
		distance = Mathf.Clamp(distance, distanceMin, distanceMax);
		// Move the camera to avoid going through objects!!!!
		transform.position = target.position + ray.normalized * distance;

		if (Vector3.Distance(target.position, CollisionPoint) > Vector3.Distance(target.position, CollisionPointRay))
		{
			transform.position = CollisionPointRay;
		} 
	}
	// -------------------------------------------------------------------
	Vector3 GetDoubleSphereCastCollision(Vector3 cameraPosition, float radius, out Vector3 normal, bool pushAlongNormal)
	{
		// Double Sphere Casting.
		// Length of the cast.
		float rayLength = 1;

		RaycastHit hit;
		// Local reference to target.position
		Vector3 origin = target.position;
		// Cast direction.
		Vector3 ray = origin - cameraPosition;
		// Dot product of transform.forward, and the ray.
		float dot = Vector3.Dot(transform.forward, ray);
		if (dot < 0)
		{
			ray *= -1;
		}

		// Project the sphere in an opposite direction of the desired character->camera vector to get some space for the real spherecast
		if (Physics.SphereCast(origin, radius, ray.normalized, out hit, rayLength, layerMask))
		{
			origin = origin + ray.normalized * hit.distance;
		}
		else
		{
			origin += ray.normalized * rayLength;
		}

		// Do final spherecast with offset origin
		ray = origin - cameraPosition;
		if (Physics.SphereCast(origin, radius, -ray.normalized, out hit, ray.magnitude, layerMask))
		{
			normal = hit.normal;

			if(pushAlongNormal)
			{
				return hit.point + hit.normal*radius;
			}
			else
			{
				return hit.point;
			}
		}
		else
		{
			normal = Vector3.zero;
			return cameraPosition;
		}
	}
	// -------------------------------------------------------------------
	Vector3 GetRayCollisionPoint(Vector3 cameraPosition)
	{
		// Local reference to target.position
		Vector3 origin = target.position;
		// Direction for raycast
		Vector3 ray = cameraPosition - origin;

		RaycastHit hit;
		if (Physics.Raycast(origin, ray.normalized, out hit, ray.magnitude, layerMask))
		{
			// Return position of hit + the normal of that hit, multiplied by a smoothing variable.
			return hit.point + hit.normal * 0.15f;
		}
		// or we just return the camera position we passed in.
		return cameraPosition;
	}
	// -------------------------------------------------------------------
	// --------------------------------------------------- End Methods ---------------------------------------------
}