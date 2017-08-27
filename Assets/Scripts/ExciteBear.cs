using UnityEngine;
using System.Collections;
using System.Collections.Generic;

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

	public AudioSource flickSoundEffect;

	public List<AudioSource> ouchSoundEffects;

	bool hasJumped = false;

	Quaternion gyroRotation = Quaternion.identity;

	void Awake()
	{
		RESETisBeingFlicked = isBeingFlicked;
		RESETflickedDirection = flickedDirection;
		RESETflickSpeed = flickSpeed;


	//	groundHeight = 1;
		Vector3 pos;
		if ( transform.position.y < groundHeight )
		{
			pos = transform.position;
			pos.y = groundHeight;
			transform.position = pos;
		}
		/*
		pos = transform.position;
		pos.y = 1;
		transform.position = pos;
		gravity = 0;
*/
		RESETposition = transform.position;


	}

	protected override void Start () 
	{
		base.Start();
		Vector3 pos = transform.position;
		pos.z = -6;
		transform.position = pos;
	}

	// Update is called once per frame
	protected override void FixedUpdate () 
	{
		if ( GameManager.Instance.gameState == GameManager.GameState.MENUS )
			return;
		if ( currentState == ExciteState.ELIGIBLE )
		{
			if ( !isBeingFlicked )
			{
				base.FixedUpdate();
				if ( hasJumped && onGround && velocity.y < 0 )
				{
					GameManager.Instance.FinishedWave();
				}
			}
			else
			{
				transform.position += flickedDirection * flickSpeed * Time.fixedDeltaTime;
			}
			CheckBoundaries();
		}
		else if ( currentState == ExciteState.CRASHING )
		{
			transform.position += velocity * Time.fixedDeltaTime;

			transform.Rotate( Vector3.forward, rotationalVelocity*Time.fixedDeltaTime );
			CheckBoundaries();
		}
		else if ( currentState == ExciteState.ENJOYINGSUCCESS )
		{
			base.FixedUpdate();
			if ( onGround )
			{
				velocity.y = 0;
				if ( SpriteDistanceApart( transform.position, RESETposition ) < 4*Time.fixedDeltaTime )
				{
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
		if ( GameManager.Instance.gameState == GameManager.GameState.MENUS )
			return;

		direction.z = 0;
		direction.Normalize();
		if ( !isBeingFlicked && rotationalVelocity == 0 && transform.position.y > groundHeight && direction.sqrMagnitude > 0.0001f )
		{
			flickedDirection = direction;
			isBeingFlicked = true;
			flickSpeed = 12; // 2 * (6f + velocity.magnitude);
			velocity.y = 0;
			if ( flickSoundEffect )
			{
				flickSoundEffect.Play();
			}
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
		else
		{
			GameManager.Instance.StopRewardSound();
		}
	}

	void CheckBoundaries()
	{
		if ( GameManager.Instance.gameState == GameManager.GameState.MENUS )
			return;
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

			int ouchSoundIndex = Random.Range( 0, ouchSoundEffects.Count );
			ouchSoundEffects[ouchSoundIndex].Play();
			if ( currentState == ExciteState.CRASHING && pos.y <= bounds.y )
			{
				GameManager.Instance.ResetGame();
			}
			else
			{
				GameManager.Instance.StopRewardSound();
				currentState = ExciteState.CRASHING;
			}
		}

	}

	override public void JumpUp()
	{
		if ( onGround )
		{
			velocity.y = gravity*1.1f;
			hasJumped = true;
		}
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
