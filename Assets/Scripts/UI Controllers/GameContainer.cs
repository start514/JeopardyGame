using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using Mirror;

public class GameContainer : NetworkBehaviour, ISelectHandler, IDeselectHandler
{
    public string gameId;
    public TMP_Text gameNameTxt;
    public Text numOfPlayersTxt, emptyTxt;
    private Color color;
    private UILobbyController uiLobby;
    private Camera camera;
    private GameObject joinBtn;
    private void Start()
    {
        // When we are spawned on the client,
        // find the parent object using its ID,
        // and set it to be our transform's parent.
        /*// getting the game name txt so we can change it to white when pressed
        Transform[] temp = gameObject.GetComponentsInChildren<Transform>();
        gameNameTxt = temp[1].gameObject.GetComponent<TMP_Text>();
        numOfPlayersTxt = temp[2].gameObject.GetComponent<TMP_Text>();
        */
        color = gameNameTxt.color;
        uiLobby = GameObject.Find("UI Lobby Controller").GetComponent<UILobbyController>();
        Canvas canvas = GameObject.Find("Canvas").GetComponent<Canvas>();
        camera = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : camera;
    } 
    private void TransferId( )
    {
        //when you press a container it passes it's id to the uilobbycontroller so it will know whice game to open
        uiLobby.pressedContainerId = this.gameId;
    }

    void ISelectHandler.OnSelect(BaseEventData eventData)
    {
        gameNameTxt.color = Color.white;
        numOfPlayersTxt.color = Color.white;
        uiLobby.ActivateJoinGameButton();
        TransferId();
    }
    public void OnDeselect(BaseEventData eventData)
    {
        // if the click is not on the button
        if (joinBtn == null)
            joinBtn = GameObject.Find("Join game Button");
        RectTransform rectTransform = joinBtn.GetComponent<RectTransform>();
        if (!RectTransformUtility.RectangleContainsScreenPoint(rectTransform, Input.mousePosition, camera))
        {
            uiLobby.DectivateJoinGameButton();
            uiLobby.pressedContainerId = null;
            gameNameTxt.color = color;//new Color (89/255f,88/255f,89/255f,1/255f);
            numOfPlayersTxt.color = color;
        }

    }
}
