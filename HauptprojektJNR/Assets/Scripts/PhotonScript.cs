using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class PhotonScript : MonoBehaviourPunCallbacks
{
    public Transform spawnPoint;
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
        //PhotonNetwork.Instantiate("Enemy", new Vector3(x, -1.57f, 0), Quaternion.identity);
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
