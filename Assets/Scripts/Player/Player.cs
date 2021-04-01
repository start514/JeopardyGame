using UnityEngine;
using Mirror;
using System;
using UnityEngine.SceneManagement;

public class Player : NetworkBehaviour
{
    public static Player localPlayer = null;

    [Header("Name/Color/Amount/isHost")]
    [SyncVar] public string playerName = "";
    [SyncVar] public int playerColor = 0;
    [SyncVar] public int playerAmount = 0;
    [SyncVar] public bool isHost = false;
    [SyncVar] public bool isReady = false;
    [Header("Match/ID/Answered")]
    [SyncVar] public string matchID;
    [SyncVar] public string playerID;
    [SyncVar] public bool hasAnswered;
    int playerIndex;
    private UILobbyController uiLobby;

    [Header("Game")]
    UIPlayerController uiPlayer;
    NetworkMatchChecker matchChecker;
    internal UIGameController uiGame;
    internal bool isSumbiting,isBuzzing, canDecide, canContinue;

    void Start()
    {
        // delete later 
        //using singleton for the local player
        // notice is client here 
        if (isLocalPlayer)
            localPlayer = this;

        DontDestroyOnLoad(this.gameObject);
        if (isLocalPlayer)
        {
            this.playerID = CreateRandomID();
            CmdUpdatePlayerId(this.playerID);
            this.uiLobby = GameObject.Find("UI Lobby Controller").GetComponent<UILobbyController>();
            this.playerAmount = 0;
            if (isClient && this.uiLobby != null && this.uiLobby.lobbyPanal.activeSelf)
                CmdUpdateGameRoomList();
            // for testing purpeses, if we are startubg from the game and not the lobby
            else
            {
                this.uiGame = GameObject.Find("UI Game Controller").GetComponent<UIGameController>();
                StartUIIfConnected.instance.ActivateUI();
            }
        }
    }
    void OnEnable()
    {
        // change after demo to check who is the local player
        this.playerAmount = 0;
        this.isHost = false;
        this.hasAnswered = false;
    }
    string CreateRandomID()
    {
        string id = "";
        for (int i = 0; i < 10; i++)
        {
            int rnd = UnityEngine.Random.Range(0, 36);
            if (rnd < 26)
            // meaning it's a letter
            {
                id += (char)(rnd + 65);
            }
            else
            // its a numer
            {
                id += (rnd - 26).ToString(); // subtracting to make it into a number 
            }
        }
        Debug.Log("Random ID is " + id);
        return id;
    }

    #region PASSING VALUES TO SERVER THEN THE SYNC VAR TRANSFERS THE DATA TO ALL CLIENTS METHODS
    // syncvar are  synchronized from the server to clients
    [Command]
    void CmdUpdatePlayerId(string newId)
    {
        this.playerID = newId;
    }
    [Command]
    void CmdUpdateMatchID(string newId)
    {
        this.matchID = newId;
    }
    [Command]
    void CmdUpdateIsHost(bool host, string matchId, string playerId)
    {
        this.isHost = host;
        SyncListGameObject players = MatchMaker.instance.FindMatchById(matchId).playersInThisMatch;
        for (int i = 0; i < players.Count; i++)
        {
            if (players[i].GetComponent<Player>().playerID == playerId)
            {
                MatchMaker.instance.FindMatchById(matchId).playersInThisMatch[i].GetComponent<Player>().isHost = host;
                Debug.LogError("Sucsesfully changed player has answered in matchmaker for id " + matchID);
            }
        }
    }

    #endregion
    #region HOST
    public void PlayerHostGame()
    {
        string id = MatchMaker.CreateRandomID();
        string gameName = uiLobby.gameNameIP.text;
        if (string.IsNullOrEmpty(gameName))
        {
            gameName = "Game #" + (uiLobby.CountGameContainers() + 1).ToString();
            //Debug.LogError("No game name", this);
        }
        CmdHostGame(id, int.Parse(uiLobby.gameSizeTxt.text), gameName, localPlayer.gameObject);
    }
    public void ChangeColor() {
        CmdChangeColor(this.matchID, this.playerID, this.playerColor);
    }
    [Command]
    void CmdChangeColor(string matchID, string playerID, int playerColor) {
        Match match = MatchMaker.instance.FindMatchById(matchID);
        for(int i=0; i<match.playersInThisMatch.Count; i++) {
            TargetCmdChangeColor(match.playersInThisMatch[i].GetComponent<NetworkIdentity>().connectionToClient, matchID, playerID, playerColor);
        }
    }
    [TargetRpc]
    void TargetCmdChangeColor(NetworkConnection target, string matchID, string playerID, int playerColor) {
        if(localPlayer.playerID != playerID || localPlayer.matchID != matchID) {
            Debug.LogError(playerID + "'s color = " + playerColor, this);
        }
    }
    [Command] // call this from a client to run it on the server
    // the problem was that we were passing in the player clone that was on the server and not the local player that called the method
    void CmdHostGame(string id, int gameSize, string gameName, GameObject player)
    {
        // tell the server we got an id, please register a new game and add this player to the list
        if (MatchMaker.instance.AddAndApproveHostingGame(id, gameName, gameSize, player.gameObject))
        // if valadation passed
        {
            this.playerColor = -1;
            this.isReady = true;
            this.playerName = "";
            CountParticipentContainers(id);
            TargetHostGame(true, id, gameSize, gameName, MatchMaker.RegularIDToGUI(id));
            Debug.Log("Server - Sucssesfly hosted a game");
            // add a game container
        }
        else
        {
            TargetHostGame(false, id, gameSize, gameName, MatchMaker.RegularIDToGUI(id));
            Debug.LogError("Server- Couldn't host game", this);
        }
    }

    [TargetRpc] // the server will run this on a specific client
    void TargetHostGame(bool success, string id, int gameSize, string gameName, Guid guid)
    {
        if (success)
        {
            this.playerIndex = 1;
            localPlayer.isHost = true;
            CmdUpdateIsHost(true, id, localPlayer.matchID);
            CmdAddGameContainer(id, gameName, gameSize, guid);
            localPlayer.matchID = id;
            CmdUpdateMatchID(localPlayer.matchID);
            GameObject transferData = Instantiate(localPlayer.uiLobby.transferDataToGamePrefab);
            transferData.name = "Transfer Data";
            localPlayer.uiLobby.NewGameStartGameButton();
            Debug.Log("Client - Sucssesfly hosted a game with name " + TransferDataToGame.instance.gameName + " and id " + id + " and participent number " + TransferDataToGame.instance.gameSize.ToString());
        }
        else
        {
            localPlayer.matchID = null;
            CmdUpdateMatchID(localPlayer.matchID);
            Debug.LogError("Client- Player couldnt host game");
        }
    }
    public void PlayerCancelHosting(string id)
    {
        CmdCancelHost(id);
    }
    [Command] // call this from a client to run it on the server
    void CmdCancelHost(string id)
    {
        Debug.Log("CmdCancelhost");
        // tell the server to tell all clients to update 
        RpcDeleteGameContainer(id);
        SyncListGameObject players = MatchMaker.instance.FindMatchById(id).playersInThisMatch;
        for (int i = 0; i < players.Count; i++)
        {
            TargetPlayerLeaveGameFromLobby(players[i].GetComponent<NetworkIdentity>().connectionToClient);
        }
        // tell the server we got an id, please register a new game and add this player to the list 

        if (MatchMaker.instance.DeleteMatch(id))
        // if valadation passed
        {
            TargetCancelHost(true, id);
            RcpCountGameContainers();
            Debug.Log("Server - Sucssesfly canceled a hosting");
        }
        else
        {
            TargetCancelHost(false, id);
            Debug.LogError("Server- Couldn't host game", this);
        }
    }

