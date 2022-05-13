using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public enum SnowManState
{
    Idle,
    Trace,  // 쫓아가기
    Return, // 되돌아가기
}

public class SnowManCtrl : MonoBehaviour, IPunObservable, IDamage
{
    private SnowData m_SnowData = new SnowData();
    public SnowData SnowData
    {
        get { return m_SnowData; }
        set { m_SnowData = value; }
    }

    public SnowManState m_CurState = SnowManState.Idle;

    private CapsuleCollider _collider = null;

    // 첫 소환시 설정할 변수
    private Vector3 m_OrgPos;
    private Quaternion m_OrgRot;

    private Transform tr;
    private const float rotSpeed = 3.0f;

    private float m_TraceDist = 10.0f;
    private float m_AttDistance = 6.0f;
    private float m_AttCool = 1.5f;

    //---- Navigation
    protected NavMeshAgent m_NvAgent;
    protected NavMeshPath m_NvMvPath;

    // 위치 정보를 송수신할 때 사용할 변수 선언 및 초깃값 설정
    private Vector3 m_CurPos = Vector3.zero;
    private Quaternion m_CurRot = Quaternion.identity;

    // Hp 관련
    public Image m_HpBarImg = null;
    [HideInInspector] public float m_CurHp = 200.0f;
    [HideInInspector] public float m_MaxHp = 200.0f;
    [HideInInspector] public float m_NetHp = 200.0f;

    private GameObject[] m_EnemyPlayers;
    private GameObject m_TargetPlayer = null;

    public GameObject m_SnowBallPrefab;
    public GameObject m_TraceRangeObj;

    private PhotonView pv = null;

    void Awake()
    {
        pv = GetComponent<PhotonView>();
        pv.ObservedComponents[0] = this;

        tr = GetComponent<Transform>();

        m_CurPos = m_OrgPos = tr.position;
        m_CurRot = m_OrgRot = tr.rotation;

        m_MaxHp = 50;
        m_CurHp = m_MaxHp;
        m_NetHp = m_MaxHp;
    }
    
