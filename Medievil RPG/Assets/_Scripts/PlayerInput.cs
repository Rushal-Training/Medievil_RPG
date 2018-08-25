using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(Controller2D))]
public class PlayerInput : MonoBehaviour
{
	[SerializeField] float accelerationTimeAirborne = .2f;
	[SerializeField] float accelerationTimeGrounded = .1f;
	[SerializeField] float dashButtonLeeway = .5f;
	[SerializeField] float dashSpeed = 15f;
	[SerializeField] float jumpHeight = 3.5f;
	[SerializeField] float timeToJumpApex = .4f;
	[SerializeField] float moveSpeed = 5f;

	float gravity, jumpVelocity, lastDashAttempt, velocityXSmoothing;
	int dashButtonPressCount;

	Animator animator;
	Controller2D controller2D;
	Vector3 velocity;

	void Start ()
	{
		animator = GetComponent<Animator>();
		controller2D = GetComponent<Controller2D>();

		gravity = -( 2 * jumpHeight ) / Mathf.Pow( timeToJumpApex, 2 );
		jumpVelocity = Mathf.Abs( gravity ) * timeToJumpApex;
	}
	
	void Update ()
	{
		if( controller2D.collisionInfo.above || controller2D.collisionInfo.below )
		{
			velocity.y = 0;
		}

		Vector2 input = new Vector2( Input.GetAxisRaw( "Horizontal" ), Input.GetAxisRaw( "Vertical" ) );

		animator.SetBool( "Running", ( Mathf.Abs( input.x ) > 0 ) );
		bool facingRight = Mathf.Sign( velocity.x ) == 1;
		transform.localScale = new Vector2( ( facingRight )?1:-1 , transform.localScale.y );

		if ( velocity.y < 0 )
		{
			animator.SetBool( "Landing", true );
		}
		else
		{
			animator.SetBool( "Landing", false );
		}

		if ( Input.GetKeyDown( KeyCode.Space ) && controller2D.collisionInfo.below )
		{
			animator.SetTrigger( "Jumping" );
			velocity.y = jumpVelocity;
		}

		if ( Input.GetKeyDown( KeyCode.LeftShift ) || Input.GetKeyDown( KeyCode.RightShift ) )
		{
			lastDashAttempt = Time.time;
			dashButtonPressCount++;

			if ( ( Time.time - lastDashAttempt ) < dashButtonLeeway )
			{
				if ( dashButtonPressCount == 1 )
				{
					//isDashing = true;
					animator.SetBool( "Dashing", true );

					if ( facingRight )
					{
						velocity = Vector2.right * dashSpeed;
					}
					else
					{
						velocity = Vector2.left * dashSpeed;
					}
				}
			}
		}

		if ( ( Time.time - lastDashAttempt ) > dashButtonLeeway )
		{
			EndDash();
		}

		float tartgetVelocityX = input.x * moveSpeed;
		velocity.x = Mathf.SmoothDamp( velocity.x, tartgetVelocityX, ref velocityXSmoothing, (controller2D.collisionInfo.below)?accelerationTimeGrounded:accelerationTimeAirborne );
		velocity.y += gravity * Time.deltaTime;

		controller2D.Move( velocity * Time.deltaTime );
	}

	void EndDash()
	{
		//isDashing = false;
		animator.SetBool( "Dashing", false );
		dashButtonPressCount = 0;
	}

}
