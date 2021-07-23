using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using PlayFab;
using PlayFab.ClientModels;

public class AutoConnect : MonoBehaviour
{
    string log = "";
    // Start is called before the first frame update
    void Start()
    {
        #if UNITY_SERVER
        PlayFabMultiplayerAgentAPI.Start(); 
        PlayFabMultiplayerAgentAPI.ReadyForPlayers();
        PlayFabMultiplayerAgentAPI.SendHeartBeatRequest();
        
        #else
        
        // if (string.IsNullOrEmpty(PlayFabSettings.staticSettings.TitleId)){
        //     /*
        //     Please change the titleId below to your own titleId from PlayFab Game Manager.
        //     If you have already set the value in the Editor Extensions, this can be skipped.
        //     */
        //     PlayFabSettings.staticSettings.TitleId = "80717";
        // }
        // var request = new LoginWithCustomIDRequest { CustomId = "GettingStartedGuide", CreateAccount = true};
        // PlayFabClientAPI.LoginWithCustomID(request, OnLoginSuccess, OnLoginFailure);
        gameObject.GetComponent<NetworkManager>().StartClient();
        #endif
    }

    private void OnLoginSuccess(LoginResult result)
    {
        var req = new PlayFab.MultiplayerModels.RequestMultiplayerServerRequest();
        req.BuildId = "8768c1bb-c2ed-44c0-97a4-ed3b161531b1";
        req.SessionId = "448a42b4-4dd2-4467-8ea9-d41c64e9215d";
        req.PreferredRegions = new List<string>() { "EastUs" };
        PlayFabMultiplayerAPI.RequestMultiplayerServer(req, onSuccess, onFailed);
    }

    private void OnLoginFailure(PlayFabError error)
    {
        Debug.LogWarning("Something went wrong with your first API call.  :(");
        Debug.LogError("Here's some debug information:");
        Debug.LogError(error.GenerateErrorReport());
    }

    void onSuccess(PlayFab.MultiplayerModels.RequestMultiplayerServerResponse response) {
        log += "IP: "+response.IPV4Address + "\nPort: " + response.Ports[0].Num;
        gameObject.GetComponent<NetworkManager>().StartClient();
    }

    void onFailed(PlayFab.PlayFabError error) {
        log += error.ToString();
        gameObject.GetComponent<NetworkManager>().StartClient();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnGUI() {
        // var activeTransport = gameObject.GetComponent<Mirror.SimpleWeb.SimpleWebTransport>();
        // activeTransport.port = ushort.Parse(GUILayout.TextField("" + activeTransport.port));
        // GUILayout.Label(log);
    }
}
