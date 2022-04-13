using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SnowBowlingCtrl : MonoBehaviour
{
    private SnowData m_SnowData = new SnowData();
    public SnowData SnowData
    {
        get { return m_SnowData; }
        set { m_SnowData = value; }
    }

    private PlayerCtrl m_PlayerCtrl;

    private Transform tr;
    private SphereCollider _collider = null;
    private Rigidbody _rigidbody = null;

    // Start is called before the first frame update
    void Start()
    {
        Init();

        m_PlayerCtrl = Camera.main.GetComponent<CameraCtrl>().Player.GetComponent<PlayerCtrl>();

        tr = GetComponent<Transform>();

        _collider = GetComponent<SphereCollider>();
        _rigidbody = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        tr.transform.Rotate(new Vector3(Time.deltaTime * 540.0f, 0, 0));
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.name.Equals("MyPlayer"))
            return;

        if (other.gameObject.name.Contains("Player"))
        {
            PlayerCtrl a_Player = other.gameObject.GetComponent<PlayerCtrl>();
            if (IsMyTeam(a_Player.m_MyTeam))
                return;
        }

        DestroyThisObj();
    }

    IEnumerator DestroySnowBowling(float tm)
    {
        yield return new WaitForSeconds(tm);

        // �浹 �ݹ� �Լ��� �߻����� �ʵ��� Collider�� ��Ȱ��ȭ
        if (_collider != null)
            _collider.enabled = false;

        // ���������� ������ ���� �ʿ� ����
        if (_rigidbody != null)
            _rigidbody.velocity = Vector3.zero;

        Destroy(this.gameObject, 0.0f);
    }

    public void Init()
    {
        m_SnowData.m_MaxHp = 50;
        m_SnowData.m_CurHp = m_SnowData.m_MaxHp;
        m_SnowData.m_Attck = 50;
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
        if (m_SnowData.m_CurHp <= 0.0f)
            DestroyThisObj();
    }

    public void DestroyThisObj()
    {
        if (m_PlayerCtrl != null)
            m_PlayerCtrl.m_IsBowling = false;

        Debug.Log(m_PlayerCtrl.m_NickName.text);

        StartCoroutine(this.DestroySnowBowling(0.0f));
    }
}