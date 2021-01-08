using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon;
using Photon.Pun;

public class GameLogic : MonoBehaviour
{
    // Start is called before the first frame update
    public PhotonScript LobbyScript;
    public PlayerController Player1;
    public PlayerController Player2;
    public PhotonView GameLogicPhotonView;
    void Start()
    {
       
    }
    // Update is called once per frame
    void Update()
    {
        
    }

    [PunRPC]
    private void GivePlayersXP(float amount)
    {
        Player1 = LobbyScript.GetPlayer(1);
        Player2 = LobbyScript.GetPlayer(2);
        Debug.LogWarning("Gebe Spieler Xp");
        if (Player1 != null && Player2 != null)
        {
            Player1.photonView.RPC("addXP", RpcTarget.AllBuffered, amount);
            Player2.photonView.RPC("addXP", RpcTarget.AllBuffered, amount);

        }
        else if (Player2 == null && Player1 != null)
        {
            Player1.photonView.RPC("addXP", RpcTarget.AllBuffered, amount);
        }
        else
        {
            Debug.LogWarning("Spieler nicht vorhanden");
        }

    }
}
