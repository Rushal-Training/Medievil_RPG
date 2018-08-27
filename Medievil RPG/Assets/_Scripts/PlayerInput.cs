using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent( typeof( Player2D ) )]
public class PlayerInput : MonoBehaviour
{
	Player2D player;

	void Start ()
	{
		player = GetComponent<Player2D>();
	}

	void Update()
	{
		Vector2 directionalInput = new Vector2( Input.GetAxisRaw( "Horizontal" ), Input.GetAxisRaw( "Vertical" ) );
		player.SetDirectionalInput( directionalInput );

		if ( Input.GetKeyDown( KeyCode.Space ) )
		{
			player.OnJumpInputDown();
		}
		if ( Input.GetKeyUp( KeyCode.Space ) )
		{
			player.OnJumpInputUp();
		}
		if ( Input.GetKeyDown( KeyCode.LeftShift ) || Input.GetKeyDown( KeyCode.RightShift ) )
		{
			player.OnDashInputDown();
		}
	}
}
