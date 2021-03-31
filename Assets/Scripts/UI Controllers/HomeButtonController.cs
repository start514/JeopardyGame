using UnityEngine;
using UnityEngine.UI;

public class HomeButtonController : MonoBehaviour
{
    void Start()
    {
        gameObject.GetComponent<Button>().onClick.AddListener(delegate { GameObject.Find("Dont Destroy On Land Canvas").GetComponent<DontDestroyOnLoad>().OpenPausePanal(); });
        
    }

    
}
