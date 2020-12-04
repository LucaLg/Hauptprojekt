using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Bolt;

public class NetworkCallbacks : GlobalEventListener
{
    public GameObject playerBody;
    public override void SceneLoadLocalDone(string scene)
    {
        Vector3 spawnPos = new Vector3(0, -1.666407f, 0);
        BoltNetwork.Instantiate(playerBody, spawnPos, Quaternion.identity);
        
    }
}
