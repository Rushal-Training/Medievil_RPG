using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(Controller2D))]
public class PlayerInput : MonoBehaviour
{
	Controller2D controller2D;
	Vector3 velocity;

	float gravity = -20f;
	float moveSpeed = 6;

	void Start ()
	{
		controller2D = GetComponent<Controller2D>();
	}
	
	void Update ()
	{
		Vector2 input = new Vector2( Input.GetAxisRaw( "Horizontal" ), Input.GetAxisRaw( "Vertical" ) );

		velocity.x = input.x * moveSpeed;
		velocity.y += gravity * Time.deltaTime;
		controller2D.Move( velocity * Time.deltaTime );
	}


}
