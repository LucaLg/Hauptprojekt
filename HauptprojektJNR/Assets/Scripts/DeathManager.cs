using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathManager :MonoBehaviourPunCallbacks
{
    // Start is called before the first frame update
    public GameObject[] players;
    public PlayerController player1;
    public PlayerController player2;
    private Vector3 latestCheckpoint;
    public GameObject[] enemies;
    private PhotonView photonView;
    public Camera player1Cam;
    public Camera player2Cam;
    public Camera deathCam;
    void Start()
    {
        enemies = GameObject.FindGameObjectsWithTag("Enemy");
        photonView = GetComponent<PhotonView>();
        
    }


    void Update()
    {
        updatePlayers();
        // Wenn im Spiel nur ein Spieler existiert und dieser Stirbt wird direkt die ReloadSceneFromLastCheckPoint Methode ausgefuehrt
        if ( players.Length == 1 && player1.dead)
        {
            photonView.RPC("RespawnLastCheckPoint",RpcTarget.AllBuffered);
        }
        if(players.Length >1) {
            //Beide Spieler Tod
            if(player1.dead && player2.dead)
            {
                //Weitesten Checkpoint holen
                if(player1.lastCheckpoint.x >= player2.lastCheckpoint.x)
                {
                    latestCheckpoint = player1.lastCheckpoint;
                }
                else
                {
                    latestCheckpoint = player2.lastCheckpoint;
                }
                photonView.RPC("RespawnLastCheckPoint", RpcTarget.AllBuffered);
            }
            //Ein von beiden Spielern tod
            if(player1.dead || player2.dead)
            {
                if (player1.dead) {
                    /*Vector3 offset = new Vector3(0, 0, 3);
                    player1Cam.transform.position = players[1].transform.position + offset;*/
                    SpawnPlayerAtNextCheckPoint(player1,player2);
                }
                if (player2.dead)
                {
                    /*Vector3 offset = new Vector3(0, 0, 3);
                    player2Cam.transform.position = players[0].transform.position + offset;*/
                    SpawnPlayerAtNextCheckPoint(player2,player1);
                }
               
            }
        }

    }
    void updatePlayers()
    {
        players = GameObject.FindGameObjectsWithTag("Player");

        if (players.Length > 1)
        {
            player1 = players[0].GetComponent<PlayerController>();
            //player1Cam = players[0].GetComponentInChildren<Camera>();
  
            player2 = players[1].GetComponent<PlayerController>();
            //player2Cam = players[1].GetComponentInChildren<Camera>();
            player1.setOtherPlayer(player2.photonView);
            player2.setOtherPlayer(player1.photonView);
        }
        else if (players.Length == 1)
        {
            player1 = players[0].GetComponent<PlayerController>();
            player1Cam = players[0].GetComponentInChildren<Camera>();
        }
    }
    /*
     * Wenn beide Spieler Tod sind wird die Szene neu geladen um die Gegner zu Respawnen
     * und die Spieler an den letzten CheckPoint platziert
     */

    [PunRPC]
    void RespawnLastCheckPoint()
    {
        
        //Respawne alle Enemies
        foreach(GameObject enemie in enemies)
        {
            
            enemie.GetComponent<EnemyController>().Respawn();
            Debug.Log("Respawn Enemy");
        }
        //Respawne Spieler am Letzten CheckPoint
        
        foreach (GameObject player in players)
        {
            latestCheckpoint = player.GetComponent<PlayerController>().lastCheckpoint;
            player.GetComponent<PlayerController>().Respawn();
            player.transform.position= player.GetComponent<PlayerController>().lastCheckpoint;
        }
        
    }
    /*
     * Wenn ein Spieler stirbt und der andere den naechsten Checkpoint erreicht wird der Tode spieler Respawnt
     */
    void SpawnPlayerAtNextCheckPoint(PlayerController playerDead,PlayerController playerAlive)
    {
        if(playerAlive.lastCheckpoint.x > playerDead.lastCheckpoint.x)
        {
            playerDead.GetComponent<PlayerController>().lastCheckpoint = playerAlive.lastCheckpoint;
            playerDead.GetComponent<PlayerController>().Respawn();
            playerDead.transform.position = playerDead.GetComponent<PlayerController>().lastCheckpoint;
        }
    }
    /*public override void OnCreatedRoom()
    {
        updatePlayers();
        Debug.LogError("Raum geoffnet");
    }
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        updatePlayers();
        Debug.LogError("PlayerJoined");
    }
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        updatePlayers();
    }*/

}
