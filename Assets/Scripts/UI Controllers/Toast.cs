using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Toast : MonoBehaviour
{
    public TextMeshProUGUI txt;
    public GameObject popup;
    public static Toast instance;
    // Start is called before the first frame update
    void Awake()
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

    public void showToast(string text,
        int duration)
    {
        txt.text = text;
        popup.SetActive(true);
    }
}
