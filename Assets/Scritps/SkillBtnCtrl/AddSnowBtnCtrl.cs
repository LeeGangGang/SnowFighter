using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class AddSnowData
{
    public bool m_IsCasting = false; // 캐스팅중
    public float m_CastTime = 1.0f; // 캐스트 필요시간
    public float m_CurCastTime = 1.0f; // 현재 캐스트 시간
}

public class AddSnowBtnCtrl : MonoBehaviour
{
    private PlayerCtrl m_PlayerCtrl;
    private AddSnowData m_Data = new AddSnowData();

    // Start is called before the first frame update
    void Start()
    {
        m_PlayerCtrl = Camera.main.GetComponent<CameraCtrl>().Player.GetComponent<PlayerCtrl>();

        m_Data.m_CastTime = m_PlayerCtrl.m_Anim.runtimeAnimatorController.animationClips[(int)AnimState.Gather].length;
        m_Data.m_CurCastTime = m_Data.m_CastTime;

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
        if (m_Data.m_IsCasting)
        {
            int a_MaxSnowCnt = m_PlayerCtrl.m_MaxSnowCnt;
            int a_CurSnowCnt = m_PlayerCtrl.m_CurSnowCnt;
            Debug.Log(string.Format("1 {0} / {1}", a_MaxSnowCnt, a_CurSnowCnt));
            if (a_MaxSnowCnt <= a_CurSnowCnt)
                return;
            if (m_PlayerCtrl.m_CurAnimState != AnimState.Gather)
                m_PlayerCtrl.MySetAnim(AnimState.Gather);
            
            m_Data.m_CurCastTime -= Time.deltaTime;

            if (m_Data.m_CurCastTime <= 0.0f)
            {
                //int a_SnowCnt = m_PlayerCtrl.m_CurSnowCnt++;
                //m_PlayerCtrl.SendSnowCnt(a_SnowCnt);
                m_Data.m_CurCastTime = m_Data.m_CastTime;
            }
        }
    }

    void OnPointerDown(PointerEventData pointerEventData)
    {
        m_Data.m_IsCasting = true;
    }

    void OnPointerUp(PointerEventData pointerEventData)
    {
        m_PlayerCtrl.MySetAnim(AnimState.Idle);
        m_Data.m_IsCasting = false;
        m_Data.m_CurCastTime = m_Data.m_CastTime;
    }
}