    [TargetRpc] // the server will run this on a specific client
    void TargetCancelHost(bool success, string id)
    {
        if (success)
        {

            this.playerIndex = -1;
            //localPlayer.gameObject.GetComponent<NetworkMatchChecker>().matchId = new Guid();
            //CmdUpdateMatchChecker(new Guid());
            localPlayer.matchID = null;
            CmdUpdateMatchID(localPlayer.matchID);
            localPlayer.uiLobby.hostGamePanal.SetActive(false);
            localPlayer.uiLobby.lobbyPanal.SetActive(true);
            localPlayer.uiLobby.CountGameContainers();
        }
        else
        {
            Debug.LogError("Client- Player couldnt  delete host");
        }
    }

    [ClientRpc]
    void RcpCountGameContainers()
    {
        localPlayer.uiLobby.CountGameContainers();

    }
    #endregion

    #region JOIN
    public void PlayerJoinGame(string idToJoin)
    {
        CmdJoinGame(idToJoin);
    }

    [Command] // call this from a client to run it on the server
    void CmdJoinGame(string id)
    // tell the server we got an id, please register a new game and add this player to the list 
    {
        if (MatchMaker.instance.AddAndApproveJoiningGame(id, this.gameObject))
        // if valadation passed
        {            
            SyncListGameObject players = MatchMaker.instance.FindMatchById(id).playersInThisMatch;

            int colorChosen = 0;
            bool duplicate = false;
            do {
                duplicate = false;
                for (int i = 0; i < players.Count; i++)
                {
                    if (this.playerID != players[i].GetComponent<Player>().playerID && colorChosen == players[i].GetComponent<Player>().playerColor) // if it is not me and has same color as me
                    {
                        duplicate = true;
                        colorChosen++;
                    }
                }
            } while(duplicate);
            this.playerColor = colorChosen;
            this.isReady = false;
            this.playerName = "";

            Debug.LogError(MatchMaker.instance.FindMatchById(id).playersInThisMatch.Count);
            Debug.Log("Server- Sucssesfly joined game");
            TargetJoinGame(true, id, MatchMaker.instance.FindMatchById(id), MatchMaker.RegularIDToGUI(id), MatchMaker.instance.FindMatchById(id).playersInThisMatch.Count - 1);
            CountParticipentContainers(id);
        }
        else
        {
            Debug.LogError("Server- Couldn't join game", this);
            TargetJoinGame(false, id, MatchMaker.instance.FindMatchById(id), MatchMaker.RegularIDToGUI(id), MatchMaker.instance.FindMatchById(id).playersInThisMatch.Count - 1);
        }
    }

    [TargetRpc] // the server will run this on a specific client
    void TargetJoinGame(bool success, string id, Match match, Guid GuiId, int index)
    {
        if (success)
        {
            this.playerIndex = index;
            //localPlayer.gameObject.GetComponent<NetworkMatchChecker>().matchId = GuiId;
            //CmdUpdateMatchChecker(GuiId);
            localPlayer.matchID = id;
            CmdUpdateMatchID(localPlayer.matchID);
            Debug.Log("Client- Game joining was sucsesfull with id " + id.ToString());
            localPlayer.uiLobby.OpenJoinPanalWithId(index,match.maxGameSize);
            CmdAddJoinContainerLocally(id, localPlayer.playerID);

            // add my contaniner to the host 
            CmdAddHostContainer(id, localPlayer.playerID);
            // add my container to the other players who have joined
            CmdAddJoinContainerToPlayersWhoAlreadyJoined(id, localPlayer.playerID);
            uiLobby.ChangeGamecontainerParticipentNumTxt(id, match.playersInThisMatch.Count, match.maxGameSize);
        }
        else
        {
            Debug.Log("Client- Game joining was not sucsesful");
        }
    }
    [Command]
    void CmdAddJoinContainerLocally(string id, string playerId)
    {
        SyncListGameObject players = MatchMaker.instance.FindMatchById(id).playersInThisMatch;
        int numOfPlayers = players.Count;
        // calling the target rpc to instatiate the needed join containers for ON MY SCREEN ONLY 
        for (int i = 0; i < numOfPlayers; i++)
        {
            if (players[i].GetComponent<Player>().isHost == true)
            {
                string hostName = players[i].GetComponent<Player>().playerName;
                int color = players[i].GetComponent<Player>().playerColor;
                if (string.IsNullOrEmpty(hostName))
                    hostName = "Host";
                TargetAddJoinContainerLocally(id, true, hostName, color, players[i].GetComponent<Player>().playerID);
                break;
            }
        }
        TargetAddJoinContainerLocallyWithIF(id, this.playerColor);
        for (int i = 0; i < numOfPlayers; i++)
        {
            if (players[i].GetComponent<Player>().isHost == false && players[i].GetComponent<Player>().playerID != playerId)
            {
                string playerName = players[i].GetComponent<Player>().playerName;
                int color = players[i].GetComponent<Player>().playerColor;
                if (string.IsNullOrEmpty(playerName))
                    playerName = "Player";
                TargetAddJoinContainerLocally(id, false, playerName, color, players[i].GetComponent<Player>().playerID);
            }
        }
    }
    [Command]
    void CmdAddJoinContainerToPlayersWhoAlreadyJoined(string id, string playerId)
    {
        SyncListGameObject players = MatchMaker.instance.FindMatchById(id).playersInThisMatch;

        for (int i = 0; i < players.Count; i++)
        {
            if (playerId != players[i].GetComponent<Player>().playerID) // if it is not me 
                TargetCmdAddJoinContainerToPlayersWhoAlreadyJoined(players[i].GetComponent<NetworkIdentity>().connectionToClient, id, this.playerColor, playerId);
        }
    }
    [TargetRpc]
    void TargetCmdAddJoinContainerToPlayersWhoAlreadyJoined(NetworkConnection target, string id, int color, string playerId)
    {
        localPlayer.uiLobby.AddJoinParticipentConteiner(id, false, "Player", color, playerId);

    }
    
    [TargetRpc]
    void TargetAddJoinContainerLocally(string id, bool host, string playerName, int color, string playerId)
    {
        Debug.Log(playerName + "'s color = " + color);
        localPlayer.uiLobby.AddJoinParticipentConteiner(id, host, playerName, color, playerId);

    }

    [TargetRpc]
    void TargetAddJoinContainerLocallyWithIF(string id, int color)
    {
        localPlayer.uiLobby.AddJoinParticipentConteinerWithInput(id, color, localPlayer.playerID);

    }
    public void PlayerCancelJoin(string id)
    {
        CmdCancelJoin(id, this.playerID);
    }
    [Command] // call this from a client to run it on the server
    void CmdCancelJoin(string id, string playerId)
    {
        // tell the server we got an id, please register a new game and add this player to the list 
        if (MatchMaker.instance.RemovePlayerFromMatch(id, this))
        // if valadation passed
        {
            DeleteParticipentContainer(id, playerId);
            CountParticipentContainers(id);
            this.playerIndex = -1;
            TargetCancelJoin(true, id, MatchMaker.instance.FindMatchById(id).playersInThisMatch.Count, MatchMaker.instance.FindMatchById(id).maxGameSize);
            Debug.Log("Server - Sucssesfly canceled a joining");
        }
        else
        {
            TargetCancelJoin(false, id, MatchMaker.instance.FindMatchById(id).playersInThisMatch.Count, MatchMaker.instance.FindMatchById(id).maxGameSize);
            Debug.LogError("Server- Couldn't cancel join game", this);
        }
    }

