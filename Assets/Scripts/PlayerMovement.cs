using UnityEngine;
using System.Collections;

public class PlayerMovement: MonoBehaviour {
	[Tooltip("Is this character currently controllable by the player"), SerializeField]
	public bool isHandlingInput = true;

	[Tooltip("The speed the player will move"), SerializeField]
	private float speed = 10;
	private Vector3 movement;
	[Tooltip("The amount of upwards force to apply to the character when they jump"), SerializeField]
	private float jumpVelocity = 20;
	[Tooltip("If the player lets go of the jump button, their y velocity will be reduced to this number."), SerializeField]
	private float jumpReduction = 10;
	[Tooltip("The maximum velocity the character should be able to reach"), SerializeField]
	private Vector3 maxVelocity;

	private bool hasJumped = false;
	private bool cutJumpShort = false;
	private bool isGrounded;

	private Rigidbody rigidbody;
	[Tooltip("The character will consider anything in this LayerMask to be 'Ground'"), SerializeField]
	private LayerMask groundLayerMask;

	void Awake () {
		rigidbody = GetComponent<Rigidbody> ();
	}

	void FixedUpdate () {
		ApplyJumpPhysics ();
		CapVelocity ();
	}
		
	void Update () {
		if (movement != Vector3.zero) {
			rigidbody.transform.rotation = Quaternion.LookRotation (movement);
		}

		if (Physics.CheckSphere (transform.position, 1f, groundLayerMask)) {
			isGrounded = true;
		} else {
			isGrounded = false;
		}

		float horizontalInput = Input.GetAxis ("Horizontal");
		float verticalInput = Input.GetAxis ("Vertical");
		ManageMovement (horizontalInput, verticalInput);

		if (Input.GetButtonDown ("Jump") && isGrounded) {
			Jump ();
		}

		if (Input.GetButtonUp ("Jump") && !isGrounded) {
			CutJumpShort ();
		}
	}

	public void ManageMovement (float h, float v) {
		if (!isHandlingInput) {
			return;
		}

		Vector3 forwardMove = Vector3.Cross (Camera.main.transform.right, Vector3.up);
		Vector3 horizontalMove = Camera.main.transform.right;

		movement = forwardMove * v + horizontalMove * h;

		movement = movement.normalized * speed * Time.deltaTime;
		rigidbody.MovePosition (transform.position + movement);
	}

	public void Jump () {
		hasJumped = true;
	}

	public void CutJumpShort () {
		cutJumpShort = true;
	}

	private void ApplyJumpPhysics () {
		if (hasJumped) {
			rigidbody.velocity = new Vector3 (rigidbody.velocity.x, jumpVelocity, rigidbody.velocity.z);
			hasJumped = false;
		}

		if (cutJumpShort) {
			if (rigidbody.velocity.y > jumpReduction) {
				rigidbody.velocity = new Vector3 (rigidbody.velocity.x, jumpReduction, rigidbody.velocity.z);
			}
			cutJumpShort = false;
		}
	}

	void CapVelocity () {
		Vector3 _velocity = GetComponent<Rigidbody> ().velocity;
		_velocity.x = Mathf.Clamp (_velocity.x, -maxVelocity.x, maxVelocity.x);
		_velocity.y = Mathf.Clamp (_velocity.y, -maxVelocity.y, maxVelocity.y);
		_velocity.z = Mathf.Clamp (_velocity.z, -maxVelocity.z, maxVelocity.z);
		rigidbody.velocity = _velocity;
	}
}