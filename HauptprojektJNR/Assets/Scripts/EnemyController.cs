using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;

public class EnemyController : MonoBehaviour,IPunObservable
{

    //Components
    [SerializeField] LayerMask playerLayers;
    private PhotonView photonView;
    private Rigidbody2D enemyRb;
    private Animator enemyAnimator;
    public Camera worldCamera;
    //Healthbar
    public Slider healthbar;
    private Color lowHealth;
    private Color highHealth;
    private Vector3 offset = new Vector3(-0.2f, 0.8f, 0);
    private float[] healthArray;
    public Transform attackPoint;
   
    //Attribute  
    public float attackRate;
    private float nextAttackTime = 0f;
    private bool blocked = false;
    public float maxHealth;
    public float health;
    bool isDead = false;
    public float enemyMoveSpeed;
    public bool hit = false;
    public float attackRange;
    public float damage;
    private Vector3 spawnPoint;
    void Start()
    {
        //SpawnPoint zum Respawn setzten
        spawnPoint = transform.position;
        //Suche AttackPoint
        foreach(Transform child in transform)
        {
            if(child.tag == "EnemyAttackPoint")
            {
                attackPoint = child.transform;
               
            }
        }
        photonView = GetComponent<PhotonView>();
        enemyRb = GetComponent<Rigidbody2D>();
        enemyAnimator = GetComponent<Animator>();
        
       
        //HealthbarSetup
        health = maxHealth;
        lowHealth = Color.red;
        highHealth = Color.green;
        healthArray = new float[] { health, maxHealth };
    }

    // Update is called once per frame
    void Update()
    {
        if (hit)
        {
            enemyAnimator.SetTrigger("hit");
            hit = false;
        }
        healthArray[0] = health;
        if (!photonView.IsMine)
        {
            return;
        }
        photonView.RPC("Die", RpcTarget.All);
        photonView.RPC("SetHealth", RpcTarget.AllBuffered, healthArray);
        if (isDead)
        {

            if (photonView.IsMine) {
                enemyAnimator.SetTrigger("Dead");
                
                Invoke("Destroy", 0.8f);
            }
        }
        //photonView.RPC("attackZeit", RpcTarget.AllBuffered);
    }
    private void FixedUpdate()
    {
        findAndAttackTarget();
    }
    /*
     * Find Target 
     */
    
    void Destroy()
    {
        photonView.RPC("disable", RpcTarget.AllBuffered);
        //PhotonNetwork.Destroy(gameObject);
    }
    [PunRPC]
    void disable()
    {
        gameObject.SetActive(false);
    }

    void findAndAttackTarget()
    {
        PlayerController playerTarget = null;
        bool targetFound = false;
        float distanceToStop = attackRange ;
        Vector3 target = transform.position;
        RaycastHit2D targetRight = Physics2D.Raycast(transform.position, Vector3.right,5f,playerLayers);
        RaycastHit2D targetLeft = Physics2D.Raycast(transform.position, Vector3.left, 5f, playerLayers);
       
        if(targetLeft.collider != null && targetRight.collider != null)
        {
            if(targetRight.distance >= targetLeft.distance)
            {
                playerTarget = targetLeft.collider.GetComponentInParent<PlayerController>();
                 target = targetLeft.transform.position;
                 photonView.RPC("FlipTrue", RpcTarget.AllBuffered);
                 targetFound = true;
                
            }
            else
            {
                playerTarget = targetRight.collider.GetComponentInParent<PlayerController>();
                target = targetRight.transform.position;
                photonView.RPC("FlipFalse", RpcTarget.AllBuffered);
                targetFound = true;
            }
        }
        if(targetLeft.collider != null)
        {
            playerTarget = targetLeft.collider.GetComponentInParent<PlayerController>();
            target = targetLeft.transform.position;
            photonView.RPC("FlipTrue", RpcTarget.AllBuffered);
            targetFound = true;
        }
        
        if(targetRight.collider != null) {
            playerTarget = targetRight.collider.GetComponentInParent<PlayerController>();
            target = targetRight.transform.position;
            photonView.RPC("FlipFalse", RpcTarget.AllBuffered);
            targetFound = true;
        }
        if (Vector3.Distance(transform.position, target) > distanceToStop) {
            enemyAnimator.SetInteger("AnimState", 1);
            transform.position = Vector3.MoveTowards(transform.position, target, Time.deltaTime * enemyMoveSpeed);
        }
        if (playerTarget != null)
        {
            if (!playerTarget.dead && targetFound && Vector3.Distance(transform.position, target) <= distanceToStop + 0.5f)
            {
                //Attack

                photonView.RPC("Attack", RpcTarget.AllBuffered);

            }
            if (!targetFound)
            {
                enemyAnimator.SetInteger("AnimState", 0);
            }
        }
    }
    [PunRPC]
     void Attack()
    {

        if (Time.time >= nextAttackTime)
        {
               
            enemyAnimator.SetInteger("AnimState", 2);
           
           
            //Detect Enemies in range of Attack
            Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, playerLayers);
            //Damage
            foreach (CircleCollider2D enemy in hitEnemies)
            {
                //Damage wird nicht von anderer Klasse manipuliert
                if (!enemy.GetComponent<PlayerController>().dead)
                {
                    enemy.GetComponent<PlayerController>().IsAttacked(damage);
                }
                //enemy.GetComponentInParent<EnemyController>().health--;
            }
            
            nextAttackTime = Time.time + attackRate;
        }
        else
        {
            enemyAnimator.SetInteger("AnimState", 0);
        }
    }
    
        public void IsAttacked(float dmg)
    {
        if (!blocked)
        {
            hit = true;
            this.health = health - dmg;
        }
        
        
    }
   [PunRPC]
    void Die()
    {
        if (health ==0)
        {
            isDead = true;
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
    [PunRPC]
    public void SetHealth(float[] healthPara)
    {

        
        healthbar.transform.position = transform.position + offset;
        healthbar.gameObject.SetActive(healthPara[0] < healthPara[1]);
        healthbar.value = healthPara[0];
        healthbar.maxValue = healthPara[1];
        healthbar.fillRect.GetComponentInChildren<Image>().color = Color.Lerp(lowHealth, highHealth, healthbar.normalizedValue);
    }
    public void Respawn()
    {
        enemyAnimator.ResetTrigger("Dead");
        isDead = false;
        gameObject.SetActive(true);
        this.transform.position = spawnPoint;
        this.health = maxHealth;
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        
        if (stream.IsWriting)
        {
            stream.SendNext(health);
        }else if (stream.IsReading)
        {
            health = (float)stream.ReceiveNext();
        }
    }
}
