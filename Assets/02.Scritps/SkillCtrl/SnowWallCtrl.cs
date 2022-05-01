using Photon.Pun;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class SnowWallCtrl : MonoBehaviour
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
        if (other.gameObject.name.Contains("SnowBall"))
        {
            SnowBallCtrl a_Snow = other.gameObject.GetComponent<SnowBallCtrl>();
            if (!ReferenceEquals(a_Snow, null))
            {
                if (IsMyTeam(a_Snow.SnowData.AttackerTeam) == false)
                    GetDamage(a_Snow.SnowData.m_Attck, a_Snow.SnowData.AttackerId);

                a_Snow.DestroyThisObj();
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

    public void Init()
    {
        m_SnowData.m_MaxHp = 50;
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
        UpdateUI_CurHpBar();
        if (m_SnowData.m_CurHp <= 0.0f)
            DestroyThisObj();
    }

    public void DestroyThisObj()
    {
        StartCoroutine(this.DestroySnowWall(0.0f));
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
}
