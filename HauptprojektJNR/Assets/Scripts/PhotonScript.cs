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
        
    }
    public override void OnJoinedRoom()
    {
        //float x = Random.Range(-3, 5);
        PhotonNetwork.Instantiate("Player", spawnPoint.position, Quaternion.identity);
        Debug.LogAssertion("RaumJoined");
        if (StaticData.load) {
            LobbyView.RPC("UpdatePlayers", RpcTarget.AllBuffered);
            player1Data = loadFromFile();
            if(player1Data != null)
            {
                //player2Data = player1Data.otherPlayerData;
                //Player1.photonView.RPC("LoadPlayerStateFromGameData", RpcTarget.AllBuffered, player1Data);
                Player1.LoadPlayerStateFromGameData(player1Data);

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
        Debug.LogAssertion("On Player enter");
        LobbyView.RPC("UpdatePlayers", RpcTarget.AllBuffered);
        if(Player2 != null && player2Data != null)
        {
            Player2.photonView.RPC("LoadPlayerStateFromGameData", RpcTarget.AllBuffered, player2Data);
            StaticData.load = false;
        }
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
    private GameData loadFromFile()
    {
        string destination = Application.persistentDataPath + "/save.json";
        //Debug.Log("Try to load from " + destination);
        //FileStream file;

        //if (File.Exists(destination)) file = File.OpenRead(destination);
        //else
        //{
        //    Debug.LogError("File not found");
        //    return null;
        //}

        //BinaryFormatter bf = new BinaryFormatter();
        //GameData data = (GameData) bf.Deserialize(file);
        //file.Close();
        GameData data = JsonUtility.FromJson<GameData>(File.ReadAllText(destination));
        Debug.Log(data.stamina);

        return data;
        Debug.Log("Loaded (?)");
    }
}
