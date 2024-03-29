﻿using System.Collections;
using System.Collections.Generic;
using System;
using System.Globalization;

using UnityEngine;

public class PlayerSlotsArry : MonoBehaviour
{
    public PlayerSlot[] playerSlots;
    public static PlayerSlotsArry instance;
    void Awake()
    {
        // DontDestroyOnLoad(this);
        if (instance != null && instance != this)
            Destroy(instance);
        else
            instance = this;
    }

    // Update is called once per frame
    void Update()
    {
        UIGameController uiGame = GameObject.FindObjectOfType<UIGameController>();
        var players = GameObject.FindObjectsOfType<Player>();
        System.Array.Reverse(players);

        for(var i = 0; i < TransferDataToGame.instance.gameSize; i++) {
            //initial value as out
            playerSlots[i].amountTxt.text = "Out";
            playerSlots[i].gameObject.SetActive(true);
        }

        for(var i=TransferDataToGame.instance.gameSize; i<playerSlots.Length; i++) {
            playerSlots[i].gameObject.SetActive(false);
        }

        for(var i = 0; i < players.Length; i++) {
            //set slot name/color/bg/active status/amount
            var player = players[i];
            if(player.matchID == Player.localPlayer.matchID && !player.isHost) {
                CultureInfo culture = CultureInfo.CreateSpecificCulture("en-US");
                culture.NumberFormat.CurrencyNegativePattern = 1; 
                culture.NumberFormat.CurrencyDecimalDigits = 0;
                String str = String.Format(culture, "{0:C}", player.playerAmount);

                playerSlots[player.playerIndex].amountTxt.text = str;
                playerSlots[player.playerIndex].nameTxt.text = player.playerName;                
                playerSlots[player.playerIndex].nameTxt.color = uiGame.playerNameColors[player.playerColor];
                playerSlots[player.playerIndex].playerShadowBg.color = uiGame.playerShadowColors[player.playerColor];
                playerSlots[player.playerIndex].playerBodyBg.color = uiGame.playerBodyColors[player.playerColor];
                playerSlots[player.playerIndex].playerAmountBg.color = uiGame.playerAmountBgColors[player.playerColor];
                playerSlots[player.playerIndex].playerAmountShadowBg.color = uiGame.playerShadowColors[player.playerColor];
                playerSlots[player.playerIndex].gameObject.SetActive(true);
            }
        }

        for(var i = 0; i < TransferDataToGame.instance.gameSize; i++) {
            if(playerSlots[i].amountTxt.text == "Out") {
                //if slot is out, make it dimmed
                playerSlots[i].tint.SetActive(true);
                //if dimmed player is card chooser and i am host
                if(TurnManager.instance.cardChooser == i && Player.localPlayer.isHost && !Player.localPlayer.uiGame.winnerPanel.activeSelf) {
                    TurnManager.instance.cardChooser = -1;
                    Player.localPlayer.GiveTurnToRandomPlayer();
                }
            }
        }
    }
}
