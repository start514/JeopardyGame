using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DontDestroyOnLoad : MonoBehaviour
{
    public GameObject pausePanal, settingsPanel, infoPanel, leaderboardPanel;
    public static DontDestroyOnLoad instance;

    void OnEnable()
    {
        if (instance != null && instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            instance = this;
        }
    }
    public void Start()
    {
        DontDestroyOnLoad(this.gameObject);
    }
    public void OpenPausePanal()
    {
        Debug.Log("Open pause panal");
        pausePanal.SetActive(true);
    }
    public void SettingsButton()
    {
        pausePanal.GetComponent<UIPausePanal>().ChangeShould(false);
        settingsPanel.SetActive(true);
    }
    public void InfoButton()
    {
        pausePanal.GetComponent<UIPausePanal>().ChangeShould(false);
        infoPanel.SetActive(true);
    }
    public void LeaderboardButton()
    {
        pausePanal.GetComponent<UIPausePanal>().ChangeShould(false);
        leaderboardPanel.SetActive(true);
    }

}
