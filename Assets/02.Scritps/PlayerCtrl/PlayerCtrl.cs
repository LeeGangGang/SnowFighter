using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public enum PlayerState
{
    Idle,
    HoldAction,
    Bowling,
    Catapult,
    Die
}

public enum AnimState
{
    Idle,
    Run,
    Shot,
    Walk,
    Hit,
    Die,
    Gather,
    count
}

public class PlayerCtrl : MonoBehaviourPunCallbacks, IPunObservable
{
    [HideInInspector] public int m_MyTeam = -1;
    [HideInInspector] public int m_PlayerId = -1;

    // Animator 관련 변수 
    [HideInInspector] public Animator m_Anim;
    [HideInInspector] public AnimState m_CurAnimState = AnimState.Idle;
    AnimState m_NetAnimState = AnimState.Idle;

    public PlayerState m_CurStatus = PlayerState.Idle;
    public float m_BowMvSpeed = 150.0f; // 눈굴리기 최소 이동속도 (최대 300)

    [HideInInspector] public Transform tr;
    public Vector3 moveDir = Vector3.zero;
    private float moveSpeed = 150.0f;
    private const float rotSpeed = 3.0f;

    private CharacterController controller = null;

    // Hp 관련
    public Image m_HpBarImg = null;
    [HideInInspector] public float m_CurHp = 200.0f;
    [HideInInspector] public float m_MaxHp = 200.0f;
    [HideInInspector] public float m_NetHp = 200.0f;

    [HideInInspector] public int m_CurSnowCnt = 10; // 현재 가지고 있는 눈덩이 갯수
    [HideInInspector] public int m_MaxSnowCnt = 10; // 최대 가질수 있는 눈덩이 갯수

    // 위치 정보를 송수신할 때 사용할 변수 선언 및 초깃값 설정
    private Vector3 m_CurPos = Vector3.zero;
    private Quaternion m_CurRot = Quaternion.identity;

    public GameObject m_DamageTxtPrefab;
    public Transform m_DamageCanvas;

    // PhotonView 컴포넌트를 할당할 변수
    [HideInInspector] public PhotonView pv = null;

    public Text m_NickName;

    void Awake()
    {
        m_MaxSnowCnt = 10;
        m_CurSnowCnt = 10;

        // 컴포넌트 할당
        tr = GetComponent<Transform>();

        // PhotonView 컴포넌트 할당
        pv = GetComponent<PhotonView>();

        pv.ObservedComponents[0] = this;

        m_Anim = GetComponentInChildren<Animator>();

        // 위치 및 회전 값을 처리할 변수의 초기값 설정
        m_CurPos = tr.position;
        m_CurRot = tr.rotation;
    }

    // Start is called before the first frame update
    void Start()
    {
        Init();
    }

