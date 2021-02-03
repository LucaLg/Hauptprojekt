using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using Photon.Pun;

public class BossGate : MonoBehaviour
{
    public GameObject bossgate;
    public PhotonView photonViewBossGate;
    
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.tag == "Player")
        {
            bossgate.active = true;
        }
    }
    
}
