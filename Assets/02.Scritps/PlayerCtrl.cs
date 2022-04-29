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

    // Animator ���� ���� 
    [HideInInspector] public Animator m_Anim;
    [HideInInspector] public AnimState m_CurAnimState = AnimState.Idle;
    AnimState m_NetAnimState = AnimState.Idle;

    public PlayerState m_CurStatus = PlayerState.Idle;
    public float m_BowMvSpeed = 150.0f; // �������� �ּ� �̵��ӵ� (�ִ� 300)

    [HideInInspector] public Transform tr;
    public Vector3 moveDir = Vector3.zero;
    private float moveSpeed = 150.0f;
    private const float rotSpeed = 3.0f;

    private CharacterController controller = null;

    // Hp ����
    public Image m_HpBarImg = null;
    [HideInInspector] public float m_CurHp = 200.0f;
    [HideInInspector] public float m_MaxHp = 200.0f;
    [HideInInspector] public float m_NetHp = 200.0f;

    ExitGames.Client.Photon.Hashtable SnowCntProps = new ExitGames.Client.Photon.Hashtable();
    [HideInInspector] public int m_CurSnowCnt = 10; // ���� ������ �ִ� ������ ����
    [HideInInspector] public int m_MaxSnowCnt = 10; // �ִ� ������ �ִ� ������ ����

    // ��ġ ������ �ۼ����� �� ����� ���� ���� �� �ʱ갪 ����
    private Vector3 m_CurPos = Vector3.zero;
    private Quaternion m_CurRot = Quaternion.identity;

    // PhotonView ������Ʈ�� �Ҵ��� ����
    [HideInInspector] public PhotonView pv = null;

    public Text m_NickName;

    void Awake()
    {
        m_MaxSnowCnt = 10;
        m_CurSnowCnt = 10;

        // ������Ʈ �Ҵ�
        tr = GetComponent<Transform>();

        // PhotonView ������Ʈ �Ҵ�
        pv = GetComponent<PhotonView>();

        pv.ObservedComponents[0] = this;

        m_Anim = GetComponentInChildren<Animator>();

        // ��ġ �� ȸ�� ���� ó���� ������ �ʱⰪ ����
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
        else // Photon Client ����ȭ
        {
            NetworkTransform_Update();
            NetworkCurHp_Update();
            NetworkAnimState_Update();
            NetworkSnowCnt_Update();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Hit : " + other.gameObject.name);
        if (other.gameObject.name.Contains("SnowBall"))
        {
            SnowBallCtrl a_Snow = other.gameObject.GetComponent<SnowBallCtrl>();
            if (a_Snow != null)
            {
                if (IsMyTeam(a_Snow.SnowData.AttackerTeam) == false)
                {
                    GetDamage(a_Snow.SnowData.m_Attck, a_Snow.SnowData.AttackerId);
                    a_Snow.DestroyThisObj();
                }
            }
        }
        else if (other.gameObject.name.Contains("SnowBowling"))
        {
            SnowBowlingCtrl a_Snow = other.gameObject.GetComponent<SnowBowlingCtrl>();
            if (a_Snow != null)
            {
                if (IsMyTeam(a_Snow.SnowData.AttackerTeam) == false)
                {
                    GetDamage(a_Snow.SnowData.m_Attck, a_Snow.SnowData.AttackerId);
                    a_Snow.DestroyThisObj();
                }
            }
        }
    }

    public void Init()
    {
        m_CurHp = m_MaxHp;
        moveSpeed = 150f;
        controller = GetComponent<CharacterController>();

        if (pv != null)
        {
            m_PlayerId = pv.Owner.ActorNumber;

            if (pv.Owner.CustomProperties.ContainsKey("MyTeam") == true)
                m_MyTeam = (int)pv.Owner.CustomProperties["MyTeam"];

            if (m_MyTeam == 0)
            {
                m_NickName.text = "Red�� : " + m_PlayerId.ToString();
                m_NickName.color = Color.red;
            }
            else
            {
                m_NickName.text = "Blue�� : " + m_PlayerId.ToString();
                m_NickName.color = Color.blue;
            }

            if (pv.IsMine == true)
            {
                SnowCntProps.Clear();
                SnowCntProps.Add("SnowCnt", 0);
                pv.Owner.SetCustomProperties(SnowCntProps);
            }
        }
    }

    public bool IsMyTeam(int a_Team)
    {
        bool IsMyTeam = false;
        if (pv.IsMine == false)
            return IsMyTeam;

        if (a_Team == m_MyTeam)
            IsMyTeam = true;

        return IsMyTeam;
    }

    public void GetDamage(float a_Dmg, int a_AttackerId)
    {
        if (pv.IsMine == false)
            return;

        if (m_PlayerId == a_AttackerId)
            return;

        if (m_CurHp <= 0f)
            return;

        m_CurHp -= a_Dmg;
        m_HpBarImg.fillAmount = (float)m_CurHp / (float)m_MaxHp;

        if (m_HpBarImg.fillAmount <= 0.4f)
            m_HpBarImg.color = Color.red;
        else if (m_HpBarImg.fillAmount <= 0.6f)
            m_HpBarImg.color = Color.yellow;
        else
            m_HpBarImg.color = Color.green;

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

    public void SetJoyStickMv(float a_JoyMvLen, Vector3 a_JoyMvDir)
    {
        if (0f < a_JoyMvLen)
            moveDir = (Vector3.forward * a_JoyMvDir.y) + (Vector3.right * a_JoyMvDir.x);
        else
            moveDir = Vector3.zero;
    }

    public bool IsAction()
    {
        if (m_prevState != null && !string.IsNullOrEmpty(m_prevState))
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

        if (m_prevState != null && !string.IsNullOrEmpty(m_prevState))
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

        m_prevState = newAnim.ToString(); // ����������Ʈ�� ���罺����Ʈ ����
        m_CurAnimState = newAnim;
    }

    private void Move_Update()
    {
        if (GameMgr.Inst.m_GameState != GameState.GS_Playing) // �������� �ƴϸ�
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
            if (controller != null)
                controller.SimpleMove(tr.rotation * Vector3.forward * m_BowMvSpeed * 3f * Time.deltaTime);

            MySetAnim(AnimState.Run);
        }
        else
        {
            if (controller != null && moveDir != Vector3.zero)
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
        // �߰����(��ġ, ȸ����) ���� ����
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
        //���� �÷��̾��� ��ġ ���� �۽�
        if (stream.IsWriting)
        {
            stream.SendNext(tr.position);
            stream.SendNext(tr.rotation);

            stream.SendNext(m_CurHp);

            stream.SendNext(m_CurAnimState);
        }
        else //���� �÷��̾��� ��ġ ���� ����
        {
            m_CurPos = (Vector3)stream.ReceiveNext();
            m_CurRot = (Quaternion)stream.ReceiveNext();

            m_NetHp = (float)stream.ReceiveNext();

            m_NetAnimState = (AnimState)stream.ReceiveNext();
        }
    }

    #region --- CustomProperties SnowCnt �ʱ�ȭ
    public void SendSnowCnt(int a_CurSnowCnt = 0)
    {
        if (pv == null || pv.IsMine == false)
            return;

        if (SnowCntProps == null)
        {
            SnowCntProps = new ExitGames.Client.Photon.Hashtable();
            SnowCntProps.Clear();
        }

        if (SnowCntProps.ContainsKey("SnowCnt") == true)
            SnowCntProps["SnowCnt"] = a_CurSnowCnt;
        else
            SnowCntProps.Add("SnowCnt", a_CurSnowCnt);

        pv.Owner.SetCustomProperties(SnowCntProps);
    }

    void NetworkSnowCnt_Update()
    {
        if (pv == null || pv.Owner == null)
            return;

        if (pv.Owner.CustomProperties.ContainsKey("SnowCnt") == true)
        {
            int a_CurSnowCnt = (int)pv.Owner.CustomProperties["SnowCnt"];
            if (m_CurSnowCnt != a_CurSnowCnt)
            {
                m_CurSnowCnt = a_CurSnowCnt;
            }
        }
    }
    #endregion
}