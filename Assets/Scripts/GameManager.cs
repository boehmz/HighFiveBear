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

	public enum GameState { MENUS, PLAYING };
	public GameState gameState = GameState.MENUS;

	enum MenuState { REGULAR, CREDITS, INFO };
	MenuState menuState;
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
	int waveDifferential = 1;

	public GameObject tutorialText;
	public GameObject creditsText;
	public GameObject hintsText;

	public TextMesh exciteText;

	public TextMesh scoreText;
	public TextMesh highScoreText;

	public UIButtonGeneric startButton;
	public UIButtonGeneric creditsButton;
	public UIButtonGeneric infoButton;

	public AudioSource rewardSound;

	public Vector2 oppositeBearsBaseMovementPercent = Vector2.one;

	void Awake()
	{
		worldBoundaries = new Vector4( -7.6f, -7, 10.8f, 4.6f );
		firstWaveAmt = currentWaveAmt;
		Application.targetFrameRate = 60;
		startButton.Pressed += StartButtonPressed; 
		infoButton.Pressed += InfoButtonPressed;
		creditsButton.Pressed += CreditsButtonPressed;
	}

	void Start()
	{

		exciteBear = (ExciteBear)GameObject.FindObjectOfType( typeof( ExciteBear ) );
		exciteText.gameObject.SetActive( false );
		gameState = GameState.MENUS;
		menuState = MenuState.REGULAR;
	}

	public void SpawnNewOppositeBear( int numBearsInGroup )
	{
		if ( numBearsInGroup < 1 )
			numBearsInGroup = 1;
		waveAmtLeft = numBearsInGroup;
		inMiddleOfWave = true;
		amtAtStartOfWave = numBearsInGroup;

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

				oppositeBearsBaseMovementPercent = Vector2.one; //  new Vector2( 1+.05f*(numBearsInGroup-1), 1+.05f*(numBearsInGroup-1) );
				go.GetComponent<OppositeBear>().SetSlowDownPercentDirectly( oppositeBearsBaseMovementPercent );

				if ( i == numBearsInGroup-1 )
				{
					go.GetComponent<OppositeBear>().isLastOfWave = true;

				}
			}
		}
	}

	public void ResetGame()
	{
		StopRewardSound();
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
		oppositeBearsBaseMovementPercent = Vector2.one;
		waveDifferential = 1;

		menuState = MenuState.REGULAR;
		SetButtonsActive( true );
		gameState = GameState.MENUS;

	}

	public void GotHighFive( OppositeBear ob )
	{
		waveAmtLeft--;
		totalScore++;
		scoreText.text = totalScore.ToString();
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
			actualText = "Hexa\n";
			break;
		}
		actualText += "High Five!";

		exciteText.text = actualText;
		exciteText.gameObject.SetActive( true );
		if ( rewardSound )
		{
			if ( !rewardSound.isPlaying )
			{
				double startTime = AudioSettings.dspTime;
				rewardSound.pitch = 1;
				rewardSound.loop = true;
				rewardSound.Play();
			//	rewardSound.SetScheduledEndTime( startTime + amtAtStartOfWave );
			}
			else
			{
				rewardSound.pitch += .125f;
			}
			rewardSound.pitch = 1 + .125f * (amtAtStartOfWave-waveAmtLeft-1);
			rewardSound.SetScheduledEndTime( AudioSettings.dspTime + (amtAtStartOfWave) );
		}
	}

	public void StopRewardSound()
	{
		if ( rewardSound && rewardSound.isPlaying )
			rewardSound.Stop();
	}

	public void FinishedWave()
	{
		if ( inMiddleOfWave )
		{
			inMiddleOfWave = false;
			exciteBear.OnCompleteWave( waveAmtLeft == 0 );
			StopRewardSound();
			if ( waveAmtLeft == 0 )
			{
				if ( amtAtStartOfWave == 1 )
					waveDifferential = 1;

				currentWaveAmt += waveDifferential;

				if ( amtAtStartOfWave == 6 )
				{
					if ( waveDifferential > 0 )
						waveDifferential = 0;
					else
						waveDifferential = -1;
				}
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

	public void StartButtonPressed()
	{
		hintsText.SetActive( false );
		tutorialText.SetActive( false );
		creditsText.SetActive( false );
		SetButtonsActive( false );
		exciteBear.gameObject.SetActive( true );
		exciteBear.Reset();
		gameState = GameState.PLAYING;
		SpawnNewOppositeBear( currentWaveAmt );
	}

	public void CreditsButtonPressed()
	{
		if ( menuState == MenuState.CREDITS )
		{
			hintsText.SetActive( false );
			creditsText.SetActive( false );
			tutorialText.SetActive( true );
			menuState = MenuState.REGULAR;
			exciteBear.gameObject.SetActive( true );
		}
		else
		{
			hintsText.SetActive( false );
			tutorialText.SetActive( false );
			creditsText.SetActive( true );
			menuState = MenuState.CREDITS;
			exciteBear.gameObject.SetActive( false );
		}
	}

	public void InfoButtonPressed()
	{
		if ( menuState == MenuState.INFO )
		{
			creditsText.SetActive( false );
			hintsText.SetActive( false );
			tutorialText.SetActive( true );
			exciteBear.gameObject.SetActive( true );
			menuState = MenuState.REGULAR;
		}
		else
		{
			tutorialText.SetActive( false );
			creditsText.SetActive( false );
			hintsText.SetActive( true );
			exciteBear.gameObject.SetActive( false );
			menuState = MenuState.INFO;
		}
	}

	void SetButtonsActive( bool nowActive )
	{
		startButton.gameObject.SetActive( nowActive );
		creditsButton.gameObject.SetActive( nowActive );
		infoButton.gameObject.SetActive( nowActive );
	}

	public void MouseLifted()
	{
		if ( gameState == GameState.MENUS )
		{
			startButton.MouseLiftedAwayFromButton();
			creditsButton.MouseLiftedAwayFromButton();
			infoButton.MouseLiftedAwayFromButton();
		}
	}
	                          
}
