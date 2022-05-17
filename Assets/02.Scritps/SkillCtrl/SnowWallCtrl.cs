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

    public GameObject m_DamageTxtPrefab;
    public Transform m_DamageCanvas;

    // Start is called before the first frame update
    void Start()
    {
        Init();

        _collider = GetComponent<BoxCollider>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("SnowBall") || other.CompareTag("SnowBowling"))
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
        m_SnowData.m_MaxHp = 60;
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
        SpawnDamageTxt(a_Dmg);
        m_SnowData.m_CurHp -= a_Dmg;
        UpdateUI_CurHpBar();
        if (m_SnowData.m_CurHp <= 0.0f)
            DestroyThisObj();
    }

    public SnowData GetData()
    {
        return SnowData;
    }

    public void SpawnDamageTxt(float dmg)
    {
        if (m_DamageTxtPrefab != null && m_DamageCanvas != null)
        {
            GameObject m_DamageObj = (GameObject)Instantiate(m_DamageTxtPrefab);
            m_DamageObj.transform.SetParent(m_DamageCanvas, false);
            m_DamageObj.transform.localPosition = new Vector3(0f, 0.85f, 0f);
            DamageTxtCtrl a_DamageTx = m_DamageObj.GetComponentInChildren<DamageTxtCtrl>();
            a_DamageTx.m_DamageVal = (int)dmg;
        }
    }
}
