using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CatapultSnowBallCtrl : MonoBehaviour
{
    private SnowData m_SnowData = new SnowData();
    public GameObject m_Effect;

    bool IsDown = false;

    public SnowData SnowData
    {
        get { return m_SnowData; }
        set { m_SnowData = value; }
    }

    // Start is called before the first frame update
    void Awake()
    {
        Init();
    }

    // Update is called once per frame
    void Update()
    {
        if (IsDown)
        {
            Vector3 a_Pos = this.transform.position;
            a_Pos.y -= Time.deltaTime * 10f;
            this.transform.position = a_Pos;
        }
    }

    public void Init()
    {
        m_SnowData.m_MaxHp = 20;
        m_SnowData.m_CurHp = m_SnowData.m_MaxHp;
        m_SnowData.m_Attck = 40;
    }

    public void Drop()
    {
        IsDown = true;
        GetComponent<ParticleSystem>().Stop(true);
    }

    void OnTriggerEnter(Collider coll)
    {
        GameObject explosion = Instantiate(m_Effect, this.transform.position, Quaternion.identity);
        Destroy(this.gameObject);
    }

    void OnDestroy()
    {
        if (!IsDown)
            return;

        float Volume = SoundManager.Instance.GetDistVolume(this.transform.position) * 0.3f;
        SoundManager.Instance.PlayEffSound("CatapultBomb", Volume);

        Collider[] colls = Physics.OverlapSphere(this.transform.position, 2.5f);
        foreach (Collider coll in colls)
        {
            if (coll.CompareTag("SnowWall") || coll.CompareTag("SnowMan"))
            {
                IDamage IDmg = coll.gameObject.GetComponent<IDamage>();
                if (!ReferenceEquals(IDmg, null))
                {
                    if (IDmg.IsMyTeam(SnowData.AttackerTeam) == false)
                        IDmg.GetDamage(SnowData.m_Attck, SnowData.AttackerId);
                }
            }
            else if (coll.CompareTag("Player"))
            {
                PlayerCtrl a_Player = coll.gameObject.GetComponent<PlayerCtrl>();
                if (a_Player.IsMyTeam(SnowData.AttackerTeam) == false)
                    a_Player.GetDamage(SnowData.m_Attck, SnowData.AttackerId);
            }
        }
    }
}
