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
    // RPC ȣ���� ���� PhotonView
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
    public Text m_WaitTmText; // ���� ������ ī��Ʈ 3, 2, 1, 0
    [HideInInspector] float m_GoWaitGame = 4f; // ���� ������ ī��Ʈ Text UI

    // ���� Ÿ�̸�
    public Text m_InGameTmText; // 180�� ���� Ÿ�̸�
    [HideInInspector] public float m_InGameTimer = 180f; // 0�ʰ� �Ǹ� ���� ����

    void Awake()
    {
        Inst = this;

        m_GameState = GameState.GS_Ready;
        m_InGameTimer = 180f;

        // PhotonView ������Ʈ �Ҵ�
        pv = GetComponent<PhotonView>();

        //��� Ŭ������ ��Ʈ��ũ �޽��� ������ �ٽ� ����

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
                    if (PhotonNetwork.IsMasterClient) // ������ Ŭ���̾�Ʈ��
                    {
                        SendGState(GameState.GS_Playing);
                    }
                }
                break;
            case GameState.GS_Playing:
                {
                    // ��Ʈ��ũ �������� �ʰ� ������ ��찡 ���� m_WaitTmText Active ����
                    if (m_WaitTmText.gameObject.activeSelf)
                        m_WaitTmText.gameObject.SetActive(false);

                    if (PhotonNetwork.IsMasterClient) // ������ Ŭ���̾�Ʈ��
                        m_InGameTimer -= Time.deltaTime;

                    if (m_InGameTimer <= 120f)
                    {
                        float a_FrostAmt = 1f * (1f - (m_InGameTimer / 120f));
                        if (a_FrostAmt >= 1f)
                            a_FrostAmt = 1f;

                        Camera.main.GetComponent<FrostEffect>().FrostAmount = a_FrostAmt;

                        if (m_InGameTimer <= 0.0f)
                        {
                            //Game Over ó��
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

    IEnumerator LoadRoomScene() //���� InGame �� �ε�
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
            m_CastingTxt.text = string.Format("{0} ({1:0.0} / {2:0.0})��", SkillName, CurTime, MaxTime);
        }
        m_CastingObj.SetActive(IsActive);
    }

    bool IsGamePossible() // ������ ������ ��������? üũ�ϴ� �Լ�
    {
        // ������ Ÿ�ֿ̹� ���� �������� �� ������ ���� ������� LoadScene()�� �� ������ �ʰ� ȣ��Ǵ� ���� �ذ��
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

            if (m_GoWaitGame <= 0f) // �̰� �ѹ��� �߻��� ���̴�.
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
        //���� �÷��̾��� ��ġ ���� �۽�
        if (stream.IsWriting)
        {
            stream.SendNext(m_InGameTimer);
        }
        else //���� �÷��̾��� ��ġ ���� ����
        {
            m_InGameTimer = (float)stream.ReceiveNext();
        }
    }

    #region -- ���� ���� ����ȭ ó��

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

    GameState ReceiveGState() // GameState �޾Ƽ� ó���ϴ� �κ�
    {
        GameState a_GS = GameState.GS_Ready;

        if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey("GameState") == true)
            a_GS = (GameState)PhotonNetwork.CurrentRoom.CustomProperties["GameState"];

        return a_GS;
    }

    #endregion
}
