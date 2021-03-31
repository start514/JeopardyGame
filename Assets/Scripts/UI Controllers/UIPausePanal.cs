using UnityEngine.UI;
using UnityEngine;
using UnityEngine.SceneManagement;


public class UIPausePanal : MonoBehaviour
{
    public Button settings, leaveGame, LeaderBoard, instructions;
    public GameObject settingsPanal, infoPanal, leaderBoardPanal, pausePanal;
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
        pausePanal.SetActive(false);
        // deleting objects so there won't be twice in the scene by accident 
        //Destroy(Player.localPlayer.gameObject);
        LoadScene("Start");
        //Application.Quit();
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
