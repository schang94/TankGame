using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PhotonInit : MonoBehaviourPunCallbacks // Pun ��Ʈ��ũ ���� ���̺귯��
{
    public string Version = "V1.1.0";
    public InputField userId;
    public GameObject roomItem;
    public Transform scrollContents;
    public InputField roomName;
    private Dictionary<string, GameObject> rooms = new Dictionary<string, GameObject>();

    void Awake()
    {
        if (!PhotonNetwork.IsConnected)
        {
            PhotonNetwork.GameVersion = Version;
            PhotonNetwork.ConnectUsingSettings();
            // ���� ��Ʈ��ũ���� ����
            roomName.text = $"Room_{Random.Range(0, 999):000}";
        }
        
    }

    public override void OnConnectedToMaster()
    {
        base.OnConnectedToMaster();
        Debug.Log($"������ Ŭ���̾�Ʈ ����");
        PhotonNetwork.JoinLobby();
    }

    public override void OnJoinedLobby()
    {
        base.OnJoinedLobby();
        Debug.Log("�κ� ����");
        //PhotonNetwork.JoinRandomRoom(); // �������� �濡 ����
        userId.text = GetUserID();
    }
    string GetUserID()
    {
        string userId = PlayerPrefs.GetString("USER_ID");
        if (string.IsNullOrEmpty(userId))
        {
            userId = $"USER_ {Random.Range(0, 999):000}";
        }

        return userId;
    }
    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        base.OnJoinRandomFailed(returnCode, message);
        Debug.Log("�� ���� ����");
        PhotonNetwork.CreateRoom("TankBattleFiledRoom", new RoomOptions { IsOpen = true, IsVisible = true, MaxPlayers = 20});
    }

    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();
        Debug.Log("���������� �� ����!");
        //CreateTank();
        StartCoroutine(LoadBattleFiled()); // �ٸ� ������ �̵��ϱ� ���� ��ŸƮ �ڷ�ƾ ����
    }

    IEnumerator LoadBattleFiled()
    {
        // �� �̵��ϴ� ���� ���� Ŭ���� ������ ���� ��Ʈ��ũ �޽��� ���� �ߴ�
        PhotonNetwork.IsMessageQueueRunning = false;
        // �񵿱������� ���ε�
        AsyncOperation ao = SceneManager.LoadSceneAsync("BattleFieldScene");

        yield return ao; // �񵿱������� ����
    }

    public void OnClickJoinRandomRoom()
    {
        PhotonNetwork.NickName = userId.text;
        PlayerPrefs.SetString("USER_ID", userId.text);
        PhotonNetwork.JoinRandomRoom();
    }

    // ���� ���� �ǰų� ������ �ڵ����� ȣ��Ǵ� �ݹ� �Լ�
    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        //foreach (GameObject obj in GameObject.FindGameObjectsWithTag("ROOMITEM"))
        //{
        //    // ���� �� ������ ���� �ٽ� ���� �Ѵ� ó�� ����
        //    Destroy(obj);
        //}

        // 1. ���� ���� �� ����Ʈ���� ���� ��ȿ�� �� �̸��鸸 ���� (���� ��ȸ�� ���� HashSet ���)
        HashSet<string> activeRoomNames = new HashSet<string>();
        foreach (RoomInfo roomInfo in roomList)
        {
            // ��Ȱ��ȭ, ���� �� ���� ��Ͽ��� ���� (PUN �⺻ ����)
            if (!roomInfo.RemovedFromList)
            {
                activeRoomNames.Add(roomInfo.Name);
            }
        }
        // 2. ���� UI�� ǥ�� ���� ��(ĳ��) �߿��� ������� �� ���� ã�� ���� (�� �κ��� '���� �� ����' ����)
        List<string> roomsToDelete = new List<string>();
        foreach (string cachedRoomName in rooms.Keys)
        {
            if (!activeRoomNames.Contains(cachedRoomName))
            {
                roomsToDelete.Add(cachedRoomName);
            }
        }
        foreach (string roomNameToDelete in roomsToDelete)
        {
            // �ش� GameObject �ı�
            Destroy(rooms[roomNameToDelete]);
            // ĳ�ÿ��� ����
            rooms.Remove(roomNameToDelete);
        }
        // 3. ���� ���� �� ����Ʈ�� �������� UI ������Ʈ �� ����
        foreach (RoomInfo roomInfo in roomList)
        {
            // �̹� ���� ó���� ���̸� �ǳʶٱ�
            if (roomInfo.RemovedFromList)
            {
                continue;
            }

            GameObject roomItemObject;
            // 3-1. �̹� �����ϴ� ���̸� ������ ������Ʈ
            if (rooms.TryGetValue(roomInfo.Name, out roomItemObject))
            {
                RoomData roomData = roomItemObject.GetComponent<RoomData>();
                roomData.roomName = roomInfo.Name;
                roomData.connectPlayer = roomInfo.PlayerCount;
                roomData.maxPlayer = roomInfo.MaxPlayers;
                roomData.DisPlayerRoomData(); // UI �ؽ�Ʈ ������Ʈ
            }
            // 3-2. ���� ���� ���̸� ����
            else
            {
                GameObject newRoom = Instantiate(roomItem, scrollContents);

                RoomData roomData = newRoom.GetComponent<RoomData>();
                roomData.roomName = roomInfo.Name;
                roomData.connectPlayer = roomInfo.PlayerCount;
                roomData.maxPlayer = roomInfo.MaxPlayers;
                roomData.DisPlayerRoomData();

                roomData.GetComponent<Button>().onClick.AddListener(() =>
                {
                    OnClickRoomItem(roomData.roomName);
                });

                // ���� ������ ���� ĳ�ÿ� �߰�
                rooms.Add(roomInfo.Name, newRoom);
            }
        }
        #region
        // ������ RoomItem �������� ���� �� �ӽ� ����
        //GameObject tempRoom = null;
        //foreach (RoomInfo roomInfo in roomList)
        //{
        //    if (roomInfo.RemovedFromList == true)
        //    {
        //        // ��ųʸ����� �� �̸����� �˻��� ����� RoomItem �������� ����
        //        rooms.TryGetValue(roomInfo.Name, out tempRoom);

        //        // RoomItem ����
        //        Destroy(tempRoom);
        //        rooms.Remove(roomInfo.Name);
        //    }
        //    else // �� ������ ���� �� ���
        //    {
        //        RoomData roomData = null;
        //        if (!rooms.ContainsKey(roomInfo.Name))
        //        {
        //            GameObject roomPrefab = Instantiate(roomItem, scrollContents);
        //            roomData = roomPrefab.GetComponent<RoomData>();

        //            // RoomItem Button ������Ʈ�� Ŭ�� �̺�Ʈ�� �������� ����
        //            // �̰��� ���� �̺�Ʈ �����ʶ�� �Ѵ�.
        //            roomData.GetComponent<Button>().onClick.AddListener(delegate
        //            { OnClickRoomItem(roomData.roomName); });

        //            rooms.Add(roomInfo.Name, roomPrefab);
        //        }
        //        else // �� �̸��� ��ųʸ��� ���� ��쿡 �� ������ ������Ʈ
        //        {
        //            rooms.TryGetValue(roomInfo.Name, out tempRoom);
        //            roomData = tempRoom.GetComponent<RoomData>();
        //        }
        //        roomData.roomName = roomInfo.Name;
        //        roomData.connectPlayer = roomInfo.PlayerCount;
        //        roomData.maxPlayer = roomInfo.MaxPlayers;

        //        roomData.DisPlayerRoomData();
        //        // �� �̸��� ��ųʸ��� ���� ��� ���� �߰�
        //        print("�� ������ ����");
        //    }
        //}
        #endregion

    }
    void OnClickRoomItem(string roomName)
    {
        PhotonNetwork.NickName = userId.text;
        PlayerPrefs.SetString("USER_ID", userId.text);
        PhotonNetwork.JoinRoom(roomName);
    }
    public void OnClickCreateRoom()
    {
        string _roomName = roomName.text;
        if (string.IsNullOrEmpty(_roomName))
        {
            _roomName = $"ROOM_{Random.Range(0, 999):000}";
        }

        PhotonNetwork.NickName = userId.text;
        PlayerPrefs.SetString("USER_ID", userId.text);
        RoomOptions roomOptions = new RoomOptions();
        roomOptions.IsOpen = true;
        roomOptions.IsVisible = true;
        roomOptions.MaxPlayers = 20;

        // ������ ���ǿ� �´� �� ����
        PhotonNetwork.CreateRoom(_roomName, roomOptions, TypedLobby.Default);
    }
    public void OnGUI()
    {
        GUILayout.Label(PhotonNetwork.NetworkClientState.ToString());
    }

    
}
