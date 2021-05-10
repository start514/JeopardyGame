using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public delegate bool CallBack();  

public class ConfirmPopup : MonoBehaviour
{
    private CallBack callback;

    public void show(CallBack callback)
    {
        this.callback = callback;
        gameObject.SetActive(true);
    }

    public void yes() {
        gameObject.SetActive(false);
        callback.Invoke();
    }
}
