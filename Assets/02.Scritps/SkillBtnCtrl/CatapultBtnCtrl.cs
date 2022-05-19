using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CatapultBtnCtrl : MonoBehaviour
{
    private Button m_ThisBtn;
    public Button m_ShotBtn;
    public Image m_SkillCoolImg;

    private PlayerCtrl m_PlayerCtrl;
    private PlayerSkillCtrl m_SkillCtrl;
    private Transform m_PlayerTr;

    private float m_CoolTime = 3f; // 쿨타임
    private float m_CurCoolTime = 0f; // 현재 쿨타임

    // Start is called before the first frame update
    void Start()
    {
        m_PlayerCtrl = Camera.main.GetComponent<CameraCtrl>().Player.GetComponent<PlayerCtrl>();
        m_SkillCtrl = Camera.main.GetComponent<CameraCtrl>().Player.GetComponent<PlayerSkillCtrl>();
        m_PlayerTr = Camera.main.GetComponent<CameraCtrl>().Player.transform;

        m_ThisBtn = GetComponent<Button>();
        m_ThisBtn.onClick.AddListener(Btn_Click);

        if (m_ShotBtn != null)
            m_ShotBtn.onClick.AddListener(Shot_Click);
    }

    // Update is called once per frame
    void Update()
    {
        CoolTime_Update();
    }

    private void Btn_Click()
    {
        if (GameMgr.Inst.m_GameState != GameState.GS_Playing) // 게임중이 아니면
            return;

        if (m_ThisBtn.enabled == false)
            return;

        if (m_PlayerCtrl == null || m_SkillCtrl == null || m_PlayerTr == null)
            return;

        if (m_PlayerCtrl.m_CurStatus == PlayerState.Die)
            return;

        if (m_PlayerCtrl.m_CurStatus != PlayerState.Catapult)
        {
            if (m_PlayerCtrl.m_CurSnowCnt <= 1)
                return;

            m_ShotBtn.gameObject.SetActive(true);
            m_PlayerCtrl.m_CurStatus = PlayerState.Catapult;

            m_SkillCtrl.CreateCatapult();
        }
        else
        {
            m_ShotBtn.gameObject.SetActive(false);
            m_SkillCtrl.DestoryAttProjector();
            m_PlayerCtrl.m_CurStatus = PlayerState.Idle;

            m_SkillCtrl.DestoryCatapult();
        }
    }

    void Shot_Click()
    {
        if (m_PlayerCtrl.m_CurStatus != PlayerState.Catapult)
            return;

        Vector3 SnowPos = m_SkillCtrl.m_Projector.transform.position;
        SnowPos.y = 15f;

        m_SkillCtrl.CatapultShot(SnowPos);
        m_ShotBtn.gameObject.SetActive(false);
        m_SkillCtrl.DestoryAttProjector();

        m_CurCoolTime = m_CoolTime;
        m_PlayerCtrl.m_CurSnowCnt -= 2;
    }

    void CoolTime_Update()
    {
        if (0.0f < m_CurCoolTime)
        {
            m_CurCoolTime -= Time.deltaTime;
            m_SkillCoolImg.fillAmount = m_CurCoolTime / m_CoolTime;
            m_ThisBtn.enabled = false;
        }
        else
        {
            m_CurCoolTime = 0.0f;
            m_SkillCoolImg.fillAmount = 0f;
            m_ThisBtn.enabled = true;
        }
    }
}