    [TargetRpc] // the server will run this on a specific client
    void TargetCancelJoin(bool success, string id, int currentPlayerCount, int maxGameSize)
    {
        if (success)
        {
            //localPlayer.gameObject.GetComponent<NetworkMatchChecker>().matchId = new Guid();
            //CmdUpdateMatchChecker(new Guid());
            Debug.Log($"Client - Sucssesfly cancaled joining");
            localPlayer.uiLobby.hostGamePanal.SetActive(false);
            localPlayer.uiLobby.joinGamePanal.SetActive(false);
            localPlayer.uiLobby.lobbyPanal.SetActive(true);
            localPlayer.matchID = null;
            localPlayer.uiLobby.ClearJoinContainers();
            CmdUpdateMatchID(localPlayer.matchID);
            uiLobby.ChangeGamecontainerParticipentNumTxt(id, currentPlayerCount, maxGameSize);
            //Player.localPlayer.CmdUpdateGameRoomList();

            /*// if we were supposed to be the last player and the match maker script has deleted the game container, add it back in
            if (match.playersInThisMatch.Count == match.maxGameSize)
            {
                CmdAddGameContainer(id, match.gameName, match.playersInThisMatch.Count, match.maxGameSize);
            }*/
        }
        else
        {
            Debug.LogError("Client- Player couldnt  delete host");
        }
    }


    #endregion

    #region I AM READY
    public void PlayerReadyGame()
    {
        if(playerName == "") return;
        CmdReadyGame();
    }

    [Command]
    void CmdReadyGame() {
        this.isReady = true;
        Match match = MatchMaker.instance.FindMatchById(this.matchID);
        bool gameFull = (match.playersInThisMatch.Count - 1 == match.maxGameSize);
        bool allReady = true;
        for(var i=0; i<match.playersInThisMatch.Count; i++) {
            if(match.playersInThisMatch[i].GetComponent<Player>().isReady == false) {
                allReady = false;
            }
        }
        RpcReadyGame(this.playerID, this.matchID, gameFull, allReady);
    }
    [ClientRpc]
    void RpcReadyGame(string playerID, string matchID, bool gameFull, bool allReady) {
        if(localPlayer.matchID != matchID) return; // message from other match

        if(playerID == localPlayer.playerID) {//ready player is me
            localPlayer.uiLobby.updateJoinPanelIAmReady();
        }
        else {//other player has ready button clicked
            if(localPlayer.isHost) {//i am host
                localPlayer.uiLobby.updateHostPanelPlayerReady(gameFull, allReady);
            } else {//i am joiner
                localPlayer.uiLobby.updateJoinPanelPlayerReady(playerID);
            }
        }
        Debug.LogError($"Player {playerID}/{localPlayer.playerID} ready", this);
    }
    #endregion

    #region BEGIN GAME
    public void PlayerBeginGame()
    {
        Debug.Log("Begine");
        bool dailyDouble = false;
        if (uiLobby.dailyDoybleTxt.text == "Enable")
            dailyDouble = true;
        int timeToAnswer = int.Parse(uiLobby.timeToAnswerTxt.text);
        int timeToBuzz = int.Parse(uiLobby.timeToBuzzTxt.text);
        CmdBeginGame(localPlayer.matchID, dailyDouble, timeToAnswer, timeToBuzz);
    }
    [Command] // call this from a client to run it on the server
    void CmdBeginGame(string id, bool dailyDouble, int timeToAnswer, int timeToBuzz)
    {
        //MatchMaker.instance.BegineGame(matchID);
        Match thisMatch = MatchMaker.instance.FindMatchById(id);
        for (int i = 0; i < thisMatch.playersInThisMatch.Count; i++)
        {
            TargetBeginGame(thisMatch.playersInThisMatch[i].GetComponent<NetworkIdentity>().connectionToClient, MatchMaker.RegularIDToGUI(id));
        }
        RpcDeleteGameContainer(id);
        if (MatchMaker.instance.DeleteMatch(id))
        // if valadation passed
        {
            RcpCountGameContainers();
            Debug.Log("Server - Sucssesfly canceled a hosting");
        }
        else
        {
            Debug.LogError("Server- Couldn't host game", this);
        }
    }

    [TargetRpc] // the server will run this on a specific client
    void TargetBeginGame(NetworkConnection target, Guid guid)
    {
        // instatiate a match chcker with the correct id
        this.matchChecker = localPlayer.gameObject.AddComponent(typeof(NetworkMatchChecker)) as NetworkMatchChecker;
        this.matchChecker.matchId = guid;
        // spawn a turn manager
        //GameObject turnManager = Instantiate(localPlayer.uiLobby.turnMangerPrefab);
        //TurnManager turnManagerScript = turnManager.GetComponent<TurnManager>();
        //turnManagerScript.AddPlayer(localPlayer);
        //turnManager.GetComponent<NetworkMatchChecker>().matchId = RegularIDToGUI(id);
        // take all players to the game
        SceneManager.LoadScene("PlayerBoard");
    }

    public void DistributeDataForFirstBoard() {
        if(isHost)
            CmdDistributeDataForFirstBoard();
    }


    #endregion

    #region CONTAINERS
    [Command]
    internal void CmdAddGameContainer(string id, string gameName, int maxPlayers, Guid guid)
    {
        Debug.Log("cmdCmdAddGameContainer");
        RpcAddGameContainer(id, gameName, maxPlayers);
        //TargetChangeMatchMakerMatchId(guid);
    }
    [ClientRpc]
    private void RpcAddGameContainer(string id, string gameName, int maxPlayers)
    {
        Debug.Log("RpcAddGameContainer");
        localPlayer.uiLobby.AddGameContainer(id, gameName, 1, maxPlayers);
        Debug.LogError(isServer + " Rpc AddGameContainer");
    }
    /*
    [TargetRpc]
    void TargetChangeMatchMakerMatchId(Guid guid)
    {
        Debug.LogError("Changing match checker id");
        localPlayer.gameObject.GetComponent<NetworkMatchChecker>().matchId = guid;
        CmdUpdateMatchChecker(guid);
    }
    [Command]
    void CmdUpdateMatchChecker(Guid GuiId)
    {
        this.gameObject.GetComponent<NetworkMatchChecker>().matchId = GuiId;

    }*/

    [ClientRpc]
    private void RpcDeleteGameContainer(string id)
    {
        localPlayer.uiLobby.DeleteGameContainer(id);
    }
    [TargetRpc]
    void TargetPlayerLeaveGameFromLobby(NetworkConnection target)
    {
        localPlayer.uiLobby.joinGamePanal.SetActive(false);
        localPlayer.uiLobby.lobbyPanal.SetActive(true);
        localPlayer.uiLobby.ClearHostContainers();
        localPlayer.uiLobby.ClearJoinContainers();
    }

    [Command]
    void CmdAddJoinContainer(string id, string playerId)
    {
        // what to do- 
        // each time a player has joined with this id
        // call a client rcp to each one of the game participent in this id, telling them a player has joined or left 
        Debug.Log(MatchMaker.instance.FindMatchById(id) == null);
        Debug.Log(id);
        SyncListGameObject playersInThisMatch = MatchMaker.instance.FindMatchById(id).playersInThisMatch;
        Debug.Log(playersInThisMatch.Count);
        for (int i = 0; i < playersInThisMatch.Count; i++)
        {
            // add my contaniner to everyone else
            Debug.Log(playersInThisMatch[i].GetComponent<NetworkIdentity>().connectionToClient);
            TargetAddJoinParticipentContainer(playersInThisMatch[i].GetComponent<NetworkIdentity>().connectionToClient, id, playersInThisMatch[i].GetComponent<Player>().playerColor, playerId);
        }
    }


