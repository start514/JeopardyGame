
using UnityEngine;
using UnityEngine.UI;

public class Toggle : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject handle;
    public GameObject onTxt;
    public GameObject offTxt;
    public bool on = true;
    void Start()
    {
        
    }

    public void ToggleSwitch ()
    {
        if (on == true)
        // you want to turn the switch off
        {
            offTxt.SetActive(true);
            onTxt.SetActive(false);
            handle.transform.localPosition = new Vector3(-41, 0, 0 ) ;
            on = false;
        }
        else
        // you want to turn the switch on
        {
            offTxt.SetActive(false);
            onTxt.SetActive(true);
            handle.transform.localPosition = new Vector3(41, 0, 0 ) ;
            on = true;
        }
    }
}
