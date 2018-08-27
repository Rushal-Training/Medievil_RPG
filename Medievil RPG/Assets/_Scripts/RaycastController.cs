using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent( typeof( BoxCollider2D ) )]
public class RaycastController : MonoBehaviour
{
	protected const float skinWidth = .015f;
	const float distanceBetweenRays = .15f;

	[SerializeField] protected LayerMask collisionMask;

	protected int horizontalRayCount;
	protected int verticalRayCount;

	protected float horizontalRaySpacing, verticalRaySpacing;

	protected BoxCollider2D playerCollider;
	protected RaycastOrigins raycastOrigins;

	protected virtual void Awake()
	{
		playerCollider = GetComponent<BoxCollider2D>();
	}

	protected virtual void Start()
	{
		CalculateRaySpacing();
	}

	protected struct RaycastOrigins
	{
		public Vector2 topLeft, topRight, bottomLeft, bottomRight;
	}

	protected void CalculateRaySpacing()
	{
		Bounds bounds = playerCollider.bounds;
		bounds.Expand( skinWidth * -2 );

		float boundsWidth = bounds.size.x;
		float boundsHeight = bounds.size.y;

		horizontalRayCount = Mathf.RoundToInt( boundsHeight / distanceBetweenRays );
		verticalRayCount = Mathf.RoundToInt( boundsWidth / distanceBetweenRays );

		horizontalRayCount = Mathf.Clamp( horizontalRayCount, 2, int.MaxValue );
		verticalRayCount = Mathf.Clamp( verticalRayCount, 2, int.MaxValue );

		horizontalRaySpacing = bounds.size.y / ( horizontalRayCount - 1 );
		verticalRaySpacing = bounds.size.x / ( verticalRayCount - 1 );
	}

	protected void UpdateRaycastOrigins()
	{
		Bounds bounds = playerCollider.bounds;
		bounds.Expand( skinWidth * -2 );

		raycastOrigins.bottomLeft = new Vector2( bounds.min.x, bounds.min.y );
		raycastOrigins.bottomRight = new Vector2( bounds.max.x, bounds.min.y );
		raycastOrigins.topLeft = new Vector2( bounds.min.x, bounds.max.y );
		raycastOrigins.topRight = new Vector2( bounds.max.x, bounds.max.y );
	}
}