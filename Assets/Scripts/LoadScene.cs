using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadScene : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void LoadSceneName(string name)
    {
        SceneManager.LoadScene(name);
    }

    public void SettingsButton()
    {
        GameObject.Find("Dont Destroy On Land Canvas").GetComponent<DontDestroyOnLoad>().SettingsButton();
    }
    public void InfoButton()
    {
        GameObject.Find("Dont Destroy On Land Canvas").GetComponent<DontDestroyOnLoad>().InfoButton();
    }
    public void LeaderboardButton()
    {
        GameObject.Find("Dont Destroy On Land Canvas").GetComponent<DontDestroyOnLoad>().LeaderboardButton();
    }
}
