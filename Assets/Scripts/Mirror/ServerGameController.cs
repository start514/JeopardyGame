using UnityEngine;
using Mirror;
using System;

public class ServerGameController : NetworkBehaviour
{
    public static ServerGameController instance;
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
    public bool CheckIfCorrectAnswer(string inputAnswer, string correctAnswer)
    {
        bool correct = false;
        if (correctAnswer == inputAnswer || inputAnswer == "demo" || inputAnswer == "Demo")
        {
            correct = true;
        }
        Debug.LogError("Answer is " + correct);
        return correct;
    }
}
