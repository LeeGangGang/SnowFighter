using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SnowBallCtrl : MonoBehaviour, IDamageCtrl
{
    SnowData m_SnowData = new SnowData();
    public void Init()
    {
        m_SnowData.m_MaxHp = 10;
        m_SnowData.m_CurHp = m_SnowData.m_MaxHp;
        m_SnowData.m_Attck = 20;
    }

    public float Speed = 1000.0f;

    private SphereCollider _collider = null;
    private Rigidbody _rigidbody = null;

    private Transform tr;

    // Start is called before the first frame update
    void Start()
    {
        Init();

        _collider = GetComponent<SphereCollider>();
        _rigidbody = GetComponent<Rigidbody>();

        GetComponent<Rigidbody>().AddForce(transform.forward * Speed);
    }

    private void OnTriggerEnter(Collider other)
    {
        //if (other.gameObject.name.Contains("Player"))
        //    return;

        IDamageCtrl a_ISnow = other.gameObject.GetComponent<IDamageCtrl>();
        if (a_ISnow != null)
        {
            a_ISnow.GetDamage(m_SnowData.m_Attck);
        }

        DestroyThisObj();
    }

    IEnumerator DestroySnowBall(float tm)
    {
        yield return new WaitForSeconds(tm);

        // 충돌 콜백 함수가 발생하지 않도록 Collider를 비활성화
        if (_collider != null)
            _collider.enabled = false;

        // 물리엔진의 영향을 받을 필요 없음
        if (_rigidbody != null)
            _rigidbody.velocity = Vector3.zero;

        Destroy(this.gameObject, 0.0f);
    }

    public void GetDamage(float a_Damaage)
    {
        m_SnowData.m_CurHp -= a_Damaage;

        if (m_SnowData.m_CurHp <= 0.0f)
            DestroyThisObj();
    }

    public float SetDamage()
    {
        Debug.Log("스노우 10.0f");
        return m_SnowData.m_Attck;
    }

    public void DestroyThisObj()
    {
        StartCoroutine(this.DestroySnowBall(0.0f));
    }
}
