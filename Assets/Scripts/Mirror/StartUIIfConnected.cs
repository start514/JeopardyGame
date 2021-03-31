using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartUIIfConnected : MonoBehaviour
{
    public static StartUIIfConnected instance;
    public GameObject ui;
    // Start is called before the first frame update
    private void Awake()
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
    public void ActivateUI()
    {
        ui.SetActive(true);
    }
}
