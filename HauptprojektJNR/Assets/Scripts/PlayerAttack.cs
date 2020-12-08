using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
public class PlayerAttack : MonoBehaviour
{
    private Animator playerAnim;
    private Transform attackPoint;
    public float attackRange = 1f;
    public LayerMask enemyLayers;
    private PhotonView photonView;

    // Start is called before the first frame update
    void Start()
    {
        playerAnim = GetComponent<Animator>();
        attackPoint = GetComponentInChildren<Transform>();
        photonView = GetComponent<PhotonView>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!photonView.IsMine) return;
        if (Input.GetKeyDown("b"))
        {
          photonView.RPC("Attack",RpcTarget.AllBuffered);
        }
       
    }
    [PunRPC]
    private void Attack()
    {
        //Play Animation
        playerAnim.SetTrigger("Attack");

        //Detect Enemies in range of Attack
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange,enemyLayers);

        //Damage
        foreach(BoxCollider2D enemy in hitEnemies)
        {
            Debug.Log("Damage");
            enemy.GetComponentInParent<EnemyController>().health--;
           
        }
    }
    
}
