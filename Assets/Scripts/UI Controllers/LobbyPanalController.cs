using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class LobbyPanalController : NetworkBehaviour
{
        void Start()
    {
        // clearing all already existing containers
        // showing the update
        if (isClient && isServer == false && Player.localPlayer != null) {
            Player.localPlayer.ClearGameContainer();
            Player.localPlayer.CmdUpdateGameRoomList();
        }
    }
}
