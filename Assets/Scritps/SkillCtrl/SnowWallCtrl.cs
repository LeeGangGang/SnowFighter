using System.Collections;
using UnityEngine;

public class SnowWallCtrl : MonoBehaviour
{
    SnowData m_SnowData = new SnowData();

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

    public void Init()
    {
        m_SnowData.m_MaxHp = 100;
        m_SnowData.m_CurHp = m_SnowData.m_MaxHp;
        m_SnowData.m_Attck = 0;
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
        StartCoroutine(this.DestroySnowWall(0.0f));
    }
}
