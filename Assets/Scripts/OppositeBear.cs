using UnityEngine;
using System.Collections;

public class OppositeBear : Bear {

	enum HFState 
	{
		PREPARING, AWAITING_HIGH_FIVE, SUCCESSFUL_HIGH_FIVE, MISSED_HIGH_FIVE
	};

	HFState currentState;
	ExciteBear exciteBear;
	float currentDistFromExciteBear;
	float maximumHighFiveDistance = .55f;
	public bool isLastOfWave = false;

	bool hitTrigger = false;

	// Use this for initialization
	protected override void Start () {

		exciteBear = (ExciteBear)GameObject.FindObjectOfType( typeof( ExciteBear ) );
		velocity.x = -6f; // Random.Range( -6.5f, -5f );
		currentState = HFState.PREPARING;
		base.Start();
	}
	
	// Update is called once per frame
	protected override void Update () 
	{
		base.Update();
		if ( currentState == HFState.AWAITING_HIGH_FIVE && exciteBear.GetCurrentState() == ExciteBear.ExciteState.ELIGIBLE )
		{
			float distFromExciteBear = Bear.SpriteDistanceApart( transform.position, exciteBear.transform.position );

			if ( distFromExciteBear > currentDistFromExciteBear && currentDistFromExciteBear > 0 )
			{
				//was at it's closest point last frame
				if ( currentDistFromExciteBear < maximumHighFiveDistance )
				{
					HitHighFive( currentDistFromExciteBear );
				}
			}
			else
			{
				currentDistFromExciteBear = distFromExciteBear;
			}

			if ( transform.position.y < GameManager.Instance.heightOfMisses && velocity.y < 0 )
			{
				MissedHighFive();
			}
		}

		if ( transform.position.x < -12f )
		{
			if ( isLastOfWave )
			{
				GameManager.Instance.FinishedWave();
			}
			Destroy( gameObject );
		}
	}

	void HitHighFive( float dist )
	{
		Debug.Log("high five!!  Bears were " + dist + " apart." );
		// successful high five
		//Time.timeScale = 0;
		rotationalVelocity = 720f;
		exciteBear.rotationalVelocity = -720f;
		SetSlowDownPercentDirectly( new Vector2( .0f, .0f ) );
		exciteBear.SetSlowDownPercentDirectly( new Vector2( .0f, .00f ) );
		exciteBear.FoundAHighFiver();
		currentState = HFState.SUCCESSFUL_HIGH_FIVE;
		GetComponent<SpriteRenderer>().color = new Color( 0, .92f, 1f );
		OppositeBear[] allOppositeBears = (OppositeBear[]) GameObject.FindObjectsOfType( typeof( OppositeBear ) );
		for ( int i = 0; i < allOppositeBears.Length; ++i )
		{
			if ( allOppositeBears[i].currentState == HFState.AWAITING_HIGH_FIVE || allOppositeBears[i].currentState == HFState.PREPARING )
			{
				allOppositeBears[i].SetSlowDownPercentDirectly( new Vector2( .4f, .35f ) );
			}
		}
		GameManager.Instance.GotHighFive( this );

	}

	void MissedHighFive()
	{
		GetComponent<SpriteRenderer>().color = Color.red;
		velocity.y -= gravity;
		currentState = HFState.MISSED_HIGH_FIVE;
	}

	public override void JumpUp()
	{
		if ( onGround )
		{
			//gravity*1.25f is a good max
			float gravMultiplier = .9f;
			velocity.y = Random.Range( gravity*(.6f-groundHeight*.1f), gravity*(.88f-groundHeight*.1f) );
		}
		else
			Debug.Log( this.name + " is not on the ground" );

		currentState = HFState.AWAITING_HIGH_FIVE;
		currentDistFromExciteBear= -1f;
	}

	protected override void OnDoneRotating()
	{
		base.OnDoneRotating();
		OppositeBear[] allOppositeBears = (OppositeBear[]) GameObject.FindObjectsOfType( typeof( OppositeBear ) );
		for ( int i = 0; i < allOppositeBears.Length; ++i )
		{
			if ( allOppositeBears[i].currentState == HFState.AWAITING_HIGH_FIVE || allOppositeBears[i].currentState == HFState.PREPARING )
				allOppositeBears[i].SetSlowDownPercentDirectly( Vector2.one ); // targetSlowDownPercent = Vector2.one;
		}
	}

	public void SetTrackPosition( int position )
	{
		groundHeight = -4.6f + position;
		Vector3 pos = transform.position;
		pos.y = groundHeight;
		pos.z = groundHeight;
		transform.position = pos;
	}

	void OnTriggerEnter2D(Collider2D other)
	{
		if ( other.GetComponent<JumpTrigger>() )
		{
			if ( !hitTrigger )
			{
				JumpUp();
				exciteBear.JumpUp();
			}
			hitTrigger = true;
		}
	}

}
