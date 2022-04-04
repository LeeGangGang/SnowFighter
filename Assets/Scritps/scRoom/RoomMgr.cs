using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class RoomMgr : MonoBehaviourPunCallbacks
{
    //RPC 호출을 위한 PhotonView
    private PhotonView pv;

    [Header("Chatting UI")]
    public Text ChatLogTxt;
    public InputField InputChatIF;
    public Dropdown ChatTypeDrop;
    public Button ChatEnterBtn;

    // 팀 관련 변수
    ExitGames.Client.Photon.Hashtable m_TeamInfoProps = new ExitGames.Client.Photon.Hashtable();
    [Header("Red Team Select UI")]
    public Button RedTeam1Btn;
    public Button RedTeam2Btn;
    public Button RedTeam3Btn;
    public Button RedTeam4Btn;
    [Header("Blue Team Select UI")]
    public Button BlueTeam1Btn;
    public Button BlueTeam2Btn;
    public Button BlueTeam3Btn;
    public Button BlueTeam4Btn;

    // 버튼 관련
    [HideInInspector] bool IsReady = false;
    ExitGames.Client.Photon.Hashtable m_PlayerReady = new ExitGames.Client.Photon.Hashtable();
    [Header("Button UI")]
    public Button SelectSkillBtn;
    public Button ReadyBtn;
    public Button ExitBtn;

    void Awake()
    {
        //PhotonView 컴포넌트 할당
        pv = GetComponent<PhotonView>();

        PhotonNetwork.AutomaticallySyncScene = true;
    }

    // Start is called before the first frame update
    void Start()
    {
        if (RedTeam1Btn != null)
            RedTeam1Btn.onClick.AddListener(() => { SendSelectTeam(0, 0); });
        if (RedTeam2Btn != null)
            RedTeam2Btn.onClick.AddListener(() => { SendSelectTeam(0, 1); });
        if (RedTeam3Btn != null)
            RedTeam3Btn.onClick.AddListener(() => { SendSelectTeam(0, 2); });
        if (RedTeam4Btn != null)
            RedTeam4Btn.onClick.AddListener(() => { SendSelectTeam(0, 3); });

        if (BlueTeam1Btn != null)
            BlueTeam1Btn.onClick.AddListener(() => { SendSelectTeam(1, 0); });
        if (BlueTeam2Btn != null)
            BlueTeam2Btn.onClick.AddListener(() => { SendSelectTeam(1, 1); });
        if (BlueTeam3Btn != null)
            BlueTeam3Btn.onClick.AddListener(() => { SendSelectTeam(1, 2); });
        if (BlueTeam4Btn != null)
            BlueTeam4Btn.onClick.AddListener(() => { SendSelectTeam(1, 3); });

        if (ExitBtn != null)
            ExitBtn.onClick.AddListener(OnClickExitRoom);

        if (ReadyBtn != null)
            ReadyBtn.onClick.AddListener(() =>
            {
                IsReady = !IsReady;
                SendReady();
                //StartCoroutine(this.LoadInGameScene());
            });

        //모든 클라우드의 네트워크 메시지 수신을 다시 연결
        PhotonNetwork.IsMessageQueueRunning = true;
    }    

    // Update is called once per frame
    void Update()
    {
        RefreshPhotonTeam(); // 리스트 UI 갱신
    }

    void RefreshPhotonTeam()
    {
        int Team1Cnt = 0;
        int Team2Cnt = 0;

        bool AllReady = true;

        int a_Team = 0;
        int a_PosIdx = 0;
        foreach (Player a_RefPlayer in PhotonNetwork.PlayerList)
        {
            ReceiveSelectTeam(a_RefPlayer, ref a_Team, ref a_PosIdx);

            if (a_Team == 0)
                Team1Cnt++;
            else
                Team2Cnt++;

            if (ReceiveReady(a_RefPlayer))
            {

            }
            else
            {
                AllReady = false;
            }
        }

        if (AllReady && Team1Cnt > 0 && Team2Cnt > 0)
        {
            if (Team1Cnt == Team2Cnt)
            {
                if (bTest == false)
                    return;

                if (PhotonNetwork.IsMasterClient)
                {
                    StartCoroutine(LoadInGameScene());
                }
            }
        }
    }

    bool bTest = true;
    IEnumerator LoadInGameScene() //최종 InGame 씬 로딩 --> 6번 or 5번
    {
        bTest = false;
        PhotonNetwork.LoadLevel("InGameScene");
        PhotonNetwork.AutomaticallySyncScene = false;
        yield return null;
    }

    void SendReady()
    {
        if (m_TeamInfoProps == null)
        {
            m_TeamInfoProps = new ExitGames.Client.Photon.Hashtable();
            m_TeamInfoProps.Clear();
        }

        if (m_TeamInfoProps.ContainsKey("IsReady") == true)
            m_TeamInfoProps["IsReady"] = IsReady;
        else
            m_TeamInfoProps.Add("IsReady", IsReady);

        PhotonNetwork.LocalPlayer.SetCustomProperties(m_TeamInfoProps);
    }

    bool ReceiveReady(Player a_Player)
    {
        if (a_Player == null)
            return false;

        if (a_Player.CustomProperties.ContainsKey("IsReady") == false)
            return false;
        
        return (bool)a_Player.CustomProperties["IsReady"];
    }

    void SendSelectTeam(int a_Team, int a_Pos)
    {
        if (m_TeamInfoProps == null)
        {
            m_TeamInfoProps = new ExitGames.Client.Photon.Hashtable();
            m_TeamInfoProps.Clear();
        }

        if (m_TeamInfoProps.ContainsKey("MyTeam") == true && m_TeamInfoProps.ContainsKey("TeamIdx") == true)
        {
            // 이전 정보가 남은 UI 제거
            int a_BeforeTeam = (int)m_TeamInfoProps["MyTeam"];
            int a_BeforePosIdx = (int)m_TeamInfoProps["TeamIdx"];
            if (a_BeforeTeam == 0)
            {
                if (a_BeforePosIdx == 0)
                    RedTeam1Btn.GetComponentInChildren<Text>().text = "";
                else if (a_BeforePosIdx == 1)
                    RedTeam2Btn.GetComponentInChildren<Text>().text = "";
                else if (a_BeforePosIdx == 2)
                    RedTeam3Btn.GetComponentInChildren<Text>().text = "";
                else if (a_BeforePosIdx == 3)
                    RedTeam4Btn.GetComponentInChildren<Text>().text = "";
            }
            else
            {
                if (a_BeforePosIdx == 0)
                    BlueTeam1Btn.GetComponentInChildren<Text>().text = "";
                else if (a_BeforePosIdx == 1)
                    BlueTeam2Btn.GetComponentInChildren<Text>().text = "";
                else if (a_BeforePosIdx == 2)
                    BlueTeam3Btn.GetComponentInChildren<Text>().text = "";
                else if (a_BeforePosIdx == 3)
                    BlueTeam4Btn.GetComponentInChildren<Text>().text = "";
            }
        }

        if (m_TeamInfoProps.ContainsKey("MyTeam") == true)
            m_TeamInfoProps["MyTeam"] = a_Team;
        else
            m_TeamInfoProps.Add("MyTeam", a_Team);

        if (m_TeamInfoProps.ContainsKey("TeamIdx") == true)
            m_TeamInfoProps["TeamIdx"] = a_Pos;
        else
            m_TeamInfoProps.Add("TeamIdx", a_Pos);

        PhotonNetwork.LocalPlayer.SetCustomProperties(m_TeamInfoProps);
    }

    public void ReceiveSelectTeam(Player a_Player, ref int a_Team, ref int a_Pos)
    {
        a_Team = 0;
        a_Pos = 0;
        if (a_Player == null)
            return;

        if (a_Player.CustomProperties.ContainsKey("MyTeam") == true)
            a_Team = (int)a_Player.CustomProperties["MyTeam"];

        if (a_Player.CustomProperties.ContainsKey("TeamIdx") == true)
            a_Pos = (int)a_Player.CustomProperties["TeamIdx"];
    }

    [PunRPC]
    void ChatLogMsg(string msg, int teamIdx)
    {
        //if (teamIdx != )
        //로그 메시지 Text UI에 텍스트를 누적시켜 표시
        ChatLogTxt.text += msg;
    }

    public void OnClickExitRoom()
    {
        //로그 메시지에 출력할 문자열 생성
        string msg = "\n<color=#ff0000>[" + PhotonNetwork.LocalPlayer.NickName + "] Disconnected</color>";
        //RPC 함수 호출
        pv.RPC("ChatLogMsg", RpcTarget.AllBuffered, msg, 0);
        //설정이 완료된 후 빌드 파일을 여러개 실행해
        //동일한 룸에 입장해보면 접속 로그가 표기되는 것을 확인할 수 있다.
        //또한 PhotonTarget.AllBuffered 옵션으로
        //RPC를 호출했기 때문에 나중에 입장해도 기존의 접속 로그 메시지가 표시된다.

        // 마지막 사람이 방을 떠날 때 룸의 CustomProperties를 초기화 해야한다.
        if (PhotonNetwork.PlayerList != null && PhotonNetwork.PlayerList.Length <= 1)
        {
            if (PhotonNetwork.CurrentRoom != null)
                PhotonNetwork.CurrentRoom.CustomProperties.Clear();
        }

        // 지금 나가려는 탱크를 찾아서 그 탱크의 모든 CustomProperties를 초기화 해주고 나가는 것이 좋다.
        if (PhotonNetwork.LocalPlayer != null)
            PhotonNetwork.LocalPlayer.CustomProperties.Clear();

        //현재 룸을 빠져나가며 생성한 모든 네트워크 객체를 삭제
        PhotonNetwork.LeaveRoom();
    }

    //룸에서 접속 종료됐을 때 호출되는 콜백 함수
    public override void OnLeftRoom() 
    {
        StartCoroutine(this.LoadLobbyScene());
    }

    IEnumerator LoadLobbyScene() //최종 InGame 씬 로딩 --> 6번 or 5번
    {
        //씬을 이동하는 동안 포톤 클라우드 서버로부터 네트워크 메시지 수신 중단
        PhotonNetwork.IsMessageQueueRunning = false;

        AsyncOperation ao = SceneManager.LoadSceneAsync("LobbyScene");

        yield return ao;
    }
}
