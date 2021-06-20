using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//게임 옵션등 설정
public class DataModel : MonoBehaviour
{
    public static DataModel instance;

    //@TODO 나중에 커스텀 가능하게
    public KeyCode leftKey = KeyCode.LeftArrow;
    public KeyCode rightKey = KeyCode.RightArrow;
    public KeyCode forwardKey = KeyCode.UpArrow;
    public KeyCode backKey = KeyCode.DownArrow;
    public KeyCode jumpKey = KeyCode.Space;
    public KeyCode deathKey = KeyCode.D;

    public List<string> deathlyTag = new List<string>();
    public Transform originalStartPoint;
    Transform currentStartPoint;     //외부 세이브 포인트에 의해 변경됨
    public Transform endPoint;
    
    public PlayerController playerPrefab;

    Transform ragdollParent; //시체 정리 쉽게 빈 오브젝트 만들어둠-
    
    public void Awake()
    {
        instance = this;
        ragdollParent = new GameObject("ragDolls").transform;
    }

    private void Start()
    {
        currentStartPoint = originalStartPoint;
        ReStartGame(0.5f);
    }

    public enum CHARACTER_SKIN {
        man_actionhero = 1,
        man_astronaut,
        man_basketball_player,
        man_boxer,
        man_business,
        man_skeleton
    }

    public void ReStartGame(float delay = 3f) {

        CancelInvoke("SpawnNewCharactor");
        Invoke("SpawnNewCharactor", delay);
    }

    void SpawnNewCharactor()
    {
        if (PlayerController.instance != null && PlayerController.instance.gameObject != null) {
            //기존 캐릭터가 있음
            //랙돌(시체)을 제외한 제거 실행
            PlayerController.instance.charRagdoll.transform.SetParent(ragdollParent, true);
            DestroyImmediate(PlayerController.instance.gameObject);
        }

        //새로 캐릭터 생성
        GameObject.Instantiate(playerPrefab, currentStartPoint.position, Quaternion.identity);
    }

    public void SaveStartPoint(Transform savePoint) {
        currentStartPoint = savePoint;
    }

    public void FinishGame(Transform endPoint) {
        if (this.endPoint == endPoint)
        {
            Debug.Log("FINISH GAME !!");
            currentStartPoint = originalStartPoint; //초기화
        }
    }
}
