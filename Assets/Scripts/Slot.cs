using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Slot : MonoBehaviour, IPointerClickHandler {

	public Player Owner;

	public bool IsTaken = false;

	public Action<Slot> OnClicked;

	public Image SlotSymbol;
	public Image SlotImage;

	[SerializeField]
	private SlotCoords _id;

	public SlotCoords ID {
		get { return _id; }
		set { _id = value; }
	}

	void Start() {
		//Grab the Image component in the child
		SlotImage = GetComponent<Image>();
		SlotSymbol = transform.GetChild(0).GetComponent<Image>();
	}

	public void OnPointerClick( PointerEventData eventData ) {
		if ( OnClicked != null )
			OnClicked(this);
	}

	public void SetOwner( Player player ) {
		Owner = player;
		SlotSymbol.enabled = true;
		SlotSymbol.sprite = player.Symbol;
		IsTaken = true;
	}

	public void ResetSlot() {
		Owner = null;
		SlotSymbol.enabled = false;
		IsTaken = false;
	}

	public void ChangeColor( Color backgroundColor , Color symbolColor ) {
		SlotImage.color = backgroundColor;
		SlotSymbol.color = symbolColor;
	}
}


