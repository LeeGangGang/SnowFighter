using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class AddSnowBtnCtrl : MonoBehaviour
{
    private PlayerCtrl m_PlayerCtrl;

    private bool m_IsCasting = false; // 캐스팅중
    private float m_CastTime = 1.0f; // 캐스트 필요시간
    private float m_CurCastTime = 1.0f; // 현재 캐스트 시간

    // Start is called before the first frame update
    void Start()
    {
        m_CurCastTime = m_CastTime;

        m_PlayerCtrl = Camera.main.GetComponent<CameraCtrl>().Player.GetComponent<PlayerCtrl>();

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
        if (m_IsCasting)
        {
            int a_MaxSnowCnt = m_PlayerCtrl.m_MaxSnowCnt;
            int a_CurSnowCnt = m_PlayerCtrl.m_CurSnowCnt;
            if (a_MaxSnowCnt <= a_CurSnowCnt)
                return;

            m_CurCastTime -= Time.deltaTime;
            if (m_CurCastTime <= 0.0f)
            {
                int a_SnowCnt = m_PlayerCtrl.m_CurSnowCnt + 1;
                m_PlayerCtrl.SendSnowCnt(a_SnowCnt);
                m_CurCastTime = m_CastTime;
            }
        }
    }

    void OnPointerDown(PointerEventData pointerEventData)
    {
        m_IsCasting = true;
    }

    void OnPointerUp(PointerEventData pointerEventData)
    {
        m_IsCasting = false;
        m_CurCastTime = m_CastTime;
    }
}