    [TargetRpc]
    private void TargetAddJoinParticipentContainer(NetworkConnection target, string id, int color, string playerId)
    {
        Debug.LogError(" target rpc adding host participent " + localPlayer.playerID);
        localPlayer.uiLobby.AddJoinParticipentConteiner(id, localPlayer.isHost, "Player", color, playerId);
    }
    void DeleteJoinContainer(string id, string playerId)
    {
        // tell the server to delete my game container to everyone in this match
        SyncListGameObject playersInThisMatch = MatchMaker.instance.FindMatchById(id).playersInThisMatch;
        for (int i = 0; i < playersInThisMatch.Count; i++)
        {
            // add my contaniner to everyone else
            if (playersInThisMatch[i].GetComponent<Player>() != this)
                TargetDeleteJoinContainer(playersInThisMatch[i].GetComponent<Player>().connectionToClient, id, playerId);
        }
    }
    [TargetRpc]
    void TargetDeleteJoinContainer(NetworkConnection target, string id, string playerId)
    {
        localPlayer.uiLobby.DeleteJoinParticipentContainer(id, playerID);
    }
    [Command]
    void CmdAddHostContainer(string id, string playerId)
    {
        SyncListGameObject playersInThisMatch = MatchMaker.instance.FindMatchById(id).playersInThisMatch;
        for (int i = 0; i < playersInThisMatch.Count; i++)
        {
            // add my contaniner to everyone else
            Debug.Log(playersInThisMatch[i].GetComponent<NetworkIdentity>().connectionToClient);
            int color = this.playerColor;
            if(playersInThisMatch[i].GetComponent<Player>().isHost)
                TargetAddHostParticipentContainer(playersInThisMatch[i].GetComponent<NetworkIdentity>().connectionToClient, id, color, playerId); ;
        }
    }

    [TargetRpc]
    private void TargetAddHostParticipentContainer(NetworkConnection target, string id, int color, string playerId)
    {
        Debug.LogError(" target rpc adding host participent ");
        localPlayer.uiLobby.AddHostParticipentContainer(id, "Player", color, playerId);
    }
    void DeleteParticipentContainer(string id, string playerId)
    {
        // tell the server to delete my game container to everyone in this match
        SyncListGameObject playersInThisMatch = MatchMaker.instance.FindMatchById(id).playersInThisMatch;
        for (int i = 0; i < playersInThisMatch.Count; i++)
        {
            // delete my contaniner to everyone else
            TargetDeleteParticipentContainer(playersInThisMatch[i].GetComponent<Player>().connectionToClient, id, playerId);
        }
    }

    [TargetRpc]
    void TargetDeleteParticipentContainer(NetworkConnection target, string id, string playerId)
    {
        if(localPlayer.isHost)
            localPlayer.uiLobby.DeleteHostParticipentContainer(id, playerId);
        else
            localPlayer.uiLobby.DeleteJoinParticipentContainer(id, playerId);
    }


    [Command]
    internal void CmdUpdateGameRoomList()
    {
        MatchMaker instance = MatchMaker.instance;

        for (int i = 0; i < instance.allGames.Count; i++)
        {
            TargetUpdateGameRoomList(instance.allGames[i].matchId, instance.allGames[i].gameName, instance.allGames[i].playersInThisMatch.Count, instance.allGames[i].maxGameSize);
        }
    }
    [TargetRpc]
    void TargetUpdateGameRoomList(string id, string gameName, int currentPlayers, int maxPlayers)
    {
        localPlayer.uiLobby.AddGameContainer(id, gameName, currentPlayers, maxPlayers);
    }

    internal void CountParticipentContainers(string id)
    {
        if (MatchMaker.instance.FindMatchById(id) == null)
            Debug.LogError("match id does not exist in match maker", this);
        SyncListGameObject playersInThisMatch = MatchMaker.instance.FindMatchById(id).playersInThisMatch;
        for (int i = 0; i < playersInThisMatch.Count; i++)
        {
            TargetCountParticipentContainers(playersInThisMatch[i].GetComponent<Player>().connectionToClient, MatchMaker.instance.FindMatchById(id).playersInThisMatch.Count, MatchMaker.instance.FindMatchById(id).maxGameSize);
        }
    }
    [TargetRpc]
    void TargetCountParticipentContainers(NetworkConnection target,int current, int max)
    {
        if(localPlayer.isHost== true)
            localPlayer.uiLobby.hostParticipentNumTxt.text = (current - 1) + " / " + max; // (current-1) -1 because we don't iclude the host as a participent
        else
            localPlayer.uiLobby.joinParticipentNumTxt.text = (current - 1) + " / " + max; //(current - 1) - 1 because we don't iclude the host as a participent

    }

    #endregion
    #region CHANGE NAME
    [Command]
    public void CmdUpdateMyJoinContainerName(string id, string thisplayerId, string playerName)
    {
        SyncListGameObject playersInThisMatch = MatchMaker.instance.FindMatchById(id).playersInThisMatch;
        for (int i = 0; i < playersInThisMatch.Count; i++)
        {
            if (playersInThisMatch[i].GetComponent<Player>().playerID != thisplayerId)
                TargetUpdateMyJoinContainerName(playersInThisMatch[i].GetComponent<NetworkIdentity>().connectionToClient, thisplayerId, playerName);
            else
                playersInThisMatch[i].GetComponent<Player>().playerName = playerName;
        }
    }
    [TargetRpc]
    void TargetUpdateMyJoinContainerName(NetworkConnection target, string thisplayerId, string playerName)
    {
        localPlayer.uiLobby.UpdateJoinContainerName(thisplayerId, playerName);
        Debug.LogError("TargetUpdateMyJoinContainerName");
    }
    [Command]
    public void CmdUpdateMyHostContainerName(string id, string thisplayerId, string playerName)
    {
        SyncListGameObject playersInThisMatch = MatchMaker.instance.FindMatchById(id).playersInThisMatch;
        for (int i = 0; i < playersInThisMatch.Count; i++)
        {
            if (playersInThisMatch[i].GetComponent<Player>().playerID != thisplayerId)
                TargetUpdateMyHostContainerName(playersInThisMatch[i].GetComponent<NetworkIdentity>().connectionToClient, thisplayerId, playerName);
            else
                playersInThisMatch[i].GetComponent<Player>().playerName = playerName;
        }
    }
    [TargetRpc]
    void TargetUpdateMyHostContainerName(NetworkConnection target, string playerId, string playerName)
    {
        if(localPlayer.isHost)
        {
        Debug.LogError("TargetUpdateMyHostContainerName");
        localPlayer.uiLobby.UpdateHostContainerName(playerId, playerName);

        }
    }
    /*
    internal void PlayerChangeName()
    {

    }

    [Command]
    void CmdChangePlayerName(string id)
    {
        SyncListGameObject playersInThisMatch = MatchMaker.instance.FindMatchById(id).playersInThisMatch;
        for (int i = 0; i < playersInThisMatch.Count; i++)
        {
            if (playersInThisMatch[i].GetComponent<Player>().playerID != playerID)
                TargetChangePlayerName(playersInThisMatch[i].GetComponent<NetworkIdentity>().connectionToClient, playerID, playerName);
        }
    }

    [TargetRpc]
    void TargetChangePlayerName(NetworkConnection target)
    {
        localPlayer.uiLobby.UpdatePlayerNameIP();
    }
    */
    #endregion

