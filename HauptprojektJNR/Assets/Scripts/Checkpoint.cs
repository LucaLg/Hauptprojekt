﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    public BoxCollider2D boxCollider2D;
   
    private void OnTriggerEnter2D(Collider2D player)
    {
      player.GetComponentInParent<PlayerController>().SetLastCheckPoint(transform);
    }
}
