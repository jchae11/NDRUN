using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//장애물
[RequireComponent(typeof(Collider))]
public class TrapObject : MonoBehaviour
{
    public enum TRIGGER_TYPE { NONE, SAVE_POINT, END_POINT, DEATHLY, GRAVITY, DESTROY, SPAWN, ROTATE, ECT }
    public TRIGGER_TYPE type;   // NONE, DEATHLY, GRAVITY 는 오버라이드 클래스 필요없음

    public bool isOnceTrigger;  //한번만 체크하는 트리거인가
    [HideInInspector]
    public bool actioned;

    float DestroyY = -100;   //해당 y축으로 넘어가면 삭제

    private void Start()
    {
        if (type == TRIGGER_TYPE.GRAVITY || type == TRIGGER_TYPE.DESTROY || type == TRIGGER_TYPE.SAVE_POINT || type == TRIGGER_TYPE.END_POINT)
            isOnceTrigger = true;
        if (type == TRIGGER_TYPE.DEATHLY)
            tag = DataModel.instance.deathlyTag.Count > 0 ? DataModel.instance.deathlyTag[0] : "";
    }

    private void FixedUpdate()
    {
        if (transform.position.y < DestroyY)
            DestroyImmediate(gameObject);
    }

    public void OnCollisionStay(Collision collision)
    {
    }

    //triger 체크된 콜라이더는 여기로 이벤트
    private void OnTriggerEnter(Collider other)
    {
        if (other == null || (actioned && isOnceTrigger)) return;
        
        //@TODO 캐릭터 인지 체크

        ActionEnterTrigger();
    }

    //triger 체크안된 콜라이더는 여기로 이벤트
    void OnCollisionEnter(Collision collision)
    {
        if (collision == null || (actioned && isOnceTrigger)) return;

        //@TODO 캐릭터 인지 체크

        ActionEnterTrigger();
    }

    public virtual void ActionEnterTrigger()
    {
        //기본 타입별 액션
        if (type == TRIGGER_TYPE.GRAVITY)
        {
            Rigidbody rigidBody = GetComponent<Rigidbody>();
            if (rigidBody != null)
            {
                rigidBody.useGravity = true;
                rigidBody.isKinematic = false;
                if (isOnceTrigger) actioned = true;
            }
        }
        //세이브 포인트에 닿으면 시작지점 변경해줌
        if (type == TRIGGER_TYPE.SAVE_POINT)
        {
            DataModel.instance.SaveStartPoint(transform);
            actioned = true;
        }
        //엔드 포인트에 닿으면 게임 엔딩
        if (type == TRIGGER_TYPE.SAVE_POINT)
        {
            DataModel.instance.FinishGame(transform);
            actioned = true;
        }

    }
}
