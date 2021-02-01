using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class BossController : MonoBehaviour
{
    /*
     * Boss hat Drei Phasen 
     * 1.Phase Melee Attack Boss Verfolgt Spieler -> Leicht Damage zu Machen Blocken nicht Notig
     * 2.Boss teleport auf Spieler drauf macht hohen Damage -> Blocken notig 
     * 3.Boss lauft weg von Spieler schiesst Summons Projektile -> Summon blocken ==> Phase 1
    **/
    //--Components--
    [SerializeField] LayerMask playerLayers;
    public Transform firepoint;
    public Transform attackPoint;
    public Transform healthBarOverHead;
    public PhotonView bossPhotonView;
    private BoxCollider2D bossCollider;
    private Animator bossAnimator;
    private Rigidbody2D bossRb;
    //--Parameter--
    public float health;
    public float maxHealth = 50;
    public float moveSpeed = 5;
    public float launchSpeed = 5f;
    private float bossPhase = 1;
    private PlayerController playerTargetController;
    private Vector3 playerTargetPosition;
    private bool targetFound = false;
    public float lookOnRange = 20f;
    public float meleeDamage = 5f;
    public float attackRange = 3f;
    private bool shootRight = false;
    //Animation Speed
    public float summonSpeed = 1f;
    public float attackSpeed = 1.5f;
    void Start()
    {
        bossAnimator = GetComponent<Animator>();
    }
    
    // Update is called once per frame
    void Update()
    {
        bossAnimator.SetFloat("AttackSpeed", attackSpeed);
    }

    void Attack()
    {
        if(bossPhase == 1)
        {
            //Langsam MeleeAttack
            attackSpeed = 0.7f;
            MeleeAttack();

        }
        else if(bossPhase == 2)
        {
            //Schnell MeleeAttack
            attackSpeed = 2f;
            MeleeAttack();
        }
        else if(bossPhase == 3)
        {
            //Summon 
            //Ranged Attack
            Shoot();
        }
    }
    void MeleeAttack()
    {
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange , playerLayers);
        //Damage
        foreach (CircleCollider2D enemy in hitEnemies)
        {
            //Damage wird nicht von anderer Klasse manipuliert
            if (!enemy.GetComponent<PlayerController>().dead)
            {
                enemy.GetComponent<PlayerController>().IsAttacked(meleeDamage);
            }
            //enemy.GetComponentInParent<EnemyController>().health--;
        }
    }
    void Shoot()
    {
        GameObject summon = PhotonNetwork.Instantiate("Arrow", firepoint.position, firepoint.rotation);
        if (shootRight)
        {
            summon.GetComponent<Rigidbody2D>().velocity = firepoint.right * launchSpeed;
        }
        else
        {
            summon.GetComponent<Rigidbody2D>().velocity = -1f * firepoint.right * launchSpeed;
        }
    }
    void Move()
    {
        if (bossPhase == 1)
        {
            //Schnell ein Spieler verfolgen
            
        }
        else if (bossPhase == 2)
        {
            //Auf einen Spieler teleportieren
        }
        else if (bossPhase == 3)
        {
            //Weglaufen vor Beiden Spielern
        }
    }
    void MovePhase1()
    {

    }
    void TelePortPhase2()
    {

    }
    void MovePhase3()
    {

    }
    void Die()
    {
        //Gebe Xp Offne Wand
    }
    public void IsAttacked(float dmg)
    {
        this.health = health - dmg;
    }
    void findTarget()
    {

        PlayerController playerTarget = null;
        Vector3 target = transform.position;
        RaycastHit2D targetRight = Physics2D.Raycast(transform.position, Vector3.right, lookOnRange, playerLayers); ;
        RaycastHit2D targetLeft = Physics2D.Raycast(transform.position, Vector3.left, lookOnRange, playerLayers);
        //Finde Links Target
        if (targetLeft.collider != null)
        {
            playerTarget = targetLeft.collider.GetComponentInParent<PlayerController>();
            if (!playerTarget.dead)
            {
                target = targetLeft.transform.position;
                bossPhotonView.RPC("FlipTrue", RpcTarget.AllBuffered);
                targetFound = true;
            }
            Debug.Log(targetFound + " Jaaa");
        }
        //Finde Rechts Target
        if (targetRight.collider != null)
        {
            playerTarget = targetRight.collider.GetComponentInParent<PlayerController>();
            if (!playerTarget.dead)
            {
                target = targetRight.transform.position;
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
                    target = targetLeft.transform.position;
                    bossPhotonView.RPC("FlipTrue", RpcTarget.AllBuffered);
                    targetFound = true;
                }

            }
            else
            {
                playerTarget = targetRight.collider.GetComponentInParent<PlayerController>();
                if (!playerTarget.dead)
                {
                    target = targetRight.transform.position;
                    bossPhotonView.RPC("FlipFalse", RpcTarget.AllBuffered);
                    targetFound = true;
                }
            }
        }
        playerTargetPosition = target;
        playerTargetController = playerTarget;

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
}