    // Start is called before the first frame update
    void Start()
    {
        Init();

        _collider = GetComponent<CapsuleCollider>();

        m_NvMvPath = new NavMeshPath();
        m_NvAgent = this.gameObject.GetComponent<NavMeshAgent>();
        m_NvAgent.updateRotation = false;
        m_NvAgent.speed = 0.0f;

        m_CurState = SnowManState.Idle;

        if (pv.IsMine)
        {
            GameObject[] Players = GameObject.FindGameObjectsWithTag("Player");
            if (Players.Length > 0)
                m_EnemyPlayers = Players.Where(player => player.GetComponent<PlayerCtrl>().IsMyTeam(m_SnowData.AttackerTeam) == false).ToArray();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (pv.IsMine)
        {
            FindNearPlayer();
            Move_Update();
        }
        else // Photon Client 동기화
        {
            NetworkTransform_Update();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.name.Contains("SnowBall") || other.gameObject.name.Contains("SnowBowling"))
        {
            IDamage IDmg = other.gameObject.GetComponent<IDamage>();
            if (!ReferenceEquals(IDmg, null))
            {
                if (IsMyTeam(IDmg.GetData().AttackerTeam) == false)
                    GetDamage(IDmg.GetData().m_Attck, IDmg.GetData().AttackerId);

                IDmg.DestroyThisObj();
            }
        }
    }

    IEnumerator DestroySnowWall(float tm)
    {
        yield return new WaitForSeconds(tm);

        // 충돌 콜백 함수가 발생하지 않도록 Collider를 비활성화
        if (!ReferenceEquals(_collider, null))
            _collider.enabled = false;

        Destroy(this.gameObject, 0.0f);
    }

    #region --- 눈덩이 던지기
    public void Shot()
    {
        if (m_SnowBallPrefab == null)
            return;

        Vector3 SnowPos = tr.position + tr.forward * 1.5f;
        SnowPos.y += 0.2f;
        Quaternion SnowRot = Quaternion.LookRotation(SnowPos - tr.position);

        ShotRPC(SnowPos, SnowRot);
        pv.RPC("ShotRPC", RpcTarget.Others, SnowPos, SnowRot);
    }

    [PunRPC]
    void ShotRPC(Vector3 a_Pos, Quaternion a_Rot)
    {
        GameObject a_SnowBall = GameObject.Instantiate(m_SnowBallPrefab, a_Pos, a_Rot);
        a_SnowBall.GetComponent<SnowBallCtrl>().SnowData.AttackerId = m_SnowData.AttackerId;
        a_SnowBall.GetComponent<SnowBallCtrl>().SnowData.AttackerTeam = m_SnowData.AttackerTeam;
    }
    #endregion

    #region --- 공격 범위 표시
    private void AttRangeOnOff(bool a_IsFind)
    {
        if (m_TraceRangeObj.activeSelf == a_IsFind)
            return;

        AttRangeOnOffRPC(a_IsFind);
        pv.RPC("AttRangeOnOffRPC", RpcTarget.Others, a_IsFind);
    }

    [PunRPC]
    public void AttRangeOnOffRPC(bool a_IsFind)
    {
        m_CurState = SnowManState.Trace;
        m_TraceRangeObj.SetActive(a_IsFind);
    }
    #endregion

    private void FindNearPlayer()
    {
        bool a_IsFind = false;
        if (m_EnemyPlayers == null)
            return;

        if (m_TargetPlayer == null)
        {
            if (m_EnemyPlayers.Length > 0)
            {
                GameObject a_NearEnemy = m_EnemyPlayers.OrderBy(player => Vector3.Distance(tr.position, player.transform.position)).FirstOrDefault();
                if (Vector3.Distance(tr.position, a_NearEnemy.transform.position) < m_TraceDist)
                {
                    m_TargetPlayer = a_NearEnemy;
                    a_IsFind = true;
                }
                else
                    m_TargetPlayer = null;
            }
        }
        else
        {
            if (Vector3.Distance(tr.position, m_TargetPlayer.transform.position) > m_TraceDist)
                m_TargetPlayer = null;
            else
                a_IsFind = true;
        }

        AttRangeOnOff(a_IsFind);
    }

    private void Move_Update()
    {
        if (!ReferenceEquals(m_TargetPlayer, null))
        {
            Vector3 a_MoveDir = m_TargetPlayer.transform.position - tr.position;
            a_MoveDir.y = 0.0f;
            Quaternion targetRot = Quaternion.LookRotation(a_MoveDir);

            if (a_MoveDir.magnitude <= m_AttDistance + 0.01f) // 타겟에 어느정도 가까워졌다고 판단
            {
                tr.rotation = Quaternion.RotateTowards(tr.rotation, targetRot, 360 * rotSpeed * Time.deltaTime);
                Attack_Update();
            }
            else
            {
                tr.rotation = Quaternion.RotateTowards(tr.rotation, targetRot, 360 * rotSpeed * Time.deltaTime);

                m_NvAgent.stoppingDistance = m_AttDistance;
                m_NvAgent.destination = m_TargetPlayer.transform.position;
                m_NvAgent.speed = 2.0f;
            }
        }
        else
        {
            Vector3 a_MoveDir = m_OrgPos - tr.position;
            a_MoveDir.y = 0.0f;
            Quaternion targetRot = Quaternion.LookRotation(a_MoveDir);

            if (a_MoveDir.magnitude <= 1.5f) // 도착했다고 생각하여 원위치
            {
                if (m_CurState == SnowManState.Return)
                {
                    tr.rotation = Quaternion.RotateTowards(tr.rotation, m_OrgRot, 360 * rotSpeed * Time.deltaTime);
                    m_CurState = SnowManState.Idle;
                    m_NvAgent.speed = 0.0f;
                }
            }
            else
            {
                tr.rotation = Quaternion.RotateTowards(tr.rotation, targetRot, 360 * rotSpeed * Time.deltaTime);

                if (m_CurState == SnowManState.Trace)
                {
                    m_CurState = SnowManState.Return;
                    m_NvAgent.stoppingDistance = 0.0f;
                    m_NvAgent.destination = m_OrgPos;
                    m_NvAgent.speed = 2.0f;
                }
            }
        }
    }

    void Attack_Update()
    {
        m_AttCool -= Time.deltaTime;
        if (m_AttCool <= 0.0f)
        {
            Shot();
            m_AttCool = 1.5f;
        }
    }

    private void NetworkTransform_Update()
    {
        // 중계받은(위치, 회전값) 값을 적용
        if (10.0f < (tr.position - m_CurPos).magnitude)
            tr.position = m_CurPos;
        else
            tr.position = Vector3.Lerp(tr.position, m_CurPos, Time.deltaTime * 10.0f);

        tr.rotation = Quaternion.Slerp(tr.rotation, m_CurRot, Time.deltaTime * 10.0f);
    }

    void UpdateUI_CurHpBar()
    {
        if (0 < m_SnowData.m_CurHp)
        {
            m_HpBarImg.fillAmount = (float)m_SnowData.m_CurHp / (float)m_SnowData.m_MaxHp;

            if (m_SnowData.m_CurHp <= 0)
                m_SnowData.m_CurHp = 0;
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(tr.position);
            stream.SendNext(tr.rotation);
        }
        else //원격 플레이어의 위치 정보 수신
        {
            m_CurPos = (Vector3)stream.ReceiveNext();
            m_CurRot = (Quaternion)stream.ReceiveNext();
        }
    }
    public void Init()
    {
        m_SnowData.m_MaxHp = 50;
        m_SnowData.m_CurHp = m_SnowData.m_MaxHp;
        m_SnowData.m_Attck = 0;
    }

    public void DestroyThisObj(float tm = 0)
    {
        StartCoroutine(this.DestroySnowWall(0.0f));
    }

    public bool IsMyTeam(int a_Team)
    {
        bool IsMyTeam = false;

        if (a_Team == m_SnowData.AttackerTeam)
            IsMyTeam = true;

        return IsMyTeam;
    }

    public void GetDamage(float a_Dmg, int a_AttackerId)
    {
        m_SnowData.m_CurHp -= a_Dmg;
        UpdateUI_CurHpBar();
        if (m_SnowData.m_CurHp <= 0.0f)
            DestroyThisObj();
    }
    public SnowData GetData()
    {
        return SnowData;
    }
}
