using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System.Security.Cryptography;
using System.Text;
using System;

[SerializeField]
public class Match
{
    // options:
    //instatiate a turn manager on all players, use [syncvar] on the varriables and send a client rpc each time it is changed
    // use the match maker as so
    //cons: takes a lot of energy to find match each time
    // pros: if a players enters the game mid- way, all of the data will be saved
    // how to get the players list to the turn manager? who has it - the 
    // keep it in the match and send a clientrpc each time we change it

    // data transfer values are here now
    // other data will be on the ui controller and will be updated via client rpc
    public string matchId, gameName;
    public int maxGameSize;
    public SyncListGameObject playersInThisMatch = new SyncListGameObject();
    public int timeToAnswer, timeToBuzz;
    public bool shouldActicateDailyDouble = true;
    public Match(string matchId, string gamaName, int maxGameSize, GameObject player)
    {
        this.matchId = matchId;
        this.gameName = gamaName;
        this.maxGameSize = maxGameSize;
        this.playersInThisMatch.Add(player);


    }
    public Match() { }
}

[SerializeField]
public class SyncListGameObject : SyncList<GameObject> { }

[SerializeField]
public class SyncListMatch : SyncList<Match> { }


public class MatchMaker : NetworkBehaviour
{
    public static MatchMaker instance;
    public UILobbyController uiLobby;
    // all of the matches (games) currently available and running
    public SyncListMatch allGames = new SyncListMatch();
    // all of the ids that are being used 
    public SyncListString allGamesIDs = new SyncListString();
    [SerializeField]
    private GameObject turnManagerPrefab;
    public void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            instance = this;
        }
        DontDestroyOnLoad(this.gameObject);
    }

    private void Update()
    {
        //if (allGamesIDs.Count >= 1)
        //Debug.Log(allGames[0].playersInThisMatch.Count);
        // there is a problem here !!! maybe with the SyncListGameObject
        // Debug.Log("num of games ids: "+allGamesIDs.Count);  
        //PrintPlayersInGames();
    }

    public void PrintPlayersInGames()
    {
        for (int i = 0; i < allGames.Count; i++)
        {
            //allGames[i].playersInThisMatch.Add(Player.localPlayer.gameObject);

            Debug.Log(allGames[i].matchId + " game has players- " + allGames[i].playersInThisMatch.Count);
        }
        {
            /*for (int k = 0; k < allGames[i].playersInThisMatch.Count; k++)
            {

                Debug.Log(allGames[i].matchId + " id with player name " + allGames[i].playersInThisMatch[k].GetComponent<Player>().PlayerName);
            }*/
        }
    }
    public static string CreateRandomID()
    {
        string id = "";
        for (int i = 0; i < 5; i++)
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
    public static Guid RegularIDToGUI(string id)
    // turn the regular id to a 13 digits gui id
    {
        MD5CryptoServiceProvider provider = new MD5CryptoServiceProvider();
        byte[] inputBytes = Encoding.Default.GetBytes(id);
        byte[] hashBytes = provider.ComputeHash(inputBytes);
        return new Guid(hashBytes);
    }
    public bool AddAndApproveHostingGame(string id, string gameName, int gameSize, GameObject player)
    // valadation purpuses
    {
        if (allGamesIDs.Contains(id) == false)
        {
            Match match = new Match(id, gameName, gameSize, player);
            allGames.Add(match);
            allGamesIDs.Add(id);
            //Debug.Log("true");
            return true;
        }
        else
        {

            //Debug.Log("false");
            Debug.LogError("Match id already exists");
            return false;
        }

    }
    public bool AddAndApproveJoiningGame(string id, GameObject player)
    // valadation purpuses
    {
        if (allGamesIDs.Contains(id))
        // add ourselfs into this match
        {
            for (int i = 0; i < allGames.Count; i++)
            {
                Debug.Log("id found: " + allGames[i].matchId);

                if (allGames[i].matchId == id)
                {
                    if (allGames[i].playersInThisMatch.Count - 1 == allGames[i].maxGameSize)
                    {
                        // meaning we hit the max num of players that can join
                        Debug.LogError("hit max players in game", this);
                        return false;
                        //add this later
                        //Player.localPlayer.CmdDeleteGameContainer(id);
                    }
                    allGames[i].playersInThisMatch.Add(player);
                    break;
                }
            }
            Debug.Log("Match joined");
            return true;
        }
        else
        {
            Debug.LogError("Match id doesn't exists");
            return false;
        }
    }

    internal Match FindMatchById(string id)
    {
        for (int i = 0; i < allGames.Count; i++)
        {
            if (allGames[i].matchId == id)
                return allGames[i];
        }
        // return just something so the method will wprk
        return null;
    }

    public bool DeleteMatch(string id)
    {
        Match match = FindMatchById(id);
        if (match != null)
        {
            allGames.Remove(match);
            allGamesIDs.Remove(id);
            return true;
        }
        return false;
    }
    public bool RemovePlayerFromMatch(string id, Player localPlayer)
    {
        Match match = FindMatchById(id);
        if (match != null)
        {
            // look for our player
            for (int i = 0; i < match.playersInThisMatch.Count; i++)
            {
                Player p = match.playersInThisMatch[i].GetComponent<Player>();
                if (p == localPlayer)
                {
                    match.playersInThisMatch.Remove(match.playersInThisMatch[i]);
                    return true;
                }
            }
        }
        return false;
    }
}
