using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

// 플레이어 탱크가 여러개 일때
// 아파치는 플레이어 탱크 중 가장 가까운 거리에 있는 탱크를 탐색해서
// 공격하는 로직
public class ApacheAI : MonoBehaviourPun, IPunObservable
{
    private readonly string tankTag = "TANK";
    public enum AppacheState { PATROL, ATTACK, DESTROY }
    public AppacheState state = AppacheState.PATROL;

    public List<Transform> patrolList;
    float rotSpeed = 15f, moveSpeed = 10f;
    Transform myTr;

    int currentPatorlIdx = 0;
    float wayCheck = 7f;
    //public bool isSearch = true;
    private float attackTime = 0f;
    private float attackRemiming = 2f;

    private ApacheAI_Attack attak;

    // 플레이어 탱크를 담을 배열
    [SerializeField] private GameObject[] playerTanks = null;
    Transform closetTank;

    Vector3 newWorkPosition = Vector3.zero;
    Quaternion newWorkRotation = Quaternion.identity;

    void Start()
    {
        photonView.Synchronization = ViewSynchronization.Unreliable;
        // 통신 유형은 UDP 방식
        photonView.ObservedComponents[0] = this;

        var pObj = GameObject.Find("Points");
        if (pObj != null)
            pObj.GetComponentsInChildren<Transform>(patrolList);

        patrolList.RemoveAt(0);

        myTr = transform;

        attak = GetComponent<ApacheAI_Attack>();
        
        newWorkPosition = myTr.position;
        newWorkRotation = myTr.rotation;

        if (PhotonNetwork.IsMasterClient)
            InvokeRepeating("UpdateTankList", 0f, 0.5f);
    }

    void FixedUpdate()
    {
        if (PhotonNetwork.IsMasterClient) // 마스터 클라이언트 방장 호스트
        {
            switch(state)
            {
                case AppacheState.PATROL:
                    WayPatrol();
                    break;
                case AppacheState.ATTACK:
                    Attack();
                    break;
            }
        }
        else // 다른 클라이언트는 네트워크로 받은 위치로 부드럽게 이동
        {
            myTr.position = Vector3.Lerp(myTr.position, newWorkPosition, Time.fixedDeltaTime * moveSpeed);
            myTr.rotation = Quaternion.Slerp(myTr.rotation, newWorkRotation, Time.fixedDeltaTime * rotSpeed);
        }
    }
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting) // 로컬인 자신의 이동과 회전은 송신
        {
            stream.SendNext(myTr.position);
            stream.SendNext(myTr.rotation);
            stream.SendNext(state); // 상태정보 동기화 하면 좋음
        }
        else // 리모트
        {
            newWorkPosition = (Vector3)stream.ReceiveNext();
            newWorkRotation = (Quaternion)stream.ReceiveNext();
            state = (AppacheState)stream.ReceiveNext();
        }
    }

    void Update()
    {
        if (!PhotonNetwork.IsMasterClient) return;
  
        CheckP();
    }

    void WayPatrol()
    {
        state = AppacheState.PATROL;
        Vector3 movePos = patrolList[currentPatorlIdx].position - myTr.position;

        myTr.rotation = Quaternion.Slerp(myTr.rotation, Quaternion.LookRotation(movePos), Time.fixedDeltaTime * rotSpeed);
        myTr.Translate(Vector3.forward * moveSpeed * Time.fixedDeltaTime);
        //Search();
        if (closetTank != null && Vector3.Distance(closetTank.position, myTr.position) < 80f)
        {
            state = AppacheState.ATTACK;
        }

    }
    void Search()
    {
        #region 탱크가 하나일 때 싱글게임일 경우의 로직
        //float tankFindDist = (GameObject.FindWithTag(tankTag).transform.position - myTr.transform.position).sqrMagnitude;
        //// Distance로 변경하는게 좋음, 자원을 많이 먹기 때문에
        //if (tankFindDist <= 80f * 80f)

        //if (!GameObject.FindWithTag(tankTag)) return;
        //if (Vector3.Distance(GameObject.FindWithTag(tankTag).transform.position, myTr.transform.position) < 80f)
        //    isSearch = false;
        //else
        //    isSearch = true;
        #endregion

        //Transform target = FindClossetTank();
         // 스타트 코루틴으로 해도 됨
        if (closetTank != null && Vector3.Distance(closetTank.position, myTr.position) < 80f)
        {
            state = AppacheState.ATTACK;
        }

    }

    void UpdateTankList()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            closetTank = FindClossetTank();
        }
    }
    private Transform FindClossetTank()
    {
        playerTanks = GameObject.FindGameObjectsWithTag(tankTag);

        if (playerTanks == null || playerTanks.Length == 0)
            return null;

        Transform target = null;
        float clossetDistSqr = Mathf.Infinity;

        foreach (GameObject _tank in playerTanks)
        {
            if (!_tank.activeInHierarchy) continue;

            float distSqr = (_tank.transform.position - myTr.position).sqrMagnitude;
            if (distSqr < clossetDistSqr)
            {
                clossetDistSqr = distSqr;
                target = _tank.transform;
            }
        }
        return target;
    }

    void CheckP()
    {
        if (Vector3.Distance(transform.position, patrolList[currentPatorlIdx].position) <= 5f)
        {
            //if (currentPatorlIdx == patrolList.Count - 1)
            //    currentPatorlIdx = 0;
            //else
            //    currentPatorlIdx++;
            currentPatorlIdx = Random.Range(0, patrolList.Count);
        }
    }

    void Attack()
    {
        if (playerTanks == null || playerTanks.Length == 0 || closetTank == null)
        {
            state = AppacheState.PATROL;
            return;
        }
        state = AppacheState.ATTACK;
        //Transform target = FindClossetTank();
        Vector3 _normal = (closetTank.position - myTr.position).normalized;
        myTr.rotation = Quaternion.Slerp(myTr.rotation, Quaternion.LookRotation(_normal), Time.fixedDeltaTime * rotSpeed);
        if (Time.time - attackTime >= attackRemiming)
        {
            attak.Fire(attak.firePosL, attak.leaserBeamL);
            attak.Fire(attak.firePosR, attak.leaserBeamR);
            attackTime = Time.time;
        }

        if (Vector3.Distance(closetTank.position, myTr.position) > 80f)
        {
            state = AppacheState.PATROL;
        }
    }

    
}
