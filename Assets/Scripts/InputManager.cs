using UnityEngine;
using System.Collections;

public class InputManager : MonoBehaviour {

	static InputManager instance;

	public static InputManager Instance
	{
		get
		{
			if ( instance == null )
			{
				instance = (InputManager)GameObject.FindObjectOfType(typeof(InputManager));
				if ( !instance )
				{
					GameObject go = new GameObject();
					instance = go.AddComponent<InputManager>();
				}
			}
			return instance;
		}
	}

	ExciteBear exciteBear;
	Vector3 mousePos = Vector3.zero;
	Vector3 initialMousePos = Vector3.zero;
	float flickMaxDistance = 1.5f;

	void Start()
	{
		exciteBear = (ExciteBear)GameObject.FindObjectOfType( typeof( ExciteBear ) );
	}

	// Update is called once per frame
	void Update () 
	{
		if ( Input.GetMouseButtonDown( 0 ) )
		{
			initialMousePos = Camera.main.ScreenToWorldPoint( Input.mousePosition );
		}
		else if ( Input.GetMouseButton( 0 ) )
		{
			mousePos = Camera.main.ScreenToWorldPoint( Input.mousePosition );
			if ( ExciteBear.SpriteDistanceApart( mousePos, initialMousePos ) > flickMaxDistance )
			{
				exciteBear.Flick( mousePos - initialMousePos );
			}
		}
		else if ( Input.GetMouseButtonUp( 0 ) )
		{
			mousePos = Camera.main.ScreenToWorldPoint( Input.mousePosition );
			if ( ExciteBear.SpriteDistanceApart( mousePos, initialMousePos ) > flickMaxDistance*.125f )
			{
				exciteBear.Flick( mousePos - initialMousePos );
			}
		}
		
	}
	
}
