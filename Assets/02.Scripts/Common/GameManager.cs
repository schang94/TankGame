using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Photon.Realtime;

public class GameManager : MonoBehaviourPunCallbacks
{
    private readonly string apacheTag = "APACHE";

    public static GameManager Instance = null;
    public bool isGameOver = false;
    public Text txtConnect;
    public Text txtLogMsg;

    // ��ũó�� Resource �̿��ؼ� �ص���
    [SerializeField] List<Transform> spawnPoints;
    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else if (Instance != this)
            Destroy(gameObject);
        // ���ܳ�Ʈ��ũ ������ ���� ���� �޽����� �޴´�.
        PhotonNetwork.IsMessageQueueRunning = true;
        CreateTank();

        var spawnPoint = GameObject.Find("SpawnPoints").transform;
        if (spawnPoint != null)
            spawnPoint.GetComponentsInChildren<Transform>(spawnPoints);

        spawnPoints.RemoveAt(0);
        
    }

    private void Start()
    {
        string msg = "\n<color=#00FF00>["+PhotonNetwork.NickName+"] Conneted</color>";
        photonView.RPC("LogMag", RpcTarget.AllBuffered, msg);
        if (spawnPoints.Count > 0 && PhotonNetwork.IsMasterClient)
            InvokeRepeating("CreateApache", 0.02f, 3.0f);
    }
    void CreateTank()
    {
        float pos = Random.Range(-100f, 100f);
        PhotonNetwork.Instantiate("Tank", new Vector3(pos, 3, pos), Quaternion.identity, 0, null);
        // Resource�� �ִ� Tank �����±�, ���� �޾ƿ� �������� �������� �ְ� name�� ������ ��
    }
    void CreateApache()
    {
        if (isGameOver) return;
        int count = (int)GameObject.FindGameObjectsWithTag(apacheTag).Length;
        if (count < 10)
        {
            int idx =Random.Range(0, spawnPoints.Count);
            PhotonNetwork.InstantiateRoomObject("Apache", spawnPoints[idx].position, spawnPoints[idx].rotation, 0, null);
            // PhotonNetwork.Instantiate �ϸ� ���� ȣ��Ʈ�� ������ ���� ���� ����
            // �׷��� ����� �پ� �־����
        }
    }
    [PunRPC]
    public void ApplyPlayerCountUpdate()
    {
        print("ApplyPlayerCountUpdate");
        Room currentRoom = PhotonNetwork.CurrentRoom;
        txtConnect.text = $"{currentRoom.PlayerCount:00} / {currentRoom.MaxPlayers:00}";
    }

    [PunRPC]
    public void GetConnectPlayerCount()
    {
        if (PhotonNetwork.IsMasterClient) // ����
        {
            print("GetConnectPlayerCount");
            photonView.RPC("ApplyPlayerCountUpdate", RpcTarget.All);
        }
    }

    public override void OnPlayerEnteredRoom(Player newPlayer) // ������ �������� ��
    {
        GetConnectPlayerCount();
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        GetConnectPlayerCount();
    }

    public void OnClickExitRoom()
    {
        string msg = "\n<color=#FF0000>[" + PhotonNetwork.NickName + "] DisConneted</color>";
        photonView.RPC("LogMag", RpcTarget.AllBuffered, msg);
        PhotonNetwork.LeaveRoom(); // ���� ��������(������ ��� ��Ʈ��ũ ����)
    }
    public override void OnLeftRoom() // �뿡�� ������ ����Ǿ��� �� ȣ��Ǵ� �ݹ� �Լ�
    {
        SceneManager.LoadScene("LobbyScene");
    }

    [PunRPC]
    public void LogMag(string msg)
    {
        txtLogMsg.text += msg;
    }
}
