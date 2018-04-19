using UnityEngine;

[System.Serializable]
public class GameEvent {
    public Player _player;
    public Slot _slotClicked;

    public GameEvent(Player player, Slot slot) {
        _player = player;
        _slotClicked = slot;
    }

}