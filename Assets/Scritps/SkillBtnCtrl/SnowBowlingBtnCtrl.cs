using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class SnowBowlingBtnCtrl : MonoBehaviour
{
    private Button m_ThisBtn;
    public Image m_SkillCoolImg;

    private PlayerCtrl m_PlayerCtrl;
    private PlayerSkillCtrl m_SkillCtrl;
    private Transform m_PlayerTr;

    private float m_CurMoveSpeed = 0.0f;
    private float m_MaxMoveSpeed = 200.0f;

    [HideInInspector] bool m_IsCasting = false; // ��¡������ Ȯ��
    [HideInInspector] float m_CastTime = 10.0f; // �ִ� ��¡ �ð�
    [HideInInspector] float m_CurCastTime = 0.0f; // ���� ��¡ �ð�

    [HideInInspector] float m_CoolTime = 0.5f; // ��Ÿ��
    [HideInInspector] float m_CurCoolTime = 0.0f; // ���� ��Ÿ��

    // Start is called before the first frame update
    void Start()
    {
        m_PlayerCtrl = Camera.main.GetComponent<CameraCtrl>().Player.GetComponent<PlayerCtrl>();
        m_SkillCtrl = Camera.main.GetComponent<CameraCtrl>().Player.GetComponent<PlayerSkillCtrl>();
        m_PlayerTr = Camera.main.GetComponent<CameraCtrl>().Player.transform;

        m_ThisBtn = GetComponent<Button>();

        EventTrigger eventTrigger = gameObject.AddComponent<EventTrigger>();
        EventTrigger.Entry entry_PointerDown = new EventTrigger.Entry();
        entry_PointerDown.eventID = EventTriggerType.PointerDown;
        entry_PointerDown.callback.AddListener((data) => { OnPointerDown((PointerEventData)data); });
        eventTrigger.triggers.Add(entry_PointerDown);

        EventTrigger.Entry entry_PointerUp = new EventTrigger.Entry();
        entry_PointerUp.eventID = EventTriggerType.PointerUp;
        entry_PointerUp.callback.AddListener((data) => { OnPointerUp((PointerEventData)data); });
        eventTrigger.triggers.Add(entry_PointerUp);
    }

    // Update is called once per frame
    void Update()
    {
        if (GameMgr.Inst.m_GameState != GameState.GS_Playing) // �������� �ƴϸ�
            return;

        CoolTime_Update();

        if (m_ThisBtn.enabled) // ��Ÿ������ �ƴҶ�
        {
            if (m_IsCasting) // ĳ�������̶��
            {
                if (m_PlayerCtrl == null || m_SkillCtrl == null || m_PlayerTr == null)
                    return;

                if (m_PlayerCtrl.m_CurSnowCnt <= 0)
                    return;

                m_CurCastTime += Time.deltaTime;
                GameMgr.Inst.CastingBar(true, "�� ������", m_CurCastTime, m_CastTime);
                if (m_CurCastTime <= m_CastTime)
                {
                    m_PlayerCtrl.m_BowMvSpeed += Time.deltaTime * 40.0f;
                    if (m_PlayerCtrl.m_BowMvSpeed > 400.0f)
                        m_PlayerCtrl.m_BowMvSpeed = 400.0f;
                }
                else
                {
                    m_CurCastTime = 0.0f;
                    m_CurCoolTime = m_CoolTime;
                    GameMgr.Inst.CastingBar(false);
                    m_PlayerCtrl.m_IsBowling = false;
                    m_PlayerCtrl.m_BowMvSpeed = 200.0f;
                }
            }
            else
            {
                m_CurCastTime = 0.0f;
                m_CurCoolTime = m_CoolTime;
                GameMgr.Inst.CastingBar(false);
                m_PlayerCtrl.m_IsBowling = false;
                m_PlayerCtrl.m_BowMvSpeed = 200.0f;
            }
        }
    }

    void OnPointerDown(PointerEventData pointerEventData)
    {
        m_IsCasting = true;
        m_PlayerCtrl.m_IsBowling = true;
        if (m_PlayerCtrl.transform.Find("SnowBowling") == null)
        {
            Vector3 BallPos = m_PlayerTr.position + (m_PlayerTr.forward * 2.0f);
            Quaternion BallRot = m_PlayerTr.rotation;
            m_SkillCtrl.CreateSnowBowling(BallPos, BallRot);
        }
    }

    void OnPointerUp(PointerEventData pointerEventData)
    {
        m_PlayerCtrl.MySetAnim(AnimState.Idle);
        m_IsCasting = false;
        m_CurCastTime = 0.0f;
        GameMgr.Inst.CastingBar(false);
        m_PlayerCtrl.m_IsBowling = false;
        m_PlayerCtrl.m_BowMvSpeed = 200.0f;
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
            m_SkillCoolImg.fillAmount = 0.0f;
            //Cool_Label.text = "";

            m_ThisBtn.enabled = true;
        }
    }
}
