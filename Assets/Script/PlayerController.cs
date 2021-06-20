using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//@TODO ���ó�� �ٵ��(���� �������� ��������� ������°� �ƴ�), �ִϸ��̼� �ٵ��
public class PlayerController : MonoBehaviour
{
    public DataModel.CHARACTER_SKIN skinType;
    public List<string> groundLayers = new List<string>();   //�浹 üũ�� ���̾��

    //�ִϸ��̼� ĳ���Ͱ� ���� ��Ʈ�ѷ�
    public Animator animator;
    public Rigidbody mainRigid;

    //�ý��� ����
    [Range(1, 10)] public int speed = 2;    
    [Range(1, 10)] public int jumpForce = 5; //������ ���� ��
    [Range(0.1f, 5f)] public float airBonTime = 0.2f; //������ ����� �߶� �ð� (�� ���̻� ���ߺ��� ������ ó��)
    public float deathlyForce = 500f;    //500�̻� �浹�� ���� ó��
    float inAirTime;
    public float distanseGround = 0.1f;
    //

    public GameObject charRagdoll;
    public Transform skinRagdollParent;
    public GameObject charAnimation;
    public Transform skinAnimationParent;

    public List<RigidbodyObject> deathCheckJoints = new List<RigidbodyObject>(); //�� ����Ʈ ���� ������ ���� ó�� (���߿� ��¦ ������Ʈ�� ����)

    public bool isGround;
    public bool isAirBon;
    bool isJump;
    bool isDeath;

    //��Ų ���� ���
    DataModel.CHARACTER_SKIN beforeSkin;
    List<int> groundLayerIDs = new List<int>();

    public static PlayerController instance;

    //�⺻ ����
    void Start()
    {
        instance = this;
        if (charRagdoll) charRagdoll.gameObject.SetActive(false);
        if (charAnimation) charAnimation.gameObject.SetActive(true);

        for (int i = 0; i < groundLayers.Count; i++)
            groundLayerIDs.Add(LayerMask.NameToLayer(groundLayers[i]));

        for (int i = 0; i < deathCheckJoints.Count; i++)
        {
            if (deathCheckJoints[i] == null) continue;
               deathCheckJoints[i].onBreakJoint = (GameObject go, float value) =>
            {
                Death();
            };
        }
      
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (DataModel.instance == null) return;
        
        Move();

        if (Input.GetKey(DataModel.instance.jumpKey))
            Jump();

        if (Input.GetKey(DataModel.instance.deathKey))
            Death();

        CheckSkin();

        //��üũ
        CheckGround();

        //������ ����� �ð�
        if (isAirBon) inAirTime += Time.deltaTime;
        else inAirTime = 0;
        
        if (inAirTime > airBonTime)
            ChangeRagdoll();
    }

    //�� üũ
    void CheckGround() {
        float distance = GetDistanceDown();
        isGround = (distance <= distanseGround);
    }

    public void Death() {

        Debug.Log("Death");

        if (isDeath) return;

        isDeath = true;

        ChangeRagdoll();
        DataModel.instance.ReStartGame();
    }

    #region ���� ����
    void ChangeRagdoll() {

        if (charRagdoll.activeInHierarchy) return;  //�̹� ���� ����

        //������ ���� ( ������ Ȥ�� �߶�)
        charRagdoll.SetActive(true);
        charAnimation.SetActive(false);
        CopyTranslate(charAnimation.transform, charRagdoll.transform);
        MainCamera.instance.lookTarget = charRagdoll.transform;
    }
    
    void CopyTranslate(Transform origin, Transform target) {
        for (int i = 0; i < origin.childCount; i++) {

            target.position = origin.position;
            target.rotation = origin.rotation;

            if (target.childCount < i + 1) return;
            CopyTranslate(origin.GetChild(i), target.GetChild(i));
        }
    }
    #endregion