    #region GAME CONTROLLERS
    #region DISTRIBUTING DATA
    [Command]
    void CmdDistributeDataForFirstBoard()
    {
        // this is called only on the host
        TargetDistributeData(false);
    }
    [Command]
    void CmdDistributeDataForSecondBoard()
    {
        // this is called only on the host
        TargetDistributeData(true);
    }
    [TargetRpc]
    void TargetDistributeData(bool isDouble)
    {
        // this is called on the host only
        localPlayer.uiGame = GameObject.Find("UI Game Controller").GetComponent<UIGameController>();
        DistributeData.instance.RandomlyDistributeCatFromData(isDouble, localPlayer.uiGame.allCatagories);
        // turn of the amount buttons for the host
        localPlayer.uiGame.DeactivateSlots();
        // all of the slots are set up here
        for (int i = 0; i < localPlayer.uiGame.allCatagories.Length; i++)
        {
            CmdCopyCatagoryData(i, localPlayer.uiGame.allCatagories[i].catagoryText.text);
        }
        for (int k = 0; k < localPlayer.uiGame.allSlots.Length; k++)
        {
            CmdCopyAmountData(k, localPlayer.uiGame.allSlots[k].answer, localPlayer.uiGame.allSlots[k].question);
            //Debug.LogError("Answer: " + localPlayer.uiGame.allSlots[k].answer);
        }
    }
    [Command]
    void CmdCopyCatagoryData(int index, string catText)
    {
        RpcCopyCatagoryData(index, catText);
    }
    [ClientRpc]
    void RpcCopyCatagoryData(int index, string catText)
    {
        this.uiGame = GameObject.Find("UI Game Controller").GetComponent<UIGameController>(); 
        //the host is already set up
        if (!localPlayer.isHost)
        {
            localPlayer.uiGame.allCatagories[index].catagoryText.text = catText;
            localPlayer.uiGame.allCatagories[index].name = catText;
            for (int k = 0; k < 5; k++)
            {
                localPlayer.uiGame.allCatagories[index].amounts[k].catagoryName = catText;
            }
        }
    }

    [Command]
    void CmdCopyAmountData(int k, string answer, string question)
    {
        RcpCopyAmountData(k, answer, question);
    }
    [ClientRpc]
    void RcpCopyAmountData(int k, string answer, string question)
    {
        // because the host already has the data
        if(!localPlayer.isHost)
        {
            localPlayer.uiGame.allSlots[k].answer = answer;
            localPlayer.uiGame.allSlots[k].question = question;
        }
    }
    #endregion
    #region OPEN PANALS
    internal void PlayerOpenDoubleJeopardyPanal()
    {
        // this method will be called only from the host
        CmdDistributeDataForSecondBoard();
        localPlayer.PlayerSetIsDoubleJeopardy(true);
        localPlayer.PlayerSetQuestionsLeft(30);
        localPlayer.PlayerPlaceDailyDouble();
        CmdOpenDoubleJeopardyPanal();
    }
    [Command]
    void CmdOpenDoubleJeopardyPanal()
    {
        RpcOpenDoubleJeopardyPanal();
    }
    [ClientRpc]
    void RpcOpenDoubleJeopardyPanal()
    {
        localPlayer.uiGame.OpenDoubleJeopardyPanal();
    }
    internal void PlayerOpenSlotsPanalToAll()
    {
        CmdOpenSlotsPanal();
    }
    [Command]
    void CmdOpenSlotsPanal()
    {
        RpcOpenSlotsPanal();
    }
    [ClientRpc]
    void RpcOpenSlotsPanal()
    {
        // reset has everyone answered 
        localPlayer.PlayerSetHasAnswered(false);
        localPlayer.uiGame.OpenSlotsPanel();
    }
    internal void PlayerOpenQuestionPanalToAll()
    {
        CmdOpenQuestionPanalToAll();
    }
    [Command]
    void CmdOpenQuestionPanalToAll()
    {
        RpcOpenQuestionPanalToAll();
    }
    [ClientRpc]
    void RpcOpenQuestionPanalToAll()
    {
        // make sure to open after updating cuurent question, answer, amount
        if (localPlayer.isHost)
            // change what should happen to host when players have time to buzz in
            localPlayer.uiGame.OpenHostQuesionPanel();
        else
            localPlayer.uiGame.OpenClientQuesionPanel();
    }

    internal void PlayerOpenHostQuestionPanal()
    {
        CmdOpenHostQuestionPanal();
    }
    [Command]
    void CmdOpenHostQuestionPanal()
    {
        RpcOpenHostQuestionPanal();
    }
    [ClientRpc]
    void RpcOpenHostQuestionPanal()
    {
        if (localPlayer.isHost)
            localPlayer.uiGame.OpenHostQuesionPanel();
    }
    //Send a COMMAND and Target RPC to reveal the answer after a player has answered 
    internal void PlayerOpenAnswerPanalToAll()
    {
        CmdOpenAnswerPanalToAll();
    }
    [Command]
    void CmdOpenAnswerPanalToAll()
    {
        RpcOpenAnswerPanalToAll();
    }
    [ClientRpc]
    void RpcOpenAnswerPanalToAll()
    {
        if (localPlayer.isHost)
            localPlayer.uiGame.OpenHostQuesionPanel();
        else
            localPlayer.uiGame.OpenClientAnswerPanel();
    }
    public void PlayerOpenFinalJeopardyPanalToAll()
    {

            localPlayer.PlayerSetIsDoubleJeopardy(false);
            localPlayer.PlayerSetIsFinalJeopardy(true);
            localPlayer.PlayerSetCurrenctQuestionAmount(0);
            CmdOpenFinalJeopardyPanalToAll();
        if(localPlayer.uiGame.currentQuestionAmount!=0)
            localPlayer.PlayerSetCurrenctQuestionAmount(0);

    }
    [Command]
    void CmdOpenFinalJeopardyPanalToAll()
    {
        RpcOpenFinalJeopardyPanalToAll();
    }
    [ClientRpc]
    void RpcOpenFinalJeopardyPanalToAll()
    {
        if(localPlayer.isHost == false)
        {
            localPlayer.uiGame.OpenFinalJeopardyPanal();
        }
        else 
            localPlayer.PlayerOpenHostQuestionPanal();
    }
    internal void PlayerOpenWinnerPanal()
    {
        CmdOpenWinnerPanal();
    }
    [Command]
    void CmdOpenWinnerPanal()
    {
        int winnerAmount = 0;
        string winnerName = "";
        RpcOpenWinnerPanal(winnerAmount, winnerName);
    }
    [ClientRpc]
    void RpcOpenWinnerPanal(int winnerAmount, string winnerName)
    {
        localPlayer.uiGame.OpenWinnerPanel(winnerAmount, winnerName);
    }
    #endregion
    #region GETTING AND SETTING VARIABLES
    // time to answer
    /*internal int PlayerGetTimeToBuzz()
    {
        return CmdGetTimeToBuzz(localPlayer.matchID);
    }
    [Command]
    void CmdGetTimeToBuzz(string id)
    {
        return MatchMaker.instance.FindMatchById(id).timeToBuzz;
    }
    // time to buzz
    internal int PlayerGetTimeToAnswer()
    {
        return CmdGetTimeToAnswer(localPlayer.matchID);
    }
    [Command]
    void CmdGetTimeToAnswer(string id)
    {
        //return MatchMaker.instance.FindMatchById(id).timeToAnswer;
    }*/
    // daily double
    // these need to happen only once when called, so it will be changed once and not once each time the player calls it 

