using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class SnowWallBtnCtrl : MonoBehaviour
{
    public GameObject m_SnowWallPrefab;

    private PlayerCtrl m_PlayerCtrl;
    private Transform m_PlayerTr;

    private bool m_IsCasting = false; // 캐스팅중
    private float m_CastTime = 1.5f; // 캐스트 필요시간
    private float m_CurCastTime = 1.5f; // 현재 캐스트 시간

    // Start is called before the first frame update
    void Start()
    {
        m_CurCastTime = m_CastTime;

        m_PlayerCtrl = Camera.main.GetComponent<CameraCtrl>().Player.GetComponent<PlayerCtrl>();
        m_PlayerTr = Camera.main.GetComponent<CameraCtrl>().Player.transform;

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
            m_CurCastTime -= Time.deltaTime;
            if (m_CurCastTime <= 0.0f)
            {
                CreateSnowWall();
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

    private void CreateSnowWall()
    {
        if (m_PlayerCtrl == null || m_PlayerTr == null ||
            m_SnowWallPrefab == null)
            return;

        Vector3 WallPos = m_PlayerTr.position + m_PlayerTr.forward;
        WallPos.y -= 1.5f;
        GameObject a_SnowWall = GameObject.Instantiate(m_SnowWallPrefab, WallPos, m_PlayerTr.rotation);
        
        m_CurCastTime = m_CastTime;
    }
}
