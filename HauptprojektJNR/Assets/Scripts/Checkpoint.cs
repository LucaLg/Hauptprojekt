using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    public BoxCollider2D boxCollider2D;
   
    private void OnTriggerEnter2D(Collider2D player)
    {
        if (player.tag == "Player")
        {
            player.GetComponentInParent<PlayerController>().lastCheckpoint = this.transform.position;
        }
       
    }
}
