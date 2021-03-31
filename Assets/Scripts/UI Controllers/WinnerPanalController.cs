using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class WinnerPanalController : MonoBehaviour
{
    public TMP_Text nameTxt, amountTxt; 

    public void PlayAgainButton()
    {
        // deleting objects so there won't be twice in the scene by accident 
        Destroy(Player.localPlayer.gameObject);
        SceneManager.LoadScene("Start", LoadSceneMode.Additive);
    }
}
