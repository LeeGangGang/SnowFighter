using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class SnowWallBtnCtrl : MonoBehaviour
{
    private Button m_ThisBtn;
    public Image m_SkillCoolImg;

    private PlayerCtrl m_PlayerCtrl;
    private PlayerSkillCtrl m_SkillCtrl;
    private Transform m_PlayerTr;

    [HideInInspector] bool m_IsCasting = false; // 캐스팅중
    [HideInInspector] float m_CastTime = 1.0f; // 캐스트 필요시간
    [HideInInspector] float m_CurCastTime = 0.0f; // 현재 캐스트 시간

    [HideInInspector] float m_CoolTime = 1.0f; // 쿨타임
    [HideInInspector] float m_CurCoolTime = 0.0f; // 현재 쿨타임

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

                if (m_PlayerCtrl.m_CurSnowCnt <= 0)
                    return;

                m_PlayerCtrl.m_CurStatus = PlayerState.HoldAction;

                if (m_PlayerCtrl.m_CurAnimState != AnimState.Gather)
                    m_PlayerCtrl.MySetAnim(AnimState.Gather);

                m_CurCastTime += Time.deltaTime;
                GameMgr.Inst.CastingBar(true, "눈벽 설치", m_CurCastTime, m_CastTime);
                if (m_CurCastTime >= m_CastTime)
                {
                    m_CurCastTime = 0.0f;
                    m_CurCoolTime = m_CoolTime;
                    GameMgr.Inst.CastingBar(false);
                    Vector3 WallPos = m_PlayerTr.position + m_PlayerTr.forward;
                    WallPos.y -= 1.5f;
                    Quaternion WallRot = m_PlayerTr.rotation;

                    m_SkillCtrl.CreateSnowWall(WallPos, WallRot);
                    m_PlayerCtrl.m_CurSnowCnt--;
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

        if (m_PlayerCtrl.m_CurSnowCnt <= 0)
            return;

        m_IsCasting = true;
        m_PlayerCtrl.m_CurStatus = PlayerState.HoldAction;
    }

    void OnPointerUp(PointerEventData pointerEventData)
    {
        if (m_PlayerCtrl == null)
            return;

        if (m_PlayerCtrl.m_CurStatus == PlayerState.Die ||
            m_PlayerCtrl.m_CurStatus == PlayerState.Catapult)
            return;

        EndSnowWall();
        m_PlayerCtrl.MySetAnim(AnimState.Idle);
    }

    public void EndSnowWall()
    {
        m_IsCasting = false;
        m_CurCastTime = 0.0f;
        GameMgr.Inst.CastingBar(false);
        m_PlayerCtrl.m_CurStatus = PlayerState.Idle;
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
            m_SkillCoolImg.fillAmount = 0.0f;
            m_ThisBtn.enabled = true;
        }
    }
}
