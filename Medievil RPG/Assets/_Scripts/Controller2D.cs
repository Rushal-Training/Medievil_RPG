using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Controller2D : RaycastController
{
	public CollisionInfo collisionInfo;
	[HideInInspector] public Vector2 playerInput;

	[SerializeField] float maxClimbAngle = 45f;
	[SerializeField] float maxDescendAngle = 45f;

	public void Move( Vector2 moveAmount, bool standingOnPlatform )
	{
		Move( moveAmount, Vector2.zero, standingOnPlatform );
	}

	public void Move( Vector2 moveamount, Vector2 input, bool standingOnPlatform = false )
	{
		UpdateRaycastOrigins();
		collisionInfo.Reset();
		collisionInfo.velocityOld = moveamount;
		playerInput = input;

		if ( moveamount.x != 0 )
		{
			collisionInfo.faceDir = (int)Mathf.Sign( moveamount.x );
		}
		if ( moveamount.y < 0 )
		{
			DescendSlope( ref moveamount );
		}
		if ( moveamount.y != 0 )
		{
			VerticalCollisions( ref moveamount );
		}

		HorizontalCollisions( ref moveamount );

		transform.Translate( moveamount );

		if( standingOnPlatform )
		{
			collisionInfo.below = true;
		}
	}

	public struct CollisionInfo
	{
		public bool above, below, left, right;

		public bool climbingSlope, descendingSlope, fallingThroughPlatform;
		public float slopeAngle, slopeAngleOld;

		public int faceDir;

		public Vector2 velocityOld;

		public void Reset()
		{
			above = below = false;
			left = right = false;
			climbingSlope = false;
			descendingSlope = false;

			slopeAngleOld = slopeAngle;
			slopeAngle = 0;
		}
	}

	protected override void Start()
	{
		base.Start();

		collisionInfo.faceDir = 1;
	}

	void HorizontalCollisions( ref Vector2 moveAmount )
	{
		float directionX = collisionInfo.faceDir;
		float rayLength = Mathf.Abs( moveAmount.x ) + skinWidth;

		if ( Mathf.Abs( moveAmount.x ) < skinWidth )
		{
			rayLength = 2 * skinWidth;
		}

		for ( int i = 0; i < horizontalRayCount; i++ )
		{
			Vector2 rayOrigin = ( directionX == -1 ) ? raycastOrigins.bottomLeft : raycastOrigins.bottomRight;
			rayOrigin += Vector2.up * ( horizontalRaySpacing * i );
			RaycastHit2D hit = Physics2D.Raycast( rayOrigin, Vector2.right * directionX, rayLength, collisionMask );

			Debug.DrawRay( rayOrigin, Vector2.right * directionX, Color.red );

			if ( hit )
			{
				if ( hit.distance == 0 )
				{
					continue;
				}

				float slopeAngle = Vector2.Angle( hit.normal, Vector2.up );
				if ( i == 0 && slopeAngle <= maxClimbAngle )
				{
					if ( collisionInfo.descendingSlope )
					{
						collisionInfo.descendingSlope = false;
						moveAmount = collisionInfo.velocityOld;
					}
					float distanceToSlopeStart = 0;
					if ( slopeAngle != collisionInfo.slopeAngleOld )
					{
						distanceToSlopeStart = hit.distance - skinWidth;
						moveAmount.x -= distanceToSlopeStart * directionX;
					}
					ClimbSlope( ref moveAmount, slopeAngle );
					moveAmount.x += distanceToSlopeStart * directionX;
				}

				if ( !collisionInfo.climbingSlope || slopeAngle > maxClimbAngle )
				{
					moveAmount.x = ( hit.distance - skinWidth ) * directionX;
					rayLength = hit.distance;

					if(collisionInfo.climbingSlope)
					{
						moveAmount.y = Mathf.Tan( collisionInfo.slopeAngle * Mathf.Deg2Rad ) * Mathf.Abs( moveAmount.x );
					}

					collisionInfo.left = directionX == -1;
					collisionInfo.right = directionX == 1;
				}
			
			}
		}
	}

	void VerticalCollisions(ref Vector2 moveAmount)
	{
		float directionY = Mathf.Sign( moveAmount.y );
		float rayLength = Mathf.Abs( moveAmount.y ) + skinWidth;

		for ( int i = 0; i < verticalRayCount; i++ )
		{
			Vector2 rayOrigin = ( directionY == -1 ) ? raycastOrigins.bottomLeft : raycastOrigins.topLeft;
			rayOrigin += Vector2.right * (verticalRaySpacing * i + moveAmount.x);
			RaycastHit2D hit = Physics2D.Raycast( rayOrigin, Vector2.up * directionY, rayLength, collisionMask );

			Debug.DrawRay( rayOrigin, Vector2.up * directionY, Color.red );

			if ( hit )
			{
				if( hit.collider.tag == "Through" )
				{
					if( directionY == 1 || hit.distance == 0 )
					{
						continue;
					}
					if ( collisionInfo.fallingThroughPlatform )
					{
						continue;
					}
					if ( playerInput.y == -1 )
					{
						collisionInfo.fallingThroughPlatform = true;
						Invoke( "ResetFallingThroughPlatform", .5f );
						continue;
					}
				}

				moveAmount.y = ( hit.distance * skinWidth ) * directionY;
				rayLength = hit.distance;

				if(collisionInfo.climbingSlope)
				{
					moveAmount.x = moveAmount.y / Mathf.Tan( collisionInfo.slopeAngle * Mathf.Deg2Rad ) * Mathf.Sign( moveAmount.x );
				}

				collisionInfo.below = directionY == -1;
				collisionInfo.above = directionY == 1;
			}
		}

		if ( collisionInfo.climbingSlope )
		{
			float directionX = Mathf.Sign( moveAmount.x );
			rayLength = Mathf.Abs( moveAmount.x ) + skinWidth;
			Vector2 rayOrigin = ( ( directionX == -1 ) ? raycastOrigins.bottomLeft : raycastOrigins.bottomRight ) + Vector2.up * moveAmount.y;
			RaycastHit2D hit = Physics2D.Raycast( rayOrigin, Vector2.right * directionX, rayLength, collisionMask );

			if ( hit )
			{
				float slopeAngle = Vector2.Angle( hit.normal, Vector2.up );
				if(slopeAngle != collisionInfo.slopeAngle)
				{
					moveAmount.x = ( hit.distance - skinWidth ) * directionX;
					collisionInfo.slopeAngle = slopeAngle;
				}
			}
		}
	}

	void ClimbSlope( ref Vector2 moveAmount, float slopeAngle )
	{
		float moveDistance = Mathf.Abs( moveAmount.x );
		float climbVelocityY = Mathf.Sin( slopeAngle * Mathf.Deg2Rad ) * moveDistance;
		if ( moveAmount.y <= climbVelocityY )
		{
			moveAmount.y = climbVelocityY;
			moveAmount.x = Mathf.Cos( slopeAngle * Mathf.Deg2Rad ) * moveDistance * Mathf.Sign( moveAmount.x );
			collisionInfo.below = true;
			collisionInfo.climbingSlope = true;
			collisionInfo.slopeAngle = slopeAngle;
		}
	}

	void DescendSlope( ref Vector2 moveAmount )
	{
		float directionX = Mathf.Sign( moveAmount.x );
		Vector2 rayOrigin = ( directionX == -1 ) ? raycastOrigins.bottomRight : raycastOrigins.bottomLeft;
		RaycastHit2D hit = Physics2D.Raycast( rayOrigin, Vector2.down, Mathf.Infinity, collisionMask );

		if ( hit )
		{
			float slopeAngle = Vector2.Angle( hit.normal, Vector2.up );
			if( slopeAngle != 0 && slopeAngle <= maxDescendAngle )
			{
				if( Mathf.Sign(hit.normal.x) == directionX )
				{
					if ( hit.distance - skinWidth <= Mathf.Tan( slopeAngle * Mathf.Deg2Rad ) * Mathf.Abs( moveAmount.x ) )
					{
						float moveDistance = Mathf.Abs( moveAmount.x );
						float descendVelocityY = Mathf.Sin( slopeAngle * Mathf.Deg2Rad ) * moveDistance;
						moveAmount.x = Mathf.Cos( slopeAngle * Mathf.Deg2Rad ) * moveDistance * Mathf.Sign( moveAmount.x );
						moveAmount.y -= descendVelocityY;

						collisionInfo.slopeAngle = slopeAngle;
						collisionInfo.descendingSlope = true;
						collisionInfo.below = true;
					}
				}
			}
		}
	}

	void ResetFallingThroughPlatform()
	{
		collisionInfo.fallingThroughPlatform = false;
	}
}