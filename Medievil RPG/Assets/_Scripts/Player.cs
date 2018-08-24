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

	string[] meleeTriggers = new string [] { "Attack01", "Attack02", "Attack03" };

	bool facingRight = true;
	bool isAlive = true;
	bool isAttacking = false;
	bool isDashing = false;

	float lastAttackAttempt;
	float lastDashAttempt;
	float startingGravity;
	float startingRunSpeed;

	int dashButtonPressCount;
	public int attackButtonPressCount; // TODO make getter/setter and fix animation scripts

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
		Jump();
		Dash();

		/*TODO Stop dashing and grab ladder?
		 * else
		{
			if ( myCollider2D.IsTouchingLayers( LayerMask.GetMask( "Climbing" ) ) )
			{
				myRigidbody.velocity = Vector2.zero;
			}
		}*/
	}

	void FlipSprite()
	{
		bool playerHasHorizontalSpeed = Mathf.Abs( myRigidbody.velocity.x ) > Mathf.Epsilon;
		if ( playerHasHorizontalSpeed )
		{
			transform.localScale = new Vector2( Mathf.Sign( myRigidbody.velocity.x ), 1f );

			if ( Mathf.Sign( myRigidbody.velocity.x ) == 1 )
			{
				facingRight = true;
			}
			else
			{
				facingRight = false;
			}
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

	void Jump() // TODO Wall riding animation
	{
		if ( myRigidbody.velocity.y < 0 )
		{
			animator.SetBool( "Landing", true );
		}
		else
		{
			animator.SetBool( "Landing", false );
		}

		if ( !myCollider2D.IsTouchingLayers( LayerMask.GetMask( "Ground" ) ) ) { return; }

		animator.ResetTrigger( "Jumping" );
		if ( CrossPlatformInputManager.GetButtonDown( "Jump" ) )
		{
			Vector2 jumpVelocityToAdd = new Vector2( 0, jumpSpeed );
			myRigidbody.velocity += jumpVelocityToAdd;
			animator.SetTrigger( "Jumping" );
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

						if ( facingRight )
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
		if ( CrossPlatformInputManager.GetButtonDown( "Fire1" ) && attackButtonPressCount < meleeTriggers.Length )
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

	/*IEnumerator TryAttack()
	{
		while ( true )
		{
			if ( CrossPlatformInputManager.GetButtonDown( "Fire1" ) )
			{
				attackButtonPressCount++;
				//runSpeed = 2f;

				animator.SetTrigger( "Attack0" + attackButtonPressCount );
				lastAttackAttempt = Time.time;
				var currentAttackAttempt = Time.time - lastAttackAttempt;

				while ( ( Time.time - lastAttackAttempt ) < attackButtonLeeway && attackButtonPressCount < 3 )
				{
					if ( CrossPlatformInputManager.GetButtonDown( "Fire1" ) && ( Time.time - lastAttackAttempt ) > 0.3f )
					{
						animator.SetTrigger( "Attack0" + attackButtonPressCount );
						attackButtonPressCount++;
						lastAttackAttempt = Time.time;
					}
					yield return null;
				}

				attackButtonPressCount = 0;
				yield return new WaitForSeconds( attackButtonLeeway - ( Time.time - lastAttackAttempt ) );
			}
			yield return null;
		}
	}*/
}