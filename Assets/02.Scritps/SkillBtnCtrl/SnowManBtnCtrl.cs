using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SnowManBtnCtrl : MonoBehaviour
{
    private Button m_ThisBtn;
    public Image m_SkillCoolImg;

    private PlayerCtrl m_PlayerCtrl;
    private PlayerSkillCtrl m_SkillCtrl;
    private Transform m_PlayerTr;

    [HideInInspector] bool m_IsCasting = false; // 캐스팅중
    [HideInInspector] float m_CastTime = 1f; // 캐스트 필요시간
    [HideInInspector] float m_CurCastTime = 0f; // 현재 캐스트 시간

    [HideInInspector] float m_CoolTime = 1f; // 쿨타임
    [HideInInspector] float m_CurCoolTime = 0f; // 현재 쿨타임

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
        if (GameMgr.Inst.m_GameState != GameState.GS_Playing) // 게임중이 아니면
            return;

        CoolTime_Update();

        if (m_PlayerCtrl.m_CurStatus == PlayerState.Die || 
            m_PlayerCtrl.m_CurStatus == PlayerState.Catapult)
            return;

        if (m_ThisBtn.enabled) // 쿨타임중이 아닐때
        {
            if (m_IsCasting) // 캐스팅중이라면
            {
                if (m_PlayerCtrl == null || m_SkillCtrl == null || m_PlayerTr == null)
                    return;

                if (m_PlayerCtrl.m_CurSnowCnt <= 1)
                    return;

                m_PlayerCtrl.m_CurStatus = PlayerState.HoldAction;

                if (m_PlayerCtrl.m_CurAnimState != AnimState.Gather)
                    m_PlayerCtrl.MySetAnim(AnimState.Gather);

                m_CurCastTime += Time.deltaTime;
                GameMgr.Inst.CastingBar(true, "눈사람 소환", m_CurCastTime, m_CastTime);
                if (m_CurCastTime >= m_CastTime)
                {
                    m_CurCastTime = 0f;
                    m_CurCoolTime = m_CoolTime;
                    GameMgr.Inst.CastingBar(false);
                    Vector3 SnowManPos = m_PlayerTr.position + m_PlayerTr.forward;
                    SnowManPos.y -= 1.5f;
                    Quaternion SnowManRot = m_PlayerTr.rotation;

                    m_SkillCtrl.CreateSnowMan(SnowManPos, SnowManRot);
                    m_PlayerCtrl.SendSnowCnt(m_PlayerCtrl.m_CurSnowCnt - 2);
                }
            }
        }
    }

    void OnPointerDown(PointerEventData pointerEventData)
    {
        if (m_PlayerCtrl == null)
            return;

        if (m_PlayerCtrl.m_CurStatus == PlayerState.Die ||
            m_PlayerCtrl.m_CurStatus == PlayerState.Catapult)
            return;

        if (m_PlayerCtrl.m_CurSnowCnt <= 1)
            return;

        m_IsCasting = true;
    }

    void OnPointerUp(PointerEventData pointerEventData)
    {
        if (m_PlayerCtrl == null)
            return;

        if (m_PlayerCtrl.m_CurStatus == PlayerState.Die ||
            m_PlayerCtrl.m_CurStatus == PlayerState.Catapult)
            return;

        EndSnowMan();
        m_PlayerCtrl.MySetAnim(AnimState.Idle);
    }

    public void EndSnowMan()
    {
        m_IsCasting = false;
        m_CurCastTime = 0f;
        GameMgr.Inst.CastingBar(false);
        m_PlayerCtrl.m_CurStatus = PlayerState.Idle;
    }

    void CoolTime_Update()
    {
        if (0f < m_CurCoolTime)
        {
            m_CurCoolTime -= Time.deltaTime;
            m_SkillCoolImg.fillAmount = m_CurCoolTime / m_CoolTime;
            m_ThisBtn.enabled = false;
        }
        else
        {
            m_CurCoolTime = 0f;
            m_SkillCoolImg.fillAmount = 0f;
            m_ThisBtn.enabled = true;
        }
    }
}
