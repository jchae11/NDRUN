using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainCamera : MonoBehaviour
{
    public static MainCamera instance;
    public Transform lookTarget;

    private void Awake()
    {
        instance = this;
    }

    private void FixedUpdate()
    {
        if (lookTarget)
        {
            transform.LookAt(lookTarget);
        }
    }

    private void OnDrawGizmos()
    {
        if (lookTarget)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(transform.position, lookTarget.transform.position);
        }
    }
}
