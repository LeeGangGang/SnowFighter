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

    private float m_CoolTime = 0.3f; // ��Ÿ��
    private float m_CurCoolTime = 0f; // ���� ��Ÿ��

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
        if (GameMgr.Inst.m_GameState != GameState.GS_Playing) // �������� �ƴϸ�
            return;

        if (m_ThisBtn.enabled == false)
            return;

        if (m_PlayerCtrl == null || m_SkillCtrl == null || m_PlayerTr == null)
            return;

        if (m_PlayerCtrl.m_CurStatus == PlayerState.Die)
            return;

        if (m_PlayerCtrl.m_CurSnowCnt <= 0)
            return;

        m_PlayerCtrl.MySetAnim(AnimState.Shot);

        Vector3 SnowPos = m_PlayerTr.position + m_PlayerTr.forward * 1.5f;
        SnowPos.y += 0.2f;
        Quaternion SnowRot = Quaternion.LookRotation(SnowPos - m_PlayerTr.position);

        m_SkillCtrl.Shot(SnowPos, SnowRot);

        m_CurCoolTime = m_CoolTime;
        int a_SnowCnt = m_PlayerCtrl.m_CurSnowCnt--;
        m_PlayerCtrl.SendSnowCnt(a_SnowCnt);
    }

    void CoolTime_Update()
    {
        if (0.0f < m_CurCoolTime)
        {
            m_CurCoolTime -= Time.deltaTime;
            m_SkillCoolImg.fillAmount = m_CurCoolTime / m_CoolTime;
            //Cool_Label.text = ((int)Cool_float).ToString();

            m_ThisBtn.enabled = false;
        }
        else
        {
            m_CurCoolTime = 0.0f;
            m_SkillCoolImg.fillAmount = 0f;
            //Cool_Label.text = "";

            m_ThisBtn.enabled = true;
        }
    }
}