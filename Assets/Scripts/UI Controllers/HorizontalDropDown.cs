using UnityEngine;
using UnityEngine.UI;

public class HorizontalDropDown : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject options;
    //public Button leftBtn, rightButton;
    public Text txt;
    private Text[] allOptions;
    private int currentOption=0;
    void Start()
    {
        allOptions = options.GetComponentsInChildren<Text>(true);
        txt.text = allOptions[currentOption].text;

    }

    // Update is called once per frame
    public void  Left()
    {
        if (currentOption>0)
        {
            currentOption--;
            txt.text = allOptions[currentOption].text;
        }
    }
    public void  Right()
    {
        if (currentOption<allOptions.Length-1)
        {
            currentOption++;
            txt.text = allOptions[currentOption].text;
        }
    }

}