    internal void PlayerSetNowAnswering(int index)
    {
        CmdUpdateNowAnswering(index);
    }
    [Command]
    void CmdUpdateNowAnswering(int index)
    {
        TurnManager.instance.nowAnswering = index;
    }
    internal void PlayerSetIsDailyDouble(bool dailyDouble)
    {
            CmdSetIsDailyDouble(dailyDouble);
    }
    [Command]
    void CmdSetIsDailyDouble(bool dailyDouble)
    {
        RpcSetIsDailyDouble(dailyDouble);
    }
    [ClientRpc]
    void RpcSetIsDailyDouble(bool dailyDouble)
    {
        localPlayer.uiGame.isDailyDoubleNow = dailyDouble;
        Debug.LogError("Daily double has changed to: " + localPlayer.uiGame.isDailyDoubleNow);
    }
    // double jeopardy
    internal void PlayerSetIsDoubleJeopardy(bool doubleJeopardy)
    {
        if (this.isHost)
            CmdSetIsDoubleJeopardy(doubleJeopardy);
    }
    [Command]
    void CmdSetIsDoubleJeopardy(bool doubleJeopardy)
    {
        RpcSetIsDoubleJeopardy(doubleJeopardy);
    }
    [ClientRpc]
    void RpcSetIsDoubleJeopardy(bool doubleJeopardy)
    {
        localPlayer.uiGame.isDailyDoubleNow = doubleJeopardy;
    }
    // final jeopardy
    internal void PlayerSetIsFinalJeopardy(bool finalJeopardy)
    {
            CmdSetIsFinalJeopardy(finalJeopardy);
    }
    [Command]
    void CmdSetIsFinalJeopardy(bool finalJeopardy)
    {
        RpcSetIsFinalJeopardy(finalJeopardy);
    }
    [ClientRpc]
    void RpcSetIsFinalJeopardy(bool finalJeopardy)
    {
        localPlayer.uiGame.isFinalJeopardyNow = finalJeopardy;
    }
    public void PlayerTest()
    {
    }
    // remeining question
    internal void PlayerSetQuestionsLeft(int left)
    {
        CmdSetQuestionsLeft(left);
    }
    [Command]
    void CmdSetQuestionsLeft(int left)
    {
        RpcSetQuestionsLeft(left);
    }
    [ClientRpc]
    void RpcSetQuestionsLeft(int left)
    {
        localPlayer.uiGame.questionsLeft = left;
        localPlayer.uiGame.remeiningQuestions.text = left + "/30";
        Debug.LogError("Question left has changed to: " + localPlayer.uiGame.questionsLeft);
    }
    // question amount
    internal void PlayerSetCurrenctQuestionAmount(int amount)
    {
        CmdSetCurrenctQuestionAmount(amount);
    }
    [Command]
    void CmdSetCurrenctQuestionAmount(int amount)
    {
        RpcSetCurrenctQuestionAmount(amount);
    }
    [ClientRpc]
    void RpcSetCurrenctQuestionAmount(int amount)
    {
        localPlayer.uiGame.currentQuestionAmount = amount;
        //localPlayer.uiGame.clientAnswerAmountText.text = "$"+amount.ToString();
        Debug.LogError("Current Question Amount has changed to: " + localPlayer.uiGame.currentQuestionAmount);
    }

    // question amount for host only
    internal void PlayerSetHostCurrenctQuestionAmount(int amount)
    {
        CmdSetHostCurrenctQuestionAmount(amount);
    }
    [Command]
    void CmdSetHostCurrenctQuestionAmount(int amount)
    {
        RpcSetHostCurrenctQuestionAmount(amount);
    }
    [ClientRpc]
    void RpcSetHostCurrenctQuestionAmount(int amount)
    {
        if(localPlayer.isHost)
        {
            localPlayer.uiGame.currentQuestionAmount = amount;
            localPlayer.uiGame.hostQuestionAmountTxt.text = "$"+amount.ToString();
        }
        Debug.LogError("Current Question Amount has changed to: " + localPlayer.uiGame.currentQuestionAmount);
    }
    // current answer and question  
    internal void PlayerSetQuestionAndAnswer(string question, string answer)
    {
        CmdSetQuestionAndAnswer(question, answer);
    }
    [Command]
    void CmdSetQuestionAndAnswer(string question, string answer)
    {
        RpcSetQuestionAndAnswer(question,answer);
    }
    [ClientRpc]
    void RpcSetQuestionAndAnswer(string question, string answer)
    {
        localPlayer.uiGame.currentQuestion = question;
        localPlayer.uiGame.currentCorrectAnswer = answer;
        Debug.LogError("Current question and answer have been changed to: " + localPlayer.uiGame.currentQuestion+ " " + localPlayer.uiGame.currentCorrectAnswer);
    }

    // current input answer
    internal void PlayerSetCurrentInputAnswer(string answer)
    {
        CmdSetCurrentInputAnswer(answer);
    }
    [Command]
    void CmdSetCurrentInputAnswer( string answer)
    {
        RpcSetCurrentInputAnswer( answer);
    }
    [ClientRpc]
    void RpcSetCurrentInputAnswer(string answer)
    {
        localPlayer.uiGame.currentInputAnswer = answer;
        Debug.LogError("Current input answer hase been changed to: " + localPlayer.uiGame.currentInputAnswer);
        if (localPlayer.isHost)
        {
            localPlayer.uiGame.correctButton.SetEnable(true);
            localPlayer.uiGame.incorrectButton.SetEnable(true);
            localPlayer.uiGame.hostInputAnswerTxt.text = answer;
        }
    }

    internal void PlayerSetHasAnswered(bool has)
    {
        this.hasAnswered = has;
        CmdSetHasAnswered(has, localPlayer.matchID, this.playerID);
    }
    [Command]
    void CmdSetHasAnswered(bool has, string matchid, string playerId)
    {
        this.hasAnswered = has;
        SyncListGameObject players = MatchMaker.instance.FindMatchById(matchid).playersInThisMatch;
        for (int i = 0; i < players.Count; i++)
        {
            if(players[i].GetComponent<Player>().playerID == playerId)
            {
                MatchMaker.instance.FindMatchById(matchid).playersInThisMatch[i].GetComponent<Player>().hasAnswered = has;
                Debug.LogError("Sucsesfully changed player has answered in matchmaker for id " + matchID);
            }
        }
    }
    #endregion
    #region GENERAL METHODS
    public void PlayerAddAmount(int amount)
    {
        if(localPlayer.uiGame.isDailyDoubleNow || localPlayer.uiGame.isFinalJeopardyNow)
        {
            this.playerAmount += amount * 2;
        }
        else
        {
            this.playerAmount += amount;
        }
        CmdPlayerAddAmount(this.playerAmount,this.playerIndex);
    }
    //Send a COMMAND and Target RPC each time the players money amout changes and change the UI accordinly
    [Command]
    internal void CmdPlayerAddAmount(int newAmount, int playerIndex)
    {
        this.playerAmount = newAmount;
        TargetUpdatePlayerUI(newAmount);
        RpcUpdateSideContainerToAll(playerIndex, newAmount);
    }

