using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Mirror;
using System;

public class UILobbyController : MonoBehaviour
{
    [Header("Lobby")]
    public GameObject lobbyPanal;
    public TMP_Text availableGamesNumTxt;
    public GameObject availableGameContent;
    public GameObject gameConteinerPrefab;
    private int numberOfGameContainers = 0;
    public Color greyColor;
    public CustomButton joinGameBtn;
    public Sprite disabledIcon, activeIcon;
    public string pressedContainerId; // we are getting this from the game container on click
    private Vector2 size = new Vector2(920, 49);
    [Header("Join Game")]
    public GameObject joinGamePanal;
    public CustomButton readyGameBtn;
    public GameObject joinParticipentContent, joinParticipentPrefab, joinParticipentPrefabIF;
    public TMP_Text joinParticipentNumTxt;
    //public Color yellow, green, darkBlue, purple, lightBlue;
    public Sprite hostRole, notHostRole;
    [Header("New Game")]
    public GameObject newGamePanal;
    public InputField gameNameIP;
    public Text gameSizeTxt;
    [Header("Host Game")]
    public TMP_Text hostParticipentNumTxt;
    public GameObject hostGamePanal, hostParticipentContent;
    public CustomButton hostGameBtn;
    public Text dailyDoybleTxt, timeToBuzzTxt, timeToAnswerTxt;
    [Header("Error Panal")]
    public GameObject errorPanal;
    public TMP_Text errorTxt;
    [Header("Prefabs")]
    public GameObject transferDataToGamePrefab;
    public GameObject HostParticipentPrefab;
    public GameObject hostContainerWithInput;
    public GameObject turnMangerPrefab;
    public Sprite[] colors;

    /*// delete this after the demo
    public void SpawnPlayer()
    {
        if (GameObject.Find("Player") == null)
        {
            GameObject player =  Instantiate(playerPrefab);
            player.name = "Player";
        }
    }*/
    private void Start()
    {
       // DontDestroyOnLoad(this);
    }
    private void OnEnable()
    {
    }

    #region BUTTONS CONTROLLERS
    public void UpdatePlayerNameIP(InputField field, bool join, string id)
    {
        Debug.Log("locally UpdatePlayerNameIP()");
        string name = field.text;
        Player.localPlayer.playerName = name;
        // update the name to everyone in the same game id
        //if (join)
            Player.localPlayer.CmdUpdateMyHostContainerName(id, Player.localPlayer.playerID, name);
        //else
            Player.localPlayer.CmdUpdateMyJoinContainerName(id, Player.localPlayer.playerID, name);

    }

    public void UpdatePlayerColor()
    {

    }
    public void NewGameStartGameButton()
    {
        string gameName = gameNameIP.text;
        if (String.IsNullOrEmpty(gameName))
        {
            gameName = "New Game";
        }
        TransferDataToGame.instance.gameName = gameName;
        TransferDataToGame.instance.gameSize = int.Parse(gameSizeTxt.text);
        OpenHostGamePanal(int.Parse(gameSizeTxt.text));
    }

    public void HostGameStartGameButton()
    {
        if (dailyDoybleTxt.text == "Enable")
            TransferDataToGame.instance.dailyDouble = true;
        else
            TransferDataToGame.instance.dailyDouble = false;
        TransferDataToGame.instance.timeToAnswer = int.Parse(timeToAnswerTxt.text);
        TransferDataToGame.instance.timeToBuzz = int.Parse(timeToBuzzTxt.text);
    }
    internal void ActivateJoinGameButton()
    {
        joinGameBtn.SetEnable(true);

    }
    internal void DectivateJoinGameButton()
    {
        joinGameBtn.SetEnable(false);
    }
    public void JoinGameButton()
    {

    }
    #endregion
    #region  OPEN AND CLOSE PANALS

    public void DisplayErrorMsg(string msg)
    {
        errorTxt.text = msg;
        errorPanal.SetActive(true);
    }
    void OpenHostGamePanal(int max)
    {
        hostParticipentNumTxt.text =  "0 / " + max; //(current - 1) - 1 because we don't iclude the host as a participent
        AddHostParticipentConteinerWithInput(Player.localPlayer.matchID, Player.localPlayer.playerID);
        newGamePanal.SetActive(false);
        hostGamePanal.SetActive(true);
    }
    // make sure to call this method only after begin game method has been called 

    internal void OpenJoinPanalWithId(int current, int max)
    {
        joinParticipentNumTxt.text = (current - 1) + " / " + max; //(current - 1) - 1 because we don't iclude the host as a participent
        lobbyPanal.SetActive(true);
        joinGamePanal.SetActive(true);
    }
    #endregion

