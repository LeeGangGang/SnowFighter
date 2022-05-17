using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PhotonInit : MonoBehaviourPunCallbacks
{
    static public bool isFocus = true;

    public Button m_PlayerSettingBtn;
    private GameObject m_PlayerSettingPanel;
    bool IsShow = false;
    float MvSpeed_PS = 2000f;
    Vector3 ShowPos_PS = new Vector3(420f, 0f, 0f);

    public Button m_ConfigBtn;
    public GameObject m_Config_Pop;
    public Button m_LogoutBtn;

    public Text m_PlayerName_Txt;
    public Button m_QuickMatch_Btn;

    public GameObject Popup_Canvas = null;
    public Button m_CreateRoom_Btn;
    public GameObject m_CreateRoom_Pop;
    public GameObject m_InputRoomPass_Pop;

    // �� ��� ������ ���� ����
    public GameObject scrollContents; // RoomItem�� ���ϵ�� ������ Parent��ü
    public GameObject roomItem; // �� ��ϸ�ŭ ������ RoomItem ������
    List<RoomInfo> myList = new List<RoomInfo>();

    void Awake()
    {
        //PhotonNetwork.SendRate = 40;            
        //PhotonNetwork.SerializationRate = 20;
        m_PlayerSettingPanel = Instantiate(Resources.Load("PlayerSetting") as GameObject, GameObject.Find("Canvas").transform);
        m_PlayerSettingPanel.transform.localPosition = new Vector3(395f, 0f, 0f);
        
        if (!PhotonNetwork.IsConnected)
        {
            //1��, ���� Ŭ���忡 ����
            PhotonNetwork.ConnectUsingSettings();
            //���� ������ ���ӽõ�(���� ���� ����) -> AppID ����� ���� 
        }

        m_PlayerName_Txt.text = GetUserId();
        
        PhotonNetwork.IsMessageQueueRunning = true;
    }

    // Start is called before the first frame update
    void Start()
    {
        if (!ReferenceEquals(m_QuickMatch_Btn, null))
            m_QuickMatch_Btn.onClick.AddListener(QuickMatch_Click);

        if (!ReferenceEquals(m_CreateRoom_Btn, null))
            m_CreateRoom_Btn.onClick.AddListener(CreateRoomPop_Click);

        if (!ReferenceEquals(m_ConfigBtn, null))
            m_ConfigBtn.onClick.AddListener(ConfigBtn_Click);

        if (!ReferenceEquals(m_LogoutBtn, null))
            m_LogoutBtn.onClick.AddListener(LogoutBtn_Click);

        if (!ReferenceEquals(m_PlayerSettingBtn, null))
            m_PlayerSettingBtn.onClick.AddListener(PlayerSettingBtn_Click);
    }

    //2��, ConnectUsingSettings() �Լ� ȣ�⿡ ���� ���� ������ �����ϸ� ȣ��Ǵ� �ݹ� �Լ�
    //PhotonNetwork.LeaveRoom(); ���� ���� ���� ���� �κ�� �����鼭 �� �Լ��� �ڵ����� ȣ��ȴ�.
    public override void OnConnectedToMaster()
    {
        Debug.Log("���� ���� �Ϸ�");
        PhotonNetwork.JoinLobby();
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

        RoomOptions roomOptions = new RoomOptions();
        roomOptions.IsOpen = true; // ���� ���� ����
        roomOptions.IsVisible = true; // �κ񿡼� ���� ���� ����
        roomOptions.MaxPlayers = 8; // �뿡 ������ �� �ִ� �ִ� ������ ��

        string a_RoomName = string.Format("_{0}", GlobalValue.nickName);
        PhotonNetwork.CreateRoom(a_RoomName, roomOptions, TypedLobby.Default);
    }

    //PhotonNetwork.CreateRoom() �� �Լ��� �����ϸ� 2��°�� �ڵ����� ȣ��Ǵ� �Լ�
    //PhotonNetwork.JoinRoom() �Լ��� �����ص� �ڵ����� ȣ��Ǵ� �Լ�
    //PhotonNetwork.JoinRandomRoom(); �Լ��� �����ص� �ڵ����� ȣ��Ǵ� �Լ�
    public override void OnJoinedRoom()
    {  //���������� ��� 5�� : ������, Ŭ���̾�Ʈ ������ ��� 4�� : ������
        Debug.Log("�� ���� �Ϸ�");
        StartCoroutine(this.LoadRoomScene());
    }

    private void OnApplicationFocus(bool focus)
    {  //������ â Ȱ��ȭ ��Ȱ��ȭ �϶�
        PhotonInit.isFocus = focus;
    }

    string GetUserId()
    {
        string nickname = GlobalValue.nickName;

        if (string.IsNullOrEmpty(nickname))
            nickname = "USER_" + Random.Range(0, 999).ToString("000");

        return nickname;
    }

    //Join Random Room ��ư Ŭ�� �� ȣ��Ǵ� �Լ�
    public void QuickMatch_Click() //3�� �� ���� ��û ��ư ����
    {
        SoundManager.Instance.PlayUISound("Button");

        //���� �÷��̾��� �̸��� ����
        PhotonNetwork.LocalPlayer.NickName = m_PlayerName_Txt.text;
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
        SoundManager.Instance.PlayUISound("Button");

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
            roomData.m_RoomPass = myList[i].Name.Split('_')[0];
            roomData.m_RoomName = myList[i].Name.Split('_')[1];
            roomData.m_ConnectPlayer = myList[i].PlayerCount;
            roomData.m_MaxPlayer = myList[i].MaxPlayers;

            roomData.DispRoomData(myList[i].IsOpen);
        }
    }

    public void OnClickRoomItem(string RoomName, string RoomPass)
    {
        if (!string.IsNullOrEmpty(RoomPass))
        {
            if (m_InputRoomPass_Pop == null)
                m_InputRoomPass_Pop = Resources.Load("InputRoomPass_Pop") as GameObject;

            GameObject a_InputRoomPass_Pop = (GameObject)Instantiate(m_InputRoomPass_Pop);
            a_InputRoomPass_Pop.GetComponent<InputRoomPassCtrl>().InitData(RoomName, RoomPass);
            a_InputRoomPass_Pop.transform.SetParent(Popup_Canvas.transform, false);
        }
        else
        {
            string a_RoomName = string.Format($"{RoomPass}_{RoomName}");
            PhotonNetwork.LocalPlayer.NickName = GlobalValue.nickName;
            PhotonNetwork.JoinRoom(a_RoomName);
        }
    }

    private void ConfigBtn_Click()
    {
        SoundManager.Instance.PlayUISound("Button");

        if (m_Config_Pop == null)
            m_Config_Pop = Resources.Load("Prefabs/ConfigPanel") as GameObject;

        GameObject a_Config_Pop = (GameObject)Instantiate(m_Config_Pop);
        a_Config_Pop.transform.SetParent(Popup_Canvas.transform, false);
    }

    private void LogoutBtn_Click()
    {
        SoundManager.Instance.PlayUISound("Button");

        GlobalValue.ClearData();
        SceneManager.LoadScene("TitleScene");
    }

    private void PlayerSettingBtn_Click()
    {
        SoundManager.Instance.PlayUISound("Button");

        m_PlayerSettingPanel.GetComponentInChildren<PlayerSettingCtrl>().IsShow = true;
    }
}