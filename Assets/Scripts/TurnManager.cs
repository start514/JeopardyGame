using Mirror;
using System.Collections.Generic;
using UnityEngine;
public class TurnManager : NetworkBehaviour
{
    // int startingPlayer;
    public static TurnManager instance;
    [SyncVar] public  int nowAnswering;
    [SyncVar] public  int cardChooser;
    [SyncVar] public  int lastCardWinner;
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
    public int RandomlyChooseStartingPlayer(int playerCount)
    {
        int rnd = Random.Range(0, playerCount - 1);
        cardChooser = rnd;
        return rnd;
    }

    public void GiveTurnToCurrentAnswerer() {
        Player.localPlayer.GiveTurnToCurrentAnswerer();
    }

    public void GiveTurnToLastWinner() {
        Player.localPlayer.GiveTurnToLastWinner();
    }

    public static bool CheckIfEveryoneAnswered(SyncListGameObject allPlayers)
    {
       
        for (int i = 0; i < allPlayers.Count; i++)
        {
            if (allPlayers[i].GetComponent<Player>().isHost == false && allPlayers[i].GetComponent<Player>().hasAnswered == false)
            {
                Debug.LogError("Everyone answered = false" );
                return false;
            }
        }
        Debug.LogError("Everyone answered = true" );
        return true;
    }
}
