using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class EnemyController : MonoBehaviour,IPunObservable
{
    public int health = 4;
    private PhotonView photonView;
    bool isDead = false;
    // Start is called before the first frame update
    void Start()
    {
        photonView = GetComponent<PhotonView>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!photonView.IsMine)
        {
            return;
        }
        photonView.RPC("Die", RpcTarget.All);
        if (isDead)
        {
            if (photonView.IsMine) { 
            PhotonNetwork.Destroy(gameObject);
            }
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

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        
        if (stream.IsWriting)
        {
            stream.SendNext(health);
        }else if (stream.IsReading)
        {
            health = (int)stream.ReceiveNext();
        }
    }
}
