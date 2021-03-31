using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System;

public class GameController : MonoBehaviour
{
    /*[Header("UI")]
    public TMP_Text remeiningQuestions, openQuestionAmountText ,openAnswerAmountText, openQuestionCatText, openAnswerCatText;
    [Header("Sprites")]
    public Sprite greyedOutSlot;

    [Header("Values")]
    internal string playerName;
    public static GameController instance;
    private TransferDataToGame participanteController;
    internal bool haveAllPlayersAnswered , PlayerHasWageredIn;
    public Player currentPlayer;
    public UIPlayerController UIPlayer;
    public UIGameController panalControllerInstance;
    public int currentQuestionAmount, unanswered;
    internal string  currentQuestion, currentCorrectAnswer, inputAnswer;

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
        private void Start()
    {
        participanteController = TransferDataToGame.instance;
        haveAllPlayersAnswered = true;
        PlayerHasWageredIn = false;
        unanswered = 30;
        currentPlayer = GameObject.Find("Player").GetComponent<Player>();
    }


    public void AddAmount(int amount)
    {
        if (PlayerHasWageredIn)
        {
            UIPlayer.AddAmount( amount*2);
        }
        else
        {
            UIPlayer.AddAmount( amount);
        }
    }
    public void DeductAmount(int amount)
    {
        Debug.Log("Deduct is called");
        UIPlayer.DeductAmount(amount);
    }
    
    public bool CheckIfCorrectAnswer(string inputAnswer)
    {
        bool correct = false;
        if (currentCorrectAnswer == inputAnswer || inputAnswer == "demo" || inputAnswer == "Demo")
        {
            correct = true;
            CorrectAnswer();
        }
        else
        {
            IncorrectAnswer();      
        }
        Debug.Log("Answer is " + correct);
        Debug.Log( currentQuestionAmount);
        return correct;
    }
    public void CorrectAnswer()
    {
        //A correct response earns the dollar value of the question and the opportunity to select the next question from the board.
        AddAmount(currentQuestionAmount);
        GiveTurnTo("the one that gave the correct answer");

    }
    public void IncorrectAnswer()
    {
        //An incorrect response or a failure to buzz in within the time limit deducts the dollar value of the question 
        //from the team's score and gives any remaining opponent(s) the opportunity to buzz in and respond.

        DeductAmount(currentQuestionAmount);
        GiveTurnTo("remaining opponent(s)");
    }
    public void GiveTurnTo(string who)
    {
        // gives the opportunity to select the next question from the board.
    }
    public void NoOneBuzzedIn()
    {
        // if no one has buzzed in, go  to the answer panel then back to the board 
        if (PlayerHasWageredIn)
        {
            // if the question is daily double or final jeopardy and the player has not buzzed in or submitted an answer 
            // reduce the amount he has wagered in and make in not possible for other player to answer 
            Debug.Log(currentQuestionAmount);
            DeductAmount(currentQuestionAmount);
        }
        StartCoroutine(panalControllerInstance.OpenAnswerInDelay());
        Debug.Log("No one has buzzed in time");
    }

    public void DidntSubmit()
    {
        //a failure to submit in within the time limit deducts the dollar value of the question
        //from the team's score and gives any remaining opponent(s) the opportunity to buzz in and respond.
        IncorrectAnswer();
        StartCoroutine(panalControllerInstance.OpenAnswerInDelay());
        Debug.Log("Player didn't sumbit their answer in time");

    }

    public void QuestionClicked(AmountSlot slot)
    {
        Debug.Log("Question Clicked");
        currentCorrectAnswer = slot.answer;
        currentQuestion = slot.question;
        currentQuestionAmount = slot.amout;
        if (slot.dailyDouble == false)
            panalControllerInstance.OpenQuesionPanel();
        else
            panalControllerInstance.OpenDailyDoublePanal();

        unanswered--;
        remeiningQuestions.text = unanswered + "/30";
        //openQuestionAmountText.text = "$"+currentQuestionAmount.ToString();
        //openAnswerAmountText.text = "$"+currentQuestionAmount.ToString();
        openAnswerCatText.text = slot.catagoryName;
        openQuestionCatText.text = slot.catagoryName;
        slot.gameObject.GetComponent<Image>().sprite = greyedOutSlot;
        //slot.gameObject.GetComponent<Button>().onClick.RemoveAllListeners();
        slot.gameObject.GetComponent<Button>().enabled = false;
    }
     public Player FindWinner()
    {
        return currentPlayer;
    }*/
}
