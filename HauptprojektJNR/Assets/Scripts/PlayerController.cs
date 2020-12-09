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

    //Attribute
    private Vector2 movement;
    private float moveSpeed = 2f;
    private float jumpForce = 5f;
    public float attackRange = 1f;
    

    
    void Start()
    {
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
        movement.x = Input.GetAxisRaw("Horizontal");
        if (!photonView.IsMine) return;
        if(Input.GetAxis("Horizontal") != 0) {
            
           playerRb.MovePosition(playerRb.position + movement * moveSpeed * Time.fixedDeltaTime);
            animPlayer.SetInteger("AnimState", 1);
        }
        else
        {
            animPlayer.SetInteger("AnimState", 0);
        }
        if (Input.GetKeyDown("space") && isGrounded()){
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
            photonView.RPC("Attack", RpcTarget.AllBuffered);
        }
    }
    private bool isGrounded()
    {
       RaycastHit2D raycastHit2D =  Physics2D.BoxCast(playerBox.bounds.center, playerBox.bounds.size, 0f, Vector2.down,1f, platformLayer);
        Debug.Log(raycastHit2D.collider != null);
        return (raycastHit2D.collider != null);
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
            
            Debug.Log("Damage");
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
