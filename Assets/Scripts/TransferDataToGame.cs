using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using Mirror;

public class TransferDataToGame : NetworkBehaviour
{
    public static TransferDataToGame instance;

    internal int participantesNumber= -1;
    public string participanteName = "";
    internal string hosteName= "";
    [SyncVar] public string gameName;
    [SyncVar] public int  gameSize;
    [SyncVar] public bool  dailyDouble;
    [SyncVar] public int timeToAnswer;
    [SyncVar] public int timeToBuzz;

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
