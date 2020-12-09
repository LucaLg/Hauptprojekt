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
    private Transform healthBar;

    //Attribute
    private Vector2 movement;
    private float moveSpeed = 2f;
    private float jumpForce = 5f;
    public float attackRange = 1f;
    public float maxHealth = 10f;
    public float health;
    

    
    void Start()
    {
        health = maxHealth;
        healthBar = transform.Find("HealthBar/Bar");
        animPlayer = GetComponent<Animator>();
        animPlayer.SetBool("Grounded", true);
        playerRb = GetComponent<Rigidbody2D>();
        photonView = GetComponent<PhotonView>();
        spriteR = GetComponent<SpriteRenderer>();
        playerBox = GetComponent<BoxCollider2D>();
        attackPoint = GetComponentInChildren<Transform>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!photonView.IsMine) return;

        //Movement
        movement.x = Input.GetAxisRaw("Horizontal");
        if(Input.GetAxis("Horizontal") != 0) {
            playerRb.MovePosition(playerRb.position + movement * moveSpeed * Time.fixedDeltaTime);
            
            animPlayer.SetBool("isMoving", true);
        }
        else
        {
            animPlayer.SetBool("isMoving", false);
        }
        bool grounded = isGrounded();
        if (Input.GetKeyDown("space") && grounded){
            Debug.Log("Jump");
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
        if (Input.GetKeyDown("b"))
        {
            Debug.Log("Attack pressed");
            photonView.RPC("Attack", RpcTarget.AllBuffered);
        }

        //Update Health (Ergibt 0 - Warum??)
        float healthPercentage = health / maxHealth;
        Debug.Log(healthPercentage);
        healthBar.localScale = new Vector3(healthPercentage, 1, 1);
    }
    private bool isGrounded()
    {
        RaycastHit2D raycastHit2D =  Physics2D.BoxCast(playerBox.bounds.center, playerBox.bounds.size, 0f, Vector2.down,0.1f, platformLayer);
        bool grounded = raycastHit2D.collider != null;
        Debug.Log(grounded);
        animPlayer.SetBool("Grounded", grounded);
        return (grounded);
    }
    [PunRPC]
    private void Attack()
    {
        //Play Animation
        animPlayer.SetTrigger("Attack");
        Debug.Log("Attack entered");
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
        //spriteR.flipX = true;
    }
    [PunRPC]
    private void FlipFalse()
    {

        transform.localScale = new Vector3(1, 1, 1);
        //spriteR.flipX = false;
    }
    
}
