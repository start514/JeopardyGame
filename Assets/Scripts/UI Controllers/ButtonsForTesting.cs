using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonsForTesting : MonoBehaviour
{

 

    public void RemoveListenersDouble()
    {
        gameObject.GetComponent<Button>().enabled = false ;
        Debug.Log("Remove listeners");
    }
}
