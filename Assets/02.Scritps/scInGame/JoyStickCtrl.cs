using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class JoyStickCtrl : MonoBehaviour
{
    private PlayerCtrl m_PlayerCtrl;

    int m_MyTouchID = -1;

    public Image m_JoyStickImg = null;
    float m_Radius = 0.0f;
    Vector2 m_OrignPos = Vector2.zero;
    Vector2 m_Axis = Vector2.zero;
    Vector2 m_JsCacVec = Vector2.zero;
    float m_JsCacDist = 0.0f;

    // Start is called before the first frame update
    void Start()
    {
        m_PlayerCtrl = Camera.main.GetComponent<CameraCtrl>().Player.GetComponent<PlayerCtrl>();

        if (!ReferenceEquals(m_JoyStickImg, null))
        {
            Vector3[] v = new Vector3[4];
            this.GetComponent<RectTransform>().GetWorldCorners(v);
            // [0] : 좌하 / [1] : 좌상 / [2] : 우상 / [3] : 우하
            m_Radius = v[2].y - v[0].y;
            m_Radius = m_Radius / 3.0f;

            m_OrignPos = m_JoyStickImg.transform.position;

            EventTrigger trigger = this.GetComponent<EventTrigger>();
            EventTrigger.Entry entry = new EventTrigger.Entry();
            entry.eventID = EventTriggerType.Drag;
            entry.callback.AddListener((data) => { OnDragJoyStick((PointerEventData)data); });
            trigger.triggers.Add(entry);

            entry = new EventTrigger.Entry();
            entry.eventID = EventTriggerType.EndDrag;
            entry.callback.AddListener((data) => { OnEndDragJoyStick((PointerEventData)data); });
            trigger.triggers.Add(entry);
        }
    }

    void OnDragJoyStick(PointerEventData _data)
    {
        if (m_JoyStickImg == null)
            return;

#if UNITY_ANDROID && !UNITY_EDITOR
        if (Input.touchCount > 0)
        {
            if (m_MyTouchID == -1)
            {
                m_MyTouchID = Input.GetTouch(Input.touchCount - 1).fingerId;
                m_JsCacVec = Input.GetTouch(Input.touchCount - 1).position - m_OrignPos;
            }

            for (int i = 0; i < Input.touchCount; i++)
            {
                if (m_MyTouchID == Input.GetTouch(i).fingerId)
                    m_JsCacVec = Input.GetTouch(i).position - m_OrignPos;
            }
        }
#else
        Vector2 mousePos = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
        m_JsCacVec = mousePos - m_OrignPos;
#endif
        m_JsCacDist = m_JsCacVec.magnitude;
        m_Axis = m_JsCacVec.normalized;

        // 조이스틱 백그라운드를 벗어나지 못하게 막는 부분
        if (m_Radius < m_JsCacDist)
        {
            m_JoyStickImg.transform.position = m_OrignPos + m_Axis * m_Radius;
        }
        else
        {
            m_JoyStickImg.transform.position = m_OrignPos + m_Axis * m_JsCacDist;
        }

        if (1.0f < m_JsCacDist)
            m_JsCacDist = 1.0f;

        // 캐릭터 이동 처리
        if (!ReferenceEquals(m_PlayerCtrl, null))
            m_PlayerCtrl.SetJoyStickMv(m_JsCacDist, m_Axis);
    }

    void OnEndDragJoyStick(PointerEventData _data)
    {
        if (m_JoyStickImg == null)
            return;

        m_MyTouchID = -1;

        m_Axis = Vector3.zero;
        m_JoyStickImg.transform.position = m_OrignPos;

        m_JsCacDist = 0.0f;

        // 캐릭터 정지 처리
        if (!ReferenceEquals(m_PlayerCtrl, null))
            m_PlayerCtrl.SetJoyStickMv(m_JsCacDist, m_Axis);
    }
}
