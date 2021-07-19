using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SidePanalController : MonoBehaviour
{

    public static SidePanalController instance;
    public PlayerSlotsArry playerSlotsArry;
    public GameObject fourPlayerSlotsPrefab, fivePlayerSlotsPrefab, sixPlayerSlotsPrefab, slotsParent;
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
    }
    
    internal void UpdateSideSlotAmount(int slotNum, int newAmount)
    {
        this.playerSlotsArry.playerSlots[slotNum].amountTxt.text = "$" + newAmount.ToString();
    }
    public void TintOneSlot(int PlayerNumWhoBuzzed, bool tint)
    {
        playerSlotsArry.playerSlots[PlayerNumWhoBuzzed].GetComponent<PlayerSlot>().tint.SetActive(tint);
    }


    public void TintAllSlotsButOne(int PlayerNumWhoBuzzed)
    {
        for (int i = 0; i < playerSlotsArry.playerSlots.Length; i++)
        {
            //tint selected player index card
            playerSlotsArry.playerSlots[i].GetComponent<PlayerSlot>().tint.SetActive(i != PlayerNumWhoBuzzed);
        }
        //activate/deactivate if slot is not host
        //if "but one" is me
        if(PlayerNumWhoBuzzed >= 0 && Player.localPlayer.playerIndex == PlayerNumWhoBuzzed)
            Player.localPlayer.uiGame.ActivateSlots();
        else
            Player.localPlayer.uiGame.DeactivateSlots();
    }
    public void UntintAllExceptAnswered()
    {
        for (int i = 0; i < playerSlotsArry.playerSlots.Length; i++)
        {
            var players = GameObject.FindObjectsOfType<Player>();
            var answered = false;
            foreach(var player in players) {
                //if someone has answered
                if(player.matchID == Player.localPlayer.matchID && player.playerIndex == i && player.hasAnswered) {
                    answered = true;
                }
            }
            playerSlotsArry.playerSlots[i].GetComponent<PlayerSlot>().tint.SetActive(answered);
        }
    }

    public void TurnSlotToMine(int slotNum)
    {
        //playerSlotsArry.playerSlots[slotNum];
    }
}