    #region  CONTAINERS
    internal int CountGameContainers()
    {
        numberOfGameContainers = availableGameContent.GetComponentsInChildren<Button>().Length;
        availableGamesNumTxt.text = numberOfGameContainers.ToString();
        return numberOfGameContainers;
    }
    public void AddGameContainer(string id, string gameName, int currentPlayers, int maxPlayers)
    {
        Debug.LogError("Adding game container for id " + id + "with name " + gameName, this);
        GameObject container = Instantiate(gameConteinerPrefab, availableGameContent.transform);
        GameContainer script = container.GetComponent<GameContainer>();
        script.gameId = id;
        ChangeGamecontainerParticipentNumTxt(id, currentPlayers, maxPlayers);
        script.gameNameTxt.text = gameName;
        // set the color of the container
        if (numberOfGameContainers % 2 == 0)
            container.GetComponent<Image>().color = Color.white;
        else
            container.GetComponent<Image>().color = greyColor;
        container.GetComponent<Button>().enabled = true;
        CountGameContainers();
    }
    public void ChangeGamecontainerParticipentNumTxt(string gameId,int currentPlayers, int maxPlayers)
    {
        GameContainer []children = availableGameContent.gameObject.GetComponentsInChildren<GameContainer>();
        for (int i = 0; i < children.Length; i++)
        {
            if(children[i].gameId == gameId)
            {
                children[i].numOfPlayersTxt.text = (currentPlayers - 1) + " / " + maxPlayers;//(current - 1) - 1 because we don't iclude the host as a participent
            }
        }

    }
    public void DeleteGameContainer(string id)
    {
        GameContainer[] children = availableGameContent.GetComponentsInChildren<GameContainer>();

        for (int i = 0; i < children.Length; i++)
        {
            if (children[i].gameId == id)
            {
                Destroy(children[i].gameObject);
                Debug.Log("Destroyed game container");
            }
        }
        CountGameContainers();
    }
    public void AddHostParticipentContainer(string id, string name, int color, string playerID)
    {
        GameObject container = Instantiate(HostParticipentPrefab, hostParticipentContent.transform);
        ParticipentConteiner script = container.GetComponent<ParticipentConteiner>();
        script.nameTxt.text = name;
        script.playerID = playerID;
        script.matchID = id;
        script.role.sprite = notHostRole;
        // add the color chooser
        script.color.sprite = colors[color];

        // set the color of the container
        SetHostContainerColor();
        updateHostPanelPlayerReady(false, false);
    }
    public void DeleteHostParticipentContainer(string id, string playerId)
    {
        updateHostPanelPlayerReady(false, false);
        if (string.IsNullOrEmpty(playerId))
        {
            Debug.LogError("Join Participent Container not found ", this);
            return;
        }
        bool found = false;
        ParticipentConteiner[] children = hostParticipentContent.GetComponentsInChildren<ParticipentConteiner>();
        for (int i = 1; i < children.Length; i++)
        {
            if (children[i].GetComponent<ParticipentConteiner>() != null)
            {
                if (children[i].GetComponent<ParticipentConteiner>().playerID == playerId)
                {
                    Debug.Log("Delete participent controller with id " + playerId);
                    Destroy(children[i].gameObject);
                    found = true;
                    break;
                }
            }
        }
        if (!found)
            Debug.LogError("id to delete not found, id: " + playerId, this);
    }
    public void AddJoinParticipentConteiner(string id, bool host, string name, int color, string playerID)
    {
        GameObject container = Instantiate(joinParticipentPrefab, joinParticipentContent.transform);
        ParticipentConteiner script = container.GetComponent<ParticipentConteiner>();
        script.nameTxt.text = name;
        script.playerID = playerID;
        script.matchID = id;
        if (host)
        {
            script.role.sprite = hostRole;
            script.color.gameObject.SetActive(false);
        }
        else
        {
            script.role.sprite = notHostRole;
            script.color.gameObject.SetActive(true);
            script.color.sprite = colors[color];
        }
        // add the color chooser
        // set the color of the container
        SetJoinContainerColor();
        
    }

    public void SetJoinContainerColor()
    {
        ParticipentConteiner[] containers = joinParticipentContent.GetComponentsInChildren<ParticipentConteiner>();
        Debug.Log("numberOfJoinContainers = " + containers.Length);
        for (int i = 0; i < containers.Length; i++)
        {
            if (i % 2 == 0)
            {
                Debug.Log("White");
                containers[i].gameObject.GetComponent<Image>().color = Color.white;
            }
            else
            {
                Debug.Log("Grey");
                containers[i].gameObject.GetComponent<Image>().color = greyColor;
            }
        }
    }
    public void SetHostContainerColor()
    {
        ParticipentConteiner[] containers = hostParticipentContent.GetComponentsInChildren<ParticipentConteiner>();
        Debug.Log("numberOfHostContainers = " + containers.Length);
        for (int i = 0; i < containers.Length; i++)
        {
            if (i % 2 == 0)
            {
                Debug.Log("White");
                containers[i].gameObject.GetComponent<Image>().color = Color.white;
            }
            else
            {
                Debug.Log("Grey");
                containers[i].gameObject.GetComponent<Image>().color = greyColor;
            }
        }
    }
    public void AddJoinParticipentConteinerWithInput(string id, int color, string playerId)
    {
        GameObject container = Instantiate(joinParticipentPrefabIF, joinParticipentContent.transform);
        ParticipentConteiner script = container.GetComponent<ParticipentConteiner>();
        script.inputField.onEndEdit.AddListener(delegate { UpdatePlayerNameIP(script.inputField, true, id); });
        script.role.sprite = notHostRole;
        script.playerID = playerId;
        script.matchID = id;
        script.color.sprite = colors[color];

        // add the color chooser
        SetJoinContainerColor();

    }
    