    public void PlayerDeductAmount(int amount)
    {
        if (this.playerAmount - amount >=0)
        {
            this.playerAmount -= amount;
            CmdPlayerDeductAmount(this.playerIndex, this.playerAmount);
        }
    }
    [Command]
    internal void CmdPlayerDeductAmount(int index, int newAmount)
    {
        this.playerAmount = newAmount;
        TargetUpdatePlayerUI(newAmount);
        RpcUpdateSideContainerToAll(index, newAmount);
    }
    [TargetRpc]
    void TargetUpdatePlayerUI(int newAmount)
    {
        if (this.uiPlayer == null)
            this.uiPlayer = GameObject.Find("UIPlayer").GetComponent<UIPlayerController>();
        this.uiPlayer.UpdateMyButtomContainer(newAmount);
    }
    [ClientRpc]
    void RpcUpdateSideContainerToAll(int myIndex, int newAmount)
    {
        SidePanalController.instance.UpdateSideSlotAmount(myIndex, newAmount);
    }
    internal void PlayerPlaceDailyDouble()
    {
        // making sure player is host to call this only once, so that the daily double for everyone will on;;y be set once
        if (localPlayer.isHost)
            CmdPlaceDailyDouble(localPlayer.matchID);
    }
    [Command]
    void CmdPlaceDailyDouble(string id)
    {
        if (MatchMaker.instance.FindMatchById(id).shouldActicateDailyDouble)
        {
            // choose a random spot to place the daily double and tell eveyone to do so
            int rnd = UnityEngine.Random.Range(0, 29);
            RpcPlaceDailyDouble(rnd);
        }
        else
            Debug.LogError("Should place daily double is set to false", this);
    }
    [ClientRpc]
    void RpcPlaceDailyDouble(int spot)
    {
        localPlayer.uiGame.PlaceDailyDouble(spot);
    }
    //Send a COMMAND and Target RPC to let all the players know a player has buzzed in and thet cannot answer anymore
    //set the correct ui each time a player buzzed in
    //Send a COMMAND and Target RPC if no player has buzzed and reveal the answer

    internal void PlayerBuzzedIn()
    {
        CmdPlayerBuzzedIn(playerID);
        this.PlayerStopTimerForAllExceptMe();
    }
    [Command]
    void CmdPlayerBuzzedIn(string playerID)
    {
        TargetPlayerBuzzedIn();
        RpcPlayerBuzzedIn(playerID);
    }
    [TargetRpc]
    void TargetPlayerBuzzedIn()
    {
        Debug.Log("Player buzzed");
        // runs only on the player that has buzzed in 
    }
    [ClientRpc]
    void RpcPlayerBuzzedIn(string playerID)
    {
        // make it so  can't buzz
        if(localPlayer.playerID != playerID) {
            localPlayer.uiGame.CantBuzz();
        }
    }
    internal void PlayerDidntBuzz()
    {
        CmdPlayerDidntBuzz(this.matchID);
    }
    [Command]
    void CmdPlayerDidntBuzz(string matchid)
    {
        TargetPlayerDidntBuzz(TurnManager.CheckIfEveryoneAnswered(MatchMaker.instance.FindMatchById(matchid).playersInThisMatch));

    }
    [TargetRpc]
    void TargetPlayerDidntBuzz(bool everyoneAnswered)
    {
        Debug.Log("Player didn't buzz in, everyone answered = " + everyoneAnswered);
        // if no player has buzzed in, reveal the answer to everyone
        localPlayer.PlayerDeductAmount(localPlayer.uiGame.currentQuestionAmount);
        localPlayer.PlayerGiveTryTo();
        localPlayer.PlayerSetHasAnswered(true);
    }

    //Send a COMMAND and Target RPC each time the player answers wrong/ true
    //Send a COMMAND and Target RPC if player has submited a wrong answer, or buzzed but not submited, giving the other players a chance to buzz
    internal void PlayerSumbited(string answer)
    {
        this.PlayerSetNowAnswering(localPlayer.playerIndex);
        localPlayer.PlayerSetCurrentInputAnswer(answer);
    }
    internal void PlayerHostDecided(bool correct)
    {
        CmdHostDecided(correct, localPlayer.matchID);
    }
    [Command]
    void CmdHostDecided(bool correct, string matchid)
    {
        Debug.LogError("Now answering indezx is " + TurnManager.instance.nowAnswering);
        if(correct)
            RpcPlayerSumbitedRight(TurnManager.instance.nowAnswering);
        else
        {
            RpcPlayerSumbitedWrong(TurnManager.CheckIfEveryoneAnswered(MatchMaker.instance.FindMatchById(matchid).playersInThisMatch), TurnManager.instance.nowAnswering);
        }
    }
    [ClientRpc]
    void RpcPlayerSumbitedRight(int whoAnswered)
    {
        if (localPlayer.playerIndex == whoAnswered)
        {
            localPlayer.PlayerSetQuestionsLeft((localPlayer.uiGame.questionsLeft-1));
            // need to call this on the player that has submited only
            Debug.Log("Answer was declared correct");
            //A correct response earns the dollar value of the question and the opportunity to select the next question from the board.
            localPlayer.PlayerAddAmount(localPlayer.uiGame.currentQuestionAmount);
            localPlayer.CmdOpenAnswerPanalToAll();
            //PlayerGiveTurnTo(localPlayer.playerIndex, true);

        }
    }

    [ClientRpc]
    void RpcPlayerSumbitedWrong( bool everyoneAnswered, int whoAnswered)
    {
        if (localPlayer.playerIndex == whoAnswered)
        {
            localPlayer.PlayerSetQuestionsLeft((localPlayer.uiGame.questionsLeft-1));
            Debug.Log("Answer was declared wrong");
            //An incorrect response or a failure to buzz in within the time limit deducts the dollar value of the question 
            //from the team's score and gives any remaining opponent(s) the opportunity to buzz in and respond.
            localPlayer.PlayerDeductAmount(localPlayer.uiGame.currentQuestionAmount);
            //change later
            //PlayerGiveTurnTo(localPlayer.playerIndex, true);
            /*change
            if (everyoneAnswered == false)
            {
                // if not double jeopardy or daily double
                if (!localPlayer.uiGame.isDailyDoubleNow && !localPlayer.uiGame.isFinalJeopardyNow)
                {
                    localPlayer.PlayerGiveTryTo();
                }
            }
            else
            {

                PlayerGiveTurnTo(localPlayer.playerIndex, false);
            }*/
        }
    }
    internal void PlayerStopTimerForAllExceptMe()
    {
        CmdStopTimerForAllExceptMe(this.playerIndex);
    }
    [Command]
    void CmdStopTimerForAllExceptMe(int index)
    {
        RpcStopTimerForAllExceptMe(index);
    }
    [ClientRpc]
    void RpcStopTimerForAllExceptMe(int index)
    {
        if(localPlayer.playerIndex!=index&& localPlayer.isHost == false)
        {
            Debug.Log("Stopping timer coutine");
            localPlayer.uiGame.StopTimerCoroutine();
        }
    }
    internal void PlayerStartTimerForAll(bool sumbit)
    {
        CmdStartTimerForAll(sumbit);
    }
    [Command]
    void CmdStartTimerForAll(bool sumbit)
    {
        RpcStartTimerForAll(sumbit);
    }
    [ClientRpc]
    void RpcStartTimerForAll(bool sumbit)
    {
        localPlayer.uiGame.StartTimerCoroutine(sumbit);
    }

