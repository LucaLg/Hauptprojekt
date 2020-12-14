using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;

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
    private Transform xpBar;
    public GameObject playerCam;
    public GameObject menuCanvas;
    public Text txtLevel;
    public Text txtAttrPoints;

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
    private int level = 1;
    private float xp = 0f;
    private float xpToNextLevel;
    private float baseXpNeeded = 100;
    private int attributePoints = 0;
    private bool menuOpen = false;
    

    
    void Start()
    {   
        photonView = GetComponent<PhotonView>();
        if (!photonView.IsMine) return;
        health = maxHealth;
        stamina = maxStamina;
        healthBarOverHead = transform.Find("HealthBarOverHead/Bar");
        healthBar = transform.Find("Camera/HealthBar/Bar");
        staminaBar = transform.Find("Camera/StaminaBar/Bar");
        xpBar = transform.Find("Camera/XpBar/Bar");
        animPlayer = GetComponent<Animator>();
        animPlayer.SetBool("Grounded", true);
        playerRb = GetComponent<Rigidbody2D>();
        
        spriteR = GetComponent<SpriteRenderer>();
        playerBox = GetComponent<BoxCollider2D>();
        attackPoint = GetComponentInChildren<Transform>();
        xpToNextLevel = baseXpNeeded * Mathf.Pow(level, 2) * 0.1f;
        if (photonView.IsMine)
        {
            playerCam.SetActive(true);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!photonView.IsMine) return;

        //Menu
        
        if (Input.GetKeyDown("tab"))
        {
            menuOpen = !menuOpen;
            menuCanvas.SetActive(menuOpen);
            
        }

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
            //Play Animation
            animPlayer.SetTrigger("Attack");
            stamina -= 2;
            photonView.RPC("Attack", RpcTarget.AllBuffered);
        }

        //Update Health, XP, Stamina (Ergibt manchmal 0 - Warum?? Integer Division?)
        
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

        if(xp >= xpToNextLevel)
        {
            LevelUp();
            
        }
        float healthPercentage = health / maxHealth;
        float staminaPercentage = stamina / maxStamina;
        float xpPercentage = xp / xpToNextLevel;
        Debug.Log(healthPercentage + " | " + staminaPercentage + " | " + xpPercentage);
        healthBar.localScale = new Vector3(healthPercentage, 1, 1);
        healthBarOverHead.localScale = new Vector3(healthPercentage, 1, 1);
        staminaBar.localScale = new Vector3(staminaPercentage, 1, 1);
        xpBar.localScale = new Vector3(xpPercentage, 1, 1);

        
        
        
    }

    private void LevelUp()
    {
        level++;
        xp -= xpToNextLevel;
        xpToNextLevel = baseXpNeeded * Mathf.Pow(level, 2) * 0.1f; //f(x) = x^2 * 0.1, x = level
        attributePoints += 1;
        txtAttrPoints.text = "Points: " + attributePoints;
        txtLevel.text = "Lvl " + level;

    }
    private bool isGrounded()
    {
        RaycastHit2D raycastHit2D =  Physics2D.BoxCast(playerBox.bounds.center, playerBox.bounds.size, 0f, Vector2.down,0.1f, platformLayer);
        bool grounded = raycastHit2D.collider != null;
        animPlayer.SetBool("Grounded", grounded);
        return (grounded);
    }

    public void IncreaseHealth()
    {

    }
    public void IncreaseStamina()
    {

    }
    public void IncreaseRange()
    {

    }
    public void IncreaseDamage()
    {

    }
    [PunRPC]
    private void Attack()
    {
        
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
