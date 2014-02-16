using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoBehaviour {

	static GameManager instance;
	
	public static GameManager Instance
	{
		get
		{
			if ( instance == null )
			{
				instance = (GameManager)GameObject.FindObjectOfType(typeof(GameManager));
				if ( !instance )
				{
					GameObject go = new GameObject();
					instance = go.AddComponent<GameManager>();
				}
			}
			return instance;
		}
	}

	public GameObject oppositeBearPrototype;
	public ExciteBear exciteBear;
	public float heightOfMisses = -4f;
	public Vector4 worldBoundaries;

	bool inMiddleOfWave = false;

	int amtAtStartOfWave = 0;
	int firstWaveAmt;
	int currentWaveAmt = 1;
	int waveAmtLeft = 0;
	int totalScore = 0;
	int highScore = 0;

	public GameObject tutorialText;
	public TextMesh exciteText;

	public TextMesh scoreText;
	public TextMesh highScoreText;

	void Awake()
	{
		worldBoundaries = new Vector4( -7.6f, -7, 10.8f, 4.6f );
		firstWaveAmt = currentWaveAmt;
	}

	void Start()
	{
		SpawnNewOppositeBear( currentWaveAmt );
		exciteBear = (ExciteBear)GameObject.FindObjectOfType( typeof( ExciteBear ) );
		exciteText.gameObject.SetActive( false );
	}

	public void SpawnNewOppositeBear( int numBearsInGroup )
	{
		waveAmtLeft = numBearsInGroup;
		inMiddleOfWave = true;
		amtAtStartOfWave = numBearsInGroup;
		Debug.Log("now spawning a wave of " + amtAtStartOfWave + " bears." );

		List<int> lanesLeft = new List<int>();
		for ( int i = 0; i < 4; ++i )
		{
			lanesLeft.Add( i );
		}

		if ( oppositeBearPrototype )
		{
			for ( int i = 0; i < numBearsInGroup; ++i )
			{
				float spacing = Random.Range( 4.75f, 5.5f );
				if ( i == 0 )
					spacing = 0;
				GameObject go = (GameObject) GameObject.Instantiate( oppositeBearPrototype, new Vector3( 10 + spacing*i, -6, -1 ), Quaternion.Euler( 0, 180, 0 ) );
				int lane = Random.Range( 0, 3 );
				if ( lanesLeft.Count > 0 )
				{
					lane = Random.Range( 0, lanesLeft.Count );
					go.GetComponent<OppositeBear>().SetTrackPosition( lanesLeft[lane] );
					lanesLeft.RemoveAt( lane );
				}
				else
				{
					go.GetComponent<OppositeBear>().SetTrackPosition( lane );
				}

				if ( i == numBearsInGroup-1 )
				{
					go.GetComponent<OppositeBear>().isLastOfWave = true;

				}
			}
		}
	}

	public void ResetGame()
	{
		Debug.Log(" failed a wave, resetting the game" );
		inMiddleOfWave = false;
		OppositeBear[] allOppositeBears = (OppositeBear[]) GameObject.FindObjectsOfType( typeof( OppositeBear ) );
		for ( int i = 0; i < allOppositeBears.Length; ++i )
		{
			Destroy( allOppositeBears[i].gameObject );
		}
		if ( totalScore > highScore )
		{
			highScore = totalScore;
			highScoreText.text = highScore.ToString();
		}
		totalScore = 0;
		tutorialText.SetActive( true );
		exciteText.gameObject.SetActive( false );
		exciteBear.Reset();
		currentWaveAmt = firstWaveAmt;
		SpawnNewOppositeBear( currentWaveAmt );

	}

	public void GotHighFive( OppositeBear ob )
	{
		waveAmtLeft--;
		totalScore++;
		scoreText.text = totalScore.ToString();
		tutorialText.SetActive( false );
		string actualText = "";
		switch ( amtAtStartOfWave - waveAmtLeft )
		{
		case 1:
			break;
		case 2:
			actualText = "Double\n";
			break;
		case 3:
			actualText = "Triple\n";
			break;
		case 4:
			actualText = "Quadra\n";
			break;
		case 5:
			actualText = "Penta\n";
			break;
		default:
			actualText = "Mega Ultra\n";
			break;
		}
		actualText += "High Five!";

		exciteText.text = actualText;
		exciteText.gameObject.SetActive( true );
	}

	public void FinishedWave()
	{
		if ( inMiddleOfWave )
		{
			Debug.Log("finished the game with " + waveAmtLeft + " to go." );
			inMiddleOfWave = false;
			exciteBear.OnCompleteWave( waveAmtLeft == 0 );
			if ( waveAmtLeft == 0 )
			{
				currentWaveAmt++;
			}
			else
			{
				ResetGame();
			}
		}
	}

	public void BeginNextWave()
	{
		OppositeBear[] allOppositeBears = (OppositeBear[]) GameObject.FindObjectsOfType( typeof( OppositeBear ) );
		for ( int i = 0; i < allOppositeBears.Length; ++i )
		{
			Destroy( allOppositeBears[i].gameObject );
		}
		SpawnNewOppositeBear( currentWaveAmt );
	}
}
