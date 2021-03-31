using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public class JsonToC : MonoBehaviour
{
    string json;
    public TextAsset catagoryJson;
    public TextAsset finalJson;
    internal Root root;
    internal FinalRoot finalRoot;
    private void Awake()
    {
        TransferToC(catagoryJson.ToString());
        TransferToCFinal(finalJson.ToString());
    }
    public void TransferToC(string josnString)
    {
        root = JsonUtility.FromJson<Root>(josnString);
    }
    public void TransferToCFinal(string josnString)
    {
        finalRoot = JsonUtility.FromJson<FinalRoot>(josnString);
    } 
}
[System.Serializable]
public class One // amount 200
{
    public string Question;
    public string Answer;
}
[System.Serializable]

public class Two
{
    public string Question;
    public string Answer;
}
[System.Serializable]

public class Three
{
    public string Question;
    public string Answer;
}
[System.Serializable]

public class Four
{
    public string Question;
    public string Answer;
}
[System.Serializable]

public class Five
{
    public string Question;
    public string Answer;
}
[System.Serializable]

public class Six
{
    public string Question;
    public string Answer;
}
[System.Serializable]

public class Seven
{
    public string Question;
    public string Answer;
}
[System.Serializable]

public class Eight
{
    public string Question;
    public string Answer;
}
[System.Serializable]

public class Nine
{
    public string Question;
    public string Answer;
}
[System.Serializable]

public class Ten
{
    public string Question;
    public string Answer;
}
[System.Serializable]

public class Levels
{
    public List<One> One;
    public List<Two> Two;
    public List<Three> Three;
    public List<Four> Four;
    public List<Five> Five;
    public List<Six> Six;
    public List<Seven> Seven;
    public List<Eight> Eight;
    public List<Nine> Nine;
    public List<Ten> Ten;
}
[System.Serializable]

public class Category
{
    public string Name;
    public Levels Levels;
}
[System.Serializable]

public class Root
{
    public List<Category> Categories;
}






[System.Serializable]

public class Question
{
    public string question;
    public string Answer;
}
[System.Serializable]
public class FinalRoot
{
    public List<Question> Questions;
}





