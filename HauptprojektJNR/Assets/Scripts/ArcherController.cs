﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class ArcherController : MonoBehaviour
{
    //Components
    [SerializeField] LayerMask playerLayers;
    private Rigidbody2D enemeyRb;
    private PhotonView photonView;
    private Animator archerAnimator;
    private GameObject Photon;
    //Properties
    public float health;
    public float maxHealth;
    public float moveSpeed;
    public float fireRate;
    public bool hit= false;
    public float damage;
    public float givenXp;
    private Vector3 spawnPoint;
    private Transform firePoint;
    private bool isDead = false;
    private PlayerController playerTargetController;
    private Vector3 playerTargetPosition;
    private bool targetFound = false;
    private float distanceToHold;
    void Start()
    {
        Photon = GameObject.Find("Photon");
        health = maxHealth;
        archerAnimator = GetComponent<Animator>();
        firePoint = GetComponentInChildren<Transform>();
    }

    // Update is called once per frame
    void Update()
    {
        if (hit)
        {
            archerAnimator.SetTrigger("hit");
            hit = false;
        }
        if (!photonView.IsMine)
        {
            return;
        }
        photonView.RPC("Die", RpcTarget.AllBuffered);
        if (isDead)
        {

            if (photonView.IsMine)
            {
                archerAnimator.SetTrigger("Dead");

                Invoke("Destroy", 0.8f);
            }
        }
        findTarget();
        if (targetFound) { 
            Move();
            Attack();
        }
        else
        {
            archerAnimator.SetInteger("AnimState", 0);
        }
    }
    private void Attack() {
        //Instantiate Arrow
        //Damage in ArrowScript
        PhotonNetwork.Instantiate("Arrow", firePoint.position, Quaternion.identity);
        archerAnimator.SetTrigger("Attack");
    }
    void Move()
    {
        //Laufe weg vom Gegner
        archerAnimator.SetInteger("AnimState",1);
        if (Vector3.Distance(playerTargetPosition, transform.position) < distanceToHold)
            {
                Vector3.MoveTowards(transform.position, playerTargetPosition, Time.deltaTime * moveSpeed * -1f);
            }
       
    }
    void findTarget()
    {
        
        PlayerController playerTarget = null;
        Vector3 target = transform.position;
        RaycastHit2D targetRight = Physics2D.Raycast(transform.position, Vector3.right, 7f, playerLayers);
        RaycastHit2D targetLeft = Physics2D.Raycast(transform.position, Vector3.left, 7f, playerLayers);
        //Finde Links Target
        if(targetLeft.collider != null)
        {
            playerTarget = targetLeft.collider.GetComponentInParent<PlayerController>();
            if (!playerTarget.dead)
            {
                target = targetLeft.transform.position;
                photonView.RPC("FlipTrue", RpcTarget.AllBuffered);
                targetFound = true;
            }
        }
        //Finde Rechts Target
        if (targetRight.collider != null)
        {
            playerTarget = targetRight.collider.GetComponentInParent<PlayerController>();
            if (!playerTarget.dead) { 
            target = targetRight.transform.position;
            photonView.RPC("FlipFalse", RpcTarget.AllBuffered);
            targetFound = true;
            }
        }
        //Links und Rechts Targets -> naherer wird Target
        if (targetLeft.collider != null && targetRight.collider != null)
        {
            PlayerController playerLeft = targetLeft.collider.GetComponentInParent<PlayerController>();
            PlayerController playerRight = playerTarget = targetRight.collider.GetComponentInParent<PlayerController>();
            if (targetRight.distance >= targetLeft.distance && !playerLeft.dead)
            {
                playerTarget = targetLeft.collider.GetComponentInParent<PlayerController>();
                if (!playerTarget.dead)
                {
                    target = targetLeft.transform.position;
                    photonView.RPC("FlipTrue", RpcTarget.AllBuffered);
                    targetFound = true;
                }

            }
            else
            {
                playerTarget = targetRight.collider.GetComponentInParent<PlayerController>();
                if (!playerTarget.dead)
                {
                    target = targetRight.transform.position;
                    photonView.RPC("FlipFalse", RpcTarget.AllBuffered);
                    targetFound = true;
                }
            }
        }
        playerTargetPosition = target;
        playerTargetController = playerTarget;
    }
    public void Respawn()
    {
        archerAnimator.ResetTrigger("Dead");
        isDead = false;
        gameObject.SetActive(true);
        this.transform.position = spawnPoint;
        this.health = maxHealth;
    }
    void Destroy()
    {
        photonView.RPC("disable", RpcTarget.AllBuffered);
    }
    public void IsAttacked(float dmg)
    {
            hit = true;
            this.health = health - dmg;
    }
    [PunRPC]
    void Die()
    {
        if (health <= 0)
        {
            isDead = true;
            Photon.GetComponent<GameLogic>().GameLogicPhotonView.RPC("GivePlayersXP", RpcTarget.AllBuffered, givenXp);
        }
    }
    [PunRPC]
    private void FlipTrue()
    {
        transform.localScale = new Vector3(-1, 1, 1);

    }
    [PunRPC]
    private void FlipFalse()
    {

        transform.localScale = new Vector3(1, 1, 1);

    }
}
