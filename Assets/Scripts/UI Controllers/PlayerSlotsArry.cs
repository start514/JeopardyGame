using System.Collections;
using System.Collections.Generic;
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
        var idx = 0;
        var players = GameObject.FindObjectsOfType<Player>();
        for(var i = 0; i < players.Length; i++) {
            var player = players[i];
            if(player.matchID == Player.localPlayer.matchID && !player.isHost) {
                playerSlots[idx].amountTxt.text = player.playerAmount.ToString("C0");
                playerSlots[idx].nameTxt.text = player.playerName;                
                playerSlots[idx].nameTxt.color = uiGame.playerNameColors[player.playerColor];
                playerSlots[idx].playerShadowBg.color = uiGame.playerShadowColors[player.playerColor];
                playerSlots[idx].playerBodyBg.color = uiGame.playerBodyColors[player.playerColor];
                playerSlots[idx].playerAmountBg.color = uiGame.playerAmountBgColors[player.playerColor];
                playerSlots[idx].playerAmountShadowBg.color = uiGame.playerShadowColors[player.playerColor];
                playerSlots[idx].gameObject.SetActive(true);
                idx++;
            }
        }
        for(var i=idx; i<playerSlots.Length; i++) {
            playerSlots[i].gameObject.SetActive(false);
        }
    }
}
