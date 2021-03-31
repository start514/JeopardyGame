using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class AmountSlot : MonoBehaviour
{
    [SerializeField]
    internal int amout; // the amount the question is worth in dollars
    public string question, answer; // the text of the question, imported from the json file 
    private bool isActive;
    private GameObject inactiveBackground;
    private Button btn;
    private TMP_Text amountText;
    internal bool dailyDouble = false;
    [SerializeField]
    internal string catagoryName;
    public int slotIndex;


    // Start is called before the first frame update
    void Start()
    {
        amountText = gameObject.GetComponentInChildren<TMP_Text>();
        amountText.text = "$" + amout.ToString();
        btn = gameObject.GetComponent<Button>();
        btn.onClick.AddListener(AmountWasPressed);
        btn.enabled = true;


    }

    // Update is called once per frame
    public void AmountWasPressed()
    {
        UIGameController.instance.QuestionClicked(this);
    }
    public void DoubleJeopardySlots()
    {
        amout = amout * 2;
        amountText.text = "$" + amout.ToString();
        btn.onClick.AddListener(AmountWasPressed);
        btn.enabled = true;
    }

    public void DeactivateButtons()
    {
        btn.enabled = false;

    }
    public void ActivateButtons()
    {
        btn.enabled = true;

    }
}
