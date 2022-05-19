using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SnowBallBtnCtrl : MonoBehaviour
{
    private Button m_ThisBtn;
    public Image m_SkillCoolImg;

    private PlayerCtrl m_PlayerCtrl;
    private PlayerSkillCtrl m_SkillCtrl;
    private Transform m_PlayerTr;

    private float m_CoolTime = 0.3f; // 쿨타임
    private float m_CurCoolTime = 0f; // 현재 쿨타임

    void Start()
    {
        m_PlayerCtrl = Camera.main.GetComponent<CameraCtrl>().Player.GetComponent<PlayerCtrl>();
        m_SkillCtrl = Camera.main.GetComponent<CameraCtrl>().Player.GetComponent<PlayerSkillCtrl>();
        m_PlayerTr = Camera.main.GetComponent<CameraCtrl>().Player.transform;

        m_ThisBtn = GetComponent<Button>();
        m_ThisBtn.onClick.AddListener(Btn_Click);
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

        if (m_PlayerCtrl.m_CurStatus == PlayerState.Die ||
            m_PlayerCtrl.m_CurStatus == PlayerState.Catapult)
            return;

        if (m_PlayerCtrl.m_CurSnowCnt <= 0)
            return;

        m_PlayerCtrl.MySetAnim(AnimState.Shot);

        Vector3 SnowPos = m_PlayerTr.position + m_PlayerTr.forward * 1.5f;
        SnowPos.y += 0.2f;
        Quaternion SnowRot = Quaternion.LookRotation(SnowPos - m_PlayerTr.position);

        m_SkillCtrl.Shot(SnowPos, SnowRot);

        m_CurCoolTime = m_CoolTime;
        m_PlayerCtrl.m_CurSnowCnt--;
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
