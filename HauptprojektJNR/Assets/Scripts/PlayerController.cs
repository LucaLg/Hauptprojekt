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
    private Collider2D playerBox;
    public Transform attackPoint;
    private Transform healthBarOverHead;
    private Transform healthBar;
    private Transform staminaBar;
    private Transform manaBar;
    private Transform xpBar;
    public GameObject playerCam;
    public GameObject menuCanvas;
    public Text txtLevel;
    public Text txtAttrPoints;
    public Text txtSkillPoints;

    //Attribute
    private Vector2 movement;
    public float moveSpeed = 2f;
    public float jumpForce = 5f;
    public float attackRange = 1f;
    public float maxHealth = 10f;
    public float health;
    public float maxStamina = 10f;
    public float stamina;
    public float mana = 10f;
    public float maxMana = 10f;
    public float staminaRegeneration = 2f;
    private int level = 1;
    private float xp = 5f;
    private float xpToNextLevel;
    private float baseXpNeeded = 100;
    private int attributePoints = 0;
    private int skillPoints = 5;
    private bool menuOpen = false;
    public float damage=1f;
    private bool blocked = false;
    public bool dead = false;
    public Vector3 lastCheckpoint;
    public int doubleJump =2;
    //BetterJump
    public float fallMultiplier = 2.5f;
    public float lowJumpMultiplier = 2f;
    private float lifeDrain;
    private float manaDrain;
    private float heal;
    private float rage;
    private int healLevel = 1;
    private int rageLevel = 1;
    private int drainManaLevel = 1;
    private int drainLifeLevel = 1;
    
    void Start()
    {   
        photonView = GetComponent<PhotonView>();
        if (!photonView.IsMine) return;
        health = maxHealth;
        stamina = maxStamina;
        healthBarOverHead = transform.Find("HealthBarOverHead/Bar");
        healthBar = transform.Find("Camera/HealthBar/Bar");
        staminaBar = transform.Find("Camera/StaminaBar/Bar");
        manaBar = transform.Find("Camera/ManaBar/Bar");
        xpBar = transform.Find("Camera/XpBar/Bar");
        animPlayer = GetComponent<Animator>();
        animPlayer.SetBool("Grounded", true);
        playerRb = GetComponent<Rigidbody2D>();
        
        spriteR = GetComponent<SpriteRenderer>();
        playerBox = GetComponent<CircleCollider2D>();
        //attackPoint = GetComponentInChildren<Transform>();
        xpToNextLevel = baseXpNeeded * Mathf.Pow(level, 2) * 0.1f;
        if (photonView.IsMine)
        {
            playerCam.SetActive(true);
        }
        heal = healLevel * 0.05f;
        rage = rageLevel * 0.05f;
        manaDrain = drainManaLevel * 0.05f;
        lifeDrain = drainLifeLevel * 0.05f;
    }

    // Update is called once per frame
    void Update()
    {
        
        if (!photonView.IsMine) return;
        if (dead)
        {
            
            return;
        }
        //Menu

        if (Input.GetKeyDown("tab"))
        {
            menuOpen = !menuOpen;
            menuCanvas.SetActive(menuOpen);

        }
        
        //Movement
        movement.x = Input.GetAxisRaw("Horizontal");
        if (Input.GetAxis("Horizontal") != 0 ) {
            playerRb.velocity = new Vector3(movement.x * moveSpeed, playerRb.velocity.y, 0);
            animPlayer.SetBool("isMoving", true);
        }
        else
        {
            animPlayer.SetBool("isMoving", false);
        }
        bool grounded = isGrounded();
        if (grounded)
        {
            doubleJump = 2;
            animPlayer.SetInteger("DoubleJump", doubleJump);
        }
        //BetterJump
        if(playerRb.velocity.y < 0)
        {
            playerRb.velocity += Vector2.up * Physics2D.gravity.y * (fallMultiplier - 1) * Time.deltaTime;
        }
        if (Input.GetKeyDown("space")  && stamina >= 3 && (doubleJump >0 || grounded)) {
            doubleJump--;
            animPlayer.SetInteger("DoubleJump", doubleJump);
            if (doubleJump == 1)
            {
                stamina -= 3;
                animPlayer.SetTrigger("Jump");
                playerRb.AddForce(new Vector2(0, 7), ForceMode2D.Impulse);
                playerRb.velocity = Vector2.up * jumpForce;
            }
            else
            {
                stamina -= 3;
                
                playerRb.AddForce(new Vector2(0, 7), ForceMode2D.Impulse);
                playerRb.velocity = Vector2.up * (jumpForce-2f);
            }
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

        giveStamina(staminaRegeneration * Time.deltaTime);

        if(xp >= xpToNextLevel)
        {
            LevelUp();

        }
        float healthPercentage = health / maxHealth;
        float staminaPercentage = stamina / maxStamina;
        float xpPercentage = xp / xpToNextLevel;
        float manaPercentage = mana / maxMana;
        healthBar.localScale = new Vector3(healthPercentage, 1, 1);
        healthBarOverHead.localScale = new Vector3(healthPercentage, 1, 1);
        staminaBar.localScale = new Vector3(staminaPercentage, 1, 1);
        xpBar.localScale = new Vector3(xpPercentage, 1, 1);
        manaBar.localScale = new Vector3(manaPercentage, 1, 1);
        Debug.Log(skillPoints + " | " + healLevel);

        if (health <= 0) { 
        photonView.RPC("Die", RpcTarget.AllBuffered);
        }
       
    }
    public void SetCamera(Camera cam)
    {
        this.playerCam.SetActive(false);
        cam.enabled = true;
    }
    private void LevelUp()
    {
        level++;
        xp -= xpToNextLevel;
        xpToNextLevel = baseXpNeeded * Mathf.Pow(level, 2) * 0.1f; //f(x) = x^2 * 0.1, x = level
        attributePoints += 1;
        skillPoints += 1;
        txtAttrPoints.text = "Points: " + attributePoints;
        txtSkillPoints.text = "Points: " + skillPoints;
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
        if(attributePoints > 0)
        {
            attributePoints -= 1;
            maxHealth += 2f;
            giveHealth(2f);
        }
        
    }
    public void IncreaseStamina()
    {
        if(attributePoints > 0)
        {
            attributePoints -= 1;
            maxStamina += 1f;
            giveStamina(1f);

        }
        
    }
    public void IncreaseRange()
    {
        if (attributePoints > 0)
        {
            attributePoints -= 1;

        }
    }
    public void IncreaseDamage()
    {
        if (attributePoints > 0)
        {
            attributePoints -= 1;
            damage += 0.3f;

        }
        
    }
    public void LevelUpDrainLife()
    {
        if(skillPoints > 0)
        {
            skillPoints -= 1;
            drainLifeLevel += 1;
            lifeDrain = drainLifeLevel * 0.05f;
        }
        

    }
    public void LevelUpDrainMana()
    {
        if (skillPoints > 0)
        {
            skillPoints -= 1;
            drainManaLevel += 1;
            manaDrain = drainManaLevel * 0.05f;
        }
        

    }
    public void LevelUpHeal()
    {
        if (skillPoints > 0)
        {
            skillPoints -= 1;
            healLevel += 1;
            heal = healLevel * 0.05f;
        }
        
    }
    public void LevelUpRage()
    {
        if (skillPoints > 0)
        {
            skillPoints -= 1;
            rageLevel += 1;
            rage = rageLevel * 0.05f;
        }
        
    }
    public void giveHealth(float pHealth)
    {
        health += pHealth;
        if (health > maxHealth){
            health = maxHealth;
        }
    }
    public void giveStamina(float pStamina)
    {
        stamina += pStamina;
        if(stamina > maxStamina)
        {
            stamina = maxStamina;
        }
    }
    public void giveMana(float pMana)
    {
        mana += pMana;
        if(mana > maxMana)
        {
            mana = maxMana;
        }
    }
    [PunRPC]
    private void Attack()
    {
        
        //Detect Enemies in range of Attack
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayers);
        //Damage
        foreach (BoxCollider2D enemy in hitEnemies)
        {
            //Damage wird nicht von anderer Klasse manipuliert
            enemy.GetComponent<EnemyController>().IsAttacked(damage);
            giveHealth(damage * lifeDrain);
            giveMana(damage * manaDrain);

            //enemy.GetComponentInParent<EnemyController>().health--;
        }
    }
   
    [PunRPC]
    void Die()
    {
        //Resette Health auf MaxHealth
        //Resette XP

        
        dead = true;
       
        
    }
    public void Respawn()
    {
        dead = false;
        xp = 0;
        health = maxHealth;
    }
   
    public void IsAttacked(float dmg)
    {
        if (!blocked)
        {
            health = health - dmg;
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
    public Vector3 CameraPosition()
    {
        return playerCam.transform.position;
    }
}
