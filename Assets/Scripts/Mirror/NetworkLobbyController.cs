using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;


public class NetworkLobbyController : NetworkBehaviour
{
    public UILobbyController uiLobby;
    // Start is called before the first frame update
    void Start()
    {
        DontDestroyOnLoad(this.gameObject);
        // at start, connect to the server and create client for every user
    }
    public void CheckIfPlayerHasBeenSpawned()
    {
        if (Player.localPlayer == null)
            uiLobby.DisplayErrorMsg("No Player Has Been Spawned");

    }
    public void HostGame()
    {
        Player.localPlayer.PlayerHostGame();
    }
    public void JoinGame()
    {
        Player.localPlayer.PlayerJoinGame(uiLobby.pressedContainerId);
    }
    public void BeginGame()
    {
        Player.localPlayer.PlayerBeginGame();
    }
    public void CancelHost()
    {
        Player.localPlayer.PlayerCancelHosting(Player.localPlayer.matchID);
    }
    public void CancelJoin()
    {
        Player.localPlayer.PlayerCancelJoin(Player.localPlayer.matchID);
    }
    public void ReadyGame()
    {
        Player.localPlayer.PlayerReadyGame();
    }
}
