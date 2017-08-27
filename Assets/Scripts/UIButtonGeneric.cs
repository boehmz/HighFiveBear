using UnityEngine;
using System.Collections;

public class UIButtonGeneric : MonoBehaviour {

	Color tintColor = Color.blue; // new Color( .74f, .43f, .43f, 1f );

	public delegate void DoAction();
	public event DoAction Pressed;

	// Use this for initialization
	void Start () 
	{
		SpriteRenderer sr = GetComponent<SpriteRenderer>();
		if ( sr )
		{
			sr.color = Color.white;
		}

	}

	void OnMouseDown() 
	{
		SpriteRenderer sr = GetComponent<SpriteRenderer>();
		if ( sr )
		{
			sr.color = tintColor;
		}
	}

	void OnMouseUpAsButton()
	{
		SpriteRenderer sr = GetComponent<SpriteRenderer>();
		if ( sr )
		{
			sr.color = Color.white;
		}
		Pressed();
	}

	public void MouseLiftedAwayFromButton()
	{
		SpriteRenderer sr = GetComponent<SpriteRenderer>();
		if ( sr )
		{
			sr.color = Color.white;
		}
	}
}