    internal void PlayerStartTimerForHost(bool sumbit)
    {
        CmdStartTimerForHost(sumbit);
    }
    [Command]
    void CmdStartTimerForHost(bool sumbit)
    {
        RpcStartTimerForHost(sumbit);
    }
    [ClientRpc]
    void RpcStartTimerForHost(bool sumbit)
    {
        if(localPlayer.isHost)
            localPlayer.uiGame.StartTimerCoroutine(sumbit);
    }
    internal void PlayerStoptTimerForHost()
    {
        CmdStopTimerForHost();
    }
    [Command]
    void CmdStopTimerForHost()
    {
        RpcStopTimerForHost();
    }
    [ClientRpc]
    void RpcStopTimerForHost()
    {
        if (localPlayer.isHost)
        {
            Debug.LogError("RpcStopTimerForHost()");
            localPlayer.uiGame.StopTimerCoroutine();
        }
    }
    void PlayerGiveTryTo()
    {
        CmdGiveTryTo(localPlayer.playerIndex, localPlayer.matchID);
    }

    [Command]
    void CmdGiveTryTo(int lastIndex, string matchid)
    {
        // make sure this method is not called from the host
        SyncListGameObject players = MatchMaker.instance.FindMatchById(matchid).playersInThisMatch;
        if (players.Count > 2)
        {
            int nextIndex;
            if (lastIndex + 1 > players.Count - 1)
                nextIndex = 0;
            else
                nextIndex = lastIndex++;
            // check that we are not giving the turn to the host
            for (int i = 0; i < players.Count; i++)
            {
                if (players[i].GetComponent<Player>().playerIndex == nextIndex)
                {
                    if (players[i].GetComponent<Player>().isHost)
                    {
                        // check we are not goint out of the list by adding 1
                        nextIndex++;
                        if (i == players.Count - 1)
                            nextIndex = 0;
                    }
                }
            }
            RpcGiveTryTo(nextIndex);
        }
        else
            RpcGiveTryTo(lastIndex);
    }
    [ClientRpc]
    void RpcGiveTryTo(int indexToGive)
    {
        if (localPlayer.isHost == false)
        {
            if (localPlayer.playerIndex == indexToGive)
                localPlayer.uiGame.GiveTurnToMe();
            else
                localPlayer.uiGame.TakeTurnFromMe();
        }
    }
    void PlayerGiveTurnTo(int lastIndex, bool me)
    {
        CmdGiveTurnTo(lastIndex, me);
    }
    [Command]
    void CmdGiveTurnTo(int lastIndex,bool me)
    {
        int indexToGive = lastIndex;
        if (me)
            RpcGiveTurnTo(lastIndex);
        else
        {
            RpcGiveTurnTo(indexToGive);

        }
    }
    [ClientRpc]
    void RpcGiveTurnTo(int indexToGive)
    {
        localPlayer.uiGame.OpenSlotsPanel();
    }
    internal void PlayerPauseGameForAll()
    {
        CmdPauseGameForAll();
    }
    [Command]
    void CmdPauseGameForAll()
    {
        RpcPauseGameForAll();
    }
    [ClientRpc]
    void RpcPauseGameForAll()
    {
        bool stopTimer = false;
        if (!localPlayer.isHost)
        {
            localPlayer.isSumbiting = false;
            localPlayer.isBuzzing = false;
            if (localPlayer.uiGame.buzzButton.gameObject.activeSelf && localPlayer.uiGame.buzzButton.enableButton)
            {
                localPlayer.isBuzzing = true;
                localPlayer.uiGame.CantBuzz();
            }
            if (localPlayer.uiGame.submitButton.gameObject.activeSelf && localPlayer.uiGame.submitButton.enableButton)
            {
                localPlayer.isSumbiting = true;
                localPlayer.uiGame.CantSumbit();
            }
        }
        else
        {
            
            if(localPlayer.uiGame.hostQuestionPanel.activeSelf)
            {
                localPlayer.canContinue = false;
                if (localPlayer.uiGame.hostContinueButton.gameObject.activeSelf)
                {
                    localPlayer.canContinue = true;
                    localPlayer.uiGame.hostContinueButton.gameObject.SetActive(false);

                }
                // if the player was able to submit before, bring it back after unpausing 
                localPlayer.canDecide = false;
                if (localPlayer.uiGame.correctButton.enableButton)
                {
                    canDecide = true;
                    localPlayer.uiGame.correctButton.SetEnable(false);
                    localPlayer.uiGame.incorrectButton.SetEnable(false);
                }
                localPlayer.uiGame.hostPauseBtn.gameObject.SetActive(false);
                localPlayer.uiGame.hostPauseImg.SetActive(true);
                localPlayer.uiGame.hostUnpauseBtn.gameObject.SetActive(true);
            }
        }

        // stop the timer
        localPlayer.uiGame.isPaused = true;
    }
    internal void PlayerUnPauseGameForAll()
    {
        CmdUnPauseGameForAll();
    }
    [Command]
    void CmdUnPauseGameForAll()
    {
        RpcUnPauseGameForAll();
    }
    [ClientRpc]
    void RpcUnPauseGameForAll()
    {
        if (localPlayer.isHost)
        {
            localPlayer.uiGame.hostUnpauseBtn.gameObject.SetActive(false);
            localPlayer.uiGame.hostPauseBtn.gameObject.SetActive(true);
            localPlayer.uiGame.hostPauseImg.SetActive(false);
            if (localPlayer.canContinue)
            {
                localPlayer.uiGame.hostContinueButton.gameObject.SetActive(true);
            }
            else
                localPlayer.uiGame.hostContinueButton.gameObject.SetActive(false);
            if (localPlayer.canDecide)
            {
                localPlayer.uiGame.correctButton.SetEnable(true);
                localPlayer.uiGame.incorrectButton.SetEnable(true);
            }
        }
        else
        {
            Debug.LogError("isSubmit = " + isSumbiting);
             if (localPlayer.isSumbiting)
                localPlayer.uiGame.CanSumbit();
             else if(localPlayer.isBuzzing)
            {
                localPlayer.uiGame.CanBuzz();

            }

        }
        localPlayer.uiGame.isPaused = false;

    }
    internal void PlayerGreyOutSlotForEveryone(int slotIndex)
    {
        CmdGreyOutSlotForEveryone(slotIndex);
    }
    [Command]
    void CmdGreyOutSlotForEveryone(int slotIndex)
    {
        RpcGreyOutSlotForEveryone(slotIndex);
    }
    [ClientRpc]
    void RpcGreyOutSlotForEveryone(int slotIndex)
    {
        localPlayer.uiGame.GreyOutSlot(slotIndex);
    }
    #endregion
    #endregion
    public void KickPlayer(string playerID) {
        CmdKickPlayer(playerID);
    }
    [Command]
    void CmdKickPlayer(string playerID) {
        SyncListGameObject players = MatchMaker.instance.FindMatchById(this.matchID).playersInThisMatch;
        for(int i=0; i<players.Count; i++) {
            NetworkConnection connection = players[i].GetComponent<NetworkIdentity>().connectionToClient;
            if(playerID == players[i].GetComponent<Player>().playerID) {
                TargetKickPlayer(connection);
            }
        }
    }
    [TargetRpc]
    void TargetKickPlayer(NetworkConnection target) {
        localPlayer.PlayerCancelJoin(localPlayer.matchID);
    }
    void OnDestroy() {
        if(isHost)
            localPlayer.PlayerCancelJoin(matchID);
        else if(localPlayer.isHost) {
            localPlayer.KickPlayer(playerID);
        }
    }
}
