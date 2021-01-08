﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeadzoneScript : MonoBehaviour
{
    // Start is called before the first frame update
    private void OnTriggerEnter2D(Collider2D playerCollider)
    {
        playerCollider.GetComponentInParent<PlayerController>().dead = true;
    }
}
