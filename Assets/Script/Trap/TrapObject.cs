using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//��ֹ�
[RequireComponent(typeof(Collider))]
public class TrapObject : MonoBehaviour
{
    public enum TRIGGER_TYPE { NONE, SAVE_POINT, END_POINT, DEATHLY, GRAVITY, DESTROY, SPAWN, ROTATE, ECT }
    public TRIGGER_TYPE type;   // NONE, DEATHLY, GRAVITY �� �������̵� Ŭ���� �ʿ����

    public bool isOnceTrigger;  //�ѹ��� üũ�ϴ� Ʈ�����ΰ�
    [HideInInspector]
    public bool actioned;

    float DestroyY = -100;   //�ش� y������ �Ѿ�� ����

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

    //triger üũ�� �ݶ��̴��� ����� �̺�Ʈ
    private void OnTriggerEnter(Collider other)
    {
        if (other == null || (actioned && isOnceTrigger)) return;
        
        //@TODO ĳ���� ���� üũ

        ActionEnterTrigger();
    }

    //triger üũ�ȵ� �ݶ��̴��� ����� �̺�Ʈ
    void OnCollisionEnter(Collision collision)
    {
        if (collision == null || (actioned && isOnceTrigger)) return;

        //@TODO ĳ���� ���� üũ

        ActionEnterTrigger();
    }

    public virtual void ActionEnterTrigger()
    {
        //�⺻ Ÿ�Ժ� �׼�
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
        //���̺� ����Ʈ�� ������ �������� ��������
        if (type == TRIGGER_TYPE.SAVE_POINT)
        {
            DataModel.instance.SaveStartPoint(transform);
            actioned = true;
        }
        //���� ����Ʈ�� ������ ���� ����
        if (type == TRIGGER_TYPE.SAVE_POINT)
        {
            DataModel.instance.FinishGame(transform);
            actioned = true;
        }

    }
}
