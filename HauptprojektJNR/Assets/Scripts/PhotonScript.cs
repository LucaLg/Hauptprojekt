using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

public class PhotonScript : MonoBehaviourPunCallbacks
{
    public Transform spawnPoint;
    public GameObject[] Players;
    public PlayerController Player1;
    public PlayerController Player2;
    public PhotonView LobbyView;
    public GameData player1Data, player2Data;
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
        PhotonNetwork.NetworkingClient.LoadBalancingPeer.DisconnectTimeout = 500000;


    }
    public override void OnJoinedRoom()
    {
        //float x = Random.Range(-3, 5);
        GameObject myPlayer = PhotonNetwork.Instantiate("Player", spawnPoint.position, Quaternion.identity);
        Debug.LogAssertion("RaumJoined");
        if (StaticData.load) {
            Debug.Log("Loading");
            //LobbyView.RPC("UpdatePlayers", RpcTarget.AllBuffered);
            player1Data = loadFromFile("save.json");
            Debug.Log(player1Data.health);
            if(player1Data != null)
            {
                //BinaryFormatter bf = new BinaryFormatter();
                //var ms = new MemoryStream();
                //bf.Serialize(ms, player1Data);
                //byte[] dataAsArray = ms.ToArray();
                //try
                //{
                //    player2Data = loadFromFile("savePlayer2.json");

                //}
                //catch (FileNotFoundException e)
                //{
                //    Debug.LogError(e.Message);
                //}
                myPlayer.GetComponent<PlayerController>().LoadPlayerStateFromGameData(player1Data);
                //Player1.photonView.RPC("LoadPlayerStateFromGameData", RpcTarget.AllBuffered, dataAsArray);


            }
            
        }
        LobbyView.RPC("UpdatePlayers", RpcTarget.AllBuffered);
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
        Debug.Log("On Player enter");
        LobbyView.RPC("UpdatePlayers", RpcTarget.AllBuffered);
        //if(Player2 != null && player2Data != null)
        //{
        //    BinaryFormatter bf = new BinaryFormatter();
        //    var ms = new MemoryStream();
        //    bf.Serialize(ms, player2Data);
        //    byte[] dataAsArray = ms.ToArray();
        //    Debug.Log("Sending other Player load command; health: " + player2Data.health);
        //    Player2.photonView.RPC("LoadPlayerStateFromGameData", RpcTarget.AllBuffered, dataAsArray);
        //    StaticData.load = false;
        //}
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
            Player1.setOtherPlayer(Player2.photonView);
            Player2.setOtherPlayer(Player1.photonView);
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
    private GameData loadFromFile(string dest)
    {
        string destination = Application.persistentDataPath + "\\" + dest;
        GameData data = JsonUtility.FromJson<GameData>(File.ReadAllText(destination));

        return data;
    }
}
