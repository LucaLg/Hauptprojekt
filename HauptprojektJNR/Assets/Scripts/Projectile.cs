using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Projectile : MonoBehaviour
{
    //Components
    private Rigidbody2D rb;
    public float speed = 10f;
    public float destroyTime = 6f;
    public float damage = 2;
    private bool hasForce = false;
    private PhotonView photonView;
    private bool lookRight = false;
    private Vector3 playerTarget;
    private Transform firePoint;
    // Start is called before the first frame update
    void Start()
    {
        firePoint = GameObject.FindGameObjectWithTag("FirePoint").transform;
        photonView = GetComponent<PhotonView>();
        rb = GetComponent<Rigidbody2D>();
        //StartCoroutine(destroyProjectile());
        
    }

    // Update is called once per frame
    void Update()
    {
        /* if (!hasForce)
         {
             photonView.RPC("AddForce", RpcTarget.AllBuffered);
         }*/
        float angle = Mathf.Atan2(rb.velocity.y, rb.velocity.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        
        if(collision.tag =="Player")
        {
            Debug.LogError("PlayerHit");
            
            collision.GetComponentInParent<PlayerController>().IsAttacked(damage);
            photonView.RPC("DestroyArrow", RpcTarget.AllBuffered);
            
        }
        else if(collision.tag !=  "Archer")
        {
            Debug.LogError("Alles Hit");
            photonView.RPC("DestroyArrow", RpcTarget.AllBuffered);
        }
        
    }
    [PunRPC]
    private void DestroyArrow()
    {
        Destroy(this.gameObject);
    }
    IEnumerator destroyProjectile()
    {
        yield return new WaitForSeconds(destroyTime);
        Destroy(this.gameObject);
    }
    [PunRPC]
    private void AddForce()
    {
        //rb.velocity = new Vector2(speed, firePoint.rotation);
        rb.AddForce(playerTarget * speed);
        //rb.AddForce(new Vector2(playerTarget.x*speed, 7));
        hasForce = true;
    }
    public void SetTarget(Vector3 player)
    {
        playerTarget = player;
        playerTarget = new Vector3(playerTarget.x, playerTarget.y + 5, playerTarget.z+5);
        
        
    }
    [PunRPC]
    private void FlipTrue()
    {
        transform.localScale = new Vector3(-1, 1, 1);
        lookRight = false;
        
    }
    [PunRPC]
    private void FlipFalse()
    {

        transform.localScale = new Vector3(1, 1, 1);
        lookRight = true;

    }
}
