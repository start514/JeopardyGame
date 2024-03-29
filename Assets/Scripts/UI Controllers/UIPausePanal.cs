﻿using UnityEngine.UI;
using UnityEngine;
using UnityEngine.SceneManagement;


public class UIPausePanal : MonoBehaviour
{
    public Button settings, leaveGame, LeaderBoard, instructions;
    public GameObject settingsPanal, infoPanal, leaderBoardPanal, pausePanal;
    public ConfirmPopup confirmPopup;
    public bool shouldOpen = false;
    // Start is called before the first frame update

    public void LoadScene(string name)
    {
        Debug.Log("Switching Scenes To " + name);
        SceneManager.LoadScene(name);
    }
    public void OpenSettings()
    {
        Debug.Log("Open settings panal");
        settingsPanal.SetActive(true);
        pausePanal.SetActive(false);
        infoPanal.SetActive(false);
        leaderBoardPanal.SetActive(false);

    }
    public void OpenLeaderBoard()
    {
        infoPanal.SetActive(false);
        pausePanal.SetActive(false);
        settingsPanal.SetActive(false);
        leaderBoardPanal.SetActive(true);


    }
    public void OpenInstructions()
    {
        infoPanal.SetActive(true);
        pausePanal.SetActive(false);
        settingsPanal.SetActive(false);
        leaderBoardPanal.SetActive(false);
    }
    public void LeaveGame()
    {
        confirmPopup.show(delegate {
            pausePanal.SetActive(false);
            // deleting objects so there won't be twice in the scene by accident 
            //Destroy(Player.localPlayer.gameObject);
            if(Player.localPlayer!=null) {
                //send message that i am leaving game
                Player.localPlayer.PlayerLeaveGame();
            }
            LoadScene("Start");
            //Application.Quit();
            return true;
        });
    }

    public void Open()
    {
        if(shouldOpen)
        {
            pausePanal.SetActive(true);
        }
    }

    public void ChangeShould(bool temp)
    {
        shouldOpen = temp;
    }
}
