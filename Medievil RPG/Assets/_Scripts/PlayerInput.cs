using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(Controller2D))]
public class PlayerInput : MonoBehaviour
{
	[SerializeField] float accelerationTimeAirborne = .2f;
	[SerializeField] float accelerationTimeGrounded = .1f;
	[SerializeField] float jumpHeight = 3.5f;
	[SerializeField] float timeToJumpApex = .4f;
	[SerializeField] float moveSpeed = 6f;

	float gravity, jumpVelocity, velocityXSmoothing;

	Controller2D controller2D;
	Vector3 velocity;

	void Start ()
	{
		
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

		if ( Input.GetKeyDown( KeyCode.Space ) && controller2D.collisionInfo.below )
		{
			velocity.y = jumpVelocity;
		}

		float tartgetVelocityX = input.x * moveSpeed;
		velocity.x = Mathf.SmoothDamp( velocity.x, tartgetVelocityX, ref velocityXSmoothing, (controller2D.collisionInfo.below)?accelerationTimeGrounded:accelerationTimeAirborne );
		velocity.y += gravity * Time.deltaTime;
		controller2D.Move( velocity * Time.deltaTime );
	}


}
