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
        SceneManager.LoadScene("Start", LoadSceneMode.Additive);
    }
}
