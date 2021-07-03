using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//@TODO 에어본처리 다듬기(현재 장착물도 닿아있으면 에어본상태가 아님), 애니메이션 다듬기
public class PlayerController : MonoBehaviour
{
    public DataModel.CHARACTER_SKIN skinType;
    public List<string> groundLayers = new List<string>();   //충돌 체크할 레이어들

    //애니메이션 캐릭터가 메인 컨트롤러
    public Animator animator;
    public Rigidbody mainRigid;

    //시스템 변수
    [Range(1, 10)] public int speed = 2;    
    [Range(1, 10)] public int jumpForce = 5; //점프시 가할 힘
    [Range(0.1f, 5f)] public float airBonTime = 0.2f; //랙돌로 변경될 추락 시간 (몇 초이상 공중부터 데미지 처리)
    public float deathlyForce = 500f;    //500이상 충돌은 죽음 처리
    float inAirTime;
    public float distanseGround = 0.1f;
    float hp;
    public float maxHP = 500; //== deathlyForce
    //

    public GameObject charRagdoll;
    public Transform skinRagdollParent;
    public GameObject charAnimation;
    public Transform skinAnimationParent;

    public List<RigidbodyObject> deathCheckJoints = new List<RigidbodyObject>(); //이 조인트 값이 깨지면 죽음 처리 (나중에 짐짝 오브젝트로 변경)

    public bool isGround;
    public bool isAirBon;
    bool isJump;
    bool isDeath;

    //스킨 변경 기능
    DataModel.CHARACTER_SKIN beforeSkin;
    List<int> groundLayerIDs = new List<int>();

    public static PlayerController instance;

    //기본 세팅
    void Start()
    {
        maxHP = deathlyForce;
        hp = maxHP;

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

        if ( ! isDeath && hp < maxHP)
            hp += (Time.deltaTime * maxHP * 0.12f);   //hp 자동 회복 (5초 풀 회복)

        Move();

        if (Input.GetKey(DataModel.instance.jumpKey))
            Jump();

        if (Input.GetKey(DataModel.instance.deathKey))
            Death();

        CheckSkin();

        //땅체크
        CheckGround();

        //랙돌로 변경될 시간
        if (isAirBon) inAirTime += Time.deltaTime;
        else inAirTime = 0;
        
        if (inAirTime > airBonTime)
            ChangeRagdoll();
    }

    //땅 체크
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

    #region 랙돌 변경
    void ChangeRagdoll() {

        if (charRagdoll.activeInHierarchy) return;  //이미 랙돌 상태

        //랙돌로 변경 ( 데미지 혹은 추락)
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

    #region 캐릭터 이동
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
        float magitude = (moveTranslate.sqrMagnitude > 0) ? moveTranslate.sqrMagnitude : 1;
        transform.Translate((moveTranslate * runSpeed * Time.deltaTime) / magitude);   //속도 완성 시간 1초

        //회전
        if (moveTranslate != Vector3.zero)
            animator.transform.forward = moveTranslate;

        animator.SetFloat("Speed", runSpeed); 
    }

    //점프
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

        //무언가 아래 닿는게 있는 상태
        return Mathf.Abs(transform.position.y - downRayHit.point.y);
    }
    #endregion
    
    //충돌 처리
    private void OnCollisionEnter(Collision collision)
    {
        if (isDeath) return;

        if (collision != null && collision.transform != null)
        {
            //랜딩 처리
            isJump = !groundLayerIDs.Contains(collision.transform.gameObject.layer);
            animator.SetBool("isGround", !isJump);

            float impluse = collision.impulse.magnitude;
            TrapObject trap = collision.transform.GetComponent<TrapObject>();

            if (trap)
            {
                if (trap.type == TrapObject.TRIGGER_TYPE.DAMAGE)    //데미지 체크 트랩이다
                    hp -= impluse;
                else if (trap.type == TrapObject.TRIGGER_TYPE.DEATHLY)
                    Death();

                if (hp < 0)
                    Death();
            }
            else if (impluse > deathlyForce)    //데미지 오브젝트가 아니더라도 우선 큰 충격에 죽음처리 
            {
                Debug.Log("Impulse >> " + collision.impulse.magnitude);
                Death();
            }
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        if (isDeath) return;
        if (collision != null && collision.transform != null)
        {   //아무 오브젝트에 닿아있음
            isAirBon = false;
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (isDeath) return;
        if (collision != null && collision.transform != null)
        {
            //if(groundLayerIDs.Contains(collision.transform.gameObject.layer))
                //isGround = false;

            isAirBon = true;
            animator.SetBool("isAirBon", true);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (isDeath) return;

        TrapObject trap = other.GetComponent<TrapObject>();
        if (trap.type == TrapObject.TRIGGER_TYPE.DEATHLY)
            Death();
    }

    #region 부가 기능
    //스킨 변경
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

    //스킨 변경
    void CheckSkin()
    {
        //스킨 변경 (나중에 정리 필요)
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
