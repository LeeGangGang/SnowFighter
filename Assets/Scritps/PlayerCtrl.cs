using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class PlayerCtrl : MonoBehaviourPunCallbacks, IPunObservable, IDamageCtrl
{
    private Transform tr;
    private Vector3 moveDir = Vector3.zero;
    private float moveSpeed = 150.0f;
    private const float rotSpeed = 3.0f;

    private CharacterController controller = null;

    // Hp ���� ---------------------
    public Image m_HpBarImg = null;
    public float m_CurHp = 200.0f;
    public float m_MaxHp = 200.0f;
    // -----------------------------

    ExitGames.Client.Photon.Hashtable SnowCntProps = new ExitGames.Client.Photon.Hashtable();
    public int m_CurSnowCnt = 0; // ���� ������ �ִ� ������ ����
    public int m_MaxSnowCnt = 3; // �ִ� ������ �ִ� ������ ����

    //��ġ ������ �ۼ����� �� ����� ���� ���� �� �ʱ갪 ����
    private Vector3 m_CurPos = Vector3.zero;
    private Quaternion m_CurRot = Quaternion.identity;

    //PhotonView ������Ʈ�� �Ҵ��� ����
    [HideInInspector] public PhotonView pv = null;

    void Awake()
    {
        //������Ʈ �Ҵ�
        tr = GetComponent<Transform>();

        //PhotonView ������Ʈ �Ҵ�
        pv = GetComponent<PhotonView>();

        pv.ObservedComponents[0] = this;

        //���� ��ũ�� ��ġ �� ȸ�� ���� ó���� ������ �ʱⰪ ����
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
        TransformNetwork();

        ReceiveSnowCnt();
    }

    public void Init()
    {
        m_CurHp = m_MaxHp;
        moveSpeed = 150.0f;
        controller = GetComponent<CharacterController>();

        if (pv != null && pv.IsMine == true)
        {
            SnowCntProps.Clear();
            SnowCntProps.Add("SnowCnt", 0);
            pv.Owner.SetCustomProperties(SnowCntProps);
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

    void ReceiveSnowCnt()
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

    public float SetDamage()
    {
        throw new System.NotImplementedException();
    }

    public void DestroyThisObj()
    {
        throw new System.NotImplementedException();
    }

    public void GetDamage(float a_Dmg)
    {
        if (pv.IsMine)
            return;

        pv.RPC("GetDamageRPC", RpcTarget.All, a_Dmg);
    }

    [PunRPC]
    public void GetDamageRPC(float a_Dmg)
    {
        if (m_CurHp <= 0.0f)
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
            Debug.Log("����");
        }
    }

    public void SetJoyStickMv(float a_JoyMvLen, Vector3 a_JoyMvDir)
    {
        if (0.0f < a_JoyMvLen)
            moveDir = (Vector3.forward * a_JoyMvDir.y) + (Vector3.right * a_JoyMvDir.x);
        else
            moveDir = Vector3.zero;
    }

    private void TransformNetwork()
    {
        if (pv.IsMine)
        {  //�ڽ��� ���� ��Ʈ��ũ ���ӿ�����Ʈ�� ��쿡�� Ű���� ���� ��ƾ ����
            if (100.0f < tr.position.y)
            {
                float pos = Random.Range(-100.0f, 100.0f);
                tr.position = new Vector3(pos, 5.0f, pos);
                return;
            }

            if (controller != null && moveDir != Vector3.zero)
            {
                controller.SimpleMove(moveDir.normalized * Time.deltaTime * moveSpeed * 5.0f);
                Quaternion targetRot = Quaternion.LookRotation(moveDir);
                tr.rotation = Quaternion.RotateTowards(tr.rotation, targetRot, 360 * rotSpeed * Time.deltaTime);
            }
        }
        else
        {  //�������� ��ũ�� ��� �߰����(��ġ, ȸ����) ���� ����
            if (10.0f < (tr.position - m_CurPos).magnitude)
                tr.position = m_CurPos;
            else
                tr.position = Vector3.Lerp(tr.position, m_CurPos, Time.deltaTime * 10.0f);
            
            tr.rotation = Quaternion.Slerp(tr.rotation, m_CurRot, Time.deltaTime * 10.0f);
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        //���� �÷��̾��� ��ġ ���� �۽�
        if (stream.IsWriting)
        {
            stream.SendNext(tr.position);
            stream.SendNext(tr.rotation);
        }
        else //���� �÷��̾��� ��ġ ���� ����
        {
            m_CurPos = (Vector3)stream.ReceiveNext();
            m_CurRot = (Quaternion)stream.ReceiveNext();
        }
    }
}