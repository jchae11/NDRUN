using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrapRotate : TrapObject
{
    public enum ROTATE_AXIS { X, Y, Z}
    public ROTATE_AXIS axisType;
    public iTween.LoopType loopType = iTween.LoopType.loop;
    public float speed = 10f;
    public float rotateValue = 360f;
    public float delay = 3;
    
    public void Start()
    {
        type = TRIGGER_TYPE.ROTATE;

        if( ! isOnceTrigger)
            iTween.RotateBy(gameObject, iTween.Hash(axisType.ToString().ToLower(),
                rotateValue, "easeType", iTween.EaseType.linear, "loopType", loopType, "speed", speed, "delay", delay));
    }

    public override void ActionEnterTrigger()
    {
        iTween.RotateBy(gameObject, iTween.Hash(axisType.ToString().ToLower(),
                rotateValue, "easeType", iTween.EaseType.linear, "loopType", loopType, "speed", speed, "delay", delay));
        actioned = true;
    }
}
