using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class JoyStickCtrl : MonoBehaviour
{
    private PlayerCtrl m_PlayerCtrl;

    public Image m_JoyStickImg = null;
    float m_Radius = 0.0f;
    Vector3 m_OrignPos = Vector3.zero;
    Vector3 m_Axis = Vector3.zero;
    Vector3 m_JsCacVec = Vector3.zero;
    float m_JsCacDist = 0.0f;

    // Start is called before the first frame update
    void Start()
    {
        m_PlayerCtrl = Camera.main.GetComponent<CameraCtrl>().Player.GetComponent<PlayerCtrl>();

        if (m_JoyStickImg != null)
        {
            Vector3[] v = new Vector3[4];
            this.GetComponent<RectTransform>().GetWorldCorners(v);
            // [0] : ���� / [1] : �»� / [2] : ��� / [3] : ����
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

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnDragJoyStick(PointerEventData _data)
    {
        if (m_JoyStickImg == null)
            return;

        m_JsCacVec = Input.mousePosition - m_OrignPos;
        m_JsCacVec.z = 0.0f;
        m_JsCacDist = m_JsCacVec.magnitude;
        m_Axis = m_JsCacVec.normalized;

        // ���̽�ƽ ��׶��带 ����� ���ϰ� ���� �κ�
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

        // ĳ���� �̵� ó��
        if (m_PlayerCtrl != null)
            m_PlayerCtrl.SetJoyStickMv(m_JsCacDist, m_Axis);
    }

    void OnEndDragJoyStick(PointerEventData _data)
    {
        if (m_JoyStickImg == null)
            return;

        m_Axis = Vector3.zero;
        m_JoyStickImg.transform.position = m_OrignPos;

        m_JsCacDist = 0.0f;

        // ĳ���� ���� ó��
        if (m_PlayerCtrl != null)
            m_PlayerCtrl.SetJoyStickMv(m_JsCacDist, m_Axis);
    }
}
