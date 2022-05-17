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

    // 룸 목록 갱신을 위한 변수
    public GameObject scrollContents; // RoomItem이 차일드로 생성될 Parent객체
    public GameObject roomItem; // 룸 목록만큼 생성될 RoomItem 프리팹
    List<RoomInfo> myList = new List<RoomInfo>();

    void Awake()
    {
        //PhotonNetwork.SendRate = 40;            
        //PhotonNetwork.SerializationRate = 20;
        m_PlayerSettingPanel = Instantiate(Resources.Load("PlayerSetting") as GameObject, GameObject.Find("Canvas").transform);
        m_PlayerSettingPanel.transform.localPosition = new Vector3(395f, 0f, 0f);
        
        if (!PhotonNetwork.IsConnected)
        {
            //1번, 포톤 클라우드에 접속
            PhotonNetwork.ConnectUsingSettings();
            //포톤 서버에 접속시도(지역 서버 접속) -> AppID 사용자 인증 
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

    //2번, ConnectUsingSettings() 함수 호출에 대한 서버 접속이 성공하면 호출되는 콜백 함수
    //PhotonNetwork.LeaveRoom(); 으로 방을 떠날 때도 로비로 나오면서 이 함수가 자동으로 호출된다.
    public override void OnConnectedToMaster()
    {
        Debug.Log("서버 접속 완료");
        PhotonNetwork.JoinLobby();
    }

    //4번, PhotonNetwork.JoinLobby() 성공시 호출되는 로비 접속 콜백함수
    public override void OnJoinedLobby()
    {
        Debug.Log("로비접속완료");
        m_PlayerName_Txt.text = GetUserId();
    }

    //PhotonNetwork.JoinRandomRoom() 이 함수 실패한 경우 호출되는 콜백 함수
    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.Log("랜덤 방 참가 실패 (참가할 방이 존재하지 않습니다.)");

        RoomOptions roomOptions = new RoomOptions();
        roomOptions.IsOpen = true; // 입장 가능 여부
        roomOptions.IsVisible = true; // 로비에서 룸의 노출 여부
        roomOptions.MaxPlayers = 8; // 룸에 입장할 수 있는 최대 접속자 수

        string a_RoomName = string.Format("_{0}", GlobalValue.nickName);
        PhotonNetwork.CreateRoom(a_RoomName, roomOptions, TypedLobby.Default);
    }

    //PhotonNetwork.CreateRoom() 이 함수가 성공하면 2번째로 자동으로 호출되는 함수
    //PhotonNetwork.JoinRoom() 함수가 성공해도 자동으로 호출되는 함수
    //PhotonNetwork.JoinRandomRoom(); 함수가 성공해도 자동으로 호출되는 함수
    public override void OnJoinedRoom()
    {  //서버역할인 경우 5번 : 방입장, 클라이언트 역할인 경우 4번 : 방입장
        Debug.Log("방 참가 완료");
        StartCoroutine(this.LoadRoomScene());
    }

    private void OnApplicationFocus(bool focus)
    {  //윈도우 창 활성화 비활성화 일때
        PhotonInit.isFocus = focus;
    }

    string GetUserId()
    {
        string nickname = GlobalValue.nickName;

        if (string.IsNullOrEmpty(nickname))
            nickname = "USER_" + Random.Range(0, 999).ToString("000");

        return nickname;
    }

    //Join Random Room 버튼 클릭 시 호출되는 함수
    public void QuickMatch_Click() //3번 방 입장 요청 버튼 누름
    {
        SoundManager.Instance.PlayUISound("Button");

        //로컬 플레이어의 이름을 설정
        PhotonNetwork.LocalPlayer.NickName = m_PlayerName_Txt.text;
        PhotonNetwork.JoinRandomRoom();
    }

    //룸 씬으로 이동하는 코루틴 함수
    IEnumerator LoadRoomScene() //최종 InGame 씬 로딩 --> 6번 or 5번
    {
        //씬을 이동하는 동안 포톤 클라우드 서버로부터 네트워크 메시지 수신 중단
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

    // PhotonNetwork.CreateRoom() 이 함수가 실패하면 호출되는 함수
    // 같은 이름의 방이 있을 때 실패함
    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        Debug.Log("방 만들기 실패"); // 주로 같은 이름으로 방을 생성할때 발생
        Debug.Log(returnCode.ToString()); // 오류코드 ErrorCode 클래스
        Debug.Log(message); // 오류메시지
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

        // 룸 목록을 다시 받았을 때 갱신하기 위해 기존에 생성된 RoomItem을 삭제
        GameObject[] ROOM_ITEM = GameObject.FindGameObjectsWithTag("ROOM_ITEM");
        if (ROOM_ITEM.Length > 0)
        {
            foreach (GameObject obj in ROOM_ITEM)
            {
                Destroy(obj);
            }
        }

        // 스크롤 영역 초기화
        scrollContents.GetComponent<RectTransform>().sizeDelta = Vector2.zero;

        for (int i = 0; i < myList.Count; i++)
        {
            GameObject room = (GameObject)Instantiate(roomItem);
            room.transform.SetParent(scrollContents.transform, false);

            // 생성한 RoomItem에 표시하기 위한 텍스트 정보 전달
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