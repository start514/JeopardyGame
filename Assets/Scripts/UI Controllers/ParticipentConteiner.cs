using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class ParticipentConteiner : MonoBehaviour
{

    public Image color, role, inputBG;
    public string colorString, playerID, matchID;
    public Text nameTxt;
    public InputField inputField;
    public GameObject readyMark;
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        Player[] players = GameObject.FindObjectsOfType<Player>();
        for(int i=0; i<players.Length; i++) {
            if(players[i].playerID == playerID) {
                if(players[i].isHost) {
                    readyMark.SetActive(false);
                } else {
                    readyMark.SetActive(players[i].isReady);
                }
            }
        } 
    }

    public void ChangeColor() {
        Player.localPlayer.ChangeColor();
    }

    public void KickPlayer() {
        Player.localPlayer.KickPlayer(playerID);
    }
}
