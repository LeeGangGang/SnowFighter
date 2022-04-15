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

    public void RollingSnow(Vector3 a_Dir, float a_Speed)
    {
        tr.SetParent( null );
        _rigidbody.velocity = a_Dir * a_Speed;
        DestroyThisObj( 2.0f );
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

    IEnumerator DestroySnowBowling(float tm = 0.0f)
    {
        yield return new WaitForSeconds(tm);

        // 충돌 콜백 함수가 발생하지 않도록 Collider를 비활성화
        if (_collider != null)
            _collider.enabled = false;

        // 물리엔진의 영향을 받을 필요 없음
        if (_rigidbody != null)
            _rigidbody.velocity = Vector3.zero;

        Destroy(this.gameObject);
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

    public void DestroyThisObj(float tm = 0.0f)
    {
        if (m_PlayerCtrl != null)
            m_PlayerCtrl.m_IsBowling = false;

        StartCoroutine(this.DestroySnowBowling( tm ) );
    }
}
