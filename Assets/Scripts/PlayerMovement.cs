using UnityEngine;
using System.Collections;

public class PlayerMovement: MonoBehaviour
{
	// ----------------------------------------------- Data members ----------------------------------------------
	public float speed = 10;
	private Vector3 movement;
	public float jumpVelocity = 20;			// For jump height.
	public float jumpReduction = 10;		// The degree to which variable jump is variable.
	public Vector3 maxVelocityCap;			// To cap velocity.

	private bool hasJumped = false;				// To check if player has pressed jump.
	private bool cutJumpShort = false;			// If true, player has stopped holding button.
	public bool isHandlingInput = true;

	public Rigidbody rigidbody;
	public LayerMask layerMask;

	private bool isGrounded;
	// ----------------------------------------------- End Data members ------------------------------------------

	// --------------------------------------------------- Methods -----------------------------------------------
	// --------------------------------------------------------------------
	void Awake()
	{
		// Set up references.
		rigidbody = GetComponent<Rigidbody>();
	}
	// --------------------------------------------------------------------
	// Put physics stuff in here.
	void FixedUpdate()
	{
		ApplyJumpPhysics();
		CapVelocity();
	}
	// --------------------------------------------------------------------
	// Called once every frame.
	void Update()
	{
		if (movement != Vector3.zero) 
		{
			// If the object is moving, rotate it to face the direction it is moving.
			rigidbody.transform.rotation = Quaternion.LookRotation (movement);
		}

		// Create a sphere at the player's feet. If the sphere collides with anything on the layer(s) layerMask
		// return true
		if (Physics.CheckSphere(transform.position, 1f, layerMask))
		{
			// If sphere collides, we're touching ground
			isGrounded = true;
		}
		else
		{
			// otherwise, we're in the air
			isGrounded = false;
		}
		// Input handling.
		float horizontalInput = Input.GetAxis("Horizontal");
		float verticalInput = Input.GetAxis("Vertical");
		ManageMovement (horizontalInput, verticalInput);

		if (Input.GetButtonDown ("Jump") && isGrounded) 
		{
			// If the player is grounded and they press Jump, make them jump!
			Jump();
		}

		// Cut jump short for variable height.
		if (Input.GetButtonUp ("Jump") && !isGrounded) 
		{
			CutJumpShort();
		}
	}
	// --------------------------------------------------------------------
	// To handle movement, incorporate pulling of objects later.
	public void ManageMovement(float h, float v)
	{
		if(!isHandlingInput)
		{
			// If we don't want to handle Input for any reason, return.
			return;
		}
		// Find the new forward and right vectors to move along.
		Vector3 forwardMove = Vector3.Cross(Camera.main.transform.right, Vector3.up);
		Vector3 horizontalMove = Camera.main.transform.right;

		// Multiply the direction vectors by the Input.GetAxis floats.
		movement = forwardMove * v + horizontalMove * h;

		// Normalise the movement vector and make it proportional to the speed per second.
		movement = movement.normalized * speed * Time.deltaTime;
		// Move the player to it's current position plus this movement.
		rigidbody.MovePosition(transform.position + movement);
	}
	// --------------------------------------------------------------------
	// To make jump.
	public void Jump() 
	{
		hasJumped = true;
	}
	// --------------------------------------------------------------------
	// To make jump variable.
	public void CutJumpShort() 
	{
		cutJumpShort = true;
	}
	// --------------------------------------------------------------------
	private void ApplyJumpPhysics()
	{
		if (hasJumped)
		{
			rigidbody.velocity = new Vector3(rigidbody.velocity.x, jumpVelocity, rigidbody.velocity.z);
			hasJumped = false;
		}

		// Cancel the jump when the button is no longer pressed
		if (cutJumpShort)
		{
			if (rigidbody.velocity.y > jumpReduction)
			{
				rigidbody.velocity = new Vector3(rigidbody.velocity.x, jumpReduction, rigidbody.velocity.z);
			}
			cutJumpShort = false;
		}
	}
	// --------------------------------------------------------------------
	// To cap velocity so  doesn't fall too fast.
	void CapVelocity() 
	{
		Vector3 _velocity = GetComponent<Rigidbody>().velocity;
		_velocity.x = Mathf.Clamp (_velocity.x, -maxVelocityCap.x, maxVelocityCap.x);
		_velocity.y = Mathf.Clamp (_velocity.y, -maxVelocityCap.y, maxVelocityCap.y);
		_velocity.z = Mathf.Clamp (_velocity.z, -maxVelocityCap.z, maxVelocityCap.z);
		rigidbody.velocity = _velocity;
	}
	// --------------------------------------------------- End Methods --------------------------------------------
}