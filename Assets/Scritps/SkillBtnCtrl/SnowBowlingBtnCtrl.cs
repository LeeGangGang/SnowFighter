using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class SnowBowlingBtnCtrl : MonoBehaviour
{
    private PlayerCtrl m_PlayerCtrl;

    private float m_CurMoveSpeed = 0.0f;
    private float m_MaxMoveSpeed = 200.0f;

    private bool m_IsChargeing = false; // 차징중인지 확인
    private float m_MaxChargeTime = 4.0f; // 최대 차징 시간
    private float m_CurChargeTime = 0.0f; // 현재 차징 시간

    // Start is called before the first frame update
    void Start()
    {
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
        //controller.SimpleMove(moveDir.normalized * Time.deltaTime * moveSpeed * 5.0f);
        //Quaternion targetRot = Quaternion.LookRotation(moveDir);
        //this.transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, 360 * rotSpeed * Time.deltaTime);
    }

    void OnPointerDown(PointerEventData pointerEventData)
    {
        m_IsChargeing = true;
    }

    void OnPointerUp(PointerEventData pointerEventData)
    {
        m_IsChargeing = false;
    }
}
