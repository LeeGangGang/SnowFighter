using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using System.Linq;
using UnityEngine.SceneManagement;

public enum GameState
{
    GS_Ready = 0,
    GS_Playing,
    Gs_GameEnd,
    Gs_GameExit,
}

public class GameMgr : MonoBehaviourPunCallbacks, IPunObservable
{
    // RPC È£­„À» À§ÇÑ PhotonView
    private PhotonView pv;

    public GameObject Popup_Canvas = null;
    public Button m_ConfigBtn;
    public GameObject m_Config_Pop;
    public Text m_SnowCntText;

    public Transform[] m_ArrSkillSlot = new Transform[2];

    public GameObject m_CastingObj;
    public Image m_CastingBar;
    public Text m_CastingTxt;

    public GameState m_GameState = GameState.GS_Ready;
    ExitGames.Client.Photon.Hashtable m_StateProps = new ExitGames.Client.Photon.Hashtable();
    public int m_WinTeam = -1; // -1 : ¹«½ÂºÎ, 0 : RedÆÀ ½Â¸®, 1 : BlueÆÀ ½Â¸®
    ExitGames.Client.Photon.Hashtable m_WinTeamProps = new ExitGames.Client.Photon.Hashtable();
    public static GameMgr Inst;

    private int m_RoundCnt = 0;

    [Header("--- GameEndPanel UI ---")]
    public GameObject m_GameEndPanel;
    public Text m_GameEndInfoTxt;
    public Button m_ExitBtn;

    [Header("--- StartTimer UI ---")]
    public Text m_WaitTmText; // °ÔÀÓ ½ÃÀÛÈÄ Ä«¿îÆ® 3, 2, 1, 0
    [HideInInspector] float m_GoWaitGame = 4f; // °ÔÀÓ ½ÃÀÛÈÄ Ä«¿îÆ® Text UI

    // °ÔÀÓ Å¸ÀÌ¸Ó
    public Text m_InGameTmText; // 180ÃÊ °ÔÀÓ Å¸ÀÌ¸Ó
    [HideInInspector] public float m_InGameTimer = 180f; // 0ÃÊ°¡ µÇ¸é °ÔÀÓ Á¾·á

    void Awake()
    {
        PhotonNetwork.AutomaticallySyncScene = true;

        Inst = this;

        m_InGameTimer = 180f;
        m_WinTeam = -1;

        // PhotonView ÄÄÆ÷³ÍÆ® ÇÒ´ç
        pv = GetComponent<PhotonView>();

        for (int i = 0; i < GlobalValue.skillSet.Length; i++)
        {
            if (GlobalValue.skillSet[i] == -1)
                continue;

            SkillType type = (SkillType)GlobalValue.skillSet[i];
            GameObject a_SkillBtnPrefab = Resources.Load<GameObject>(string.Format("SkillBtnPrefabs/{0}Btn", type.ToString()));
            GameObject a_SkillBtn = Instantiate(a_SkillBtnPrefab, m_ArrSkillSlot[i]);
            a_SkillBtn.name = string.Format("{0}Btn", type.ToString());
        }

        InitGStateProps();
    }

    // Start is called before the first frame update
    void Start()
    {
        PhotonNetwork.IsMessageQueueRunning = true;

        if (!ReferenceEquals(m_ConfigBtn, null))
            m_ConfigBtn.onClick.AddListener(ConfigBtn_Click);

        if (!ReferenceEquals(m_ExitBtn, null))
            m_ExitBtn.onClick.AddListener(ExitBtn_Click);
    }

    // Update is called once per frame
    void Update()
    {
        m_SnowCntText.text = "x " + Camera.main.GetComponent<CameraCtrl>().Player.GetComponent<PlayerCtrl>().m_CurSnowCnt.ToString();
        m_InGameTmText.text = System.TimeSpan.FromSeconds(m_InGameTimer).ToString(@"mm\:ss");

        if (IsGamePossible() == false)
            return;
        
        switch (m_GameState)
        {
            case GameState.GS_Ready:
                if (GameStartObserver())
                {
                    if (PhotonNetwork.IsMasterClient) // ¸¶½ºÅÍ Å¬¶óÀÌ¾ðÆ®¸¸
                    {
                        SendGState(GameState.GS_Playing);
                    }
                }
                break;
            case GameState.GS_Playing:
                {
                    // ³×Æ®¿öÅ© Áö¿¬À¸·Î ´Ê°Ô µé¾î¿À´Â °æ¿ì°¡ »ý°Ü m_WaitTmText Active ²¨ÁÜ
                    if (m_WaitTmText.gameObject.activeSelf)
                        m_WaitTmText.gameObject.SetActive(false);

                    if (PhotonNetwork.IsMasterClient) // ¸¶½ºÅÍ Å¬¶óÀÌ¾ðÆ®¸¸
                    {
                        m_InGameTimer -= Time.deltaTime;
                        GameEndCheck();
                    }

                    if (m_InGameTimer <= 120f)
                    {
                        float a_FrostAmt = 1f * (1f - (m_InGameTimer / 120f));
                        if (a_FrostAmt >= 1f)
                            a_FrostAmt = 1f;

                        Camera.main.GetComponent<FrostEffect>().FrostAmount = a_FrostAmt;

                        if (m_InGameTimer <= 0.0f)
                        {
                            //Game Over Ã³¸®
                            if (PhotonNetwork.IsMasterClient)
                                SendGState(GameState.Gs_GameEnd);
                        }
                    }
                }
                break;
            case GameState.Gs_GameEnd:
                {
                    ShowGameEndPanel();
                }
                break;
        }   
    }