    #region ĳ���� �̵�
    float keyDownTime;
    void Move()
    {
        if (isDeath) return;

        Vector3 moveTranslate = new Vector3();
        keyDownTime += Time.deltaTime;

        if (Input.GetKey(DataModel.instance.leftKey))
            moveTranslate.x -= 1;
        if (Input.GetKey(DataModel.instance.rightKey))
            moveTranslate.x += 1;
        if (Input.GetKey(DataModel.instance.forwardKey))
            moveTranslate.z += 1;
        if (Input.GetKey(DataModel.instance.backKey))
            moveTranslate.z -= 1;
        if ( ! Input.anyKey || moveTranslate.sqrMagnitude < 1)
            keyDownTime = 0f;

        float runSpeed = Mathf.Min(keyDownTime, 1f) * speed;
        transform.Translate(moveTranslate * runSpeed * Time.deltaTime);   //�ӵ� �ϼ� �ð� 1��

        //ȸ��
        if (moveTranslate != Vector3.zero)

            animator.transform.forward = moveTranslate;

        animator.SetFloat("Speed", runSpeed); 
    }

    //����
    float lastJumpTime;
    void Jump()
    {
        if ( ! isGround || isJump || Time.time - lastJumpTime < 0.1f) return;

        lastJumpTime = Time.time;
        animator.SetTrigger("Jump");
        isJump = true;

        if (mainRigid)
            mainRigid.AddForce(Vector3.up * jumpForce * 50 * mainRigid.mass, ForceMode.Force);
    }
    
    RaycastHit downRayHit = new RaycastHit();
    float GetDistanceDown() {
        if (!Physics.Raycast(transform.position + (Vector3.up * 0.2f), Vector3.down, out downRayHit, 100)) return 100;

        //���� �Ʒ� ��°� �ִ� ����
        return Mathf.Abs(transform.position.y - downRayHit.point.y);
    }
    #endregion
    
    //�浹 ó��
    private void OnCollisionEnter(Collision collision)
    {
        if (isDeath) return;
        if (collision != null && collision.transform != null)
        {
            //���� ó��
            isJump = !groundLayerIDs.Contains(collision.transform.gameObject.layer);
            animator.SetBool("isGround", !isJump);
            float impluse = collision.impulse.magnitude;
            if (impluse > deathlyForce)
                Death();

            //Debug.Log("relativeVelocity >>" + collision.relativeVelocity.magnitude + " >> " + collision.gameObject.name);
            Debug.Log("Impulse >> " + collision.impulse.magnitude);

            //��� ������Ʈ�� ������ ���� ó�� (�±׷� ����)
            if (DataModel.instance.deathlyTag.Contains(collision.transform.tag))
                Death();
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        if (collision != null && collision.transform != null)
        {   //�ƹ� ������Ʈ�� �������
            isAirBon = false;
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision != null && collision.transform != null)
        {
            //if(groundLayerIDs.Contains(collision.transform.gameObject.layer))
                //isGround = false;

            isAirBon = true;
            animator.SetBool("isAirBon", true);
        }
    }


    #region �ΰ� ���
    //��Ų ����
    public void ChangeSkin(Transform skinParent, DataModel.CHARACTER_SKIN skin)
    {
        skinType = skin;

        if (skinParent)
        {
            int childCount = skinParent.childCount;
            for (int i = 0; i < childCount; i++)
            {
                Transform child = skinParent.GetChild(i);
                if (child == null) continue;
                child.gameObject.SetActive(child.name == skinType.ToString().ToLower());
            }
        }
    }

    //��Ų ����
    void CheckSkin()
    {
        //��Ų ���� (���߿� ���� �ʿ�)
        if (beforeSkin != skinType)
        {

            ChangeSkin(skinRagdollParent, skinType);
            ChangeSkin(skinAnimationParent, skinType);

            beforeSkin = skinType;
        }

    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Vector3 startPos = transform.position + (Vector3.up * 0.1f);
        Gizmos.DrawLine(startPos, startPos + (Vector3.down * distanseGround));
    }
    #endregion

}
