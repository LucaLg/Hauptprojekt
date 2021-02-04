using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.Tilemaps;
public class BossController : MonoBehaviour, IPunObservable
{
    /*
     * Boss hat Drei Phasen 
     * 1.Phase Melee Attack Boss Verfolgt Spieler -> Leicht Damage zu Machen Blocken nicht Notig
     * 2.Boss teleport auf Spieler drauf macht hohen Damage -> Blocken notig 
     * 3.Boss lauft weg von Spieler schiesst Summons Projektile -> Summon blocken ==> Phase 1
    **/
    //--Components--
    [SerializeField] LayerMask playerLayers;
    [SerializeField] LayerMask wallLayer;
    public Transform firepoint;
    public Transform attackPoint;
    public Transform healthBarOverHead;
    public PhotonView bossPhotonView;
    private BoxCollider2D bossCollider;
    private Animator bossAnimator;
    private Rigidbody2D bossRb;
    public Transform summon0;
    public Transform summon1;
    public Transform summon2;
    public GameObject bossGate;
    public Transform boundsLeft;
    public Transform boundsRight;
    public GameObject Photon;
    //--Parameter--
    public float health;
    public float maxHealth = 50;
    public float moveSpeed = 5f;
    public float launchSpeed = 5f;
    public int bossPhase = 1;
    private PlayerController playerTargetController;
    private Vector3 playerTargetPosition;
    private Vector3[] playerTargetPositions;
    private bool targetFound = false;
    public float lookOnRange = 20f;
    public float meleeDamage = 5f;
    public float attackRange = 1f;
    private bool shootRight = false;
    private bool jump = true;
    private GameObject[] summons;
    private Vector2 spawnPoint;
    private bool mL = true;
    //Animation Speed
    public float summonSpeed = 1f;
    public float attackSpeed = 1.5f;
    private int phaseDuration = 0;
    public GameObject summon;
    public int counterPhase3 = 5;
    public float xpOnDeath = 20f;
    void Start()
    {
        bossAnimator = GetComponent<Animator>();
        health = maxHealth;
        summons = new GameObject[3];
        spawnPoint = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        if (health <= 0)
        {
            bossAnimator.SetTrigger("Die");
        }
        float healthPercentage = health / maxHealth;
        healthBarOverHead.localScale = new Vector3(healthPercentage, 1, 1);
        bossAnimator.SetInteger("IdleState", bossPhase - 1);
        bossAnimator.SetFloat("AttackSpeed", attackSpeed);
        if(playerTargetPosition.x > this.transform.position.x)
        {
            bossPhotonView.RPC("FlipFalse", RpcTarget.AllBuffered);
        }
        else
        {
            bossPhotonView.RPC("FlipTrue", RpcTarget.AllBuffered);
        }
        newFindTarget();
        if (bossGate.activeSelf == true)
        {
            if (targetFound && bossPhase == 1)
            {
                
                if (Vector2.Distance(playerTargetPosition, this.transform.position) <= 1.5f)
                {

                    Attack();
                }
                else
                {
                    Move();
                }

            }
            if (targetFound && bossPhase == 3)
            {
                Move();
                Attack();

            }
            if (targetFound && bossPhase == 2)
            {
                Move();

            }

        }
        if (health < maxHealth)
        {
            bossPhotonView.RPC("updateBossPhase", RpcTarget.AllBuffered);
        }
    }
    [PunRPC]
    void updateBossPhase()
    {
        float change = maxHealth / 4;

        if (health > 3 * change || health < change || counterPhase3 ==0)
        {
            bossPhase = 1;
            
        }
        if (health < 3 * change)
        {
            bossPhase = 2;
        }
        if (health < 2 * change && counterPhase3 >0)
        {
            bossPhase = 3;
        }
    }
    void Attack()
    {
        if (bossPhase == 1)
        {
            //Langsam MeleeAttack
            attackSpeed = 0.7f;
            bossAnimator.SetTrigger("Attack");

        }
        else if (bossPhase == 2)
        {
            //Schnell MeleeAttack
            attackSpeed = 2f;
            bossAnimator.SetTrigger("Attack");
        }
        else if (bossPhase == 3)
        {
            //Summon 
            //Ranged Attack
            
            bossAnimator.SetTrigger("Shoot");
            //Shoot();
        }
    }
    void MeleeAttack()
    {
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, playerLayers);
        //Damage
        foreach (CircleCollider2D enemy in hitEnemies)
        {
            //Damage wird nicht von anderer Klasse manipuliert
            if (!enemy.GetComponent<PlayerController>().dead)
            {
                enemy.GetComponent<PlayerController>().IsAttacked(meleeDamage);
            }

        }
    }
    void Shoot()
    {

        spawnSummon();
        counterPhase3--;
        foreach (GameObject summon in summons)
        {
            summon.GetComponent<Rigidbody2D>().velocity = summon.transform.up.normalized * -1f * launchSpeed;
        }
    }
    void Move()
    {
        if (bossPhase == 1)
        {
            //Schnell ein Spieler verfolgen

            transform.position = Vector2.MoveTowards(transform.position, playerTargetPosition, Time.deltaTime * moveSpeed);

        }
        else if (bossPhase == 2)
        {

            bossAnimator.SetTrigger("teleport");
            //Auf einen Spieler teleportieren
        }
        else if (bossPhase == 3)
        {
            //Hochfliegen 5 nach rechts dann 5 nach links repeat
            MovePhase3();
        }
    }

    void TelePortPhase2()
    {
        if (jump)
        {

            transform.position = playerTargetPosition + new Vector3(0.8f, 1, 0);
            bossAnimator.SetTrigger("Attack");
            jump = false;
        }
        else
        {
            transform.position = spawnPoint;
            jump = true;
        }
    }
    void MovePhase3()
    {
        if (Vector2.Distance(boundsLeft.position, transform.position) < 1 && mL)
        {
            mL = false;
        }
        if (Vector2.Distance(boundsRight.position, transform.position) < 1 && !mL)
        {
            mL = true;
        }
        if (mL)
        {
            moveLeft();
        }
        else
        {
            moveRight();
        }
    }
    void moveRight()
    {
        transform.position = Vector2.MoveTowards(this.transform.position, boundsRight.position, moveSpeed * Time.deltaTime);
    }
    void moveLeft()
    {
        transform.position = Vector2.MoveTowards(this.transform.position, boundsLeft.position, moveSpeed * Time.deltaTime);
    }
    void spawnSummon()
    {

        summons[0] = Instantiate(summon, summon0.position, summon0.rotation);

        summons[1] =Instantiate(summon, summon1.position, summon1.rotation);

        summons[2] =  Instantiate(summon, summon2.position, summon2.rotation);
    }
    [PunRPC]
    void Die()
    {
        //Gebe Xp Offne Wand
        Photon.GetComponent<GameLogic>().GameLogicPhotonView.RPC("GivePlayersXP", RpcTarget.AllBuffered, xpOnDeath);
        this.gameObject.SetActive(false);
        bossGate.SetActive(false);
    }
    public void IsAttacked(float dmg)
    {
        this.health = health - dmg;
    }
    void newFindTarget()
    {
        RaycastHit2D[] targetsLeft = Physics2D.RaycastAll(attackPoint.position, Vector3.left, lookOnRange, playerLayers);
        RaycastHit2D[] targetsRight = Physics2D.RaycastAll(attackPoint.position, Vector3.right, lookOnRange, playerLayers);
        RaycastHit2D[] targets = new RaycastHit2D[] { };
        if (targetsLeft.Length > 0)
        {
            targets = targetsLeft;
        }
        if (targetsRight.Length > 0)
        {
            targets = targetsRight;
        }
        if (targetsRight.Length > 0 && targetsLeft.Length > 0)
        {
            targets = new RaycastHit2D[] { targetsLeft[0], targetsRight[0] };
        }

        if (targets.Length == 1)
        {
            playerTargetPosition = targets[0].transform.position;
            targetFound = true;
        }
        else if(targets.Length >1)
        {
            PlayerController player1 = targets[0].collider.GetComponentInParent<PlayerController>();
            PlayerController player2 = targets[1].collider.GetComponentInParent<PlayerController>();

            if (Vector2.Distance(this.transform.position, targets[0].transform.position) <= Vector2.Distance(this.transform.position, targets[1].transform.position))
            {
                
                    playerTargetController = player1;
                    playerTargetPosition = targets[0].transform.position;
                    targetFound = true;
            }
            else
            {
                playerTargetController = player2;
                playerTargetPosition = targets[1].transform.position;
                targetFound = true;
            }
            if (player1.dead)
            {
                playerTargetController = player2;
                playerTargetPosition = targets[1].transform.position;
                targetFound = true;
            }
            if (player2.dead)
            {
                playerTargetController = player1;
                playerTargetPosition = targets[0].transform.position;
                targetFound = true;
            }
           
        }

    }
    /*void findTarget()
    {

        PlayerController playerTarget = null;
        Vector2 target = new Vector2(0, 0);
        RaycastHit2D targetRight = Physics2D.Raycast(attackPoint.position, Vector3.right, lookOnRange, playerLayers);
        RaycastHit2D targetLeft = Physics2D.Raycast(attackPoint.position, Vector3.left, lookOnRange, playerLayers);
        *//*if (PhotonNetwork.PlayerList.Length > 1)
        {
            RaycastHit2D[] targetsLeft = Physics2D.RaycastAll(attackPoint.position, Vector3.left, lookOnRange, playerLayers);
            RaycastHit2D[] targetsRight = Physics2D.RaycastAll(attackPoint.position, Vector3.right, lookOnRange, playerLayers);
            if (targetsLeft.Length == 2 || targetsRight.Length == 2 || (targetsRight.Length == 1 && targetsRight.Length == 1))
            {

                //direction false = rechts true = links
                bool direction = false;
                RaycastHit2D[] targets;

                if (targetsLeft.Length == 2)
                {
                    targets = targetsLeft;
                    direction = true;
                }
                else if (targetsRight.Length == 2)
                {
                    targets = targetsRight;
                    direction = false;

                }
                else if(targetsRight.Length == 1 && targetsRight.Length == 1)
                {
                    targets = new RaycastHit2D[] { targetsLeft[0], targetsRight[0] };
                }
                PlayerController player1 = targets[0].collider.GetComponentInParent<PlayerController>();
                PlayerController player2 = targets[1].collider.GetComponentInParent<PlayerController>();
                if (player1.dead)
                {
                    playerTargetController = player2;
                    playerTargetPosition = targets[1].transform.position;

                    targetFound = true;
                }
                if (player2.dead)
                {
                    playerTargetController = player1;
                    playerTargetPosition = targets[0].transform.position;

                    targetFound = true;
                }
                if (Vector2.Distance(this.transform.position, targets[0].transform.position) <= Vector2.Distance(this.transform.position, targets[1].transform.position))
                {
                    playerTargetController = player1;
                    playerTargetPosition = targets[0].transform.position;
                    direction = true;
                    targetFound = true;
                }
                else
                {
                    playerTargetController = player2;
                    playerTargetPosition = targets[1].transform.position;
                    direction = false;
                    targetFound = true;
                }
                if (direction)
                {
                    bossPhotonView.RPC("FlipTrue", RpcTarget.AllBuffered);
                }
                else
                {
                    bossPhotonView.RPC("FlipFalse", RpcTarget.AllBuffered);
                }
            }
        }
        else
        {*//*
        if (targetLeft.collider != null)
        {
            playerTarget = targetLeft.collider.GetComponentInParent<PlayerController>();
            if (!playerTarget.dead)
            {
                playerTargetPosition = targetLeft.transform.position;
                bossPhotonView.RPC("FlipTrue", RpcTarget.AllBuffered);
                targetFound = true;
            }

        }
        //Finde Rechts Target
        if (targetRight.collider != null)
        {
            playerTarget = targetRight.collider.GetComponentInParent<PlayerController>();
            if (!playerTarget.dead)
            {
                playerTargetPosition = targetRight.transform.position;
                bossPhotonView.RPC("FlipFalse", RpcTarget.AllBuffered);
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
                    playerTargetPosition = targetLeft.transform.position;
                    bossPhotonView.RPC("FlipTrue", RpcTarget.AllBuffered);
                    targetFound = true;
                }

            }
            else
            {
                playerTarget = targetRight.collider.GetComponentInParent<PlayerController>();
                if (!playerTarget.dead)
                {
                    playerTargetPosition = targetRight.transform.position;
                    bossPhotonView.RPC("FlipFalse", RpcTarget.AllBuffered);
                    targetFound = true;
                }
            }
        }

        *//*}*//*

    }*/
    [PunRPC]
    public void respawn()
    {
        bossAnimator.ResetTrigger("Die");
        this.gameObject.SetActive(true);
        this.gameObject.transform.position = spawnPoint;
        bossPhase = 1;
        health = maxHealth;
        bossGate.SetActive(false);
    }
    [PunRPC]
    private void FlipTrue()
    {
        transform.localScale = new Vector3(-1, 1, 1);
        shootRight = false;
    }
    [PunRPC]
    private void FlipFalse()
    {

        transform.localScale = new Vector3(1, 1, 1);
        shootRight = true;
    }
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {

        if (stream.IsWriting)
        {
            stream.SendNext(health);
        }
        else if (stream.IsReading)
        {
            health = (float)stream.ReceiveNext();
        }
    }
}
