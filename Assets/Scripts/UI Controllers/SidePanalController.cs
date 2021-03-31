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

    private void Start()
    {
        //ActivateSlots();    
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
            if (i != PlayerNumWhoBuzzed)
                playerSlotsArry.playerSlots[i].GetComponent<PlayerSlot>().tint.SetActive(true);
        }
    }
    public void UntintAll()
    {
        for (int i = 0; i < playerSlotsArry.playerSlots.Length; i++)
        {
            playerSlotsArry.playerSlots[i].GetComponent<PlayerSlot>().tint.SetActive(false);
        }
    }


    public void ActivateSlots()
    {
        int numOfPlayers = TransferDataToGame.instance.gameSize;
        if (numOfPlayers == 4)
            Instantiate(fourPlayerSlotsPrefab, slotsParent.transform);
        if (numOfPlayers == 5)
            Instantiate(fivePlayerSlotsPrefab, slotsParent.transform);
        if (numOfPlayers == 6)
            Instantiate(sixPlayerSlotsPrefab, slotsParent.transform);
        playerSlotsArry = PlayerSlotsArry.instance;
    }

    public void TurnSlotToMine(int slotNum)
    {
        //playerSlotsArry.playerSlots[slotNum];
    }
}
