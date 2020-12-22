using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathManager : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject[] players;
    public PlayerController player1;
    public PlayerController player2;
    
    public GameObject[] enemies;
    private PhotonView photonView;
    void Start()
    {
        enemies = GameObject.FindGameObjectsWithTag("Enemy");
        photonView = GetComponent<PhotonView>();
        
    }


    void Update()
    {
        if (PhotonNetwork.PlayerList.Length >= 1)
        {
            updatePlayers();
        }
        // Wenn im Spiel nur ein Spieler existiert und dieser Stirbt wird direkt die ReloadSceneFromLastCheckPoint Methode ausgefuehrt
        if( players.Length == 1 && player1.dead)
        {
            photonView.RPC("RespawnLastCheckPoint",RpcTarget.AllBuffered);
        }
        if(players.Length >1) {
            if(player1.dead && player2.dead)
            {
                photonView.RPC("RespawnLastCheckPoint", RpcTarget.AllBuffered);
            }
            if(player1.dead || player2.dead)
            {

            }
        }

    }
    void updatePlayers()
    {
        players = GameObject.FindGameObjectsWithTag("Player");

        if (players.Length > 1)
        {
            player1 = players[0].GetComponent<PlayerController>();
            player2 = players[1].GetComponent<PlayerController>();
        }
        else if(players.Length ==1 )
        {
            player1 = players[0].GetComponent<PlayerController>();
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
            player.SetActive(true);
            player.GetComponent<PlayerController>().dead = false;
            player.transform.position= player.GetComponent<PlayerController>().lastCheckpoint;


        }
        
       // PhotonNetwork.Instantiate("Player",Lastcheckpoint.position , Quaternion.identity);
    }
    /*
     * Wenn ein Spieler stirbt und der andere den naechsten Checkpoint erreicht wird der Tode spieler Respawnt
     */
    void SpawnPlayerAtNextCheckPoint()
    {

    }
}
