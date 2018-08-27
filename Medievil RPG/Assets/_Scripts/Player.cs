using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

public class Player : MonoBehaviour
{
	[SerializeField] float attackButtonLeeway = .5f;
	[SerializeField] float climbSpeed = 3f;
	[SerializeField] float dashButtonLeeway = .5f;
	[SerializeField] float dashSpeed = 15f;
	[SerializeField] float jumpSpeed = 16f;
	[SerializeField] float runSpeed = 4f;

	[SerializeField] Vector2 wallJumpClimb;
	[SerializeField] Vector2 wallJumpOff;
	[SerializeField] Vector2 wallJumpAcross;
	[SerializeField] float wallSlideSpeedMax = 5f;
	[SerializeField] float wallStickTime = .25f;
	[SerializeField] float wallUnstickTime;

	string [] meleeTriggers = new string [] { "Attack01", "Attack02", "Attack03" };

	bool isAlive = true;
	bool isAttacking = false;
	bool isDashing = false;
	bool isWallSliding = false;

	float lastAttackAttempt;
	float lastDashAttempt;
	float startingGravity;
	float startingRunSpeed;

	int attackButtonPressCount;
	int dashButtonPressCount;
	int wallDirX;

	Animator animator;
	CapsuleCollider2D myBodyCollider;
	Rigidbody2D myRigidbody;
	Vector2 directionalInput;

	void Start ()
	{
		animator = GetComponent<Animator>();
		myBodyCollider = GetComponent<CapsuleCollider2D>();
		myRigidbody = GetComponent<Rigidbody2D>();

		startingGravity = myRigidbody.gravityScale;
		startingRunSpeed = runSpeed;
	}
	
	void Update ()
	{
		directionalInput = new Vector2
			(
				CrossPlatformInputManager.GetAxisRaw( "Horizontal" ),
				CrossPlatformInputManager.GetAxisRaw( "Vertical" )
			);
		

		if ( animator.GetBool( "Climbing" ) )
		{
			runSpeed = 2f;
		}
		else
		{
			runSpeed = startingRunSpeed;
		}

		if ( !isDashing )
		{
			Run();
			Melee();
		}
		Climb();

		if ( CrossPlatformInputManager.GetButtonDown( "Jump" ) )
		{
			Jump();
		}

		Dash();
		HandleWallSliding();

		//TODO Stop dashing and grab ladder?
	}

	bool IsOnGround()
	{
		RaycastHit2D hit = Physics2D.Raycast( myRigidbody.transform.position, Vector2.down, 0.5f, LayerMask.GetMask( "Ground" ) );
		return hit.collider != null;
	}

	bool IsOnWall()
	{
		RaycastHit2D hit = Physics2D.Raycast( myRigidbody.transform.position, Vector2.left, 0.5f, LayerMask.GetMask( "Ground" ) );
		if ( hit.collider )
		{
			wallDirX = -1;
			transform.localScale = new Vector2( 1f, 1f );
		}

		if ( !hit.collider )
		{
			hit = Physics2D.Raycast( myRigidbody.transform.position, Vector2.right, 0.5f, LayerMask.GetMask( "Ground" ) );
			if ( hit.collider )
			{
				wallDirX = 1;
				transform.localScale = new Vector2( -1f, 1f );
			}
		}

		return hit.collider != null;
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
		if ( !myBodyCollider.IsTouchingLayers( LayerMask.GetMask( "Climbing" ) ) )
		{
			myRigidbody.gravityScale = startingGravity;
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
			animator.SetBool( "ClimbingIdle", false );
			animator.SetBool( "Climbing", true );
			myRigidbody.gravityScale = 0;
		}
		else
		{
			animator.SetBool( "ClimbingIdle", true );
			animator.SetBool( "Climbing", false );
			myRigidbody.gravityScale = 0;
		}
	}

	void Jump()
	{
		if ( myRigidbody.velocity.y < 0 )
		{
			animator.ResetTrigger( "Jumping" );
			animator.SetBool( "Landing", true );
		}
		else
		{
			animator.SetBool( "Landing", false );
		}

		if ( isWallSliding )
		{
			if ( directionalInput.x == wallDirX )
			{
				myRigidbody.velocity = new Vector2( -transform.localScale.x * wallJumpClimb.x, wallJumpClimb.y );
			}
			else if ( directionalInput.x == 0 )
			{
				myRigidbody.velocity = new Vector2( -transform.localScale.x * wallJumpOff.x, wallJumpOff.y );
			}
			else
			{
				myRigidbody.velocity = new Vector2( -transform.localScale.x * wallJumpAcross.x, wallJumpAcross.y );
			}
		}

		if ( !IsOnGround() ) { return; }

		animator.SetTrigger( "Jumping" );
		Vector2 jumpVelocityToAdd = new Vector2( 0, jumpSpeed );
		myRigidbody.velocity += jumpVelocityToAdd;
	}

	void HandleWallSliding()
	{
		isWallSliding = false;
		if ( IsOnWall() && !IsOnGround() )
		{
			isWallSliding = true;
			animator.SetBool( "Wall Slide", true );
			if ( myRigidbody.velocity.y < -wallSlideSpeedMax )
			{
				var slideSpeed = myRigidbody.velocity;
				slideSpeed.y = -wallSlideSpeedMax;
			}

			if( wallUnstickTime > 0 )
			{
				myRigidbody.velocity = new Vector2( 0, myRigidbody.velocity.y );

				if( directionalInput.x == wallDirX && directionalInput.x != 0 )
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
		else
		{
			FlipSprite();
			animator.SetBool( "Wall Slide", false );
		}
	}

	void Dash()
	{
		if ( !isAttacking )
		{
			var playerMovement = CrossPlatformInputManager.GetAxis( "Horizontal" );
			if ( Input.GetKeyDown( KeyCode.LeftShift ) || Input.GetKeyDown( KeyCode.RightShift ) )
			{
				lastDashAttempt = Time.time;
				dashButtonPressCount++;

				if ( ( Time.time - lastDashAttempt ) < dashButtonLeeway )
				{
					if ( dashButtonPressCount == 1 )
					{
						isDashing = true;
						animator.SetBool( "Dashing", true );

						if ( directionalInput.x == 1 )
						{
							myRigidbody.velocity = Vector2.right * dashSpeed;
						}
						else
						{
							myRigidbody.velocity = Vector2.left * dashSpeed;
						}
					}
				}
			}
		}

		if ( ( Time.time - lastDashAttempt ) > dashButtonLeeway )
		{
			EndDash();
		}
	}

	void EndDash()
	{
		isDashing = false;
		animator.SetBool( "Dashing", false );
		dashButtonPressCount = 0;
	}

	void Melee()
	{
		if ( CrossPlatformInputManager.GetButtonDown( "Fire1" ) && attackButtonPressCount < 3 )
		{
			isAttacking = true;
			lastAttackAttempt = Time.time;
			var currentAttackAttempt = Time.time - lastAttackAttempt;

			animator.SetTrigger( meleeTriggers [attackButtonPressCount] );
			attackButtonPressCount++;
		}

		if ( attackButtonPressCount > 0 )
		{
			if ( ( Time.time - lastAttackAttempt ) > attackButtonLeeway )
			{
				EndMeleeAnimation();
				attackButtonPressCount = 0;
			}
		}
	}

	void EndMeleeAnimation()
	{
		isAttacking = false;
	}
}