using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public enum GameState
{
    GS_Ready = 0,
    GS_Playing,
    Gs_GameEnd,
}

public class GameMgr : MonoBehaviourPunCallbacks, IPunObservable
{
    // RPC È£­„À» À§ÇÑ PhotonView
    private PhotonView pv;

    public GameObject Popup_Canvas = null;
    public Button m_ConfigBtn;
    public GameObject m_Config_Pop;
    public Text m_SnowCntText;

    public GameObject m_CastingObj;
    public Image m_CastingBar;
    public Text m_CastingTxt;

    public GameState m_GameState = GameState.GS_Ready;
    ExitGames.Client.Photon.Hashtable m_StateProps = new ExitGames.Client.Photon.Hashtable();
    public static GameMgr Inst;

    private int m_RoundCnt = 0;

    [Header("--- StartTimer UI ---")]
    public Text m_WaitTmText; // °ÔÀÓ ½ÃÀÛÈÄ Ä«¿îÆ® 3, 2, 1, 0
    [HideInInspector] float m_GoWaitGame = 4f; // °ÔÀÓ ½ÃÀÛÈÄ Ä«¿îÆ® Text UI

    // °ÔÀÓ Å¸ÀÌ¸Ó
    public Text m_InGameTmText; // 180ÃÊ °ÔÀÓ Å¸ÀÌ¸Ó
    [HideInInspector] public float m_InGameTimer = 180f; // 0ÃÊ°¡ µÇ¸é °ÔÀÓ Á¾·á

    void Awake()
    {
        Inst = this;

        m_GameState = GameState.GS_Ready;
        m_InGameTimer = 180f;

        // PhotonView ÄÄÆ÷³ÍÆ® ÇÒ´ç
        pv = GetComponent<PhotonView>();

        //¸ðµç Å¬¶ó¿ìµåÀÇ ³×Æ®¿öÅ© ¸Þ½ÃÁö ¼ö½ÅÀ» ´Ù½Ã ¿¬°á

        InitGStateProps();
    }

    // Start is called before the first frame update
    void Start()
    {
        PhotonNetwork.IsMessageQueueRunning = true;

        if (!ReferenceEquals(m_ConfigBtn, null))
            m_ConfigBtn.onClick.AddListener(ConfigBtn_Click);
    }

    // Update is called once per frame
    void Update()
    {
        m_SnowCntText.text = "x " + Camera.main.GetComponent<CameraCtrl>().Player.GetComponent<PlayerCtrl>().m_CurSnowCnt.ToString();
        m_InGameTmText.text = m_InGameTimer.ToString("0.0");

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
                        m_InGameTimer -= Time.deltaTime;

                    if (m_InGameTimer <= 120f)
                    {
                        float a_FrostAmt = 1f * (1f - (m_InGameTimer / 120f));
                        if (a_FrostAmt >= 1f)
                            a_FrostAmt = 1f;

                        Camera.main.GetComponent<FrostEffect>().FrostAmount = a_FrostAmt;

                        if (m_InGameTimer <= 0.0f)
                        {
                            //Game Over Ã³¸®
                            if (PhotonNetwork.IsMasterClient == true)
                            {
                                SendGState(GameState.Gs_GameEnd);    
                            }
                        }
                    }
                }
                break;
            case GameState.Gs_GameEnd:
                {
                    if (PhotonNetwork.IsMasterClient)
                        StartCoroutine(LoadRoomScene());
                }
                break;
        }   
    }

    public void ConfigBtn_Click()
    {
        if (m_Config_Pop == null)
            m_Config_Pop = Resources.Load("Prefabs/ConfigPanel") as GameObject;

        GameObject a_Config_Pop = (GameObject)Instantiate(m_Config_Pop);
        a_Config_Pop.transform.SetParent(Popup_Canvas.transform, false);
    }

    IEnumerator LoadRoomScene() //ÃÖÁ¾ InGame ¾À ·Îµù
    {
        PhotonNetwork.LoadLevel("RoomScene");
        PhotonNetwork.AutomaticallySyncScene = false;
        yield return null;
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

        PhotonNetwork.CurrentRoom.SetCustomProperties(m_StateProps);
    }

    GameState ReceiveGState() // GameState ¹Þ¾Æ¼­ Ã³¸®ÇÏ´Â ºÎºÐ
    {
        GameState a_GS = GameState.GS_Ready;

        if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey("GameState") == true)
            a_GS = (GameState)PhotonNetwork.CurrentRoom.CustomProperties["GameState"];

        return a_GS;
    }

    #endregion
}
