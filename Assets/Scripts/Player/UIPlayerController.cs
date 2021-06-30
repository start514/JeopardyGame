using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System;
using UnityEngine;
using TMPro;


public class UIPlayerController : MonoBehaviour
{
    public static UIPlayerController instance;
    public TMP_Text buttonNameText, buttonAmountText;
    private SidePanalController sidePanalController;
    private GameObject myTint;
    //private TransferDataToGame transferDataController;
    // Start is called before the first frame update
    void OnEnable()
    {
        if (instance != null && instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            instance = this;
        }
        Debug.Log("Starting UI player");
        
	    CultureInfo culture = CultureInfo.CreateSpecificCulture("en-US");
	    culture.NumberFormat.CurrencyNegativePattern = 1; 
        culture.NumberFormat.CurrencyDecimalDigits = 0;
	    String str = String.Format(culture, "{0:C}", Player.localPlayer.playerAmount);

        buttonAmountText.text = str;
        buttonNameText.text = Player.localPlayer.playerName;
        /*
        // setting the players name to the one he has put in the input field in the lobby scene
        transferDataController = TransferDataToGame.instance;
        Debug.Log("Player Name " + transferDataController.participanteName);
        player = GameObject.Find("Player").GetComponent<Player>();
        // activate this before building
        player.PlayerName = transferDataController.participanteName;

        if (player.PlayerName.Length < 1)
            player.PlayerName = "Player 1";
        buttonNameText.text = player.PlayerName;
        sideNameText.text = player.PlayerName;

        // umark this after building 
        //sidePanalController.TurnSlotToMine(player.playerIndex);*/
    }
    internal void UpdateMyButtomContainer(int newAmount)
    {
	    CultureInfo culture = CultureInfo.CreateSpecificCulture("en-US");
	    culture.NumberFormat.CurrencyNegativePattern = 1; 
        culture.NumberFormat.CurrencyDecimalDigits = 0;
	    String str = String.Format(culture, "{0:C}", newAmount);
        buttonAmountText.text = str;
    }
    public void TintMySlot()
    {

    }
}
