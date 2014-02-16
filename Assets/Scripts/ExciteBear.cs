using UnityEngine;
using System.Collections;

public class ExciteBear : Bear {

	public enum ExciteState
	{
		ELIGIBLE, CRASHING, ENJOYINGSUCCESS
	};

	ExciteState currentState = ExciteState.ELIGIBLE;
	bool isBeingFlicked = false;
	Vector3 flickedDirection = Vector3.zero;
	float flickSpeed = 6f;
	
	bool RESETisBeingFlicked;
	Vector3 RESETflickedDirection;
	float RESETflickSpeed;
	Vector3 RESETposition;

	bool hasJumped = false;

	Quaternion gyroRotation = Quaternion.identity;

	void Awake()
	{
		RESETisBeingFlicked = isBeingFlicked;
		RESETflickedDirection = flickedDirection;
		RESETflickSpeed = flickSpeed;
		if ( transform.position.y < groundHeight )
		{
			Vector3 pos = transform.position;
			pos.y = groundHeight;
			transform.position = pos;
		}
		RESETposition = transform.position;
	}
	
	// Update is called once per frame
	protected override void Update () 
	{
		if ( currentState == ExciteState.ELIGIBLE )
		{
			if ( !isBeingFlicked )
			{
				base.Update();
				if ( hasJumped && onGround && velocity.y < 0 )
				{
					Debug.Log("just landed" );
					GameManager.Instance.FinishedWave();
				//	GameManager.Instance.FinishedWave();
				}
			}
			else
			{
				transform.position += flickedDirection * flickSpeed * Time.deltaTime;
			}
			CheckBoundaries();
		}
		else if ( currentState == ExciteState.CRASHING )
		{
			transform.position += velocity * Time.deltaTime;

			transform.Rotate( Vector3.forward, rotationalVelocity*Time.deltaTime );
			CheckBoundaries();
		}
		else if ( currentState == ExciteState.ENJOYINGSUCCESS )
		{
			base.Update();
			if ( onGround )
			{
				velocity.y = 0;
				if ( SpriteDistanceApart( transform.position, RESETposition ) < 4*Time.deltaTime )
				{
					Debug.Log("now start the next wave" );
					GameManager.Instance.BeginNextWave();
					isBeingFlicked = false;
					flickedDirection = RESETflickedDirection;
					flickSpeed = RESETflickSpeed;
					velocity = Vector3.zero;
					transform.position = RESETposition;
					currentState = ExciteState.ELIGIBLE;
				}
				else
				{
					if ( transform.position.x > RESETposition.x )
					{
						velocity.x = -4f;
					}
					else if ( transform.position.x < RESETposition.x )
					{
						velocity.x = 4f;
					}
					else
					{
					}
				}
			}
		}

	}

	protected override void LateUpdate()
	{
		if ( currentState == ExciteState.ELIGIBLE || currentState == ExciteState.ENJOYINGSUCCESS )
		{
			base.LateUpdate();
		}
	}

	public void FoundAHighFiver()
	{
		isBeingFlicked = false;
	//	GameManager.Instance.SpawnNewOppositeBear();
	}

	public void Flick( Vector3 direction )
	{
		if ( !isBeingFlicked && rotationalVelocity == 0 && transform.position.y > groundHeight )
		{
			flickedDirection = direction;
			flickedDirection.z = 0;
			flickedDirection.Normalize();
			isBeingFlicked = true;
			flickSpeed = 12; // 2 * (6f + velocity.magnitude);
			velocity.y = 0;
		}
	}

	public void OnCompleteWave( bool success )
	{
		if ( success )
		{
			hasJumped = false;
			rotationalVelocity = 0;
			SetSlowDownPercentDirectly( Vector2.one );
			currentState = ExciteState.ENJOYINGSUCCESS;
		}
	}

	void CheckBoundaries()
	{
		Vector3 pos = transform.position;
		Vector4 bounds = GameManager.Instance.worldBoundaries;
		if ( pos.x < bounds.x || pos.y < bounds.y || pos.x > bounds.z || pos.y > bounds.w )
		{
			if ( pos.x < bounds.x )
			{
				pos.x = bounds.x;
				flickedDirection.y *= -1;
				velocity.y *= -1;
			}
			if ( pos.y < bounds.y )
			{
				pos.y = bounds.y;
				flickedDirection.x *= -1;
				velocity.x *= -1;
			}
			if ( pos.x > bounds.z )
			{
				pos.x = bounds.z;
				flickedDirection.y *= -1;
				velocity.y *= -1;
			}
			if ( pos.y > bounds.w )
			{
				pos.y = bounds.w;
				flickedDirection.x *= -1;
				velocity.x *= -1;
			}


			transform.position = pos;
			rotationalVelocity = 1440f;
			if ( currentState == ExciteState.ELIGIBLE )
			{
				velocity = flickedDirection * -10f;

			}
			else
			{
				velocity *= -1;
			}

			if ( currentState == ExciteState.CRASHING && pos.y <= bounds.y )
			{
				GameManager.Instance.ResetGame();
			}
			else
				currentState = ExciteState.CRASHING;
		}

	}

	override public void JumpUp()
	{
		if ( onGround )
		{
			velocity.y = gravity*1.1f;
			hasJumped = true;
		}
		else
			Debug.Log( this.name + " is not on the ground" );
	}

	public void Reset()
	{
		isBeingFlicked = RESETisBeingFlicked;
		flickedDirection = RESETflickedDirection;
		flickSpeed = RESETflickSpeed;

		transform.rotation = Quaternion.identity;
		transform.position = RESETposition;

		slowDownPercent = Vector2.one;
		targetSlowDownPercent = Vector2.one;
		rotationalVelocity = 0f;
		velocity = Vector3.zero;
		currentState = ExciteState.ELIGIBLE;
		onGround = true;
		hasJumped = false;
	}

	public ExciteState GetCurrentState()
	{
		return currentState;
	}
}
