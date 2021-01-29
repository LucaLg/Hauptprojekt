using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Projectile : MonoBehaviour
{
    //Components
    private Rigidbody2D rb;
    public float destroyTime = 6f;
    public float damage = 0.5f;
    private PhotonView photonView;
    private bool lookRight = false;
    private Vector3 playerTarget;
    // Start is called before the first frame update
    void Start()
    {
        photonView = GetComponent<PhotonView>();
        rb = GetComponent<Rigidbody2D>();

    }

    // Update is called once per frame
    void Update()
    {

        float angle = Mathf.Atan2(rb.velocity.y, rb.velocity.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        
        if (collision.tag == "Player")
        {
            Debug.LogError("PlayerHit");

            collision.GetComponentInParent<PlayerController>().IsAttacked(damage);
            photonView.RPC("DestroyArrow", RpcTarget.AllBuffered);

        }
        if(collision.tag == "Boden")
        {
            photonView.RPC("DestroyArrow", RpcTarget.AllBuffered);
        }
        else if (collision.tag != "Archer")
        {
            StartCoroutine(destroyProjectile());
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
