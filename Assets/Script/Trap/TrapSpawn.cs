using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrapSpawn : TrapObject
{
    public GameObject spawnPrefab;
    public Transform spawnPosition;

    Collider collider;

    public void Awake()
    {
        type = TRIGGER_TYPE.SPAWN;
        collider = GetComponent<Collider>();
    }

    public override void ActionEnterTrigger()
    {
        if (spawnPrefab == null || spawnPosition == null) return;
        GameObject go = GameObject.Instantiate(spawnPrefab, spawnPosition);
        go.transform.localPosition = Vector3.zero;
        actioned = true;

        if (collider) collider.isTrigger = true;
        
    }
}
