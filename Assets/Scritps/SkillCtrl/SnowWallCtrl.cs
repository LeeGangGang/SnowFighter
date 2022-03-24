using System.Collections;
using UnityEngine;

public class SnowWallCtrl : MonoBehaviour, IDamageCtrl
{
    SnowData m_SnowData = new SnowData();
    public void Init()
    {
        m_SnowData.m_MaxHp = 100;
        m_SnowData.m_CurHp = m_SnowData.m_MaxHp;
        m_SnowData.m_Attck = 0;
    }

    private BoxCollider _collider = null;

    private Transform tr;

    // Start is called before the first frame update
    void Start()
    {
        Init();

        _collider = GetComponent<BoxCollider>();
    }

    IEnumerator DestroySnowWall(float tm)
    {
        yield return new WaitForSeconds(tm);

        // 충돌 콜백 함수가 발생하지 않도록 Collider를 비활성화
        if (_collider != null)
            _collider.enabled = false;

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
        return 0;
    }

    public void DestroyThisObj()
    {
        StartCoroutine(this.DestroySnowWall(0.0f));
    }
}
