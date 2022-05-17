using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SnowBallCtrl : MonoBehaviour, IDamage
{
    private SnowData m_SnowData = new SnowData();
    public SnowData SnowData
    {
        get { return m_SnowData; }
        set { m_SnowData = value; }
    }

    public float Speed = 1000.0f;

    private SphereCollider _collider = null;
    private Rigidbody _rigidbody = null;

    // Start is called before the first frame update
    void Awake()
    {
        Init();

        _collider = GetComponent<SphereCollider>();
        _rigidbody = GetComponent<Rigidbody>();
        _rigidbody.AddForce(transform.forward * Speed);
    }

    IEnumerator DestroySnowBall(float tm)
    {
        yield return new WaitForSeconds(tm);

        // �浹 �ݹ� �Լ��� �߻����� �ʵ��� Collider�� ��Ȱ��ȭ
        if (!ReferenceEquals(_collider, null))
            _collider.enabled = false;

        // ���������� ������ ���� �ʿ� ����
        if (!ReferenceEquals(_rigidbody, null))
            _rigidbody.velocity = Vector3.zero;

        float Volume = SoundManager.Instance.GetDistVolume(this.transform.position);
        SoundManager.Instance.PlayEffSound("Hit", Volume);

        Destroy(this.gameObject, 0.0f);
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player") && 
            !other.CompareTag("SnowMan"))
            DestroyThisObj();
    }

    public void Init()
    {
        m_SnowData.m_MaxHp = 10;
        m_SnowData.m_CurHp = m_SnowData.m_MaxHp;
        m_SnowData.m_Attck = 20;
    }

    public void DestroyThisObj(float tm = 0)
    {
        StartCoroutine(this.DestroySnowBall(tm));
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

    public SnowData GetData()
    {
        return SnowData;
    }
}
