using Mirror;
using System.Collections.Generic;
using UnityEngine;
public class TurnManager : NetworkBehaviour
{
    // int startingPlayer;
    public static TurnManager instance;
    [SyncVar ]public  int nowAnswering;
    void Awake()
    {
        DontDestroyOnLoad(this);
        if (instance != null && instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            instance = this;
        }
    }

    /*
    public Player NextTurn()
    {
        currentPlayerInList++;
        if (currentPlayerInList == allPlayers.Count) // if this is the last player in the list turn;
        {
            currentPlayerInList = 0;
        }
        return allPlayers[currentPlayerInList].GetComponent<Player>();
    }
    public int RandomlyChooseStartingPlayer()
    {
        int count = allPlayers.Count; // number of players
        int rnd = Random.Range(0, count);
        return rnd;

    }*/
    public static bool CheckIfEveryoneAnswered(SyncListGameObject allPlayers)
    {
       
        for (int i = 0; i < allPlayers.Count; i++)
        {
            if (allPlayers[i].GetComponent<Player>().hasAnswered == false)
            {
                Debug.LogError("Everyone answered = false" );
                return false;
            }
        }
        Debug.LogError("Everyone answered = true" );
        return true;
    }

    public static void FindWinner(SyncListGameObject allPlayers)
    {

    }

   // public static int WhosNextInTurn(int lastIndex, SyncListGameObject players)
    //{
       
    //}
}
