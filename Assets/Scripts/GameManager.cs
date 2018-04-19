using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour {
    public List<GameEvent> GameEvents = new List<GameEvent>();
    public GridLayoutGroup GameGridLayoutGroup;
    public string GameOverMessage;

    public Text GameOverText;
    public Color HighlightColor, OriginalColor;
    public Player Player1, Player2;
    public Image Player1Hud, Player2Hud;
    public Image Player1Panel, Player2Panel;
    public GameObject RestartPanel;
    public List<Slot> SlotList = new List<Slot>();
    public GameObject SlotPrefab;
    public Sprite[] Symbols;
    public Image Player1Arrow, Player2Arrow;

    public static GameManager Instance { get; private set; }
    public GameState GameState { get; set; }
    public PlayerState PlayerState { get; set; }
    public int RowCount { get; set; }

    private void Awake() {
        Instance = this;
        InitPlayers();
    }

    private void InitPlayers() {
        //Setup the player symbols
        Player1 = new Player(1);
        Player2 = new Player(2);
        Player1Panel.color = HighlightColor;
        Player2Panel.color = OriginalColor;
        Player1Arrow.enabled = true;
        Player2Arrow.enabled = false;
        SwitchTurn(PlayerState.Player1Turn);
        RandomizeSymbols();
    }

    private void RandomizeSymbols() {
        var randomIndex = Random.Range(0 , Symbols.Length);
        Player1.Symbol = Symbols[randomIndex];
        Player1Hud.sprite = Player1.Symbol;

        //pick a different symbol than player 1
        var newRandom = RandomExclude(0 , Symbols.Length , randomIndex);
        Player2.Symbol = Symbols[newRandom];
        Player2Hud.sprite = Player2.Symbol;
    }

    public void CreateGrid( int rows ) {
        //set the Row count in the grid layout
        RowCount = rows;
        GameGridLayoutGroup.constraintCount = rows;
        StartCoroutine(SlotAnimation());

        for ( var x = 0; x < rows; x++ )
            for ( var y = 0; y < rows; y++ ) {
                var slotGameObject = Instantiate(SlotPrefab);
                slotGameObject.transform.SetParent(GameGridLayoutGroup.transform);

                var slotComponent = slotGameObject.GetComponent<Slot>();
                if ( slotComponent ) {
                    slotComponent.OnClicked += OnSlotClicked;
                    slotComponent.ID = new SlotCoords(x , y);
                    slotGameObject.name = "Slot " + slotComponent.ID.x + ":" + slotComponent.ID.y;
                    SlotList.Add(slotComponent);
                }
            }
    }


    /// <summary>
    ///     This method writes the turn history file into the Resources folder.
    /// </summary>
    public void WriteTurnHistory() {
        var path = Application.dataPath + "/Resources/History.txt";
        var writer = new StreamWriter(path , false);

        foreach ( var gameEvent in GameEvents )
            writer.WriteLine("Player " + gameEvent._player.ID + " clicked on slot (" + gameEvent._slotClicked.ID.x +
                             "," + gameEvent._slotClicked.ID.y + ")");
        writer.Close();
    }

    /// <summary>
    ///     Adds a msg to the history file
    /// </summary>
    /// <param name="msg"></param>
    public void AddToHistory( string msg ) {
        var path = Application.dataPath + "/Resources/History.txt";
        var writer = new StreamWriter(path , true);
        writer.WriteLine(msg);
        writer.Close();
    }

    /// <summary>
    ///     When a slot is clicked this will add a symbol, and check for a win.
    /// </summary>
    /// <param name="slot"></param>
    private void OnSlotClicked( Slot slot ) {
        if ( slot.IsTaken )
            return;

        switch ( PlayerState ) {
            case PlayerState.Player1Turn:
                slot.SetOwner(Player1);
                GameEvents.Add(new GameEvent(Player1 , slot));

                SwitchTurn(PlayerState.Player2Turn);
                break;

            case PlayerState.Player2Turn:
                slot.SetOwner(Player2);
                GameEvents.Add(new GameEvent(Player2 , slot));

                SwitchTurn(PlayerState.Player1Turn);
                break;
        }

        CheckForWin(slot);
    }


    private void SwitchTurn( PlayerState player ) {
        switch ( player ) {
            case PlayerState.Player1Turn:
                Player1Panel.color = HighlightColor;
                Player2Panel.color = OriginalColor;
                Player2Arrow.enabled = false;
                Player1Arrow.enabled = true;
                break;
            case PlayerState.Player2Turn:
                Player1Panel.color = OriginalColor;
                Player2Panel.color = HighlightColor;
                Player1Arrow.enabled = false;
                Player2Arrow.enabled = true;
                break;
        }
        PlayerState = player;
    }

    /// <summary>
    ///     This method will check the slots ID for horizontal,
    ///     vertical, and diagonal matches.
    ///     If all slots are filled it will end in a draw.
    /// </summary>
    /// <param name="slotRef"></param>
    private void CheckForWin( Slot slotRef ) {
        //check for diagonal between top left and bottom right
        var diagonal = SlotList
            .Where(o => o.ID.y == o.ID.x)
            .ToList();

        if ( diagonal.All(o => o.Owner == slotRef.Owner) ) {
            foreach ( var slot in SlotList ) {
                slot.GetComponent<Image>().color = new Color(0 , 0 , 0 , .5f);
                slot.SlotSymbol.color = new Color(0 , 0 , 0 , 1);
            }

            foreach ( var slot in diagonal )
                slot.SlotSymbol.color = new Color(1f , 1f , 1f , 1f);
            WinGame(slotRef.Owner);
            return;
        }

        //check diagonal between top right and bottom left
        var diag2 = new List<Slot>();
        //other diagonal
        diag2 = RowCount == 4
            ? new List<Slot>
            {
                SlotList[RowCount - 1],
                SlotList[RowCount + RowCount / 2],
                SlotList[3 * RowCount - (RowCount - 1)],
                SlotList[RowCount * 3]
            }
            : new List<Slot>
            {
                SlotList[2],
                SlotList[4],
                SlotList[6]
            };

        if ( diag2.All(o => o.Owner == slotRef.Owner) ) {
            foreach ( var slot in SlotList ) {
                slot.GetComponent<Image>().color = new Color(0 , 0 , 0 , .5f);
                slot.SlotSymbol.color = new Color(0 , 0 , 0 , 1);
            }

            foreach ( var slot in diag2 )
                slot.SlotSymbol.color = Color.white;
            WinGame(slotRef.Owner);
        }

        //check horizontals
        var horizontal = SlotList.Where(o => o.ID.x == slotRef.ID.x).ToList();
        if ( horizontal.All(x => x.Owner == slotRef.Owner) ) {
            foreach ( var slot in SlotList ) {
                slot.GetComponent<Image>().color = new Color(0 , 0 , 0 , .5f);
                slot.SlotSymbol.color = new Color(0 , 0 , 0 , 1);
            }

            foreach ( var slot in horizontal )
                slot.SlotSymbol.color = new Color(1 , 1 , 1 , 1);
            WinGame(slotRef.Owner);
            return;
        }

        //check verticals
        var vertical = SlotList.Where(y => y.ID.y == slotRef.ID.y).ToList();
        if ( vertical.All(y => y.Owner == slotRef.Owner) ) {
            foreach ( var slot in SlotList ) {
                slot.GetComponent<Image>().color = new Color(0 , 0 , 0 , .5f);
                slot.SlotSymbol.color = new Color(0 , 0 , 0 , 1);
            }

            foreach ( var slot in vertical )
                slot.SlotSymbol.color = Color.white;

            WinGame(slotRef.Owner);
            return;
        }

        //check for a draw
        var draw = SlotList.TrueForAll(s => s.IsTaken);
        if ( draw )
            EndGameAsDraw();
    }

    public void EndGameAsDraw() {
        //game was a draw
        WriteTurnHistory();
        AddToHistory("The game ended in a draw!");

        ShowRestartPanel();
        GameOverMessage = "Draw!";
        GameOverText.text = GameOverMessage;
        PlayerState = PlayerState.Player1Turn;
    }

    private void ClearSlots() {
        foreach ( var slot in SlotList )
            Destroy(slot.gameObject);
        SlotList.Clear();
    }

    public void ShowRestartPanel() {
        RestartPanel.SetActive(true);
    }

    public void WinGame( Player slotRefOwner ) {
        WriteTurnHistory();
        AddToHistory("Player " + slotRefOwner.ID + " won the game!");
        ShowRestartPanel();
        GameOverMessage = "Player " + slotRefOwner.ID + " has won!";
        GameOverText.text = GameOverMessage;
        PlayerState = PlayerState.Player1Turn;
    }

    /// <summary>
    ///     Returns random number,excluding one
    /// </summary>
    /// <param name="min"></param>
    /// <param name="max"></param>
    /// <param name="exclude"></param>
    /// <returns></returns>
    private int RandomExclude( int min , int max , int exclude ) {
        var excludes = new HashSet<int> { exclude };
        var range = Enumerable.Range(min , max).Where(i => !excludes.Contains(i));

        var rand = new System.Random();
        var index = rand.Next(min , max - excludes.Count);
        return range.ElementAt(index);
    }

    public void RestartGame() {
        SwitchTurn(PlayerState.Player1Turn);
        RandomizeSymbols();
        ClearSlots();
    }

    public IEnumerator SlotAnimation() {
        float startSpacing = -128f;
        var currentLerpTime = 0f;
        var lerpTime = 1f;

        while ( startSpacing < 0 ) {
            currentLerpTime += Time.deltaTime;
            if ( currentLerpTime > lerpTime )
                currentLerpTime = lerpTime;

            float percent = currentLerpTime / lerpTime;

            startSpacing = Mathf.Lerp(startSpacing , 0 , percent);

            GameGridLayoutGroup.spacing = new Vector2(startSpacing , startSpacing);
            yield return null;
        }
        //just to make sure.
        GameGridLayoutGroup.spacing = Vector2.zero;
    }
}