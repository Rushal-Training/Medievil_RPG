using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
	[SerializeField] float verticalOffset;
	[SerializeField] float lookAheadDistanceX;
	[SerializeField] float lookAheadSmoothTimeX;
	[SerializeField] float verticalSmoothTime;

	[SerializeField] Controller2D target;
	[SerializeField] Vector2 focusAreaSize;

	bool lookAheadStopped;
	float currentLookAheadX, targetLookAheadX, lookAheadDirX, smoothLookAheadVelocityX, smoothLookAheadVelocityY;

	FocusArea focusArea;

	struct FocusArea
	{
		public Vector2 center, velocity;
		float left, right, top, bottom;

		public FocusArea( Bounds targetBounds, Vector2 size )
		{
			left = targetBounds.center.x - size.x / 2;
			right = targetBounds.center.x + size.x / 2;
			bottom = targetBounds.min.y;
			top = targetBounds.max.y;

			center = new Vector2( ( left + right ) / 2, ( top + bottom ) / 2 );
			velocity = Vector2.zero;
		}

		public void Update( Bounds targetBounds )
		{
			float shiftX = 0;
			if ( targetBounds.min.x < left )
			{
				shiftX = targetBounds.min.x - left;
			}
			else if ( targetBounds.max.x > right )
			{
				shiftX = targetBounds.max.x - right;
			}
			left += shiftX;
			right += shiftX;

			float shiftY = 0;
			if ( targetBounds.min.y < bottom )
			{
				shiftY = targetBounds.min.y - bottom;
			}
			else if ( targetBounds.max.y > top )
			{
				shiftY = targetBounds.max.y - top;
			}
			top += shiftY;
			bottom += shiftY;
			center = new Vector2( ( left + right ) / 2, ( top + bottom ) / 2 );
			velocity = new Vector2( shiftX, shiftY );
		}
	}

	void Start()
	{
		focusArea = new FocusArea( target.GetComponent<Collider2D>().bounds, focusAreaSize );
	}

	void LateUpdate()
	{
		focusArea.Update( target.GetComponent<Collider2D>().bounds );

		Vector2 focusPos = focusArea.center + Vector2.up * verticalOffset;

		if( focusArea.velocity.x != 0 )
		{
			lookAheadDirX = Mathf.Sign( focusArea.velocity.x );

			if(Mathf.Sign(target.playerInput.x) == Mathf.Sign(focusArea.velocity.x) && target.playerInput.x != 0)
			{
				lookAheadStopped = false;
				targetLookAheadX = lookAheadDirX * lookAheadDistanceX;
			}
			else
			{
				if( !lookAheadStopped )
				{
					lookAheadStopped = true;
					targetLookAheadX = currentLookAheadX + ( lookAheadDirX * lookAheadDistanceX - currentLookAheadX ) / 4f;
				}
			}
		}

		currentLookAheadX = Mathf.SmoothDamp( currentLookAheadX, targetLookAheadX, ref smoothLookAheadVelocityX, lookAheadSmoothTimeX );

		focusPos.y = Mathf.SmoothDamp( transform.position.y, focusPos.y, ref smoothLookAheadVelocityY, verticalSmoothTime );
		focusPos += Vector2.right * currentLookAheadX;
		transform.position = (Vector3)focusPos + Vector3.forward * -10;
	}

	void OnDrawGizmos()
	{
		Gizmos.color = new Color( 1, 0, 0, .5f );
		Gizmos.DrawCube( focusArea.center, focusAreaSize );
	}
}
