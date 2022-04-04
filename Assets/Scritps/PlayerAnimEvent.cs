using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimEvent : MonoBehaviour
{
    PlayerCtrl m_PlayerCtrl;

    // Start is called before the first frame update
    void Start()
    {
        m_PlayerCtrl = transform.parent.GetComponent<PlayerCtrl>(); 
    }

    void Event_AddSnow()
    {
        if (m_PlayerCtrl.pv.IsMine == false)
            return;

        int a_SnowCnt = m_PlayerCtrl.m_CurSnowCnt++;
        m_PlayerCtrl.SendSnowCnt(a_SnowCnt);
    }

    void Event_StartAnim()
    {
        if (m_PlayerCtrl.pv.IsMine == false)
            return;

        m_PlayerCtrl.m_MovePossible = false;
    }

    void Event_EndAnim()
    {
        if (m_PlayerCtrl.pv.IsMine == false)
            return;

        m_PlayerCtrl.m_MovePossible = true;
        m_PlayerCtrl.MySetAnim(AnimState.Idle);
    }
}