    public void AddHostParticipentConteinerWithInput(string id, string playerId)
    {
        GameObject container = Instantiate(hostContainerWithInput, hostParticipentContent.transform);
        ParticipentConteiner script = container.GetComponent<ParticipentConteiner>();
        script.inputField.onEndEdit.AddListener(delegate { UpdatePlayerNameIP(script.inputField, false, id); });
        script.role.sprite = hostRole;
        script.playerID = playerId;
        script.matchID = id;
        if(Player.localPlayer.isHost) script.color.gameObject.SetActive(false);
        else script.color.sprite = colors[Player.localPlayer.playerColor];
        // add the color chooser
        SetHostContainerColor();
    }
    public void DeleteJoinParticipentContainer(string id, string playerId)
    {
        if (string.IsNullOrEmpty(playerId))
        {
            Debug.LogError("Join Participent Container not found ", this);
            return;
        }

        bool found = false;
        ParticipentConteiner[] children = joinParticipentContent.GetComponentsInChildren<ParticipentConteiner>();
        for (int i = 1; i < children.Length; i++)
        {
            if (children[i].GetComponent<ParticipentConteiner>() != null)
            {
                if (children[i].GetComponent<ParticipentConteiner>().playerID == playerId)
                {
                    Debug.Log("Delete participent controller with id " + playerId);
                    Destroy(children[i].gameObject);
                    found = true;
                    break;
                }
            }
        }
        if (!found)
            Debug.LogError("id to delete not found to delete join participent, id: " + playerId, this);
    }
    public void UpdateJoinContainerName(string playerId, string newName)
    {
        ParticipentConteiner[] children = joinParticipentContent.GetComponentsInChildren<ParticipentConteiner>();
        for (int i = 0; i < children.Length; i++)
        {
            if (children[i].playerID == playerId)
            {
                Debug.Log("changing join name for id" + playerId);
                children[i].nameTxt.text = newName;
            }
        }
    }
    public void UpdateHostContainerName(string playerId, string newName)
    {
        ParticipentConteiner[] children = hostParticipentContent.GetComponentsInChildren<ParticipentConteiner>();
        for (int i = 0; i < children.Length; i++)
        {
            if (children[i].playerID == playerId)
            {
                Debug.Log("changing host name for id" + playerId);
                children[i].nameTxt.text = newName;
            }
        }
    }

    public void ClearJoinContainers()
    {
        foreach (Transform child in joinParticipentContent.transform)
        {
            Destroy(child.gameObject);
        }
    }
    public void ClearHostContainers()
    {
        foreach (Transform child in hostParticipentContent.transform)
        {
            Destroy(child.gameObject);
        }
    }
    #endregion
    #region I AM READY
    public void updateJoinPanelIAmReady() {
    }
    public void updateHostPanelPlayerReady(bool gameFull, bool allReady) {
        Player[] players = GameObject.FindObjectsOfType<Player>();
        int playersCount = 0;
        for(int i=0; i<players.Length; i++) {
            if(players[i].matchID == Player.localPlayer.matchID) {
                playersCount ++;
            }
        } 
        Debug.LogError($"Full: {gameFull} AllReady: {allReady} Total: {playersCount}");
        if(allReady && gameFull && playersCount!=1) {
            hostGameBtn.SetEnable(true);
        } else {
            hostGameBtn.SetEnable(false);
        }
    }
    public void updateJoinPanelPlayerReady(string playerID) {
    }
    #endregion
    void Update() {
        CountGameContainers();
        if(joinGamePanal!=null && joinGamePanal.activeSelf) {
            ParticipentConteiner[] participents = joinGamePanal.GetComponentsInChildren<ParticipentConteiner>();
            for(int i=0; i<participents.Length; i++) {
                if(participents[i].playerID == Player.localPlayer.playerID) {//It's me
                    participents[i].inputBG.enabled = !Player.localPlayer.isReady;
                    participents[i].inputField.enabled = !Player.localPlayer.isReady;
                }
            }
            readyGameBtn.SetEnable(!Player.localPlayer.isReady);
        }
    }
}
