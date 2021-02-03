using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathManager :MonoBehaviourPunCallbacks
{
    // Start is called before the first frame update
    public GameObject[] Players;
    public PlayerController Player1;
    public PlayerController Player2;
    private Vector3 latestCheckpoint;
    public GameObject[] enemies;
    public GameObject[] archers;
    private PhotonView photonView;
    public Camera player1Cam;
    public Camera player2Cam;
    public Camera deathCam;
    public GameObject LobbyObject;
    public PhotonScript LobbyScript;
    void Start()
    {
        enemies = GameObject.FindGameObjectsWithTag("Enemy");
        photonView = GetComponent<PhotonView>();
        LobbyObject = GameObject.FindGameObjectWithTag("LobbyManager");
        LobbyScript = LobbyObject.GetComponent<PhotonScript>();
        archers = GameObject.FindGameObjectsWithTag("Archer");
    }


    void Update()
    {
        Players = LobbyScript.GetPlayers();
        if(PhotonNetwork.PlayerList.Length > 1)
        {
            Player1 = LobbyScript.GetPlayer(1);
            Player2 = LobbyScript.GetPlayer(2);
        }
        else
        {
            Player1 = LobbyScript.GetPlayer(1);
        }
       
        // Wenn im Spiel nur ein Spieler existiert und dieser Stirbt wird direkt die ReloadSceneFromLastCheckPoint Methode ausgefuehrt
        if ( Players.Length == 1 && Player1.dead)
        {
            
            //photonView.RPC("RespawnLastCheckPoint",RpcTarget.AllBuffered);
            Invoke("RespawnLastCheckPoint",2f);
        }
        if(Players.Length >1) {
            //Beide Spieler Tod
            if(Player1.dead && Player2.dead)
            {
                //Weitesten Checkpoint holen
                if(Player1.lastCheckpoint.x >= Player2.lastCheckpoint.x)
                {
                    latestCheckpoint = Player1.lastCheckpoint;
                }
                else
                {
                    latestCheckpoint = Player2.lastCheckpoint;
                }
                photonView.RPC("RespawnLastCheckPoint", RpcTarget.AllBuffered);
            }
            //Ein von beiden Spielern tod
            if(Player1.dead || Player2.dead)
            {
                if (Player1.dead) {
                    Vector3 offset = new Vector3(0, 2, Player1.playerCam.transform.position.z);
                    Player1.playerCam.SetActive(false);
                   
                    // player1Cam.transform.position = players[1].transform.position + offset;
                    deathCam.transform.position = Players[1].transform.position + offset;
                    SpawnPlayerAtNextCheckPoint(Player1,Player2);
                }
                if (Player2.dead)
                {
                    Vector3 offset = new Vector3(0, 2, Player2.playerCam.transform.position.z);
                    Player2.playerCam.SetActive(false);
                    
                    // player1Cam.transform.position = players[1].transform.position + offset;
                    deathCam.transform.position = Players[0].transform.position + offset;
                    /*Vector3 offset = new Vector3(0, 0, 3);
                    player2Cam.transform.position = players[0].transform.position + offset;*/
                    SpawnPlayerAtNextCheckPoint(Player2,Player1);
                }
               
            }
        }

    }
  
    /*
     * Wenn beide Spieler Tod sind wird die Szene neu geladen um die Gegner zu Respawnen
     * und die Spieler an den letzten CheckPoint platziert
     */

    [PunRPC]
    void RespawnLastCheckPoint()
    {
        foreach(GameObject archer in archers)
        {
            archer.GetComponent<ArcherController>().Respawn();
        }
        //Respawne alle Enemies
        foreach(GameObject enemie in enemies)
        {
            
            enemie.GetComponent<EnemyController>().Respawn();
            Debug.Log("Respawn Enemy");
        }
        //Respawne Spieler am Letzten CheckPoint
        
        foreach (GameObject player in Players)
        {
            latestCheckpoint = player.GetComponent<PlayerController>().lastCheckpoint;
            player.GetComponent<PlayerController>().Respawn();
            player.transform.position= player.GetComponent<PlayerController>().lastCheckpoint;
        }
        
    }
    /*
     * Wenn ein Spieler stirbt und der andere den naechsten Checkpoint erreicht wird der Tode spieler Respawnt
     */
    void SpawnPlayerAtNextCheckPoint(PlayerController playerDead, PlayerController playerAlive)
    {
        if (playerAlive.lastCheckpoint.x > playerDead.lastCheckpoint.x)
        {
            playerDead.GetComponent<PlayerController>().lastCheckpoint = playerAlive.lastCheckpoint;
            playerDead.GetComponent<PlayerController>().Respawn();
            playerDead.transform.position = playerDead.GetComponent<PlayerController>().lastCheckpoint;
        }
    }
    
}
