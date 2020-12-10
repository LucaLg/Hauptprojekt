using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;

public class EnemyController : MonoBehaviour,IPunObservable
{
    public float maxHealth;
    public float health;
    private PhotonView photonView;
    bool isDead = false;
    public float enemyMoveSpeed;
    private Rigidbody2D enemyRb;
    private Animator enemyAnimator;
    //Healthbar
    public Slider healthbar;
    private Color lowHealth;
    private Color highHealth;
    private Vector3 offset = new Vector3(0f, 0.8f, 0);
    private float[] healthArray;
    //Hit
    public bool hit = false;
   
    [SerializeField] LayerMask playerLayer;
    // Start is called before the first frame update
    void Start()
    {
        photonView = GetComponent<PhotonView>();
        enemyRb = GetComponent<Rigidbody2D>();
        enemyAnimator = GetComponent<Animator>();
        
       
        //HealthbarSetup
        health = maxHealth;
        //healthbar = GetComponentInChildren<Canvas>().GetComponentInChildren<Slider>();
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
        PhotonNetwork.Destroy(gameObject);
    }
   
    void findAndAttackTarget()
    {
        
        bool targetFound = false;
        float distanceToStop = 1.1f;
        Vector3 target = transform.position;
        RaycastHit2D targetRight = Physics2D.Raycast(transform.position, Vector3.right,5f,playerLayer);
        RaycastHit2D targetLeft = Physics2D.Raycast(transform.position, Vector3.left, 5f, playerLayer);
        if(targetLeft.collider != null && targetRight.collider != null)
        {
            if(targetRight.distance >= targetLeft.distance)
            {
                 
                 target = targetLeft.transform.position;
                 photonView.RPC("FlipTrue", RpcTarget.AllBuffered);
                targetFound = true;
            }
            else
            {
                target = targetRight.transform.position;
                photonView.RPC("FlipFalse", RpcTarget.AllBuffered);
                targetFound = true;
            }
        }
        if(targetLeft.collider != null)
        {

             target= targetLeft.transform.position;
            photonView.RPC("FlipTrue", RpcTarget.AllBuffered);
            targetFound = true;
        }
        
        if(targetRight.collider != null) {
            
            target = targetRight.transform.position;
            photonView.RPC("FlipFalse", RpcTarget.AllBuffered);
            targetFound = true;
        }
        if (Vector3.Distance(transform.position, target) > distanceToStop) {
            enemyAnimator.SetInteger("AnimState", 1);
            transform.position = Vector3.MoveTowards(transform.position, target, Time.deltaTime * enemyMoveSpeed);
        }
        if (targetFound && Vector3.Distance(transform.position, target) <= distanceToStop+0.2f) {
            enemyAnimator.SetInteger("AnimState", 2);
        }
        if (!targetFound)
        {
            enemyAnimator.SetInteger("AnimState", 0);
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
        healthbar.transform.position = Camera.main.WorldToScreenPoint(transform.position + offset);
        healthbar.gameObject.SetActive(healthPara[0] < healthPara[1]);
        healthbar.value = healthPara[0];
        healthbar.maxValue = healthPara[1];
        healthbar.fillRect.GetComponentInChildren<Image>().color = Color.Lerp(lowHealth, highHealth, healthbar.normalizedValue);
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
