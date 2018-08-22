using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

public class Player : MonoBehaviour
{
	[SerializeField] float climbSpeed = 3f;
	[SerializeField] float jumpSpeed = 16f;
	[SerializeField] float runSpeed = 4f;

	float startingGravity;
	float startingRunSpeed;
	bool isAlive = true;

	Animator animator;
	Collider2D myCollider2D;
	Rigidbody2D myRigidbody;

	void Start ()
	{
		animator = GetComponent<Animator>();
		myCollider2D = GetComponent<Collider2D>();
		myRigidbody = GetComponent<Rigidbody2D>();

		startingGravity = myRigidbody.gravityScale;
		startingRunSpeed = runSpeed;
	}
	
	void Update ()
	{
		FlipSprite();
		Climb();
		Run();
		Jump();
	}

	void FlipSprite()
	{
		bool playerHasHorizontalSpeed = Mathf.Abs( myRigidbody.velocity.x ) > Mathf.Epsilon;
		if ( playerHasHorizontalSpeed )
		{
			transform.localScale = new Vector2( Mathf.Sign( myRigidbody.velocity.x ), 1f );
		}
	}

	void Run()
	{
		float controlThrow = CrossPlatformInputManager.GetAxis( "Horizontal" );
		Vector2 runVelocity = new Vector2( controlThrow * runSpeed, myRigidbody.velocity.y );
		myRigidbody.velocity = runVelocity;

		bool playerHasHorizontalSpeed = Mathf.Abs( myRigidbody.velocity.x ) > Mathf.Epsilon;
		animator.SetBool( "Running", playerHasHorizontalSpeed );
	}

	void Climb()
	{
		if ( !myCollider2D.IsTouchingLayers( LayerMask.GetMask( "Climbing" ) ) )
		{
			myRigidbody.gravityScale = startingGravity;
			runSpeed = startingRunSpeed;

			animator.SetBool( "ClimbingIdle", false );
			animator.SetBool( "Climbing", false );
			return;
		}

		animator.SetBool( "ClimbingIdle", true );

		float controlThrow = CrossPlatformInputManager.GetAxis( "Vertical" );
		Vector2 climbVelocity = new Vector2( myRigidbody.velocity.x, controlThrow * climbSpeed );
		myRigidbody.velocity = climbVelocity;

		bool horizSpeed = Mathf.Abs( myRigidbody.velocity.x ) > Mathf.Epsilon;
		bool playerHasVerticalSpeed = Mathf.Abs( myRigidbody.velocity.y ) > Mathf.Epsilon;
		if ( playerHasVerticalSpeed || horizSpeed )
		{
			runSpeed = 2f;
			animator.SetBool( "ClimbingIdle", false );
			animator.SetBool( "Climbing", true );
			myRigidbody.gravityScale = 0;
		}
		else
		{
			runSpeed = 2f;
			animator.SetBool( "ClimbingIdle", true );
			animator.SetBool( "Climbing", false );
			myRigidbody.gravityScale = 0;
		}
	}

	void Jump()
	{
		// TODO Fix falling animation when next to a wall
		// TODO Wall riding animation
		// If falling, set animation
		if ( myRigidbody.velocity.y < 0 )
		{
			animator.SetBool( "Landing", true );
		}
		else
		{
			animator.SetBool( "Landing", false );
		}

		// If not touching the Ground layer, exit
		// Otherwise set landing animation bool
		if ( !myCollider2D.IsTouchingLayers( LayerMask.GetMask( "Ground" ) ) ) { return; }

		animator.ResetTrigger( "Jumping" );

		if ( CrossPlatformInputManager.GetButtonDown( "Jump" ) )
		{
			Vector2 jumpVelocityToAdd = new Vector2( 0, jumpSpeed );
			myRigidbody.velocity += jumpVelocityToAdd;
			animator.SetTrigger( "Jumping" );
		}
	}

}