    // Update is called once per frame
    void Update()
    {
        if (pv.IsMine)
        {
            Move_Update();
        }
        else // Photon Client 동기화
        {
            NetworkTransform_Update();
            NetworkCurHp_Update();
            NetworkAnimState_Update();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Hit : " + other.gameObject.name);
        if (other.CompareTag("SnowBall") || other.CompareTag("SnowBowling"))
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

    public void Init()
    {
        m_CurHp = m_MaxHp;
        moveSpeed = 150f;
        controller = GetComponent<CharacterController>();

        if (!ReferenceEquals(pv, null))
        {
            m_PlayerId = pv.Owner.ActorNumber;

            if (pv.Owner.CustomProperties.ContainsKey("MyTeam") == true)
                m_MyTeam = (int)pv.Owner.CustomProperties["MyTeam"];

            if (m_MyTeam == 0)
            {
                m_NickName.text = "Red팀 : " + m_PlayerId.ToString();
                m_NickName.color = Color.red;
            }
            else
            {
                m_NickName.text = "Blue팀 : " + m_PlayerId.ToString();
                m_NickName.color = Color.blue;
            }
        }
    }

    public bool IsMyTeam(int a_Team)
    {
        bool IsMyTeam = false;

        if (a_Team == m_MyTeam)
            IsMyTeam = true;

        return IsMyTeam;
    }

    public void GetDamage(float a_Dmg, int a_AttackerId)
    {
        if (m_PlayerId == a_AttackerId)
            return;

        if (m_CurHp <= 0f)
            return;

        SpawnDamageTxt(a_Dmg);

        if (pv.IsMine == false)
            return;

        m_CurHp -= a_Dmg;
        m_HpBarImg.fillAmount = (float)m_CurHp / (float)m_MaxHp;

        if (m_CurHp <= 0)
            m_CurHp = 0;

        if (m_CurHp <= 0)
        {
            MySetAnim(AnimState.Die);
            m_CurStatus = PlayerState.Die;
            controller.enabled = false;
        }
        else
        {
            MySetAnim(AnimState.Hit);
        }
    }

    public void SpawnDamageTxt(float dmg)
    {
        if (m_DamageTxtPrefab != null && m_DamageCanvas != null)
        {
            //DamageRoot
            GameObject m_DamageObj = (GameObject)Instantiate(m_DamageTxtPrefab);
            m_DamageObj.transform.SetParent(m_DamageCanvas, false);
            m_DamageObj.transform.localPosition = new Vector3(0f, 0.85f, 0f);
            DamageTxtCtrl a_DamageTx = m_DamageObj.GetComponentInChildren<DamageTxtCtrl>();
            a_DamageTx.m_DamageVal = (int)dmg;
        }
    }

    public void SetJoyStickMv(float a_JoyMvLen, Vector3 a_JoyMvDir)
    {
        if (0f < a_JoyMvLen)
            moveDir = (Vector3.forward * a_JoyMvDir.y) + (Vector3.right * a_JoyMvDir.x);
        else
            moveDir = Vector3.zero;
    }

    public bool IsAction()
    {
        if (!ReferenceEquals(m_prevState, null) && !string.IsNullOrEmpty(m_prevState))
        {
            if (m_prevState.ToString() == AnimState.Hit.ToString() ||
                m_prevState.ToString() == AnimState.Die.ToString() ||
                m_prevState.ToString() == AnimState.Shot.ToString() ||
                m_prevState.ToString() == AnimState.Gather.ToString())
            {
                return true;
            }
        }
        return false;
    }

    public string m_prevState = "";
    public void MySetAnim(AnimState newAnim)
    {
        if (m_Anim == null)
            return;

        if (!ReferenceEquals(m_prevState, null) && !string.IsNullOrEmpty(m_prevState))
        {
            if (m_prevState.ToString() == newAnim.ToString())
                return;
        }

        if (!string.IsNullOrEmpty(m_prevState))
        {
            m_Anim.ResetTrigger(m_prevState.ToString());
            m_prevState = null;
        }
        
        m_Anim.SetTrigger(newAnim.ToString());

        m_prevState = newAnim.ToString(); // 이전스테이트에 현재스테이트 저장
        m_CurAnimState = newAnim;
    }

    private void Move_Update()
    {
        if (GameMgr.Inst.m_GameState != GameState.GS_Playing) // 게임중이 아니면
            return;

        if (m_CurStatus == PlayerState.HoldAction || m_CurStatus == PlayerState.Die)
            return;

        if (100f < tr.position.y)
        {
            float pos = Random.Range(-100f, 100f);
            tr.position = new Vector3(pos, 1.5f, pos);
            return;
        }

        if (m_CurStatus == PlayerState.Bowling)
        {
            // the vector that we want to measure an angle from
            Vector3 referenceForward = tr.forward; /* some vector that is not Vector3.up */
            // the vector perpendicular to referenceForward (90 degrees clockwise)
            // (used to determine if angle is positive or negative)
            Vector3 referenceRight = Vector3.Cross(Vector3.up, referenceForward);
            // the vector of interest
            Vector3 newDirection = moveDir;/* some vector that we're interested in */
            // Get the angle in degrees between 0 and 180
            float angle = Vector3.Angle(newDirection, referenceForward);
            // Determine if the degree value should be negative.  Here, a positive value
            // from the dot product means that our vector is on the right of the reference vector   
            // whereas a negative value means we're on the left.
            float sign = Mathf.Sign(Vector3.Dot(newDirection, referenceRight));
            float finalAngle = sign * angle;

            float a_MoveAngle = 0f;
            if (finalAngle > 0)
                a_MoveAngle = 1f;
            else if (finalAngle < 0)
                a_MoveAngle = -1f;

            Vector3 a_Rot = tr.rotation.eulerAngles;
            a_Rot.y += (a_MoveAngle * 15f * rotSpeed * Time.deltaTime);
            if (a_Rot.y > 180f)
                a_Rot.y -= 360f;

            Quaternion targetRot = Quaternion.Euler(a_Rot);
            tr.rotation = Quaternion.RotateTowards(tr.rotation, targetRot, 360 * rotSpeed * Time.deltaTime);
            if (!ReferenceEquals(controller, null))
                controller.SimpleMove(tr.rotation * Vector3.forward * m_BowMvSpeed * 3f * Time.deltaTime);

            MySetAnim(AnimState.Run);
        }
        if (m_CurStatus == PlayerState.Catapult)
        {
            if (moveDir != Vector3.zero)
            {
                Transform a_ProjectorTr = this.GetComponent<PlayerSkillCtrl>().m_Projector.transform;
                a_ProjectorTr.Translate(moveDir * Time.deltaTime * 10f);

                Quaternion a_Rot = Quaternion.LookRotation(a_ProjectorTr.position);
                a_Rot.x = 0f;
                a_Rot.z = 0f;
                tr.transform.rotation = a_Rot;
            }
        }
        else
        {
            if (!ReferenceEquals(controller, null) && moveDir != Vector3.zero)
            {
                Quaternion targetRot = Quaternion.LookRotation(moveDir);
                tr.rotation = Quaternion.RotateTowards(tr.rotation, targetRot, 360 * rotSpeed * Time.deltaTime);
                controller.SimpleMove(tr.rotation * Vector3.forward * moveSpeed * 3f * Time.deltaTime);
                
                MySetAnim(AnimState.Run);
            }
            else
            {
                if (IsAction() == false)
                    MySetAnim(AnimState.Idle);
            }
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

    void NetworkCurHp_Update()
    {
        if (0 < m_CurHp)
        {
            m_CurHp = m_NetHp;
            m_HpBarImg.fillAmount = (float)m_CurHp / (float)m_MaxHp;

            if (m_HpBarImg.fillAmount <= 0.4f)
                m_HpBarImg.color = Color.red;
            else if (m_HpBarImg.fillAmount <= 0.6f)
                m_HpBarImg.color = Color.yellow;
            else
                m_HpBarImg.color = Color.green;

            if (m_CurHp <= 0)
                m_CurHp = 0;
        }
    }

    private void NetworkAnimState_Update()
    {
        MySetAnim(m_NetAnimState);
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        //로컬 플레이어의 위치 정보 송신
        if (stream.IsWriting)
        {
            stream.SendNext(tr.position);
            stream.SendNext(tr.rotation);

            stream.SendNext(m_CurHp);

            stream.SendNext(m_CurAnimState);
        }
        else //원격 플레이어의 위치 정보 수신
        {
            m_CurPos = (Vector3)stream.ReceiveNext();
            m_CurRot = (Quaternion)stream.ReceiveNext();

            m_NetHp = (float)stream.ReceiveNext();

            m_NetAnimState = (AnimState)stream.ReceiveNext();
            if (m_NetAnimState == AnimState.Die)
                m_CurStatus = PlayerState.Die;
        }
    }
}