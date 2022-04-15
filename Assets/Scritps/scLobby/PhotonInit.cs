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

    // 룸 목록 갱신을 위한 변수
    public GameObject scrollContents; // RoomItem이 차일드로 생성될 Parent객체
    public GameObject roomItem; // 룸 목록만큼 생성될 RoomItem 프리팹
    List<RoomInfo> myList = new List<RoomInfo>();

    void Awake()
    {
        //PhotonNetwork.SendRate = 40;            
        //PhotonNetwork.SerializationRate = 20;

        if (!PhotonNetwork.IsConnected)
        {
            //1번, 포톤 클라우드에 접속
            PhotonNetwork.ConnectUsingSettings();
            //포톤 서버에 접속시도(지역 서버 접속) -> AppID 사용자 인증 
        }

        //사용자 이름 설정
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

    //2번, ConnectUsingSettings() 함수 호출에 대한 서버 접속이 성공하면 호출되는 콜백 함수
    //PhotonNetwork.LeaveRoom(); 으로 방을 떠날 때도 로비로 나오면서 이 함수가 자동으로 호출된다.
    public override void OnConnectedToMaster()
    {
        Debug.Log("서버 접속 완료");
        //단순 포톤 서버 접속만 된 상태 (ConnectedToMaster)

        //3번, 규모가 작은 게임에서는 로비가 보통 하나이고...
        PhotonNetwork.JoinLobby();
        //대형 게임인 경우 상급자로비, 중급자로비, 초보자로비 처럼 로비가 여러개일 수 있다. 
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

        RoomOptions roomOptions = new RoomOptions(); // using Photon.Realtime;
        roomOptions.IsOpen = true;  // 입장 가능 여부
        roomOptions.IsVisible = true;   // 로비에서 룸의 노출 여부
        roomOptions.MaxPlayers = 8; // 룸에 입장할 수 있는 최대 접속자 수

        //지정한 조건에 맞는 룸 생성 함수
        PhotonNetwork.CreateRoom("MyRoom", roomOptions, TypedLobby.Default);
    }

    //PhotonNetwork.CreateRoom() 이 함수가 성공하면 2번째로 자동으로 호출되는 함수
    //PhotonNetwork.JoinRoom() 함수가 성공해도 자동으로 호출되는 함수
    //PhotonNetwork.JoinRandomRoom(); 함수가 성공해도 자동으로 호출되는 함수
    public override void OnJoinedRoom()
    {  //서버역할인 경우 5번 : 방입장, 클라이언트 역할인 경우 4번 : 방입장
        Debug.Log("방 참가 완료");

        //룸 씬으로 이동하는 코루틴 실행
        StartCoroutine(this.LoadRoomScene());
    }

    private void OnApplicationFocus(bool focus)
    {  //윈도우 창 활성화 비활성화 일때
        PhotonInit.isFocus = focus;
    }

    //로컬에 저장된 플레이어 이름을 변환하거나 생성하는 함수
    string GetUserId()
    {
        string userId = PlayerPrefs.GetString("USER_ID");

        if (string.IsNullOrEmpty(userId))
            userId = "USER_" + Random.Range(0, 999).ToString("000");

        return userId;
    }

    //Join Random Room 버튼 클릭 시 호출되는 함수
    public void QuickMatch_Click()         //3번 방 입장 요청 버튼 누름
    {
        //로컬 플레이어의 이름을 설정
        PhotonNetwork.LocalPlayer.NickName = m_PlayerName_Txt.text;
        //플레이어 이름을 저장
        PlayerPrefs.SetString("USER_ID", m_PlayerName_Txt.text);

        //5번 무작위로 추출된 방으로 입장
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
            roomData.roomName = myList[i].Name;
            roomData.connectPlayer = myList[i].PlayerCount;
            roomData.maxPlayer = myList[i].MaxPlayers;

            // 텍스트 정보를 표시
            roomData.DispRoomData(myList[i].IsOpen);
        }
    }

    public void OnClickRoomItem(string roomName)
    {
        // 로컬 플레이어의 이름을 설정
        PhotonNetwork.LocalPlayer.NickName = m_PlayerName_Txt.text;
        // 플레이어 이름을 저장
        PlayerPrefs.SetString("USER_ID", m_PlayerName_Txt.text);
        // 인자로 전달된 이름에 해당하는 룸으로 입장
        PhotonNetwork.JoinRoom(roomName);
    }
}