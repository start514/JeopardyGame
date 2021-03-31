using Mirror;
using UnityEngine;

public class ClientGameController : NetworkBehaviour
{
    public static ClientGameController instance;
    private TransferDataToGame transferData;
    public UIGameController uiGame;
    public TurnManager turnManager;
    public SyncListGameObject playersInThisMatch = new SyncListGameObject();


    void OnEnable()
    {
        transferData = TransferDataToGame.instance;
        if (instance != null && instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            instance = this;
        }
    }
    public void AddAmount(int amount)
    {
        Player.localPlayer.PlayerAddAmount(amount);
    }
    public void DeductAmount(int amount)
    {
        Player.localPlayer.PlayerDeductAmount(amount);
    }
    public void Test()
    {
        Player.localPlayer.PlayerTest();
    }

    public void SwitchToDoubleJeopardy()
    {
        Player.localPlayer.PlayerOpenDoubleJeopardyPanal();
    }
    public void SwitchToFinalJeopardy()
    {
        Player.localPlayer.PlayerOpenFinalJeopardyPanalToAll();
    }
    public void GiveTurnTo(string who)
    {
        // gives the opportunity to select the next question from the board.
    }
}
