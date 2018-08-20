using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

public class Player : MonoBehaviour
{
	[SerializeField] float runSpeed = 3f;
	[SerializeField] float jumpSpeed = 3f;

	bool isAlive = true;

	Animator animator;
	Collider2D myCollider2D;
	Rigidbody2D myRigidbody;

	void Start ()
	{
		animator = GetComponent<Animator>();
		myCollider2D = GetComponent<Collider2D>();
		myRigidbody = GetComponent<Rigidbody2D>();
	}
	
	void FixedUpdate ()
	{
		Run();
		FlipSprite();
		Jump();
		print( myRigidbody.velocity.y );
	}

	void Run()
	{
		float controlThrow = CrossPlatformInputManager.GetAxis( "Horizontal" );
		Vector2 playerVelocity = new Vector2( controlThrow * runSpeed, myRigidbody.velocity.y );
		myRigidbody.velocity = playerVelocity;

		bool playerHasHorizontalSpeed = Mathf.Abs( myRigidbody.velocity.x ) > Mathf.Epsilon;
		animator.SetBool( "Running", playerHasHorizontalSpeed );
	}

	void Jump()
	{
		// TODO Fix falling animation when next to a wall
		// TODO Wall riding animation
		// If falling, set animation
		if ( myRigidbody.velocity.y < Mathf.Epsilon )
		{
			animator.SetBool( "Landing", true );
		}

		// If not touching the Ground layer, exit
		// Otherwise set landing animation bool
		if ( !myCollider2D.IsTouchingLayers( LayerMask.GetMask( "Ground" ))) { return; }

		animator.ResetTrigger( "Jumping" );
		animator.SetBool( "Landing", false );

		if ( CrossPlatformInputManager.GetButtonDown( "Jump" ) )
		{
			Vector2 jumpVelocityToAdd = new Vector2( 0, jumpSpeed );
			myRigidbody.velocity += jumpVelocityToAdd;
			animator.SetTrigger( "Jumping" );
		}
	}

	void FlipSprite()
	{
		bool playerHasHorizontalSpeed = Mathf.Abs( myRigidbody.velocity.x ) > Mathf.Epsilon;
		if ( playerHasHorizontalSpeed )
		{
			transform.localScale = new Vector2( Mathf.Sign( myRigidbody.velocity.x ), 1f);
		}
	}
}
