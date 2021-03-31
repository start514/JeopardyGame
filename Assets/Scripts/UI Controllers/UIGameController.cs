﻿using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class UIGameController : MonoBehaviour
{
    [Header("Panels")]
    public GameObject finalPanal, dailyPanel, winnerPanel, slotsPanel, clientQuestionPanel, clientAnswerPanel, hostQuestionPanel, amountChoser, clientBottonPanal;


    [Header("UI")]
    public TMP_Text clientQuestionTimerText, hostTimerText, answerTimerText, amountChoserText, clientQuestionAmountTxt, hostQuestionAmountTxt,
        clientAnswerAmountText, clientQuestionText, clientAnswerTxt, hostQuestionText, hostCatagotyTxt, clientQuestionCatagotyTxt, clientAnswerCatagotyTxt, remeiningQuestions, hostPlayerContainerTxt;
    public Text hostRightAnswerTxt, hostInputAnswerTxt;
    public Text whereIsDailyDouble;
    public InputField inputFieldAnswer;
    public CustomButton submitButton, buzzButton;
    public CustomInputField answerInput;
    public GameObject HostBottom;
    public CustomButton correctButton, incorrectButton;
    public GameObject singleIcon, doubleIcon;
    [Header("Sprites")]
    public Sprite notGreyedOutSlot, greyedOutSlot, singleCatagory, doubleCatagory;
    public Button hostContinueButton, hostPauseBtn,hostUnpauseBtn;
    public GameObject hostPauseImg;
    [Header("Scripts")]
    public static UIGameController instance;
    public SidePanalController sidePanalController;
    public ClientGameController clientGameController;
    Player localPlayer;

    [Header("Values")]
    public bool timerRunning;
    public int jumpsAmount = 100;
    private float timeToAnswer = 120, timeToBuzz = 120;
    internal Coroutine buzzTimer, submitTimer, backToBoard;
    internal string finalQuestion, finalAnswer;
    public string currentQuestion, currentCorrectAnswer, currentInputAnswer;
    public bool isDailyDoubleNow = false, haveAllPlayersAnswered = false, isSecondBoardNow = false, isFinalJeopardyNow = false;
    public int currentQuestionAmount, questionsLeft = 30;
    public JsonToC jsonToCScript;
    public int openAnswerDelay = 0, closeAnswerDelay = 6;
    public AmountSlot[] allSlots;
    public CatagorySlot[] allCatagories;
    // delete later
    public Button final, doubleBtn;
    public bool isPaused = false;

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
        localPlayer = Player.localPlayer;
        localPlayer.uiGame = this;
        //transferDataInstance = TransferDataToGame.instance;
        OpenSingleJeopardyPanal();
        Player.localPlayer.PlayerPlaceDailyDouble();
        timerRunning = false;
        isDailyDoubleNow = false;
        haveAllPlayersAnswered = false;
        isSecondBoardNow = false;
        isFinalJeopardyNow = false;
        questionsLeft = 30;
        isPaused = false;
        //buzzTimer = ActivateTimer(timeToBuzz, false); 
        //submitTimer = ActivateTimer(timeToAnswer, true);
        submitTimer = StartCoroutine(ActivateTimer(timeToAnswer, true));
        buzzTimer = StartCoroutine(ActivateTimer(timeToBuzz, false));
        StopTimerCoroutine();
        //backToBoard = GoBackToBoard(closeAnswerDelay);

        if (localPlayer.isHost)
        {
            HostBottom.SetActive(true);
        }
        // load the first board
        if (allSlots.Length != 30)
        {
            Debug.LogError("Not enough amounts slots", this);
            return;
        }
        if (allCatagories.Length != 6)
        {
            Debug.LogError("Not enough catagory slots", this);
            return;
        }

        // for later demo
        //timeToBuzz = Player.localPlayer.PlayerGetTimeToBuzz();
        //timeToAnswer = Player.localPlayer.PlayerGetTimeToAnswer();
    }

    // Update is called once per frame
    void Update()
    {
        if (localPlayer.isHost)
        {
            if (isSecondBoardNow == false && questionsLeft == 0 && isFinalJeopardyNow == false)
            {
                // meaning all of the questions on the first board have been answered
                localPlayer.PlayerOpenDoubleJeopardyPanal();
            }
            else if (isSecondBoardNow == true && questionsLeft == 0)
            // meaning you have finished the second board, and go to final jeopardy
            {
                localPlayer.PlayerOpenFinalJeopardyPanalToAll();
            }
            hostPlayerContainerTxt.text = localPlayer.playerName;
        }
    }

    public void QuestionClicked(AmountSlot slot)
    {
        clientAnswerCatagotyTxt.text = slot.catagoryName;
        clientQuestionCatagotyTxt.text = slot.catagoryName;
        hostCatagotyTxt.text = slot.catagoryName;
        clientQuestionAmountTxt.text = "$" + currentQuestionAmount;
        remeiningQuestions.text = questionsLeft + "/30";
        Debug.Log("Slot Amount = " + slot.amout);
        localPlayer.PlayerSetCurrenctQuestionAmount(slot.amout);
        localPlayer.PlayerSetQuestionAndAnswer(slot.question, slot.answer);
        localPlayer.PlayerGreyOutSlotForEveryone(slot.slotIndex);
        if (slot.dailyDouble)
        {
            localPlayer.PlayerSetIsDailyDouble(true);
            OpenDailyDoublePanal();
            slot.dailyDouble = false;
        }
        else
            localPlayer.PlayerOpenQuestionPanalToAll();
    }

    public void GreyOutSlot(int slotIndex)
    {
        Debug.Log("slot indez = " + slotIndex);
        allSlots[slotIndex].gameObject.GetComponent<Image>().sprite = greyedOutSlot;
        allSlots[slotIndex].gameObject.GetComponent<Button>().onClick.RemoveAllListeners();
        allSlots[slotIndex].gameObject.GetComponent<Button>().enabled = false;
    }
    #region  PANELS CONTROLLERS
    public void OpenSlotsPanel()
    {
        
        slotsPanel.SetActive(true);
        clientQuestionPanel.SetActive(false);
        clientAnswerPanel.SetActive(false);
        hostQuestionPanel.SetActive(false);
        buzzButton.gameObject.SetActive(true);
        buzzButton.SetEnable(false);
        submitButton.gameObject.SetActive(false);
        inputFieldAnswer.text = "";
        localPlayer.PlayerSetHasAnswered(false);
        if (isDailyDoubleNow)
        {
            localPlayer.PlayerSetIsDailyDouble(false);
        }
        if (localPlayer.isHost)
        {
            hostContinueButton.gameObject.SetActive(false);
            localPlayer.canDecide = false;
            localPlayer.canContinue = false;
            hostPauseBtn.interactable = false;

        }
        else
        {
            localPlayer.isSumbiting = false;
            localPlayer.isBuzzing = false;
        }
    }
    public void OpenClientQuesionPanel()
    {
        // all players except host have x time to buzz in
        Debug.Log("opened Question panal");
        //wiring the qestion text 
        clientQuestionText.text = currentQuestion;
        clientQuestionAmountTxt.text = "$" + currentQuestionAmount.ToString();
        slotsPanel.SetActive(false);
        clientQuestionPanel.SetActive(true);
        if (isDailyDoubleNow || isFinalJeopardyNow)
        {
            // the player doesnt need to buzz in to answer, it happens automaticaly
            // change tint
            //sidePanalController.TintAllSlotsButOne(3);
            answerInput.SetEnable(true);
            submitButton.gameObject.SetActive(true);
            submitButton.SetEnable(true);
            buzzButton.gameObject.SetActive(false);
            StartTimerCoroutine(true);
            localPlayer.PlayerStartTimerForHost(true);

        }
        else
        {
            buzzButton.gameObject.SetActive(true);
            buzzButton.SetEnable(true);
            submitButton.gameObject.SetActive(false);
            answerInput.SetEnable(false);
            StartTimerCoroutine(false);
            localPlayer.PlayerStartTimerForHost(false);

            if (Mathf.FloorToInt(timeToBuzz % 60) == 0)
                clientQuestionTimerText.text = Mathf.FloorToInt(timeToBuzz / 60) + ":00";
            else
                clientQuestionTimerText.text = Mathf.FloorToInt(timeToBuzz / 60) + ":" + Mathf.FloorToInt(timeToBuzz % 60);
        }
        
        if (dailyPanel.activeSelf == true)
        {
            dailyPanel.SetActive(false);
            amountChoser.SetActive(false);
        }
        if (finalPanal.activeSelf == true)
        {
            finalPanal.SetActive(false);
            amountChoser.SetActive(false);
        }
    }
    public void OpenHostQuesionPanel()
    {
        Debug.Log("opened Question panal");
        slotsPanel.SetActive(false);
        //wiring the qestion text 
        hostQuestionText.text = currentQuestion;
        hostInputAnswerTxt.text = "";
        hostPauseBtn.interactable = true;
        hostRightAnswerTxt.text = currentCorrectAnswer;
        hostQuestionAmountTxt.text = "$"+currentQuestionAmount.ToString();
        hostQuestionPanel.SetActive(true);
    }
    public void OpenClientAnswerPanel()
    {
        slotsPanel.SetActive(false);
        clientAnswerPanel.SetActive(true);
        submitButton.SetEnable(false);
        submitButton.gameObject.SetActive(true);
        answerInput.SetEnable(false);
        clientAnswerTxt.text = currentInputAnswer;
        clientAnswerAmountText.text = "$"+currentQuestionAmount.ToString();
    }
    public void OpenSingleJeopardyPanal()
    {
        singleIcon.SetActive(true);
        doubleIcon.SetActive(false);
        for (int k = 0; k < allCatagories.Length; k++)
        {
            allCatagories[k].gameObject.GetComponent<Image>().sprite = singleCatagory;

        }
    }
    public void OpenDoubleJeopardyPanal()
    {
        // one all of the question on the single board are done, load the double board
        singleIcon.SetActive(false);
        doubleIcon.SetActive(true);
        remeiningQuestions.text = "30/30";
        for (int i = 0; i < allSlots.Length; i++)
        {
            allSlots[i].GetComponent<Button>().onClick.RemoveAllListeners();
            allSlots[i].DoubleJeopardySlots();
            allSlots[i].gameObject.GetComponent<Image>().sprite = notGreyedOutSlot;
        }
        for (int k = 0; k < allCatagories.Length; k++)
        {
            allCatagories[k].gameObject.GetComponent<Image>().sprite = doubleCatagory;
        }
    }
    public void OpenDailyDoublePanal()
    {
        // בכל אחד מהלוחות תהיה שאלת daily double אחת והיא צריכה להיבחר רנדומלית
        // change daily double to true
        localPlayer.PlayerSetCurrenctQuestionAmount(0);
        Debug.Log("Daily doubly");
        dailyPanel.SetActive(true);
        slotsPanel.SetActive(false);
        amountChoser.SetActive(true);
        localPlayer.PlayerOpenHostQuestionPanal();
    }
    public void DailyDoubleContunieButton()
    {
        // one player has clicked the daily double slot
        // he wagers in and clicks this button
        // open question panal just for me
        OpenClientQuesionPanel();

    }
    public void FinalJeopardyContunieButton()
    {
        OpenClientQuesionPanel();
    }
    public void OpenFinalJeopardyPanal()
    {
        // only the 3 participents with the highest dollars reach final jeopardy
        // ובדומה לdaily double הם בוחרים את הסכום שהם רוצים לסכן (מ0 עד לכל הסכום שהם צברו)
        // once all 3 participents have choosen their amounts 
        //open the question panel

        slotsPanel.SetActive(false);
        finalPanal.SetActive(true);
        amountChoser.SetActive(true);
        // change gameControllerInstance.currentCorrectAnswer = finalAnswer;
        // change gameControllerInstance.currentQuestion = finalQuestion;
        if (localPlayer.isHost)
        {
            finalQuestion = jsonToCScript.finalRoot.Questions[0].question;
            finalAnswer = jsonToCScript.finalRoot.Questions[0].Answer;
            localPlayer.PlayerSetQuestionAndAnswer(finalQuestion, finalAnswer);
        }
        amountChoserText.text = "$0";
    }
    public void OpenWinnerPanel(int winnerAmount, string winnerName)
    {
        amountChoser.SetActive(false);
        winnerPanel.SetActive(true);
        WinnerPanalController script = winnerPanel.GetComponent<WinnerPanalController>();
        script.amountTxt.text = "$" + winnerAmount.ToString();
        script.nameTxt.text = winnerName.ToString();
    }
    #endregion
    #region CLIENT METODS
    public void PlaceDailyDouble(int spot)
    {
        allSlots[spot].dailyDouble = true;
        whereIsDailyDouble.text = "Daily double is in slot number: " + (spot + 1);
        Debug.LogError("Daily double is in the " + (spot + 1) + " slot");
    }    
    public void Buzz()
    {
        if (timerRunning) // you still have time to buzz
        {
            Debug.LogError("Player Buzzed");

            // change tint
            //sidePanalController.TintAllSlotsButOne(3);
            localPlayer.PlayerBuzzedIn();
            localPlayer.PlayerSetHasAnswered(true);
            StartTimerCoroutine(true);
            localPlayer.PlayerStartTimerForHost(true);
            CanSumbit();
        }
        else
        // you didn't buzz in time
        {
            CantBuzz();
            // once you set the buttons to active there is no way to go farward, so open the next screen after a few seconds
            localPlayer.PlayerDidntBuzz();
        }

        // when a player has buzzed, Light the player slot of the one who has buzzed
    }

    public void CantBuzz()
    {
        submitButton.gameObject.SetActive(false);
        buzzButton.SetEnable(false);
        buzzButton.gameObject.SetActive(true);
        answerInput.SetEnable(false);
    }
    public void CantSumbit()
    {
        buzzButton.gameObject.SetActive(false);
        answerInput.SetEnable(false);
        submitButton.gameObject.SetActive(true);
        submitButton.SetEnable(false);
    }
    public void CanBuzz()
    {
        submitButton.gameObject.SetActive(false);
        buzzButton.SetEnable(true);
        buzzButton.gameObject.SetActive(true);
        answerInput.SetEnable(false);
    }
    public void CanSumbit()
    {
        buzzButton.gameObject.SetActive(false);
        answerInput.SetEnable(true);
        submitButton.gameObject.SetActive(true);
        submitButton.SetEnable(true);
    }

    public void Submit()
    {

        if (timerRunning)
        // meaning the player have submitted the answer in time 
        {
            Debug.LogError("Player Submitted");
            string answer = inputFieldAnswer.text;
            if(string.IsNullOrEmpty(answer))
            {
                localPlayer.PlayerHostDecided(false);
                localPlayer.PlayerOpenSlotsPanalToAll();
            }
            else
            {

            localPlayer.PlayerSumbited(answer);
            }
            StopTimerCoroutine();
            localPlayer.PlayerStoptTimerForHost();
            timerRunning = false;
        }
        else
        // have not submitted in in time
        {
            // same as submited wrong
            localPlayer.PlayerHostDecided(false);
            // once you set the buttons to active there is no way to go farward, so open the next screen after a few seconds
        }
        CantSumbit();
        // change tint
        //sidePanalController.UntintAll();
    }
    public void ChosingAmountRight()
    {
        // for the daily and final jeopardy panel
        int amount = localPlayer.playerAmount;
        if (currentQuestionAmount + jumpsAmount <= amount)
        {
            amountChoserText.text = "$" + (currentQuestionAmount + jumpsAmount).ToString();
            currentQuestionAmount = (currentQuestionAmount + jumpsAmount);
            localPlayer.PlayerSetHostCurrenctQuestionAmount(currentQuestionAmount);
        }
    }
    public void ChosingAmountLeft()
    {
        // amount 500
        //current 100
        // jumps 100

        // for the daily and final jeopardy panel 
        if (currentQuestionAmount - jumpsAmount >=0)
        {
            amountChoserText.text = "$" + (currentQuestionAmount - jumpsAmount).ToString();
            currentQuestionAmount = (currentQuestionAmount - jumpsAmount);
            localPlayer.PlayerSetHostCurrenctQuestionAmount(currentQuestionAmount);
        }
    }

    public void GiveTurnToMe()
    {
        CanBuzz();
    }
    internal void TakeTurnFromMe()
    {
    }
    internal void DeactivateSlots()
    {
        for (int i = 0; i < allSlots.Length; i++)
        {
            allSlots[i].DeactivateButtons();
        }
    }
    #endregion
    #region HOST METHODS
    public void HostContinueButton()
    {
        if (!isFinalJeopardyNow)
        {
            localPlayer.PlayerOpenSlotsPanalToAll();
        }
        else
            localPlayer.PlayerOpenWinnerPanal();
        //hostContinueButton.gameObject.SetActive(false);

    }
    public void HostCorrectButton()
    {
        hostContinueButton.gameObject.SetActive(true);
        correctButton.SetEnable(false);
        incorrectButton.SetEnable(false);
        localPlayer.PlayerHostDecided(true);
    }
    public void HostWrongButton()
    {
        hostContinueButton.gameObject.SetActive(true);
        correctButton.SetEnable(false);
        incorrectButton.SetEnable(false);
        localPlayer.PlayerHostDecided(false);
    }
    public void HostPauseButton()
    {
        localPlayer.PlayerPauseGameForAll();

    }
    public void HostUnauseButton()
    {
        localPlayer.PlayerUnPauseGameForAll();

    }
    #endregion
    public void StopTimerCoroutine()
    {
        Debug.LogError("Stopping timer");
        timerRunning = false;
        StopCoroutine(submitTimer);
        StopCoroutine(buzzTimer);
    }
    public void StartTimerCoroutine(bool answer)
    {

        StopTimerCoroutine();
        timerRunning = true;
        if(answer)
        submitTimer = StartCoroutine(ActivateTimer(timeToAnswer, true));
        else
            buzzTimer = StartCoroutine(ActivateTimer(timeToBuzz, false));
    }
    internal IEnumerator ActivateTimer(float duration, bool shouldSubmitWhenTimerEnds)
    {
        Debug.LogError("Starting timer");
        timerRunning = true;
        float t = duration;
        bool isHost = localPlayer.isHost;
        while (t > 1)
        {
            while (isPaused)
            {
                yield return null;
            }

            t -= Time.deltaTime;
            if (Mathf.FloorToInt(t % 60) == 0)
            {
                if (isHost)
                    hostTimerText.text = Mathf.FloorToInt(t / 60) + ":00";
                else
                    clientQuestionTimerText.text = Mathf.FloorToInt(t / 60) + ":00";

            }
            else
            {
                if (isHost)
                    hostTimerText.text = Mathf.FloorToInt(t / 60) + ":" + Mathf.FloorToInt(t % 60);
                else
                clientQuestionTimerText.text = Mathf.FloorToInt(t / 60) + ":" + Mathf.FloorToInt(t % 60);

            }
            yield return null;
        }
        
        timerRunning = false;
        if (shouldSubmitWhenTimerEnds)
            Submit();
        else
            Buzz();
    }
    internal IEnumerator GoBackToBoard(float duration)
    {
        float t = duration;
        bool isHost = localPlayer.isHost;
        while (t > 1)
        {
            t -= Time.deltaTime;
            if (Mathf.FloorToInt(t % 60) == 0)
            {
                if(isHost)
                    hostTimerText.text = Mathf.FloorToInt(t / 60) + ":00";
                else
                    answerTimerText.text = Mathf.FloorToInt(t / 60) + ":00";
            }
            else
            {
                if (isHost)
                    hostTimerText.text = Mathf.FloorToInt(t / 60) + ":" + Mathf.FloorToInt(t % 60);
                else
                    answerTimerText.text = Mathf.FloorToInt(t / 60) + ":" + Mathf.FloorToInt(t % 60);
            }

            yield return null;
        }
        if (isFinalJeopardyNow == false)
        {
            OpenSlotsPanel();
            Debug.Log("slots panel opened");
        }

    }
    public IEnumerator OpenAnswerInDelay()
    {
        float t = openAnswerDelay;
        while (t > 1)
        {
            t -= Time.deltaTime;
            if (Mathf.FloorToInt(t % 60) == 0)
                clientQuestionTimerText.text = Mathf.FloorToInt(t / 60) + ":00";
            else
                clientQuestionTimerText.text = Mathf.FloorToInt(t / 60) + ":" + Mathf.FloorToInt(t % 60);

            yield return null;
        }
        localPlayer.PlayerOpenAnswerPanalToAll();
        //OpenClientAnswerPanel();
    }
}
