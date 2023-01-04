/*
	I was originally going to use a pre-made First-Person Controller, but since I managed to make a lot of progress, I decided to create my own movement system which allowed me to have more control over the gameplay.
	The reason I used Character Controller instead of Rigidbody is to have more fine-tuned control over the movement, even though it is less realistic. In the end I think it feels nice and doesn't run poorly.
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class Player : MonoBehaviour
{
	[SerializeField]
	private byte Sensitivity, WalkSpeed, RunSpeed, CrouchSpeed, FallSpeed, JumpStrength;

	[SerializeField]
	private bool StairSnap, CameraInterpolation, IsCrouched;

	[SerializeField]
	private float Acceleration, CameraInterpolationStrength, StandHeight, CrouchHeight;

	[SerializeField]
	private LayerMask Mask;


	private CharacterController CC;


	private Vector3 CurrentSpeed, CurrentVelocity, PreviousPosition;
	private float CameraPosition, CameraVelocity, RotationLimit, Gravity;

	private bool LastGrounded;

	private byte jumpthing;

	

	private void Start()
	{
		CC = GetComponent<CharacterController>();
		Cursor.lockState = CursorLockMode.Locked;

		CameraPosition = Camera.main.transform.position.y;
		CC.Move(Vector2.down * 9999);
	}

	private void Update()
	{
		//This Ensures That More Velocity Will Only Be In A Certain Direction, This Means That If You Look Behind You Whilst Moving, You Won't Continue Moving Forward But Rather Backwards As You Will Still Be Slowing Down
		Vector3 Direction = transform.TransformDirection(Vector3.forward) * Input.GetAxisRaw("Vertical") + transform.TransformDirection(Vector3.right) * Input.GetAxisRaw("Horizontal");
		
		//Movement Is Normalized To Ensure That Diagonal Movement Will Stay The Same
		Direction.Normalize();

		//Smooth Damp Will Ensure That The Acceleration Will Be Constant Across Different Framerates
		if (IsCrouched)
			CurrentSpeed = Vector3.SmoothDamp(CurrentSpeed, Direction * CrouchSpeed, ref CurrentVelocity, Acceleration, Mathf.Infinity, Time.unscaledDeltaTime);
		else if (Input.GetKey(KeyCode.LeftShift) && LastGrounded)
			CurrentSpeed = Vector3.SmoothDamp(CurrentSpeed, Direction * RunSpeed, ref CurrentVelocity, Acceleration, Mathf.Infinity, Time.unscaledDeltaTime);
		else CurrentSpeed = Vector3.SmoothDamp(CurrentSpeed, Direction * WalkSpeed, ref CurrentVelocity, Acceleration, Mathf.Infinity, Time.unscaledDeltaTime);

		CC.Move(CurrentSpeed * Time.unscaledDeltaTime);
		//CurrentSpeed = new Vector3(CC.velocity.x, 0, CC.velocity.z);

		GravityCheck();
		CrouchCheck();

		//This Will Be Used For The Next Gravity Check, More Information Is Given In The Function
		PreviousPosition = transform.position;
	}



	private void GravityCheck()
	{
		Gravity = Mathf.MoveTowards(Gravity, FallSpeed * 2, FallSpeed * Time.deltaTime);
		CC.Move(Vector2.down * Gravity * Time.deltaTime);

		Gravity = CC.isGrounded || CC.collisionFlags == CollisionFlags.Above ? 0 : Gravity;
	

		if (LastGrounded && !CC.isGrounded)
		{
			//This Is To Ensure That The Player Will Go Down The Stairs Properly, Without Bouncing Awkwardly Down; Giving The Illusion Of Your Character Actually Going Down The Stairs
			//The For Loop Is Used To Ensure That Low Framerate Won't Affect The Stair Movement, This Can Be Tested With Very High Movement Speeds, And Will Hopefully Always Work

			bool DebugDetect = true;

			//Note, It Is Recommended That Camera Interpolation Is Checked To Avoid The Jittered Movement When You Go Down The Stairs
			if (StairSnap)
			{
				float InverseDistance = 1 / Vector3.Distance(PreviousPosition, transform.position);

				for (float T = 0; T < 1; T += InverseDistance)
				{
					Vector3 LerpedPosition = Vector3.Lerp(PreviousPosition, transform.position, T);

					if (Physics.Raycast(LerpedPosition, Vector2.down, CC.stepOffset * 2 + CC.skinWidth))
					{
						RaycastHit Information;
						//Get Another Raycast From The Current Position To Move The Character Accordingly Down The Stairs
						Physics.Raycast(transform.position, Vector2.down, out Information);
						CC.Move(Vector2.down * Information.distance);
						T = 1;

						DebugDetect = false;
					}
				}

				if (DebugDetect)
					print("Falling");
			}
		}

		if (CC.isGrounded && Input.GetKeyDown(KeyCode.Space) && !Physics.CapsuleCast(transform.position + Vector3.up * CC.stepOffset, transform.position + Vector3.up * CC.stepOffset, CC.radius, Vector3.up, StandHeight - CC.stepOffset + CC.skinWidth, Mask))
		{
			//This Will Ensure That Stair Snapping Will Not Work In The Next Frame
			LastGrounded = false;
			Gravity = -JumpStrength;
		}
		
		else LastGrounded = CC.isGrounded;
	}

	private void CrouchCheck()
	{
		if (LastGrounded)
		{
			if (Input.GetKey(KeyCode.LeftControl) ||
				Physics.CapsuleCast(transform.position + Vector3.up * CC.stepOffset, transform.position + Vector3.up * CC.stepOffset, CC.radius, Vector3.up, StandHeight - CC.stepOffset + CC.skinWidth, Mask))
			{
				CC.height = CrouchHeight;
				IsCrouched = true;
			}

			else
			{
				CC.height = StandHeight;
				IsCrouched = false;
			}
		}

		CC.center = Vector2.up * CC.height / 2;
	}

	private void LateUpdate()
	{
		transform.Rotate(Vector2.up * Input.GetAxis("Mouse X") * Sensitivity);

		RotationLimit += Input.GetAxis("Mouse Y") * Sensitivity;
		RotationLimit = Mathf.Clamp(RotationLimit, -60, 60);

		Camera.main.transform.localEulerAngles = Vector2.right * RotationLimit;

		//Smooth Camera Movement
		if (CameraInterpolation)
		{
			CameraPosition = Mathf.SmoothDamp(CameraPosition, transform.position.y + CC.height, ref CameraVelocity, CameraInterpolationStrength);
			Camera.main.transform.position = new Vector3(Camera.main.transform.position.x, CameraPosition, Camera.main.transform.position.z);
		}
	}

	public Vector3 GetVelocity()
	{
		return transform.InverseTransformDirection(CurrentSpeed);
	}

	public Vector3 GetCurrentSpeed()
	{
		return CurrentSpeed;
	}

	public float GetRunSpeed()
	{
		return RunSpeed;
	}

	public float GetCrouchSpeed()
	{
		return CrouchSpeed;
	}

	public bool IsGrounded()
	{
		return CC.isGrounded;
	}

	public void Kill()
	{

	}
}
