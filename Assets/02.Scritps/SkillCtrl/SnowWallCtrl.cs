using Photon.Pun;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class SnowWallCtrl : MonoBehaviour, IDamage
{
    private SnowData m_SnowData = new SnowData();
    public SnowData SnowData
    {
        get { return m_SnowData; }
        set { m_SnowData = value; }
    }

    public Image m_HpBarImg = null;

    private BoxCollider _collider = null;

    // Start is called before the first frame update
    void Start()
    {
        Init();

        _collider = GetComponent<BoxCollider>();
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

    void UpdateUI_CurHpBar()
    {
        if (0 < m_SnowData.m_CurHp)
        {
            m_HpBarImg.fillAmount = (float)m_SnowData.m_CurHp / (float)m_SnowData.m_MaxHp;

            if (m_SnowData.m_CurHp <= 0)
                m_SnowData.m_CurHp = 0;
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
