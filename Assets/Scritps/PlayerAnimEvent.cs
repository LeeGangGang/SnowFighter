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
        int a_SnowCnt = m_PlayerCtrl.m_CurSnowCnt++;
        m_PlayerCtrl.SendSnowCnt(a_SnowCnt);
        m_PlayerCtrl.MySetAnim(AnimState.Idle);
    }
}
