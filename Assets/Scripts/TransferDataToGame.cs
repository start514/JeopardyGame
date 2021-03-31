using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class TransferDataToGame : MonoBehaviour
{
    public static TransferDataToGame instance;

    internal int participantesNumber= -1;
    public string participanteName = "";
    internal string hosteName= "";
    public string gameName;
    public int  gameSize;
    public bool  dailyDouble;
    public int timeToAnswer;
    public int timeToBuzz;

    // Update is called once per frame

    void Awake()
    {
        DontDestroyOnLoad(this);
        // using singleton
        if (instance == null)
            instance = this;
        else if (instance != this)
            Destroy(this);
    }
   
}
