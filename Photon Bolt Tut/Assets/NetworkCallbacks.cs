using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Bolt;

public class NetworkCallbacks : GlobalEventListener
{
    public GameObject body;
    public override void SceneLoadLocalDone(string scene)
    {
        Vector3 spawnPos = new Vector3(0, 0, 0);
        BoltNetwork.Instantiate(body, spawnPos, Quaternion.identity);
    }
}
