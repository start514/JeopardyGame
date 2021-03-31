using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DontDestroyOnLoad : MonoBehaviour
{
    public GameObject pausePanal;
    public static DontDestroyOnLoad instance;

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
    public void Start()
    {
        DontDestroyOnLoad(this.gameObject);
    }
    public void OpenPausePanal()
    {
        Debug.Log("Open pause panal");
        pausePanal.SetActive(true);
    }

}
