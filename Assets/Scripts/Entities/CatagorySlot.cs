using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


// this script will be attached to each catagory object
public class CatagorySlot : MonoBehaviour
{
    internal TMP_Text catagoryText;
    internal string catagory;
    [SerializeField]
    internal AmountSlot[] amounts; // all of this specific catagories amounts will be attched here
    // Start is called before the first frame update
    void Awake()
    {
        if (amounts.Length != 5)
        {
            Debug.LogError("Not enough amounts", this);
            return;
        }
        catagoryText = gameObject.GetComponentInChildren<TMP_Text>();

        // choose 12 random catagories from the data arry
        // use 6 for the first board and the other 6 for the second board
        // choose a random quesion for each value in

        if (amounts.Length != 5)
            Debug.LogError("not anough amounts", this);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
