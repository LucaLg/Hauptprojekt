using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovePlayer : Bolt.EntityBehaviour<ICustomSpielerState>
{
    private Vector3 movement;
    public float moveSpeed = 1f;
    private Rigidbody2D rb;
    //void Start()
   public override void Attached(){
        state.SetTransforms(state.spielerTransform, gameObject.transform);
        rb = gameObject.GetComponent<Rigidbody2D>();

   }
   public override void SimulateOwner(){
        movement.x = Input.GetAxisRaw("Horizontal");
        if(movement != Vector3.zero)
        {
            transform.position = transform.position + (movement.normalized * moveSpeed * BoltNetwork.FrameDeltaTime);
        }
       //rb.MovePosition(rb.position + movement * moveSpeed * Time.fixedDeltaTime);
    }
}
