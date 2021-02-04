﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public class PlayerController : MonoBehaviourPun, IPunObservable
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
    public Transform healthBarOverHead;
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
    public float mana;
    public float maxMana = 10f;
    public float staminaRegeneration = 2f;
    private int level = 1;
    private float xp = 5f;
    private float xpToNextLevel;
    private float baseXpNeeded = 100;
    private int attributePoints = 0;
    private int skillPoints = 0;
    private bool menuOpen = false;
    public float damage=1f;
    private bool blocked = false;
    public bool dead = false;
    public Vector3 lastCheckpoint;
    public int doubleJump =2;
    public float healthPercentage;
    public PhotonView otherPlayer;
    GameData otherPlayerData = null;

    //BetterJump
    public float fallMultiplier = 2.5f;
    public float lowJumpMultiplier = 2f;

    private float lifeDrain;
    private float manaDrain;
    private float heal;
    private float rage;
    public float damageModifier = 1f;
    private int healLevel = 1;
    private int rageLevel = 1;
    private int drainManaLevel = 1;
    private int drainLifeLevel = 1;
    //AttackSpeed
    public float AttackSpeed;
    private float nextAttackTime = 0f;
    private float AttackAnimSpeedMultiplier = 1;
    private void Awake()
    {
        animPlayer = GetComponent<Animator>();
        animPlayer.SetBool("Grounded", true);
        health = maxHealth;
        stamina = maxStamina;
        mana = maxMana;
        Debug.Log("Awake finished");
    }
    void Start()
    {   
        
        photonView = GetComponent<PhotonView>();
        if (!photonView.IsMine) return;
       //healthBarOverHead = transform.Find("HealthBarOverHead/Bar");
        healthBar = transform.Find("Camera/HealthBar/Bar");
        staminaBar = transform.Find("Camera/StaminaBar/Bar");
        manaBar = transform.Find("Camera/ManaBar/Bar");
        xpBar = transform.Find("Camera/XpBar/Bar");
       
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
        rage = rageLevel * 3.05f;
        manaDrain = drainManaLevel * 0.05f;
        lifeDrain = drainLifeLevel * 0.05f;
        Debug.Log("Start finished");
    }

    // Update is called once per frame
    void Update()
    {

        if (!photonView.IsMine)
        {
            return;
        }
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
            
            if (doubleJump == 1)
            {
                stamina -= 3;
                animPlayer.SetTrigger("Jump");
                playerRb.AddForce(new Vector2(0, 7), ForceMode2D.Impulse);
                playerRb.velocity = Vector2.up * jumpForce;
            }
            else if(doubleJump == 0)
            {
                animPlayer.SetInteger("DoubleJump", doubleJump);
                stamina -= 3;
                playerRb.AddForce(new Vector2(0, 7), ForceMode2D.Impulse);
                playerRb.velocity = Vector2.up * (jumpForce-2f);
            }
        }
        if(Input.GetKeyDown("v") && mana > 3)
        {
            addMana(-3);
            photonView.RPC("CastHeal", RpcTarget.AllBuffered);
        }
        if(Input.GetKeyDown("c") && mana > 3)
        {
            addMana(-3);
            photonView.RPC("CastRage", RpcTarget.AllBuffered);
        }
        if (Input.GetKeyDown(KeyCode.A))
        {
            photonView.RPC("FlipTrue", RpcTarget.AllBuffered);
        }
        if (Input.GetKeyDown(KeyCode.D))
        {
            photonView.RPC("FlipFalse", RpcTarget.AllBuffered);
        }
        if (Time.time >= nextAttackTime && Input.GetKeyDown("b") && stamina >= 2)
        {
            //Play Animation
            animPlayer.SetTrigger("Attack");
            stamina -= 2;
            photonView.RPC("Attack", RpcTarget.AllBuffered);
            nextAttackTime = Time.time + AttackSpeed;
        }
       
        

        //Update Health, XP, Stamina (Ergibt manchmal 0 - Warum?? Integer Division?)

        addStamina(staminaRegeneration * Time.deltaTime);

       // healthPercentage = health / maxHealth;
        
        float staminaPercentage = stamina / maxStamina;
        float xpPercentage = xp / xpToNextLevel;
        float manaPercentage = mana / maxMana;
        healthPercentage = health / maxHealth;
        healthBar.localScale = new Vector3(healthPercentage, 1, 1);
        photonView.RPC("UpdateOverheadBar", RpcTarget.AllBuffered);
        staminaBar.localScale = new Vector3(staminaPercentage, 1, 1);
        xpBar.localScale = new Vector3(xpPercentage, 1, 1);
        manaBar.localScale = new Vector3(manaPercentage, 1, 1);
       //Debug.Log(skillPoints + " | " + healLevel);

        if (health <= 0) {
            photonView.RPC("Die", RpcTarget.AllBuffered);
        }
        //AttackAnimationSpeed
        photonView.RPC("IncreaseAttackAnimSpeed",RpcTarget.AllBuffered);
    }

    //Save & Load
    public void saveGame()
    {
        string destination = Application.persistentDataPath + "/save.json";
        //if(otherPlayer != null)
        //{
        //    otherPlayer.RPC("requestGameData", RpcTarget.AllBuffered);
        //}
        GameData data = new GameData(level, skillPoints, attributePoints, drainLifeLevel, drainManaLevel, rageLevel, healLevel, health, maxHealth, 
                mana, maxMana, maxStamina, xp, lastCheckpoint, damage, AttackSpeed, otherPlayerData);
        string jsonData = JsonUtility.ToJson(data, true);
        File.WriteAllText(destination, jsonData);
        Debug.Log("Saved at " + destination);
    }
    [PunRPC]
    public void requestGameData()
    {
        Debug.Log("Entered requestGameData");
        GameData data = new GameData(level, skillPoints, attributePoints, drainLifeLevel, drainManaLevel, rageLevel, healLevel, health, maxHealth,
                mana, maxMana,  maxStamina, xp, lastCheckpoint, damage, AttackSpeed);
        BinaryFormatter bf = new BinaryFormatter();
        var ms = new MemoryStream();
        bf.Serialize(ms, data);
        byte[] dataAsArray = ms.ToArray();
        otherPlayer.RPC("SaveOtherPlayerData", RpcTarget.AllBuffered, dataAsArray);
    }
    [PunRPC]
    public void SaveOtherPlayerData(byte[] dataAsArray)
    {
        Debug.Log("Entered SaveOtherPlayerData");
        string destination = Application.persistentDataPath + "/savePlayer2.json";
        //Convert bytearray zu GameData
        var ms = new MemoryStream();
        var bf = new BinaryFormatter();
        ms.Write(dataAsArray, 0, dataAsArray.Length);
        ms.Seek(0, SeekOrigin.Begin);
        GameData data = (GameData)bf.Deserialize(ms);
        string jsonData = JsonUtility.ToJson(data, true);
        File.WriteAllText(destination, jsonData);
    }

    //RPC: LoadFromGameData wird von masterClient.PhotonScript aufgerufen
    //[PunRPC]
    //public void LoadPlayerStateFromGameData(byte[] dataAsArray)
    //{
    //    //Convert bytearray zu GameData
    //    Debug.Log("LoadPlayer entered");
    //    var ms = new MemoryStream();
    //    var bf = new BinaryFormatter();
    //    ms.Write(dataAsArray, 0, dataAsArray.Length);
    //    ms.Seek(0, SeekOrigin.Begin);
    //    GameData data = (GameData) bf.Deserialize(ms);

    //    Debug.Log("Entered // Level:" + data.level + "Health: " + data.health + "Mana: " + data.mana);
    //    level = data.level;
    //    skillPoints = data.skillPoints;
    //    attributePoints = data.attributePoints;
    //    drainLifeLevel = data.lifeDrainLevel;
    //    drainManaLevel = data.manaDrainLevel;
    //    rageLevel = data.rageLevel;
    //    healLevel = data.healLevel;
    //    health = data.health;
    //    maxHealth = data.maxHealth;
    //    mana = data.mana;
    //    maxMana = data.maxMana;
    //    stamina = data.stamina;
    //    maxStamina = data.maxStamina;
    //    xp = data.XP;
    //    lastCheckpoint = new Vector3(data.currentCheckpointX, data.currentCheckpointY, 0);
    //    AttackSpeed = data.attackspeed;
    //    damage = data.damage;
    //    transform.position = lastCheckpoint;
    //    txtAttrPoints.text = "Points: " + attributePoints;
    //    txtSkillPoints.text = "Points: " + skillPoints;
    //    txtLevel.text = "Lvl " + level;
    //    Debug.Log("Load finished");
    //}
    public void LoadPlayerStateFromGameData(GameData data)
    {
        

        Debug.Log("Entered // Level:" + data.level + "Health: " + data.health + "Mana: " + data.mana);
        level = data.level;
        skillPoints = data.skillPoints;
        attributePoints = data.attributePoints;
        drainLifeLevel = data.lifeDrainLevel;
        drainManaLevel = data.manaDrainLevel;
        rageLevel = data.rageLevel;
        healLevel = data.healLevel;
        health = data.health;
        maxHealth = data.maxHealth;
        mana = data.mana;
        maxMana = data.maxMana;
        stamina = data.stamina;
        maxStamina = data.maxStamina;
        xp = data.XP;
        lastCheckpoint = new Vector3(data.currentCheckpointX, data.currentCheckpointY, 0);
        AttackSpeed = data.attackspeed;
        damage = data.damage;
        transform.position = lastCheckpoint;
        txtAttrPoints.text = "Points: " + attributePoints;
        txtSkillPoints.text = "Points: " + skillPoints;
        txtLevel.text = "Lvl " + level;
        Debug.Log("Load finished");
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
        if(xp >= xpToNextLevel)
        {
            LevelUp();
        }

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
            txtAttrPoints.text = "Points: " + attributePoints.ToString();
            maxHealth += 2f;
            addHealth(2f);
        }
        
    }
    public void IncreaseStamina()
    {
        if(attributePoints > 0)
        {
            attributePoints -= 1;
            txtAttrPoints.text = "Points: " + attributePoints.ToString();
            maxStamina += 1f;
            addStamina(1f);

        }
        
    }
    /*
     * AttackSpeed Rpc Um Animation Anzupassen
     */
    public void IncreaseAttackspeed()
    {
        if (attributePoints > 0)
        {
            attributePoints -= 1;
            txtAttrPoints.text = "Points: " + attributePoints.ToString();
            AttackSpeed -= 0.1f;
            AttackAnimSpeedMultiplier += 0.05f;
            
        }
    }
    [PunRPC]
    public void IncreaseAttackAnimSpeed()
    {
        animPlayer.SetFloat("AttackSpeed", AttackAnimSpeedMultiplier);
    }
    public void IncreaseDamage()
    {
        if (attributePoints > 0)
        {
            attributePoints -= 1;
            txtAttrPoints.text = "Points: " + attributePoints.ToString();
            damage += 0.3f;

        }
        
    }
    public void LevelUpDrainLife()
    {
        if(skillPoints > 0)
        {
            skillPoints -= 1;
            txtSkillPoints.text = "Points: " + skillPoints;
            drainLifeLevel += 1;
            lifeDrain = drainLifeLevel * 0.05f;
        }
        

    }
    public void LevelUpDrainMana()
    {
        if (skillPoints > 0)
        {
            skillPoints -= 1;
            txtSkillPoints.text = "Points: " + skillPoints;
            drainManaLevel += 1;
            manaDrain = drainManaLevel * 0.05f;
        }
        

    }
    public void LevelUpHeal()
    {
        if (skillPoints > 0)
        {
            skillPoints -= 1;
            txtSkillPoints.text = "Points: " + skillPoints;
            healLevel += 1;
            heal = healLevel * 0.05f;
        }
        
    }
    public void LevelUpRage()
    {
        if (skillPoints > 0)
        {
            
            skillPoints -= 1;
            txtSkillPoints.text = "Points: " + skillPoints;
            rageLevel += 1;
            rage = rageLevel * 0.05f;
        }
        
    }

    [PunRPC]
    public void addHealth(float pHealth)
    {
        health += pHealth;
        if (health > maxHealth){
            health = maxHealth;
        }
        if(health < 0)
        {
            health = 0;
        }
    }
    public void addStamina(float pStamina)
    {
        stamina += pStamina;
        if(stamina > maxStamina)
        {
            stamina = maxStamina;
        }
        if (stamina < 0)
        {
            stamina = 0;
        }
    }
    public void addMana(float pMana)
    {
        mana += pMana;
        if(mana > maxMana)
        {
            mana = maxMana;
        }
        if (mana < 0)
        {
            mana = 0;
        }
    }
    [PunRPC]
    private void addXP(float amount)
    {
        
        xp += amount;
        if(xp >= xpToNextLevel)
        {
            LevelUp();
        }
    }
    [PunRPC]
    private void UpdateOverheadBar()
    {
        healthBarOverHead.localScale = new Vector3(healthPercentage, 1, 1);
    }
    [PunRPC]
    private void CastHeal()
    {
        photonView.RPC("addHealth", RpcTarget.AllBuffered, maxHealth*heal);
        if(otherPlayer != null)
        {
            otherPlayer.RPC("addHealth", RpcTarget.AllBuffered, maxHealth * heal);
        }
    }
    [PunRPC]
    private void CastRage()
    {
        photonView.RPC("giveRageBuff", RpcTarget.AllBuffered, rage);
        if (otherPlayer != null)
        {
            otherPlayer.RPC("giveRageBuff", RpcTarget.AllBuffered, rage);
        }
    }
    [PunRPC]
    private void giveRageBuff(float buff)
    {
        IEnumerator coroutine = RageBuff(buff);
        StartCoroutine(coroutine);
    }
    private IEnumerator RageBuff(float buff)
    {
        Debug.Log(damageModifier);
        damageModifier += buff;
        Debug.Log(damageModifier);
        yield return new WaitForSeconds(5f);
        damageModifier -= buff;
        Debug.Log(damageModifier);


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
            enemy.GetComponent<EnemyController>().IsAttacked(damage * damageModifier);
            addHealth(damage * damageModifier * lifeDrain);
            addMana(damage * damageModifier * manaDrain);

            //enemy.GetComponentInParent<EnemyController>().health--;
        }
    }
   
    [PunRPC]
    void Die()
    {

        animPlayer.SetBool("Alive",false);
        animPlayer.SetTrigger("Die");
        dead = true;
    }
    public void Respawn()
    {
        if (photonView.IsMine)
        {
            playerCam.SetActive(true);
        }
        animPlayer.SetBool("Alive",true);
        dead = false;
        xp = 0;
        health = maxHealth;
    }
   
    public void IsAttacked(float dmg)
    {
        if (!blocked)
        {
            //photonView.RPC("addHealth", RpcTarget.AllBuffered, -dmg);
            addHealth(-dmg);
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
    public void setOtherPlayer(PhotonView other)
    {
        otherPlayer = other;
    }
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(health);
            stream.SendNext(AttackAnimSpeedMultiplier);
            stream.SendNext(healthPercentage);
        }
        else
        {
            health = (float)stream.ReceiveNext();
            AttackAnimSpeedMultiplier = (float)stream.ReceiveNext();
            healthPercentage = (float)stream.ReceiveNext();
        }
    }
}
