using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class RoomMgr : MonoBehaviourPunCallbacks
{
    //RPC ȣ���� ���� PhotonView
    private PhotonView pv;

    [Header("Chatting UI")]
    public Text ChatLogTxt;
    public InputField InputChatIF;
    public Dropdown ChatTypeDrop;
    public Button ChatEnterBtn;

    // �� ���� ����
    private bool[] InPlayer = new bool[8]; // 0 ~ 3 : Red��, 4 ~ 7 : Blue��
    ExitGames.Client.Photon.Hashtable m_TeamInfoProps = new ExitGames.Client.Photon.Hashtable();
    [Header("Red Team Select UI")]
    public Button[] RedTeamBtn = new Button[4];
    [Header("Blue Team Select UI")]
    public Button[] BlueTeamBtn = new Button[4];

    // ��ư ����
    [HideInInspector] bool IsReady = false;
    ExitGames.Client.Photon.Hashtable m_PlayerReady = new ExitGames.Client.Photon.Hashtable();
    [Header("Button UI")]
    public Button SelectSkillBtn;
    public Button ReadyBtn;
    public Button StartBtn;
    public bool IsExit = false;
    public Button ExitBtn;

    void Awake()
    {
        //PhotonView ������Ʈ �Ҵ�
        pv = GetComponent<PhotonView>();

        PhotonNetwork.AutomaticallySyncScene = true;
    }

    // Start is called before the first frame update
    void Start()
    {
        if (!ReferenceEquals(RedTeamBtn[0], null))
            RedTeamBtn[0].onClick.AddListener(() => { SendSelectTeam(0, 0); });
        if (!ReferenceEquals(RedTeamBtn[1], null))
            RedTeamBtn[1].onClick.AddListener(() => { SendSelectTeam(0, 1); });
        if (!ReferenceEquals(RedTeamBtn[2], null))
            RedTeamBtn[2].onClick.AddListener(() => { SendSelectTeam(0, 2); });
        if (!ReferenceEquals(RedTeamBtn[3], null))
            RedTeamBtn[3].onClick.AddListener(() => { SendSelectTeam(0, 3); });

        if (!ReferenceEquals(BlueTeamBtn[0], null))
            BlueTeamBtn[0].onClick.AddListener(() => { SendSelectTeam(1, 0); });
        if (!ReferenceEquals(BlueTeamBtn[1], null))
            BlueTeamBtn[1].onClick.AddListener(() => { SendSelectTeam(1, 1); });
        if (!ReferenceEquals(BlueTeamBtn[2], null))
            BlueTeamBtn[2].onClick.AddListener(() => { SendSelectTeam(1, 2); });
        if (!ReferenceEquals(BlueTeamBtn[3], null))
            BlueTeamBtn[3].onClick.AddListener(() => { SendSelectTeam(1, 3); });

        if (!ReferenceEquals(ExitBtn, null))
            ExitBtn.onClick.AddListener(OnClickExitRoom);

        if (!ReferenceEquals(ReadyBtn, null))
            ReadyBtn.onClick.AddListener(() =>
            {
                IsReady = !IsReady;
                SendReady();
            });

        if (!ReferenceEquals(StartBtn, null))
            StartBtn.onClick.AddListener(() =>
            {
                StartCoroutine(LoadInGameScene());    
            });

        // ó�� �������� ù �ڸ� ��� ���� Ȯ��
        foreach (Player a_RefPlayer in PhotonNetwork.PlayerList)
        {
            if (a_RefPlayer.ActorNumber == PhotonNetwork.LocalPlayer.ActorNumber)
                continue;

            int a_Team = -1;
            int a_PosIdx = -1;
            ReceiveSelectTeam(a_RefPlayer, ref a_Team, ref a_PosIdx);
            if (a_Team == -1 || a_PosIdx == -1)
                continue;

            InPlayer[(a_Team + 1) * (a_PosIdx + 1) - 1] = true;
        }
        for (int i = 0; i < InPlayer.Length; i++)
        {
            if (InPlayer[i] == false)
            {
                SendSelectTeam((int)(i / 4), i % 4, true);
                break;
            }
        }

        //��� Ŭ������ ��Ʈ��ũ �޽��� ������ �ٽ� ����
        PhotonNetwork.IsMessageQueueRunning = true;

        PhotonNetwork.CurrentRoom.IsOpen = true;
        PhotonNetwork.CurrentRoom.IsVisible = true;
    }    

    // Update is called once per frame
    void Update()
    {
        RefreshPhotonTeam(); // ����Ʈ UI ����
    }

    bool IsFirstTime = true; // ������ ó�� ��������
    void RefreshPhotonTeam()
    {
        if (IsExit)
            return;

        for (int team = 0; team < 2; team++)
            for (int pos = 0; pos < 4; pos++)
                RefreshUI(null, team, pos, false);

        bool AllReady = true; // ���� �غ� �Ǿ����� Ȯ�� ����

        int Team1Cnt = 0;
        int Team2Cnt = 0;
        foreach (Player a_RefPlayer in PhotonNetwork.PlayerList)
        {
            int a_Team = -1;
            int a_PosIdx = -1;
            ReceiveSelectTeam(a_RefPlayer, ref a_Team, ref a_PosIdx);
            if (a_Team == -1 || a_PosIdx == -1)
                continue;

            if (a_Team == 0)
                Team1Cnt++;
            else
                Team2Cnt++;

            if (ReceiveReady(a_RefPlayer))
            {
                RefreshUI(a_RefPlayer, a_Team, a_PosIdx, true);
            }
            else
            {
                RefreshUI(a_RefPlayer, a_Team, a_PosIdx, false);
                AllReady = false;
            }
        }

        bool PossibleStart = false;
        if (AllReady && Team1Cnt > 0 && Team2Cnt > 0)
        {
            if (Team1Cnt == Team2Cnt)
            {
                if (PhotonNetwork.IsMasterClient)
                    PossibleStart = true;
            }
        }

        if (PossibleStart)
        {
            ReadyBtn.gameObject.SetActive(false);
            StartBtn.gameObject.SetActive(true);
        }
        else
        {
            ReadyBtn.gameObject.SetActive(true);
            StartBtn.gameObject.SetActive(false);
        }
    }

    IEnumerator LoadInGameScene() //���� InGame �� �ε�
    {
        PhotonNetwork.CurrentRoom.IsOpen = false;
        PhotonNetwork.CurrentRoom.IsVisible = false;

        PhotonNetwork.LoadLevel("InGameScene");
        PhotonNetwork.AutomaticallySyncScene = false;
        yield return null;
    }

    void RefreshUI(Player a_Player, int a_Team, int a_Pos, bool a_IsReady)
    {
        Button a_Btn;
        if (a_Team == 0)
            a_Btn = RedTeamBtn[a_Pos];
        else
            a_Btn = BlueTeamBtn[a_Pos];

        if (!ReferenceEquals(a_Player, null))
        {
            a_Btn.GetComponent<Image>().color = Color.green;
            string a_PlayerNickName = string.Format("\n{0}", a_Player.NickName);
            if (a_Player.IsMasterClient)
                a_PlayerNickName = string.Format("[����]\r\n{0}", a_Player.NickName);

            a_Btn.transform.GetComponentInChildren<Text>().text = a_PlayerNickName;
            a_Btn.transform.Find("ReadyBar").gameObject.SetActive(a_IsReady);
            a_Btn.enabled = false;
            InPlayer[(a_Team + 1) * (a_Pos + 1) - 1] = true;
        }
        else
        {
            a_Btn.GetComponent<Image>().color = Color.white;

            a_Btn.transform.GetComponentInChildren<Text>().text = string.Empty;
            a_Btn.transform.Find("ReadyBar").gameObject.SetActive(false);
            a_Btn.enabled = true;
            InPlayer[(a_Team + 1) * (a_Pos + 1) - 1] = false;
        }
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

    void SendSelectTeam(int a_Team, int a_Pos, bool IsEnterRoom = false)
    {
        if (m_TeamInfoProps == null)
        {
            m_TeamInfoProps = new ExitGames.Client.Photon.Hashtable();
            m_TeamInfoProps.Clear();
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
        //�α� �޽��� Text UI�� �ؽ�Ʈ�� �������� ǥ��
        ChatLogTxt.text += msg;
    }

    public void OnClickExitRoom()
    {
        IsExit = true;
        //�α� �޽����� ����� ���ڿ� ����
        string msg = "\n<color=#ff0000>[" + PhotonNetwork.LocalPlayer.NickName + "] Disconnected</color>";
        //RPC �Լ� ȣ��
        pv.RPC("ChatLogMsg", RpcTarget.AllBuffered, msg, 0);
        //������ �Ϸ�� �� ���� ������ ������ ������
        //������ �뿡 �����غ��� ���� �αװ� ǥ��Ǵ� ���� Ȯ���� �� �ִ�.
        //���� PhotonTarget.AllBuffered �ɼ�����
        //RPC�� ȣ���߱� ������ ���߿� �����ص� ������ ���� �α� �޽����� ǥ�õȴ�.

        // ������ ����� ���� ���� �� ���� CustomProperties�� �ʱ�ȭ
        if (!ReferenceEquals(PhotonNetwork.PlayerList, null) && PhotonNetwork.PlayerList.Length <= 1)
        {
            if (!ReferenceEquals(PhotonNetwork.CurrentRoom, null))
                PhotonNetwork.CurrentRoom.CustomProperties.Clear();
        }

        // ���� �������� Player�� ��� CustomProperties�� �ʱ�ȭ
        if (!ReferenceEquals(PhotonNetwork.LocalPlayer, null))
            PhotonNetwork.LocalPlayer.CustomProperties.Clear();

        // ���� ���� ���������� ������ ��� ��Ʈ��ũ ��ü�� ����
        PhotonNetwork.LeaveRoom();
    }

    // �뿡�� ���� ������� �� ȣ��Ǵ� �ݹ� �Լ�
    public override void OnLeftRoom() 
    {
        StartCoroutine(this.LoadLobbyScene());
    }

    IEnumerator LoadLobbyScene()
    {
        //���� �̵��ϴ� ���� ���� Ŭ���� �����κ��� ��Ʈ��ũ �޽��� ���� �ߴ�
        PhotonNetwork.IsMessageQueueRunning = false;

        AsyncOperation ao = SceneManager.LoadSceneAsync("LobbyScene");

        yield return ao;
    }
}
