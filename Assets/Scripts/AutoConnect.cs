using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using PlayFab;

public class AutoConnect : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        #if UNITY_SERVER
        // PlayFabMultiplayerAgentAPI.Start(); 
        // PlayFabMultiplayerAgentAPI.ReadyForPlayers();
        #else
        gameObject.GetComponent<NetworkManager>().StartClient();
        #endif
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