    void ShowGameEndPanel()
    {
        m_GameState = GameState.Gs_GameExit;
        m_GameEndPanel.SetActive(true);
        if (PhotonNetwork.IsMasterClient == false)
            m_ExitBtn.gameObject.SetActive(false);

        if (m_WinTeam == -1)
            m_GameEndInfoTxt.text = "¹«½ÂºÎ";
        else if (m_WinTeam == 0)
            m_GameEndInfoTxt.text = "RedÆÀ ½Â¸®";
        else
            m_GameEndInfoTxt.text = "BlueÆÀ ½Â¸®";
    }

    void ExitBtn_Click()
    {
        StartCoroutine(LoadRoomScene());
    }

    IEnumerator LoadRoomScene()
    {
        PhotonNetwork.LoadLevel("RoomScene");
        yield return null;
    }

    public void ConfigBtn_Click()
    {
        if (m_Config_Pop == null)
            m_Config_Pop = Resources.Load("Prefabs/ConfigPanel") as GameObject;

        GameObject a_Config_Pop = (GameObject)Instantiate(m_Config_Pop);
        a_Config_Pop.transform.SetParent(Popup_Canvas.transform, false);
    }


    public void CreatePlayer(Vector3 pos)
    {
        GameObject MyPlayer = PhotonNetwork.Instantiate("Player", pos, Quaternion.identity, 0);
        MyPlayer.name = "MyPlayer";
        Camera.main.GetComponent<CameraCtrl>().Player = MyPlayer;
    }

    public void CastingBar(bool IsActive, string SkillName = "", float CurTime = 0f, float MaxTime = 0f)
    {
        if (IsActive)
        {
            m_CastingBar.fillAmount = CurTime / MaxTime;
            m_CastingTxt.text = string.Format("{0} ({1:0.0} / {2:0.0})ÃÊ", SkillName, CurTime, MaxTime);
        }
        m_CastingObj.SetActive(IsActive);
    }

    bool IsGamePossible() // °ÔÀÓÀÌ °¡´ÉÇÑ »óÅÂÀÎÁö? Ã¼Å©ÇÏ´Â ÇÔ¼ö
    {
        // ³ª°¡´Â Å¸ÀÌ¹Ö¿¡ Æ÷Åæ Á¤º¸µéÀÌ ÇÑ ÇÁ·¹ÀÓ ¸ÕÀú »ç¶óÁö°í LoadScene()ÀÌ ÇÑ ÇÁ·¹ÀÓ ´Ê°Ô È£ÃâµÇ´Â ¹®Á¦ ÇØ°á¹ý
        if (PhotonNetwork.CurrentRoom == null || PhotonNetwork.LocalPlayer == null)
            return false;

        m_GameState = ReceiveGState();

        return true;
    }

    void GameEndCheck()
    {
        int MyTeam = Camera.main.GetComponent<CameraCtrl>().Player.GetComponent<PlayerCtrl>().m_MyTeam;
        PlayerState MySts = Camera.main.GetComponent<CameraCtrl>().Player.GetComponent<PlayerCtrl>().m_CurStatus;
        GameObject[] ArrMyPlayer = GameObject.FindGameObjectsWithTag("Player").Where(player => player.GetComponent<PlayerCtrl>().IsMyTeam(MyTeam)).ToArray();
        GameObject[] ArrEnPlayer = GameObject.FindGameObjectsWithTag("Player").Where(player => !player.GetComponent<PlayerCtrl>().IsMyTeam(MyTeam)).ToArray();

        if (MySts == PlayerState.Die) // MasterClient ±âÁØÀ¸·Î Ã¼Å©ÇÏ±â
        {
            if (ArrMyPlayer.Where(player => player.GetComponent<PlayerCtrl>().m_CurStatus != PlayerState.Die).Count() > 0)
                return;
            else
            {
                m_WinTeam = MyTeam == 0 ? 1 : 0;
                SendGState(GameState.Gs_GameEnd);
            }
        }
        else
        {
            if (ArrEnPlayer.Where(player => player.GetComponent<PlayerCtrl>().m_CurStatus != PlayerState.Die).Count() > 0)
                return;
            else
            {
                m_WinTeam = MyTeam == 0 ? 0 : 1;
                SendGState(GameState.Gs_GameEnd);
            }
        }
    }

