using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlayerController : MonoBehaviourPun
{
    [SerializeField] private LayerMask platformLayer;
    public LayerMask enemyLayers;

    //Components
    private Animator animPlayer;
    Rigidbody2D playerRb;
    PhotonView photonView;
    SpriteRenderer spriteR;
    private BoxCollider2D playerBox;
    private Transform attackPoint;
    private Transform healthBarOverHead;
    private Transform healthBar;
    private Transform staminaBar;
    public GameObject playerCam;

    //Attribute
    private Vector2 movement;
    private float moveSpeed = 2f;
    private float jumpForce = 5f;
    public float attackRange = 1f;
    public float maxHealth = 10f;
    public float health;
    public float maxStamina = 10f;
    public float stamina;
    public float healthRegeneration = 0.5f;
    public float staminaRegeneration = 2f;
    private float regenerationTimer;
    

    
    void Start()
    {
        health = maxHealth;
        stamina = maxStamina;
        healthBarOverHead = transform.Find("HealthBarOverHead/Bar");
        healthBar = transform.Find("Camera/HealthBar/Bar");
        staminaBar = transform.Find("Camera/StaminaBar/Bar");
        animPlayer = GetComponent<Animator>();
        animPlayer.SetBool("Grounded", true);
        playerRb = GetComponent<Rigidbody2D>();
        photonView = GetComponent<PhotonView>();
        spriteR = GetComponent<SpriteRenderer>();
        playerBox = GetComponent<BoxCollider2D>();
        attackPoint = GetComponentInChildren<Transform>();
        if (photonView.IsMine)
        {
            playerCam.SetActive(true);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!photonView.IsMine) return;

        //Movement
        movement.x = Input.GetAxisRaw("Horizontal");
        if(Input.GetAxis("Horizontal") != 0) {
            playerRb.velocity = new Vector3(movement.x * moveSpeed, playerRb.velocity.y, 0);
            animPlayer.SetBool("isMoving", true);
        }
        else
        {
            animPlayer.SetBool("isMoving", false);
        }
        bool grounded = isGrounded();
        if (Input.GetKeyDown("space") && grounded && stamina >= 3){
            stamina -= 3;
            animPlayer.SetTrigger("Jump");
            playerRb.AddForce(new Vector2(0, 7), ForceMode2D.Impulse);
            playerRb.velocity = Vector2.up * jumpForce;
        }
        
        if (Input.GetKeyDown(KeyCode.A))
        {
            photonView.RPC("FlipTrue", RpcTarget.AllBuffered);
        }
        if (Input.GetKeyDown(KeyCode.D))
        {
            photonView.RPC("FlipFalse", RpcTarget.AllBuffered);
        }
        if (Input.GetKeyDown("b") && stamina >= 2)
        {
            stamina -= 2;
            photonView.RPC("Attack", RpcTarget.AllBuffered);
        }

        //Update Health & Stamina (Ergibt 0 - Warum?? Integer Division?)
        
         health += healthRegeneration*Time.deltaTime;
         if(health >= maxHealth)
         {
         health = maxHealth;
         }
         stamina += staminaRegeneration*Time.deltaTime;
         if(stamina >= maxStamina)
         {
         stamina = maxStamina;
         }
        float healthPercentage = health / maxHealth;
        float staminaPercentage = stamina / maxStamina;
        Debug.Log(healthPercentage + " | " + staminaPercentage + " | " + regenerationTimer);
        healthBar.localScale = new Vector3(healthPercentage, 1, 1);
        healthBarOverHead.localScale = new Vector3(healthPercentage, 1, 1);
        staminaBar.localScale = new Vector3(staminaPercentage, 1, 1);
        regenerationTimer += Time.deltaTime;
        
    }
    private bool isGrounded()
    {
        RaycastHit2D raycastHit2D =  Physics2D.BoxCast(playerBox.bounds.center, playerBox.bounds.size, 0f, Vector2.down,0.1f, platformLayer);
        bool grounded = raycastHit2D.collider != null;
        animPlayer.SetBool("Grounded", grounded);
        return (grounded);
    }
    [PunRPC]
    private void Attack()
    {
        //Play Animation
        animPlayer.SetTrigger("Attack");
        //Detect Enemies in range of Attack
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayers);
        //Damage
        foreach (BoxCollider2D enemy in hitEnemies)
        {
            enemy.GetComponentInParent<EnemyController>().health--;

        }
    }
    [PunRPC]
    private void FlipTrue()
    {
        transform.localScale = new Vector3(-1, 1, 1);
        playerCam.transform.localScale = new Vector3(-1, 1, 1);
        //spriteR.flipX = true;
    }
    [PunRPC]
    private void FlipFalse()
    {

        transform.localScale = new Vector3(1, 1, 1);
        playerCam.transform.localScale = new Vector3(1, 1, 1);
        //spriteR.flipX = false;
    }

}
