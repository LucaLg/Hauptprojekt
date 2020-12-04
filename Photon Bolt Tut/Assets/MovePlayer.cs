using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovePlayer : Bolt.EntityBehaviour<ICustomSpielerState>
{
    private Vector2 movement;
    //void Start()
   public override void Attached(){
      state.SetTransforms(state.spielerTransform,gameObject.transform);
   }
   public override void SimulateOwner(){
       movement.y = Input.GetAxisRaw("Vertical");
   }
}
