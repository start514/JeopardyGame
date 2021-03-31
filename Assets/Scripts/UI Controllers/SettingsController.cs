using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SettingsController : MonoBehaviour
{
    public void Start()
    {
        DontDestroyOnLoad(this.gameObject);
    }
    public void ControlSound(Toggle toggle)
    {
        bool on = toggle.on;
        if (on)
        // turn off
        {

        }
        else
        // turn on
        {

        }
    }
    public void ControlMusic(Toggle toggle)
    {
        bool on = toggle.on;
        if (on)
        // turn off
        {

        }
        else
        // turn on
        {

        }
    }


}
