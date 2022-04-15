using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PhotonInit : MonoBehaviourPunCallbacks  //MonoBehaviour
{
    static public bool isFocus = true;

    public Text m_PlayerName_Txt;
    public Button m_QuickMatch_Btn;


    public GameObject Popup_Canvas = null;
    public Button m_CreateRoom_Btn;
    public GameObject m_CreateRoom_Pop;

    // �� ��� ������ ���� ����
    public GameObject scrollContents; // RoomItem�� ���ϵ�� ������ Parent��ü
    public GameObject roomItem; // �� ��ϸ�ŭ ������ RoomItem ������
    List<RoomInfo> myList = new List<RoomInfo>();

    void Awake()
    {
        //PhotonNetwork.SendRate = 40;            
        //PhotonNetwork.SerializationRate = 20;

        if (!PhotonNetwork.IsConnected)
        {
            //1��, ���� Ŭ���忡 ����
            PhotonNetwork.ConnectUsingSettings();
            //���� ������ ���ӽõ�(���� ���� ����) -> AppID ����� ���� 
        }

        //����� �̸� ����
        m_PlayerName_Txt.text = GetUserId();
        
        PhotonNetwork.IsMessageQueueRunning = true;
    }

    // Start is called before the first frame update
    void Start()
    {
        if (m_QuickMatch_Btn != null)
            m_QuickMatch_Btn.onClick.AddListener(QuickMatch_Click);

        if (m_CreateRoom_Btn != null)
            m_CreateRoom_Btn.onClick.AddListener(CreateRoomPop_Click);
    }

    //2��, ConnectUsingSettings() �Լ� ȣ�⿡ ���� ���� ������ �����ϸ� ȣ��Ǵ� �ݹ� �Լ�
    //PhotonNetwork.LeaveRoom(); ���� ���� ���� ���� �κ�� �����鼭 �� �Լ��� �ڵ����� ȣ��ȴ�.
    public override void OnConnectedToMaster()
    {
        Debug.Log("���� ���� �Ϸ�");
        //�ܼ� ���� ���� ���Ӹ� �� ���� (ConnectedToMaster)

        //3��, �Ը� ���� ���ӿ����� �κ� ���� �ϳ��̰�...
        PhotonNetwork.JoinLobby();
        //���� ������ ��� ����ڷκ�, �߱��ڷκ�, �ʺ��ڷκ� ó�� �κ� �������� �� �ִ�. 
    }

    //4��, PhotonNetwork.JoinLobby() ������ ȣ��Ǵ� �κ� ���� �ݹ��Լ�
    public override void OnJoinedLobby()
    {
        Debug.Log("�κ����ӿϷ�");
        m_PlayerName_Txt.text = GetUserId();
    }

    //PhotonNetwork.JoinRandomRoom() �� �Լ� ������ ��� ȣ��Ǵ� �ݹ� �Լ�
    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.Log("���� �� ���� ���� (������ ���� �������� �ʽ��ϴ�.)");

        RoomOptions roomOptions = new RoomOptions(); // using Photon.Realtime;
        roomOptions.IsOpen = true;  // ���� ���� ����
        roomOptions.IsVisible = true;   // �κ񿡼� ���� ���� ����
        roomOptions.MaxPlayers = 8; // �뿡 ������ �� �ִ� �ִ� ������ ��

        //������ ���ǿ� �´� �� ���� �Լ�
        PhotonNetwork.CreateRoom("MyRoom", roomOptions, TypedLobby.Default);
    }

    //PhotonNetwork.CreateRoom() �� �Լ��� �����ϸ� 2��°�� �ڵ����� ȣ��Ǵ� �Լ�
    //PhotonNetwork.JoinRoom() �Լ��� �����ص� �ڵ����� ȣ��Ǵ� �Լ�
    //PhotonNetwork.JoinRandomRoom(); �Լ��� �����ص� �ڵ����� ȣ��Ǵ� �Լ�
    public override void OnJoinedRoom()
    {  //���������� ��� 5�� : ������, Ŭ���̾�Ʈ ������ ��� 4�� : ������
        Debug.Log("�� ���� �Ϸ�");

        //�� ������ �̵��ϴ� �ڷ�ƾ ����
        StartCoroutine(this.LoadRoomScene());
    }

    private void OnApplicationFocus(bool focus)
    {  //������ â Ȱ��ȭ ��Ȱ��ȭ �϶�
        PhotonInit.isFocus = focus;
    }

    //���ÿ� ����� �÷��̾� �̸��� ��ȯ�ϰų� �����ϴ� �Լ�
    string GetUserId()
    {
        string userId = PlayerPrefs.GetString("USER_ID");

        if (string.IsNullOrEmpty(userId))
            userId = "USER_" + Random.Range(0, 999).ToString("000");

        return userId;
    }

    //Join Random Room ��ư Ŭ�� �� ȣ��Ǵ� �Լ�
    public void QuickMatch_Click()         //3�� �� ���� ��û ��ư ����
    {
        //���� �÷��̾��� �̸��� ����
        PhotonNetwork.LocalPlayer.NickName = m_PlayerName_Txt.text;
        //�÷��̾� �̸��� ����
        PlayerPrefs.SetString("USER_ID", m_PlayerName_Txt.text);

        //5�� �������� ����� ������ ����
        PhotonNetwork.JoinRandomRoom();
    }

    //�� ������ �̵��ϴ� �ڷ�ƾ �Լ�
    IEnumerator LoadRoomScene() //���� InGame �� �ε� --> 6�� or 5��
    {
        //���� �̵��ϴ� ���� ���� Ŭ���� �����κ��� ��Ʈ��ũ �޽��� ���� �ߴ�
        PhotonNetwork.IsMessageQueueRunning = false;

        AsyncOperation ao = SceneManager.LoadSceneAsync("RoomScene");

        yield return ao;
    }

    public void CreateRoomPop_Click()
    {
        if (m_CreateRoom_Pop == null)
            m_CreateRoom_Pop = Resources.Load("CreateRoom_Pop") as GameObject;

        GameObject a_CreateRoom_Pop = (GameObject)Instantiate(m_CreateRoom_Pop);
        a_CreateRoom_Pop.transform.SetParent(Popup_Canvas.transform, false);
    }
    

    // PhotonNetwork.CreateRoom() �� �Լ��� �����ϸ� ȣ��Ǵ� �Լ�
    // ���� �̸��� ���� ���� �� ������
    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        Debug.Log("�� ����� ����"); // �ַ� ���� �̸����� ���� �����Ҷ� �߻�
        Debug.Log(returnCode.ToString()); // �����ڵ� ErrorCode Ŭ����
        Debug.Log(message); // �����޽���
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        int roomCnt = roomList.Count;
        for (int i = 0; i < roomCnt; i++)
        {
            if (!roomList[i].RemovedFromList)
            {
                if (!myList.Contains(roomList[i]))
                    myList.Add(roomList[i]);
                else
                    myList[myList.IndexOf(roomList[i])] = roomList[i];
            }
            else
            {
                if (myList.IndexOf(roomList[i]) != -1)
                    myList.RemoveAt(myList.IndexOf(roomList[i]));
            }
        }

        // �� ����� �ٽ� �޾��� �� �����ϱ� ���� ������ ������ RoomItem�� ����
        GameObject[] ROOM_ITEM = GameObject.FindGameObjectsWithTag("ROOM_ITEM");
        if (ROOM_ITEM.Length > 0)
        {
            foreach (GameObject obj in ROOM_ITEM)
            {
                Destroy(obj);
            }
        }

        // ��ũ�� ���� �ʱ�ȭ
        scrollContents.GetComponent<RectTransform>().sizeDelta = Vector2.zero;

        for (int i = 0; i < myList.Count; i++)
        {
            GameObject room = (GameObject)Instantiate(roomItem);
            room.transform.SetParent(scrollContents.transform, false);

            // ������ RoomItem�� ǥ���ϱ� ���� �ؽ�Ʈ ���� ����
            RoomData roomData = room.GetComponent<RoomData>();
            roomData.roomName = myList[i].Name;
            roomData.connectPlayer = myList[i].PlayerCount;
            roomData.maxPlayer = myList[i].MaxPlayers;

            // �ؽ�Ʈ ������ ǥ��
            roomData.DispRoomData(myList[i].IsOpen);
        }
    }

    public void OnClickRoomItem(string roomName)
    {
        // ���� �÷��̾��� �̸��� ����
        PhotonNetwork.LocalPlayer.NickName = m_PlayerName_Txt.text;
        // �÷��̾� �̸��� ����
        PlayerPrefs.SetString("USER_ID", m_PlayerName_Txt.text);
        // ���ڷ� ���޵� �̸��� �ش��ϴ� ������ ����
        PhotonNetwork.JoinRoom(roomName);
    }
}