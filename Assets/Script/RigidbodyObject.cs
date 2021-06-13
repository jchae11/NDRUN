using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RigidbodyObject : MonoBehaviour
{
    public delegate void CallbackBreakJoint(GameObject go, float value);
    public CallbackBreakJoint onBreakJoint;

    private void OnJointBreak(float breakForce)
    {
        if (onBreakJoint != null) onBreakJoint(gameObject, breakForce);
        Debug.Log("breakForce >>" + breakForce);
    }
}