    bool GameStartObserver()
    {
        bool IsStart = false;

        if (0f < m_GoWaitGame)
        {
            m_GoWaitGame -= Time.deltaTime;
            if (!ReferenceEquals(m_WaitTmText, null))
            {
                m_WaitTmText.gameObject.SetActive(true);
                m_WaitTmText.text = ((int)m_GoWaitGame).ToString();
            }

            if (m_GoWaitGame <= 0f) // ÀÌ°Ç ÇÑ¹ø¸¸ ¹ß»ýÇÒ °ÍÀÌ´Ù.
            {
                m_RoundCnt++;
                m_WaitTmText.gameObject.SetActive(false);
                m_GoWaitGame = 0f;
                IsStart = true;
            }

            // Photon µô·¹ÀÌ ¶§¹®ÀÎÁö ¿©±â¼­ ÃÊ±âÈ­
            m_GameState = GameState.GS_Ready;
            m_GameEndPanel.SetActive(false);
        }

        return IsStart;
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        //·ÎÄÃ ÇÃ·¹ÀÌ¾îÀÇ À§Ä¡ Á¤º¸ ¼Û½Å
        if (stream.IsWriting)
        {
            stream.SendNext(m_InGameTimer);
        }
        else //¿ø°Ý ÇÃ·¹ÀÌ¾îÀÇ À§Ä¡ Á¤º¸ ¼ö½Å
        {
            m_InGameTimer = (float)stream.ReceiveNext();
        }
    }

    #region -- °ÔÀÓ »óÅÂ µ¿±âÈ­ Ã³¸®

    void InitGStateProps()
    {
        if (PhotonNetwork.CurrentRoom == null)
            return;

        m_StateProps.Clear();
        m_StateProps.Add("GameState", (int)GameState.GS_Ready);
        PhotonNetwork.CurrentRoom.SetCustomProperties(m_StateProps);

        m_WinTeamProps.Clear();
        m_WinTeamProps.Add("WinTeam", -1);
        PhotonNetwork.CurrentRoom.SetCustomProperties(m_WinTeamProps);
    }

    void SendGState(GameState a_GState)
    {
        if (m_StateProps == null)
        {
            m_StateProps = new ExitGames.Client.Photon.Hashtable();
            m_StateProps.Clear();
        }

        if (m_StateProps.ContainsKey("GameState") == true)
            m_StateProps["GameState"] = (int)a_GState;
        else
            m_StateProps.Add("GameState", (int)a_GState);

        if (a_GState == GameState.Gs_GameEnd)
            SendWinTeamProps(m_WinTeam);

        PhotonNetwork.CurrentRoom.SetCustomProperties(m_StateProps);
    }

    GameState ReceiveGState() // GameState ¹Þ¾Æ¼­ Ã³¸®ÇÏ´Â ºÎºÐ
    {
        GameState a_GS = GameState.GS_Ready;

        if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey("GameState") == true)
            a_GS = (GameState)PhotonNetwork.CurrentRoom.CustomProperties["GameState"];

        if (a_GS == GameState.Gs_GameEnd)
            m_WinTeam = ReceiveWinTeam();

        return a_GS;
    }

    void SendWinTeamProps(int a_WinTeam)
    {
        if (m_WinTeamProps == null)
        {
            m_WinTeamProps = new ExitGames.Client.Photon.Hashtable();
            m_WinTeamProps.Clear();
        }

        if (m_WinTeamProps.ContainsKey("WinTeam") == true)
            m_WinTeamProps["WinTeam"] = a_WinTeam;
        else
            m_WinTeamProps.Add("WinTeam", a_WinTeam);

        PhotonNetwork.CurrentRoom.SetCustomProperties(m_WinTeamProps);
    }

    int ReceiveWinTeam() // GameState ¹Þ¾Æ¼­ Ã³¸®ÇÏ´Â ºÎºÐ
    {
        int a_WinTeam = -1;

        if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey("WinTeam") == true)
            a_WinTeam = (int)PhotonNetwork.CurrentRoom.CustomProperties["WinTeam"];

        return a_WinTeam;
    }
    #endregion
}
