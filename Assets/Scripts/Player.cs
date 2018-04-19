using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class Player
{
	[SerializeField]
	private int _id;

	public Sprite Symbol;

	public int ID {
		get { return _id; }
		set { _id = value; }
	}

	public Player(int id) {
		ID = id;
	}
}
