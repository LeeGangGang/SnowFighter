using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public enum ChatType
{
    Red = 0,
    Blue,
    All
}

public class RoomMgr : MonoBehaviourPunCallbacks
{
    //RPC ȣ���� ���� PhotonView
    private PhotonView pv;

    [Header("Chatting UI")]
    public Text ChatLogTxt;
    public InputField InputChatIF;
    public Dropdown ChatTypeDrop;
    public Button ChatEnterBtn;

    [Header("PlayerSetting UI")]
    public Button PlayerSettingShowBtn;
    private GameObject m_PlayerSettingPanel;

    [Header("RoomInfo UI")]
    public Text RoomNameTxt;
    public Text PlayerCntTxt;

    // �� ���� ����
    private bool[] InPlayer = new bool[8]; // 0 ~ 3 : Red��, 4 ~ 7 : Blue��
    ExitGames.Client.Photon.Hashtable m_PlayerInfoProps = new ExitGames.Client.Photon.Hashtable(); // ��, ���� ���� ����
    [Header("Red Team Select UI")]
    public Button[] RedTeamBtn = new Button[4];
    [Header("Blue Team Select UI")]
    public Button[] BlueTeamBtn = new Button[4];

    // ��ư ����
    [HideInInspector] bool IsReady = false;
    [Header("Button UI")]
    public Button ReadyBtn;
    public Button StartBtn;
    public bool IsExit = false;
    public Button ExitBtn;

    void Awake()
    {
        Time.timeScale = 1f;
        //PhotonView ������Ʈ �Ҵ�
        pv = GetComponent<PhotonView>();
        m_PlayerSettingPanel = Instantiate(Resources.Load("PlayerSetting") as GameObject, GameObject.Find("Canvas").transform);
        m_PlayerSettingPanel.transform.localPosition = new Vector3(395f, 0f, 0f);

        RoomNameTxt.text = PhotonNetwork.CurrentRoom.Name.Split('_')[1];

        IsReady = false;
        SendReady();
        PhotonNetwork.AutomaticallySyncScene = false;
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
                SoundManager.Instance.PlayUISound("Button");
                IsReady = !IsReady;
                PhotonNetwork.AutomaticallySyncScene = IsReady;
                SendReady();
            });

        if (!ReferenceEquals(StartBtn, null))
            StartBtn.onClick.AddListener(() =>
            {
                SoundManager.Instance.PlayUISound("Button");
                StartCoroutine(LoadInGameScene());    
            });

        if (!ReferenceEquals(PlayerSettingShowBtn, null))
            PlayerSettingShowBtn.onClick.AddListener(PlayerSettingBtn_Click);

        if (!ReferenceEquals(ChatEnterBtn, null))
            ChatEnterBtn.onClick.AddListener(EnterChat);

        SoundManager.Instance.PlayBGM("RoomBgm");

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

        string msg = string.Format("{0}_<color=#00ff00>[ {1} ] �����Ͽ����ϴ�.</color>", (int)ChatType.All, PhotonNetwork.LocalPlayer.NickName);
        pv.RPC("ChatLogMsg", RpcTarget.Others, msg, 0);
    }    

    // Update is called once per frame
    void Update()
    {
        if (PhotonNetwork.CurrentRoom != null)
            PlayerCntTxt.text = string.Format("({0} / {1})", PhotonNetwork.CurrentRoom.PlayerCount, PhotonNetwork.CurrentRoom.MaxPlayers);

        RefreshPhotonTeam(); // ����Ʈ UI ����
    }

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
        foreach (Player a_Player in PhotonNetwork.PlayerList)
            a_Player.CustomProperties["IsReady"] = false;
        
        PhotonNetwork.CurrentRoom.IsOpen = false;
        PhotonNetwork.CurrentRoom.IsVisible = false;

        PhotonNetwork.LoadLevel("InGameScene");

        PhotonNetwork.IsMessageQueueRunning = true;
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
        if (m_PlayerInfoProps == null)
        {
            m_PlayerInfoProps = new ExitGames.Client.Photon.Hashtable();
            m_PlayerInfoProps.Clear();
        }

        if (m_PlayerInfoProps.ContainsKey("IsReady") == true)
            m_PlayerInfoProps["IsReady"] = IsReady;
        else
            m_PlayerInfoProps.Add("IsReady", IsReady);

        PhotonNetwork.LocalPlayer.SetCustomProperties(m_PlayerInfoProps);
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
        if (m_PlayerInfoProps == null)
        {
            m_PlayerInfoProps = new ExitGames.Client.Photon.Hashtable();
            m_PlayerInfoProps.Clear();
        }

        if (m_PlayerInfoProps.ContainsKey("MyTeam") == true)
            m_PlayerInfoProps["MyTeam"] = a_Team;
        else
            m_PlayerInfoProps.Add("MyTeam", a_Team);

        if (m_PlayerInfoProps.ContainsKey("TeamIdx") == true)
            m_PlayerInfoProps["TeamIdx"] = a_Pos;
        else
            m_PlayerInfoProps.Add("TeamIdx", a_Pos);

        PhotonNetwork.LocalPlayer.SetCustomProperties(m_PlayerInfoProps);
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
        if (int.Parse(msg.Split('_')[0]) != (int)ChatType.All)
        {
            if (int.Parse(msg.Split('_')[0]) != (int)m_PlayerInfoProps["MyTeam"])
                return;
        }

        if (int.Parse(msg.Split('_')[0]) == (int)ChatType.All)
            msg  = "<color=#FFFFFF>" + msg.Substring(2) + "</color>\n";
        else
            msg = "<color=#0099FF>" + msg.Substring(2) + "</color>\n";

        ChatLogTxt.text += msg;
    }

    public void OnClickExitRoom()
    {
        SoundManager.Instance.PlayUISound("Button");

        IsExit = true;
        string msg = string.Format("{0}_<color=#ff0000>[ {1} ] �������ϴ�.</color>", (int)ChatType.All, PhotonNetwork.LocalPlayer.NickName);
        pv.RPC("ChatLogMsg", RpcTarget.All, msg, 0);

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

    private void EnterChat()
    {
        if (string.IsNullOrEmpty(InputChatIF.text))
            return;

        SoundManager.Instance.PlayUISound("Button");

        int Type = (int)ChatType.All;
        if (ChatTypeDrop.value == 1)
            Type = (int)m_PlayerInfoProps["MyTeam"];

        string msg = string.Format("{0}_{1} : {2}", Type, GlobalValue.nickName, InputChatIF.text);
        InputChatIF.text = string.Empty;
        pv.RPC("ChatLogMsg", RpcTarget.All, msg, 0);
    }

    private void PlayerSettingBtn_Click()
    {
        SoundManager.Instance.PlayUISound("Button");
        m_PlayerSettingPanel.GetComponentInChildren<PlayerSettingCtrl>().IsShow = true;
    }
}
