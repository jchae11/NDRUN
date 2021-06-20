using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//���� �ɼǵ� ����
public class DataModel : MonoBehaviour
{
    public static DataModel instance;

    //@TODO ���߿� Ŀ���� �����ϰ�
    public KeyCode leftKey = KeyCode.LeftArrow;
    public KeyCode rightKey = KeyCode.RightArrow;
    public KeyCode forwardKey = KeyCode.UpArrow;
    public KeyCode backKey = KeyCode.DownArrow;
    public KeyCode jumpKey = KeyCode.Space;
    public KeyCode deathKey = KeyCode.D;

    public List<string> deathlyTag = new List<string>();
    public Transform originalStartPoint;
    Transform currentStartPoint;     //�ܺ� ���̺� ����Ʈ�� ���� �����
    public Transform endPoint;
    
    public PlayerController playerPrefab;

    Transform ragdollParent; //��ü ���� ���� �� ������Ʈ ������-
    
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
            //���� ĳ���Ͱ� ����
            //����(��ü)�� ������ ���� ����
            PlayerController.instance.charRagdoll.transform.SetParent(ragdollParent, true);
            DestroyImmediate(PlayerController.instance.gameObject);
        }

        //���� ĳ���� ����
        GameObject.Instantiate(playerPrefab, currentStartPoint.position, Quaternion.identity);
    }

    public void SaveStartPoint(Transform savePoint) {
        currentStartPoint = savePoint;
    }

    public void FinishGame(Transform endPoint) {
        if (this.endPoint == endPoint)
        {
            Debug.Log("FINISH GAME !!");
            currentStartPoint = originalStartPoint; //�ʱ�ȭ
        }
    }
}
