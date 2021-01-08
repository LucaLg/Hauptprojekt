using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class PhotonScript : MonoBehaviourPunCallbacks
{
    public Transform spawnPoint;
    public GameObject[] Players;
    public PlayerController Player1;
    public PlayerController Player2;
    public PhotonView LobbyView;
    // Start is called before the first frame update
    void Start()
    {
        PhotonNetwork.ConnectUsingSettings();
    }
    public override void OnConnectedToMaster()
    {
        RoomOptions roomOptions = new RoomOptions();
        roomOptions.MaxPlayers = 2;
        PhotonNetwork.JoinOrCreateRoom("Room1", roomOptions, null);
        
    }
    public override void OnJoinedRoom()
    {
        //float x = Random.Range(-3, 5);
        PhotonNetwork.Instantiate("Player", spawnPoint.position, Quaternion.identity);
        Debug.LogAssertion("RaumJoined");
        LobbyView.RPC("UpdatePlayers", RpcTarget.All);
        //PhotonNetwork.Instantiate("Enemy", new Vector3(x, -1.57f, 0), Quaternion.identity);
    }
    // Update is called once per frame
    void Update()
    {
       
    }
    public override void OnCreatedRoom()
    {
        Debug.LogAssertion("RoomCreated");
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)

    {
        Debug.LogAssertion("On Player enter");
        LobbyView.RPC("UpdatePlayers", RpcTarget.All);
    }
    [PunRPC]
    void UpdatePlayers()
    {
        Players = GameObject.FindGameObjectsWithTag("Player");
        Debug.LogError("Players Lange" + Players.Length);
        Debug.LogError("SpielerListe" + PhotonNetwork.PlayerList.Length);
        if (Players.Length == 2)
        {
            Player1 = Players[0].GetComponent<PlayerController>();
            Player2 = Players[1].GetComponent<PlayerController>();
        }
        else if (Players.Length == 1)
        {
            Player1 = Players[0].GetComponent<PlayerController>();
        }
    }
    public PlayerController GetPlayer(int PlayerNumber)
    {
        if (PlayerNumber == 1)
        {
            return Player1;
        }else if( PlayerNumber == 2)
        {
            return Player2;
        }
        return null;
    }
    public GameObject[] GetPlayers()
    {
        return Players;
    }
}
