using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(Controller2D))]
public class Player2D : MonoBehaviour
{
	[SerializeField] float accelerationTimeAirborne = .2f;
	[SerializeField] float accelerationTimeGrounded = .1f;
	[SerializeField] float dashButtonLeeway = .5f;
	[SerializeField] float dashSpeed = 15f;
	[SerializeField] float maxJumpHeight = 3.5f;
	[SerializeField] float minJumpHeight = 1f;
	[SerializeField] float timeToJumpApex = .4f;
	[SerializeField] float moveSpeed = 5f;
	[SerializeField] float wallSlideSpeedMax = 3f;
	[SerializeField] Vector2 wallJumpClimb;
	[SerializeField] Vector2 wallJumpOff;
	[SerializeField] Vector2 wallJumpAcross;
	[SerializeField] float wallStickTime;
	[SerializeField] float wallUnstickTime;

	bool facingRight, wallSliding;
	float gravity, maxJumpVelocity, minJumpVelocity, lastDashAttempt, velocityXSmoothing;
	int dashButtonPressCount, wallDirX;

	Animator animator;
	Controller2D controller2D;
	Vector2 directionalInput;
	Vector3 velocity;

	public void SetDirectionalInput( Vector2 input )
	{
		directionalInput = input;
	}

	public void OnJumpInputDown()
	{
		if ( wallSliding )
		{
			if ( wallDirX == directionalInput.x )
			{
				velocity.x = -wallDirX * wallJumpClimb.x;
				velocity.y = wallJumpClimb.y;
			}
			else if ( directionalInput.x == 0 )
			{
				velocity.x = -wallDirX * wallJumpOff.x;
				velocity.y = wallJumpOff.y;
			}
			else
			{
				velocity.x = -wallDirX * wallJumpAcross.x;
				velocity.y = wallJumpAcross.y;
			}
		}
		if ( controller2D.collisionInfo.below )
		{

		}
		animator.SetTrigger( "Jumping" );
		velocity.y = maxJumpVelocity;
	}

	public void OnJumpInputUp()
	{
		if ( velocity.y > minJumpVelocity )
		{
			velocity.y = minJumpVelocity;
		}
	}

	public void OnDashInputDown()
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

	void Start ()
	{
		animator = GetComponent<Animator>();
		controller2D = GetComponent<Controller2D>();

		gravity = -( 2 * maxJumpHeight ) / Mathf.Pow( timeToJumpApex, 2 );
		maxJumpVelocity = Mathf.Abs( gravity ) * timeToJumpApex;
		minJumpVelocity = Mathf.Sqrt( 2 * Mathf.Abs( gravity ) * minJumpHeight );
	}
	
	void Update ()
	{
		if ( velocity.y < 0 )
		{
			animator.SetBool( "Landing", true );
		}
		else
		{
			animator.SetBool( "Landing", false );
		}

		CalculateVelocity();
		HandleWallSliding();

		animator.SetBool( "Running", ( Mathf.Abs( directionalInput.x ) > 0 ) );
		facingRight = Mathf.Sign( velocity.x ) == 1;
		transform.localScale = new Vector2( ( facingRight ) ? 1 : -1, transform.localScale.y );

		if ( ( Time.time - lastDashAttempt ) > dashButtonLeeway )
		{
			EndDash();
		}

		controller2D.Move( velocity * Time.deltaTime, directionalInput );

		if ( controller2D.collisionInfo.above || controller2D.collisionInfo.below )
		{
			velocity.y = 0;
		}
	}

	void HandleWallSliding()
	{
		wallDirX = ( controller2D.collisionInfo.left ) ? -1 : 1;
		wallSliding = false;

		if ( ( controller2D.collisionInfo.left || controller2D.collisionInfo.right ) && !controller2D.collisionInfo.below && velocity.y < 0 )
		{
			wallSliding = true;

			if ( velocity.y < -wallSlideSpeedMax )
			{
				velocity.y = -wallSlideSpeedMax;
			}

			if ( wallUnstickTime > 0 )
			{
				velocity.x = 0;
				velocityXSmoothing = 0;
				if ( directionalInput.x != wallDirX && directionalInput.x != 0 )
				{
					wallUnstickTime -= Time.deltaTime;
				}
				else
				{
					wallUnstickTime = wallStickTime;
				}
			}
			else
			{
				wallUnstickTime = wallStickTime;
			}
		}
	}

	void CalculateVelocity()
	{
		float tartgetVelocityX = directionalInput.x * moveSpeed;
		velocity.x = Mathf.SmoothDamp( velocity.x, tartgetVelocityX, ref velocityXSmoothing, ( controller2D.collisionInfo.below ) ? accelerationTimeGrounded : accelerationTimeAirborne );
		velocity.y += gravity * Time.deltaTime;
	}

	void EndDash()
	{
		//isDashing = false;
		animator.SetBool( "Dashing", false );
		dashButtonPressCount = 0;
	}

}
