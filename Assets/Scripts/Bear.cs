using UnityEngine;
using System.Collections;

public class Bear : MonoBehaviour {

	public Vector3 velocity = Vector3.zero;

	protected bool onGround = false;
	protected float groundHeight = -4.6f;
	protected float gravity = 9f;
	protected Vector2 slowDownPercent = Vector2.one;
	protected Vector2 targetSlowDownPercent = Vector2.one;
	public float rotationalVelocity = 0f;
	float rotationalFriction = 720f;
	Quaternion naturalRotation = Quaternion.identity;
	
	virtual protected void Start()
	{
		naturalRotation = transform.rotation;
		Vector3 pos = transform.position;
		pos.z = groundHeight;
		transform.position = pos;
	}


	// Update is called once per frame
	virtual protected void Update () 
	{
		transform.position += velocity * Time.deltaTime * slowDownPercent.x;
		velocity.y -= gravity * Time.deltaTime * slowDownPercent.y;
		if ( rotationalVelocity > 0 )
		{
			rotationalVelocity -= rotationalFriction*Time.deltaTime;
			transform.Rotate( Vector3.forward, rotationalVelocity*Time.deltaTime );
			if ( rotationalVelocity <= 0 )
			{
				OnDoneRotating();
			}
		}
		else if ( rotationalVelocity < 0 )
		{
			rotationalVelocity += rotationalFriction*Time.deltaTime;
			transform.Rotate( Vector3.forward, rotationalVelocity*Time.deltaTime );
			if ( rotationalVelocity >= 0 )
			{
				OnDoneRotating();
			}
		}
		else if ( transform.rotation != naturalRotation )
		{
			transform.rotation = Quaternion.RotateTowards( transform.rotation, naturalRotation, 180f * Time.deltaTime );
		}

		if ( targetSlowDownPercent.x > slowDownPercent.x )
		{
			slowDownPercent.x += Time.deltaTime;
			if ( slowDownPercent.x > targetSlowDownPercent.x )
				slowDownPercent.x = targetSlowDownPercent.x;
		}
		else if ( targetSlowDownPercent.x < slowDownPercent.x )
		{
			slowDownPercent.x -= Time.deltaTime;
			if ( slowDownPercent.x < targetSlowDownPercent.x )
				slowDownPercent.x = targetSlowDownPercent.x;
		}
		if ( targetSlowDownPercent.y > slowDownPercent.y )
		{
			slowDownPercent.y += Time.deltaTime;
			if ( slowDownPercent.y > targetSlowDownPercent.y )
				slowDownPercent.y = targetSlowDownPercent.y;
		}
		else if ( targetSlowDownPercent.y < slowDownPercent.y )
		{
			slowDownPercent.y -= Time.deltaTime;
			if ( slowDownPercent.y < targetSlowDownPercent.y )
				slowDownPercent.y = targetSlowDownPercent.y;
		}
	}

	virtual protected void LateUpdate()
	{
		onGround = false;
		if ( transform.position.y <= groundHeight )
		{
			Vector3 pos = transform.position;
			pos.y = groundHeight;
			transform.position = pos;
			onGround = true;
		}
	}

	virtual public void JumpUp()
	{
		if ( onGround )
		{
			velocity.y = gravity*1.1f;
		}
		else
			Debug.Log( this.name + " is not on the ground" );
	}

	public void SetSlowDownPercentDirectly( Vector2 newPercent )
	{
		slowDownPercent = newPercent;
		targetSlowDownPercent = newPercent;
	}

	public static float SpriteDistanceApart( Vector3 a, Vector3 b )
	{
		Vector2 a2 = new Vector2( a.x, a.y );
		Vector2 b2 = new Vector2( b.x, b.y );
		return Vector2.Distance( a2, b2 );
	}

	protected virtual void OnDoneRotating()
	{
		rotationalVelocity = 0;
		targetSlowDownPercent = Vector2.one;
		velocity.y = 0;
	}
}
