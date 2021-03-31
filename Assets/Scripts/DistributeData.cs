using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DistributeData : MonoBehaviour
{
    public static DistributeData instance;
    public JsonToC jsonToCScript;
    private Root data;
    private List<int> singleValue = new List<int>();
    private List<int> doubleValue = new List<int>();

    // what is the problem: the method uses random numbers so each call the values will be different
    // options: call on host and copy the data to all players
    internal void Awake()
    {
        // called once at start of server
        // handaling data 
        data = jsonToCScript.root;
        // initilizing the values list, separating the double questions from the single question by checking which level lists are empty, and which are not
        for (int i = 0; i < data.Categories.Count; i++)
        {
            if (data.Categories[i].Levels.One.Count != 0) // single question
                singleValue.Add(i);
            else // double questions
                doubleValue.Add(i);
        }
        Debug.Log("there are " + singleValue.Count + " Single quesrions ");
        Debug.Log("there are " + doubleValue.Count + " double  quesrions ");
        Debug.Log("Reading values:");
        //ReadValues();
    }
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
        Player.localPlayer.DistributeDataForFirstBoard();
    }
    public void RandomlyDistributeCatFromData(bool isDouble, CatagorySlot[] allCatagories)
    {
        int rnd, rndI;
        for (int i = 0; i < allCatagories.Length; i++)// runs 6 times 
        {
            // meaning we are on the first board)
            if (isDouble == false)
            {
                Debug.Log("Distrebuting for single");
                rndI = UnityEngine.Random.Range(0, singleValue.Count);
                rnd = singleValue[rndI];
                singleValue.Remove(rnd);
            }
            else
            // meaning we are on  the second board
            {
                Debug.Log("Distrebuting for double");
                rndI = UnityEngine.Random.Range(0, doubleValue.Count);
                rnd = doubleValue[rndI];
                doubleValue.Remove(rnd);
            }
            // choosing a random catagory from the json
            Category currentCatInJson = data.Categories[rnd];
            // applying that catagory
            allCatagories[i].catagoryText.text = currentCatInJson.Name;
            allCatagories[i].name = currentCatInJson.Name;

            // setting that the amounts in this catagory have the correct catagory name
            for (int k = 0; k < 5; k++)
            {
                allCatagories[i].amounts[k].catagoryName = currentCatInJson.Name;

            }
            // now set the questions in each catagory
            DistributeQuestions(allCatagories[i], currentCatInJson, isDouble);

        }
    }
    public void DistributeQuestions(CatagorySlot currectCat, Category currentJson, bool isDouble)
    {
        int rnd;
        // now set the questions in each catagory
        if (isDouble == false)
        // meaning we are on the first board
        {
            // choose a random question from current value in current catagory 
            rnd = UnityEngine.Random.Range(0, currentJson.Levels.One.Count);
            currectCat.amounts[0].question = currentJson.Levels.One[rnd].Question;
            currectCat.amounts[0].answer = currentJson.Levels.One[rnd].Answer;

            rnd = UnityEngine.Random.Range(0, currentJson.Levels.Two.Count);
            currectCat.amounts[1].question = currentJson.Levels.Two[rnd].Question;
            currectCat.amounts[1].answer = currentJson.Levels.Two[rnd].Answer;

            rnd = UnityEngine.Random.Range(0, currentJson.Levels.Three.Count);
            currectCat.amounts[2].question = currentJson.Levels.Three[rnd].Question;
            currectCat.amounts[2].answer = currentJson.Levels.Three[rnd].Answer;

            rnd = UnityEngine.Random.Range(0, currentJson.Levels.Four.Count);
            currectCat.amounts[3].question = currentJson.Levels.Four[rnd].Question;
            currectCat.amounts[3].answer = currentJson.Levels.Four[rnd].Answer;

            rnd = UnityEngine.Random.Range(0, currentJson.Levels.Five.Count);
            currectCat.amounts[4].question = currentJson.Levels.Five[rnd].Question;
            currectCat.amounts[4].answer = currentJson.Levels.Five[rnd].Answer;
        }
        else // we are on the second board
        {
            // choose a random question from current value in current catagory 
            rnd = UnityEngine.Random.Range(0, currentJson.Levels.Six.Count);
            currectCat.amounts[0].question = currentJson.Levels.Six[rnd].Question;
            currectCat.amounts[0].answer = currentJson.Levels.Six[rnd].Answer;

            rnd = UnityEngine.Random.Range(0, currentJson.Levels.Seven.Count);
            currectCat.amounts[1].question = currentJson.Levels.Seven[rnd].Question;
            currectCat.amounts[1].answer = currentJson.Levels.Seven[rnd].Answer;

            rnd = UnityEngine.Random.Range(0, currentJson.Levels.Eight.Count);
            currectCat.amounts[2].question = currentJson.Levels.Eight[rnd].Question;
            currectCat.amounts[2].answer = currentJson.Levels.Eight[rnd].Answer;

            rnd = UnityEngine.Random.Range(0, currentJson.Levels.Nine.Count);
            currectCat.amounts[3].question = currentJson.Levels.Nine[rnd].Question;
            currectCat.amounts[3].answer = currentJson.Levels.Nine[rnd].Answer;

            rnd = UnityEngine.Random.Range(0, currentJson.Levels.Ten.Count);
            currectCat.amounts[4].question = currentJson.Levels.Ten[rnd].Question;
            currectCat.amounts[4].answer = currentJson.Levels.Ten[rnd].Answer;
        }
    }
    public void ReadValues()
    {
        foreach (int num in singleValue)
        {
            Debug.Log("single" + num);
        }
        foreach (int num in doubleValue)
        {
            Debug.Log("double" + num);
        }
    }
}
