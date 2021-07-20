using UnityEngine;
using UnityEngine.UI;
using Mirror;
using System;
using System.Collections;
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
    [SyncVar] public int playerIndex;
    [SyncVar] public int finalAmount = 0;
    public ArrayList answererList = new ArrayList();
    public ArrayList answerList = new ArrayList();
    public ArrayList amountList = new ArrayList();
    public ArrayList playerIndexList = new ArrayList();
    public UILobbyController uiLobby;

    [Header("Game")]
    UIPlayerController uiPlayer;
    NetworkMatchChecker matchChecker;
    public UIGameController uiGame;
    internal bool isSumbiting, isBuzzing, canDecide, canContinue;

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
            this.playerAmount = 0;
        }
    }
    public void UpdateGameRoomList() {
        CmdUpdateGameRoomList();
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
        CmdHostGame(id, int.Parse(uiLobby.gameSizeTxt.text), gameName, localPlayer.gameObject);
    }
    public void ChangeColor()
    {
        CmdChangeColor(this.matchID, this.playerID, this.playerColor);
    }
    [Command]
    void CmdChangeColor(string matchID, string playerID, int playerColor)
    {
        Match match = MatchMaker.instance.FindMatchById(matchID);
        for (int i = 0; i < match.playersInThisMatch.Count; i++)
        {
            TargetCmdChangeColor(match.playersInThisMatch[i].GetComponent<NetworkIdentity>().connectionToClient, matchID, playerID, playerColor);
        }
    }
    [TargetRpc]
    void TargetCmdChangeColor(NetworkConnection target, string matchID, string playerID, int playerColor)
    {
        if (localPlayer.playerID != playerID || localPlayer.matchID != matchID)
        {
            Debug.LogError(playerID + "'s color = " + playerColor, this);
        }
    }
    [Command] // call this from a client to run it on the server
    // the problem was that we were passing in the player clone that was on the server and not the local player that called the method
    void CmdHostGame(string id, int gameSize, string gameName, GameObject player)
    {
        //Check if game name exists or game name is empty
        bool exist = false;
        foreach(var game in MatchMaker.instance.allGames) {
            if(game.gameName == gameName) exist = true;
        }
        var newname = gameName;
        if (string.IsNullOrEmpty(gameName) || exist)
        {
            exist = true;
            int gameidx = 0;
            while(exist) {
                gameidx ++;
                newname = "Game #" + gameidx;
                exist = false;
                foreach(var game in MatchMaker.instance.allGames) {
                    if(game.gameName == newname) exist = true;
                }
            }
            //Debug.LogError("No game name", this);
        }
        // tell the server we got an id, please register a new game and add this player to the list
        if (MatchMaker.instance.AddAndApproveHostingGame(id, newname, gameSize, player.gameObject))
        // if valadation passed
        {
            this.playerColor = -1;
            this.isReady = true;
            this.playerName = "Host";
            this.isHost = true;
            this.playerIndex = -1; //Host is not a player in sidebars
            CountParticipentContainers(id);
            TargetHostGame(true, id, gameSize, newname, MatchMaker.RegularIDToGUI(id));
            Debug.Log("Server - Sucssesfly hosted a game");
            // add a game container
        }
        else
        {
            TargetHostGame(false, id, gameSize, newname, MatchMaker.RegularIDToGUI(id));
            Debug.LogError("Server- Couldn't host game", this);
        }
    }

    [TargetRpc] // the server will run this on a specific client
    void TargetHostGame(bool success, string id, int gameSize, string gameName, Guid guid)
    {
        if (success)
        {
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
    public void OtherPlayerDisconnected(bool isHost, string playerName, string playerID, string matchID, int playerIndex) {
        CmdOtherPlayerDisconnected(isHost, playerName, playerID, matchID, playerIndex);
    }
    [Command]
    void CmdOtherPlayerDisconnected(bool isHost, string playerName, string playerID, string matchID, int playerIndex) {
        RpcPlayerLeaveGame(isHost, playerName, playerID, matchID, playerIndex);
    }
    public void PlayerLeaveGame() {
        CmdPlayerLeaveGame(localPlayer.isHost, localPlayer.playerName, localPlayer.playerID, localPlayer.matchID, localPlayer.playerIndex);
    }
    [Command]
    void CmdPlayerLeaveGame(bool isHost, string playerName, string playerID, string matchID, int playerIndex) {
        this.matchID = "";
        RpcPlayerLeaveGame(isHost, playerName, playerID, matchID, playerIndex);
    }
    [ClientRpc]
    void RpcPlayerLeaveGame(bool isHost, string playerName, string playerID, string matchID, int playerIndex) {
        if(localPlayer == null) return;
        if(localPlayer.matchID != matchID) return;
        if(isHost && localPlayer.playerID != playerID) {
            Toast.instance.showToast("The host has left the game", 3);
            SceneManager.LoadScene("Lobby");
            localPlayer.CancelGame(localPlayer.matchID);
        } else if(!isHost && localPlayer.playerID != playerID) {
            //Check if only one player has left
            var players = GameObject.FindObjectsOfType<Player>();
            int playersInThisMatch = 0;
            Player remainer;
            foreach(var player in players) {
                if(player.matchID == localPlayer.matchID && player.isHost == false && player.playerID != playerID) {
                    playersInThisMatch ++;
                    remainer = player;
                }
            }
            if(playersInThisMatch > 1) {
                Toast.instance.showToast($"{playerName} has left the game", 3);
                if(localPlayer.isHost && playerIndex == TurnManager.instance.cardChooser) {
                    //if i am host and player with the turn has left the game
                    //give turn to other players
                    localPlayer.GiveTurnToRandomPlayer();
                }
            } else {
                if(localPlayer.isHost) {
                    //declare remainer as winner
                    localPlayer.PlayerOpenWinnerPanal();
                }
            }
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

            //localPlayer.gameObject.GetComponent<NetworkMatchChecker>().matchId = new Guid();
            //CmdUpdateMatchChecker(new Guid());
            localPlayer.matchID = null;
            CmdUpdateMatchID(localPlayer.matchID);
            localPlayer.uiLobby.hostGamePanal.SetActive(false);
            localPlayer.uiLobby.lobbyPanal.SetActive(true);
        }
        else
        {
            Debug.LogError("Client- Player couldnt  delete host");
        }
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
            do
            {
                duplicate = false;
                for (int i = 0; i < players.Count; i++)
                {
                    if (this.playerID != players[i].GetComponent<Player>().playerID && colorChosen == players[i].GetComponent<Player>().playerColor) // if it is not me and has same color as me
                    {
                        duplicate = true;
                        colorChosen++;
                    }
                }
            } while (duplicate);
            this.playerColor = colorChosen;
            this.isReady = false;
            this.playerAmount = 0;
            this.matchID = id;
            this.isHost = false;
            this.playerIndex = colorChosen;
            this.playerName = "Player #" + (this.playerIndex + 1);

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
            localPlayer.matchID = id;
            localPlayer.isHost = false;
            //localPlayer.gameObject.GetComponent<NetworkMatchChecker>().matchId = GuiId;
            //CmdUpdateMatchChecker(GuiId);
            CmdUpdateIsHost(false, id, localPlayer.matchID);
            CmdUpdateMatchID(localPlayer.matchID);
            Debug.Log("Client- Game joining was sucsesfull with id " + id.ToString());
            // localPlayer.uiLobby.OpenJoinPanalWithId(index, match.maxGameSize);
            CmdAddJoinContainerLocally(id, localPlayer.playerID, index, match.maxGameSize);

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
    void CmdAddJoinContainerLocally(string id, string playerId, int index, int maxGameSize)
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
                TargetAddJoinContainerLocally(id, true, hostName, color, players[i].GetComponent<Player>().playerID, -1, maxGameSize, -1);
                break;
            }
        }
        bool onlyMe = (numOfPlayers == 2);
        TargetAddJoinContainerLocallyWithIF(id, this.playerColor, index, maxGameSize, onlyMe);
        for (int i = 0; i < numOfPlayers; i++)
        {
            if (players[i].GetComponent<Player>().isHost == false && players[i].GetComponent<Player>().playerID != playerId)
            {
                string playerName = players[i].GetComponent<Player>().playerName;
                int color = players[i].GetComponent<Player>().playerColor;
                if (string.IsNullOrEmpty(playerName))
                    playerName = "Player";
                TargetAddJoinContainerLocally(id, false, playerName, color, players[i].GetComponent<Player>().playerID, index, maxGameSize, i);
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
                TargetCmdAddJoinContainerToPlayersWhoAlreadyJoined(players[i].GetComponent<NetworkIdentity>().connectionToClient, id, this.playerColor, playerId, this.playerName);
        }
    }
    [TargetRpc]
    void TargetCmdAddJoinContainerToPlayersWhoAlreadyJoined(NetworkConnection target, string id, int color, string playerId, string playerNameToAdd)
    {
        localPlayer.uiLobby.AddJoinParticipentConteiner(id, false, playerNameToAdd, color, playerId);

    }

    [TargetRpc]
    void TargetAddJoinContainerLocally(string id, bool host, string playerName, int color, string playerId, int joined, int maxGameSize, int index)
    {
        Debug.Log(playerName + "'s color = " + color);
        localPlayer.uiLobby.AddJoinParticipentConteiner(id, host, playerName, color, playerId);
        //to get zero based index, minus 2 to exclude host player
        //if this is last container to add, show join panel
        if(joined - 1 == index) localPlayer.uiLobby.OpenJoinPanalWithId(joined, maxGameSize);
    }

    [TargetRpc]
    void TargetAddJoinContainerLocallyWithIF(string id, int color, int joined, int maxGameSize, bool onlyMe)
    {
        localPlayer.uiLobby.AddJoinParticipentConteinerWithInput(id, color, localPlayer.playerID);
        //if i am only the participant, open join panel now
        if(onlyMe) localPlayer.uiLobby.OpenJoinPanalWithId(joined, maxGameSize);

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
        if (playerName == "") return;
        CmdReadyGame();
    }

    [Command]
    void CmdReadyGame()
    {
        this.isReady = true;
        Match match = MatchMaker.instance.FindMatchById(this.matchID);
        bool gameFull = (match.playersInThisMatch.Count - 1 == match.maxGameSize);
        bool allReady = true;
        for (var i = 0; i < match.playersInThisMatch.Count; i++)
        {
            if (match.playersInThisMatch[i].GetComponent<Player>().isReady == false)
            {
                allReady = false;
            }
        }
        RpcReadyGame(this.playerID, this.matchID, gameFull, allReady);
    }
    [ClientRpc]
    void RpcReadyGame(string playerID, string matchID, bool gameFull, bool allReady)
    {
        if (localPlayer.matchID != matchID) return; // message from other match

        if (playerID == localPlayer.playerID)
        {//ready player is me
            localPlayer.uiLobby.updateJoinPanelIAmReady();
        }
        else
        {//other player has ready button clicked
            if (localPlayer.isHost)
            {//i am host
                localPlayer.uiLobby.updateHostPanelPlayerReady(gameFull, allReady);
            }
            else
            {//i am joiner
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
        CmdBeginGame(localPlayer.matchID, dailyDouble, timeToAnswer, timeToBuzz, TransferDataToGame.instance.gameName, TransferDataToGame.instance.gameSize);
    }
    [Command] // call this from a client to run it on the server
    void CmdBeginGame(string id, bool dailyDouble, int timeToAnswer, int timeToBuzz, string gameName, int gameSize)
    {
        //Game Settings
        TransferDataToGame.instance.dailyDouble = dailyDouble;
        TransferDataToGame.instance.timeToAnswer = timeToAnswer;
        TransferDataToGame.instance.timeToBuzz = timeToBuzz;
        TransferDataToGame.instance.gameName = gameName;
        TransferDataToGame.instance.gameSize = gameSize;

        //MatchMaker.instance.BegineGame(matchID);
        Match thisMatch = MatchMaker.instance.FindMatchById(id);

        //Randomly choose starting player
        TurnManager.instance.RandomlyChooseStartingPlayer(gameSize, matchID);
        TurnManager.instance.lastCardWinner = -1;

        thisMatch.started = true;
        RpcDeleteGameContainer(id);

        for (int i = 0; i < thisMatch.playersInThisMatch.Count; i++)
        {
            TargetBeginGame(thisMatch.playersInThisMatch[i].GetComponent<NetworkIdentity>().connectionToClient, MatchMaker.RegularIDToGUI(id), dailyDouble, timeToAnswer, timeToBuzz, gameName, gameSize, TurnManager.instance.cardChooser, TurnManager.instance.lastCardWinner);
        }
    }

    [TargetRpc] // the server will run this on a specific client
    void TargetBeginGame(NetworkConnection target, Guid guid, bool dailyDouble, int timeToAnswer, int timeToBuzz, string gameName, int gameSize, int cardChooser, int lastCardWinner)
    {
        //Game Settings
        TransferDataToGame.instance.dailyDouble = dailyDouble;
        TransferDataToGame.instance.timeToAnswer = timeToAnswer;
        TransferDataToGame.instance.timeToBuzz = timeToBuzz;
        TransferDataToGame.instance.gameName = gameName;
        TransferDataToGame.instance.gameSize = gameSize;
        TurnManager.instance.cardChooser = cardChooser;
        TurnManager.instance.lastCardWinner = lastCardWinner;
        // instatiate a match chcker with the correct id
        if(localPlayer.matchChecker == null) localPlayer.matchChecker = localPlayer.gameObject.AddComponent(typeof(NetworkMatchChecker)) as NetworkMatchChecker;
        localPlayer.matchChecker.matchId = guid;
        // spawn a turn manager
        //GameObject turnManager = Instantiate(localPlayer.uiLobby.turnMangerPrefab);
        //TurnManager turnManagerScript = turnManager.GetComponent<TurnManager>();
        //turnManagerScript.AddPlayer(localPlayer);
        //turnManager.GetComponent<NetworkMatchChecker>().matchId = RegularIDToGUI(id);
        // take all players to the game
        SceneManager.LoadScene("PlayerBoard");
    }

    public void DistributeDataForFirstBoard()
    {
        if (isHost)
            CmdDistributeDataForFirstBoard();
    }


    #endregion

    #region CONTAINERS
    public void ClearGameContainer()
    {
        localPlayer.uiLobby.ClearGameContainer();
    }
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
        localPlayer.uiLobby?.DeleteGameContainer(id);
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
            string playerNameToAdd = this.playerName;
            if (playersInThisMatch[i].GetComponent<Player>().isHost)
                TargetAddHostParticipentContainer(playersInThisMatch[i].GetComponent<NetworkIdentity>().connectionToClient, id, color, playerId, playerNameToAdd); ;
        }
    }

    [TargetRpc]
    private void TargetAddHostParticipentContainer(NetworkConnection target, string id, int color, string playerId, string playerNameToAdd)
    {
        Debug.LogError(" target rpc adding host participent ");
        localPlayer.uiLobby.AddHostParticipentContainer(id, playerNameToAdd, color, playerId);
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
        if (localPlayer.isHost)
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
            if (instance.allGames[i].started == false)
            {
                TargetUpdateGameRoomList(instance.allGames[i].matchId, instance.allGames[i].gameName, instance.allGames[i].playersInThisMatch.Count, instance.allGames[i].maxGameSize);
            }
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
    void TargetCountParticipentContainers(NetworkConnection target, int current, int max)
    {
        if (localPlayer.isHost == true)
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
        if (localPlayer.isHost)
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
            CmdCopyCatagoryData(i, localPlayer.uiGame.allCatagories[i].catagoryText.text, localPlayer.matchID);
        }
        for (int k = 0; k < localPlayer.uiGame.allSlots.Length; k++)
        {
            CmdCopyAmountData(k, localPlayer.uiGame.allSlots[k].answer, localPlayer.uiGame.allSlots[k].question, localPlayer.matchID);
            //Debug.LogError("Answer: " + localPlayer.uiGame.allSlots[k].answer);
        }
        if(isDouble) {
            localPlayer.PlayerSetIsDoubleJeopardy(true);
            localPlayer.PlayerPlaceDailyDouble();
            localPlayer.PlayerSetQuestionsLeft(30);
            CmdOpenDoubleJeopardyPanal(localPlayer.matchID);
        }
    }
    [Command]
    void CmdCopyCatagoryData(int index, string catText, string matchID)
    {
        RpcCopyCatagoryData(index, catText, matchID);
    }
    [ClientRpc]
    void RpcCopyCatagoryData(int index, string catText, string matchID)
    {
        if(localPlayer.matchID!=matchID) return;
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
    void CmdCopyAmountData(int k, string answer, string question, string matchID)
    {
        RcpCopyAmountData(k, answer, question, matchID);
    }
    [ClientRpc]
    void RcpCopyAmountData(int k, string answer, string question, string matchID)
    {
        if(localPlayer.matchID!=matchID) return;
        // because the host already has the data
        if (!localPlayer.isHost)
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
    }
    [Command]
    void CmdOpenDoubleJeopardyPanal(string matchID)
    {
        RpcOpenDoubleJeopardyPanal(matchID);
    }
    [ClientRpc]
    void RpcOpenDoubleJeopardyPanal(string matchID)
    {
        if(localPlayer.matchID != matchID) return;
        localPlayer.uiGame.OpenDoubleJeopardyPanal();
    }
    internal void PlayerOpenSlotsPanalToAll()
    {
        CmdOpenSlotsPanal(localPlayer.matchID);
    }
    [Command]
    void CmdOpenSlotsPanal(string matchID)
    {
        RpcOpenSlotsPanal(matchID);
    }
    [ClientRpc]
    void RpcOpenSlotsPanal(string matchID)
    {
        if(localPlayer.matchID != matchID) return;
        // reset has everyone answered 
        localPlayer.PlayerSetHasAnswered(false);
        localPlayer.uiGame.OpenSlotsPanel();
    }
    public void PlayerOpenQuestionPanalToUnansweredPlayers() {
        CmdOpenQuestionPanalToUnansweredPlayers(localPlayer.matchID);
    }
    [Command]
    void CmdOpenQuestionPanalToUnansweredPlayers(string matchID) {
        RpcOpenQuestionPanalToUnansweredPlayers(matchID);
    }
    [ClientRpc]
    void RpcOpenQuestionPanalToUnansweredPlayers(string matchID) {
        if(localPlayer.matchID != matchID) return;
        // make sure to open after updating cuurent question, answer, amount
        if (localPlayer.isHost)
            // change what should happen to host when players have time to buzz in
            localPlayer.uiGame.OpenHostQuesionPanel();
        else if(!localPlayer.hasAnswered)
            localPlayer.uiGame.OpenClientQuesionPanel();
    }
    internal void PlayerOpenQuestionPanalToAll(bool eligibility = false)
    {
        CmdOpenQuestionPanalToAll(localPlayer.matchID, eligibility);
    }
    [Command]
    void CmdOpenQuestionPanalToAll(string matchID, bool eligibility)
    {
        RpcOpenQuestionPanalToAll(matchID, eligibility);
    }
    [ClientRpc]
    void RpcOpenQuestionPanalToAll(string matchID, bool eligibility)
    {
        if(localPlayer.matchID != matchID) return;
        if(!eligibility) SidePanalController.instance.UntintAllExceptAnswered();
        // make sure to open after updating cuurent question, answer, amount
        if (localPlayer.isHost) {
            // change what should happen to host when players have time to buzz in
            if(!localPlayer.uiGame.hostQuestionPanel.activeSelf) localPlayer.uiGame.OpenHostQuesionPanel();
        }
        else if(!eligibility || !localPlayer.uiGame.eligibleToPlay)
            localPlayer.uiGame.OpenClientQuesionPanel(false);
    }

    internal void PlayerOpenHostQuestionPanal()
    {
        localPlayer.answererList.Clear();
        localPlayer.answerList.Clear();
        localPlayer.amountList.Clear();
        localPlayer.playerIndexList.Clear();
        localPlayer.uiGame.finalAnswered = 0;
        CmdOpenHostQuestionPanal(localPlayer.matchID);
    }
    [Command]
    void CmdOpenHostQuestionPanal(string matchID)
    {
        RpcOpenHostQuestionPanal(matchID);
    }
    [ClientRpc]
    void RpcOpenHostQuestionPanal(string matchID)
    {
        if(localPlayer.matchID != matchID) return;
        if (localPlayer.isHost) {
            Debug.LogError("Host Panel Opened");
            localPlayer.uiGame.OpenHostQuesionPanel();
        }
    }
    //Send a COMMAND and Target RPC to reveal the answer after a player has answered 
    internal void PlayerOpenAnswerPanalToAll()
    {
        CmdOpenAnswerPanalToAll(localPlayer.matchID);
    }
    [Command]
    void CmdOpenAnswerPanalToAll(string matchID)
    {
        RpcOpenAnswerPanalToAll(matchID);
    }
    [ClientRpc]
    void RpcOpenAnswerPanalToAll(string matchID)
    {
        if(localPlayer.matchID != matchID) return;
        if (localPlayer.isHost)
        {
            // localPlayer.uiGame.OpenHostQuesionPanel();
        }
        else
            localPlayer.uiGame.OpenClientAnswerPanel();
    }
    internal void PlayerOpenCorrectAnswerPanalToAll()
    {
        CmdOpenCorrectAnswerPanalToAll(localPlayer.matchID);
    }
    [Command]
    void CmdOpenCorrectAnswerPanalToAll(string matchID)
    {
        RpcOpenCorrectAnswerPanalToAll(matchID);
    }
    [ClientRpc]
    void RpcOpenCorrectAnswerPanalToAll(string matchID)
    {
        if(localPlayer.matchID != matchID) return;
        if (localPlayer.isHost)
        {
            // localPlayer.uiGame.OpenHostQuesionPanel();
        }
        else localPlayer.uiGame.OpenClientCorrectAnswerPanel();
    }
    
    public void PlayerOpenFinalJeopardyPanalToAll()
    {
        localPlayer.PlayerSetIsDoubleJeopardy(false);
        localPlayer.PlayerSetIsFinalJeopardy(true);
        localPlayer.PlayerSetCurrenctQuestionAmount(0);
        CmdOpenFinalJeopardyPanalToAll(localPlayer.matchID);
        if (localPlayer.uiGame.currentQuestionAmount != 0)
            localPlayer.PlayerSetCurrenctQuestionAmount(0);

    }
    [Command]
    void CmdOpenFinalJeopardyPanalToAll(string matchID)
    {
        RpcOpenFinalJeopardyPanalToAll(matchID);
    }
    [ClientRpc]
    void RpcOpenFinalJeopardyPanalToAll(string matchID)
    {
        if(localPlayer.matchID != matchID) return;
        var players = GameObject.FindObjectsOfType<Player>();
        if (localPlayer.isHost == false)
        {
            //Only open to top 3 players
            int myrank = 1;
            foreach(var player in players) {
                //if someone has higher amount than me, my rank is increased.
                if(player.matchID == localPlayer.matchID && player.playerID != localPlayer.playerID && player.playerAmount > localPlayer.playerAmount && player.isHost == false) {
                    myrank ++;
                }
            }
            //Only top 3 can take part in final jeopardy
            localPlayer.uiGame.OpenFinalJeopardyPanal(myrank<=3 && localPlayer.playerAmount>0);
        }
        else {
            localPlayer.uiGame.finalQuestion = localPlayer.uiGame.jsonToCScript.finalRoot.Questions[0].question;
            localPlayer.uiGame.finalAnswer = localPlayer.uiGame.jsonToCScript.finalRoot.Questions[0].Answer;
            localPlayer.PlayerSetQuestionAndAnswer(localPlayer.uiGame.finalQuestion, localPlayer.uiGame.finalAnswer);
            //Find final jeopardy participant count
            int participants = 0;
            foreach(var player in players) {
                if(player.matchID == localPlayer.matchID && player.isHost == false) {
                    int myrank = 1;
                    foreach(var player2 in players) {
                        //if someone has higher amount than me, my rank is increased.
                        if(player2.matchID == localPlayer.matchID && player2.playerAmount > player.playerAmount && player2.isHost == false) {
                            myrank ++;
                        }
                    }
                    if(myrank<=3 && player.playerAmount>0) participants++;
                }
            }

            localPlayer.uiGame.finalJeopardyParticipants = participants;
            localPlayer.uiGame.isFinalJeopardyNow = true;
            if(participants>0) localPlayer.PlayerOpenHostQuestionPanal();
            else localPlayer.PlayerOpenWinnerPanal();
        }
    }
    internal void PlayerOpenWinnerPanal()
    {
        CmdOpenWinnerPanal(localPlayer.matchID);
    }
    [Command]
    void CmdOpenWinnerPanal(string matchID)
    {
        int winnerAmount = int.MinValue;
        string winnerName = "";
        Match match = MatchMaker.instance.FindMatchById(matchID);
        for(var i=0; i<match.playersInThisMatch.Count; i++) {
            if(match.playersInThisMatch[i] == null) continue;
            var player = match.playersInThisMatch[i].GetComponent<Player>();
            if(player == null || player.matchID != matchID || player.isHost) continue;
            if(player.playerAmount > winnerAmount) {
                winnerAmount = player.playerAmount;
                winnerName = player.playerName;
            } else if(player.playerAmount == winnerAmount) {
                if(winnerName == "") winnerName = player.playerName;
                else winnerName = winnerName + ", " + player.playerName;
            }
        }
        RpcOpenWinnerPanal(winnerAmount, winnerName, matchID);
    }
    [ClientRpc]
    void RpcOpenWinnerPanal(int winnerAmount, string winnerName, string matchID)
    {
        if(localPlayer.matchID != matchID) return;
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
        CmdSetIsDailyDouble(dailyDouble, localPlayer.matchID);
    }
    [Command]
    void CmdSetIsDailyDouble(bool dailyDouble, string matchID)
    {
        RpcSetIsDailyDouble(dailyDouble, matchID);
    }
    [ClientRpc]
    void RpcSetIsDailyDouble(bool dailyDouble, string matchID)
    {
        if(localPlayer.matchID != matchID) return;
        localPlayer.uiGame.isDailyDoubleNow = dailyDouble;
        Debug.LogError("Daily double has changed to: " + localPlayer.uiGame.isDailyDoubleNow);
    }
    // double jeopardy
    internal void PlayerSetIsDoubleJeopardy(bool doubleJeopardy)
    {
        if (this.isHost)
            CmdSetIsDoubleJeopardy(doubleJeopardy, localPlayer.matchID);
    }
    [Command]
    void CmdSetIsDoubleJeopardy(bool doubleJeopardy, string matchID)
    {
        RpcSetIsDoubleJeopardy(doubleJeopardy, matchID);
    }
    [ClientRpc]
    void RpcSetIsDoubleJeopardy(bool doubleJeopardy, string matchID)
    {
        if(localPlayer.matchID != matchID) return;
        localPlayer.uiGame.isDoubleJeopardyNow = doubleJeopardy;
    }
    // final jeopardy
    internal void PlayerSetIsFinalJeopardy(bool finalJeopardy)
    {
        CmdSetIsFinalJeopardy(finalJeopardy, localPlayer.matchID);
    }
    [Command]
    void CmdSetIsFinalJeopardy(bool finalJeopardy, string matchID)
    {
        RpcSetIsFinalJeopardy(finalJeopardy, matchID);
    }
    [ClientRpc]
    void RpcSetIsFinalJeopardy(bool finalJeopardy, string matchID)
    {
        if(localPlayer.matchID != matchID) return;
        localPlayer.uiGame.isFinalJeopardyNow = finalJeopardy;
    }
    public void PlayerTest()
    {
    }
    // remeining question
    internal void PlayerSetQuestionsLeft(int left)
    {
        CmdSetQuestionsLeft(left, localPlayer.matchID);
    }
    [Command]
    void CmdSetQuestionsLeft(int left, string matchID)
    {
        RpcSetQuestionsLeft(left, matchID);
    }
    [ClientRpc]
    void RpcSetQuestionsLeft(int left, string matchID)
    {
        if(localPlayer.matchID != matchID) return;
        localPlayer.uiGame.questionsLeft = left;
        localPlayer.uiGame.remeiningQuestions.text = left + "/30";
        Debug.LogError("Question left has changed to: " + localPlayer.uiGame.questionsLeft);
    }
    // question category
    internal void PlayerSetCurrentCategory(string name) {
        CmdSetCurrentCategory(name, localPlayer.matchID);
    }
    [Command]
    void CmdSetCurrentCategory(string name, string matchID) {
        RpcSetCurrentCategory(name, matchID);
    }
    [ClientRpc]
    void RpcSetCurrentCategory(string name, string matchID) {
        if(localPlayer.matchID != matchID) return;
        localPlayer.uiGame.setCurrentCategory(name);
    }
    // question amount
    internal void PlayerSetCurrenctQuestionAmount(int amount)
    {
        CmdSetCurrenctQuestionAmount(amount, localPlayer.matchID);
    }
    [Command]
    void CmdSetCurrenctQuestionAmount(int amount, string matchID)
    {
        RpcSetCurrenctQuestionAmount(amount, matchID);
    }
    [ClientRpc]
    void RpcSetCurrenctQuestionAmount(int amount, string matchID)
    {
        if(localPlayer.matchID != matchID) return;
        localPlayer.uiGame.currentQuestionAmount = amount;
        //localPlayer.uiGame.clientAnswerAmountText.text = "$"+amount.ToString();
        Debug.LogError("Current Question Amount has changed to: " + localPlayer.uiGame.currentQuestionAmount);
    }

    // question amount for host only
    internal void PlayerSetHostCurrenctQuestionAmount(int amount)
    {
        CmdSetHostCurrenctQuestionAmount(amount, localPlayer.matchID);
    }
    [Command]
    void CmdSetHostCurrenctQuestionAmount(int amount, string matchID)
    {
        RpcSetHostCurrenctQuestionAmount(amount, matchID);
    }
    [ClientRpc]
    void RpcSetHostCurrenctQuestionAmount(int amount, string matchID)
    {
        if(localPlayer.matchID != matchID) return;
        if (localPlayer.isHost)
        {
            localPlayer.uiGame.currentQuestionAmount = amount;
            localPlayer.uiGame.hostQuestionAmountTxt.text = "$" + amount.ToString();
        }
        Debug.LogError("Current Question Amount has changed to: " + localPlayer.uiGame.currentQuestionAmount);
    }
    // current answer and question  
    internal void PlayerSetQuestionAndAnswer(string question, string answer)
    {
        CmdSetQuestionAndAnswer(question, answer, localPlayer.matchID);
    }
    [Command]
    void CmdSetQuestionAndAnswer(string question, string answer, string matchID)
    {
        RpcSetQuestionAndAnswer(question, answer, matchID);
    }
    [ClientRpc]
    void RpcSetQuestionAndAnswer(string question, string answer, string matchID)
    {
        if(localPlayer.matchID != matchID) return;
        localPlayer.uiGame.currentQuestion = question;
        localPlayer.uiGame.currentCorrectAnswer = answer;
        Debug.LogError("Current question and answer have been changed to: " + localPlayer.uiGame.currentQuestion + " " + localPlayer.uiGame.currentCorrectAnswer);
    }

    // current input answer
    internal void PlayerSetCurrentInputAnswer(string who, string answer)
    {
        CmdSetCurrentInputAnswer(who, answer, localPlayer.matchID);
    }
    [Command]
    void CmdSetCurrentInputAnswer(string who, string answer, string matchID)
    {
        RpcSetCurrentInputAnswer(who, answer, matchID);
    }
    [ClientRpc]
    void RpcSetCurrentInputAnswer(string who, string answer, string matchID)
    {
        if(localPlayer.matchID != matchID) return;
        localPlayer.uiGame.currentInputAnswer = answer;
        Debug.LogError("Current input answer hase been changed to: " + localPlayer.uiGame.currentInputAnswer);
        if (localPlayer.isHost)
        {
            localPlayer.uiGame.correctButton.SetEnable(true);
            localPlayer.uiGame.incorrectButton.SetEnable(true);
            localPlayer.uiGame.hostPauseBtn.interactable = false;
            localPlayer.uiGame.hostInputAnswerTxt.text = answer;
            localPlayer.uiGame.hostAnswerer.text = who;
        }
    }
    // current input answer amount
    internal void PlayerSetCurrentInputAnswerAmount(string who, string answer, int amount, int pidx)
    {
        CmdSetCurrentInputAnswerAmount(who, answer, amount, pidx, localPlayer.matchID);
    }
    [Command]
    void CmdSetCurrentInputAnswerAmount(string who, string answer, int amount, int pidx, string matchID)
    {
        RpcSetCurrentInputAnswerAmount(who, answer, amount, pidx, matchID);
    }
    [ClientRpc]
    void RpcSetCurrentInputAnswerAmount(string who, string answer, int amount, int pidx, string matchID)
    {
        if(localPlayer.matchID != matchID) return;
        localPlayer.uiGame.currentInputAnswer = answer;
        Debug.LogError("##  Current input answer hase been changed to: " + localPlayer.uiGame.currentInputAnswer);
        if (localPlayer.isHost)
        {
            if(localPlayer.answererList.Count == 0) {
                localPlayer.uiGame.correctButton.SetEnable(true);
                localPlayer.uiGame.incorrectButton.SetEnable(true);
                localPlayer.uiGame.hostPauseBtn.interactable = false;
                localPlayer.uiGame.hostInputAnswerTxt.text = answer;
                localPlayer.uiGame.hostAnswerer.text = who;
                localPlayer.uiGame.currentQuestionAmount = amount;
                localPlayer.uiGame.currentPlayerIndex = pidx;
                localPlayer.uiGame.hostQuestionAmountTxt.text = "$" + amount.ToString();
            }
            localPlayer.answererList.Add(who);
            localPlayer.answerList.Add(answer);
            localPlayer.amountList.Add(amount);
            localPlayer.playerIndexList.Add(pidx);
        }
    }

    internal void PlayerSetHasAnswered(bool has)
    {
        CmdSetHasAnswered(has, localPlayer.matchID, this.playerID);
    }
    [Command]
    void CmdSetHasAnswered(bool has, string matchid, string playerId)
    {
        this.hasAnswered = has;
        SyncListGameObject players = MatchMaker.instance.FindMatchById(matchid).playersInThisMatch;
        for (int i = 0; i < players.Count; i++)
        {
            if (players[i].GetComponent<Player>().playerID == playerId)
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
        if (localPlayer.uiGame.isDailyDoubleNow || localPlayer.uiGame.isFinalJeopardyNow)
        {
            this.playerAmount += amount * 2;
        }
        else
        {
            this.playerAmount += amount;
        }
        CmdPlayerAddAmount(this.playerAmount, this.playerIndex);
    }
    //Send a COMMAND and Target RPC each time the players money amout changes and change the UI accordinly
    [Command]
    internal void CmdPlayerAddAmount(int newAmount, int playerIndex)
    {
        this.playerAmount = newAmount;
        TargetUpdatePlayerUI(newAmount);
        // RpcUpdateSideContainerToAll(playerIndex, newAmount);
    }

    public void PlayerDeductAmount(int amount)
    {
        // if (this.playerAmount - amount >= 0)
        // {
            this.playerAmount -= amount;
            CmdPlayerDeductAmount(this.playerIndex, this.playerAmount);
        // }
    }
    [Command]
    internal void CmdPlayerDeductAmount(int index, int newAmount)
    {
        this.playerAmount = newAmount;
        TargetUpdatePlayerUI(newAmount);
        // RpcUpdateSideContainerToAll(index, newAmount);
    }
    [TargetRpc]
    void TargetUpdatePlayerUI(int newAmount)
    {
        if (this.uiPlayer == null)
            this.uiPlayer = GameObject.Find("UIPlayer").GetComponent<UIPlayerController>();
        this.uiPlayer.UpdateMyButtomContainer(newAmount);
    }
    [ClientRpc]
    void RpcUpdateSideContainerToAll(int myIndex, int newAmount, string matchID)
    {
        if(localPlayer.matchID != matchID) return;
        SidePanalController.instance.UpdateSideSlotAmount(myIndex, newAmount);
    }
    internal void PlayerPlaceDailyDouble()
    {
        // making sure player is host to call this only once, so that the daily double for everyone will on;;y be set once
        if (localPlayer.isHost)
            CmdPlaceDailyDouble(localPlayer.matchID);
    }
    [Command]
    void CmdPlaceDailyDouble(string matchID)
    {
        if (TransferDataToGame.instance.dailyDouble)
        {
            // choose a random spot to place the daily double and tell eveyone to do so
            int rnd = UnityEngine.Random.Range(0, 29);
            RpcPlaceDailyDouble(rnd, matchID);
        }
        else
            Debug.LogError("Should place daily double is set to false", this);
    }
    [ClientRpc]
    void RpcPlaceDailyDouble(int spot, string matchID)
    {
        if(localPlayer.matchID != matchID) return;
        if(localPlayer.uiGame == null) {
            UIGameController.doubleSlot = spot;
        } else {
            localPlayer.uiGame.PlaceDailyDouble(spot);
        }
    }
    //Send a COMMAND and Target RPC to let all the players know a player has buzzed in and thet cannot answer anymore
    //set the correct ui each time a player buzzed in
    //Send a COMMAND and Target RPC if no player has buzzed and reveal the answer

    internal void PlayerBuzzedIn()
    {
        CmdPlayerBuzzedIn(playerID, localPlayer.matchID);
        this.PlayerStopTimerForAllExceptMe();
    }
    [Command]
    void CmdPlayerBuzzedIn(string playerID, string matchID)
    {
        TargetPlayerBuzzedIn();
        RpcPlayerBuzzedIn(playerID, matchID);
    }
    [TargetRpc]
    void TargetPlayerBuzzedIn()
    {
        Debug.Log("Player buzzed");
        // runs only on the player that has buzzed in 
    }
    [ClientRpc]
    void RpcPlayerBuzzedIn(string playerID, string matchID)
    {
        if(localPlayer.matchID != matchID) return;
        // make it so  can't buzz
        if (localPlayer.playerID != playerID)
        {
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
        // localPlayer.PlayerDeductAmount(localPlayer.uiGame.currentQuestionAmount);
        // localPlayer.PlayerGiveTryTo();
        localPlayer.PlayerSetHasAnswered(true);
    }

    //Send a COMMAND and Target RPC each time the player answers wrong/ true
    //Send a COMMAND and Target RPC if player has submited a wrong answer, or buzzed but not submited, giving the other players a chance to buzz
    internal void PlayerSumbited(string answer)
    {
        if(localPlayer.uiGame.isFinalJeopardyNow) {
            localPlayer.PlayerSetCurrentInputAnswerAmount(localPlayer.playerName, answer, localPlayer.uiGame.currentQuestionAmount, localPlayer.playerIndex);
        }
        else {
            localPlayer.PlayerSetCurrentInputAnswer(localPlayer.playerName, answer);
        }
    }
    public void PlayerAddAmountTo(int playerIndex, int amount) {
        CmdPlayerAddAmountTo(playerIndex, amount, localPlayer.matchID);
    }
    [Command]
    void CmdPlayerAddAmountTo(int playerIndex, int amount, string matchID) {
        RpcPlayerAddAmountTo(playerIndex, amount, matchID);
    }
    [ClientRpc]
    void RpcPlayerAddAmountTo(int playerIndex, int amount, string matchID) {
        if(localPlayer.matchID != matchID) return;
        if(localPlayer.playerIndex == playerIndex) {
            localPlayer.PlayerAddAmount(amount);
        }
    }
    public void PlayerDeductAmountTo(int playerIndex, int amount) {
        CmdPlayerDeductAmountTo(playerIndex, amount, localPlayer.matchID);
    }
    [Command]
    void CmdPlayerDeductAmountTo(int playerIndex, int amount, string matchID) {
        RpcPlayerDeductAmountTo(playerIndex, amount, matchID);
    }
    [ClientRpc]
    void RpcPlayerDeductAmountTo(int playerIndex, int amount, string matchID) {
        if(localPlayer.matchID != matchID) return;
        if(localPlayer.playerIndex == playerIndex) {
            localPlayer.PlayerDeductAmount(amount);
        }
    }
    internal void PlayerHostDecided(bool correct)
    {
        CmdHostDecided(correct, localPlayer.matchID);
    }
    [Command]
    void CmdHostDecided(bool correct, string matchID)
    {
        Debug.LogError("Now answering indezx is " + TurnManager.instance.nowAnswering);
        if (correct)
            RpcPlayerSumbitedRight(TurnManager.instance.nowAnswering, matchID);
        else
        {
            RpcPlayerSumbitedWrong(TurnManager.CheckIfEveryoneAnswered(MatchMaker.instance.FindMatchById(matchID).playersInThisMatch), TurnManager.instance.nowAnswering, matchID);
        }
    }
    [ClientRpc]
    void RpcPlayerSumbitedRight(int whoAnswered, string matchID)
    {
        if(localPlayer.matchID != matchID) return;
        if (localPlayer.playerIndex == whoAnswered)
        {
            // need to call this on the player that has submited only
            Debug.Log("Answer was declared correct");
            //A correct response earns the dollar value of the question and the opportunity to select the next question from the board.
            localPlayer.PlayerAddAmount(localPlayer.uiGame.currentQuestionAmount);
            localPlayer.CmdOpenAnswerPanalToAll(matchID);
            //PlayerGiveTurnTo(localPlayer.playerIndex, true);

        }
    }

    [ClientRpc]
    void RpcPlayerSumbitedWrong(bool everyoneAnswered, int whoAnswered, string matchID)
    {
        if(localPlayer.matchID != matchID) return;
        if (localPlayer.playerIndex == whoAnswered)
        {
            Debug.Log("Answer was declared wrong");
            //An incorrect response or a failure to buzz in within the time limit deducts the dollar value of the question 
            //from the team's score and gives any remaining opponent(s) the opportunity to buzz in and respond.
            localPlayer.PlayerDeductAmount(localPlayer.uiGame.currentQuestionAmount);
            //change later
            //PlayerGiveTurnTo(localPlayer.playerIndex, true);
        } else if(localPlayer.isHost) {
            if (localPlayer.uiGame.isDailyDoubleNow) {
                localPlayer.PlayerOpenCorrectAnswerPanalToAll();
            }
            if (everyoneAnswered) //set everyoneAnswered as true so that continue button will show slots panel to all
            {
                localPlayer.uiGame.everyoneAnswered = true;
                PlayerOpenCorrectAnswerPanalToAll();
            }
        }
    }
    internal void PlayerStopTimerForAllExceptMe()
    {
        CmdStopTimerForAllExceptMe(this.playerIndex, localPlayer.matchID);
    }
    [Command]
    void CmdStopTimerForAllExceptMe(int index, string matchID)
    {
        RpcStopTimerForAllExceptMe(index, matchID);
    }
    [ClientRpc]
    void RpcStopTimerForAllExceptMe(int index, string matchID)
    {
        if(localPlayer.matchID != matchID) return;
        if (localPlayer.playerIndex != index && localPlayer.isHost == false)
        {
            Debug.Log("Stopping timer coutine");
            localPlayer.uiGame.StopTimerCoroutine();
        }
    }
    internal void PlayerStartTimerForAll(bool sumbit)
    {
        CmdStartTimerForAll(sumbit, localPlayer.matchID);
    }
    [Command]
    void CmdStartTimerForAll(bool sumbit, string matchID)
    {
        RpcStartTimerForAll(sumbit, matchID);
    }
    [ClientRpc]
    void RpcStartTimerForAll(bool sumbit, string matchID)
    {
        if(localPlayer.matchID != matchID) return;
        localPlayer.uiGame.StartTimerCoroutine(sumbit);
    }

    internal void PlayerStartTimerForHost(bool sumbit)
    {
        CmdStartTimerForHost(sumbit, localPlayer.matchID);
    }
    [Command]
    void CmdStartTimerForHost(bool sumbit, string matchID)
    {
        RpcStartTimerForHost(sumbit, matchID);
    }
    [ClientRpc]
    void RpcStartTimerForHost(bool sumbit, string matchID)
    {
        if(localPlayer.matchID != matchID) return;
        if (localPlayer.isHost)
            localPlayer.uiGame.StartTimerCoroutine(sumbit);
    }
    internal void PlayerStoptTimerForHost()
    {
        CmdStopTimerForHost(localPlayer.matchID);
    }
    [Command]
    void CmdStopTimerForHost(string matchID)
    {
        RpcStopTimerForHost(matchID);
    }
    [ClientRpc]
    void RpcStopTimerForHost(string matchID)
    {
        if(localPlayer.matchID != matchID) return;
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
    void CmdGiveTryTo(int lastIndex, string matchID)
    {
        // make sure this method is not called from the host
        SyncListGameObject players = MatchMaker.instance.FindMatchById(matchID).playersInThisMatch;
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
            RpcGiveTryTo(nextIndex, matchID);
        }
        else
            RpcGiveTryTo(lastIndex, matchID);
    }
    [ClientRpc]
    void RpcGiveTryTo(int indexToGive, string matchID)
    {
        if(localPlayer.matchID != matchID) return;
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
        CmdGiveTurnTo(lastIndex, me, localPlayer.matchID);
    }
    [Command]
    void CmdGiveTurnTo(int lastIndex, bool me, string matchID)
    {
        int indexToGive = lastIndex;
        if (me)
            RpcGiveTurnTo(lastIndex, matchID);
        else
        {
            RpcGiveTurnTo(indexToGive, matchID);

        }
    }
    [ClientRpc]
    void RpcGiveTurnTo(int indexToGive, string matchID)
    {
        if(localPlayer.matchID != matchID) return;
        localPlayer.uiGame.OpenSlotsPanel();
    }
    internal void PlayerPauseGameForAll()
    {
        CmdPauseGameForAll(localPlayer.matchID);
    }
    [Command]
    void CmdPauseGameForAll(string matchID)
    {
        RpcPauseGameForAll(matchID);
    }
    [ClientRpc]
    void RpcPauseGameForAll(string matchID)
    {
        if(localPlayer.matchID != matchID) return;
        // bool stopTimer = false;
        if (!localPlayer.isHost)
        {
            localPlayer.isSumbiting = false;
            localPlayer.isBuzzing = false;
            if (localPlayer.uiGame.buzzButton.gameObject.activeSelf && localPlayer.uiGame.buzzButton.isEnabled)
            {
                localPlayer.isBuzzing = true;
                localPlayer.uiGame.CantBuzz();
            }
            if (localPlayer.uiGame.submitButton.gameObject.activeSelf && localPlayer.uiGame.submitButton.isEnabled)
            {
                localPlayer.isSumbiting = true;
                localPlayer.uiGame.CantSumbit();
            }
        }
        else
        {

            if (localPlayer.uiGame.hostQuestionPanel.activeSelf)
            {
                localPlayer.canContinue = false;
                if (localPlayer.uiGame.hostContinueButton.isEnabled)
                {
                    localPlayer.canContinue = true;
                    localPlayer.uiGame.hostContinueButton.SetEnable(false);

                }
                // if the player was able to submit before, bring it back after unpausing 
                localPlayer.canDecide = false;
                if (localPlayer.uiGame.correctButton.isEnabled)
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
        CmdUnPauseGameForAll(localPlayer.matchID);
    }
    [Command]
    void CmdUnPauseGameForAll(string matchID)
    {
        RpcUnPauseGameForAll(matchID);
    }
    [ClientRpc]
    void RpcUnPauseGameForAll(string matchID)
    {
        if(localPlayer.matchID != matchID) return;
        if (localPlayer.isHost)
        {
            localPlayer.uiGame.hostUnpauseBtn.gameObject.SetActive(false);
            localPlayer.uiGame.hostPauseBtn.gameObject.SetActive(true);
            localPlayer.uiGame.hostPauseImg.SetActive(false);
            if (localPlayer.canContinue)
            {
                localPlayer.uiGame.hostContinueButton.SetEnable(true);
            }
            else
                localPlayer.uiGame.hostContinueButton.SetEnable(false);
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
            else if (localPlayer.isBuzzing)
            {
                localPlayer.uiGame.CanBuzz();

            }

        }
        localPlayer.uiGame.isPaused = false;

    }
    internal void PlayerGreyOutSlotForEveryone(int slotIndex)
    {
        CmdGreyOutSlotForEveryone(slotIndex, localPlayer.matchID);
    }
    [Command]
    void CmdGreyOutSlotForEveryone(int slotIndex, string matchID)
    {
        RpcGreyOutSlotForEveryone(slotIndex, matchID);
    }
    [ClientRpc]
    void RpcGreyOutSlotForEveryone(int slotIndex, string matchID)
    {
        if(localPlayer.matchID != matchID) return;
        localPlayer.uiGame.GreyOutSlot(slotIndex);
    }
    #endregion
    #endregion
    public void KickPlayer(string playerID)
    {
        CmdKickPlayer(playerID);
    }
    [Command]
    void CmdKickPlayer(string playerID)
    {
        SyncListGameObject players = MatchMaker.instance.FindMatchById(this.matchID).playersInThisMatch;
        for (int i = 0; i < players.Count; i++)
        {
            NetworkConnection connection = players[i].GetComponent<NetworkIdentity>().connectionToClient;
            if (playerID == players[i].GetComponent<Player>().playerID)
            {
                TargetKickPlayer(connection);
            }
        }
    }
    [TargetRpc]
    void TargetKickPlayer(NetworkConnection target)
    {
        Toast.instance.showToast("You were kicked by the host", 3);
        localPlayer.PlayerCancelJoin(localPlayer.matchID);
    }
    public void TintAllSlotsButOne(int playerIndex)
    {
        CmdTintAllSlotsButOne(playerIndex, localPlayer.matchID);
    }
    [Command]
    void CmdTintAllSlotsButOne(int playerIndex, string matchID)
    {
        RpcTintAllSlotsButOne(playerIndex, matchID);
    }
    [ClientRpc]
    void RpcTintAllSlotsButOne(int playerIndex, string matchID)
    {
        if(localPlayer.matchID != matchID) return;
        SidePanalController.instance.TintAllSlotsButOne(playerIndex);
    }
    public void UntintAllExceptAnswered()
    {
        CmdUntintAllExceptAnswered(localPlayer.matchID);
    }
    [Command]
    void CmdUntintAllExceptAnswered(string matchID) {
        RpcUntintAllExceptAnswered(matchID);
    }
    [ClientRpc]
    void RpcUntintAllExceptAnswered(string matchID) {
        if(localPlayer.matchID != matchID) return;
        SidePanalController.instance.UntintAllExceptAnswered();
    }
    public void CancelGame(string matchID) {
        CmdCancelGame(matchID);
    }
    [Command]
    void CmdCancelGame(string matchID) {
        Match match = MatchMaker.instance.FindMatchById(matchID);
        match.started = true;
    }

    public void GiveTurnToCurrentAnswerer() {
        CmdGiveTurnToCurrentAnswerer(localPlayer.matchID);
    }
    
    [Command]
    void CmdGiveTurnToCurrentAnswerer(string matchID) {
        TurnManager.instance.cardChooser = TurnManager.instance.nowAnswering;
        TurnManager.instance.lastCardWinner = TurnManager.instance.nowAnswering;
        RpcGiveTurnToCurrentAnswerer(TurnManager.instance.cardChooser, matchID);
    }

    [ClientRpc]
    void RpcGiveTurnToCurrentAnswerer(int cardChooser, string matchID) {
        if(localPlayer.matchID != matchID) return;
        SidePanalController.instance.TintAllSlotsButOne(cardChooser);
    }

    public void GiveTurnToLastWinner() {
        CmdGiveTurnToLastWinner(localPlayer.matchID);
    }
    
    [Command]
    void CmdGiveTurnToLastWinner(string matchID) {
        if(TurnManager.instance.lastCardWinner != -1)
            TurnManager.instance.cardChooser = TurnManager.instance.lastCardWinner;
        else {
            //Get current match
            Match thisMatch = MatchMaker.instance.FindMatchById(matchID);

            //Randomly choose starting player
            TurnManager.instance.RandomlyChooseStartingPlayer(TransferDataToGame.instance.gameSize, matchID);
        }
        RpcGiveTurnToLastWinner(TurnManager.instance.cardChooser, matchID);
    }

    [ClientRpc]
    void RpcGiveTurnToLastWinner(int cardChooser, string matchID) {
        if(localPlayer.matchID != matchID) return;
        SidePanalController.instance.TintAllSlotsButOne(cardChooser);
    }
    public void GiveTurnToRandomPlayer() {
        CmdGiveTurnToRandomPlayer(localPlayer.matchID);
    }
    [Command]
    void CmdGiveTurnToRandomPlayer(string matchID) {
        bool lastWinnerLeft = (TurnManager.instance.cardChooser == TurnManager.instance.lastCardWinner);
        TurnManager.instance.RandomlyChooseStartingPlayer(TransferDataToGame.instance.gameSize, matchID);
        if(lastWinnerLeft) TurnManager.instance.lastCardWinner = TurnManager.instance.cardChooser;
        RpcGiveTurnToRandomPlayer(TurnManager.instance.cardChooser, lastWinnerLeft, matchID);
    }
    [ClientRpc]
    void RpcGiveTurnToRandomPlayer(int newTurn, bool lastWinnerLeft, string matchID) {
        if(localPlayer.matchID != matchID) return;
        TurnManager.instance.cardChooser = newTurn;
        if(lastWinnerLeft) TurnManager.instance.lastCardWinner = newTurn;
        if(SidePanalController.instance != null) {
            SidePanalController.instance.TintAllSlotsButOne(newTurn);
        }
    }
    void OnDestroy()
    {
        if(isHost && localPlayer != this && localPlayer != null && matchID == localPlayer.matchID) {
            //When host is destoryed and he is not my match and i am not host
            Toast.instance.showToast("The host has left the game", 3);
            SceneManager.LoadScene("Lobby");
            localPlayer.CancelGame(localPlayer.matchID);
        } else if(localPlayer != null && localPlayer.isHost && !isHost && matchID == localPlayer.matchID) {
            //When I am host and one of participant has disconnected (player object destroyed)
            //Send message to every participant and me to show toast "somebody disconnected"
            localPlayer.OtherPlayerDisconnected(isHost, playerName, playerID, matchID, playerIndex);
        } else if(localPlayer == this || localPlayer == null) {
            //When my network is disconnected (my player object is destroyed)
            // Toast.instance.showToast("Network disconnected", 3);
        }
    }
    public void OpenDailyDoublePanalToAll() {
        CmdOpenDailyDoublePanalToAll(this.playerID, this.matchID);
    }

    [Command]
    void CmdOpenDailyDoublePanalToAll(string playerID, string matchID) {
        RpcOpenDailyDoublePanalToAll(playerID, matchID);
    }

    [ClientRpc]
    void RpcOpenDailyDoublePanalToAll(string playerID, string matchID) {
        if(localPlayer.matchID != matchID) return;
        if(!localPlayer.isHost) localPlayer.uiGame.OpenDailyDoublePanal(localPlayer.playerID == playerID);
    }
}
